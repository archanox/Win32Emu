using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

public class AddInstructionDebugTest
{
private readonly ITestOutputHelper _output;

public AddInstructionDebugTest(ITestOutputHelper output)
{
_output = output;
}

[Fact]
public void TestAdd_FromSingleStepTest39()
{
// Reproduce test 39 from 03.MOO.gz
var testFile = FindTestFile("03.MOO.gz");
if (testFile == null)
{
_output.WriteLine("Skipping: Test file not found");
return;
}

var mooFile = MooFileParser.Parse(testFile);
var test = mooFile.Tests[39]; // Test 39: add ax,[ds:bx]

_output.WriteLine($"Test: {test.Name}");
_output.WriteLine($"Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");

// Show initial state
_output.WriteLine($"\nInitial state:");
_output.WriteLine($"  EAX=0x{test.InitialState.Registers.Eax:X8}");
_output.WriteLine($"  EBX=0x{test.InitialState.Registers.Ebx:X8}");
_output.WriteLine($"  ESP=0x{test.InitialState.Registers.Esp:X8}");
_output.WriteLine($"  EIP=0x{test.InitialState.Registers.Eip:X8}");
_output.WriteLine($"  CS=0x{test.InitialState.Registers.Cs:X4}");
_output.WriteLine($"  DS=0x{test.InitialState.Registers.Ds:X4}");
_output.WriteLine($"  EFLAGS=0x{test.InitialState.Registers.Eflags:X8}");

// Show expected final state
_output.WriteLine($"\nExpected final state:");
_output.WriteLine($"  EAX=0x{test.FinalState.Registers.Eax:X8}");
_output.WriteLine($"  ESP=0x{test.FinalState.Registers.Esp:X8}");
_output.WriteLine($"  EIP=0x{test.FinalState.Registers.Eip:X8}");
_output.WriteLine($"  EFLAGS=0x{test.FinalState.Registers.Eflags:X8}");

// Run the test
var runner = new SingleStepTestRunner(NullLogger.Instance);
var result = runner.ExecuteTest(test);

_output.WriteLine($"\nTest result: {(result.Success ? "PASS" : "FAIL")}");
if (!result.Success)
{
_output.WriteLine($"Execution error: {result.ExecutionError}");
foreach (var mismatch in result.RegisterMismatches)
{
_output.WriteLine($"  {mismatch}");
}
foreach (var mismatch in result.MemoryMismatches.Take(10))
{
_output.WriteLine($"  {mismatch}");
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
