using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Debug specific failing tests to understand root causes
/// </summary>
public class DebugSpecificTest
{
	private readonly ITestOutputHelper _output;
	private readonly ILogger _logger;
	
	public DebugSpecificTest(ITestOutputHelper output)
	{
		_output = output;
		_logger = new XUnitLogger(output);
	}
	
	[Fact]
	public void Debug_03MOO_Test39()
	{
		// This test fails with: add ax,[ds:bx]
		// Register mismatches: ESP(expected=0x00008DF0, actual=0x00008DF6), 
		// EIP(expected=0x000060C1, actual=0x00003163), EFLAGS(expected=0xFFFC0C82, actual=0xFFFC0446)
		// Memory mismatches: 6 locations
		
		var testFile = TestFileHelper.FindTestFile("03.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found - skipping");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		var runner = new SingleStepTestRunner(_logger);
		var test = mooFile.Tests[39];
		
		_output.WriteLine($"Test 39: {test.Name}");
		_output.WriteLine($"Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
		_output.WriteLine($"\nInitial state:");
		_output.WriteLine($"  EAX=0x{test.InitialState.Registers.Eax:X8}");
		_output.WriteLine($"  EBX=0x{test.InitialState.Registers.Ebx:X8}");
		_output.WriteLine($"  ESP=0x{test.InitialState.Registers.Esp:X8}");
		_output.WriteLine($"  EIP=0x{test.InitialState.Registers.Eip:X8}");
		_output.WriteLine($"  CS=0x{test.InitialState.Registers.Cs:X4}");
		_output.WriteLine($"  DS=0x{test.InitialState.Registers.Ds:X4}");
		_output.WriteLine($"  EFLAGS=0x{test.InitialState.Registers.Eflags:X8}");
		
		// Calculate the physical address being accessed by [ds:bx]
		var ds = test.InitialState.Registers.Ds;
		var bx = test.InitialState.Registers.Ebx & 0xFFFF;
		var physicalAddr = (uint)((ds << 4) + bx);
		_output.WriteLine($"\nMemory access:");
		_output.WriteLine($"  [DS:BX] = [0x{ds:X4}:0x{bx:X4}] = physical address 0x{physicalAddr:X8}");
		
		// Check IVT entry for General Protection Fault (vector 13, interrupt 0xD)
		_output.WriteLine($"\nIVT check for #GP (vector 13):");
		var ivtEntryAddr = 13 * 4; // Each IVT entry is 4 bytes (IP:2, CS:2)
		var ivtEntry = test.InitialState.Memory.Where(m => m.Address >= ivtEntryAddr && m.Address < ivtEntryAddr + 4).ToList();
		if (ivtEntry.Any())
		{
			_output.WriteLine($"  IVT has memory at vector 13:");
			foreach (var entry in ivtEntry.OrderBy(e => e.Address))
			{
				_output.WriteLine($"    @0x{entry.Address:X8} = 0x{entry.Value:X2}");
			}
		}
		else
		{
			_output.WriteLine($"  IVT vector 13 not initialized in test data");
		}
		
		// Check expected final memory for IVT and stack changes
		_output.WriteLine($"\nExpected final memory changes: {test.FinalState.Memory.Count} locations");
		foreach (var mem in test.FinalState.Memory.Take(10))
		{
			_output.WriteLine($"  @0x{mem.Address:X8} = 0x{mem.Value:X2}");
		}
		
		var result = runner.ExecuteTest(test);
		
		_output.WriteLine($"\nResult: {(result.Success ? "PASS" : "FAIL")}");
		if (!result.Success)
		{
			if (!string.IsNullOrEmpty(result.ExecutionError))
			{
				_output.WriteLine($"Execution error: {result.ExecutionError}");
			}
			
			_output.WriteLine($"\nRegister mismatches:");
			foreach (var mismatch in result.RegisterMismatches)
			{
				_output.WriteLine($"  {mismatch}");
			}
			_output.WriteLine($"\nMemory mismatches: {result.MemoryMismatches.Count}");
			foreach (var mismatch in result.MemoryMismatches.Take(10))
			{
				_output.WriteLine($"  {mismatch}");
			}
		}
		
		_output.WriteLine($"\nExpected final state:");
		_output.WriteLine($"  EAX=0x{test.FinalState.Registers.Eax:X8}");
		_output.WriteLine($"  ESP=0x{test.FinalState.Registers.Esp:X8}");
		_output.WriteLine($"  EIP=0x{test.FinalState.Registers.Eip:X8}");
		_output.WriteLine($"  EFLAGS=0x{test.FinalState.Registers.Eflags:X8}");
	}
	
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
