using Iced.Intel;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

public class InstructionDecodingTest
{
	private readonly ITestOutputHelper _output;
	
	public InstructionDecodingTest(ITestOutputHelper _output)
	{
		this._output = _output;
	}
	
	[Fact]
	public void DecodeAddInstruction_WithHltTerminator()
	{
		// From 00.MOO first test: "add [ss:bp+60h],bl"
		// Bytes: 00 5E 60 F4
		// Expected: First 3 bytes are ADD, last byte (F4) is HLT
		
		var bytes = new byte[] { 0x00, 0x5E, 0x60, 0xF4 };
		var decoder = Decoder.Create(16, bytes);
		decoder.IP = 0x72A0;
		
		// Decode first instruction
		var insn1 = decoder.Decode();
		_output.WriteLine($"First instruction: {insn1}");
		_output.WriteLine($"  Mnemonic: {insn1.Mnemonic}");
		_output.WriteLine($"  Length: {insn1.Length}");
		_output.WriteLine($"  Decoder IP after: 0x{decoder.IP:X}");
		
		// Decode second instruction (should be HLT)
		var insn2 = decoder.Decode();
		_output.WriteLine($"\nSecond instruction: {insn2}");
		_output.WriteLine($"  Mnemonic: {insn2.Mnemonic}");
		_output.WriteLine($"  Length: {insn2.Length}");
		_output.WriteLine($"  Decoder IP after: 0x{decoder.IP:X}");
		
		Assert.Equal(Mnemonic.Add, insn1.Mnemonic);
		Assert.Equal(3, insn1.Length);  // ADD should be 3 bytes
		Assert.Equal(Mnemonic.Hlt, insn2.Mnemonic);  // Next should be HLT
		Assert.Equal(1, insn2.Length);  // HLT is 1 byte
	}
}
