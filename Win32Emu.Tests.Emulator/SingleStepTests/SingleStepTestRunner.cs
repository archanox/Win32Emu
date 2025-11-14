using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Test runner for executing SingleStepTests/80386 test cases against Win32Emu's CPU implementation.
/// Validates CPU behavior against hardware-generated test cases.
/// </summary>
public class SingleStepTestRunner
{
	private readonly ILogger _logger;
	
	public SingleStepTestRunner(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}
	
	/// <summary>
	/// Execute a single MOO test case and return the result
	/// </summary>
	public TestResult ExecuteTest(MooTestCase testCase)
	{
		var result = new TestResult
		{
			TestName = testCase.Name,
			Success = false
		};
		
		try
		{
			// Create a fresh memory instance for this test
			var memory = new VirtualMemory();
			
			// Create CPU instance for 16-bit real mode (SingleStepTests are from real 80386 hardware in real mode)
			var cpu = new IcedCpu(memory, bitness: 16);
			
			// Apply initial state
			ApplyInitialState(cpu, memory, testCase);
			
			// Execute instructions until HLT is reached
			// According to SingleStepTests/80386 documentation:
			// "Each test is actually a sequence of two instructions, the instruction under test,
			//  and a HALT opcode 0xF4. [...] End execution at the HALT instruction."
			try
			{
				const int maxInstructions = 10;  // Safety limit to prevent infinite loops
				int instructionCount = 0;
				
				while (instructionCount < maxInstructions)
				{
					var eip = cpu.GetEip();
					var opcode = memory.Read8(eip);
					
					// Execute one instruction
					cpu.SingleStep(memory);
					instructionCount++;
					
					// Check if we just executed a HLT instruction (opcode 0xF4)
					if (opcode == 0xF4)
					{
						break;
					}
				}
				
				if (instructionCount >= maxInstructions)
				{
					result.ExecutionError = $"Executed {maxInstructions} instructions without reaching HLT";
					_logger.LogWarning("Test {TestName} exceeded instruction limit without reaching HLT", testCase.Name);
					return result;
				}
			}
			catch (Exception ex)
			{
				result.ExecutionError = ex.Message;
				_logger.LogWarning(ex, "Test {TestName} failed during execution", testCase.Name);
				return result;
			}
			
			// Validate final state
			result.Success = ValidateFinalState(cpu, memory, testCase.FinalState, result);
		}
		catch (Exception ex)
		{
			result.ExecutionError = ex.Message;
			_logger.LogError(ex, "Test {TestName} failed with exception", testCase.Name);
		}
		
		return result;
	}
	
	/// <summary>
	/// Apply initial CPU and memory state from test case
	/// </summary>
	private void ApplyInitialState(IcedCpu cpu, VirtualMemory memory, MooTestCase testCase)
	{
		var initialState = testCase.InitialState;
		
		// Set registers
		var regs = initialState.Registers;
		cpu.SetRegister("EAX", regs.Eax);
		cpu.SetRegister("EBX", regs.Ebx);
		cpu.SetRegister("ECX", regs.Ecx);
		cpu.SetRegister("EDX", regs.Edx);
		cpu.SetRegister("ESI", regs.Esi);
		cpu.SetRegister("EDI", regs.Edi);
		cpu.SetRegister("EBP", regs.Ebp);
		cpu.SetRegister("ESP", regs.Esp);
		cpu.SetEip(regs.Eip);
		cpu.SetRegister("EFLAGS", regs.Eflags);
		
		// Set segment registers
		cpu.SetRegister("CS", regs.Cs);
		cpu.SetRegister("DS", regs.Ds);
		cpu.SetRegister("ES", regs.Es);
		cpu.SetRegister("FS", regs.Fs);
		cpu.SetRegister("GS", regs.Gs);
		cpu.SetRegister("SS", regs.Ss);
		
		// Write instruction bytes to memory at EIP
		// This is critical - the CPU needs to read the instruction from memory!
		for (var i = 0; i < testCase.InstructionBytes.Length; i++)
		{
			var address = regs.Eip + (uint)i;
			if (address < regs.Eip)
			{
				_logger.LogWarning("Instruction bytes extend beyond 32-bit address space at EIP={EIP:X8} in test {TestName}", regs.Eip, testCase.Name);
				break;
			}
			memory.Write8(address, testCase.InstructionBytes[i]);
		}
		
		// Write initial memory state
		foreach (var memEntry in initialState.Memory)
		{
			memory.Write8(memEntry.Address, memEntry.Value);
		}
	}
	
