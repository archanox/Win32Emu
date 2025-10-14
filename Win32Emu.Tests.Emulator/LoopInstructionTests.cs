using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for x86 LOOP family instructions (LOOP, LOOPE, LOOPNE)
/// </summary>
public class LoopInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public LoopInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void LOOP_WithNonZeroECX_ShouldDecrementAndJump()
    {
        // Arrange: LOOP -5 (E2 FB) - jump back 5 bytes
        _helper.SetReg("ECX", 0x00000003);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE2, 0xFB);  // LOOP -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000002u, _helper.GetReg("ECX")); // ECX should be decremented
        Assert.Equal(0x00400FFDu, _helper.Cpu.GetEip()); // Should jump back (0x401002 - 5 = 0x400FFD)
    }

    [Fact]
    public void LOOP_WithECXEqualsOne_ShouldDecrementToZeroAndNotJump()
    {
        // Arrange: LOOP -5 (E2 FB)
        _helper.SetReg("ECX", 0x00000001);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE2, 0xFB);  // LOOP -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("ECX")); // ECX should be decremented to 0
        Assert.Equal(0x00401002u, _helper.Cpu.GetEip()); // Should NOT jump, continue to next instruction
    }

    [Fact]
    public void LOOP_WithECXEqualsZero_ShouldDecrementToMaxAndNotJump()
    {
        // Arrange: LOOP -5 (E2 FB)
        _helper.SetReg("ECX", 0x00000000);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE2, 0xFB);  // LOOP -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0xFFFFFFFFu, _helper.GetReg("ECX")); // ECX should wrap to 0xFFFFFFFF
        Assert.Equal(0x00400FFDu, _helper.Cpu.GetEip()); // Should jump (ECX is now non-zero)
    }

    [Fact]
    public void LOOPE_WithNonZeroECXAndZFSet_ShouldDecrementAndJump()
    {
        // Arrange: LOOPE -5 (E1 FB) - jump back 5 bytes if ZF=1 and ECX!=0
        _helper.SetReg("ECX", 0x00000003);
        _helper.SetFlag(CpuFlag.Zf, true);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE1, 0xFB);  // LOOPE -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000002u, _helper.GetReg("ECX")); // ECX should be decremented
        Assert.Equal(0x00400FFDu, _helper.Cpu.GetEip()); // Should jump
    }

    [Fact]
    public void LOOPE_WithNonZeroECXAndZFClear_ShouldDecrementAndNotJump()
    {
        // Arrange: LOOPE -5 (E1 FB)
        _helper.SetReg("ECX", 0x00000003);
        _helper.SetFlag(CpuFlag.Zf, false);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE1, 0xFB);  // LOOPE -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000002u, _helper.GetReg("ECX")); // ECX should be decremented
        Assert.Equal(0x00401002u, _helper.Cpu.GetEip()); // Should NOT jump (ZF is clear)
    }

    [Fact]
    public void LOOPE_WithECXEqualsOne_ShouldDecrementToZeroAndNotJump()
    {
        // Arrange: LOOPE -5 (E1 FB)
        _helper.SetReg("ECX", 0x00000001);
        _helper.SetFlag(CpuFlag.Zf, true);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE1, 0xFB);  // LOOPE -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("ECX")); // ECX should be decremented to 0
        Assert.Equal(0x00401002u, _helper.Cpu.GetEip()); // Should NOT jump (ECX is zero)
    }

    [Fact]
    public void LOOPNE_WithNonZeroECXAndZFClear_ShouldDecrementAndJump()
    {
        // Arrange: LOOPNE -5 (E0 FB) - jump back 5 bytes if ZF=0 and ECX!=0
        _helper.SetReg("ECX", 0x00000003);
        _helper.SetFlag(CpuFlag.Zf, false);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE0, 0xFB);  // LOOPNE -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000002u, _helper.GetReg("ECX")); // ECX should be decremented
        Assert.Equal(0x00400FFDu, _helper.Cpu.GetEip()); // Should jump
    }

    [Fact]
    public void LOOPNE_WithNonZeroECXAndZFSet_ShouldDecrementAndNotJump()
    {
        // Arrange: LOOPNE -5 (E0 FB)
        _helper.SetReg("ECX", 0x00000003);
        _helper.SetFlag(CpuFlag.Zf, true);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE0, 0xFB);  // LOOPNE -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000002u, _helper.GetReg("ECX")); // ECX should be decremented
        Assert.Equal(0x00401002u, _helper.Cpu.GetEip()); // Should NOT jump (ZF is set)
    }

    [Fact]
    public void LOOPNE_WithECXEqualsOne_ShouldDecrementToZeroAndNotJump()
    {
        // Arrange: LOOPNE -5 (E0 FB)
        _helper.SetReg("ECX", 0x00000001);
        _helper.SetFlag(CpuFlag.Zf, false);
        _helper.Cpu.SetEip(0x00401000);
        _helper.WriteCode(0xE0, 0xFB);  // LOOPNE -5

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("ECX")); // ECX should be decremented to 0
        Assert.Equal(0x00401002u, _helper.Cpu.GetEip()); // Should NOT jump (ECX is zero)
    }

    public void Dispose()
    {
        _helper.Dispose();
    }
}
