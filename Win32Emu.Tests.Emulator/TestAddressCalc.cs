using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Iced.Intel;

namespace Win32Emu.Tests.Emulator;

public class TestAddressCalc
{
    private readonly ITestOutputHelper _output;
    
    public TestAddressCalc(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ShowAddressCalculation()
    {
        var bytes = new byte[] { 0x00, 0x5E, 0x60, 0xF4 };
        
        var decoder = Decoder.Create(16, bytes);
        decoder.IP = 0x72A0;
        
        var insn = decoder.Decode();
        
        _output.WriteLine($"Instruction: {insn}");
        _output.WriteLine($"MemoryDisplacement32: 0x{insn.MemoryDisplacement32:X8}");
        _output.WriteLine($"MemoryDisplacement64: 0x{insn.MemoryDisplacement64:X16}");
        _output.WriteLine($"MemoryDisplSize: {insn.MemoryDisplSize}");
        
        // Now simulate the address calculation
        uint offset = insn.MemoryDisplacement32;
        _output.WriteLine($"\nInitial offset (from displacement): 0x{offset:X8}");
        
        // Add BP (simulated as 0x0001)
        ushort bp = 0x0001;
        offset += bp;
        _output.WriteLine($"After adding BP (0x{bp:X4}): 0x{offset:X8}");
        
        // Apply 16-bit mask (as done in CalcMemAddress)
        uint addr = offset & 0xFFFF;
        _output.WriteLine($"After 16-bit mask: 0x{addr:X8}");
        
        // Also try without masking
        _output.WriteLine($"Without mask: 0x{offset:X8}");
    }
}
