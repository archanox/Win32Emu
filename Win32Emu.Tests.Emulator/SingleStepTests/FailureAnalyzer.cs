using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Analyzes failing SingleStep tests to identify patterns and root causes
/// </summary>
public class FailureAnalyzer
{
	private readonly ITestOutputHelper _output;
	private readonly ILogger _logger;
	
	public FailureAnalyzer(ITestOutputHelper output)
	{
		_output = output;
		_logger = new XUnitLogger(output);
	}
	
	[Fact]
	public void AnalyzeFailingTests_03MOO()
	{
		AnalyzeTestFile("03.MOO.gz", maxTests: 100);
	}
	
	[Fact]
	public void AnalyzeFailingTests_6766A1MOO()
	{
		AnalyzeTestFile("6766A1.MOO.gz", maxTests: 100);
	}
	
	[Fact]
	public void AnalyzeFailingTests_D5MOO()
	{
		// D5.MOO.gz contains AAD instruction tests
		// According to investigation, this has 15.3% pass rate with pure EFLAGS issues
		AnalyzeTestFile("D5.MOO.gz", maxTests: 20);
	}
	
	[Fact]
	public void AnalyzeAADFlagPattern()
	{
		// Detailed analysis of AAD flag behavior
		var testFile = TestFileHelper.FindTestFile("D5.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("D5.MOO.gz not found");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		_output.WriteLine($"Analyzing AAD flag patterns from {mooFile.Tests.Count} tests\n");
		
		for (int i = 0; i < Math.Min(20, mooFile.Tests.Count); i++)
		{
			var test = mooFile.Tests[i];
			var initial = test.InitialState.Registers;
			var final = test.FinalState.Registers;
			
			var al_init = (byte)(initial.Eax & 0xFF);
			var ah_init = (byte)((initial.Eax >> 8) & 0xFF);
			var al_final = (byte)(final.Eax & 0xFF);
			var ah_final = (byte)((final.Eax >> 8) & 0xFF);
			
			// Get the base from instruction
			var base_ = test.InstructionBytes.Length > 1 ? test.InstructionBytes[1] : (byte)10;
			
			// Calculate what the result should be
			var temp = ah_init * base_ + al_init;
			var expected_al = (byte)(temp & 0xFF);
			
			// Extract flags
			var finalFlags = final.Eflags;
			var cf = (finalFlags & 0x1) != 0;
			var pf = (finalFlags & 0x4) != 0;
			var af = (finalFlags & 0x10) != 0;
			var zf = (finalFlags & 0x40) != 0;
			var sf = (finalFlags & 0x80) != 0;
			var of = (finalFlags & 0x800) != 0;
			
			_output.WriteLine($"Test {i}: base=0x{base_:X2}");
			_output.WriteLine($"  Calculation: AH=0x{ah_init:X2} * 0x{base_:X2} + AL=0x{al_init:X2} = {temp} (0x{temp:X4})");
			_output.WriteLine($"  Result: AL=0x{al_final:X2} (expected 0x{expected_al:X2}), AH=0x{ah_final:X2}");
			_output.WriteLine($"  Overflow: temp > 255? {temp > 255}");
			_output.WriteLine($"  Flags: CF={cf}, OF={of}, SF={sf}, ZF={zf}, PF={pf}, AF={af}");
			_output.WriteLine("");
		}
	}
	
	[Fact]
	public void AnalyzeFailingTests_SHRD()
	{
		// 660FAC.MOO.gz contains SHRD instruction tests
		// According to investigation, this has 30.7% pass rate with EFLAGS issues
		AnalyzeTestFile("660FAC.MOO.gz", maxTests: 20);
	}
	
	private void AnalyzeTestFile(string fileName, int maxTests)
	{
		var testFile = TestFileHelper.FindTestFile(fileName);
		if (testFile == null)
		{
			_output.WriteLine($"Skipping: Test file {fileName} not found");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		var runner = new SingleStepTestRunner(_logger);
		
		var failures = new List<(int index, MooTestCase test, TestResult result)>();
		
		for (var i = 0; i < Math.Min(maxTests, mooFile.Tests.Count); i++)
		{
			var test = mooFile.Tests[i];
			var result = runner.ExecuteTest(test);
			
			if (!result.Success)
			{
				failures.Add((i, test, result));
			}
		}
		
		_output.WriteLine($"\n========================================");
		_output.WriteLine($"Failure Analysis for {fileName}");
		_output.WriteLine($"========================================");
		_output.WriteLine($"Total failures: {failures.Count} out of {Math.Min(maxTests, mooFile.Tests.Count)}");
		
		if (failures.Any())
		{
			_output.WriteLine($"\nDetailed failure analysis:");
			
			foreach (var (index, test, result) in failures.Take(10))
			{
				_output.WriteLine($"\n--- Test {index}: {test.Name} ---");
				_output.WriteLine($"Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
				
				if (!string.IsNullOrEmpty(result.ExecutionError))
				{
					_output.WriteLine($"Execution error: {result.ExecutionError}");
				}
				
				if (result.RegisterMismatches.Any())
				{
					_output.WriteLine($"Register mismatches:");
					foreach (var mismatch in result.RegisterMismatches)
					{
						_output.WriteLine($"  {mismatch}");
						
						// For EFLAGS, decode which flags are wrong
						if (mismatch.RegisterName == "EFLAGS")
						{
							var expectedFlags = mismatch.Expected;
							var actualFlags = mismatch.Actual;
							var diff = expectedFlags ^ actualFlags;
							
							_output.WriteLine($"    Flag differences (bits that differ): 0x{diff:X8}");
							if ((diff & 0x0001) != 0) _output.WriteLine($"      CF (Carry): expected={((expectedFlags & 0x0001) != 0)}, actual={((actualFlags & 0x0001) != 0)}");
							if ((diff & 0x0004) != 0) _output.WriteLine($"      PF (Parity): expected={((expectedFlags & 0x0004) != 0)}, actual={((actualFlags & 0x0004) != 0)}");
							if ((diff & 0x0010) != 0) _output.WriteLine($"      AF (Adjust): expected={((expectedFlags & 0x0010) != 0)}, actual={((actualFlags & 0x0010) != 0)}");
							if ((diff & 0x0040) != 0) _output.WriteLine($"      ZF (Zero): expected={((expectedFlags & 0x0040) != 0)}, actual={((actualFlags & 0x0040) != 0)}");
							if ((diff & 0x0080) != 0) _output.WriteLine($"      SF (Sign): expected={((expectedFlags & 0x0080) != 0)}, actual={((actualFlags & 0x0080) != 0)}");
							if ((diff & 0x0800) != 0) _output.WriteLine($"      OF (Overflow): expected={((expectedFlags & 0x0800) != 0)}, actual={((actualFlags & 0x0800) != 0)}");
						}
					}
				}
				
				if (result.MemoryMismatches.Any())
				{
					_output.WriteLine($"Memory mismatches: {result.MemoryMismatches.Count} locations");
					foreach (var mismatch in result.MemoryMismatches.Take(5))
					{
						_output.WriteLine($"  {mismatch}");
					}
				}
			}
		}
	}
	
	private class XUnitLogger : ILogger
	{
		private readonly ITestOutputHelper _output;
		
		public XUnitLogger(ITestOutputHelper output)
		{
			_output = output;
		}
		
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		
		public bool IsEnabled(LogLevel logLevel) => false; // Disable logging for cleaner output
		
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			// Disabled for analysis
		}
	}
}
