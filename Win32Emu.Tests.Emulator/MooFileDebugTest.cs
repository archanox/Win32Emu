using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using System.Linq;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Debug test to examine the actual MOO file test data
/// </summary>
public class MooFileDebugTest
{
	private readonly ITestOutputHelper _output;
	
	public MooFileDebugTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void ExamineFirstTestCase_00MOO()
	{
		var testFile = FindTestFile("00.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found, skipping");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		Assert.NotEmpty(mooFile.Tests);
		
		// Examine first test case
		var test = mooFile.Tests[0];
		
		_output.WriteLine($"Test Name: {test.Name}");
		_output.WriteLine($"Instruction Bytes: {string.Join(" ", test.InstructionBytes.Select(b => $"{b:X2}"))}");
		_output.WriteLine($"Instruction Bytes Length: {test.InstructionBytes.Length}");
		
		_output.WriteLine($"\nInitial State:");
		_output.WriteLine($"  EIP: 0x{test.InitialState.Registers.Eip:X8}");
		_output.WriteLine($"  EFLAGS: 0x{test.InitialState.Registers.Eflags:X8}");
		_output.WriteLine($"  EAX: 0x{test.InitialState.Registers.Eax:X8}");
		_output.WriteLine($"  EBX: 0x{test.InitialState.Registers.Ebx:X8}");
		_output.WriteLine($"  Memory entries: {test.InitialState.Memory.Count}");
		
		_output.WriteLine($"\nFinal State:");
		_output.WriteLine($"  EIP: 0x{test.FinalState.Registers.Eip:X8}");
		_output.WriteLine($"  EFLAGS: 0x{test.FinalState.Registers.Eflags:X8}");
		_output.WriteLine($"  EAX: 0x{test.FinalState.Registers.Eax:X8}");
		_output.WriteLine($"  EBX: 0x{test.FinalState.Registers.Ebx:X8}");
		_output.WriteLine($"  Memory entries: {test.FinalState.Memory.Count}");
		
		_output.WriteLine($"\nExpected EIP advancement: {test.FinalState.Registers.Eip - test.InitialState.Registers.Eip} bytes");
		_output.WriteLine($"Instruction bytes provided: {test.InstructionBytes.Length} bytes");
	}
	
	[Fact]
	public void ExamineMultipleTestCases_00MOO()
	{
		var testFile = FindTestFile("00.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found, skipping");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		
		// Examine first 10 test cases
		for (int i = 0; i < Math.Min(10, mooFile.Tests.Count); i++)
		{
			var test = mooFile.Tests[i];
			var eipAdvance = test.FinalState.Registers.Eip - test.InitialState.Registers.Eip;
			var instrLen = test.InstructionBytes.Length;
			
			_output.WriteLine($"Test {i}: {test.Name}");
			_output.WriteLine($"  Bytes: {string.Join(" ", test.InstructionBytes.Select(b => $"{b:X2}"))}");
			_output.WriteLine($"  Initial EIP: 0x{test.InitialState.Registers.Eip:X8}");
			_output.WriteLine($"  Final EIP: 0x{test.FinalState.Registers.Eip:X8}");
			_output.WriteLine($"  Expected advance: {eipAdvance} bytes");
			_output.WriteLine($"  Instruction length: {instrLen} bytes");
			_output.WriteLine($"  Match: {(eipAdvance == instrLen ? "YES" : "NO")}");
			_output.WriteLine("");
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
