using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for legacy BCD (Binary Coded Decimal) instructions
/// These instructions (AAD, AAM, DAS, DAA) were used in early x86 for decimal arithmetic
/// and are rarely used in modern code but must be supported for legacy executables
/// </summary>
public class BcdInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public BcdInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void AAD_ShouldConvertUnpackedBcdToBinary()
    {
        // AAD - ASCII Adjust AX Before Division
        // Opcode: D5 0A (base 10)
        // Formula: AL = AH * 10 + AL, AH = 0
        
        // Arrange: AH=0x02, AL=0x05 (unpacked BCD for 25)
        _helper.SetReg("EAX", 0x00000205);
        _helper.WriteCode(0xD5, 0x0A);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // AL should be 2*10 + 5 = 25 (0x19), AH should be 0
        Assert.Equal(0x00000019u, _helper.GetReg("EAX") & 0xFFFF);
    }

    [Fact]
    public void AAD_WithZeroResult_ShouldSetZeroFlag()
    {
        // Arrange: AH=0x00, AL=0x00
        _helper.SetReg("EAX", 0x00000000);
        _helper.WriteCode(0xD5, 0x0A);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("EAX") & 0xFFFF);
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set for zero result");
    }

    [Fact]
    public void AAM_ShouldConvertBinaryToUnpackedBcd()
    {
        // AAM - ASCII Adjust AX After Multiply
        // Opcode: D4 0A (base 10)
        // Formula: AH = AL / 10, AL = AL % 10
        
        // Arrange: AL=0x19 (25 in decimal)
        _helper.SetReg("EAX", 0x00000019);
        _helper.WriteCode(0xD4, 0x0A);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // AH should be 25/10 = 2, AL should be 25%10 = 5
        Assert.Equal(0x0205u, _helper.GetReg("EAX") & 0xFFFF);
    }

    [Fact]
    public void AAM_WithLargeValue_ShouldDivideCorrectly()
    {
        // Arrange: AL=0x63 (99 in decimal)
        _helper.SetReg("EAX", 0x00000063);
        _helper.WriteCode(0xD4, 0x0A);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // AH should be 99/10 = 9, AL should be 99%10 = 9
        Assert.Equal(0x0909u, _helper.GetReg("EAX") & 0xFFFF);
    }

    [Fact]
    public void DAA_ShouldAdjustAfterBcdAddition()
    {
        // DAA - Decimal Adjust AL After Addition
        // Opcode: 27
        
        // Arrange: AL=0x0F (would be 15 in BCD, but invalid as packed BCD digit)
        // After adding 0x09 + 0x06 in binary, we get 0x0F
        // DAA should adjust this to 0x15 with AF set
        _helper.SetReg("EAX", 0x0000000F);
        _helper.WriteCode(0x27); // DAA

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // 0x0F + 0x06 = 0x15 (low nibble > 9, so add 6)
        Assert.Equal(0x15u, _helper.GetReg("EAX") & 0xFF);
    }

    [Fact]
    public void DAA_WithCarry_ShouldAdjustHighNibble()
    {
        // Arrange: AL=0xA5 (high nibble > 9)
        _helper.SetReg("EAX", 0x000000A5);
        _helper.WriteCode(0x27); // DAA

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // High nibble 0xA > 9, so add 0x60
        // 0xA5 + 0x60 = 0x05 (with carry)
        Assert.Equal(0x05u, _helper.GetReg("EAX") & 0xFF);
        Assert.True(_helper.IsFlagSet(CpuFlag.Cf), "CF should be set for carry out");
    }

    [Fact]
    public void DAS_ShouldAdjustAfterBcdSubtraction()
    {
        // DAS - Decimal Adjust AL After Subtraction
        // Opcode: 2F
        
        // Arrange: AL=0x0A (would occur after subtracting in BCD)
        // If we subtracted and got 0x0A, DAS adjusts it
        _helper.SetReg("EAX", 0x0000000A);
        _helper.WriteCode(0x2F); // DAS

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // Low nibble 0xA > 9, so subtract 6
        // 0x0A - 0x06 = 0x04
        Assert.Equal(0x04u, _helper.GetReg("EAX") & 0xFF);
    }

    [Fact]
    public void SLDT_ToMemory_ShouldStoreZero()
    {
        // SLDT - Store Local Descriptor Table Register
        // Opcode: 0F 00 /0
        // In flat memory model, this should store 0
        
        // Arrange: Write to memory at address 0x1000
        _helper.SetReg("EBX", 0x00001000);
        // SLDT [EBX] - 0F 00 03
        _helper.WriteCode(0x0F, 0x00, 0x03);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // Should write 0 to [EBX]
        Assert.Equal(0x0000u, _helper.Memory.Read16(0x1000));
    }

    [Fact]
    public void SLDT_ToRegister_ShouldSetZero()
    {
        // Arrange
        _helper.SetReg("EAX", 0xFFFFFFFF);
        // SLDT EAX - 0F 00 C0
        _helper.WriteCode(0x0F, 0x00, 0xC0);

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // Should set EAX to 0
        Assert.Equal(0x00000000u, _helper.GetReg("EAX"));
    }

    [Fact]
    public void ARPL_ShouldClearZeroFlag()
    {
        // ARPL - Adjust RPL Field of Segment Selector
        // Opcode: 63 /r
        // In flat memory model, always reports no adjustment (ZF=0)
        
        // Arrange: Set ZF first
        _helper.SetReg("EAX", 0x00000000);
        _helper.SetReg("EBX", 0x00000000);
        // First do a CMP to set ZF
        _helper.WriteCode(0x39, 0xC0); // CMP EAX, EAX (sets ZF)
        _helper.ExecuteInstruction();
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set initially");

        // Now test ARPL
        _helper.WriteCode(0x63, 0xD8); // ARPL AX, BX

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // ARPL should clear ZF (no adjustment made)
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be cleared by ARPL");
    }

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
