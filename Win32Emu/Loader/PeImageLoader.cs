using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Tls;
using Microsoft.Extensions.Logging;
using Win32Emu.Memory;
using System.IO;
using System.Linq;

namespace Win32Emu.Loader;

/// <summary>
/// PE loader using a single PEImage load. Maps section raw data, applies base relocations if needed,
/// and replaces IAT entries with synthetic addresses for interception via an import map.
/// </summary>
public class PeImageLoader(VirtualMemory vm, ILogger? logger = null)
{
	// Threshold for filtering likely uninitialized IAT entries during initial processing.
	// IAT entries below this value (0x00400000) are likely uninitialized (e.g., 0x00000000) or corrupted.
	// This is NOT used for validating final IAT values, which may be >= 0x00400000 (image base) or emulator special ranges (e.g., 0x0F000000).
	private const uint IMAGE_BASE_THRESHOLD = 0x00400000;
	
	// Maximum number of TLS callbacks to extract (safety limit to prevent infinite loops on corrupted PE files)
	// While the PE format allows unlimited callbacks, legitimate executables rarely have more than a few
	private const int MAX_TLS_CALLBACKS = 64;
	
	/// <summary>
	/// Validates if a file is a valid PE32 executable by parsing the PE structure (and may map sections into memory).
	/// </summary>
	/// <param name="path">Path to the executable file</param>
	/// <returns>True if the file is a valid PE32 executable, false otherwise</returns>
	public static bool IsPE32(string path)
	{
		try
		{
			var image = PEImage.FromFile(path);
			var pe = image.PEFile;
			if (pe == null)
			{
				return false;
			}

			var opt = pe.OptionalHeader;
			if (opt == null)
			{
				return false;
			}

			return opt.Magic == OptionalHeaderMagic.PE32;
		}
		catch
		{
			return false;
		}
	}

	public LoadedImage Load(string path)
	{
		var image = PEImage.FromFile(path);
		var headerBytes = File.ReadAllBytes(path);
		return LoadFromImage(image, path, headerBytes, null);
	}

	/// <summary>
	/// Loads a PE image from a file at a specified base address.
	/// If the base address differs from the preferred ImageBase, relocations will be applied.
	/// </summary>
	/// <param name="path">Path to the PE file</param>
	/// <param name="baseAddress">Base address to load the image at. Should be page-aligned (0x1000). Caller is responsible for ensuring alignment.</param>
	/// <returns>Loaded image information</returns>
	public LoadedImage Load(string path, uint baseAddress)
	{
		var image = PEImage.FromFile(path);
		var headerBytes = File.ReadAllBytes(path);
		return LoadFromImage(image, path, headerBytes, baseAddress);
	}

	public LoadedImage LoadFromBytes(byte[] bytes)
	{
		var image = PEImage.FromBytes(bytes);
		// For LoadFromBytes, we use a synthetic path since there's no real file
		return LoadFromImage(image, "<memory>", bytes, null);
	}

	/// <summary>
	/// Loads a PE image from bytes at a specified base address.
	/// If the base address differs from the preferred ImageBase, relocations will be applied.
	/// </summary>
	/// <param name="bytes">PE file contents</param>
	/// <param name="baseAddress">Base address to load the image at. Should be page-aligned (0x1000). Caller is responsible for ensuring alignment.</param>
	/// <returns>Loaded image information</returns>
	public LoadedImage LoadFromBytes(byte[] bytes, uint baseAddress)
	{
		var image = PEImage.FromBytes(bytes);
		return LoadFromImage(image, "<memory>", bytes, baseAddress);
	}

