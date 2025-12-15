using Microsoft.Extensions.Logging;
using Win32Emu.Memory;
using Win32Emu.NeParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Win32Emu.Loader;

/// <summary>
/// NE (New Executable) loader for Win16 applications.
/// Supports loading 16-bit Windows executables in the NE format.
/// Uses Win32Emu.NeParser for parsing NE structures.
/// </summary>
public class NeImageLoader(VirtualMemory vm, ILogger? logger = null)
{
	// Base address for NE executables (64KB to avoid NULL pointer conflicts)
	private const uint NE_BASE_ADDRESS = 0x00010000;
	
	// Full segment size for NE executables (64KB)
	private const uint FULL_SEGMENT_SIZE = 0x10000;
	
	// Paragraph alignment (16 bytes)
	private const uint PARAGRAPH_MASK = 0xF;
	private const uint PARAGRAPH_ALIGN = 0xFFFFFFF0;
	
	// NE segment flags (for CreateSectionsFromSegments)
	private const ushort NE_SEGMENT_DATA = 0x0001;
	private const ushort NE_SEGMENT_READONLY = 0x0008;
	
	// NE relocation entry size
	private const int NE_RELOCATION_ENTRY_SIZE = 8;
	
	// Minimum bytes per relocation fixup (used to calculate maximum reasonable relocations)
	private const int MIN_BYTES_PER_RELOCATION = 2;
	
	// Maximum reasonable relocations fallback (used when segment length is 0)
	private const int MAX_REASONABLE_RELOCATIONS_FALLBACK = 1000;
	
	// PE subsystem values
	private const ushort PE_SUBSYSTEM_CUI = 2;
	private const ushort PE_SUBSYSTEM_GUI = 3;
	
	// PE default sizes
	private const uint DEFAULT_HEADER_END_RVA = 0x1000;
	private const uint DEFAULT_STACK_RESERVE = 0x10000;
	private const uint DEFAULT_STACK_COMMIT = 0x1000;
	private const uint DEFAULT_HEAP_RESERVE = 0x10000;
	private const uint DEFAULT_HEAP_COMMIT = 0x1000;
	private const uint DEFAULT_SECTION_ALIGNMENT = 0x1000;
	private const uint DEFAULT_FILE_ALIGNMENT = 0x200;
	
	// PE machine type and characteristics
	private const ushort IMAGE_FILE_MACHINE_I386 = 0x014C;
	private const ushort IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002;
	
	/// <summary>
	/// Validates if a file is a valid NE (Win16) executable by checking the NE signature.
	/// </summary>
	/// <param name="path">Path to the executable file</param>
	/// <returns>True if the file is a valid NE executable, false otherwise</returns>
	public static bool IsNE(string path)
	{
		return NeParser.NeParser.IsNE(path);
	}
	
	/// <summary>
	/// Validates if a byte array contains a valid NE executable.
	/// </summary>
	/// <param name="bytes">The byte array to validate</param>
	/// <returns>True if the byte array contains a valid NE executable, false otherwise</returns>
	public static bool IsNE(byte[] bytes)
	{
		return NeParser.NeParser.IsNE(bytes);
	}
	
	public LoadedImage Load(string path)
	{
		var bytes = File.ReadAllBytes(path);
		return LoadFromBytes(bytes, path);
	}
	
