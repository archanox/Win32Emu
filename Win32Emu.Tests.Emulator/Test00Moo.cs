using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

public class Test00Moo
{
    private readonly ITestOutputHelper _output;
    
    public Test00Moo(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void DebugFirstTest()
    {
        var testFile = FindTestFile("00.MOO.gz");
        if (testFile == null)
        {
            _output.WriteLine("Test file not found");
            return;
        }
        
        var mooFile = MooFileParser.Parse(testFile);
        var test = mooFile.Tests[0];
        
        _output.WriteLine($"Test: {test.Name}");
        _output.WriteLine($"Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
        
        // Create CPU and memory
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
        cpu.SetRegister("SS", regs.Ss);
        cpu.SetRegister("DS", regs.Ds);
        
        _output.WriteLine($"\nInitial CPU state:");
        _output.WriteLine($"  EIP: 0x{cpu.GetEip():X8}");
        _output.WriteLine($"  EBP: 0x{cpu.GetRegister("EBP"):X8}");
        _output.WriteLine($"  BL: 0x{cpu.GetRegister("EBX") & 0xFF:X2}");
        _output.WriteLine($"  SS: 0x{cpu.GetRegister("SS"):X4}");
        _output.WriteLine($"  EFLAGS: 0x{cpu.GetRegister("EFLAGS"):X8}");
        
        // Write instruction bytes
        for (var i = 0; i < test.InstructionBytes.Length; i++)
        {
            memory.Write8(regs.Eip + (uint)i, test.InstructionBytes[i]);
        }
        
        // Write initial memory
        foreach (var memEntry in test.InitialState.Memory)
        {
            _output.WriteLine($"Writing initial memory: [0x{memEntry.Address:X8}] = 0x{memEntry.Value:X2}");
            memory.Write8(memEntry.Address, memEntry.Value);
        }
        
        // Calculate expected memory address for [SS:BP+0x60]
        var bp = (ushort)cpu.GetRegister("EBP");
        var offset = 0x60u;
        var addr = (uint)((bp + offset) & 0xFFFF);
        _output.WriteLine($"\nCalculated address [BP+0x60]: 0x{addr:X8}");
        _output.WriteLine($"  BP = 0x{bp:X4}, offset = 0x{offset:X2}");
        _output.WriteLine($"  Memory at address before: 0x{memory.Read8(addr):X2}");
        
        // Execute instruction
        cpu.SingleStep(memory);
        
        _output.WriteLine($"\nAfter execution:");
        _output.WriteLine($"  EIP: 0x{cpu.GetEip():X8}");
        _output.WriteLine($"  EFLAGS: 0x{cpu.GetRegister("EFLAGS"):X8}");
        _output.WriteLine($"  Memory at address after: 0x{memory.Read8(addr):X2}");
        
        // Check all memory locations that changed
        _output.WriteLine($"\nChecking final memory state:");
        foreach (var memEntry in test.FinalState.Memory)
        {
            var actualValue = memory.Read8(memEntry.Address);
            var match = actualValue == memEntry.Value ? "✓" : "✗";
            _output.WriteLine($"  {match} [0x{memEntry.Address:X8}] expected=0x{memEntry.Value:X2}, actual=0x{actualValue:X2}");
        }
        
        _output.WriteLine($"\nExpected final state:");
        _output.WriteLine($"  EIP: 0x{test.FinalState.Registers.Eip:X8}");
        _output.WriteLine($"  EFLAGS: 0x{test.FinalState.Registers.Eflags:X8}");
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