	private LoadedImage LoadFromImage(PEImage image, string sourcePath, byte[] fileBytes, uint? customBaseAddress = null)
	{
		var pe = image.PEFile ?? throw new InvalidOperationException("PEImage missing PEFile.");
		var opt = pe.OptionalHeader ?? throw new InvalidOperationException("Missing optional header.");

		if(opt.Magic != OptionalHeaderMagic.PE32)
		{
			throw new NotSupportedException("Only PE32 format is supported.");
		}

		// Use custom base address if provided, otherwise use preferred ImageBase
		var imageBase = customBaseAddress ?? (uint)opt.ImageBase;
		var entryPoint = imageBase + opt.AddressOfEntryPoint;
		var imageSize = opt.SizeOfImage;
		var subsystem = (ushort)opt.SubSystem;

		// Load PE headers into memory at the image base
		// The Windows PE loader maps headers into memory, which some programs may read
		// Headers include DOS header, NT headers, section table, etc.
		var sizeOfHeaders = opt.SizeOfHeaders;
		
		// Compute first section RVA and headerEndRva up front so other components can use it
		uint minSectionRva = uint.MaxValue;
		foreach (var section in pe.Sections.Where(s => s.Rva > 0))
		{
			if (section.Rva < minSectionRva)
			{
				minSectionRva = section.Rva;
			}
		}
		// Determine effective header size: use minimum of SizeOfHeaders and first section RVA
		uint headerEndRva = (minSectionRva == uint.MaxValue || minSectionRva > sizeOfHeaders)
			? sizeOfHeaders
			: minSectionRva;
		
		// Safety check: ensure SizeOfHeaders is reasonable (typically <= 4KB for most PE files)
		// Some malformed PE files might have invalid SizeOfHeaders values
		if (sizeOfHeaders > 0 && sizeOfHeaders <= 0x10000) // Cap at 64KB
		{
			logger?.LogDebug("[Loader] Loading PE headers: Size=0x{HeaderSize:X8} at ImageBase=0x{ImageBase:X8}", sizeOfHeaders, imageBase);
			
			try
			{
				// We already computed headerEndRva above using the first section RVA.
				// Safely calculate header size, ensuring we don't overflow int.MaxValue
				long headerSizeLong = Math.Min((long)sizeOfHeaders, (long)headerEndRva);
				if (headerSizeLong > int.MaxValue)
				{
					logger?.LogWarning("[Loader] Calculated header size (0x{Size:X8}) exceeds int.MaxValue, capping at 0x{Max:X8}", headerSizeLong, int.MaxValue);
					headerSizeLong = int.MaxValue;
				}
				var actualHeaderSize = (int)headerSizeLong;
				
				if (actualHeaderSize < sizeOfHeaders)
				{
					logger?.LogWarning("[Loader] SizeOfHeaders (0x{Size:X8}) extends beyond first section RVA (0x{FirstRva:X8}), truncating to first section", sizeOfHeaders, headerEndRva);
				}
				
				// Read only the required header bytes from the file
				// Note: When loading from file, we re-read for headers. When loading from bytes,
				// we already have them in memory from fileBytes parameter
				var headerData = new byte[actualHeaderSize];
				Array.Copy(fileBytes, 0, headerData, 0, Math.Min(actualHeaderSize, fileBytes.Length));
				
				vm.WriteBytes(imageBase, headerData);
				logger?.LogDebug("[Loader] Loaded 0x{Size:X8} bytes of PE headers", headerData.Length);
			}
			catch (Exception ex)
			{
				logger?.LogWarning(ex, "[Loader] Failed to load PE headers");
			}
		}
		else
		{
			logger?.LogWarning("[Loader] Skipping PE header loading: SizeOfHeaders (0x{Size:X8}) is invalid or out of range", sizeOfHeaders);
		}

		// Map sections (raw contents only; uninitialized data left zeroed).
		foreach (var section in pe.Sections)
		{
			if (section.Contents is null)
			{
				logger?.LogDebug("[Loader] Skipping section {SectionName} at RVA 0x{Rva:X8}: Contents is null", section.Name, section.Rva);
				continue;
			}

			try
			{
				var rawData = section.Contents.WriteIntoArray();
				var virtualSize = section.Contents.GetVirtualSize();
				var sectionRva = section.Rva;
				
				logger?.LogDebug("[Loader] Loading section {SectionName}: RVA=0x{Rva:X8}, VirtualSize=0x{VSize:X8}, RawDataSize=0x{RawSize:X8}, Flags=0x{Flags:X8}", 
					section.Name, sectionRva, virtualSize, rawData.Length, (uint)section.Characteristics);
				
				// Determine how many bytes to actually write to memory
				// Per PE format specification:
				// - If RawDataSize > VirtualSize, only write VirtualSize bytes (extra bytes in file are padding/garbage)
				// - If VirtualSize > RawDataSize, write all RawDataSize bytes (extra virtual bytes remain zero)
				var bytesToWrite = Math.Min((uint)rawData.Length, virtualSize);
				
				// Safety check: ensure bytesToWrite fits in int for array indexing
				// PE sections should never be this large in practice (>2GB), but we check defensively
				if (bytesToWrite > int.MaxValue)
				{
					logger?.LogError("[Loader] Section {SectionName} is too large to load: bytesToWrite=0x{BytesToWrite:X} (min of RawDataSize=0x{RawSize:X}, VirtualSize=0x{VSize:X}) exceeds int.MaxValue", 
						section.Name, bytesToWrite, rawData.Length, virtualSize);
					throw new InvalidOperationException($"Section {section.Name} size (0x{bytesToWrite:X}) exceeds maximum supported size (0x{int.MaxValue:X})");
				}
				
				if (bytesToWrite < rawData.Length)
				{
				    logger?.LogDebug("[Loader] Section {SectionName} has RawDataSize (0x{RawSize:X8}) > VirtualSize (0x{VSize:X8}), truncating to VirtualSize to avoid writing beyond section bounds",
				        section.Name, rawData.Length, virtualSize);
				}
				else if (virtualSize > rawData.Length)
				{
				    // If VirtualSize is larger than raw data size, the extra bytes should remain zero
				    // (VirtualMemory already initializes to zero, so we don't need to explicitly zero-fill)
				    logger?.LogDebug("[Loader] Section {SectionName} has VirtualSize (0x{VSize:X8}) > RawDataSize (0x{RawSize:X8}), extra 0x{Extra:X8} bytes remain zero-filled",
				        section.Name, virtualSize, rawData.Length, virtualSize - (uint)rawData.Length);
				}

				// Only write if there is data to write.
				// Cast is safe because we've verified bytesToWrite <= int.MaxValue above.
				if (bytesToWrite > 0)
				{
				    vm.WriteBytes(imageBase + sectionRva, rawData.AsSpan(0, (int)bytesToWrite));
				}
			}
			catch (Exception ex) when (ex is System.IO.EndOfStreamException or ArgumentException)
			{
				// Skip corrupted sections that extend beyond file boundaries
				// This can happen with malformed PE files where section headers indicate
				// sizes that don't match actual file data
				logger?.LogWarning("Skipping corrupted section {SectionName} at RVA {SectionRva:X8}: {ErrorMessage}", section.Name, section.Rva, ex.Message);
			}
		}

		// Apply base relocations if the image is loaded at a different address than preferred
		// Note: For emulation purposes, we typically load at the preferred base, but this
		// implements the full PE loader behavior for correctness
		ApplyRelocations(image, imageBase, opt.ImageBase, vm, logger);

		var (importMap, iatEntryMap) = BuildImportMap(image, imageBase);
		
		// Register IAT entries for runtime verification and auto-fix
		vm.RegisterIatEntries(iatEntryMap);
		logger?.LogInformation("[Loader] Registered {Count} IAT entries for runtime protection", iatEntryMap.Count);
		
		var (exportsByName, exportsByOrdinal, forwardedByName, forwardedByOrdinal) = BuildExportMaps(image, imageBase);
		var exportMetadata = BuildExportMetadata(image, logger);
		var tlsCallbacks = ExtractTlsCallbacks(image, imageBase, vm, logger);

		// Stack and heap sizes from optional header (PE-provided)
		uint sizeOfStackReserve;
		uint sizeOfStackCommit;
		uint sizeOfHeapReserve;
		uint sizeOfHeapCommit;
		try
		{
			sizeOfStackReserve = (uint)opt.SizeOfStackReserve;
			sizeOfStackCommit = (uint)opt.SizeOfStackCommit;
			sizeOfHeapReserve = (uint)opt.SizeOfHeapReserve;
			sizeOfHeapCommit = (uint)opt.SizeOfHeapCommit;
		}
		catch
		{
			// Fallback if types are larger than uint (shouldn't happen for PE32), cap to uint max
			sizeOfStackReserve = (uint)Math.Min((ulong)uint.MaxValue, Convert.ToUInt64(opt.SizeOfStackReserve));
			sizeOfStackCommit  = (uint)Math.Min((ulong)uint.MaxValue, Convert.ToUInt64(opt.SizeOfStackCommit));
			sizeOfHeapReserve = (uint)Math.Min((ulong)uint.MaxValue, Convert.ToUInt64(opt.SizeOfHeapReserve));
			sizeOfHeapCommit  = (uint)Math.Min((ulong)uint.MaxValue, Convert.ToUInt64(opt.SizeOfHeapCommit));
		}

		// Extract FileHeader fields
		var fileHeader = pe.FileHeader;
		var machine = (ushort)fileHeader.Machine;
		var timeDateStamp = fileHeader.TimeDateStamp;
		var characteristics = (ushort)fileHeader.Characteristics;
		
		// Extract additional OptionalHeader fields
		var majorLinkerVersion = opt.MajorLinkerVersion;
		var minorLinkerVersion = opt.MinorLinkerVersion;
		var majorOperatingSystemVersion = opt.MajorOperatingSystemVersion;
		var minorOperatingSystemVersion = opt.MinorOperatingSystemVersion;
		var majorImageVersion = opt.MajorImageVersion;
		var minorImageVersion = opt.MinorImageVersion;
		var majorSubsystemVersion = opt.MajorSubsystemVersion;
		var minorSubsystemVersion = opt.MinorSubsystemVersion;
		var dllCharacteristics = (ushort)opt.DllCharacteristics;
		var checkSum = opt.CheckSum;
		var sectionAlignment = opt.SectionAlignment;
		var fileAlignment = opt.FileAlignment;
		
		// Base and size fields from optional header
		var baseOfCode = opt.BaseOfCode;
		var baseOfData = opt.BaseOfData; // PE32 only, will be 0 for PE32+
		var sizeOfCode = opt.SizeOfCode;
		var sizeOfInitializedData = opt.SizeOfInitializedData;
		var sizeOfUninitializedData = opt.SizeOfUninitializedData;

		// Extract section information for identifying code/data regions
		var sections = ExtractSectionInfo(pe, logger);

		return new LoadedImage(
			imageBase,
			entryPoint,
			imageSize,
			importMap,
			sourcePath,
			exportsByName,
			exportsByOrdinal,
			forwardedByName,
			forwardedByOrdinal,
			(ushort)subsystem,
			headerEndRva,
			sizeOfStackReserve,
			sizeOfStackCommit,
			sizeOfHeapReserve,
			sizeOfHeapCommit,
			tlsCallbacks,
			sections,
			iatEntryMap,
			exportMetadata,
			// FileHeader fields
			machine,
			timeDateStamp,
			characteristics,
			// OptionalHeader additional fields
			majorLinkerVersion,
			minorLinkerVersion,
			majorOperatingSystemVersion,
			minorOperatingSystemVersion,
			majorImageVersion,
			minorImageVersion,
			majorSubsystemVersion,
			minorSubsystemVersion,
			dllCharacteristics,
			checkSum,
			sectionAlignment,
			fileAlignment,
			baseOfCode,
			baseOfData,
			sizeOfCode,
			sizeOfInitializedData,
			sizeOfUninitializedData);
	}
	
