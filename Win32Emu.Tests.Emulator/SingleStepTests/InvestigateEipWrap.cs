using Xunit;
using Xunit.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Investigate EIP wrapping issue in test 6766A3.MOO.gz
/// </summary>
public class InvestigateEipWrap
{
	private readonly ITestOutputHelper _output;

	public InvestigateEipWrap(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void AnalyzeTest642_EipWrapping()
	{
		var testFile = TestFileHelper.FindTestFile("6766A3.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found");
			return;
		}

		var mooFile = MooFileParser.Parse(testFile);
		var test = mooFile.Tests[642];

		_output.WriteLine($"Test 642: {test.Name}");
		_output.WriteLine($"Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
		_output.WriteLine($"Instruction length: {test.InstructionBytes.Length} bytes");
		
		var initialRegs = test.InitialState.Registers;
		var finalRegs = test.FinalState.Registers;
		
		_output.WriteLine($"\nInitial state:");
		_output.WriteLine($"  CS=0x{initialRegs.Cs:X4}, EIP=0x{initialRegs.Eip:X8}");
		_output.WriteLine($"  Physical address: 0x{(initialRegs.Cs << 4) + initialRegs.Eip:X8}");
		
		_output.WriteLine($"\nExpected final state:");
		_output.WriteLine($"  CS=0x{finalRegs.Cs:X4}, EIP=0x{finalRegs.Eip:X8}");
		_output.WriteLine($"  Physical address: 0x{(finalRegs.Cs << 4) + finalRegs.Eip:X8}");
		
		_output.WriteLine($"\nEIP calculation:");
		_output.WriteLine($"  Initial EIP: 0x{initialRegs.Eip:X8}");
		_output.WriteLine($"  Instruction length: {test.InstructionBytes.Length}");
		_output.WriteLine($"  Expected EIP: 0x{initialRegs.Eip + test.InstructionBytes.Length:X8}");
		_output.WriteLine($"  Actual expected EIP: 0x{finalRegs.Eip:X8}");
		
		// The issue: In 16-bit mode, when EIP reaches 0x10000, it should wrap to 0x0000
		// But test expects 0x00010000 (unwrapped 32-bit value)
		_output.WriteLine($"\nAnalysis:");
		if (initialRegs.Eip + test.InstructionBytes.Length > 0xFFFF)
		{
			_output.WriteLine($"  EIP will exceed 0xFFFF after instruction!");
			_output.WriteLine($"  CPU stores full 32-bit value: 0x{initialRegs.Eip + test.InstructionBytes.Length:X8}");
			_output.WriteLine($"  Test expects: 0x{finalRegs.Eip:X8}");
			_output.WriteLine($"  This matches real 386 hardware behavior (no wrapping when storing EIP)");
		}

		// Now execute the test
		var runner = new SingleStepTestRunner();
		var result = runner.ExecuteTest(test);
		
		_output.WriteLine($"\nExecution result:");
		_output.WriteLine($"  Success: {result.Success}");
		if (!result.Success)
		{
			_output.WriteLine($"  Error: {result}");
		}
	}
	
	[Fact]
	public void AnalyzeAllEipWrapFailures()
	{
		var testFile = TestFileHelper.FindTestFile("6766A3.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found");
			return;
		}

		var mooFile = MooFileParser.Parse(testFile);
		var runner = new SingleStepTestRunner();
		
		var eipFailures = new List<(int index, MooTestCase test, TestResult result)>();
		
		for (var i = 0; i < mooFile.Tests.Count; i++)
		{
			var test = mooFile.Tests[i];
			var result = runner.ExecuteTest(test);
			
			if (!result.Success && result.RegisterMismatches.Count == 1 && 
			    result.RegisterMismatches[0].RegisterName == "EIP")
			{
				eipFailures.Add((i, test, result));
			}
		}
		
		_output.WriteLine($"Found {eipFailures.Count} tests with EIP-only failures:");
		foreach (var (index, test, result) in eipFailures)
		{
			var initialRegs = test.InitialState.Registers;
			var eipMismatch = result.RegisterMismatches[0];
			
			_output.WriteLine($"\nTest {index}: {test.Name}");
			_output.WriteLine($"  Initial EIP: 0x{initialRegs.Eip:X8}");
			_output.WriteLine($"  Instruction length: {test.InstructionBytes.Length}");
			_output.WriteLine($"  Expected EIP: 0x{eipMismatch.Expected:X8}");
			_output.WriteLine($"  Actual EIP: 0x{eipMismatch.Actual:X8}");
			_output.WriteLine($"  Will exceed 0xFFFF: {initialRegs.Eip + test.InstructionBytes.Length > 0xFFFF}");
		}
	}
}
