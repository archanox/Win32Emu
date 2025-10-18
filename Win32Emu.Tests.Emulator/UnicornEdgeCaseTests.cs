using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Edge case tests for CPU instructions using Unicorn Engine as reference
/// These tests focus on boundary conditions, overflow, underflow, and corner cases
/// </summary>
public class UnicornEdgeCaseTests : IDisposable
{
    private readonly UnicornTestHelper _helper;

    public UnicornEdgeCaseTests()
    {
        _helper = new UnicornTestHelper();
    }

    #region Overflow and Underflow Tests

    [Fact]
    public void ADD_MaxUInt_Plus1_ShouldWrapAround()
    {
        // Arrange: ADD EAX, EBX (01 D8)
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.SetReg("EBX", 0x00000001);
        _helper.WriteCode(0x01, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX");
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf); // Should wrap to 0 with carry
    }

    [Fact]
    public void SUB_Zero_Minus1_ShouldUnderflow()
    {
        // Arrange: SUB EAX, EBX (29 D8)
        _helper.SetReg("EAX", 0x00000000);
        _helper.SetReg("EBX", 0x00000001);
        _helper.WriteCode(0x29, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0xFFFFFFFF
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Sf); // Carry and sign flags
    }

    [Fact]
    public void INC_MaxUInt_ShouldWrapToZero()
    {
        // Arrange: INC EAX (40)
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.WriteCode(0x40);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0
        _helper.AssertFlagsMatch(CpuFlag.Zf); // Zero flag should be set
        // Note: INC does NOT affect carry flag
    }

    [Fact]
    public void DEC_Zero_ShouldWrapToMaxUInt()
    {
        // Arrange: DEC EAX (48)
        _helper.SetReg("EAX", 0x00000000);
        _helper.WriteCode(0x48);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0xFFFFFFFF
        _helper.AssertFlagsMatch(CpuFlag.Sf); // Sign flag should be set
        // Note: DEC does NOT affect carry flag
    }

    #endregion

    #region Zero Result Tests