	public LoadedImage LoadFromBytes(byte[] bytes, string sourcePath = "<memory>")
	{
		logger?.LogInformation("[NE Loader] Loading NE executable from {Path}", sourcePath);
		
		// Parse NE executable using NeParser library
		var neExe = NeParser.NeParser.Parse(bytes);
		var neHeader = neExe.Header;
		
		logger?.LogInformation("[NE Loader] NE version: {Major}.{Minor}", neHeader.MajorLinkerVersion, neHeader.MinorLinkerVersion);
		logger?.LogInformation("[NE Loader] Target OS: {TargetOS} (2=Windows)", neHeader.TargetOS);
		logger?.LogInformation("[NE Loader] Program flags: 0x{Flags:X4}", neHeader.ProgramFlags);
		logger?.LogInformation("[NE Loader] Application type: 0x{Type:X4}", neHeader.ApplicationType);
		logger?.LogInformation("[NE Loader] Auto data segment: {AutoData}, Init heap: {Heap}, Init stack: {Stack}",
			neHeader.AutoDataSegment, neHeader.InitHeapSize, neHeader.InitStackSize);
		logger?.LogInformation("[NE Loader] Sector alignment shift: {Shift} (sector size: {Size})",
			neHeader.SectorAlignmentShift, 1 << neHeader.SectorAlignmentShift);
		logger?.LogInformation("[NE Loader] Loaded {Count} segments", neExe.Segments.Length);
		logger?.LogInformation("[NE Loader] Found {Count} entry points", neExe.EntryPoints.Count);
		logger?.LogInformation("[NE Loader] Resident names: {Resident}, Non-resident names: {NonResident}", 
			neExe.ResidentNames.Count, neExe.NonResidentNames.Count);
		logger?.LogInformation("[NE Loader] Import modules: {Count}", neExe.ImportModules.Count);
		
		// Calculate base address for 16-bit segments
		// NE executables typically use a separate 16-bit address space
		// We'll map them starting at 64KB to avoid conflicts with NULL pointers
		uint baseAddress = NE_BASE_ADDRESS;
		
		// Load segments into memory
		uint currentAddress = baseAddress;
		var segmentMap = new Dictionary<int, (uint address, uint size)>();
		
		foreach (var segment in neExe.Segments)
		{
			// Calculate memory allocation size
			var memorySize = segment.MinAllocation > 0 ? Math.Max(segment.Length, segment.MinAllocation) : segment.Length;
			if (memorySize == 0)
			{
				memorySize = FULL_SEGMENT_SIZE; // 64KB for zero-length segments with allocation
			}
			
			// Align to paragraph boundary (16 bytes)
			currentAddress = (currentAddress + PARAGRAPH_MASK) & PARAGRAPH_ALIGN;
			
			// Store segment mapping with allocated size
			segmentMap[segment.SegmentNumber] = (currentAddress, memorySize);
			
			// Load segment data from file if present
			if (segment.FileOffset > 0 && segment.Length > 0 && segment.FileOffset + segment.Length <= bytes.Length)
			{
				var segmentData = new byte[segment.Length];
				Array.Copy(bytes, segment.FileOffset, segmentData, 0, segment.Length);
				vm.WriteBytes(currentAddress, segmentData);
				
				var segFlags = (NeParser.NeSegmentFlags)segment.Flags;
				logger?.LogDebug("[NE Loader] Loaded segment {Num}: Address=0x{Addr:X8}, FileSize=0x{FileSize:X4}, MemSize=0x{MemSize:X4}, Flags={Flags}",
					segment.SegmentNumber, currentAddress, segment.Length, memorySize, segFlags);
			}
			else if (memorySize > 0)
			{
				// Zero-initialize BSS segments
				logger?.LogDebug("[NE Loader] Initialized segment {Num}: Address=0x{Addr:X8}, MemSize=0x{MemSize:X4} (no file data)",
					segment.SegmentNumber, currentAddress, memorySize);
			}
			
			currentAddress += memorySize;
		}
		
		// Process segment relocations
		ProcessRelocations(bytes, neHeader, neExe.Segments, segmentMap, neExe.ImportModules);
		
		// Calculate entry point address
		// Entry point is specified as segment:offset in NE format
		uint entryPointAddress = 0;
		if (neHeader.EntryPointSegment > 0 && neHeader.EntryPointSegment <= neExe.Segments.Length &&
			segmentMap.TryGetValue(neHeader.EntryPointSegment, out var entrySegment))
		{
			entryPointAddress = entrySegment.address + neHeader.EntryPointOffset;
			logger?.LogInformation("[NE Loader] Entry point: Segment {Seg}, Offset 0x{Off:X4} -> VA 0x{VA:X8}",
				neHeader.EntryPointSegment, neHeader.EntryPointOffset, entryPointAddress);
		}
		
		// Create import map for Win16 API calls
		var importMap = BuildImportMap(neExe.ImportModules, neExe.ResidentNames, neExe.NonResidentNames, neExe.EntryPoints);
		
		// Build export maps from resident and non-resident name tables
		var (exportsByName, exportsByOrdinal) = BuildExportMaps(neExe.ResidentNames, neExe.NonResidentNames, segmentMap, neExe.EntryPoints);
		
		// Create PE sections from NE segments for compatibility
		var sections = CreateSectionsFromSegments(neExe.Segments, segmentMap, baseAddress);
		
		uint imageSize = currentAddress - baseAddress;
		
		// Return LoadedImage compatible with existing infrastructure
		// Note: Some PE-specific fields are set to defaults for NE executables
		return new LoadedImage(
			baseAddress,
			entryPointAddress,
			imageSize,
			importMap,
			sourcePath,
			exportsByName,
			exportsByOrdinal,
			new Dictionary<string, string>(), // No forwarded exports in NE
			new Dictionary<uint, string>(),   // No forwarded exports by ordinal
			// Map NE application type to PE subsystem
			// NE: 2=Windows GUI compatible, 3=Uses PM (GUI) API
			// PE: 2=WINDOWS_GUI, 3=WINDOWS_CUI (console)
			(ushort)((neHeader.ApplicationType & 0x03) >= 2 ? PE_SUBSYSTEM_GUI : PE_SUBSYSTEM_CUI),
			0,                                // HeaderEndRva - NE has no PE header, code can start at base address
			DEFAULT_STACK_RESERVE,            // SizeOfStackReserve - 64KB default for 16-bit
			DEFAULT_STACK_COMMIT,             // SizeOfStackCommit - 4KB default
			DEFAULT_HEAP_RESERVE,             // SizeOfHeapReserve - 64KB default
			DEFAULT_HEAP_COMMIT,              // SizeOfHeapCommit - 4KB default
			Array.Empty<uint>(),              // No TLS callbacks in NE
			sections,
			new Dictionary<uint, uint>(),     // IAT entry map - handled differently for NE
			new Dictionary<string, ExportMetadata>(), // Export metadata - TODO: infer from calling conventions
			// FileHeader fields
			IMAGE_FILE_MACHINE_I386,          // Machine
			0,                                // TimeDateStamp
			IMAGE_FILE_EXECUTABLE_IMAGE,      // Characteristics
			// OptionalHeader additional fields
			neHeader.MajorLinkerVersion,
			neHeader.MinorLinkerVersion,
			(ushort)(neHeader.ExpectedWindowsVersion >> 8),      // Major OS version
			(ushort)(neHeader.ExpectedWindowsVersion & 0xFF),    // Minor OS version
			0,                                // MajorImageVersion
			0,                                // MinorImageVersion
			(ushort)(neHeader.ExpectedWindowsVersion >> 8),      // MajorSubsystemVersion
			(ushort)(neHeader.ExpectedWindowsVersion & 0xFF),    // MinorSubsystemVersion
			0,                                // DllCharacteristics
			0,                                // CheckSum
			DEFAULT_SECTION_ALIGNMENT,        // SectionAlignment - 4KB
			DEFAULT_FILE_ALIGNMENT,           // FileAlignment - 512 bytes
			baseAddress,                      // BaseOfCode
			baseAddress,                      // BaseOfData
			imageSize,                        // SizeOfCode
			0,                                // SizeOfInitializedData
			0                                 // SizeOfUninitializedData
		);
	}
	
