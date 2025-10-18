using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Comprehensive CPU conformance tests that validate Win32Emu against Unicorn Engine
/// These tests ensure our emulator behaves identically to a reference implementation
/// </summary>
public class UnicornConformanceTests : IDisposable
{
    private readonly UnicornTestHelper _helper;

    public UnicornConformanceTests()
    {
        _helper = new UnicornTestHelper();
    }

    #region Arithmetic Instructions

    [Fact]
    public void ADD_EAX_EBX_ShouldMatchUnicorn()
    {
        // Arrange: ADD EAX, EBX (01 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000003);
        _helper.WriteCode(0x01, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert - registers and flags should match
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EBX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void ADD_WithCarry_ShouldMatchUnicorn()
    {
        // Arrange: ADD EAX, EBX (01 D8)
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.SetReg("EBX", 0x00000001);
        _helper.WriteCode(0x01, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void SUB_EAX_EBX_ShouldMatchUnicorn()
    {
        // Arrange: SUB EAX, EBX (29 D8)
        _helper.SetReg("EAX", 0x00000010);
        _helper.SetReg("EBX", 0x00000005);
        _helper.WriteCode(0x29, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void SUB_WithBorrow_ShouldMatchUnicorn()
    {
        // Arrange: SUB EAX, EBX (29 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000010);
        _helper.WriteCode(0x29, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void INC_EAX_ShouldMatchUnicorn()
    {
        // Arrange: INC EAX (40)
        _helper.SetReg("EAX", 0x00000005);
        _helper.WriteCode(0x40);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void DEC_EAX_ShouldMatchUnicorn()
    {
        // Arrange: DEC EAX (48)
        _helper.SetReg("EAX", 0x00000005);
        _helper.WriteCode(0x48);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void IMUL_EBX_ShouldMatchUnicorn()
    {
        // Arrange: IMUL EBX (F7 EB)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000003);
        _helper.SetReg("EDX", 0xFFFFFFFF);
        _helper.WriteCode(0xF7, 0xEB);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EDX");
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of);
    }

    [Fact]
    public void DIV_EBX_ShouldMatchUnicorn()
    {
        // Arrange: DIV EBX (F7 F3)
        _helper.SetReg("EAX", 0x0000000F);
        _helper.SetReg("EDX", 0x00000000);
        _helper.SetReg("EBX", 0x00000003);
        _helper.WriteCode(0xF7, 0xF3);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EDX");
    }

    #endregion

    #region Logical Instructions

    [Fact]
    public void AND_EAX_EBX_ShouldMatchUnicorn()
    {
        // Arrange: AND EAX, EBX (21 D8)
        _helper.SetReg("EAX", 0xFF00FF00);
        _helper.SetReg("EBX", 0xF0F0F0F0);
        _helper.WriteCode(0x21, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
    }

    [Fact]
    public void OR_EAX_EBX_ShouldMatchUnicorn()
    {
        // Arrange: OR EAX, EBX (09 D8)
        _helper.SetReg("EAX", 0x00FF00FF);
        _helper.SetReg("EBX", 0xFF00FF00);
        _helper.WriteCode(0x09, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
    }

    [Fact]
    public void XOR_EAX_EAX_ShouldMatchUnicorn()
    {
        // Arrange: XOR EAX, EAX (31 C0)
        _helper.SetReg("EAX", 0x12345678);
        _helper.WriteCode(0x31, 0xC0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf, CpuFlag.Cf, CpuFlag.Of);
    }

    [Fact]
    public void XOR_EAX_EBX_ShouldMatchUnicorn()
    {
        // Arrange: XOR EAX, EBX (31 D8)
        _helper.SetReg("EAX", 0xFF00FF00);
        _helper.SetReg("EBX", 0xF0F0F0F0);
        _helper.WriteCode(0x31, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf, CpuFlag.Cf, CpuFlag.Of);
    }

    [Fact]
    public void NOT_EAX_ShouldMatchUnicorn()
    {
        // Arrange: NOT EAX (F7 D0)
        _helper.SetReg("EAX", 0x12345678);
        _helper.WriteCode(0xF7, 0xD0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
    }

    [Fact]
    public void NEG_EAX_ShouldMatchUnicorn()
    {
        // Arrange: NEG EAX (F7 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.WriteCode(0xF7, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Cf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    #endregion

    #region Bit Manipulation Instructions

    [Fact]
    public void SHL_EAX_1_ShouldMatchUnicorn()
    {
        // Arrange: SHL EAX, 1 (D1 E0)
        _helper.SetReg("EAX", 0x00000005);
        _helper.WriteCode(0xD1, 0xE0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf);
    }

    [Fact]
    public void SHL_EAX_WithCarry_ShouldMatchUnicorn()
    {
        // Arrange: SHL EAX, 1 (D1 E0)
        _helper.SetReg("EAX", 0x80000000);
        _helper.WriteCode(0xD1, 0xE0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf);
    }

    [Fact]
    public void SHR_EAX_1_ShouldMatchUnicorn()
    {
        // Arrange: SHR EAX, 1 (D1 E8)
        _helper.SetReg("EAX", 0x0000000A);
        _helper.WriteCode(0xD1, 0xE8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf);
    }

    [Fact]
    public void SAR_EAX_1_ShouldMatchUnicorn()
    {
        // Arrange: SAR EAX, 1 (D1 F8)
        _helper.SetReg("EAX", 0x80000000);
        _helper.WriteCode(0xD1, 0xF8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf);
    }

    [Fact]
    public void ROL_EAX_1_ShouldMatchUnicorn()
    {
        // Arrange: ROL EAX, 1 (D1 C0)
        _helper.SetReg("EAX", 0x80000001);
        _helper.WriteCode(0xD1, 0xC0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of);
    }

    [Fact]
    public void ROR_EAX_1_ShouldMatchUnicorn()
    {
        // Arrange: ROR EAX, 1 (D1 C8)
        _helper.SetReg("EAX", 0x00000003);
        _helper.WriteCode(0xD1, 0xC8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of);
    }

    #endregion

    #region Comparison Instructions

    [Fact]
    public void CMP_EAX_EBX_Equal_ShouldMatchUnicorn()
    {
        // Arrange: CMP EAX, EBX (39 D8)
        _helper.SetReg("EAX", 0x12345678);
        _helper.SetReg("EBX", 0x12345678);
        _helper.WriteCode(0x39, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert - registers should not change
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EBX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void CMP_EAX_EBX_Less_ShouldMatchUnicorn()
    {
        // Arrange: CMP EAX, EBX (39 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000010);
        _helper.WriteCode(0x39, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EBX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void CMP_EAX_EBX_Greater_ShouldMatchUnicorn()
    {
        // Arrange: CMP EAX, EBX (39 D8)
        _helper.SetReg("EAX", 0x00000010);
        _helper.SetReg("EBX", 0x00000005);
        _helper.WriteCode(0x39, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EBX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void TEST_EAX_EBX_ShouldMatchUnicorn()
    {
        // Arrange: TEST EAX, EBX (85 D8)
        _helper.SetReg("EAX", 0x12345678);
        _helper.SetReg("EBX", 0x12345678);
        _helper.WriteCode(0x85, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert - registers should not change
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EBX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
    }

    [Fact]
    public void TEST_ZeroResult_ShouldMatchUnicorn()
    {
        // Arrange: TEST EAX, EBX (85 D8)
        _helper.SetReg("EAX", 0x00FF00FF);
        _helper.SetReg("EBX", 0xFF00FF00);
        _helper.WriteCode(0x85, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EBX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
    }

    #endregion

    #region Sign Extension Instructions

    [Fact]
    public void CDQ_WithPositiveEAX_ShouldMatchUnicorn()
    {
        // Arrange: CDQ (99)
        _helper.SetReg("EAX", 0x00000042);
        _helper.SetReg("EDX", 0xFFFFFFFF);
        _helper.WriteCode(0x99);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EDX");
    }

    [Fact]
    public void CDQ_WithNegativeEAX_ShouldMatchUnicorn()
    {
        // Arrange: CDQ (99)
        _helper.SetReg("EAX", 0x80000000);
        _helper.SetReg("EDX", 0x00000000);
        _helper.WriteCode(0x99);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EDX");
    }

    #endregion

    #region Stack Operations

    [Fact]
    public void PUSH_EAX_ShouldMatchUnicorn()
    {
        // Arrange: PUSH EAX (50)
        _helper.SetReg("EAX", 0x12345678);
        var initialEsp = _helper.GetWin32EmuReg("ESP");
        _helper.WriteCode(0x50);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("ESP");
        
        // Check that value was pushed to stack
        var expectedEsp = initialEsp - 4;
        Assert.Equal(expectedEsp, _helper.GetWin32EmuReg("ESP"));
        Assert.Equal(0x12345678u, _helper.ReadWin32EmuMemory32(expectedEsp));
        Assert.Equal(0x12345678u, _helper.ReadUnicornMemory32(expectedEsp));
    }

    [Fact]
    public void POP_EAX_ShouldMatchUnicorn()
    {
        // Arrange: Setup stack, then POP EAX (58)
        var stackAddr = _helper.GetWin32EmuReg("ESP") - 4;
        _helper.SetReg("ESP", stackAddr);
        _helper.WriteMemory32(stackAddr, 0x87654321);
        _helper.SetReg("EAX", 0x00000000);
        _helper.WriteCode(0x58);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("ESP");
    }

    #endregion

    #region Move Instructions

    [Fact]
    public void MOV_EAX_EBX_ShouldMatchUnicorn()
    {
        // Arrange: MOV EAX, EBX (89 D8)
        _helper.SetReg("EAX", 0x00000000);
        _helper.SetReg("EBX", 0x12345678);
        _helper.WriteCode(0x89, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertRegistersMatch("EBX");
    }

    [Fact]
    public void MOV_EAX_Immediate_ShouldMatchUnicorn()
    {
        // Arrange: MOV EAX, 0x12345678 (B8 78 56 34 12)
        _helper.SetReg("EAX", 0x00000000);
        _helper.WriteCode(0xB8, 0x78, 0x56, 0x34, 0x12);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
    }

    #endregion

    #region ADC and SBB Instructions (with carry)

    [Fact]
    public void ADC_EAX_EBX_WithoutCarry_ShouldMatchUnicorn()
    {
        // Arrange: ADC EAX, EBX (11 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000003);
        _helper.SetReg("EFLAGS", 0x00000000); // Clear carry flag
        _helper.WriteCode(0x11, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void ADC_EAX_EBX_WithCarry_ShouldMatchUnicorn()
    {
        // Arrange: ADC EAX, EBX (11 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000003);
        _helper.SetReg("EFLAGS", 0x00000001); // Set carry flag
        _helper.WriteCode(0x11, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void SBB_EAX_EBX_WithoutBorrow_ShouldMatchUnicorn()
    {
        // Arrange: SBB EAX, EBX (19 D8)
        _helper.SetReg("EAX", 0x00000010);
        _helper.SetReg("EBX", 0x00000005);
        _helper.SetReg("EFLAGS", 0x00000000); // Clear carry flag
        _helper.WriteCode(0x19, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    [Fact]
    public void SBB_EAX_EBX_WithBorrow_ShouldMatchUnicorn()
    {
        // Arrange: SBB EAX, EBX (19 D8)
        _helper.SetReg("EAX", 0x00000010);
        _helper.SetReg("EBX", 0x00000005);
        _helper.SetReg("EFLAGS", 0x00000001); // Set carry flag (borrow)
        _helper.WriteCode(0x19, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
    }

    #endregion

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
