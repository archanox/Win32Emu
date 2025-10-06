using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for basic x86 arithmetic and logic instructions (8086/286/386 era)
/// These are foundational instructions that should be well-tested
/// </summary>
public class BasicInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public BasicInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void ADD_EAX_EBX_ShouldAddRegisters()
    {
        // Arrange: ADD EAX, EBX (01 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000003);
        _helper.WriteCode(0x01, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000008u, _helper.GetReg("EAX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear for non-zero result");
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear for no carry");
    }

    [Fact]
    public void ADD_WithCarry_ShouldSetCarryFlag()
    {
        // Arrange: ADD EAX, EBX (01 D8)
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.SetReg("EBX", 0x00000001);
        _helper.WriteCode(0x01, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("EAX"));
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set for zero result");
        Assert.True(_helper.IsFlagSet(CpuFlag.Cf), "CF should be set for carry");
    }

    [Fact]
    public void SUB_EAX_EBX_ShouldSubtractRegisters()
    {
        // Arrange: SUB EAX, EBX (29 D8)
        _helper.SetReg("EAX", 0x00000010);
        _helper.SetReg("EBX", 0x00000005);
        _helper.WriteCode(0x29, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x0000000Bu, _helper.GetReg("EAX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear for non-zero result");
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear for no borrow");
    }

    [Fact]
    public void SUB_WithBorrow_ShouldSetCarryFlag()
    {
        // Arrange: SUB EAX, EBX (29 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000010);
        _helper.WriteCode(0x29, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0xFFFFFFF5u, _helper.GetReg("EAX")); // Underflow
        Assert.True(_helper.IsFlagSet(CpuFlag.Cf), "CF should be set for borrow");
        Assert.True(_helper.IsFlagSet(CpuFlag.Sf), "SF should be set for negative result");
    }

    [Fact]
    public void XOR_EAX_EAX_ShouldClearRegister()
    {
        // Arrange: XOR EAX, EAX (31 C0)
        _helper.SetReg("EAX", 0x12345678);
        _helper.WriteCode(0x31, 0xC0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("EAX"));
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set for zero result");
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear for XOR");
        Assert.False(_helper.IsFlagSet(CpuFlag.Of), "OF should be clear for XOR");
    }

    [Fact]
    public void AND_EAX_EBX_ShouldPerformBitwiseAnd()
    {
        // Arrange: AND EAX, EBX (21 D8)
        _helper.SetReg("EAX", 0xFF00FF00);
        _helper.SetReg("EBX", 0xF0F0F0F0);
        _helper.WriteCode(0x21, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0xF000F000u, _helper.GetReg("EAX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear for non-zero result");
    }

    [Fact]
    public void OR_EAX_EBX_ShouldPerformBitwiseOr()
    {
        // Arrange: OR EAX, EBX (09 D8)
        _helper.SetReg("EAX", 0x00FF00FF);
        _helper.SetReg("EBX", 0xFF00FF00);
        _helper.WriteCode(0x09, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0xFFFFFFFFu, _helper.GetReg("EAX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear for non-zero result");
    }

    [Fact]
    public void TEST_EAX_EBX_ShouldNotModifyRegisters()
    {
        // Arrange: TEST EAX, EBX (85 D8)
        _helper.SetReg("EAX", 0x12345678);
        _helper.SetReg("EBX", 0x12345678);
        _helper.WriteCode(0x85, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert - Registers should not change
        Assert.Equal(0x12345678u, _helper.GetReg("EAX"));
        Assert.Equal(0x12345678u, _helper.GetReg("EBX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear for non-zero result");
    }

    [Fact]
    public void TEST_ZeroResult_ShouldSetZeroFlag()
    {
        // Arrange: TEST EAX, EBX (85 D8)
        _helper.SetReg("EAX", 0x00FF00FF);
        _helper.SetReg("EBX", 0xFF00FF00);
        _helper.WriteCode(0x85, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set when AND result is zero");
    }

    [Fact]
    public void INC_EAX_ShouldIncrementRegister()
    {
        // Arrange: INC EAX (40)
        _helper.SetReg("EAX", 0x00000005);
        _helper.WriteCode(0x40);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000006u, _helper.GetReg("EAX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear for non-zero result");
    }

    [Fact]
    public void DEC_EAX_ShouldDecrementRegister()
    {
        // Arrange: DEC EAX (48)
        _helper.SetReg("EAX", 0x00000005);
        _helper.WriteCode(0x48);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000004u, _helper.GetReg("EAX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear for non-zero result");
    }

    [Fact]
    public void SHL_EAX_1_ShouldShiftLeft()
    {
        // Arrange: SHL EAX, 1 (D1 E0)
        _helper.SetReg("EAX", 0x00000005); // Binary: 0101
        _helper.WriteCode(0xD1, 0xE0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x0000000Au, _helper.GetReg("EAX")); // Binary: 1010
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear when MSB was 0");
    }

    [Fact]
    public void SHR_EAX_1_ShouldShiftRight()
    {
        // Arrange: SHR EAX, 1 (D1 E8)
        _helper.SetReg("EAX", 0x0000000A); // Binary: 1010
        _helper.WriteCode(0xD1, 0xE8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000005u, _helper.GetReg("EAX")); // Binary: 0101
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear when LSB was 0");
    }

    [Fact]
    public void CMP_EAX_EBX_Equal_ShouldSetZeroFlag()
    {
        // Arrange: CMP EAX, EBX (39 D8)
        _helper.SetReg("EAX", 0x12345678);
        _helper.SetReg("EBX", 0x12345678);
        _helper.WriteCode(0x39, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert - Registers should not change
        Assert.Equal(0x12345678u, _helper.GetReg("EAX"));
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set when values are equal");
    }

    [Fact]
    public void CMP_EAX_EBX_Less_ShouldSetCarryFlag()
    {
        // Arrange: CMP EAX, EBX (39 D8)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000010);
        _helper.WriteCode(0x39, 0xD8);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.True(_helper.IsFlagSet(CpuFlag.Cf), "CF should be set when first operand is less");
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear when values are not equal");
    }
    
    
    [Fact]
    public void IMUL_EBX_ShouldMultiplySigned()
    {
        // Arrange: IMUL EBX (F7 EB)
        // EDX:EAX = EAX * EBX (signed)
        _helper.SetReg("EAX", 0x00000005);
        _helper.SetReg("EBX", 0x00000003);
        _helper.SetReg("EDX", 0xFFFFFFFF); // Should be updated
        _helper.WriteCode(0xF7, 0xEB);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x0000000Fu, _helper.GetReg("EAX")); // 5 * 3 = 15
        Assert.Equal(0x00000000u, _helper.GetReg("EDX")); // High 32 bits should be 0
        Assert.False(_helper.IsFlagSet(CpuFlag.Cf), "CF should be clear when result fits");
    }

    [Fact]
    public void DIV_EBX_ShouldDivideUnsigned()
    {
        // Arrange: DIV EBX (F7 F3)
        // EAX = EDX:EAX / EBX (unsigned)
        // EDX = EDX:EAX % EBX (remainder)
        _helper.SetReg("EAX", 0x0000000F); // 15
        _helper.SetReg("EDX", 0x00000000);
        _helper.SetReg("EBX", 0x00000003); // divisor = 3
        _helper.WriteCode(0xF7, 0xF3);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000005u, _helper.GetReg("EAX")); // 15 / 3 = 5
        Assert.Equal(0x00000000u, _helper.GetReg("EDX")); // 15 % 3 = 0
    }

    [Fact]
    public void DIV_WithRemainder_ShouldSetRemainder()
    {
        // Arrange: DIV EBX (F7 F3)
        _helper.SetReg("EAX", 0x00000010); // 16
        _helper.SetReg("EDX", 0x00000000);
        _helper.SetReg("EBX", 0x00000003); // divisor = 3
        _helper.WriteCode(0xF7, 0xF3);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000005u, _helper.GetReg("EAX")); // 16 / 3 = 5
        Assert.Equal(0x00000001u, _helper.GetReg("EDX")); // 16 % 3 = 1
    }

    [Fact]
    public void CDQ_WithPositiveEAX_ShouldSetEDXToZero()
    {
        // Arrange: CDQ (99) - sign-extends EAX into EDX:EAX
        _helper.SetReg("EAX", 0x00000042); // Positive number (bit 31 = 0)
        _helper.SetReg("EDX", 0xFFFFFFFF); // Set to non-zero to verify it changes
        _helper.WriteCode(0x99);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000042u, _helper.GetReg("EAX")); // EAX unchanged
        Assert.Equal(0x00000000u, _helper.GetReg("EDX")); // EDX = 0 for positive EAX
    }

    [Theory]
    [InlineData(0x80000000u, 0x00000000u)] // Negative number (bit 31 = 1)
    [InlineData(0xFFFFFFF5u, 0x12345678u)] // -11 in two's complement (bit 31 = 1)
    public void CDQ_WithNegativeEAX_ShouldSetEDXToFFFFFFFF(uint eaxValue, uint edxInitial)
    {
        // Arrange: CDQ (99) - sign-extends EAX into EDX:EAX
        _helper.SetReg("EAX", eaxValue);
        _helper.SetReg("EDX", edxInitial); // Set to known value to verify it changes
        _helper.WriteCode(0x99);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(eaxValue, _helper.GetReg("EAX")); // EAX unchanged
        Assert.Equal(0xFFFFFFFFu, _helper.GetReg("EDX")); // EDX = 0xFFFFFFFF for negative EAX
    }
//TODO: redo IDIV_EBX_ShouldDivideSigned, MUL_WithOverflow_ShouldSetCarryAndOverflowFlags, MUL_WithOverflow_ShouldSetCarryFlag, MUL_EBX_ShouldMultiplyEAXByEBX & MUL_EBX_ShouldMultiplyUnsigned tests

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