	/// <summary>
	/// Process segment relocations for all segments that have relocation data.
	/// </summary>
	private void ProcessRelocations(byte[] bytes, NeParser.NeHeader header, NeParser.NeSegment[] segments, 
		Dictionary<int, (uint address, uint size)> segmentMap, List<string> importModules)
	{
		foreach (var segment in segments)
		{
			// Check if segment has relocations
			if ((segment.Flags & (ushort)NeParser.NeSegmentFlags.HasRelocations) == 0)
			{
				continue;
			}
			
			if (!segmentMap.TryGetValue(segment.SegmentNumber, out var segInfo))
			{
				continue;
			}
			
			// Relocation data follows segment data
			var relocationOffset = segment.FileOffset + segment.Length;
			if (relocationOffset + 2 > bytes.Length)
			{
				logger?.LogWarning("[NE Loader] Segment {Num} relocation data extends beyond file", segment.SegmentNumber);
				continue;
			}
			
			// Read relocation count
			var relocationCount = BitConverter.ToUInt16(bytes, (int)relocationOffset);
			relocationOffset += 2;
			
			// Validate relocation count - if it's unreasonably large, the segment likely doesn't have relocations
			// or the data is corrupted. A reasonable upper bound is the segment size divided by minimum bytes per fixup
			var maxReasonableRelocations = segment.Length > 0 ? segment.Length / MIN_BYTES_PER_RELOCATION : MAX_REASONABLE_RELOCATIONS_FALLBACK;
			if (relocationCount > maxReasonableRelocations)
			{
				logger?.LogWarning("[NE Loader] Segment {Num} has suspicious relocation count {Count} (max reasonable: {Max}), skipping relocations",
					segment.SegmentNumber, relocationCount, maxReasonableRelocations);
				continue;
			}
			
			// Also check if we have enough space for all relocations
			var requiredSpace = relocationOffset + (relocationCount * NE_RELOCATION_ENTRY_SIZE);
			if (requiredSpace > bytes.Length)
			{
				logger?.LogWarning("[NE Loader] Segment {Num} relocation table extends beyond file (needs {Required} bytes, have {Available})",
					segment.SegmentNumber, requiredSpace, bytes.Length);
				continue;
			}
			
			logger?.LogDebug("[NE Loader] Processing {Count} relocations for segment {Num}", relocationCount, segment.SegmentNumber);
			
			// Process each relocation entry
			for (var i = 0; i < relocationCount; i++)
			{
				if (relocationOffset + NE_RELOCATION_ENTRY_SIZE > bytes.Length)
				{
					logger?.LogWarning("[NE Loader] Relocation entry {Index} extends beyond file", i);
					break;
				}
				
				var reloc = new NeRelocation
				{
					SourceType = bytes[relocationOffset],
					TargetFlags = bytes[relocationOffset + 1],
					SourceOffset = BitConverter.ToUInt16(bytes, (int)relocationOffset + 2),
					TargetSegment = BitConverter.ToUInt16(bytes, (int)relocationOffset + 4),
					TargetOffset = BitConverter.ToUInt16(bytes, (int)relocationOffset + 6)
				};
				
				ApplyRelocation(reloc, segInfo.address, segmentMap, importModules, bytes, header);
				relocationOffset += NE_RELOCATION_ENTRY_SIZE;
			}
		}
	}
	
