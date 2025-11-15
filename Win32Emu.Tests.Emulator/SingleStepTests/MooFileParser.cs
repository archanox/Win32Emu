using System.Buffers.Binary;
using System.Buffers;
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
			
			// Use ArrayPool to reduce GC pressure from large temporary allocations
			var bufferSize = 64 * 1024; // 64KB chunks
			var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
			try
			{
				using var memoryStream = new MemoryStream();
				int bytesRead;
				while ((bytesRead = gzipStream.Read(buffer, 0, bufferSize)) > 0)
				{
					memoryStream.Write(buffer, 0, bytesRead);
				}
				data = memoryStream.ToArray();
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
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
		
		// Read MOO chunk header
		if (!ReadMagic(data, ref pos, "MOO "))
		{
			throw new InvalidDataException("Invalid MOO file: missing MOO header");
		}
		
		var mooChunkLength = ReadUInt32(data, ref pos);
		var mooChunkEnd = pos + (int)mooChunkLength;
		
		// Read MOO header data
		result.VersionMajor = data[pos++];
		result.VersionMinor = data[pos++];
		pos += 2; // Reserved bytes
		result.TestCount = ReadUInt32(data, ref pos);
		
		// Read CPU name - the remaining bytes in the MOO chunk
		var cpuNameLength = mooChunkEnd - pos;
		if (cpuNameLength > 0 && cpuNameLength <= 8)
		{
			result.CpuName = Encoding.ASCII.GetString(data, pos, cpuNameLength);
		}
		else if (cpuNameLength > 8)
		{
			// Shouldn't happen, but handle gracefully - read only first 8 bytes
			result.CpuName = Encoding.ASCII.GetString(data, pos, 8);
		}
		
		// Skip to end of MOO chunk
		pos = mooChunkEnd;
		
		// Read all test cases
		while (pos < data.Length)
		{
			// Peek at chunk type
			if (pos + 8 > data.Length)
				break;
				
			var chunkType = Encoding.ASCII.GetString(data, pos, 4);
			
			if (chunkType == "TEST")
			{
				var test = ReadTestCase(data, ref pos);
				result.Tests.Add(test);
			}
			else if (chunkType == "META")
			{
				// Skip META chunk
				pos += 4; // Skip type
				var metaLength = ReadUInt32(data, ref pos);
				pos += (int)metaLength;
			}
			else
			{
				// Skip unknown chunk
				pos += 4; // Skip type
				if (pos + 4 <= data.Length)
				{
					var unknownLength = ReadUInt32(data, ref pos);
					pos += (int)unknownLength;
				}
				else
				{
					break;
				}
			}
		}
		
		return result;
	}
	
	private static MooTestCase ReadTestCase(byte[] data, ref int pos)
	{
		var test = new MooTestCase();
		
		// Skip "TEST" magic (already verified by caller)
		pos += 4;
		
		var testLength = ReadUInt32(data, ref pos);
		var testEndPos = pos + (int)testLength;
		
		// Read test index
		test.Index = ReadUInt32(data, ref pos);
		
		while (pos < testEndPos && pos < data.Length)
		{
			// Peek at chunk type
			if (pos + 8 > data.Length)
				break;
				
			var chunkType = Encoding.ASCII.GetString(data, pos, 4);
			pos += 4;
			var chunkLength = ReadUInt32(data, ref pos);
			var chunkEndPos = pos + (int)chunkLength;
			
			if (chunkType == "GMET")
			{
				// Skip GMET data
				pos = chunkEndPos;
			}
			else if (chunkType == "NAME")
			{
				var nameLength = ReadUInt32(data, ref pos);
				var availableBytes = chunkEndPos - pos;
				var nameBytesToRead = (int)Math.Min(nameLength, availableBytes);
				var nameBytes = new byte[nameBytesToRead];
				Array.Copy(data, pos, nameBytes, 0, nameBytesToRead);
				test.Name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
				pos = chunkEndPos;
			}
			else if (chunkType == "BYTS")
			{
				var instrLength = ReadUInt32(data, ref pos);
				var bytesAvailable = chunkEndPos - pos;
				var bytesToRead = Math.Min((int)instrLength, bytesAvailable);
				test.InstructionBytes = new byte[bytesToRead];
				Array.Copy(data, pos, test.InstructionBytes, 0, bytesToRead);
				pos = chunkEndPos;
			}
			else if (chunkType == "INIT")
			{
				test.InitialState = ReadStateSection(data, ref pos, chunkEndPos);
			}
			else if (chunkType == "FINA")
			{
				test.FinalState = ReadStateSection(data, ref pos, chunkEndPos);
			}
			else if (chunkType == "CYCL")
			{
				// Skip cycle data for now
				pos = chunkEndPos;
			}
			else if (chunkType == "HASH")
			{
				test.Hash = new byte[chunkLength];
				Array.Copy(data, pos, test.Hash, 0, (int)chunkLength);
				pos = chunkEndPos;
			}
			else
			{
				// Skip unknown chunk
				pos = chunkEndPos;
			}
			
			// Ensure we're at chunk boundary
			if (pos != chunkEndPos)
			{
				pos = chunkEndPos;
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
	
	private static CpuTestState ReadStateSection(byte[] data, ref int pos, int endPos)
	{
		var state = new CpuTestState();
		
		while (pos < endPos && pos < data.Length)
		{
			// Peek at chunk type
			if (pos + 8 > data.Length || pos + 8 > endPos)
				break;
				
			var chunkType = Encoding.ASCII.GetString(data, pos, 4);
			pos += 4;
			var chunkLength = ReadUInt32(data, ref pos);
			var chunkEndPos = pos + (int)chunkLength;
			
			if (chunkType == "RG32")
			{
				state.Registers = ReadRegisterState(data, ref pos, (int)chunkLength);
			}
			else if (chunkType == "EA32")
			{
				// EA32 contains effective address info, skip for now
				pos = chunkEndPos;
			}
			else if (chunkType == "RAM ")
			{
				state.Memory = ReadMemoryState(data, ref pos, (int)chunkLength);
			}
			else
			{
				// Skip unknown chunk
				pos = chunkEndPos;
			}
			
			// Ensure we're at chunk boundary
			if (pos != chunkEndPos)
			{
				pos = chunkEndPos;
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
	public byte VersionMajor { get; set; }
	public byte VersionMinor { get; set; }
	public uint TestCount { get; set; }
	public string CpuName { get; set; } = string.Empty;
	public List<MooTestCase> Tests { get; set; } = new();
}

/// <summary>
/// Represents a single CPU test case from a MOO file
/// </summary>
public class MooTestCase
{
	public uint Index { get; set; }
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
