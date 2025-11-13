using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Parser for MOO (Moolah) test files from the SingleStepTests/80386 repository.
/// These files contain hardware-generated CPU test cases for validating x86 emulators.
/// </summary>
public class MooFileParser
{
	/// <summary>
	/// Parse a MOO file (optionally gzipped) and extract all test cases
	/// </summary>
	public static MooTestFile Parse(string filePath)
	{
		byte[] data;
		
		// Handle gzipped files
		if (filePath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
		{
			using var fileStream = File.OpenRead(filePath);
			using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
			using var memoryStream = new MemoryStream();
			gzipStream.CopyTo(memoryStream);
			data = memoryStream.ToArray();
		}
		else
		{
			data = File.ReadAllBytes(filePath);
		}
		
		return ParseBytes(data);
	}
	
	/// <summary>
	/// Parse MOO file data from a byte array
	/// </summary>
	public static MooTestFile ParseBytes(byte[] data)
	{
		var result = new MooTestFile();
		var pos = 0;
		
		// Read header
		if (!ReadMagic(data, ref pos, "MOO "))
		{
			throw new InvalidDataException("Invalid MOO file: missing MOO header");
		}
		
		// Skip version/size info (12 bytes)
		pos += 12;
		
		// Read metadata section
		if (ReadMagic(data, ref pos, "386E"))
		{
			ReadMagic(data, ref pos, "META");
			var metaLength = ReadUInt32(data, ref pos);
			// Skip metadata for now
			pos += (int)metaLength;
		}
		
		// Read all test cases
		while (pos < data.Length)
		{
			if (ReadMagic(data, ref pos, "TEST"))
			{
				var test = ReadTestCase(data, ref pos);
				result.Tests.Add(test);
			}
			else
			{
				// Skip unknown sections
				pos++;
			}
		}
		
		return result;
	}
	
	private static MooTestCase ReadTestCase(byte[] data, ref int pos)
	{
		var test = new MooTestCase();
		var testLength = ReadUInt32(data, ref pos);
		var testEndPos = pos + (int)testLength;
		
		while (pos < testEndPos && pos < data.Length)
		{
			if (ReadMagic(data, ref pos, "GMET"))
			{
				var gmetLength = ReadUInt32(data, ref pos);
				pos += (int)gmetLength; // Skip GMET data
			}
			else if (ReadMagic(data, ref pos, "NAME"))
			{
				var nameLength = ReadUInt32(data, ref pos);
				var skipBytes = ReadUInt32(data, ref pos);
				var nameBytes = new byte[nameLength - 4];
				Array.Copy(data, pos, nameBytes, 0, nameBytes.Length);
				test.Name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
				pos += nameBytes.Length;
			}
			else if (ReadMagic(data, ref pos, "BYTS"))
			{
				var bytsLength = ReadUInt32(data, ref pos);
				var instrLength = ReadUInt32(data, ref pos);
				test.InstructionBytes = new byte[instrLength];
				Array.Copy(data, pos, test.InstructionBytes, 0, (int)instrLength);
				pos += (int)(bytsLength - 4);
			}
			else if (ReadMagic(data, ref pos, "INIT"))
			{
				test.InitialState = ReadStateSection(data, ref pos);
			}
			else if (ReadMagic(data, ref pos, "FINA"))
			{
				test.FinalState = ReadStateSection(data, ref pos);
			}
			else if (ReadMagic(data, ref pos, "CYCL"))
			{
				var cyclLength = ReadUInt32(data, ref pos);
				pos += (int)cyclLength; // Skip cycle data for now
			}
			else if (ReadMagic(data, ref pos, "HASH"))
			{
				var hashLength = ReadUInt32(data, ref pos);
				test.Hash = new byte[hashLength];
				Array.Copy(data, pos, test.Hash, 0, (int)hashLength);
				pos += (int)hashLength;
			}
			else
			{
				pos++;
			}
		}
		
		return test;
	}
	
	private static CpuTestState ReadStateSection(byte[] data, ref int pos)
	{
		var state = new CpuTestState();
		var initLength = ReadUInt32(data, ref pos);
		var initEndPos = pos + (int)initLength;
		
		while (pos < initEndPos && pos < data.Length)
		{
			if (ReadMagic(data, ref pos, "RG32"))
			{
				var regLength = ReadUInt32(data, ref pos);
				state.Registers = ReadRegisterState(data, ref pos, (int)regLength);
			}
			else if (ReadMagic(data, ref pos, "EA32"))
			{
				var eaLength = ReadUInt32(data, ref pos);
				// EA32 contains effective address info, skip for now
				pos += (int)eaLength;
			}
			else if (ReadMagic(data, ref pos, "RAM "))
			{
				var ramLength = ReadUInt32(data, ref pos);
				state.Memory = ReadMemoryState(data, ref pos, (int)ramLength);
			}
			else
			{
				pos++;
			}
		}
		
		return state;
	}
	
	private static RegisterState ReadRegisterState(byte[] data, ref int pos, int length)
	{
		var regs = new RegisterState();
		var startPos = pos;
		
		// The RG32 section contains register values in a specific order
		// Based on analysis: appears to be EFLAGS, then general purpose registers
		// We need at least 11 uint32 values for the basic registers
		if (length < 44)
		{
			// Not enough data, return empty state
			return regs;
		}
		
		// Register order appears to be:
		// EFLAGS (with high word), EIP, then general purpose registers
		// This is based on reverse engineering the format
		regs.Eflags = ReadUInt32(data, ref pos);
		var flagsHigh = ReadUInt32(data, ref pos); // High word of flags (ignored for now)
		regs.Eip = ReadUInt32(data, ref pos);
		regs.Eax = ReadUInt32(data, ref pos);
		regs.Ebx = ReadUInt32(data, ref pos);
		regs.Ecx = ReadUInt32(data, ref pos);
		regs.Edx = ReadUInt32(data, ref pos);
		regs.Esi = ReadUInt32(data, ref pos);
		regs.Edi = ReadUInt32(data, ref pos);
		regs.Ebp = ReadUInt32(data, ref pos);
		regs.Esp = ReadUInt32(data, ref pos);
		
		// Read segment registers if available
		if (length >= 68)
		{
			regs.Cs = ReadUInt32(data, ref pos);
			regs.Ds = ReadUInt32(data, ref pos);
			regs.Es = ReadUInt32(data, ref pos);
			regs.Fs = ReadUInt32(data, ref pos);
			regs.Gs = ReadUInt32(data, ref pos);
			regs.Ss = ReadUInt32(data, ref pos);
		}
		
		// Skip any remaining register data
		var bytesRead = pos - startPos;
		var remaining = length - bytesRead;
		if (remaining > 0)
		{
			pos += remaining;
		}
		
		return regs;
	}
	
	private static List<MemoryEntry> ReadMemoryState(byte[] data, ref int pos, int length)
	{
		var memory = new List<MemoryEntry>();
		var endPos = pos + length;
		
		// Each memory entry is 5 bytes: 4-byte address + 1-byte value
		while (pos < endPos - 5 + 1 && pos < data.Length - 5 + 1)
		{
			var address = ReadUInt32(data, ref pos);
			var value = data[pos++];
			
			memory.Add(new MemoryEntry
			{
				Address = address,
				Value = value
			});
		}
		
		// Advance to end of section to prevent subsequent parsing errors
		pos = endPos;
		
		return memory;
	}
	
	private static bool ReadMagic(byte[] data, ref int pos, string expected)
	{
		if (pos + expected.Length > data.Length)
			return false;
			
		var actual = Encoding.ASCII.GetString(data, pos, expected.Length);
		if (actual == expected)
		{
			pos += expected.Length;
			return true;
		}
		return false;
	}
	
	private static uint ReadUInt32(byte[] data, ref int pos)
	{
		var value = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(pos, 4));
		pos += 4;
		return value;
	}
}