	private (Dictionary<uint, (string dll, string name)> importMap, Dictionary<uint, uint> iatEntryMap) BuildImportMap(PEImage image, uint imageBase)
	{
		var map = new Dictionary<uint, (string dll, string name)>();
		var iatEntryMap = new Dictionary<uint, uint>(); // IAT VA -> expected synthetic address
		var imports = image.Imports; // IEnumerable<ImportModule>
		var synth = 0;
		
		// IMPORT HINTS: We do NOT use import hints for optimization
		// 
		// In a traditional Windows PE loader:
		// - Hints suggest the index in a DLL's export table where a function name might be found
		// - This speeds up the search from O(log n) binary search to O(1) if the hint is correct
		// 
		// In Win32Emu:
		// - We intercept ALL imports with synthetic addresses at load time
		// - We never load real DLLs or search their export tables
		// - Therefore, hints provide zero benefit
		// - We use symbol.Name (for named imports) or symbol.Ordinal (for ordinal imports)
		// 
		// See docs/implementation/IMPORT_HINTS.md for detailed explanation
		
		// First, create the syscall dispatcher stub at a fixed address
		// This stub will be hit by all import calls and will trigger our syscall handler
		// Format: INT 0x80 (syscall); RET (RET executes after the syscall handler returns control to the CPU/emulator)
		var syscallStub = new byte[]
		{
			0xCD, 0x80, // INT 0x80 - triggers syscall
			0xC3        // RET - return (won't execute in normal flow)
		};
		vm.WriteBytes(MemoryRegions.SyscallDispatcherAddress, syscallStub);
		logger?.LogInformation("[Loader] Created syscall dispatcher at 0x{Address:X8}", MemoryRegions.SyscallDispatcherAddress);
		
		// Track all IAT entry addresses to validate for duplicates or invalid entries
		var iatEntries = new HashSet<uint>();
		
		foreach (var module in imports)
		{
			var dll = module.Name ?? string.Empty;
			foreach (var sym in module.Symbols)
			{
				// Get IAT entry RVA - this is required to write the import stub address
				var rva = sym.AddressTableEntry?.Rva;
				if (rva is null or 0)
				{
					// Cannot process this import - no IAT entry location available
					// This can happen with delay-loaded imports or malformed PE files
					// Throw an error rather than silently skipping, as calling this import will crash
					var symName = sym.Name ?? $"Ordinal_{sym.Ordinal}";
					throw new InvalidOperationException(
						$"Cannot load PE file: Import {dll.ToUpperInvariant()}!{symName} has no AddressTableEntry RVA. " +
						$"This may indicate a delay-loaded import, bound import, or corrupted PE file. " +
						$"The emulator cannot safely load this executable.");
				}

				var va = imageBase + rva.Value;
				
				// VALIDATION: Check for duplicate IAT entries (potential corruption)
				if (iatEntries.Contains(va))
				{
					logger?.LogWarning("[Loader] Duplicate IAT entry detected at VA 0x{Va:X8} for {Dll}!{Name}. This may indicate PE corruption or incorrect parsing.", 
						va, dll.ToUpperInvariant(), sym.Name ?? $"Ordinal_{sym.Ordinal}");
				}
				iatEntries.Add(va);
				
				var synthetic = 0x0F000000u + (uint)(synth++ * 0x10u);
				
				// VALIDATION: Read existing value at IAT entry to check if it's already been written
				// A non-zero value here might indicate the IAT has already been processed or contains unexpected data
				var existingValue = vm.Read32(va);
				// Note: It's normal for some loaders to have non-zero values in IAT entries before processing
				// Only log if value seems unexpected (below image base threshold and not in emulator special ranges)
				// Values >= IMAGE_BASE_THRESHOLD (0x00400000) are typically valid pre-filled IAT entries or in image base range
				if (existingValue != 0 && existingValue < IMAGE_BASE_THRESHOLD)
				{
					logger?.LogDebug("[Loader] IAT entry at VA 0x{Va:X8} contains unusual value 0x{Value:X8} before writing synthetic address.", va, existingValue);
				}
				
				// Write the synthetic address to the IAT entry
				vm.Write32(va, synthetic);
				
				// Store IAT entry mapping for runtime verification
				iatEntryMap[va] = synthetic;
				
				// Verify the write was successful
				var verifyValue = vm.Read32(va);
				if (verifyValue != synthetic)
				{
					throw new InvalidOperationException(
						$"Failed to initialize IAT entry at VA 0x{va:X8} for {dll.ToUpperInvariant()}!{sym.Name ?? $"Ordinal_{sym.Ordinal}"}. " +
						$"Expected synthetic address 0x{synthetic:X8}, but read back 0x{verifyValue:X8}. " +
						$"This may indicate memory protection issues or PE loading conflicts.");
				}
				
				// Create import stub using retrowin32-style approach:
				// CALL [syscall_dispatcher]; RET argBytes
				// The RET instruction will be patched at runtime with the correct argBytes value
				// This allows proper stdcall stack cleanup
				//
				// Format:
				// - CALL to syscall dispatcher (5 bytes: E8 + rel32 offset)
				// - RET imm16 (3 bytes: C2 + imm16) - Will be patched at runtime with argBytes
				// 
				// Calculate relative offset from stub address to syscall dispatcher
				var stubAddr = synthetic;
				var callOffset = (int)(MemoryRegions.SyscallDispatcherAddress - (stubAddr + 5)); // +5 for size of CALL instruction
				
				var stub = new byte[]
				{
					0xE8, // CALL rel32
					(byte)(callOffset & 0xFF),
					(byte)((callOffset >> 8) & 0xFF),
					(byte)((callOffset >> 16) & 0xFF),
					(byte)((callOffset >> 24) & 0xFF),
					0xC2, 0x00, 0x00, // RET 0 - will be patched at runtime with actual argBytes
					// Padding to maintain 16-byte alignment
					0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90
				};
				vm.WriteBytes(synthetic, stub);
				
				// Use Ordinal for ordinal-based imports, NOT Hint
				// Ordinal is the actual export ordinal number (for imports by ordinal)
				// Hint is just an optimization suggestion for searching export tables
				var name = sym.Name ?? ($"Ordinal_{sym.Ordinal}");
				map[synthetic] = (dll.ToUpperInvariant(), name);
				logger?.LogTrace("[Loader] Mapped import #{Index}: {Dll}!{Name} at 0x{Synthetic:X8} -> syscall at 0x{Syscall:X8}", 
					synth - 1, dll.ToUpperInvariant(), name, synthetic, MemoryRegions.SyscallDispatcherAddress);
			}
		}
		
		// VALIDATION: Log summary of import mapping to detect anomalies
		logger?.LogInformation("[Loader] Import mapping complete: {Count} imports mapped to addresses 0x{StartAddr:X8} - 0x{LastAddr:X8}", 
			synth, MemoryRegions.ImportHookBase, synth > 0 ? MemoryRegions.ImportHookBase + (uint)((synth - 1) * MemoryRegions.ImportStubSize) : MemoryRegions.ImportHookBase);
		
		// VERIFICATION: Check all IAT entries to ensure they contain the correct synthetic addresses
		// This detects if anything corrupted the IAT after we wrote to it
		var verificationErrors = 0;
		var verificationIndex = 0;
		foreach (var module in imports)
		{
			var dll = module.Name ?? string.Empty;
			foreach (var sym in module.Symbols)
			{
				var rva = sym.AddressTableEntry?.Rva;
				if (rva is null or 0)
				{
					continue; // Skip entries without IAT slots
				}
				
				var va = imageBase + rva.Value;
				var expectedSynthetic = MemoryRegions.ImportHookBase + (uint)(verificationIndex * MemoryRegions.ImportStubSize);
				var actualValue = vm.Read32(va);
				
				if (actualValue != expectedSynthetic)
				{
					var symName = sym.Name ?? $"Ordinal_{sym.Ordinal}";
					logger?.LogError("[Loader] IAT VERIFICATION FAILED: {Dll}!{Name} at VA 0x{Va:X8} contains 0x{Actual:X8}, expected 0x{Expected:X8}", 
						dll.ToUpperInvariant(), symName, va, actualValue, expectedSynthetic);
					verificationErrors++;
					
					// FIX: Re-write the correct synthetic address
					logger?.LogWarning("[Loader] Fixing IAT entry at 0x{Va:X8}: writing 0x{Synthetic:X8}", va, expectedSynthetic);
					vm.Write32(va, expectedSynthetic);
				}
				
				verificationIndex++;
			}
		}
		
		if (verificationErrors > 0)
		{
			logger?.LogWarning("[Loader] Fixed {Count} corrupted IAT entries", verificationErrors);
		}
		else
		{
			logger?.LogInformation("[Loader] IAT verification passed: all {Count} entries are correct", verificationIndex);
		}
		
		// VALIDATION: Check if there are any IAT entries in memory beyond what we mapped
		// This could indicate extra entries that shouldn't exist
		if (synth > 0)
		{
			var maxMappedAddr = 0x0F000000u + (uint)((synth - 1) * 0x10u);
			var scanRangeEnd = 0x0F000000u + 0x1000u; // Scan up to 256 slots (indices 0-255)
			for (uint addr = maxMappedAddr + 0x10; addr < scanRangeEnd; addr += 0x10)
			{
				try
				{
					// Read first few bytes to check if there's any code/data at unmapped import addresses
					var byte1 = vm.Read8(addr);
					var byte2 = vm.Read8(addr + 1);
					// Check if it looks like it might be code (not all zeros)
					if (byte1 != 0 || byte2 != 0)
					{
						logger?.LogWarning("[Loader] Unexpected non-zero data at unmapped import address 0x{Addr:X8}: 0x{B1:X2} 0x{B2:X2}. This should be investigated.", 
							addr, byte1, byte2);
					}
				}
				catch (Exception ex)
				{
					// Memory not mapped at this address - this is expected and fine
					logger?.LogDebug("[Loader] Exception while reading unmapped import address 0x{Addr:X8}: {Message}", addr, ex.Message);
					break;
				}
			}
		}

		return (map, iatEntryMap);
	}

