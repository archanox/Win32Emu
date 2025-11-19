using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

public class HighPassRateAnalyzer
{
private readonly ITestOutputHelper _output;

public HighPassRateAnalyzer(ITestOutputHelper output)
{
_output = output;
}

[Fact]
public void Analyze_82_0_MOO() // XOR instruction - 99.6% pass rate
{
AnalyzeFailures("82.0.MOO.gz");
}

[Fact]
public void Analyze_22_MOO() // 97.3% pass rate
{
AnalyzeFailures("22.MOO.gz");
}

private void AnalyzeFailures(string fileName)
{
var testFile = FindTestFile(fileName);
if (testFile == null)
{
_output.WriteLine($"Skipping: {fileName} not found");
return;
}

var mooFile = MooFileParser.Parse(testFile);
var runner = new SingleStepTestRunner(NullLogger.Instance);

_output.WriteLine($"\n=== Analyzing failures in {fileName} ===");
_output.WriteLine($"Total tests in file: {mooFile.Tests.Count}");

var failedTests = new List<(int index, MooTestCase test, TestResult result)>();

for (var i = 0; i < mooFile.Tests.Count; i++)
{
var test = mooFile.Tests[i];
var result = runner.ExecuteTest(test);

if (!result.Success)
{
failedTests.Add((i, test, result));
}
}

_output.WriteLine($"Failed tests: {failedTests.Count}");
_output.WriteLine($"\nDetailed analysis of all failures:");

foreach (var (index, test, result) in failedTests)
{
_output.WriteLine($"\n--- Test {index}: {test.Name} ---");
_output.WriteLine($"Bytes: {BitConverter.ToString(test.InstructionBytes)}");

if (!string.IsNullOrEmpty(result.ExecutionError))
{
_output.WriteLine($"Error: {result.ExecutionError}");
}

if (result.RegisterMismatches.Any())
{
_output.WriteLine("Register mismatches:");
foreach (var m in result.RegisterMismatches)
{
_output.WriteLine($"  {m}");
if (m.RegisterName == "EFLAGS")
{
var diff = m.Expected ^ m.Actual;
_output.WriteLine($"    Diff: 0x{diff:X8}");
if ((diff & 0x0001) != 0) _output.WriteLine($"      CF");
if ((diff & 0x0004) != 0) _output.WriteLine($"      PF");
if ((diff & 0x0010) != 0) _output.WriteLine($"      AF");
if ((diff & 0x0040) != 0) _output.WriteLine($"      ZF");
if ((diff & 0x0080) != 0) _output.WriteLine($"      SF");
if ((diff & 0x0800) != 0) _output.WriteLine($"      OF");
}
}
}

if (result.MemoryMismatches.Any())
{
_output.WriteLine($"Memory mismatches: {result.MemoryMismatches.Count}");
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

	[Fact]
	public void Analyze_22_MOO_Detailed() // AND instruction - 97.3% pass rate
	{
		AnalyzeFailures("22.MOO.gz");
	}
}