/// <summary>
/// Represents a parsed MOO test file
/// </summary>
public class MooTestFile
{
	public List<MooTestCase> Tests { get; set; } = new();
}

/// <summary>
/// Represents a single CPU test case from a MOO file
/// </summary>
public class MooTestCase
{
	public string Name { get; set; } = string.Empty;
	public byte[] InstructionBytes { get; set; } = Array.Empty<byte>();
	public CpuTestState InitialState { get; set; } = new();
	public CpuTestState FinalState { get; set; } = new();
	public byte[] Hash { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Represents CPU state for a test (initial or final)
/// </summary>
public class CpuTestState
{
	public RegisterState Registers { get; set; } = new();
	public List<MemoryEntry> Memory { get; set; } = new();
}

/// <summary>
/// Represents CPU register values
/// </summary>
public class RegisterState
{
	public uint Eax { get; set; }
	public uint Ebx { get; set; }
	public uint Ecx { get; set; }
	public uint Edx { get; set; }
	public uint Esi { get; set; }
	public uint Edi { get; set; }
	public uint Ebp { get; set; }
	public uint Esp { get; set; }
	public uint Eip { get; set; }
	public uint Eflags { get; set; }
	
	// Segment registers
	public uint Cs { get; set; }
	public uint Ds { get; set; }
	public uint Es { get; set; }
	public uint Fs { get; set; }
	public uint Gs { get; set; }
	public uint Ss { get; set; }
}

/// <summary>
/// Represents a single memory location and its value
/// </summary>
public class MemoryEntry
{
	public uint Address { get; set; }
	public byte Value { get; set; }
}