	private (Dictionary<string, uint> byName, Dictionary<uint, uint> byOrdinal, Dictionary<string, string> forwardedByName, Dictionary<uint, string> forwardedByOrdinal) BuildExportMaps(PEImage image, uint imageBase)
	{
		var byName = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		var byOrdinal = new Dictionary<uint, uint>();
		var forwardedByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var forwardedByOrdinal = new Dictionary<uint, string>();

		if (image.Exports == null)
		{
			return (byName, byOrdinal, forwardedByName, forwardedByOrdinal);
		}

		foreach (var export in image.Exports.Entries)
		{
			// Check if this is a forwarded export
			if (export.IsForwarder)
			{
				// Store forwarded export information
				forwardedByOrdinal[export.Ordinal] = export.ForwarderName;
				
				if (!string.IsNullOrEmpty(export.Name))
				{
					forwardedByName[export.Name] = export.ForwarderName;
				}
				continue;
			}

			// Skip exports with no RVA (shouldn't happen for non-forwarded exports)
			if (export.Address == null || !export.Address.IsBounded)
			{
				continue;
			}

			var rva = export.Address.Rva;
			var va = imageBase + rva;

			// Add by ordinal
			byOrdinal[export.Ordinal] = va;

			// Add by name if it has one
			if (!string.IsNullOrEmpty(export.Name))
			{
				byName[export.Name] = va;
			}
		}

		return (byName, byOrdinal, forwardedByName, forwardedByOrdinal);
	}

