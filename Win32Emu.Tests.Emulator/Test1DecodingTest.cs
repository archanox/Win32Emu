using Iced.Intel;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

public class Test1DecodingTest
{
	private readonly ITestOutputHelper _output;
	
	public Test1DecodingTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void DecodeTest1_AddWithCSPrefix()
	{
		// Test 1 from 00.MOO: "add [cs:bp+di+4Eh],cl"
		// Bytes: 2E 00 4B 4E F4
		// 2E = CS segment override prefix
		// 00 4B 4E = add [bp+di+4Eh], cl
		// F4 = HLT
		
		var bytes = new byte[] { 0x2E, 0x00, 0x4B, 0x4E, 0xF4 };
		var decoder = Decoder.Create(16, bytes);
		decoder.IP = 0x850;
		
		var insn = decoder.Decode();
		_output.WriteLine($"Instruction: {insn}");
		_output.WriteLine($"Length: {insn.Length}");
		_output.WriteLine($"Has CS prefix: {insn.SegmentPrefix == Register.CS}");
		_output.WriteLine($"Decoder IP after: 0x{decoder.IP:X}");
		
		_output.WriteLine("");
		var insn2 = decoder.Decode();
		_output.WriteLine($"Next instruction: {insn2}");
		_output.WriteLine($"Length: {insn2.Length}");
		
		Assert.Equal(4, insn.Length);  // Should be 4 bytes with CS prefix
	}
}
