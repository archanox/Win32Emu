using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Debug SHRD instruction failures
/// </summary>
public class DebugShrdTest
{
	private readonly ITestOutputHelper _output;
	private readonly ILogger _logger;
	
	public DebugShrdTest(ITestOutputHelper output)
	{
		_output = output;
		_logger = new XUnitLogger(output);
	}
	
	[Fact]
	public void Debug_SHRD_Test0()
	{
		// First failing test from 660FAC.MOO.gz
		var testFile = TestFileHelper.FindTestFile("660FAC.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found - skipping");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		var runner = new SingleStepTestRunner(_logger);
		
		// Run first 10 tests to understand the pattern
		for (var i = 0; i < 10; i++)
		{
			var test = mooFile.Tests[i];
			_output.WriteLine($"\n=== Test {i}: {test.Name} ===");
			_output.WriteLine($"Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
			_output.WriteLine($"\nInitial state:");
			_output.WriteLine($"  EDI=0x{test.InitialState.Registers.Edi:X8}");
			_output.WriteLine($"  ECX=0x{test.InitialState.Registers.Ecx:X8}");
			_output.WriteLine($"  EBX=0x{test.InitialState.Registers.Ebx:X8}");
			_output.WriteLine($"  EFLAGS=0x{test.InitialState.Registers.Eflags:X8}");
			
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
				
				if (result.MemoryMismatches.Count > 0)
				{
					_output.WriteLine($"\nMemory mismatches: {result.MemoryMismatches.Count}");
					foreach (var mismatch in result.MemoryMismatches.Take(5))
					{
						_output.WriteLine($"  {mismatch}");
					}
				}
			}
			
			_output.WriteLine($"\nExpected final state:");
			_output.WriteLine($"  EDI=0x{test.FinalState.Registers.Edi:X8}");
			_output.WriteLine($"  EFLAGS=0x{test.FinalState.Registers.Eflags:X8}");
		}
	}
	
	[Fact]
	public void Debug_SHRD_ManualTest()
	{
		// Manually test SHRD to understand the operation
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, bitness: 16);
		
		// Test case: shrd edi, ecx, 0x81
		// Initial: EDI=?, ECX=?, EFLAGS=?
		
		_output.WriteLine("Manual SHRD test");
		_output.WriteLine("Testing: shrd edi, ecx, 0x81");
		
		// Set up initial state
		cpu.SetRegister("EDI", 0x12345678);
		cpu.SetRegister("ECX", 0xABCDEF01);
		cpu.SetRegister("EFLAGS", 0x00000202);
		
		_output.WriteLine($"\nBefore:");
		_output.WriteLine($"  EDI=0x{cpu.GetRegister("EDI"):X8}");
		_output.WriteLine($"  ECX=0x{cpu.GetRegister("ECX"):X8}");
		_output.WriteLine($"  EFLAGS=0x{cpu.GetRegister("EFLAGS"):X8}");
		
		// Write SHRD instruction: 66 0F AC CF 81 (shrd edi, ecx, 0x81)
		// Note: This is in 16-bit mode but with 32-bit operand override (66)
		uint instructionAddr = 0x1000;
		memory.Write8(instructionAddr + 0, 0x66); // Operand size override
		memory.Write8(instructionAddr + 1, 0x0F);
		memory.Write8(instructionAddr + 2, 0xAC);
		memory.Write8(instructionAddr + 3, 0xCF); // ModRM: 11 001 111 = register, ECX, EDI
		memory.Write8(instructionAddr + 4, 0x81); // Immediate count
		memory.Write8(instructionAddr + 5, 0xF4); // HLT
		
		cpu.SetEip(instructionAddr);
		cpu.SetRegister("CS", 0);
		
		// Execute the instruction
		cpu.SingleStep(memory);
		
		_output.WriteLine($"\nAfter:");
		_output.WriteLine($"  EDI=0x{cpu.GetRegister("EDI"):X8}");
		_output.WriteLine($"  ECX=0x{cpu.GetRegister("ECX"):X8}");
		_output.WriteLine($"  EFLAGS=0x{cpu.GetRegister("EFLAGS"):X8}");
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
