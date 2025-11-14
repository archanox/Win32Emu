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
		
		// FINA section only contains changed registers/memory from INIT
		// We need to merge FINA with INIT to get the complete final state
		MergeFinalStateWithInitial(test);
		
		return test;
	}
	
	/// <summary>
	/// Merge final state with initial state to handle sparse FINA format.
	/// According to SingleStepTests/80386 format, FINA only contains changed values.
	/// Registers not present in FINA should use values from INIT.
	/// </summary>
	private static void MergeFinalStateWithInitial(MooTestCase test)
	{
		// Start with a copy of the initial register state
		var mergedRegs = new RegisterState
		{
			Eax = test.InitialState.Registers.Eax,
			Ebx = test.InitialState.Registers.Ebx,
			Ecx = test.InitialState.Registers.Ecx,
			Edx = test.InitialState.Registers.Edx,
			Esi = test.InitialState.Registers.Esi,
			Edi = test.InitialState.Registers.Edi,
			Ebp = test.InitialState.Registers.Ebp,
			Esp = test.InitialState.Registers.Esp,
			Eip = test.InitialState.Registers.Eip,
			Eflags = test.InitialState.Registers.Eflags,
			Cs = test.InitialState.Registers.Cs,
			Ds = test.InitialState.Registers.Ds,
			Es = test.InitialState.Registers.Es,
			Fs = test.InitialState.Registers.Fs,
			Gs = test.InitialState.Registers.Gs,
			Ss = test.InitialState.Registers.Ss
		};
		
		// Apply changes from final state - only for registers that were present in FINA
		if (test.FinalState.Registers.HasRegister(2)) mergedRegs.Eax = test.FinalState.Registers.Eax;
		if (test.FinalState.Registers.HasRegister(3)) mergedRegs.Ebx = test.FinalState.Registers.Ebx;
		if (test.FinalState.Registers.HasRegister(4)) mergedRegs.Ecx = test.FinalState.Registers.Ecx;
		if (test.FinalState.Registers.HasRegister(5)) mergedRegs.Edx = test.FinalState.Registers.Edx;
		if (test.FinalState.Registers.HasRegister(6)) mergedRegs.Esi = test.FinalState.Registers.Esi;
		if (test.FinalState.Registers.HasRegister(7)) mergedRegs.Edi = test.FinalState.Registers.Edi;
		if (test.FinalState.Registers.HasRegister(8)) mergedRegs.Ebp = test.FinalState.Registers.Ebp;
		if (test.FinalState.Registers.HasRegister(9)) mergedRegs.Esp = test.FinalState.Registers.Esp;
		if (test.FinalState.Registers.HasRegister(10)) mergedRegs.Cs = test.FinalState.Registers.Cs;
		if (test.FinalState.Registers.HasRegister(11)) mergedRegs.Ds = test.FinalState.Registers.Ds;
		if (test.FinalState.Registers.HasRegister(12)) mergedRegs.Es = test.FinalState.Registers.Es;
		if (test.FinalState.Registers.HasRegister(13)) mergedRegs.Fs = test.FinalState.Registers.Fs;
		if (test.FinalState.Registers.HasRegister(14)) mergedRegs.Gs = test.FinalState.Registers.Gs;
		if (test.FinalState.Registers.HasRegister(15)) mergedRegs.Ss = test.FinalState.Registers.Ss;
		if (test.FinalState.Registers.HasRegister(16)) mergedRegs.Eip = test.FinalState.Registers.Eip;
		if (test.FinalState.Registers.HasRegister(17)) mergedRegs.Eflags = test.FinalState.Registers.Eflags;
		
		test.FinalState.Registers = mergedRegs;
		
		// For memory, we need to merge too
		// Start with initial memory as baseline
		var mergedMemory = new Dictionary<uint, byte>();
		foreach (var entry in test.InitialState.Memory)
		{
			mergedMemory[entry.Address] = entry.Value;
		}
		
		// Apply changes from final memory
		foreach (var entry in test.FinalState.Memory)
		{
			mergedMemory[entry.Address] = entry.Value;
		}
		
		// Convert back to list
		test.FinalState.Memory = mergedMemory.Select(kvp => new MemoryEntry
		{
			Address = kvp.Key,
			Value = kvp.Value
		}).ToList();
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
		
		// The RG32 section format:
		// - First 4 bytes: bitmask indicating which registers are present
		// - Following bytes: register values for each bit set in the bitmask (in order)
		
		if (length < 4)
		{
			// Not enough data for bitmask
			pos += length;
			return regs;
		}
		
		// Read the bitmask
		var bitmask = ReadUInt32(data, ref pos);
		
		// Register order based on the reference implementation:
		// 0: CR0, 1: CR3, 2: EAX, 3: EBX, 4: ECX, 5: EDX,
		// 6: ESI, 7: EDI, 8: EBP, 9: ESP,
		// 10: CS, 11: DS, 12: ES, 13: FS, 14: GS, 15: SS,
		// 16: EIP, 17: EFLAGS, 18: DR6, 19: DR7
		
		// Read register values for each bit set in the bitmask
		for (int i = 0; i < 32; i++)
		{
			if ((bitmask & (1u << i)) != 0)
			{
				if (pos + 4 > startPos + length)
					break; // Prevent reading past the chunk
					
				var value = ReadUInt32(data, ref pos);
				
				// Store the register value and mark it as present
				regs.SetRegister(i, value);
			}
		}
		
		// Skip any remaining data in the chunk
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
		
		// According to MOO format v1, RAM section starts with entry count
		// Format: Entry Count (4 bytes) + Entries (5 bytes each: uint32 address + uint8 value)
		var entryCount = ReadUInt32(data, ref pos);
		
		// Read the specified number of entries
		for (uint i = 0; i < entryCount; i++)
		{
			// Ensure there are at least 5 bytes left for address (4) + value (1)
			if (pos + 5 > data.Length)
				break;
				
			var address = ReadUInt32(data, ref pos);
			var value = data[pos++];
			
			memory.Add(new MemoryEntry
			{
				Address = address,
				Value = value
			});
		}
		
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
	
	// Track which registers were explicitly set (for sparse FINA format)
	// Bit 0 = Eax, Bit 1 = Ebx, etc.
	internal uint PresenceMask { get; set; }
	
	internal void SetRegister(int bitIndex, uint value)
	{
		PresenceMask |= (1u << bitIndex);
		
		switch (bitIndex)
		{
			case 2: Eax = value; break;
			case 3: Ebx = value; break;
			case 4: Ecx = value; break;
			case 5: Edx = value; break;
			case 6: Esi = value; break;
			case 7: Edi = value; break;
			case 8: Ebp = value; break;
			case 9: Esp = value; break;
			case 10: Cs = value; break;
			case 11: Ds = value; break;
			case 12: Es = value; break;
			case 13: Fs = value; break;
			case 14: Gs = value; break;
			case 15: Ss = value; break;
			case 16: Eip = value; break;
			case 17: Eflags = value; break;
		}
	}
	
	internal bool HasRegister(int bitIndex)
	{
		return (PresenceMask & (1u << bitIndex)) != 0;
	}
}

/// <summary>
/// Represents a single memory location and its value
/// </summary>
public class MemoryEntry
{
	public uint Address { get; set; }
	public byte Value { get; set; }
}
