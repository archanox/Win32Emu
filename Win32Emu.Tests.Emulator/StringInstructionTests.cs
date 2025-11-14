using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for x86 string instructions (MOVS, STOS, LODS, INS, OUTS, etc.)
/// </summary>
public class StringInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public StringInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void INSB_ShouldWriteByteToMemory()
    {
        // Arrange: INSB (6C)
        // Sets up EDI to point to memory and executes INSB
        // Since I/O ports are stubbed, it should write 0
        _helper.SetReg("EDI", 0x00001000);
        _helper.WriteCode(0x6C); // INSB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00u, _helper.ReadMemory8(0x00001000));
        Assert.Equal(0x00001001u, _helper.GetReg("EDI")); // EDI should increment by 1 (DF=0)
    }

    [Fact]
    public void INSB_WithDF_ShouldDecrementEDI()
    {
        // Arrange: INSB with DF flag set
        _helper.SetReg("EDI", 0x00001000);
        _helper.SetFlag(CpuFlag.Df, true);
        _helper.WriteCode(0x6C); // INSB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00u, _helper.ReadMemory8(0x00001000));
        Assert.Equal(0x00000FFFu, _helper.GetReg("EDI")); // EDI should decrement by 1 (DF=1)
    }

    [Fact]
    public void INSW_ShouldWriteWordToMemory()
    {
        // Arrange: INSW (6D with operand size prefix)
        // 66 6D = INSW (0x66 is the operand-size override prefix)
        _helper.SetReg("EDI", 0x00001000);
        _helper.WriteCode(0x66, 0x6D); // INSW

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x0000u, _helper.ReadMemory16(0x00001000));
        Assert.Equal(0x00001002u, _helper.GetReg("EDI")); // EDI should increment by 2
    }

    [Fact]
    public void INSD_ShouldWriteDwordToMemory()
    {
        // Arrange: INSD (6D)
        _helper.SetReg("EDI", 0x00001000);
        _helper.WriteCode(0x6D); // INSD

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000000u, _helper.ReadMemory32(0x00001000));
        Assert.Equal(0x00001004u, _helper.GetReg("EDI")); // EDI should increment by 4
    }

    [Fact]
    public void REP_INSB_ShouldWriteMultipleBytes()
    {
        // Arrange: REP INSB (F3 6C)
        _helper.SetReg("EDI", 0x00001000);
        _helper.SetReg("ECX", 0x00000005); // Write 5 bytes
        _helper.WriteCode(0xF3, 0x6C); // REP INSB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        for (uint i = 0; i < 5; i++)
        {
            Assert.Equal(0x00u, _helper.ReadMemory8(0x00001000 + i));
        }
        Assert.Equal(0x00001005u, _helper.GetReg("EDI")); // EDI should increment by 5
        Assert.Equal(0x00000000u, _helper.GetReg("ECX")); // ECX should be 0 after REP
    }

    [Fact]
    public void OUTSB_ShouldReadByteFromMemory()
    {
        // Arrange: OUTSB (6E)
        // Sets up ESI to point to memory and executes OUTSB
        // Since I/O ports are stubbed, it should just read and advance ESI
        _helper.SetReg("ESI", 0x00001000);
        _helper.WriteMemory32(0x00001000, 0x12345678); // Write test data
        _helper.WriteCode(0x6E); // OUTSB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00001001u, _helper.GetReg("ESI")); // ESI should increment by 1 (DF=0)
    }

    [Fact]
    public void OUTSB_WithDF_ShouldDecrementESI()
    {
        // Arrange: OUTSB with DF flag set
        _helper.SetReg("ESI", 0x00001000);
        _helper.WriteMemory32(0x00001000, 0x12345678);
        _helper.SetFlag(CpuFlag.Df, true);
        _helper.WriteCode(0x6E); // OUTSB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00000FFFu, _helper.GetReg("ESI")); // ESI should decrement by 1 (DF=1)
    }

    [Fact]
    public void OUTSW_ShouldReadWordFromMemory()
    {
        // Arrange: OUTSW (66 6F)
        _helper.SetReg("ESI", 0x00001000);
        _helper.WriteMemory32(0x00001000, 0x12345678);
        _helper.WriteCode(0x66, 0x6F); // OUTSW

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00001002u, _helper.GetReg("ESI")); // ESI should increment by 2
    }

    [Fact]
    public void OUTSD_ShouldReadDwordFromMemory()
    {
        // Arrange: OUTSD (6F)
        _helper.SetReg("ESI", 0x00001000);
        _helper.WriteMemory32(0x00001000, 0x12345678);
        _helper.WriteCode(0x6F); // OUTSD

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00001004u, _helper.GetReg("ESI")); // ESI should increment by 4
    }

    [Fact]
    public void REP_OUTSB_ShouldReadMultipleBytes()
    {
        // Arrange: REP OUTSB (F3 6E)
        _helper.SetReg("ESI", 0x00001000);
        _helper.SetReg("ECX", 0x00000005); // Read 5 bytes
        _helper.WriteMemory32(0x00001000, 0x12345678);
        _helper.WriteCode(0xF3, 0x6E); // REP OUTSB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x00001005u, _helper.GetReg("ESI")); // ESI should increment by 5
        Assert.Equal(0x00000000u, _helper.GetReg("ECX")); // ECX should be 0 after REP
    }

    [Fact]
    public void REPNZ_SCASB_ShouldFindNullTerminator()
    {
        // Arrange: REPNZ SCASB (F2 AE) - This is the instruction that was causing the infinite loop
        // Write a string "Hello\0" to memory
        _helper.Memory.Write8(0x00001000, (byte)'H');
        _helper.Memory.Write8(0x00001001, (byte)'e');
        _helper.Memory.Write8(0x00001002, (byte)'l');
        _helper.Memory.Write8(0x00001003, (byte)'l');
        _helper.Memory.Write8(0x00001004, (byte)'o');
        _helper.Memory.Write8(0x00001005, 0x00); // Null terminator
        
        _helper.SetReg("EDI", 0x00001000);
        _helper.SetReg("ECX", 0xFFFFFFFF); // Maximum value - this was causing the hang
        _helper.SetReg("EAX", 0x00); // Search for null terminator (AL = 0)
        _helper.WriteCode(0xF2, 0xAE); // REPNZ SCASB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // Should find null at position 5 (after 'o')
        // EDI should point to byte after the match (0x00001006)
        Assert.Equal(0x00001006u, _helper.GetReg("EDI"));
        // ECX should be decremented by number of comparisons (6 comparisons)
        // 0xFFFFFFFF - 6 = 0xFFFFFFF9
        Assert.Equal(0xFFFFFFF9u, _helper.GetReg("ECX"));
        // ZF should be set (match found)
        Assert.True(_helper.IsFlagSet(CpuFlag.Zf));
    }

    [Fact]
    public void REPNZ_SCASB_WithNoMatch_ShouldExhaustECX()
    {
        // Arrange: REPNZ SCASB with no null terminator
        // Write non-zero bytes
        for (uint i = 0; i < 10; i++)
        {
            _helper.Memory.Write8(0x00001000 + i, 0xFF);
        }
        
        _helper.SetReg("EDI", 0x00001000);
        _helper.SetReg("ECX", 0x0000000A); // Scan 10 bytes
        _helper.SetReg("EAX", 0x00); // Search for null terminator
        _helper.WriteCode(0xF2, 0xAE); // REPNZ SCASB

        // Act
        _helper.ExecuteInstruction();

        // Assert
        // Should scan all 10 bytes without finding a match
        Assert.Equal(0x0000100Au, _helper.GetReg("EDI")); // EDI += 10
        Assert.Equal(0x00000000u, _helper.GetReg("ECX")); // ECX exhausted
        // ZF should be clear (no match found)
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf));
    }

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