	/// <summary>
	/// Builds export metadata (calling convention, arg bytes) from PE exports.
	/// Attempts to infer calling convention from export name decoration.
	/// </summary>
	private Dictionary<string, ExportMetadata> BuildExportMetadata(PEImage image, ILogger? logger)
	{
		var metadata = new Dictionary<string, ExportMetadata>(StringComparer.OrdinalIgnoreCase);

		if (image.Exports == null)
		{
			return metadata;
		}

		foreach (var export in image.Exports.Entries)
		{
			// Skip forwarded exports - they don't need metadata
			if (export.IsForwarder)
			{
				continue;
			}

			// Only process exports with names
			if (string.IsNullOrEmpty(export.Name))
			{
				continue;
			}

			// Try to parse calling convention from decorated name
			var exportMeta = ExportMetadata.FromDecoratedName(export.Name);
			
			if (exportMeta != null)
			{
				// Successfully inferred from decoration
				metadata[export.Name] = exportMeta;
				logger?.LogDebug("[Loader] Export '{Name}' inferred as {Convention} with {ArgBytes} stack bytes", 
					export.Name, exportMeta.Convention, exportMeta.StackArgBytes);
			}
			else
			{
				// No decoration found - use default (stdcall with 0 args)
				// This is a safe default but may be incorrect for cdecl functions
				metadata[export.Name] = ExportMetadata.Default;
				logger?.LogDebug("[Loader] Export '{Name}' has no decoration, using default {Convention}", 
					export.Name, ExportMetadata.Default.Convention);
			}
		}

		logger?.LogInformation("[Loader] Built metadata for {Count} exports", metadata.Count);
		return metadata;
	}