	/// <summary>
	/// Apply a single relocation fixup.
	/// </summary>
	private void ApplyRelocation(NeRelocation reloc, uint segmentAddress, 
		Dictionary<int, (uint address, uint size)> segmentMap, List<string> importModules,
		byte[] bytes, NeParser.NeHeader header)
	{
		var targetType = (NeParser.NeRelocationTargetType)(reloc.TargetFlags & 0x03);
		var isAdditive = (reloc.TargetFlags & 0x04) != 0;
		var sourceType = (NeParser.NeRelocationSourceType)(reloc.SourceType & 0x0F);
		
		// Validate source type - if it's not a known type, skip this relocation
		// Using switch expression for better performance than Enum.IsDefined
		var isValidSourceType = sourceType switch
		{
			NeParser.NeRelocationSourceType.LoByte => true,
			NeParser.NeRelocationSourceType.Selector => true,
			NeParser.NeRelocationSourceType.Pointer32 => true,
			NeParser.NeRelocationSourceType.Offset16 => true,
			NeParser.NeRelocationSourceType.Offset32 => true,
			// Pointer48 (48-bit far pointer) is valid but not yet implemented
			NeParser.NeRelocationSourceType.Pointer48 => true,
			_ => false
		};
		
		if (!isValidSourceType)
		{
			logger?.LogWarning("[NE Loader] Unsupported relocation source type: {Type}", reloc.SourceType);
			return;
		}
		
		uint fixupAddress = segmentAddress + reloc.SourceOffset;
		uint targetValue = 0;
		
		switch (targetType)
		{
			case NeParser.NeRelocationTargetType.InternalRef:
				// Internal reference - target is segment:offset within this module
				if (segmentMap.TryGetValue(reloc.TargetSegment, out var targetSeg))
				{
					targetValue = targetSeg.address + reloc.TargetOffset;
				}
				else
				{
					logger?.LogWarning("[NE Loader] Internal relocation references invalid segment {Seg}", reloc.TargetSegment);
					return;
				}
				break;
				
			case NeParser.NeRelocationTargetType.ImportOrdinal:
				// Import by ordinal - TargetSegment is module index (1-based), TargetOffset is ordinal
				if (reloc.TargetSegment > 0 && reloc.TargetSegment <= importModules.Count)
				{
					var moduleName = importModules[reloc.TargetSegment - 1];
					var ordinal = reloc.TargetOffset;
					
					// Create a synthetic import address for this function
					// Using a range in the import area (0x0F000000+)
					targetValue = MemoryRegions.ImportHookBase + 
						(uint)((reloc.TargetSegment - 1) * 0x10000 + ordinal * 4);
					
					logger?.LogDebug("[NE Loader] Import by ordinal: {Module}!{Ordinal} -> 0x{Addr:X8}",
						moduleName, ordinal, targetValue);
				}
				break;
				
			case NeParser.NeRelocationTargetType.ImportName:
				// Import by name - TargetSegment is module index, TargetOffset is name offset
				if (reloc.TargetSegment > 0 && reloc.TargetSegment <= importModules.Count)
				{
					var moduleName = importModules[reloc.TargetSegment - 1];
					var nameOffset = header.BaseOffset + header.ImportedNamesTableOffset + reloc.TargetOffset;
					var funcName = ReadPascalString(bytes, nameOffset);
					
					// Create a synthetic import address
					targetValue = MemoryRegions.ImportHookBase + 
						(uint)((reloc.TargetSegment - 1) * 0x10000 + reloc.TargetOffset);
					
					logger?.LogDebug("[NE Loader] Import by name: {Module}!{Name} -> 0x{Addr:X8}",
						moduleName, funcName ?? "??", targetValue);
				}
				break;
				
			case NeParser.NeRelocationTargetType.OsFixup:
				// Operating system fixup - various OS-specific addresses
				switch (reloc.TargetOffset)
				{
					case 1: // Floating point fixup
						logger?.LogDebug("[NE Loader] OS fixup: floating point");
						return;
					default:
						logger?.LogDebug("[NE Loader] OS fixup type {Type}", reloc.TargetOffset);
						return;
				}
		}
		
		// Apply fixup based on source type
		switch (sourceType)
		{
			case NeParser.NeRelocationSourceType.LoByte:
				// Low byte fixup
				var lobyte = (byte)(targetValue & 0xFF);
				if (isAdditive)
				{
					lobyte = (byte)(lobyte + vm.Read8(fixupAddress));
				}
				vm.Write8(fixupAddress, lobyte);
				break;
				
			case NeParser.NeRelocationSourceType.Selector:
				// 16-bit segment selector - in flat memory model, use segment base >> 4
				var selector = (ushort)(targetValue >> 4);
				if (isAdditive)
				{
					selector = (ushort)(selector + vm.Read16(fixupAddress));
				}
				vm.Write16(fixupAddress, selector);
				break;
				
			case NeParser.NeRelocationSourceType.Pointer32:
				// 32-bit far pointer (selector:offset)
				var offset16 = (ushort)(targetValue & 0xFFFF);
				var seg16 = (ushort)(targetValue >> 4);
				if (isAdditive)
				{
					offset16 = (ushort)(offset16 + vm.Read16(fixupAddress));
					seg16 = (ushort)(seg16 + vm.Read16(fixupAddress + 2));
				}
				vm.Write16(fixupAddress, offset16);
				vm.Write16(fixupAddress + 2, seg16);
				break;
				
			case NeParser.NeRelocationSourceType.Offset16:
				// 16-bit offset fixup
				var offset = (ushort)(targetValue & 0xFFFF);
				if (isAdditive)
				{
					offset = (ushort)(offset + vm.Read16(fixupAddress));
				}
				vm.Write16(fixupAddress, offset);
				break;
				
			case NeParser.NeRelocationSourceType.Pointer48:
				// 48-bit far pointer (seg:off32) - 16-bit selector + 32-bit offset
				// This is rare in NE files and not fully implemented yet.
				// Pointer48 relocations are intentionally not applied until full implementation is added.
				logger?.LogWarning("[NE Loader] Pointer48 relocation not yet implemented, skipping");
				break;
				
			case NeParser.NeRelocationSourceType.Offset32:
				// 32-bit offset fixup
				var offset32 = targetValue;
				if (isAdditive)
				{
					offset32 += vm.Read32(fixupAddress);
				}
				vm.Write32(fixupAddress, offset32);
				break;
				
			default:
				logger?.LogWarning("[NE Loader] Unsupported relocation source type: {Type}", sourceType);
				break;
		}
	}
	
