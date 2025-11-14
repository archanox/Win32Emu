using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using Win32Emu.Memory;
using Win32Emu.Cpu.Iced;
using System.Linq;

namespace Win32Emu.Tests.Emulator;

public class SegmentRegisterTest
{
	private readonly ITestOutputHelper _output;
	
	public SegmentRegisterTest(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void TestSegmentRegisterAddressing()
	{
		var testFile = FindTestFile("00.MOO.gz");
		if (testFile == null)
		{
			_output.WriteLine("Test file not found, skipping");
			return;
		}
		
		var mooFile = MooFileParser.Parse(testFile);
		var test = mooFile.Tests[0]; // First test: add [ss:bp+60h],bl
		
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
		
		_output.WriteLine($"Initial BP: 0x{regs.Ebp:X8} (16-bit: 0x{(regs.Ebp & 0xFFFF):X4})");
		_output.WriteLine($"Initial SS: 0x{regs.Ss:X4}");
		_output.WriteLine($"Initial BL: 0x{(regs.Ebx & 0xFF):X2}");
		_output.WriteLine("");
		
		// Calculate expected address
		var bp16 = (ushort)(regs.Ebp & 0xFFFF);
		var expectedAddr = (regs.Ss << 4) + bp16 + 0x60;
		_output.WriteLine($"Expected memory address: SS:BP+60h = 0x{regs.Ss:X4}:0x{bp16:X4}+60h = (0x{regs.Ss:X4} << 4) + 0x{bp16 + 0x60:X4} = 0x{expectedAddr:X8}");
		_output.WriteLine("");
		
		// Write instruction bytes to memory
		for (var i = 0; i < test.InstructionBytes.Length; i++)
		{
			memory.Write8(regs.Eip + (uint)i, test.InstructionBytes[i]);
		}
		
		// Write initial memory state
		foreach (var memEntry in test.InitialState.Memory)
		{
			memory.Write8(memEntry.Address, memEntry.Value);
			_output.WriteLine($"Initial memory [0x{memEntry.Address:X8}] = 0x{memEntry.Value:X2}");
		}
		_output.WriteLine("");
		
		// Execute until HLT
		int instructionCount = 0;
		while (instructionCount < 10)
		{
			var eip = cpu.GetEip();
			var opcode = memory.Read8(eip);
			
			_output.WriteLine($"Executing instruction at 0x{eip:X8}, opcode: 0x{opcode:X2}");
			cpu.SingleStep(memory);
			instructionCount++;
			
			if (opcode == 0xF4) break;
		}
		
		_output.WriteLine("");
		_output.WriteLine($"Final EIP: 0x{cpu.GetEip():X8}");
		_output.WriteLine($"Expected EIP: 0x{test.FinalState.Registers.Eip:X8}");
		
		// Check final memory state
		_output.WriteLine("");
		_output.WriteLine("Final memory changes:");
		foreach (var memEntry in test.FinalState.Memory)
		{
			var actualValue = memory.Read8(memEntry.Address);
			var match = actualValue == memEntry.Value ? "✓" : "✗";
			_output.WriteLine($"  [0x{memEntry.Address:X8}] expected: 0x{memEntry.Value:X2}, actual: 0x{actualValue:X2} {match}");
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
