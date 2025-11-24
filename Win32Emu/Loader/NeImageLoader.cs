using Microsoft.Extensions.Logging;
using Win32Emu.Memory;
using System;
using System.Collections.Generic;
using System.IO;
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
	
	// Base address for NE executables (64KB to avoid NULL pointer conflicts)
	private const uint NE_BASE_ADDRESS = 0x00010000;
	
	// Full segment size for NE executables (64KB)
	private const uint FULL_SEGMENT_SIZE = 0x10000;
	
	// Paragraph alignment (16 bytes)
	private const uint PARAGRAPH_MASK = 0xF;
	private const uint PARAGRAPH_ALIGN = 0xFFFFFFF0;
	
	// NE segment flags
	private const ushort NE_SEGMENT_READONLY = 0x0008;
	
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
			
			// Seek to offset 0x3C which contains the offset to the NE/PE header
			stream.Seek(0x3C, SeekOrigin.Begin);
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
			if (bytes.Length < 0x40)
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
			var neOffset = BitConverter.ToUInt32(bytes, 0x3C);
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
			if (segment.Length > 0)
			{
				// Align to paragraph boundary (16 bytes)
				currentAddress = (currentAddress + PARAGRAPH_MASK) & PARAGRAPH_ALIGN;
				
				segmentMap[segment.SegmentNumber] = (currentAddress, segment.Length);
				
				// Load segment data
				if (segment.FileOffset > 0 && segment.FileOffset + segment.Length <= bytes.Length)
				{
					var segmentData = new byte[segment.Length];
					Array.Copy(bytes, segment.FileOffset, segmentData, 0, segment.Length);
					vm.WriteBytes(currentAddress, segmentData);
					
					logger?.LogDebug("[NE Loader] Loaded segment {Num}: Address=0x{Addr:X8}, Size=0x{Size:X4}, Flags=0x{Flags:X4}",
						segment.SegmentNumber, currentAddress, segment.Length, segment.Flags);
				}
				
				currentAddress += segment.Length;
			}
		}
		
		// Calculate entry point address
		// Entry point is specified as segment:offset in NE format
		uint entryPointAddress = 0;
		if (neHeader.EntryPointSegment > 0 && neHeader.EntryPointSegment <= segments.Length)
		{
			if (segmentMap.TryGetValue(neHeader.EntryPointSegment, out var entrySegment))
			{
				entryPointAddress = entrySegment.address + neHeader.EntryPointOffset;
				logger?.LogInformation("[NE Loader] Entry point: Segment {Seg}, Offset 0x{Off:X4} -> VA 0x{VA:X8}",
					neHeader.EntryPointSegment, neHeader.EntryPointOffset, entryPointAddress);
			}
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
			(ushort)(neHeader.ApplicationType == 2 ? 3 : 2), // Map to PE subsystem (GUI vs CUI)
			0x1000,                          // HeaderEndRva - use default
			0x10000,                         // SizeOfStackReserve - 64KB default for 16-bit
			0x1000,                          // SizeOfStackCommit - 4KB default
			0x10000,                         // SizeOfHeapReserve - 64KB default
			0x1000,                          // SizeOfHeapCommit - 4KB default
			Array.Empty<uint>(),             // No TLS callbacks in NE
			sections,
			new Dictionary<uint, uint>(),    // IAT entry map - handled differently for NE
			new Dictionary<string, ExportMetadata>(), // Export metadata - TODO: infer from calling conventions
			// FileHeader fields
			0x014C,                          // Machine - IMAGE_FILE_MACHINE_I386
			0,                               // TimeDateStamp
			0x0002,                          // Characteristics - IMAGE_FILE_EXECUTABLE_IMAGE
			// OptionalHeader additional fields
			neHeader.MajorLinkerVersion,
			neHeader.MinorLinkerVersion,
			(ushort)(neHeader.ExpectedWindowsVersion >> 8),      // Major OS version
			(ushort)(neHeader.ExpectedWindowsVersion & 0xFF),    // Minor OS version
			0,                               // MajorImageVersion
			0,                               // MinorImageVersion
			(ushort)(neHeader.ExpectedWindowsVersion >> 8),      // MajorSubsystemVersion
			(ushort)(neHeader.ExpectedWindowsVersion & 0xFF),    // MinorSubsystemVersion
			0,                               // DllCharacteristics
			0,                               // CheckSum
			0x1000,                          // SectionAlignment - 4KB
			0x200,                           // FileAlignment - 512 bytes
			baseAddress,                     // BaseOfCode
			baseAddress,                     // BaseOfData
			imageSize,                       // SizeOfCode
			0,                               // SizeOfInitializedData
			0                                // SizeOfUninitializedData
		);
	}
	
	private NeHeader ParseNeHeader(byte[] bytes)
	{
		// Get offset to NE header from DOS stub
		var neOffset = (int)BitConverter.ToUInt32(bytes, 0x3C);
		
		return new NeHeader
		{
			Signature = BitConverter.ToUInt16(bytes, neOffset),
			MajorLinkerVersion = bytes[neOffset + 2],
			MinorLinkerVersion = bytes[neOffset + 3],
			EntryTableOffset = BitConverter.ToUInt16(bytes, neOffset + 4),
			EntryTableLength = BitConverter.ToUInt16(bytes, neOffset + 6),
			CrcChecksum = BitConverter.ToUInt32(bytes, neOffset + 8),
			ProgramFlags = BitConverter.ToUInt16(bytes, neOffset + 12),
			ApplicationType = BitConverter.ToUInt16(bytes, neOffset + 14),
			SegmentTableOffset = BitConverter.ToUInt16(bytes, neOffset + 34),
			SegmentCount = BitConverter.ToUInt16(bytes, neOffset + 28),
			ResourceTableOffset = BitConverter.ToUInt16(bytes, neOffset + 36),
			ResidentNameTableOffset = BitConverter.ToUInt16(bytes, neOffset + 38),
			ModuleReferenceTableOffset = BitConverter.ToUInt16(bytes, neOffset + 40),
			ImportedNamesTableOffset = BitConverter.ToUInt16(bytes, neOffset + 42),
			NonResidentNameTableOffset = BitConverter.ToUInt32(bytes, neOffset + 44),
			MovableEntryCount = BitConverter.ToUInt16(bytes, neOffset + 48),
			TargetOS = bytes[neOffset + 54],
			ExpectedWindowsVersion = BitConverter.ToUInt16(bytes, neOffset + 62),
			EntryPointSegment = BitConverter.ToUInt16(bytes, neOffset + 20),
			EntryPointOffset = BitConverter.ToUInt16(bytes, neOffset + 22),
			BaseOffset = neOffset
		};
	}
	
	private NeSegment[] ParseSegmentTable(byte[] bytes, NeHeader header)
	{
		var segments = new List<NeSegment>();
		var offset = header.BaseOffset + header.SegmentTableOffset;
		
		for (var i = 0; i < header.SegmentCount; i++)
		{
			var fileOffset = (uint)(BitConverter.ToUInt16(bytes, offset) << 4); // Convert sectors to bytes
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
			offset += 8; // Each segment table entry is 8 bytes
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
			var bundleCount = bytes[offset];
			if (bundleCount == 0)
			{
				break; // End of entry table
			}
			
			var segmentIndicator = bytes[offset + 1];
			offset += 2;
			
			for (var i = 0; i < bundleCount; i++)
			{
				if (segmentIndicator == 0x00)
				{
					// Unused entry
					ordinal++;
					continue;
				}
				
				if (segmentIndicator == 0xFF)
				{
					// Movable segment
					var flags = bytes[offset];
					var int3F = BitConverter.ToUInt16(bytes, offset + 1);
					var segment = bytes[offset + 3];
					var segmentOffset = BitConverter.ToUInt16(bytes, offset + 4);
					
					entryPoints[ordinal] = new NeEntryPoint
					{
						Ordinal = ordinal,
						Segment = segment,
						Offset = segmentOffset,
						Flags = flags
					};
					
					offset += 6;
				}
				else
				{
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
					
					offset += 3;
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
		
		// First entry is module name, skip it
		var nameLength = bytes[offset];
		offset += nameLength + 3; // name length + ordinal (2 bytes)
		
		while (offset < bytes.Length)
		{
			nameLength = bytes[offset];
			if (nameLength == 0)
			{
				break;
			}
			
			var name = Encoding.ASCII.GetString(bytes, offset + 1, nameLength);
			var ordinal = BitConverter.ToUInt16(bytes, offset + nameLength + 1);
			
			names[name] = ordinal;
			offset += nameLength + 3;
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
		
		// First entry is module description, skip it
		var nameLength = bytes[offset];
		offset += nameLength + 3;
		
		while (offset < bytes.Length)
		{
			nameLength = bytes[offset];
			if (nameLength == 0)
			{
				break;
			}
			
			var name = Encoding.ASCII.GetString(bytes, offset + 1, nameLength);
			var ordinal = BitConverter.ToUInt16(bytes, offset + nameLength + 1);
			
			names[name] = ordinal;
			offset += nameLength + 3;
		}
		
		return names;
	}
	
	private List<string> ParseImportModuleTable(byte[] bytes, NeHeader header)
	{
		var modules = new List<string>();
		var moduleTableOffset = header.BaseOffset + header.ModuleReferenceTableOffset;
		var importNamesOffset = header.BaseOffset + header.ImportedNamesTableOffset;
		
		// Read module count from first entry
		// Each entry is 2 bytes (offset into imported names table)
		var offset = moduleTableOffset;
		
		while (offset < importNamesOffset)
		{
			var nameOffset = BitConverter.ToUInt16(bytes, offset);
			if (nameOffset == 0)
			{
				break;
			}
			
			var actualOffset = header.BaseOffset + header.ImportedNamesTableOffset + nameOffset;
			if (actualOffset >= bytes.Length)
			{
				break;
			}
			
			var nameLength = bytes[actualOffset];
			var moduleName = Encoding.ASCII.GetString(bytes, actualOffset + 1, nameLength);
			modules.Add(moduleName);
			
			offset += 2;
		}
		
		return modules;
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
		var synth = 0;
		
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
		foreach (var module in importModules)
		{
			var normalizedModule = module.ToUpperInvariant();
			
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
		var sections = new List<PeSection>();
		
		foreach (var segment in segments)
		{
			if (segmentMap.TryGetValue(segment.SegmentNumber, out var mappedSegment))
			{
				var rva = mappedSegment.address - baseAddress;
				var name = $"SEG{segment.SegmentNumber}";
				
				// Convert NE segment flags to PE section characteristics
				var characteristics = PeSectionCharacteristics.MemRead;
				
				// Bit 0: Data segment (vs code segment)
				if ((segment.Flags & 0x0001) != 0)
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
				
				sections.Add(new PeSection(name, rva, mappedSegment.size, mappedSegment.size, characteristics));
			}
		}
		
		return sections.ToArray();
	}
}

/// <summary>
/// NE (New Executable) header structure.
/// </summary>
internal class NeHeader
{
	public ushort Signature { get; init; }                    // "NE" signature (0x454E)
	public byte MajorLinkerVersion { get; init; }             // Linker major version
	public byte MinorLinkerVersion { get; init; }             // Linker minor version
	public ushort EntryTableOffset { get; init; }             // Offset to entry table
	public ushort EntryTableLength { get; init; }             // Length of entry table
	public uint CrcChecksum { get; init; }                    // CRC checksum
	public ushort ProgramFlags { get; init; }                 // Program flags
	public ushort ApplicationType { get; init; }              // Application type flags
	public ushort SegmentTableOffset { get; init; }           // Offset to segment table
	public ushort SegmentCount { get; init; }                 // Number of segments
	public ushort ResourceTableOffset { get; init; }          // Offset to resource table
	public ushort ResidentNameTableOffset { get; init; }      // Offset to resident name table
	public ushort ModuleReferenceTableOffset { get; init; }   // Offset to module reference table
	public ushort ImportedNamesTableOffset { get; init; }     // Offset to imported names table
	public uint NonResidentNameTableOffset { get; init; }     // File offset to non-resident name table
	public ushort MovableEntryCount { get; init; }            // Number of movable entries
	public byte TargetOS { get; init; }                       // Target operating system
	public ushort ExpectedWindowsVersion { get; init; }       // Expected Windows version
	public ushort EntryPointSegment { get; init; }            // Entry point segment number
	public ushort EntryPointOffset { get; init; }             // Entry point offset within segment
	public int BaseOffset { get; init; }                      // Base offset of NE header in file
}

/// <summary>
/// NE segment table entry.
/// </summary>
internal class NeSegment
{
	public int SegmentNumber { get; init; }    // Segment number (1-based)
	public uint FileOffset { get; init; }      // File offset to segment data
	public uint Length { get; init; }          // Length of segment in bytes
	public ushort Flags { get; init; }         // Segment flags
	public ushort MinAllocation { get; init; } // Minimum allocation size
}

/// <summary>
/// NE entry point.
/// </summary>
internal class NeEntryPoint
{
	public ushort Ordinal { get; init; }  // Ordinal number
	public byte Segment { get; init; }    // Segment number
	public ushort Offset { get; init; }   // Offset within segment
	public byte Flags { get; init; }      // Entry flags
}