	/// <summary>
	/// Extracts TLS (Thread Local Storage) callback function addresses from the PE image.
	/// TLS callbacks are executed before the main entry point and on thread attach/detach events.
	/// </summary>
	/// <param name="image">The PE image to extract TLS callbacks from</param>
	/// <param name="imageBase">The base address where the image is loaded</param>
	/// <param name="vm">Virtual memory (unused, kept for consistency)</param>
	/// <param name="logger">Logger for diagnostic messages</param>
	/// <returns>Array of TLS callback virtual addresses</returns>
	private static uint[] ExtractTlsCallbacks(PEImage image, uint imageBase, VirtualMemory vm, ILogger? logger)
	{
		var callbacks = new List<uint>();

		// Check if the image has a TLS directory
		var tlsDirectory = image.TlsDirectory;
		if (tlsDirectory == null)
		{
			logger?.LogDebug("[Loader] No TLS directory found in PE image");
			return callbacks.ToArray();
		}

		logger?.LogInformation("[Loader] TLS directory found, extracting callbacks");

		// Get the callback functions from the TLS directory
		var callbackFunctions = tlsDirectory.CallbackFunctions;
		if (callbackFunctions == null)
		{
			logger?.LogDebug("[Loader] TLS directory has no callback functions");
			return callbacks.ToArray();
		}

		// Extract callback addresses
		// CallbackFunctions contains ISegmentReferences which have RVAs
		// We need to convert these to VAs by adding the image base
		var index = 0;
		foreach (var callback in callbackFunctions)
		{
			if (callback != null && callback.IsBounded)
			{
				var callbackRva = callback.Rva;
				var callbackVa = imageBase + callbackRva;
				callbacks.Add(callbackVa);
				logger?.LogInformation("[Loader] TLS callback #{Index} at VA 0x{CallbackVa:X8} (RVA 0x{CallbackRva:X8})", 
					index, callbackVa, callbackRva);
				index++;
			}
			else
			{
				logger?.LogWarning("[Loader] TLS callback #{Index} is null or unbounded, skipping", index);
				index++;
			}

			// Safety check: prevent infinite loops in case of corrupted PE files
			if (index >= MAX_TLS_CALLBACKS)
			{
				logger?.LogWarning("[Loader] TLS callback array exceeds maximum size ({Max} entries), stopping extraction", MAX_TLS_CALLBACKS);
				break;
			}
		}

		logger?.LogInformation("[Loader] Extracted {Count} TLS callbacks", callbacks.Count);
		return callbacks.ToArray();
	}

