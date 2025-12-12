using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Win32Emu.NeParser
{
	/// <summary>
	/// Parser for NE (New Executable) format files used by Win16 applications.
	/// This is a standalone library with no dependencies on the emulator.
	/// </summary>
	public class NeParser
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
	
	// NE segment table entry size
	private const int NE_SEGMENT_ENTRY_SIZE = 8;
	
	// NE entry table constants
	private const byte NE_ENTRY_UNUSED = 0x00;
	private const byte NE_ENTRY_MOVABLE = 0xFF;
	private const int NE_ENTRY_MOVABLE_SIZE = 6;
	private const int NE_ENTRY_FIXED_SIZE = 3;
	private const int NE_IMPORTED_ENTRY_SIZE = 6;  // Size of an imported entry in the entry table
	
	// NE relocation record constants
	private const int NE_RELOC_HEADER_SIZE = 2;  // Relocation table starts with count (2 bytes)
	private const int NE_RELOC_ENTRY_SIZE = 8;   // Each relocation entry is 8 bytes
	private const byte NE_RELOC_TARGET_TYPE_MASK = 0x03;  // Mask to extract target type from relocation type byte
	
	// Module reference table format detection
	private const ushort MAX_STANDARD_FORMAT_OFFSET = 0x1000;  // Threshold to distinguish standard vs inline format
	
	// String validation constants
	private const int MAX_MODULE_NAME_LENGTH = 50;  // Maximum length for module names
	private const byte ASCII_PRINTABLE_MIN = 32;    // Minimum printable ASCII character
	private const byte ASCII_PRINTABLE_MAX = 126;   // Maximum printable ASCII character
	
	// NE name table entry suffix size (name length byte + 2-byte ordinal)
	private const int NE_NAME_ENTRY_SUFFIX_SIZE = 3;
	
	// NE module reference entry size
	private const int NE_MODULE_REF_ENTRY_SIZE = 2;
	
	// NE header field offsets
	private const int NE_OFFSET_ENTRY_TABLE = 0x04;
	private const int NE_OFFSET_ENTRY_TABLE_LENGTH = 0x06;
	private const int NE_OFFSET_SEGMENT_COUNT = 0x1E;
	private const int NE_OFFSET_MODULE_REF_COUNT = 0x20;
	private const int NE_OFFSET_MODULE_REF_TABLE = 0x2A;
	private const int NE_OFFSET_IMPORTED_NAMES_TABLE = 0x2C;
	
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
	
	/// <summary>
	/// Parses an NE executable from a file path.
	/// </summary>
	/// <param name="path">Path to the NE executable</param>
	/// <returns>Parsed NE executable data</returns>
	public static NeExecutable Parse(string path)
	{
		var bytes = File.ReadAllBytes(path);
		return Parse(bytes);
	}
	
	/// <summary>
	/// Parses an NE executable from byte array.
	/// </summary>
	/// <param name="bytes">Raw bytes of the NE executable</param>
	/// <returns>Parsed NE executable data</returns>
	public static NeExecutable Parse(byte[] bytes)
	{
		// Parse NE header
		var header = ParseNeHeader(bytes);
		
		// Parse segment table
		var segments = ParseSegmentTable(bytes, header);
		
		// Parse entry table
		var entryPoints = ParseEntryTable(bytes, header);
		
		// Parse resident and non-resident name tables
		var residentNames = ParseResidentNameTable(bytes, header);
		var nonResidentNames = ParseNonResidentNameTable(bytes, header);
		
		// Parse import module name table
		var importModules = ParseImportModuleTable(bytes, header);
		
		// Parse imports
		var imports = ParseNeImports(bytes, header, importModules);
		
		return new NeExecutable
		{
			Header = header,
			Segments = segments,
			EntryPoints = entryPoints,
			ResidentNames = residentNames,
			NonResidentNames = nonResidentNames,
			ImportModules = importModules,
			Imports = imports
		};
	}
	
	private static NeHeader ParseNeHeader(byte[] bytes)
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
	
	private static NeSegment[] ParseSegmentTable(byte[] bytes, NeHeader header)
	{
		var segments = new List<NeSegment>();
		var offset = header.BaseOffset + header.SegmentTableOffset;
		
		// Validate segment table bounds
		var requiredSize = offset + (header.SegmentCount * NE_SEGMENT_ENTRY_SIZE);
		if (requiredSize > bytes.Length)
		{
			throw new InvalidDataException($"Segment table extends beyond file bounds");
		}
		
		// Use the sector alignment shift from the header
		var sectorShift = header.SectorAlignmentShift;
		
		for (var i = 0; i < header.SegmentCount; i++)
		{
			// The file offset is stored as a shifted value
			var fileOffset = (uint)(BitConverter.ToUInt16(bytes, offset) << sectorShift);
			var lengthRaw = BitConverter.ToUInt16(bytes, offset + 2);
			var flags = BitConverter.ToUInt16(bytes, offset + 4);
			var minAllocation = BitConverter.ToUInt16(bytes, offset + 6);
			
			// If length is 0, it means full 64KB segment
			uint length = lengthRaw;
			if (length == 0 && minAllocation > 0)
			{
				length = 0x10000; // 64KB full segment
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
	
	private static Dictionary<ushort, NeEntryPoint> ParseEntryTable(byte[] bytes, NeHeader header)
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
	
	private static Dictionary<string, ushort> ParseResidentNameTable(byte[] bytes, NeHeader header)
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
	
	private static Dictionary<string, ushort> ParseNonResidentNameTable(byte[] bytes, NeHeader header)
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
	
	private static List<string> ParseImportModuleTable(byte[] bytes, NeHeader header)
	{
		var modules = new List<string>();
		var moduleTableOffset = header.BaseOffset + header.ModuleReferenceTableOffset;
		var importNamesOffset = header.BaseOffset + header.ImportedNamesTableOffset;
		
		var moduleCount = header.ModuleReferenceCount;
		
		// The Module Reference Table format varies between NE implementations:
		// Standard format: Array of 2-byte offsets into the Imported Names Table
		// Alternative format (Windows ME and some others): Inline Pascal strings
		
		// Try to detect which format by checking if the first entry looks like an offset or a string
		if (moduleTableOffset + 2 > bytes.Length)
			return modules;
		
		var firstValue = BitConverter.ToUInt16(bytes, moduleTableOffset);
		var potentialNameAddr = importNamesOffset + firstValue;
		
		// If first value is a small number and the target address has a valid Pascal string, use standard format
		// Otherwise use inline format
		bool useInlineFormat = false;
		
		if (firstValue < MAX_STANDARD_FORMAT_OFFSET && potentialNameAddr + 1 < bytes.Length)
		{
			var nameLen = bytes[potentialNameAddr];
			if (nameLen > 0 && nameLen < MAX_MODULE_NAME_LENGTH && potentialNameAddr + nameLen + 1 < bytes.Length)
			{
				// Check if it's a valid string
				bool validString = true;
				for (int j = 1; j <= nameLen && validString; j++)
				{
					var ch = bytes[potentialNameAddr + j];
					if (ch < ASCII_PRINTABLE_MIN || ch > ASCII_PRINTABLE_MAX)
						validString = false;
				}
				if (!validString)
					useInlineFormat = true;
			}
			else
			{
				useInlineFormat = true;
			}
		}
		else
		{
			useInlineFormat = true;
		}
		
		if (useInlineFormat)
		{
			// Parse inline Pascal strings
			var offset = moduleTableOffset;
			for (var i = 0; i < moduleCount; i++)
			{
				if (offset + 1 > bytes.Length)
					break;
				
				var nameLength = bytes[offset];
				if (nameLength == 0)
				{
					offset++;
					continue;
				}
				
				if (offset + nameLength + 1 > bytes.Length)
					break;
				
				// Validate printable ASCII
				bool valid = true;
				for (var j = 1; j <= nameLength; j++)
				{
					var ch = bytes[offset + j];
					if (ch < ASCII_PRINTABLE_MIN || ch > ASCII_PRINTABLE_MAX)
					{
						valid = false;
						break;
					}
				}
				
				if (valid)
				{
					var moduleName = Encoding.ASCII.GetString(bytes, offset + 1, nameLength);
					modules.Add(moduleName);
				}
				
				offset += nameLength + 1;
			}
		}
		else
		{
			// Standard format: 2-byte offsets
			for (var i = 0; i < moduleCount; i++)
			{
				var offset = moduleTableOffset + (i * NE_MODULE_REF_ENTRY_SIZE);
				
				if (offset + NE_MODULE_REF_ENTRY_SIZE > bytes.Length)
					break;
				
				var nameOffset = BitConverter.ToUInt16(bytes, offset);
				if (nameOffset == 0)
					continue;
				
				var actualOffset = importNamesOffset + nameOffset;
				
				if (actualOffset + 1 > bytes.Length)
					continue;
				
				var nameLength = bytes[actualOffset];
				if (nameLength == 0 || nameLength > MAX_MODULE_NAME_LENGTH)
					continue;
				
				if (actualOffset + nameLength + 1 > bytes.Length)
					continue;
				
				// Validate printable ASCII characters
				bool isValidName = true;
				for (var j = 1; j <= nameLength; j++)
				{
					var ch = (char)bytes[actualOffset + j];
					if (ch < ASCII_PRINTABLE_MIN || ch > ASCII_PRINTABLE_MAX)
					{
						isValidName = false;
						break;
					}
				}
				
				if (!isValidName)
					continue;
				
				var moduleName = Encoding.ASCII.GetString(bytes, actualOffset + 1, nameLength);
				modules.Add(moduleName);
			}
		}
		
		return modules;
	}
	
	/// <summary>
	/// Parses the NE (New Executable) format to extract imported functions with module and function details.
	/// In NE format, imports are stored in segment relocation records, not in the entry table.
	/// </summary>
	private static Dictionary<string, List<NeImportedFunction>> ParseNeImports(byte[] fileBytes, NeHeader header, List<string> importModules)
	{
		var importsByModule = new Dictionary<string, List<NeImportedFunction>>(StringComparer.OrdinalIgnoreCase);

		if (fileBytes == null || header == null || importModules == null || importModules.Count == 0)
		{
			return importsByModule;
		}

		try
		{
			var importedNamesTableOffset = header.BaseOffset + header.ImportedNamesTableOffset;
			var segments = ParseSegmentTable(fileBytes, header);
			
			// Parse relocation records in each segment to find imports
			foreach (var segment in segments.Where(s => (s.Flags & (ushort)NeSegmentFlags.HasRelocations) != 0))
			{
				
				// Relocations are stored at the end of the segment data
				// The last 2 bytes of the segment give the count of relocation entries
				if (segment.FileOffset == 0 || segment.Length == 0)
					continue;
				
				// Read relocation count from end of segment
				var relocCountOffset = (int)(segment.FileOffset + segment.Length);
				if (relocCountOffset + 2 > fileBytes.Length)
					continue;
				
				var relocCount = BitConverter.ToUInt16(fileBytes, relocCountOffset);
				if (relocCount == 0)
					continue;
				
				// Relocation entries follow the count
				var relocOffset = relocCountOffset + NE_RELOC_HEADER_SIZE;
				
				// Track seen imports to avoid duplicates efficiently
				var seenImports = new HashSet<string>();
				
				for (var i = 0; i < relocCount; i++)
				{
					if (relocOffset + NE_RELOC_ENTRY_SIZE > fileBytes.Length)
						break;
					
					// Relocation entry structure (8 bytes):
					// Byte 0: Address type (source type)
					// Byte 1: Relocation type (target flags)
					// Bytes 2-3: Offset in segment
					// Bytes 4-7: Target specification
					
					var relocationType = fileBytes[relocOffset + 1];
					
					// Extract relocation target type from lower 3 bits
					var targetType = (NeRelocationTargetType)(relocationType & NE_RELOC_TARGET_TYPE_MASK);
					
					if (targetType == NeRelocationTargetType.ImportOrdinal || targetType == NeRelocationTargetType.ImportName)
					{
						// This is an import!
						// Bytes 4-5: Module index (1-based)
						// Bytes 6-7: Ordinal or name offset
						var moduleIndex = BitConverter.ToUInt16(fileBytes, relocOffset + 4);
						var importRef = BitConverter.ToUInt16(fileBytes, relocOffset + 6);
						
						if (moduleIndex > 0 && moduleIndex <= importModules.Count)
						{
							var moduleName = importModules[moduleIndex - 1];
							
							if (!importsByModule.ContainsKey(moduleName))
								importsByModule[moduleName] = new List<NeImportedFunction>();
							
							if (targetType == NeRelocationTargetType.ImportOrdinal)
							{
								// Imported by ordinal
								var importKey = $"{moduleName}:Ordinal_{importRef}";
								if (!seenImports.Contains(importKey))
								{
									seenImports.Add(importKey);
									importsByModule[moduleName].Add(new NeImportedFunction
									{
										Name = $"Ordinal_{importRef}",
										Ordinal = importRef,
										ImportedByOrdinal = true
									});
								}
							}
							else if (targetType == NeRelocationTargetType.ImportName)
							{
								// Imported by name
								var nameOffset = importedNamesTableOffset + importRef;
								if (nameOffset < fileBytes.Length)
								{
									var nameLength = fileBytes[nameOffset];
									if (nameLength > 0 && nameOffset + 1 + nameLength <= fileBytes.Length)
									{
										var functionName = Encoding.ASCII.GetString(fileBytes, (int)nameOffset + 1, nameLength);
										if (!string.IsNullOrWhiteSpace(functionName))
										{
											// Avoid duplicates
											var importKey = $"{moduleName}:{functionName}";
											if (!seenImports.Contains(importKey))
											{
												seenImports.Add(importKey);
												importsByModule[moduleName].Add(new NeImportedFunction
												{
													Name = functionName,
													ImportedByOrdinal = false
												});
											}
										}
									}
								}
							}
						}
					}
					
					relocOffset += NE_RELOC_ENTRY_SIZE;
				}
			}
		}
		catch
		{
			// Return partial results on error - library users can check if result is empty
			return importsByModule;
		}

		return importsByModule;
	}
	
	/// <summary>
	/// Read a Pascal string (length-prefixed) from the file.
	/// </summary>
	internal static string? ReadPascalString(byte[] bytes, int offset)
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
}
}
