using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

public class InvestigateTest39
{
	private readonly ITestOutputHelper _output;

	public InvestigateTest39(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void AnalyzeTest39Details()
	{
		var testFile = TestFileHelper.FindTestFile("03.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found");
			return;
		}

		var mooFile = MooFileParser.Parse(testFile);
		var test = mooFile.Tests[39];

		_output.WriteLine($"Test 39: {test.Name}");
		_output.WriteLine($"\nInstruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
		
		_output.WriteLine($"\nInitial state:");
		_output.WriteLine($"  EAX=0x{test.InitialState.Registers.Eax:X8}");
		_output.WriteLine($"  EBX=0x{test.InitialState.Registers.Ebx:X8}");
		_output.WriteLine($"  ESP=0x{test.InitialState.Registers.Esp:X8}");
		_output.WriteLine($"  EIP=0x{test.InitialState.Registers.Eip:X8}");
		_output.WriteLine($"  CS=0x{test.InitialState.Registers.Cs:X4}");
		_output.WriteLine($"  DS=0x{test.InitialState.Registers.Ds:X4}");
		_output.WriteLine($"  SS=0x{test.InitialState.Registers.Ss:X4}");
		_output.WriteLine($"  EFLAGS=0x{test.InitialState.Registers.Eflags:X8}");

		_output.WriteLine($"\nFinal state:");
		_output.WriteLine($"  EAX=0x{test.FinalState.Registers.Eax:X8}");
		_output.WriteLine($"  EBX=0x{test.FinalState.Registers.Ebx:X8}");
		_output.WriteLine($"  ESP=0x{test.FinalState.Registers.Esp:X8}");
		_output.WriteLine($"  EIP=0x{test.FinalState.Registers.Eip:X8}");
		_output.WriteLine($"  CS=0x{test.FinalState.Registers.Cs:X4}");
		_output.WriteLine($"  SS=0x{test.FinalState.Registers.Ss:X4}");
		_output.WriteLine($"  EFLAGS=0x{test.FinalState.Registers.Eflags:X8}");

		// Analyze stack address
		var stackPhysAddr = (test.FinalState.Registers.Ss << 4) + (test.FinalState.Registers.Esp & 0xFFFF);
		_output.WriteLine($"\nFinal stack physical address: SS:ESP = 0x{test.FinalState.Registers.Ss:X4}:0x{test.FinalState.Registers.Esp & 0xFFFF:X4} = 0x{stackPhysAddr:X5}");

		// Analyze accessed memory address
		var dsOffset = test.InitialState.Registers.Ebx & 0xFFFF;
		var dsPhysAddr = (test.InitialState.Registers.Ds << 4) + dsOffset;
		_output.WriteLine($"\nMemory access: DS:BX = 0x{test.InitialState.Registers.Ds:X4}:0x{dsOffset:X4} = 0x{dsPhysAddr:X5}");
		_output.WriteLine($"This tries to read 16-bit value from DS:0x{dsOffset:X4}");
		if (dsOffset == 0xFFFF)
		{
			_output.WriteLine($"WARNING: Reading from offset 0xFFFF would access bytes at 0xFFFF and 0x0000 (segment wrap)!");
		}

		_output.WriteLine($"\nMemory changes in final state: {test.FinalState.Memory.Count}");
		foreach (var mem in test.FinalState.Memory)
		{
			// Try to figure out what segment this belongs to
			var possibleSS_offset = mem.Address - (test.FinalState.Registers.Ss << 4);
			_output.WriteLine($"  @0x{mem.Address:X8} = 0x{mem.Value:X2}  (SS:0x{possibleSS_offset:X})");
		}
	}
}