	/// <summary>
	/// Applies base relocations to the loaded PE image if it's loaded at a different address than preferred.
	/// Base relocations fix up absolute addresses in the code and data sections when the image cannot be
	/// loaded at its preferred ImageBase address.
	/// </summary>
	/// <param name="image">The PE image containing relocation information</param>
	/// <param name="actualBase">The actual address where the image is loaded</param>
	/// <param name="preferredBase">The preferred ImageBase address from the PE header</param>
	/// <param name="vm">Virtual memory to apply relocations to</param>
	/// <param name="logger">Logger for diagnostic messages</param>
	private static void ApplyRelocations(PEImage image, uint actualBase, ulong preferredBase, VirtualMemory vm, ILogger? logger)
	{
		// Calculate the delta (difference) between actual and preferred base addresses
		// Note: For PE32, both values should fit in 32 bits
		var delta = (long)actualBase - (long)preferredBase;
		
		// If loaded at preferred base, no relocations needed
		if (delta == 0)
		{
			logger?.LogDebug("[Loader] Image loaded at preferred base 0x{PreferredBase:X8}, no relocations needed", preferredBase);
			return;
		}

		// Check if relocations are available
		if (image.Relocations == null || image.Relocations.Count == 0)
		{
			logger?.LogWarning("[Loader] Image loaded at 0x{ActualBase:X8} instead of preferred 0x{PreferredBase:X8}, but no relocations available. Image may not function correctly.", 
				actualBase, preferredBase);
			return;
		}

		logger?.LogInformation("[Loader] Applying {Count} base relocations (delta: 0x{Delta:X})", image.Relocations.Count, delta);

		var relocationsApplied = 0;
		var relocationsFailed = 0;

		// Process each relocation entry
		foreach (var relocation in image.Relocations)
		{
			try
			{
				// Get the RVA of the location to be relocated
				if (!TryGetRvaFromLocation(relocation.Location, out var rva))
				{
					logger?.LogWarning("[Loader] Skipping relocation with unsupported or null location type: {Type}", 
						relocation.Location?.GetType().Name ?? "null");
					relocationsFailed++;
					continue;
				}

				var va = actualBase + rva;

				// Apply the relocation based on its type
				switch (relocation.Type)
				{
					case RelocationType.Absolute:
						// IMAGE_REL_BASED_ABSOLUTE (0): No-op, used for padding
						break;

					case RelocationType.HighLow:
						// IMAGE_REL_BASED_HIGHLOW (3): Apply all 32 bits of delta
						// This is the most common relocation type for PE32
						{
							var originalValue = vm.Read32((ulong)va);
							var newValue = (uint)((long)originalValue + delta);
							vm.Write32((ulong)va, newValue);
							relocationsApplied++;
							logger?.LogTrace("[Loader] Applied HIGHLOW relocation at RVA 0x{Rva:X8} (VA 0x{Va:X8}): 0x{Original:X8} -> 0x{New:X8}", 
								rva, va, originalValue, newValue);
						}
						break;

					case RelocationType.High:
						// IMAGE_REL_BASED_HIGH (1): Apply high 16 bits of delta to high 16 bits
						{
							var originalValue = vm.Read16((ulong)va);
							var newValue = (ushort)(originalValue + (delta >> 16));
							vm.Write16((ulong)va, newValue);
							relocationsApplied++;
							logger?.LogTrace("[Loader] Applied HIGH relocation at RVA 0x{Rva:X8} (VA 0x{Va:X8}): 0x{Original:X4} -> 0x{New:X4}", 
								rva, va, originalValue, newValue);
						}
						break;

					case RelocationType.Low:
						// IMAGE_REL_BASED_LOW (2): Apply low 16 bits of delta to low 16 bits
						{
							var originalValue = vm.Read16((ulong)va);
							var newValue = (ushort)(originalValue + (delta & 0xFFFF));
							vm.Write16((ulong)va, newValue);
							relocationsApplied++;
							logger?.LogTrace("[Loader] Applied LOW relocation at RVA 0x{Rva:X8} (VA 0x{Va:X8}): 0x{Original:X4} -> 0x{New:X4}", 
								rva, va, originalValue, newValue);
						}
						break;

					case RelocationType.Dir64:
						// IMAGE_REL_BASED_DIR64 (10): Apply 64-bit delta (PE32+ only, but handle for completeness)
						// This should not occur in PE32 files, but if it does, log a warning
						logger?.LogWarning("[Loader] Encountered DIR64 relocation in PE32 image at RVA 0x{Rva:X8}, skipping", rva);
						relocationsFailed++;
						break;

					case RelocationType.HighAdj:
						// IMAGE_REL_BASED_HIGHADJ (4): Complex relocation occupying two slots
						// This is rarely used and requires special handling
						logger?.LogWarning("[Loader] Encountered unsupported HIGHADJ relocation at RVA 0x{Rva:X8}, skipping", rva);
						relocationsFailed++;
						break;

					default:
						// Unknown or unsupported relocation type
						logger?.LogWarning("[Loader] Encountered unknown relocation type {Type} at RVA 0x{Rva:X8}, skipping", 
							relocation.Type, rva);
						relocationsFailed++;
						break;
				}
			}
			catch (Exception ex)
			{
				// Try to get RVA for error logging, default to 0 if unavailable
				TryGetRvaFromLocation(relocation.Location, out var errorRva);
				logger?.LogError(ex, "[Loader] Failed to apply relocation at RVA 0x{Rva:X8}", errorRva);
				relocationsFailed++;
			}
		}

		logger?.LogInformation("[Loader] Relocations complete: {Applied} applied, {Failed} failed", 
			relocationsApplied, relocationsFailed);
	}

