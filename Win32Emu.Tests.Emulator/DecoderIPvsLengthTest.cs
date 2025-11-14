using Iced.Intel;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

public class DecoderIPvsLengthTest
{
	private readonly ITestOutputHelper _output;
	
	public DecoderIPvsLengthTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void CompareDecoderIPvsInstructionLength()
	{
		// Test case 0 from 00.MOO
		var bytes = new byte[] { 0x00, 0x5E, 0x60, 0xF4 };
		var decoder = Decoder.Create(16, bytes);
		
		var initialIP = 0x72A0UL;
		decoder.IP = initialIP;
		
		var insn = decoder.Decode();
		
		_output.WriteLine($"Instruction: {insn}");
		_output.WriteLine($"Initial IP: 0x{initialIP:X}");
		_output.WriteLine($"Instruction Length: {insn.Length}");
		_output.WriteLine($"Decoder IP after decode: 0x{decoder.IP:X}");
		_output.WriteLine($"Expected next IP (Initial + Length): 0x{(initialIP + (ulong)insn.Length):X}");
		_output.WriteLine($"Match: {decoder.IP == (initialIP + (ulong)insn.Length)}");
	}
}
