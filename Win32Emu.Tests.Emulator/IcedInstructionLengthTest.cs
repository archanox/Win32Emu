using Iced.Intel;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Test to diagnose instruction length and EIP advancement issues in 16-bit mode
/// </summary>
public class IcedInstructionLengthTest
{
	private readonly ITestOutputHelper _output;
	
	public IcedInstructionLengthTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void TestAddInstructionLength_16BitMode()
	{
		// Test case from 00.MOO: "add [ss:bp+60h],bl"
		// Expected: 4 bytes (36 00 5E 60)
		// 36 = SS segment override prefix
		// 00 5E 60 = add [bp+60h], bl
		
		var bytes = new byte[] { 0x36, 0x00, 0x5E, 0x60, 0x00, 0x00 };
		var decoder = Decoder.Create(16, bytes);
		decoder.IP = 0x72A3;  // Initial EIP from test
		
		var insn = decoder.Decode();
		
		_output.WriteLine($"Instruction: {insn}");
		_output.WriteLine($"Length: {insn.Length}");
		_output.WriteLine($"Decoder IP before: 0x{0x72A3:X}");
		_output.WriteLine($"Decoder IP after: 0x{decoder.IP:X}");
		_output.WriteLine($"Expected EIP after: 0x{0x72A4:X}");
		_output.WriteLine($"Calculated EIP (oldEIP + length): 0x{(0x72A3 + insn.Length):X}");
		
		// The test expects EIP to be 0x72A4 after executing a 1-byte instruction starting at 0x72A3
		// But our code produces 0x72A3
		// This suggests the instruction is 1 byte, but the test expects the next instruction to be at 0x72A4
		
		Assert.Equal(1, insn.Length);  // Let's see what the actual length is
	}
	
	[Fact]
	public void TestAddRegisterInstruction_16BitMode()
	{
		// Test case from 00.MOO: "add ch,dl" 
		// This is a simple 2-byte instruction
		// Expected EIP: 0x00003A94 -> 0x00003A95 (advance by 1)
		
		// ADD r8, r8 is encoded as: 00 /r (ModR/M byte follows)
		// For "add ch,dl", it would be: 00 EA
		// 00 = ADD opcode (register to register, byte)
		// EA = ModR/M byte: 11 101 010 = CH, DL
		
		var bytes = new byte[] { 0x00, 0xEA, 0x00, 0x00 };
		var decoder = Decoder.Create(16, bytes);
		decoder.IP = 0x3A94;
		
		var insn = decoder.Decode();
		
		_output.WriteLine($"Instruction: {insn}");
		_output.WriteLine($"Length: {insn.Length}");
		_output.WriteLine($"Decoder IP after: 0x{decoder.IP:X}");
		_output.WriteLine($"Expected EIP after: 0x{0x3A95:X}");
		
		// If the instruction is 2 bytes, oldEIP + length = 0x3A94 + 2 = 0x3A96
		// But the test expects 0x3A95, which means the instruction should be 1 byte?
		// Or the test data might have EIP pointing mid-instruction?
	}
}