    [Fact]
    public void XOR_SameRegister_ShouldProduceZero()
    {
        // Arrange: XOR EBX, EBX (31 DB)
        _helper.SetReg("EBX", 0x12345678);
        _helper.WriteCode(0x31, 0xDB);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EBX"); // Should be 0
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Of); // ZF set, CF/OF clear
    }

    [Fact]
    public void SUB_SameValue_ShouldProduceZero()
    {
        // Arrange: SUB EAX, EAX (29 C0)
        _helper.SetReg("EAX", 0x12345678);
        _helper.WriteCode(0x29, 0xC0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0
        _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf); // ZF set, CF clear (no borrow)
    }

    [Fact]
    public void AND_NoCommonBits_ShouldProduceZero()
    {
        // Arrange: AND EAX, EBX (21 D8)
        _helper.SetReg("EAX", 0x55555555); // 01010101...
        _helper.SetReg("EBX", 0xAAAAAAAA); // 10101010...
        _helper.WriteCode(0x21, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0
        _helper.AssertFlagsMatch(CpuFlag.Zf); // ZF set
    }

    #endregion

    #region Sign Flag Tests

    [Fact]
    public void NEG_PositiveValue_ShouldSetSignFlag()
    {
        // Arrange: NEG EAX (F7 D8)
        _helper.SetReg("EAX", 0x00000001);
        _helper.WriteCode(0xF7, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0xFFFFFFFF (-1)
        _helper.AssertFlagsMatch(CpuFlag.Sf, CpuFlag.Cf); // SF and CF set
    }

    [Fact]
    public void NEG_NegativeValue_ShouldClearSignFlag()
    {
        // Arrange: NEG EAX (F7 D8)
        _helper.SetReg("EAX", 0xFFFFFFFF); // -1
        _helper.WriteCode(0xF7, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0x00000001 (1)
        _helper.AssertFlagsMatch(CpuFlag.Cf); // CF set, SF clear
    }

    [Fact]
    public void NEG_Zero_ShouldStayZero()
    {
        // Arrange: NEG EAX (F7 D8)
        _helper.SetReg("EAX", 0x00000000);
        _helper.WriteCode(0xF7, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should still be 0
        _helper.AssertFlagsMatch(CpuFlag.Zf); // ZF set, CF clear
    }

    #endregion

    #region Shift Edge Cases

    [Fact]
    public void SHL_ShiftOut_MSB_ShouldSetCarry()
    {
        // Arrange: SHL EAX, 1 (D1 E0)
        _helper.SetReg("EAX", 0x80000000); // MSB set
        _helper.WriteCode(0xD1, 0xE0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf); // Carry from MSB, result is zero
    }

    [Fact]
    public void SHR_ShiftOut_LSB_ShouldSetCarry()
    {
        // Arrange: SHR EAX, 1 (D1 E8)
        _helper.SetReg("EAX", 0x00000001); // LSB set
        _helper.WriteCode(0xD1, 0xE8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf); // Carry from LSB, result is zero
    }

    [Fact]
    public void SAR_NegativeNumber_ShouldPreserveSignBit()
    {
        // Arrange: SAR EAX, 1 (D1 F8)
        _helper.SetReg("EAX", 0xFFFFFFFF); // -1
        _helper.WriteCode(0xD1, 0xF8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should still be 0xFFFFFFFF (-1)
        _helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Sf); // Carry from LSB, sign preserved
    }

    #endregion

    #region Multiplication Edge Cases

    [Fact]
    public void IMUL_Negative_Times_Positive()
    {
        // Arrange: IMUL EBX (F7 EB)
        _helper.SetReg("EAX", 0xFFFFFFFF); // -1
        _helper.SetReg("EBX", 0x00000005); // 5
        _helper.SetReg("EDX", 0x00000000);
        _helper.WriteCode(0xF7, 0xEB);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0xFFFFFFFB (-5)
        _helper.AssertRegistersMatch("EDX"); // High part should be 0xFFFFFFFF
    }

    [Fact]
    public void IMUL_Negative_Times_Negative()
    {
        // Arrange: IMUL EBX (F7 EB)
        _helper.SetReg("EAX", 0xFFFFFFFF); // -1
        _helper.SetReg("EBX", 0xFFFFFFFB); // -5
        _helper.SetReg("EDX", 0x00000000);
        _helper.WriteCode(0xF7, 0xEB);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 5
        _helper.AssertRegistersMatch("EDX"); // High part should be 0
    }

    [Fact]
    public void IMUL_Zero_ShouldProduceZero()
    {
        // Arrange: IMUL EBX (F7 EB)
        _helper.SetReg("EAX", 0x00000000);
        _helper.SetReg("EBX", 0x12345678);
        _helper.SetReg("EDX", 0xFFFFFFFF);
        _helper.WriteCode(0xF7, 0xEB);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0
        _helper.AssertRegistersMatch("EDX"); // Should be 0
    }

    #endregion

    #region Division Edge Cases

    [Fact]
    public void DIV_ByOne_ShouldReturnDividend()
    {
        // Arrange: DIV EBX (F7 F3)
        _helper.SetReg("EAX", 0x12345678);
        _helper.SetReg("EDX", 0x00000000);
        _helper.SetReg("EBX", 0x00000001);
        _helper.WriteCode(0xF7, 0xF3);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0x12345678
        _helper.AssertRegistersMatch("EDX"); // Remainder should be 0
    }

    [Fact]
    public void DIV_LargeNumber_ShouldProduceQuotientAndRemainder()
    {
        // Arrange: DIV EBX (F7 F3)
        _helper.SetReg("EAX", 0x0000000A); // 10
        _helper.SetReg("EDX", 0x00000000);
        _helper.SetReg("EBX", 0x00000003); // divisor = 3
        _helper.WriteCode(0xF7, 0xF3);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 3
        _helper.AssertRegistersMatch("EDX"); // Should be 1 (remainder)
    }

    #endregion

    #region Immediate Value Tests

    [Fact]
    public void MOV_ImmediateZero_ShouldSetToZero()
    {
        // Arrange: MOV EAX, 0 (B8 00 00 00 00)
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.WriteCode(0xB8, 0x00, 0x00, 0x00, 0x00);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0
    }

    [Fact]
    public void MOV_ImmediateMax_ShouldSetToMax()
    {
        // Arrange: MOV EAX, 0xFFFFFFFF (B8 FF FF FF FF)
        _helper.SetReg("EAX", 0x00000000);
        _helper.WriteCode(0xB8, 0xFF, 0xFF, 0xFF, 0xFF);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should be 0xFFFFFFFF
    }

    #endregion

    #region Rotate Edge Cases

    [Fact]
    public void ROL_AllOnes_ShouldStayAllOnes()
    {
        // Arrange: ROL EAX, 1 (D1 C0)
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.WriteCode(0xD1, 0xC0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should still be 0xFFFFFFFF
        _helper.AssertFlagsMatch(CpuFlag.Cf); // Carry from MSB
    }

    [Fact]
    public void ROR_AllOnes_ShouldStayAllOnes()
    {
        // Arrange: ROR EAX, 1 (D1 C8)
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.WriteCode(0xD1, 0xC8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        _helper.AssertRegistersMatch("EAX"); // Should still be 0xFFFFFFFF
        _helper.AssertFlagsMatch(CpuFlag.Cf); // Carry from LSB
    }

    #endregion

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
