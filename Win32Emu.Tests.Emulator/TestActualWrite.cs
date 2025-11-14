using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

public class TestActualWrite
{
    private readonly ITestOutputHelper _output;
    
    public TestActualWrite(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ShowActualMemoryWrite()
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
        
        // Take snapshot of memory before execution
        var memBefore = new Dictionary<uint, byte>();
        for (uint addr = 0; addr < Math.Min(0x100000, memory.Size); addr++)
        {
            var val = memory.Read8(addr);
            if (val != 0)
            {
                memBefore[addr] = val;
            }
        }
        
        // Execute instruction
        cpu.SingleStep(memory);
        cpu.SingleStep(memory); // Execute HLT too
        
        // Find what changed
        _output.WriteLine($"\nMemory locations that changed:");
        for (uint addr = 0; addr < Math.Min(0x100000, memory.Size); addr++)
        {
            var valAfter = memory.Read8(addr);
            var valBefore = memBefore.ContainsKey(addr) ? memBefore[addr] : (byte)0;
            
            if (valAfter != valBefore)
            {
                _output.WriteLine($"  [0x{addr:X8}] changed from 0x{valBefore:X2} to 0x{valAfter:X2}");
            }
        }
        
        _output.WriteLine($"\nExpected write to: 0x00000001 with value 0x21");
        _output.WriteLine($"BL = 0x{regs.Ebx & 0xFF:X2}");
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
