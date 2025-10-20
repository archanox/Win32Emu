using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for x87 FPU instructions
/// </summary>
public class FpuInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public FpuInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void FCOMP_ST1_ShouldCompareAndPop()
    {
        // Arrange: FCOMP ST(1) - Compare ST(0) with ST(1) and pop
        // Opcode: D8 D9
        // We'll need to set up the FPU stack first
        
        // FLD1 - Load 1.0 onto FPU stack (D9 E8)
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FLD1 - Load another 1.0 onto FPU stack (D9 E8)
        // Now stack is: ST(0)=1.0, ST(1)=1.0
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FCOMP ST(1) - Compare ST(0) with ST(1) and pop (D8 D9)
        _helper.WriteCode(0xD8, 0xD9);
        _helper.ExecuteInstruction();
        
        // Assert: When comparing equal values, ZF should be set, CF and PF should be clear
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set when values are equal");
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear when values are equal");
        Assert.False(_helper.IsFlagSet(CpuFlag.Pf), "PF should be clear when values are equal");
    }

    [Fact]
    public void FCOMP_WithMemory_ShouldCompareFloat32()
    {
        // Arrange: FCOMP dword ptr [address] - Compare ST(0) with memory float
        // Opcode: D8 1D + address
        
        // Write a float value 2.5 to memory
        var memAddr = 0x00200000u;
        var floatBits = BitConverter.SingleToInt32Bits(2.5f);
        _helper.WriteMemory32(memAddr, unchecked((uint)floatBits));
        
        // FLD1 - Load 1.0 onto FPU stack (D9 E8)
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FCOMP dword ptr [memAddr] - Compare ST(0) with memory (D8 1D + address)
        // Note: This is a simplified encoding for testing
        _helper.WriteCode(
            0xD8, 0x1D,  // FCOMP dword ptr [...]
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Assert: ST(0)=1.0 < memory=2.5, so CF should be set
        Assert.True(_helper.IsFlagSet(CpuFlag.Cf), "CF should be set when ST(0) < source");
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear when values are not equal");
        Assert.False(_helper.IsFlagSet(CpuFlag.Pf), "PF should be clear for ordered comparison");
    }

    [Fact]
    public void FCOMP_GreaterThan_ShouldClearAllFlags()
    {
        // Arrange: Test ST(0) > source case
        // We'll use memory comparison to test the greater-than case
        
        // Write a float value 0.5 to memory
        var memAddr = 0x00200000u;
        var floatBits = BitConverter.SingleToInt32Bits(0.5f);
        _helper.WriteMemory32(memAddr, unchecked((uint)floatBits));
        
        // FLD1 - Load 1.0 onto FPU stack (D9 E8)
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FCOMP dword ptr [memAddr] - Compare ST(0) with memory (D8 1D + address)
        _helper.WriteCode(
            0xD8, 0x1D,  // FCOMP dword ptr [...]
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Assert: ST(0)=1.0 > memory=0.5, so all flags should be clear
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear when ST(0) > source");
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear when ST(0) > source");
        Assert.False(_helper.IsFlagSet(CpuFlag.Pf), "PF should be clear for ordered comparison");
    }

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
