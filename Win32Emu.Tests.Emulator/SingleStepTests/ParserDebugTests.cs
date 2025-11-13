using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

public class ParserDebugTests
{
	private readonly ITestOutputHelper _output;
	
	public ParserDebugTests(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void Parser_ShouldReadCorrectFinalState()
	{
		var testFile = FindTestFile("00.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Skipping: Test files not found");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		
		Assert.NotNull(mooFile);
		Assert.NotEmpty(mooFile.Tests);
		
		// Test first 3 tests
		for (int i = 0; i < Math.Min(3, mooFile.Tests.Count); i++)
		{
			var test = mooFile.Tests[i];
			_output.WriteLine($"\nTest {i}: {test.Name}");
			_output.WriteLine($"  Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
			
			_output.WriteLine($"  Initial state:");
			_output.WriteLine($"    EAX: 0x{test.InitialState.Registers.Eax:X8}");
			_output.WriteLine($"    EBX: 0x{test.InitialState.Registers.Ebx:X8}");
			_output.WriteLine($"    ECX: 0x{test.InitialState.Registers.Ecx:X8}");
			_output.WriteLine($"    EDX: 0x{test.InitialState.Registers.Edx:X8}");
			_output.WriteLine($"    EIP: 0x{test.InitialState.Registers.Eip:X8}");
			_output.WriteLine($"    EFLAGS: 0x{test.InitialState.Registers.Eflags:X8}");
			_output.WriteLine($"    Memory entries: {test.InitialState.Memory.Count}");
			
			_output.WriteLine($"  Final state:");
			_output.WriteLine($"    EAX: 0x{test.FinalState.Registers.Eax:X8}");
			_output.WriteLine($"    EBX: 0x{test.FinalState.Registers.Ebx:X8}");
			_output.WriteLine($"    ECX: 0x{test.FinalState.Registers.Ecx:X8}");
			_output.WriteLine($"    EDX: 0x{test.FinalState.Registers.Edx:X8}");
			_output.WriteLine($"    EIP: 0x{test.FinalState.Registers.Eip:X8}");
			_output.WriteLine($"    EFLAGS: 0x{test.FinalState.Registers.Eflags:X8}");
			_output.WriteLine($"    Memory entries: {test.FinalState.Memory.Count}");
		}
		
		// The final state should not be all zeros
		var test0 = mooFile.Tests[0];
		Assert.NotEqual(0u, test0.FinalState.Registers.Eip);
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
