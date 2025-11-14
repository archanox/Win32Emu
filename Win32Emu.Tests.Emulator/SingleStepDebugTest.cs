using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using Win32Emu.Memory;
using Win32Emu.Cpu.Iced;
using System.Linq;

namespace Win32Emu.Tests.Emulator;

public class SingleStepDebugTest
{
	private readonly ITestOutputHelper _output;
	
	public SingleStepDebugTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void ExecuteFirstTest_WithDetailedLogging()
	{
		var testFile = FindTestFile("00.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found, skipping");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		var test = mooFile.Tests[0];
		
		_output.WriteLine($"Test: {test.Name}");
		_output.WriteLine($"Instruction bytes: {string.Join(" ", test.InstructionBytes.Select(b => $"{b:X2}"))}");
		_output.WriteLine("");
		
		// Create memory and CPU
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, bitness: 16);
		
		// Set initial state
		var regs = test.InitialState.Registers;
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
		
		_output.WriteLine($"Initial EIP: 0x{regs.Eip:X8}");
		
		// Write instruction bytes to memory
		for (var i = 0; i < test.InstructionBytes.Length; i++)
		{
			memory.Write8(regs.Eip + (uint)i, test.InstructionBytes[i]);
		}
		
		// Write initial memory state
		foreach (var memEntry in test.InitialState.Memory)
		{
			memory.Write8(memEntry.Address, memEntry.Value);
		}
		
		_output.WriteLine($"Memory at EIP before execution:");
		for (int i = 0; i < test.InstructionBytes.Length; i++)
		{
			var addr = regs.Eip + (uint)i;
			var val = memory.Read8(addr);
			_output.WriteLine($"  [0x{addr:X8}] = 0x{val:X2}");
		}
		_output.WriteLine("");
		
		// Execute instruction
		_output.WriteLine("Executing instruction...");
		cpu.SingleStep(memory);
		
		var finalEip = cpu.GetEip();
		_output.WriteLine($"Final EIP: 0x{finalEip:X8}");
		_output.WriteLine($"Expected EIP: 0x{test.FinalState.Registers.Eip:X8}");
		_output.WriteLine($"Difference: {(int)(test.FinalState.Registers.Eip - finalEip)} bytes");
		
		Assert.Equal(test.FinalState.Registers.Eip, finalEip);
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
