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
public void Analyze_82_0_MOO() // XOR instruction - 100% pass rate now
{
AnalyzeFailures("82.0.MOO.gz");
}

[Fact]
public void Analyze_22_MOO() // 100% pass rate now
{
AnalyzeFailures("22.MOO.gz");
}

[Fact]
public void Analyze_A3_MOO() // MOVS - 99.9% pass rate (1 failure)
{
AnalyzeFailures("A3.MOO.gz");
}

[Fact]
public void Analyze_6766A3_MOO() // MOVS with prefix - 99.9% pass rate (3 failures)
{
AnalyzeFailures("6766A3.MOO.gz");
}

[Fact]
public void Analyze_13_MOO() // ADC - 99.7% pass rate (7 failures)
{
AnalyzeFailures("13.MOO.gz");
}

[Fact]
public void Analyze_1B_MOO() // SBB - 99.7% pass rate (8 failures)
{
AnalyzeFailures("1B.MOO.gz");
}

[Fact]
public void Analyze_09_MOO() // OR - 99.6% pass rate (9 failures)
{
AnalyzeFailures("09.MOO.gz");
}

[Fact]
public void Analyze_2B_MOO() // SUB - 99.6% pass rate (9 failures)
{
AnalyzeFailures("2B.MOO.gz");
}

[Fact]
public void Analyze_29_MOO() // SUB - 99.6% pass rate (9 failures)
{
AnalyzeFailures("29.MOO.gz");
}

[Fact]
public void Analyze_33_MOO() // XOR - 99.6% pass rate (9 failures)
{
AnalyzeFailures("33.MOO.gz");
}

[Fact]
public void Analyze_03_MOO() // ADD - 99.6% pass rate (9 failures)
{
AnalyzeFailures("03.MOO.gz");
}

private void AnalyzeFailures(string fileName)
{
var testFile = TestFileHelper.FindTestFile(fileName);
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
}
