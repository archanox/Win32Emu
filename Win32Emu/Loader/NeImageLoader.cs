using Microsoft.Extensions.Logging;
using Win32Emu.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Win32Emu.Loader;

/// <summary>
/// NE (New Executable) loader for Win16 applications.
/// Supports loading 16-bit Windows executables in the NE format.
/// </summary>
public class NeImageLoader(VirtualMemory vm, ILogger? logger = null)
{
	// NE header signature "NE" (0x454E)
	private const ushort NE_SIGNATURE = 0x454E;
	
	// MZ DOS header signature "MZ" (0x5A4D in little-endian)
	private const ushort MZ_SIGNATURE = 0x5A4D;
	
	// DOS header constants
	private const int DOS_HEADER_MIN_SIZE = 0x40;
	private const int DOS_HEADER_NE_PE_OFFSET = 0x3C;
	
	// NE header size (minimum required to read all header fields)
	private const int NE_HEADER_MIN_SIZE = 64;
	
	// Base address for NE executables (64KB to avoid NULL pointer conflicts)
	private const uint NE_BASE_ADDRESS = 0x00010000;
	
	// Full segment size for NE executables (64KB)
	private const uint FULL_SEGMENT_SIZE = 0x10000;
	
	// Paragraph alignment (16 bytes)
	private const uint PARAGRAPH_MASK = 0xF;
	private const uint PARAGRAPH_ALIGN = 0xFFFFFFF0;
	
	// NE segment flags
	private const ushort NE_SEGMENT_DATA = 0x0001;
	private const ushort NE_SEGMENT_READONLY = 0x0008;
	
	// NE entry table constants
	private const byte NE_ENTRY_UNUSED = 0x00;
	private const byte NE_ENTRY_MOVABLE = 0xFF;
	private const int NE_ENTRY_MOVABLE_SIZE = 6;
	private const int NE_ENTRY_FIXED_SIZE = 3;
	
	// NE segment table entry size
	private const int NE_SEGMENT_ENTRY_SIZE = 8;
	
	// NE relocation entry size
	private const int NE_RELOCATION_ENTRY_SIZE = 8;
	
	// Minimum bytes per relocation fixup (used to calculate maximum reasonable relocations)
	private const int MIN_BYTES_PER_RELOCATION = 2;
	
	// Maximum reasonable relocations fallback (used when segment length is 0)
	private const int MAX_REASONABLE_RELOCATIONS_FALLBACK = 1000;
	
	// NE name table entry suffix size (name length byte + 2-byte ordinal)
	private const int NE_NAME_ENTRY_SUFFIX_SIZE = 3;
	
	// NE module reference entry size
	private const int NE_MODULE_REF_ENTRY_SIZE = 2;
	
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
		try
		{
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);
			
			// Check DOS MZ signature
			var mzSignature = reader.ReadUInt16();
			if (mzSignature != MZ_SIGNATURE)
			{
				return false;
			}
			
			// Seek to offset which contains the offset to the NE/PE header
			stream.Seek(DOS_HEADER_NE_PE_OFFSET, SeekOrigin.Begin);
			var neOffset = reader.ReadUInt32();
			
			// Seek to NE header
			stream.Seek(neOffset, SeekOrigin.Begin);
			var neSignature = reader.ReadUInt16();
			
