using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

public class HighPassRateAnalyzer2
{
private readonly ITestOutputHelper _output;

public HighPassRateAnalyzer2(ITestOutputHelper output)
{
_output = output;
}

[Fact]
public void Analyze_22_MOO() // AND - 97.3% pass
{
AnalyzeFailures("22.MOO.gz", maxToAnalyze: 10);
}

private void AnalyzeFailures(string fileName, int maxToAnalyze = int.MaxValue)
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
if (failedTests.Count >= maxToAnalyze)
break;
}
}

_output.WriteLine($"Failed tests: {failedTests.Count}");
_output.WriteLine($"\nDetailed analysis:");

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
}
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