	/// <summary>
	/// Read a Pascal string (length-prefixed) from the file.
	/// </summary>
	private static string? ReadPascalString(byte[] bytes, int offset)
	{
		if (offset < 0 || offset >= bytes.Length)
		{
			return null;
		}
		
		var length = bytes[offset];
		if (length == 0 || offset + 1 + length > bytes.Length)
		{
			return null;
		}
		
		return Encoding.ASCII.GetString(bytes, offset + 1, length);
	}
	
	private Dictionary<uint, (string dll, string name)> BuildImportMap(
		List<string> importModules,
		Dictionary<string, ushort> residentNames,
		Dictionary<string, ushort> nonResidentNames,
		Dictionary<ushort, NeParser.NeEntryPoint> entryPoints)
	{
		// For NE executables, imports are handled differently than PE
		// We'll create synthetic addresses for imported functions similar to PE loader
		var map = new Dictionary<uint, (string dll, string name)>();
		
		// Create syscall dispatcher stub at fixed address
		var syscallStub = new byte[]
		{
			0xCD, 0x80, // INT 0x80 - triggers syscall
			0xC3        // RET
		};
		vm.WriteBytes(MemoryRegions.SyscallDispatcherAddress, syscallStub);
		logger?.LogInformation("[NE Loader] Created syscall dispatcher at 0x{Address:X8}", MemoryRegions.SyscallDispatcherAddress);
		
		// Map Win16 imports to Win32 equivalents
		// For now, we'll create entries for common Win16 DLLs
		foreach (var normalizedModule in importModules.Select(m => m.ToUpperInvariant()))
		{
			// Map Win16 module names to Win32 equivalents
			var win32Module = normalizedModule switch
			{
				"KERNEL" => "KERNEL32.DLL",
				"USER" => "USER32.DLL",
				"GDI" => "GDI32.DLL",
				"KEYBOARD" => "USER32.DLL",
				"SOUND" => "WINMM.DLL",
				"SYSTEM" => "KERNEL32.DLL",
				_ => normalizedModule + ".DLL"
			};
			
			// For each import module, we would need to resolve specific function imports
			// This is a simplified approach - full implementation would parse import tables
			logger?.LogDebug("[NE Loader] Mapping Win16 module {Win16} to {Win32}", normalizedModule, win32Module);
		}
		
		return map;
	}
	
