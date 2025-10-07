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

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
