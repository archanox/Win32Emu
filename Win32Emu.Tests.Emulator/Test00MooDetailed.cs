using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using System.Linq;

namespace Win32Emu.Tests.Emulator;

public class Test00MooDetailed
{
    private readonly ITestOutputHelper _output;
    
    public Test00MooDetailed(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ShowMemoryMismatches()
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
        
        // Create CPU and memory
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
        cpu.SetRegister("CS", regs.Cs);
        cpu.SetRegister("DS", regs.Ds);
        cpu.SetRegister("ES", regs.Es);
        cpu.SetRegister("FS", regs.Fs);
        cpu.SetRegister("GS", regs.Gs);
        cpu.SetRegister("SS", regs.Ss);
        
        // Write instruction bytes
        for (var i = 0; i < test.InstructionBytes.Length; i++)
        {
            memory.Write8(regs.Eip + (uint)i, test.InstructionBytes[i]);
        }
        
        // Write initial memory
        foreach (var memEntry in test.InitialState.Memory)
        {
            memory.Write8(memEntry.Address, memEntry.Value);
        }
        
        // Execute instruction
        cpu.SingleStep(memory);
        
        // Now check which memory locations are mismatched
        _output.WriteLine("\nMemory locations that should have changed:");
        var initialMemAddresses = test.InitialState.Memory.Select(m => m.Address).ToHashSet();
        var finalMemAddresses = test.FinalState.Memory.Select(m => m.Address).ToHashSet();
        var changedAddresses = finalMemAddresses.Except(initialMemAddresses).ToList();
        
        foreach (var addr in changedAddresses)
        {
            var expectedValue = test.FinalState.Memory.First(m => m.Address == addr).Value;
            var actualValue = memory.Read8(addr);
            var match = actualValue == expectedValue ? "✓" : "✗";
            _output.WriteLine($"  {match} NEW address [0x{addr:X8}] expected=0x{expectedValue:X2}, actual=0x{actualValue:X2}");
        }
        
        _output.WriteLine("\nMemory locations that should have same value:");
        var unchangedAddresses = initialMemAddresses.Intersect(finalMemAddresses).ToList();
        foreach (var addr in unchangedAddresses.Take(5))
        {
            var expectedValue = test.FinalState.Memory.First(m => m.Address == addr).Value;
            var actualValue = memory.Read8(addr);
            var match = actualValue == expectedValue ? "✓" : "✗";
            _output.WriteLine($"  {match} UNCHANGED address [0x{addr:X8}] expected=0x{expectedValue:X2}, actual=0x{actualValue:X2}");
        }
        
        _output.WriteLine("\nAll memory addresses in final state:");
        foreach (var memEntry in test.FinalState.Memory.OrderBy(m => m.Address))
        {
            var inInit = initialMemAddresses.Contains(memEntry.Address);
            var actualValue = memory.Read8(memEntry.Address);
            var match = actualValue == memEntry.Value ? "✓" : "✗";
            var tag = inInit ? "INIT+FINA" : "FINA only";
            _output.WriteLine($"  {match} [{tag}] [0x{memEntry.Address:X8}] expected=0x{memEntry.Value:X2}, actual=0x{actualValue:X2}");
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