	private (Dictionary<string, uint> byName, Dictionary<uint, uint> byOrdinal) BuildExportMaps(
		Dictionary<string, ushort> residentNames,
		Dictionary<string, ushort> nonResidentNames,
		Dictionary<int, (uint address, uint size)> segmentMap,
		Dictionary<ushort, NeParser.NeEntryPoint> entryPoints)
	{
		var byName = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		var byOrdinal = new Dictionary<uint, uint>();
		
		// Process resident names
		foreach (var (name, ordinal) in residentNames)
		{
			if (entryPoints.TryGetValue(ordinal, out var entry) &&
			    segmentMap.TryGetValue(entry.Segment, out var segment))
			{
				var address = segment.address + entry.Offset;
				byName[name] = address;
				byOrdinal[ordinal] = address;
			}
		}
		
		// Process non-resident names
		foreach (var (name, ordinal) in nonResidentNames)
		{
			if (entryPoints.TryGetValue(ordinal, out var entry) &&
			    segmentMap.TryGetValue(entry.Segment, out var segment))
			{
				var address = segment.address + entry.Offset;
				byName[name] = address;
				byOrdinal[ordinal] = address;
			}
		}
		
		return (byName, byOrdinal);
	}
	
	private PeSection[] CreateSectionsFromSegments(
		NeParser.NeSegment[] segments,
		Dictionary<int, (uint address, uint size)> segmentMap,
		uint baseAddress)
	{
		return segments
			.Where(segment => segmentMap.ContainsKey(segment.SegmentNumber))
			.Select(segment =>
			{
				var mappedSegment = segmentMap[segment.SegmentNumber];
				var rva = mappedSegment.address - baseAddress;
				var name = $"SEG{segment.SegmentNumber}";
				
				// Convert NE segment flags to PE section characteristics
				var characteristics = PeSectionCharacteristics.MemRead;
				
				// Bit 0: Data segment (vs code segment)
				if ((segment.Flags & NE_SEGMENT_DATA) != 0)
				{
					characteristics |= PeSectionCharacteristics.ContainsInitializedData;
				}
				else
				{
					characteristics |= PeSectionCharacteristics.ContainsCode | PeSectionCharacteristics.MemExecute;
				}
				
				// Bit 1: Allocated (not used for characteristics)
				// Bit 2: Loaded (not used for characteristics)
				
				// Writable check - most NE segments are writable unless marked as read-only
				if ((segment.Flags & NE_SEGMENT_READONLY) == 0) // If not read-only
				{
					characteristics |= PeSectionCharacteristics.MemWrite;
				}
				
				return new PeSection(name, rva, mappedSegment.size, mappedSegment.size, characteristics);
			})
			.ToArray();
	}
}