	/// <summary>
	/// Validate CPU and memory state against expected final state
	/// </summary>
	private bool ValidateFinalState(IcedCpu cpu, VirtualMemory memory, CpuTestState expectedState, TestResult result)
	{
		var isValid = true;
		var regs = expectedState.Registers;
		
		// Validate registers
		isValid &= ValidateRegister(cpu, "EAX", regs.Eax, result);
		isValid &= ValidateRegister(cpu, "EBX", regs.Ebx, result);
		isValid &= ValidateRegister(cpu, "ECX", regs.Ecx, result);
		isValid &= ValidateRegister(cpu, "EDX", regs.Edx, result);
		isValid &= ValidateRegister(cpu, "ESI", regs.Esi, result);
		isValid &= ValidateRegister(cpu, "EDI", regs.Edi, result);
		isValid &= ValidateRegister(cpu, "EBP", regs.Ebp, result);
		isValid &= ValidateRegister(cpu, "ESP", regs.Esp, result);
		isValid &= ValidateRegister(cpu, "EIP", cpu.GetEip(), regs.Eip, result);
		isValid &= ValidateRegister(cpu, "EFLAGS", cpu.GetRegister("EFLAGS"), regs.Eflags, result);
		
		// Validate memory
		foreach (var memEntry in expectedState.Memory)
		{
			var actualValue = memory.Read8(memEntry.Address);
			if (actualValue != memEntry.Value)
			{
				result.MemoryMismatches.Add(new MemoryMismatch
				{
					Address = memEntry.Address,
					Expected = memEntry.Value,
					Actual = actualValue
				});
				isValid = false;
			}
		}
		
		return isValid;
	}
	
	private bool ValidateRegister(IcedCpu cpu, string name, uint expected, TestResult result)
	{
		var actual = cpu.GetRegister(name);
		return ValidateRegister(cpu, name, actual, expected, result);
	}
	
	private bool ValidateRegister(IcedCpu cpu, string name, uint actual, uint expected, TestResult result)
	{
		if (actual != expected)
		{
			result.RegisterMismatches.Add(new RegisterMismatch
			{
				RegisterName = name,
				Expected = expected,
				Actual = actual
			});
			return false;
		}
		return true;
	}
}

/// <summary>
/// Result of executing a single test case
/// </summary>
public class TestResult
{
	public string TestName { get; set; } = string.Empty;
	public bool Success { get; set; }
	public string? ExecutionError { get; set; }
	public List<RegisterMismatch> RegisterMismatches { get; set; } = new();
	public List<MemoryMismatch> MemoryMismatches { get; set; } = new();
	
	public override string ToString()
	{
		if (Success)
		{
			return $"PASS: {TestName}";
		}
		
		var details = new List<string>();
		
		if (!string.IsNullOrEmpty(ExecutionError))
		{
			details.Add($"Execution error: {ExecutionError}");
		}
		
		if (RegisterMismatches.Any())
		{
			details.Add($"Register mismatches: {string.Join(", ", RegisterMismatches.Select(r => r.ToString()))}");
		}
		
		if (MemoryMismatches.Any())
		{
			details.Add($"Memory mismatches: {MemoryMismatches.Count} locations");
		}
		
		return $"FAIL: {TestName} - {string.Join("; ", details)}";
	}
}

/// <summary>
/// Represents a mismatch in register value
/// </summary>
public class RegisterMismatch
{
	public string RegisterName { get; set; } = string.Empty;
	public uint Expected { get; set; }
	public uint Actual { get; set; }
	
	public override string ToString()
	{
		return $"{RegisterName}(expected=0x{Expected:X8}, actual=0x{Actual:X8})";
	}
}

/// <summary>
/// Represents a mismatch in memory value
/// </summary>
public class MemoryMismatch
{
	public uint Address { get; set; }
	public byte Expected { get; set; }
	public byte Actual { get; set; }
	
	public override string ToString()
	{
		return $"@0x{Address:X8}(expected=0x{Expected:X2}, actual=0x{Actual:X2})";
	}
}
