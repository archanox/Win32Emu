using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// CPU conformance tests using the SingleStepTests/80386 test suite.
/// These are hardware-generated tests that validate CPU implementation against real 386 behavior.
/// 
/// NOTE: These tests require test files from https://github.com/SingleStepTests/80386
/// Place test files in the TestData/SingleStepTests directory to run these tests.
/// </summary>
public class SingleStepConformanceTests
{
	private readonly ITestOutputHelper _output;
	private readonly ILogger _logger;
	
	public SingleStepConformanceTests(ITestOutputHelper output)
	{
		_output = output;
		_logger = new XUnitLogger(output);
	}
	
	[Fact]
	public void Parser_ShouldLoadMooFile()
	{
		// This is a basic test to verify the parser works
		// Skip if test files are not available
		var testFile = FindTestFile("00.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Skipping: Test files not found. Download from https://github.com/SingleStepTests/80386");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		
		Assert.NotNull(mooFile);
		Assert.NotEmpty(mooFile.Tests);
		_output.WriteLine($"Loaded {mooFile.Tests.Count} tests from {Path.GetFileName(testFile)}");
	}
	
	/// <summary>
	/// Dynamically discovers all MOO test files in the TestData directory.
	/// Each test file becomes a test case with configurable number of tests to run.
	/// </summary>
	public static IEnumerable<object[]> GetTestFiles()
	{
		var testDataPaths = new[]
		{
			Path.Combine("TestData", "SingleStepTests"),
			Path.Combine("SingleStepTests"),
			Path.Combine("..", "TestData", "SingleStepTests")
		};
		
		foreach (var basePath in testDataPaths)
		{
			if (Directory.Exists(basePath))
			{
				var testFiles = Directory.GetFiles(basePath, "*.MOO.gz")
					.OrderBy(f => f)
					.Select(f => Path.GetFileName(f))
					.ToList();
				
				if (testFiles.Any())
				{
					// Return each test file with maxTests = int.MaxValue (no limit, run all available tests)
					foreach (var fileName in testFiles)
					{
						yield return new object[] { fileName, int.MaxValue };
					}
					yield break;
				}
			}
		}
		
		// If no test files found, return empty to allow graceful skipping
		yield break;
	}
	
	// Cache parsed MOO files to avoid re-parsing for each test run
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, MooTestFile> _mooFileCache = new();
	
	[Theory]
	[MemberData(nameof(GetTestFiles))]
	public void CPU_ShouldPassHardwareTests(string fileName, int maxTests)
	{
		// Skip if test files are not available
		var testFile = FindTestFile(fileName);
		if (testFile == null)
		{
			_output.WriteLine($"Skipping: Test file {fileName} not found");
			return;
		}
		
		// Use cached parsed data to avoid re-parsing
		var mooFile = _mooFileCache.GetOrAdd(testFile, MooFileParser.Parse);
		var runner = new SingleStepTestRunner(_logger);
		
		var passCount = 0;
		var failCount = 0;
		var testCount = Math.Min(maxTests, mooFile.Tests.Count);
		
		// Track failure reasons for better diagnostics
		var failuresByReason = new Dictionary<string, int>();
		
		for (var i = 0; i < testCount; i++)
		{
			var test = mooFile.Tests[i];
			var result = runner.ExecuteTest(test);
			
			if (result.Success)
			{
				passCount++;
			}
			else
			{
				failCount++;
				
				// Categorize the failure
				var failureReason = CategorizeFailure(result);
				if (!failuresByReason.ContainsKey(failureReason))
					failuresByReason[failureReason] = 0;
				failuresByReason[failureReason]++;
				
				// Only output first 5 failures to avoid overwhelming output
				if (failCount <= 5)
				{
					_output.WriteLine($"Test {i}: {result}");
				}
			}
		}
		
		_output.WriteLine($"\n========================================");
		_output.WriteLine($"Results for {fileName}:");
		_output.WriteLine($"  Total tests: {testCount} (out of {mooFile.Tests.Count} available)");
		_output.WriteLine($"  Passed: {passCount} ({100.0 * passCount / testCount:F1}%)");
		_output.WriteLine($"  Failed: {failCount} ({100.0 * failCount / testCount:F1}%)");
		
		if (failuresByReason.Any())
		{
			_output.WriteLine($"\nFailure breakdown:");
			foreach (var kvp in failuresByReason.OrderByDescending(x => x.Value))
			{
				_output.WriteLine($"  {kvp.Key}: {kvp.Value}");
			}
		}
		
		_output.WriteLine($"========================================\n");
		
		// Fail the test if there are any failures
		if (failCount > 0)
		{
			Assert.Fail($"{failCount} out of {testCount} tests failed for {fileName}. See detailed output above for failure reasons.");
		}
	}
	
	/// <summary>
	/// Categorize test failure for better diagnostics
	/// </summary>
	private string CategorizeFailure(TestResult result)
	{
		if (!string.IsNullOrEmpty(result.ExecutionError))
		{
			return "Execution Error";
		}
		
		// Check what's wrong (handle lazy-initialized lists)
		var registerMismatches = result.RegisterMismatches;
		var memoryMismatches = result.MemoryMismatches;
		
		var hasEipMismatch = registerMismatches.Any(r => r.RegisterName == "EIP");
		var hasFlagsMismatch = registerMismatches.Any(r => r.RegisterName == "EFLAGS");
		var hasOtherRegMismatch = registerMismatches.Any(r => r.RegisterName != "EIP" && r.RegisterName != "EFLAGS");
		var hasMemoryMismatch = memoryMismatches.Any();
		
		if (hasEipMismatch && !hasFlagsMismatch && !hasOtherRegMismatch && !hasMemoryMismatch)
		{
			return "EIP only (instruction length issue)";
		}
		
		if (hasFlagsMismatch && !hasEipMismatch && !hasOtherRegMismatch && !hasMemoryMismatch)
		{
			return "EFLAGS only (flag calculation issue)";
		}
		
		if (hasEipMismatch && hasFlagsMismatch && !hasOtherRegMismatch && !hasMemoryMismatch)
		{
			return "EIP + EFLAGS";
		}
		
		if (hasOtherRegMismatch)
		{
			return "Register value error";
		}
		
		if (hasMemoryMismatch)
		{
			return "Memory error";
		}
		
		return "Multiple issues";
	}
	
	/// <summary>
	/// Find a test file in the TestData directory
	/// </summary>
	private string? FindTestFile(string fileName)
	{
		// Look for test files in several possible locations
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
	
	/// <summary>
	/// Simple logger adapter for xUnit ITestOutputHelper
	/// </summary>
	private class XUnitLogger : ILogger
	{
		private readonly ITestOutputHelper _output;
		
		public XUnitLogger(ITestOutputHelper output)
		{
			_output = output;
		}
		
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		
		public bool IsEnabled(LogLevel logLevel) => true;
		
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			_output.WriteLine($"[{logLevel}] {formatter(state, exception)}");
			if (exception != null)
			{
				_output.WriteLine(exception.ToString());
			}
		}
	}
}
