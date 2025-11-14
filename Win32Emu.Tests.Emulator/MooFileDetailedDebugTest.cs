using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using System.Linq;

namespace Win32Emu.Tests.Emulator;

public class MooFileDetailedDebugTest
{
	private readonly ITestOutputHelper _output;
	
	public MooFileDetailedDebugTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void ExamineFirstTestInDetail()
	{
		var testFile = FindTestFile("00.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		var test = mooFile.Tests[0];
		
		_output.WriteLine($"==== TEST 0 ====");
		_output.WriteLine($"Name: {test.Name}");
		_output.WriteLine("");
		
		_output.WriteLine("INSTRUCTION BYTES:");
		for (int i = 0; i < test.InstructionBytes.Length; i++)
		{
			_output.WriteLine($"  [{i}] = 0x{test.InstructionBytes[i]:X2}");
		}
		_output.WriteLine("");
		
		_output.WriteLine("INITIAL STATE:");
		_output.WriteLine($"  EIP: 0x{test.InitialState.Registers.Eip:X8}");
		_output.WriteLine($"  EAX: 0x{test.InitialState.Registers.Eax:X8}");
		_output.WriteLine($"  EBX: 0x{test.InitialState.Registers.Ebx:X8}");
		_output.WriteLine($"  EBP: 0x{test.InitialState.Registers.Ebp:X8}");
		_output.WriteLine($"  ESP: 0x{test.InitialState.Registers.Esp:X8}");
		_output.WriteLine($"  EFLAGS: 0x{test.InitialState.Registers.Eflags:X8}");
		_output.WriteLine($"  Memory entries: {test.InitialState.Memory.Count}");
		
		_output.WriteLine("");
		_output.WriteLine("INITIAL MEMORY STATE:");
		foreach (var mem in test.InitialState.Memory.OrderBy(m => m.Address))
		{
			_output.WriteLine($"  [0x{mem.Address:X8}] = 0x{mem.Value:X2}");
		}
		
		_output.WriteLine("");
		_output.WriteLine("FINAL STATE:");
		_output.WriteLine($"  EIP: 0x{test.FinalState.Registers.Eip:X8}");
		_output.WriteLine($"  EAX: 0x{test.FinalState.Registers.Eax:X8}");
		_output.WriteLine($"  EBX: 0x{test.FinalState.Registers.Ebx:X8}");
		_output.WriteLine($"  EBP: 0x{test.FinalState.Registers.Ebp:X8}");
		_output.WriteLine($"  ESP: 0x{test.FinalState.Registers.Ebp:X8}");
		_output.WriteLine($"  EFLAGS: 0x{test.FinalState.Registers.Eflags:X8}");
		_output.WriteLine($"  Memory entries: {test.FinalState.Memory.Count}");
		
		_output.WriteLine("");
		_output.WriteLine("FINAL MEMORY STATE:");
		foreach (var mem in test.FinalState.Memory.OrderBy(m => m.Address))
		{
			_output.WriteLine($"  [0x{mem.Address:X8}] = 0x{mem.Value:X2}");
		}
		
		_output.WriteLine("");
		_output.WriteLine("ANALYSIS:");
		_output.WriteLine($"  EIP advancement: {test.FinalState.Registers.Eip - test.InitialState.Registers.Eip} bytes");
		_output.WriteLine($"  Instruction bytes length: {test.InstructionBytes.Length}");
		
		// Check if any instruction bytes overlap with initial memory
		var eip = test.InitialState.Registers.Eip;
		for (int i = 0; i < test.InstructionBytes.Length; i++)
		{
			var addr = eip + (uint)i;
			var memEntry = test.InitialState.Memory.FirstOrDefault(m => m.Address == addr);
			if (memEntry != null)
			{
				_output.WriteLine($"  WARNING: Instruction byte at 0x{addr:X8} overlaps with initial memory!");
				_output.WriteLine($"    Instruction byte: 0x{test.InstructionBytes[i]:X2}");
				_output.WriteLine($"    Memory value: 0x{memEntry.Value:X2}");
			}
		}
	}
	
	private string? FindTestFile(string fileName)
	{
		var searchPaths = new[]
		{
			Path.Combine("TestData", "SingleStepTests", fileName),
			Path.Combine("SingleStepTests", fileName),
			Path.Combine("..", "TestData", "SingleStepTests", fileName),
			fileName
		};
		
		foreach (var path in searchPaths)
		{
			if (File.Exists(path))
			{
				return path;
			}
		}
		
		return null;
	}
}