			return neSignature == NE_SIGNATURE;
		}
		catch
		{
			return false;
		}
	}
	
	/// <summary>
	/// Validates if a byte array contains a valid NE executable.
	/// </summary>
	/// <param name="bytes">The byte array to validate</param>
	/// <returns>True if the byte array contains a valid NE executable, false otherwise</returns>
	public static bool IsNE(byte[] bytes)
	{
		try
		{
			if (bytes.Length < DOS_HEADER_MIN_SIZE)
			{
				return false;
			}
			
			// Check DOS MZ signature
			var mzSignature = BitConverter.ToUInt16(bytes, 0);
			if (mzSignature != MZ_SIGNATURE)
			{
				return false;
			}
			
			// Get offset to NE header
			var neOffset = BitConverter.ToUInt32(bytes, DOS_HEADER_NE_PE_OFFSET);
			if (neOffset + 2 > bytes.Length)
			{
				return false;
			}
			
			// Check NE signature
			var neSignature = BitConverter.ToUInt16(bytes, (int)neOffset);
			return neSignature == NE_SIGNATURE;
		}
		catch
		{
			// Return false on any parsing errors (out of bounds, etc.)
			return false;
		}
	}
	
	public LoadedImage Load(string path)
	{
		var bytes = File.ReadAllBytes(path);
		return LoadFromBytes(bytes, path);
	}
	
	public LoadedImage LoadFromBytes(byte[] bytes, string sourcePath = "<memory>")
	{
		logger?.LogInformation("[NE Loader] Loading NE executable from {Path}", sourcePath);
		
		// Parse NE header
		var neHeader = ParseNeHeader(bytes);
		
		logger?.LogInformation("[NE Loader] NE version: {Major}.{Minor}", neHeader.MajorLinkerVersion, neHeader.MinorLinkerVersion);
		logger?.LogInformation("[NE Loader] Target OS: {TargetOS} (2=Windows)", neHeader.TargetOS);
		logger?.LogInformation("[NE Loader] Program flags: 0x{Flags:X4}", neHeader.ProgramFlags);
		logger?.LogInformation("[NE Loader] Application type: 0x{Type:X4}", neHeader.ApplicationType);
		logger?.LogInformation("[NE Loader] Auto data segment: {AutoData}, Init heap: {Heap}, Init stack: {Stack}",
			neHeader.AutoDataSegment, neHeader.InitHeapSize, neHeader.InitStackSize);
		logger?.LogInformation("[NE Loader] Sector alignment shift: {Shift} (sector size: {Size})",
			neHeader.SectorAlignmentShift, 1 << neHeader.SectorAlignmentShift);
		
		// Parse segment table
		var segments = ParseSegmentTable(bytes, neHeader);
		logger?.LogInformation("[NE Loader] Loaded {Count} segments", segments.Length);
		
		// Parse entry table
		var entryPoints = ParseEntryTable(bytes, neHeader);
		logger?.LogInformation("[NE Loader] Found {Count} entry points", entryPoints.Count);
		
		// Parse resident and non-resident name tables
		var residentNames = ParseResidentNameTable(bytes, neHeader);
		var nonResidentNames = ParseNonResidentNameTable(bytes, neHeader);
		logger?.LogInformation("[NE Loader] Resident names: {Resident}, Non-resident names: {NonResident}", 
			residentNames.Count, nonResidentNames.Count);
		
		// Parse import module name table
		var importModules = ParseImportModuleTable(bytes, neHeader);
		logger?.LogInformation("[NE Loader] Import modules: {Count}", importModules.Count);
		
		// Calculate base address for 16-bit segments
		// NE executables typically use a separate 16-bit address space
		// We'll map them starting at 64KB to avoid conflicts with NULL pointers
		uint baseAddress = NE_BASE_ADDRESS;
		
		// Load segments into memory
		uint currentAddress = baseAddress;
		var segmentMap = new Dictionary<int, (uint address, uint size)>();
		
		foreach (var segment in segments)
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
				
				var segFlags = (NeSegmentFlags)segment.Flags;
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
		ProcessRelocations(bytes, neHeader, segments, segmentMap, importModules);
		
		// Calculate entry point address
		// Entry point is specified as segment:offset in NE format
		uint entryPointAddress = 0;
		if (neHeader.EntryPointSegment > 0 && neHeader.EntryPointSegment <= segments.Length &&
			segmentMap.TryGetValue(neHeader.EntryPointSegment, out var entrySegment))
		{
			entryPointAddress = entrySegment.address + neHeader.EntryPointOffset;
			logger?.LogInformation("[NE Loader] Entry point: Segment {Seg}, Offset 0x{Off:X4} -> VA 0x{VA:X8}",
				neHeader.EntryPointSegment, neHeader.EntryPointOffset, entryPointAddress);
		}
		
		// Create import map for Win16 API calls
		var importMap = BuildImportMap(importModules, residentNames, nonResidentNames, entryPoints);
		
		// Build export maps from resident and non-resident name tables
		var (exportsByName, exportsByOrdinal) = BuildExportMaps(residentNames, nonResidentNames, segmentMap, entryPoints);
		
		// Create PE sections from NE segments for compatibility
		var sections = CreateSectionsFromSegments(segments, segmentMap, baseAddress);
		
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
	
	private NeHeader ParseNeHeader(byte[] bytes)
	{
		// Get offset to NE header from DOS stub
		var neOffset = (int)BitConverter.ToUInt32(bytes, DOS_HEADER_NE_PE_OFFSET);
		
		// Validate NE header can be fully read (NE header is 64 bytes minimum)
		if (neOffset < 0 || neOffset + NE_HEADER_MIN_SIZE > bytes.Length)
		{
			throw new InvalidDataException($"Invalid NE header offset: {neOffset}");
		}
		
		return new NeHeader
		{
			// 0x00-0x01: Signature
			Signature = BitConverter.ToUInt16(bytes, neOffset + 0x00),
			// 0x02: Major linker version
			MajorLinkerVersion = bytes[neOffset + 0x02],
			// 0x03: Minor linker version
			MinorLinkerVersion = bytes[neOffset + 0x03],
			// 0x04-0x05: Entry table offset
			EntryTableOffset = BitConverter.ToUInt16(bytes, neOffset + 0x04),
			// 0x06-0x07: Entry table length
			EntryTableLength = BitConverter.ToUInt16(bytes, neOffset + 0x06),
			// 0x08-0x0B: CRC checksum
			CrcChecksum = BitConverter.ToUInt32(bytes, neOffset + 0x08),
			// 0x0C-0x0D: Program flags
			ProgramFlags = BitConverter.ToUInt16(bytes, neOffset + 0x0C),
			// 0x0E-0x0F: Application type flags
			ApplicationType = BitConverter.ToUInt16(bytes, neOffset + 0x0E),
			// 0x10-0x11: Auto data segment (DGROUP)
			AutoDataSegment = BitConverter.ToUInt16(bytes, neOffset + 0x10),
			// 0x12-0x13: Initial heap size
			InitHeapSize = BitConverter.ToUInt16(bytes, neOffset + 0x12),
			// 0x14-0x15: Initial stack size
			InitStackSize = BitConverter.ToUInt16(bytes, neOffset + 0x14),
			// 0x16-0x17: Entry point segment (CS)
			EntryPointSegment = BitConverter.ToUInt16(bytes, neOffset + 0x16),
			// 0x18-0x19: Entry point offset (IP)
			EntryPointOffset = BitConverter.ToUInt16(bytes, neOffset + 0x18),
			// 0x1A-0x1B: Initial stack segment (SS)
			InitStackSegment = BitConverter.ToUInt16(bytes, neOffset + 0x1A),
			// 0x1C-0x1D: Initial stack pointer (SP)
			InitStackPointer = BitConverter.ToUInt16(bytes, neOffset + 0x1C),
			// 0x1E-0x1F: Segment count
			SegmentCount = BitConverter.ToUInt16(bytes, neOffset + 0x1E),
			// 0x20-0x21: Module reference count
			ModuleReferenceCount = BitConverter.ToUInt16(bytes, neOffset + 0x20),
			// 0x22-0x23: Non-resident name table size
			NonResidentNameTableSize = BitConverter.ToUInt16(bytes, neOffset + 0x22),
			// 0x24-0x25: Segment table offset
			SegmentTableOffset = BitConverter.ToUInt16(bytes, neOffset + 0x24),
			// 0x26-0x27: Resource table offset
			ResourceTableOffset = BitConverter.ToUInt16(bytes, neOffset + 0x26),
			// 0x28-0x29: Resident name table offset
			ResidentNameTableOffset = BitConverter.ToUInt16(bytes, neOffset + 0x28),
			// 0x2A-0x2B: Module reference table offset
			ModuleReferenceTableOffset = BitConverter.ToUInt16(bytes, neOffset + 0x2A),
			// 0x2C-0x2D: Imported names table offset
			ImportedNamesTableOffset = BitConverter.ToUInt16(bytes, neOffset + 0x2C),
			// 0x2E-0x31: Non-resident name table offset (absolute file offset)
			NonResidentNameTableOffset = BitConverter.ToUInt32(bytes, neOffset + 0x2E),
			// 0x32-0x33: Movable entry point count
			MovableEntryCount = BitConverter.ToUInt16(bytes, neOffset + 0x32),
			// 0x34-0x35: Sector alignment shift
			SectorAlignmentShift = BitConverter.ToUInt16(bytes, neOffset + 0x34),
			// 0x36-0x37: Resource segment count
			ResourceSegmentCount = BitConverter.ToUInt16(bytes, neOffset + 0x36),
			// 0x38: Target OS
			TargetOS = bytes[neOffset + 0x38],
			// 0x39: Other flags (OS/2)
			OtherFlags = bytes[neOffset + 0x39],
			// 0x3A-0x3B: Return thunks offset (gang load)
			ReturnThunksOffset = BitConverter.ToUInt16(bytes, neOffset + 0x3A),
			// 0x3C-0x3D: Segment reference thunks offset
			SegmentReferenceThunksOffset = BitConverter.ToUInt16(bytes, neOffset + 0x3C),
			// 0x3E-0x3F: Minimum code swap size
			SwapCodeSize = BitConverter.ToUInt16(bytes, neOffset + 0x3E),
			// 0x40-0x41: Expected Windows version
			ExpectedWindowsVersion = BitConverter.ToUInt16(bytes, neOffset + 0x40),
			BaseOffset = neOffset
		};
	}
	
	private NeSegment[] ParseSegmentTable(byte[] bytes, NeHeader header)
	{
		var segments = new List<NeSegment>();
		var offset = header.BaseOffset + header.SegmentTableOffset;
		
		// Validate segment table bounds
		var requiredSize = offset + (header.SegmentCount * NE_SEGMENT_ENTRY_SIZE);
		if (requiredSize > bytes.Length)
		{
			throw new InvalidDataException($"Segment table extends beyond file bounds");
		}
		
		// Use the sector alignment shift from the header, not a hardcoded value
		// This varies between files (commonly 4, 8, or 9)
		var sectorShift = header.SectorAlignmentShift;
		
		for (var i = 0; i < header.SegmentCount; i++)
		{
			// The file offset is stored as a shifted value (divided by sector size)
			// Shift it back by the alignment shift to get the actual file offset
			var fileOffset = (uint)(BitConverter.ToUInt16(bytes, offset) << sectorShift);
			var lengthRaw = BitConverter.ToUInt16(bytes, offset + 2);
			var flags = BitConverter.ToUInt16(bytes, offset + 4);
			var minAllocation = BitConverter.ToUInt16(bytes, offset + 6);
			
			// If length is 0, use full 64KB segment
			uint length = lengthRaw;
			if (length == 0 && minAllocation > 0)
			{
				length = FULL_SEGMENT_SIZE; // 64KB full segment
			}
			
			var segment = new NeSegment
			{
				SegmentNumber = i + 1,
				FileOffset = fileOffset,
				Length = length,
				Flags = flags,
				MinAllocation = minAllocation
			};
			
			segments.Add(segment);
			offset += NE_SEGMENT_ENTRY_SIZE;
		}
		
		return segments.ToArray();
	}
	
	private Dictionary<ushort, NeEntryPoint> ParseEntryTable(byte[] bytes, NeHeader header)
	{
		var entryPoints = new Dictionary<ushort, NeEntryPoint>();
		var offset = header.BaseOffset + header.EntryTableOffset;
		var endOffset = offset + header.EntryTableLength;
		
		ushort ordinal = 1;
		
		while (offset < endOffset)
		{
			// Validate bounds before reading
			if (offset + 2 > endOffset || offset + 2 > bytes.Length)
			{
				break;
			}
			
			var bundleCount = bytes[offset];
			if (bundleCount == 0)
			{
				break; // End of entry table
			}
			
			var segmentIndicator = bytes[offset + 1];
			offset += 2;
			
			for (var i = 0; i < bundleCount; i++)
			{
				if (segmentIndicator == NE_ENTRY_UNUSED)
				{
					// Unused entry
					ordinal++;
					continue;
				}
				
				if (segmentIndicator == NE_ENTRY_MOVABLE)
				{
					// Validate bounds for movable entry
					if (offset + NE_ENTRY_MOVABLE_SIZE > bytes.Length)
					{
						break;
					}
					
					// Movable segment
					var flags = bytes[offset];
					// Skip int3F field (bytes[offset + 1..2]) - not used
					var segment = bytes[offset + 3];
					var segmentOffset = BitConverter.ToUInt16(bytes, offset + 4);
					
					entryPoints[ordinal] = new NeEntryPoint
					{
						Ordinal = ordinal,
						Segment = segment,
						Offset = segmentOffset,
						Flags = flags
					};
					
					offset += NE_ENTRY_MOVABLE_SIZE;
				}
				else
				{
					// Validate bounds for fixed entry
					if (offset + NE_ENTRY_FIXED_SIZE > bytes.Length)
					{
						break;
					}
					
					// Fixed segment
					var flags = bytes[offset];
					var segmentOffset = BitConverter.ToUInt16(bytes, offset + 1);
					
					entryPoints[ordinal] = new NeEntryPoint
					{
						Ordinal = ordinal,
						Segment = segmentIndicator,
						Offset = segmentOffset,
						Flags = flags
					};
					
					offset += NE_ENTRY_FIXED_SIZE;
				}
				
				ordinal++;
			}
		}
		
		return entryPoints;
	}
	
	private Dictionary<string, ushort> ParseResidentNameTable(byte[] bytes, NeHeader header)
	{
		var names = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);
		var offset = header.BaseOffset + header.ResidentNameTableOffset;
		
		// Validate initial bounds
		if (offset + 1 > bytes.Length)
		{
			return names;
		}
		
		// First entry is module name, skip it
		var nameLength = bytes[offset];
		if (offset + nameLength + NE_NAME_ENTRY_SUFFIX_SIZE > bytes.Length)
		{
			return names;
		}
		offset += nameLength + NE_NAME_ENTRY_SUFFIX_SIZE;
		
		while (offset < bytes.Length)
		{
			// Validate bounds before reading name length
			if (offset + 1 > bytes.Length)
			{
				break;
			}
			
			nameLength = bytes[offset];
			if (nameLength == 0)
			{
				break;
			}
			
			// Validate bounds for complete entry
			if (offset + nameLength + NE_NAME_ENTRY_SUFFIX_SIZE > bytes.Length)
			{
				break;
			}
			
			var name = Encoding.ASCII.GetString(bytes, offset + 1, nameLength);
			var ordinal = BitConverter.ToUInt16(bytes, offset + nameLength + 1);
			
			names[name] = ordinal;
			offset += nameLength + NE_NAME_ENTRY_SUFFIX_SIZE;
		}
		
		return names;
	}
	
	private Dictionary<string, ushort> ParseNonResidentNameTable(byte[] bytes, NeHeader header)
	{
		var names = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);
		var offset = (int)header.NonResidentNameTableOffset;
		
		if (offset == 0 || offset >= bytes.Length)
		{
			return names;
		}
		
		// Validate initial bounds
		if (offset + 1 > bytes.Length)
		{
			return names;
		}
		
		// First entry is module description, skip it
		var nameLength = bytes[offset];
		if (offset + nameLength + NE_NAME_ENTRY_SUFFIX_SIZE > bytes.Length)
		{
			return names;
		}
		offset += nameLength + NE_NAME_ENTRY_SUFFIX_SIZE;
		
		while (offset < bytes.Length)
		{
			// Validate bounds before reading name length
			if (offset + 1 > bytes.Length)
			{
				break;
			}
			
			nameLength = bytes[offset];
			if (nameLength == 0)
			{
				break;
			}
			
			// Validate bounds for complete entry
			if (offset + nameLength + NE_NAME_ENTRY_SUFFIX_SIZE > bytes.Length)
			{
				break;
			}
			
			var name = Encoding.ASCII.GetString(bytes, offset + 1, nameLength);
			var ordinal = BitConverter.ToUInt16(bytes, offset + nameLength + 1);
			
			names[name] = ordinal;
			offset += nameLength + NE_NAME_ENTRY_SUFFIX_SIZE;
		}
		
		return names;
	}
	
	private List<string> ParseImportModuleTable(byte[] bytes, NeHeader header)
	{
		var modules = new List<string>();
		var moduleTableOffset = header.BaseOffset + header.ModuleReferenceTableOffset;
		var importNamesOffset = header.BaseOffset + header.ImportedNamesTableOffset;
		
		// Use module reference count from header instead of iterating until importNamesOffset
		// Each entry is 2 bytes (offset into imported names table)
		// Note: We use 'continue' instead of 'break' for invalid entries to be resilient
		// to partially corrupted files - this allows loading valid modules even if some
		// entries are corrupted. Real-world NE files may have corruption or padding issues.
		var moduleCount = header.ModuleReferenceCount;
		
		for (var i = 0; i < moduleCount; i++)
		{
			var offset = moduleTableOffset + (i * NE_MODULE_REF_ENTRY_SIZE);
			
			// Validate bounds for reading module reference entry
			if (offset + NE_MODULE_REF_ENTRY_SIZE > bytes.Length)
			{
				logger?.LogWarning("[NE Loader] Module reference entry {Index} is out of bounds", i);
				break;
			}
			
			var nameOffset = BitConverter.ToUInt16(bytes, offset);
			if (nameOffset == 0)
			{
				logger?.LogWarning("[NE Loader] Module reference entry {Index} has null offset", i);
				continue; // Skip null entries but continue with remaining modules
			}
			
			// NE format specification: Module reference table entries contain offsets into the imported names table.
			// According to Microsoft documentation, these offsets are relative to the start of the imported names table.
			// Try to read the module name using this standard interpretation.
			var actualOffset = importNamesOffset + nameOffset;
			var moduleName = TryReadModuleName(bytes, actualOffset, i);
			
			// If standard interpretation failed, try alternative: offset might be relative to NE header base
			// This handles some non-standard or older NE files
			if (moduleName == null)
			{
				actualOffset = header.BaseOffset + nameOffset;
				moduleName = TryReadModuleName(bytes, actualOffset, i);
			}
			
			// If both interpretations failed, skip this entry
			if (moduleName == null)
			{
				continue;
			}
			
			modules.Add(moduleName);
			logger?.LogDebug("[NE Loader] Parsed import module {Index}: {ModuleName}", i + 1, moduleName);
		}
		
		return modules;
	}
	
	/// <summary>
	/// Attempts to read a Pascal-style module name (length-prefixed string) from the specified offset.
	/// Returns null if the offset is invalid, the name is empty, or the name extends beyond file bounds.
	/// </summary>
	private string? TryReadModuleName(byte[] bytes, int actualOffset, int entryIndex)
	{
		// Validate offset is within bounds
		if (actualOffset < 0 || actualOffset >= bytes.Length || actualOffset + 1 > bytes.Length)
		{
			return null;
		}
		
		// Read length byte
		var nameLength = bytes[actualOffset];
		if (nameLength == 0)
		{
			return null;
		}
		
		// Validate name doesn't extend beyond file
		if (actualOffset + nameLength + 1 > bytes.Length)
		{
			return null;
		}
		
		// Validate that the name contains printable ASCII characters (basic sanity check)
		// Module names should be alphanumeric with possible underscores, hyphens, or periods
		for (var j = 1; j <= nameLength; j++)
		{
			var ch = bytes[actualOffset + j];
			// Allow A-Z, a-z, 0-9, underscore, hyphen, period, and basic punctuation
			if (!((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || 
			      (ch >= '0' && ch <= '9') || ch == '_' || ch == '-' || ch == '.' || ch == ' '))
			{
				// Invalid character found - this is likely not a valid module name
				return null;
			}
		}
		
		// Read and return the module name
		return Encoding.ASCII.GetString(bytes, actualOffset + 1, nameLength);
	}
	
	/// <summary>
	/// Process segment relocations for all segments that have relocation data.
	/// </summary>
	private void ProcessRelocations(byte[] bytes, NeHeader header, NeSegment[] segments, 
		Dictionary<int, (uint address, uint size)> segmentMap, List<string> importModules)
	{
		foreach (var segment in segments)
		{
			// Check if segment has relocations
			if ((segment.Flags & (ushort)NeSegmentFlags.HasRelocations) == 0)
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
		byte[] bytes, NeHeader header)
	{
		var targetType = (NeRelocationTargetType)(reloc.TargetFlags & 0x03);
		var isAdditive = (reloc.TargetFlags & 0x04) != 0;
		var sourceType = (NeRelocationSourceType)(reloc.SourceType & 0x0F);
		
		// Validate source type - if it's not a known type, skip this relocation
		// Using switch expression for better performance than Enum.IsDefined
		var isValidSourceType = sourceType switch
		{
			NeRelocationSourceType.LoByte => true,
			NeRelocationSourceType.Selector => true,
			NeRelocationSourceType.Pointer32 => true,
			NeRelocationSourceType.Offset16 => true,
			NeRelocationSourceType.Offset32 => true,
			// Pointer48 (48-bit far pointer) is valid but not yet implemented
			NeRelocationSourceType.Pointer48 => true,
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
			case NeRelocationTargetType.InternalRef:
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
				
			case NeRelocationTargetType.ImportOrdinal:
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
				
			case NeRelocationTargetType.ImportName:
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
				
			case NeRelocationTargetType.OsFixup:
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
			case NeRelocationSourceType.LoByte:
				// Low byte fixup
				var lobyte = (byte)(targetValue & 0xFF);
				if (isAdditive)
				{
					lobyte = (byte)(lobyte + vm.Read8(fixupAddress));
				}
				vm.Write8(fixupAddress, lobyte);
				break;
				
			case NeRelocationSourceType.Selector:
				// 16-bit segment selector - in flat memory model, use segment base >> 4
				var selector = (ushort)(targetValue >> 4);
				if (isAdditive)
				{
					selector = (ushort)(selector + vm.Read16(fixupAddress));
				}
				vm.Write16(fixupAddress, selector);
				break;
				
			case NeRelocationSourceType.Pointer32:
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
				
			case NeRelocationSourceType.Offset16:
				// 16-bit offset fixup
				var offset = (ushort)(targetValue & 0xFFFF);
				if (isAdditive)
				{
					offset = (ushort)(offset + vm.Read16(fixupAddress));
				}
				vm.Write16(fixupAddress, offset);
				break;
				
			case NeRelocationSourceType.Pointer48:
				// 48-bit far pointer (seg:off32) - 16-bit selector + 32-bit offset
				// This is rare in NE files and not fully implemented yet.
				// Pointer48 relocations are intentionally not applied until full implementation is added.
				logger?.LogWarning("[NE Loader] Pointer48 relocation not yet implemented, skipping");
				break;
				
			case NeRelocationSourceType.Offset32:
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
		Dictionary<ushort, NeEntryPoint> entryPoints)
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
		Dictionary<ushort, NeEntryPoint> entryPoints)
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
		NeSegment[] segments,
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

/// <summary>
/// NE (New Executable) header structure.
/// Based on https://wiki.osdev.org/NE and https://www.fileformat.info/format/exe/corion-ne.htm
/// </summary>
internal class NeHeader
{
	public ushort Signature { get; init; }                    // 0x00: "NE" signature (0x454E)
	public byte MajorLinkerVersion { get; init; }             // 0x02: Linker major version
	public byte MinorLinkerVersion { get; init; }             // 0x03: Linker minor version
	public ushort EntryTableOffset { get; init; }             // 0x04: Offset to entry table
	public ushort EntryTableLength { get; init; }             // 0x06: Length of entry table
	public uint CrcChecksum { get; init; }                    // 0x08: CRC checksum (32-bit)
	public ushort ProgramFlags { get; init; }                 // 0x0C: Program flags (DGROUP type, etc.)
	public ushort ApplicationType { get; init; }              // 0x0E: Application type flags (DLL, GUI, etc.)
	public ushort AutoDataSegment { get; init; }              // 0x10: Auto data segment index (DGROUP)
	public ushort InitHeapSize { get; init; }                 // 0x12: Initial heap size
	public ushort InitStackSize { get; init; }                // 0x14: Initial stack size
	public ushort EntryPointSegment { get; init; }            // 0x16: Entry point segment number (CS:IP)
	public ushort EntryPointOffset { get; init; }             // 0x18: Entry point offset (CS:IP)
	public ushort InitStackSegment { get; init; }             // 0x1A: Initial stack segment number (SS:SP)
	public ushort InitStackPointer { get; init; }             // 0x1C: Initial stack pointer (SS:SP)
	public ushort SegmentCount { get; init; }                 // 0x1E: Number of segments
	public ushort ModuleReferenceCount { get; init; }         // 0x20: Number of module reference entries
	public ushort NonResidentNameTableSize { get; init; }     // 0x22: Size of non-resident name table
	public ushort SegmentTableOffset { get; init; }           // 0x24: Offset to segment table
	public ushort ResourceTableOffset { get; init; }          // 0x26: Offset to resource table
	public ushort ResidentNameTableOffset { get; init; }      // 0x28: Offset to resident name table
	public ushort ModuleReferenceTableOffset { get; init; }   // 0x2A: Offset to module reference table
	public ushort ImportedNamesTableOffset { get; init; }     // 0x2C: Offset to imported names table
	public uint NonResidentNameTableOffset { get; init; }     // 0x2E: File offset to non-resident name table (absolute)
	public ushort MovableEntryCount { get; init; }            // 0x32: Number of movable entry points
	public ushort SectorAlignmentShift { get; init; }         // 0x34: Sector alignment shift (log2 of sector size)
	public ushort ResourceSegmentCount { get; init; }         // 0x36: Number of resource segments
	public byte TargetOS { get; init; }                       // 0x38: Target operating system
	public byte OtherFlags { get; init; }                     // 0x39: Other executable flags (OS/2)
	public ushort ReturnThunksOffset { get; init; }           // 0x3A: Offset to return thunks (gang load area)
	public ushort SegmentReferenceThunksOffset { get; init; } // 0x3C: Offset to segment reference thunks
	public ushort SwapCodeSize { get; init; }                 // 0x3E: Minimum code swap area size
	public ushort ExpectedWindowsVersion { get; init; }       // 0x40: Expected Windows version (minor.major)
	public int BaseOffset { get; init; }                      // Base offset of NE header in file
}

/// <summary>
/// NE segment table entry.
/// </summary>
internal class NeSegment
{
	public int SegmentNumber { get; init; }    // Segment number (1-based)
	public uint FileOffset { get; init; }      // File offset to segment data (shifted)
	public uint Length { get; init; }          // Length of segment in file
	public ushort Flags { get; init; }         // Segment flags
	public ushort MinAllocation { get; init; } // Minimum allocation size in memory
	public uint MemoryAddress { get; set; }    // Loaded memory address (set during loading)
	public uint MemorySize { get; set; }       // Allocated memory size (set during loading)
}

/// <summary>
/// NE segment flags.
/// </summary>
[Flags]
internal enum NeSegmentFlags : ushort
{
	Data = 0x0001,          // Segment contains data (vs code)
	Allocated = 0x0002,     // Memory allocated for segment
	Loaded = 0x0004,        // Segment is loaded
	Iterated = 0x0008,      // Segment data is iterated (compressed)
	Movable = 0x0010,       // Segment is movable
	Shareable = 0x0020,     // Segment is shareable (pure)
	Preload = 0x0040,       // Segment should be preloaded
	ExecuteOnly = 0x0080,   // Code segment is execute-only
	ReadOnly = 0x0080,      // Data segment is read-only
	HasRelocations = 0x0100,// Segment has relocation data
	Conforming = 0x0200,    // Code segment is conforming
	PrivilegeLevel = 0x0C00,// Privilege level (DPL)
	Discardable = 0x1000,   // Segment is discardable
	Is32Bit = 0x2000,       // 32-bit segment
	Huge = 0x4000,          // Huge segment (>64KB)
}

/// <summary>
/// NE entry point.
/// </summary>
internal class NeEntryPoint
{
	public ushort Ordinal { get; init; }  // Ordinal number
	public byte Segment { get; init; }    // Segment number (0 = movable)
	public ushort Offset { get; init; }   // Offset within segment
	public byte Flags { get; init; }      // Entry flags (exported, shared data, etc.)
}

/// <summary>
/// NE relocation record.
/// </summary>
internal class NeRelocation
{
	public byte SourceType { get; init; }      // Source type (fixup type)
	public byte TargetFlags { get; init; }     // Target flags and type
	public ushort SourceOffset { get; init; }  // Offset within segment to fixup
	public ushort TargetSegment { get; init; } // Target segment (or module index)
	public ushort TargetOffset { get; init; }  // Target offset (or ordinal/name offset)
}

/// <summary>
/// NE relocation source types.
/// </summary>
internal enum NeRelocationSourceType : byte
{
	LoByte = 0,         // Low byte fixup
	Selector = 2,       // 16-bit selector fixup
	Pointer32 = 3,      // 32-bit far pointer fixup (seg:off)
	Offset16 = 5,       // 16-bit offset fixup
	Pointer48 = 11,     // 48-bit far pointer fixup (seg:off32)
	Offset32 = 13,      // 32-bit offset fixup
}

/// <summary>
/// NE relocation target types.
/// </summary>
internal enum NeRelocationTargetType : byte
{
	InternalRef = 0,    // Internal reference (within this module)
	ImportOrdinal = 1,  // Import by ordinal
	ImportName = 2,     // Import by name
	OsFixup = 3,        // Operating system fixup
	Additive = 4,       // Additive fixup (add, don't replace)
}

/// <summary>
/// NE program flags (offset 0x0C in NE header).
/// </summary>
[Flags]
internal enum NeProgramFlags : ushort
{
	/// <summary>DGROUP type mask (bits 0-1)</summary>
	DGroupTypeMask = 0x0003,
	/// <summary>No DGROUP (DGROUP = NONE)</summary>
	DGroupNone = 0x0000,
	/// <summary>Single DGROUP (DGROUP = GROUP)</summary>
	DGroupSingle = 0x0001,
	/// <summary>Multiple DGROUP (DGROUP = MULTIPLE)</summary>
	DGroupMultiple = 0x0002,
	/// <summary>DGROUP is null (for library)</summary>
	DGroupNull = 0x0003,
	/// <summary>Global initialization required</summary>
	GlobalInit = 0x0004,
	/// <summary>Protected mode only</summary>
	ProtectedModeOnly = 0x0008,
	/// <summary>8086 instructions used</summary>
	Has8086 = 0x0010,
	/// <summary>80286 instructions used</summary>
	Has80286 = 0x0020,
	/// <summary>80386 instructions used</summary>
	Has80386 = 0x0040,
	/// <summary>80x87 (FPU) instructions used</summary>
	HasFpu = 0x0080,
}

/// <summary>
/// NE application type flags (offset 0x0E in NE header).
/// </summary>
[Flags]
internal enum NeApplicationType : ushort
{
	/// <summary>Full screen application (not aware of Windows)</summary>
	FullScreen = 0x0001,
	/// <summary>Aware of Windows/Presentation Manager</summary>
	WindowsAware = 0x0002,
	/// <summary>Uses Windows/Presentation Manager API (GUI application)</summary>
	WindowsApi = 0x0003,
	/// <summary>Application type mask (bits 0-1)</summary>
	TypeMask = 0x0003,
	/// <summary>OS/2 family application (runs on OS/2 and DOS)</summary>
	FamilyApp = 0x0008,
	/// <summary>Self-loading application (has self-loading prolog)</summary>
	SelfLoading = 0x0800,
	/// <summary>Linker errors occurred (file may be invalid)</summary>
	LinkerErrors = 0x2000,
	/// <summary>Module is a library (DLL)</summary>
	Library = 0x8000,
}

/// <summary>
/// NE target operating system values (offset 0x38 in NE header).
/// </summary>
internal enum NeTargetOS : byte
{
	/// <summary>Unknown operating system</summary>
	Unknown = 0,
	/// <summary>OS/2</summary>
	OS2 = 1,
	/// <summary>Windows</summary>
	Windows = 2,
	/// <summary>DOS 4.x</summary>
	DOS4 = 3,
	/// <summary>Windows 386 (enhanced mode)</summary>
	Windows386 = 4,
	/// <summary>Borland Operating System Services</summary>
	BOSS = 5,
}
