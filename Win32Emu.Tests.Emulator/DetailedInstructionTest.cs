using Iced.Intel;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

public class DetailedInstructionTest
{
	private readonly ITestOutputHelper _output;
	
	public DetailedInstructionTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void AnalyzeAddInstructionBytes()
	{
		// Try different interpretations of the bytes
		var allBytes = new byte[] { 0x00, 0x5E, 0x60, 0xF4 };
		
		_output.WriteLine("Analyzing bytes: 00 5E 60 F4");
		_output.WriteLine("");
		
		// Decode starting from each position
		for (int start = 0; start < allBytes.Length; start++)
		{
			var decoder = Decoder.Create(16, allBytes.Skip(start).ToArray());
			decoder.IP = (ulong)(0x72A0 + start);
			var insn = decoder.Decode();
			
			_output.WriteLine($"Starting at byte {start} (address 0x{0x72A0 + start:X}):");
			_output.WriteLine($"  Instruction: {insn}");
			_output.WriteLine($"  Mnemonic: {insn.Mnemonic}");
			_output.WriteLine($"  Length: {insn.Length}");
			_output.WriteLine($"  Op0: {insn.Op0Kind} - {insn.GetOpRegister(0)}");
			_output.WriteLine($"  Op1: {insn.Op1Kind} - {insn.GetOpRegister(1)}");
			if (insn.Op0Kind == OpKind.Memory)
			{
				_output.WriteLine($"  Memory base: {insn.MemoryBase}");
				_output.WriteLine($"  Memory disp: 0x{insn.MemoryDisplacement32:X}");
				_output.WriteLine($"  Segment: {insn.SegmentPrefix}");
			}
			_output.WriteLine("");
		}
		
		// Now check what a 4-byte ADD would look like
		_output.WriteLine("If we force-decode as 4 bytes from start:");
		var fullDecoder = Decoder.Create(16, allBytes);
		fullDecoder.IP = 0x72A0;
		var fullInsn = fullDecoder.Decode();
		_output.WriteLine($"  Instruction: {fullInsn}");
		_output.WriteLine($"  Length: {fullInsn.Length}");
		_output.WriteLine($"  Next instruction IP: 0x{fullDecoder.IP:X}");
	}
}