	/// <summary>
	/// Extracts the RVA (Relative Virtual Address) from an ISegmentReference.
	/// Handles the concrete types that implement ISegmentReference.
	/// </summary>
	/// <param name="location">The segment reference to extract RVA from</param>
	/// <param name="rva">The extracted RVA, or 0 if extraction failed</param>
	/// <returns>True if RVA was successfully extracted, false otherwise</returns>
	private static bool TryGetRvaFromLocation(ISegmentReference? location, out uint rva)
	{
		rva = 0;
		
		if (location == null)
		{
			return false;
		}

		if (location is SegmentReference segRef)
		{
			rva = segRef.Rva;
			return true;
		}
		
		if (location is RelativeReference relRef)
		{
			rva = relRef.Rva;
			return true;
		}
		
		if (location is VirtualAddress virtAddr)
		{
			rva = virtAddr.Rva;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Extracts section information from the PE file, including name, address, size, and characteristics.
	/// This information is used to identify code vs data regions, executable sections, etc.
	/// </summary>
	/// <param name="pe">The PE file to extract sections from</param>
	/// <param name="logger">Logger for diagnostic messages</param>
	/// <returns>Array of PeSection records describing each section</returns>
	private static PeSection[] ExtractSectionInfo(PEFile pe, ILogger? logger)
	{
		var sections = new List<PeSection>();

		foreach (var section in pe.Sections)
		{
			var name = section.Name ?? string.Empty;
			var rva = section.Rva;
			var virtualSize = section.Contents?.GetVirtualSize() ?? 0;
			// Note: WriteIntoArray() creates a copy to get the length. This is acceptable since
			// it only happens once during PE load time (not performance-critical path).
			// AsmResolver's PESection doesn't expose raw data size directly - must materialize contents.
			var rawSize = (uint)(section.Contents?.WriteIntoArray().Length ?? 0);
			var characteristics = (PeSectionCharacteristics)(uint)section.Characteristics;

			sections.Add(new PeSection(name, rva, virtualSize, rawSize, characteristics));

			logger?.LogDebug("[Loader] Section {Name}: RVA=0x{Rva:X8}, VirtualSize=0x{VSize:X8}, RawSize=0x{RawSize:X8}, Characteristics=0x{Chars:X8}",
				name, rva, virtualSize, rawSize, (uint)characteristics);
		}

		logger?.LogInformation("[Loader] Extracted {Count} sections from PE file", sections.Count);
		return sections.ToArray();
	}
}