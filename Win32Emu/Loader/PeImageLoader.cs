using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using Microsoft.Extensions.Logging;
using Win32Emu.Memory;
using System.IO;

namespace Win32Emu.Loader;

/// <summary>
/// PE loader using a single PEImage load. Maps section raw data and replaces IAT entries with synthetic
/// addresses for interception via an import map. Relocations not yet handled.
/// </summary>
public class PeImageLoader(VirtualMemory vm, ILogger? logger = null)
{
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
		var pe = image.PEFile ?? throw new InvalidOperationException("PEImage missing PEFile.");
		var opt = pe.OptionalHeader ?? throw new InvalidOperationException("Missing optional header.");

		if(opt.Magic != OptionalHeaderMagic.PE32)
		{
			throw new NotSupportedException("Only PE32 format is supported.");
		}

		var imageBase = (uint)opt.ImageBase;
		var entryPoint = imageBase + opt.AddressOfEntryPoint;
		var imageSize = opt.SizeOfImage;
		var subsystem = (ushort)opt.SubSystem;

		// Load PE headers into memory at the image base
		// The Windows PE loader maps headers into memory, which some programs may read
		// Headers include DOS header, NT headers, section table, etc.
		var sizeOfHeaders = opt.SizeOfHeaders;
		
		// Safety check: ensure SizeOfHeaders is reasonable (typically <= 4KB for most PE files)
		// Some malformed PE files might have invalid SizeOfHeaders values
		if (sizeOfHeaders > 0 && sizeOfHeaders <= 0x10000) // Cap at 64KB
		{
			logger?.LogDebug("[Loader] Loading PE headers: Size=0x{HeaderSize:X8} at ImageBase=0x{ImageBase:X8}", sizeOfHeaders, imageBase);
			
			try
			{
				// Additional safety: ensure we don't overwrite any sections
				// Headers should end before the first section starts
				uint firstSectionRva = uint.MaxValue;
				foreach (var section in pe.Sections)
				{
					if (section.Rva < firstSectionRva)
					{
						firstSectionRva = section.Rva;
					}
				}
				
				// If no sections found or all sections start after SizeOfHeaders, use SizeOfHeaders
				if (firstSectionRva == uint.MaxValue || firstSectionRva > sizeOfHeaders)
				{
					firstSectionRva = sizeOfHeaders;
				}
				
				// Safely calculate header size, ensuring we don't overflow int.MaxValue
				long headerSizeLong = Math.Min((long)sizeOfHeaders, (long)firstSectionRva);
				if (headerSizeLong > int.MaxValue)
				{
					logger?.LogWarning("[Loader] Calculated header size (0x{Size:X8}) exceeds int.MaxValue, capping at 0x{Max:X8}", headerSizeLong, int.MaxValue);
					headerSizeLong = int.MaxValue;
				}
				var actualHeaderSize = (int)headerSizeLong;
				
				if (actualHeaderSize < sizeOfHeaders)
				{
					logger?.LogWarning("[Loader] SizeOfHeaders (0x{Size:X8}) extends beyond first section RVA (0x{FirstRva:X8}), truncating to first section", sizeOfHeaders, firstSectionRva);
				}
				
				// Read only the required header bytes from the file
				var headerData = new byte[actualHeaderSize];
				using (var fileStream = File.OpenRead(path))
				{
					var bytesRead = fileStream.Read(headerData, 0, actualHeaderSize);
					if (bytesRead < actualHeaderSize)
					{
						logger?.LogWarning("[Loader] Only read 0x{BytesRead:X8} of 0x{Expected:X8} header bytes", bytesRead, actualHeaderSize);
					}
				}
				
				vm.WriteBytes(imageBase, headerData);
				logger?.LogDebug("[Loader] Loaded 0x{Size:X8} bytes of PE headers", headerData.Length);
			}
			catch (Exception ex)
			{
				logger?.LogWarning("[Loader] Failed to load PE headers: {ErrorMessage}", ex.Message);
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
				
				// Write the raw data from the file
				vm.WriteBytes(imageBase + sectionRva, rawData);
				
				// If VirtualSize is larger than raw data size, the extra bytes should remain zero
				// (VirtualMemory already initializes to zero, so we don't need to explicitly zero-fill)
				if (virtualSize > rawData.Length)
				{
					logger?.LogDebug("[Loader] Section {SectionName} has VirtualSize (0x{VSize:X8}) > RawDataSize (0x{RawSize:X8}), extra 0x{Extra:X8} bytes remain zero-filled", 
						section.Name, virtualSize, rawData.Length, virtualSize - (uint)rawData.Length);
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

		var importMap = BuildImportMap(image, imageBase);
		var (exportsByName, exportsByOrdinal, forwardedByName, forwardedByOrdinal) = BuildExportMaps(image, imageBase);
		return new LoadedImage(imageBase, entryPoint, imageSize, importMap, path, exportsByName, exportsByOrdinal, forwardedByName, forwardedByOrdinal, (ushort)subsystem);
	}

	// Syscall dispatcher address - this is where all import stubs will call into
	private const uint SYSCALL_DISPATCHER_ADDRESS = 0x0E000000;
	
	private Dictionary<uint, (string dll, string name)> BuildImportMap(PEImage image, uint imageBase)
	{
		var map = new Dictionary<uint, (string dll, string name)>();
		var imports = image.Imports; // IEnumerable<ImportModule>
		var synth = 0;
		
		// First, create the syscall dispatcher stub at a fixed address
		// This stub will be hit by all import calls and will trigger our syscall handler
		// Format: INT 0x80 (syscall); RET (RET executes after the syscall handler returns control to the CPU/emulator)
		var syscallStub = new byte[]
		{
			0xCD, 0x80, // INT 0x80 - triggers syscall
			0xC3        // RET - return (won't execute in normal flow)
		};
		vm.WriteBytes(SYSCALL_DISPATCHER_ADDRESS, syscallStub);
		logger?.LogInformation("[Loader] Created syscall dispatcher at 0x{Address:X8}", SYSCALL_DISPATCHER_ADDRESS);
		
		// Track all IAT entry addresses to validate for duplicates or invalid entries
		var iatEntries = new HashSet<uint>();
		
		foreach (var module in imports)
		{
			var dll = module.Name ?? string.Empty;
			foreach (var sym in module.Symbols)
			{
				// Prefer IAT entry RVA when available.
				var rva = sym.AddressTableEntry?.Rva; // fallback
				if (rva is null or 0)
				{
					continue;
				}

				var va = imageBase + rva.Value;
				
				// VALIDATION: Check for duplicate IAT entries (potential corruption)
				if (iatEntries.Contains(va))
				{
					logger?.LogWarning("[Loader] Duplicate IAT entry detected at VA 0x{Va:X8} for {Dll}!{Name}. This may indicate PE corruption or incorrect parsing.", 
						va, dll.ToUpperInvariant(), sym.Name ?? $"Ordinal_{sym.Hint}");
				}
				iatEntries.Add(va);
				
				var synthetic = 0x0F000000u + (uint)(synth++ * 0x10u);
				
				// VALIDATION: Read existing value at IAT entry to check if it's already been written
				// A non-zero value here might indicate the IAT has already been processed or contains unexpected data
				var existingValue = vm.Read32(va);
				if (existingValue != 0)
				{
					logger?.LogDebug("[Loader] IAT entry at VA 0x{Va:X8} already contains value 0x{Value:X8} before writing synthetic address. This is normal for some loaders.", va, existingValue);
				}
				
				// Write the synthetic address to the IAT entry
				vm.Write32(va, synthetic);
				
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
				var callOffset = (int)(SYSCALL_DISPATCHER_ADDRESS - (stubAddr + 5)); // +5 for size of CALL instruction
				
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
				
				var name = sym.Name ?? ($"Ordinal_{sym.Hint}");
				map[synthetic] = (dll.ToUpperInvariant(), name);
				logger?.LogTrace("[Loader] Mapped import #{Index}: {Dll}!{Name} at 0x{Synthetic:X8} -> syscall at 0x{Syscall:X8}", 
					synth - 1, dll.ToUpperInvariant(), name, synthetic, SYSCALL_DISPATCHER_ADDRESS);
			}
		}
		
		// VALIDATION: Log summary of import mapping to detect anomalies
		logger?.LogInformation("[Loader] Import mapping complete: {Count} imports mapped to addresses 0x0F000000 - 0x{LastAddr:X8}", 
			synth, synth > 0 ? 0x0F000000u + (uint)((synth - 1) * 0x10u) : 0x0F000000u);
		
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

		return map;
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
}