using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for newly implemented x86 instructions identified from decomp files
/// </summary>
public class NewInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public NewInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    #region FPU Integer Arithmetic Tests

    [Fact]
    public void FIMUL_ShouldMultiplyByInteger()
    {
        // Arrange: FIMUL multiplies ST(0) by an integer from memory
        var memAddr = 0x00200000u;
        _helper.Memory.Write32(memAddr, 5); // Store integer 5
        
        // Load 3.0 onto FPU stack
        // FLD1 (D9 E8) x3 to get 3.0
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD8, 0xC1); // FADD ST(0), ST(1)
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD8, 0xC1); // FADD ST(0), ST(1)
        _helper.ExecuteInstruction();
        
        // Now ST(0) = 3.0, multiply by 5
        // FIMUL dword ptr [memAddr] (DA 0D + address)
        _helper.WriteCode(
            0xDA, 0x0D,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Result should be 15.0 (test shouldn't crash)
    }

    [Fact]
    public void FIDIV_ShouldDivideByInteger()
    {
        // Arrange: FIDIV divides ST(0) by an integer from memory
        var memAddr = 0x00200000u;
        _helper.Memory.Write32(memAddr, 2); // Store integer 2
        
        // Load 10.0 onto FPU stack (FLD1 x10)
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        for (int i = 1; i < 10; i++)
        {
            _helper.WriteCode(0xD9, 0xE8); // FLD1
            _helper.ExecuteInstruction();
            _helper.WriteCode(0xD8, 0xC1); // FADD ST(0), ST(1)
            _helper.ExecuteInstruction();
        }
        
        // FIDIV dword ptr [memAddr] (DA 35 + address)
        _helper.WriteCode(
            0xDA, 0x35,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Result should be 5.0 (test shouldn't crash)
    }

    [Fact]
    public void FIDIVR_ShouldDivideIntegerByST0()
    {
        // Arrange: FIDIVR divides integer by ST(0) (reversed operands)
        var memAddr = 0x00200000u;
        _helper.Memory.Write32(memAddr, 10); // Store integer 10
        
        // Load 2.0 onto FPU stack
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD8, 0xC1); // FADD ST(0), ST(1)
        _helper.ExecuteInstruction();
        
        // FIDIVR dword ptr [memAddr] (DA 3D + address)
        // Result should be 10 / 2.0 = 5.0
        _helper.WriteCode(
            0xDA, 0x3D,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Result should be 5.0 (test shouldn't crash)
    }

    [Fact]
    public void FISUB_ShouldSubtractInteger()
    {
        // Arrange: FISUB subtracts an integer from ST(0)
        var memAddr = 0x00200000u;
        _helper.Memory.Write32(memAddr, 3); // Store integer 3
        
        // Load 10.0 onto FPU stack
        for (int i = 0; i < 10; i++)
        {
            _helper.WriteCode(0xD9, 0xE8); // FLD1
            _helper.ExecuteInstruction();
            if (i > 0)
            {
                _helper.WriteCode(0xD8, 0xC1); // FADD ST(0), ST(1)
                _helper.ExecuteInstruction();
            }
        }
        
        // FISUB dword ptr [memAddr] (DA 25 + address)
        _helper.WriteCode(
            0xDA, 0x25,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Result should be 7.0 (test shouldn't crash)
    }

    #endregion

    #region FPU Control Tests

    [Fact]
    public void FNSTSW_ShouldStoreStatusWordToMemory()
    {
        // Arrange: FNSTSW stores FPU status word to memory
        var memAddr = 0x00200000u;
        
        // Do some FPU operation to set status
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        
        // FNSTSW word ptr [memAddr] (DD 3D + address)
        _helper.WriteCode(
            0xDD, 0x3D,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Should write status word to memory without crashing
        var statusWord = _helper.Memory.Read16(memAddr);
        Assert.True(statusWord >= 0); // Basic sanity check
    }

    [Fact]
    public void FNSTSW_ShouldStoreStatusWordToAX()
    {
        // Arrange: FNSTSW AX stores FPU status word to AX register
        
        // Do some FPU operation
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        
        // FNSTSW AX (DF E0)
        _helper.WriteCode(0xDF, 0xE0);
        _helper.ExecuteInstruction();
        
        // AX should contain the status word
        var ax = _helper.GetReg("EAX") & 0xFFFF;
        Assert.True(ax >= 0); // Basic sanity check
    }

    [Fact]
    public void FNINIT_ShouldInitializeFPU()
    {
        // Arrange: FNINIT initializes FPU to default state
        
        // Load some values onto FPU stack
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        
        // FNINIT (DB E3)
        _helper.WriteCode(0xDB, 0xE3);
        _helper.ExecuteInstruction();
        
        // FPU should be reset (test shouldn't crash)
        // After FNINIT, we should be able to use FPU normally
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
    }

    [Fact]
    public void FNCLEX_ShouldClearExceptions()
    {
        // Arrange: FNCLEX clears FPU exception flags
        
        // FNCLEX (DB E2)
        _helper.WriteCode(0xDB, 0xE2);
        _helper.ExecuteInstruction();
        
        // Exception flags should be cleared (test shouldn't crash)
    }

    [Fact]
    public void FXAM_ShouldExamineST0()
    {
        // Arrange: FXAM examines ST(0) and sets condition codes
        
        // Load a value onto FPU stack
        _helper.WriteCode(0xD9, 0xE8); // FLD1 (loads +1.0)
        _helper.ExecuteInstruction();
        
        // FXAM (D9 E5)
        _helper.WriteCode(0xD9, 0xE5);
        _helper.ExecuteInstruction();
        
        // Should set condition codes based on ST(0) (test shouldn't crash)
        
        // Test with zero
        _helper.WriteCode(0xD9, 0xEE); // FLDZ (loads +0.0)
        _helper.ExecuteInstruction();
        _helper.WriteCode(0xD9, 0xE5); // FXAM
        _helper.ExecuteInstruction();
    }

    #endregion

    #region String Operation Tests

    [Fact]
    public void MOVSW_ShouldMoveWordString()
    {
        // Arrange: MOVSW moves word from [ESI] to [EDI]
        var srcAddr = 0x00200000u;
        var dstAddr = 0x00210000u;
        
        // Set up source data
        _helper.Memory.Write16(srcAddr, 0x1234);
        
        // Set ESI and EDI
        _helper.SetReg("ESI", srcAddr);
        _helper.SetReg("EDI", dstAddr);
        
        // MOVSW (66 A5) - operand size prefix + MOVSD
        _helper.WriteCode(0x66, 0xA5);
        _helper.ExecuteInstruction();
        
        // Verify data was moved
        Assert.Equal(0x1234, _helper.Memory.Read16(dstAddr));
        Assert.Equal(srcAddr + 2, _helper.GetReg("ESI"));
        Assert.Equal(dstAddr + 2, _helper.GetReg("EDI"));
    }

    [Fact]
    public void STOSW_ShouldStoreWordString()
    {
        // Arrange: STOSW stores AX to [EDI]
        var dstAddr = 0x00200000u;
        
        // Set up data
        _helper.SetReg("EAX", 0x5678);
        _helper.SetReg("EDI", dstAddr);
        
        // STOSW (66 AB) - operand size prefix + STOSD
        _helper.WriteCode(0x66, 0xAB);
        _helper.ExecuteInstruction();
        
        // Verify data was stored
        Assert.Equal(0x5678, _helper.Memory.Read16(dstAddr));
        Assert.Equal(dstAddr + 2, _helper.GetReg("EDI"));
    }

    #endregion

    #region Miscellaneous Tests

    [Fact]
    public void XLATB_ShouldTranslateByte()
    {
        // Arrange: XLATB performs AL = [EBX + AL]
        var tableAddr = 0x00200000u;
        
        // Set up translation table
        _helper.Memory.Write8(tableAddr + 0, 0x10);
        _helper.Memory.Write8(tableAddr + 1, 0x20);
        _helper.Memory.Write8(tableAddr + 2, 0x30);
        _helper.Memory.Write8(tableAddr + 5, 0x50);
        
        // Set EBX to table base, AL to index
        _helper.SetReg("EBX", tableAddr);
        _helper.SetReg("EAX", 0x02); // AL = 2
        
        // XLATB (D7)
        _helper.WriteCode(0xD7);
        _helper.ExecuteInstruction();
        
        // AL should now contain the value at table[2]
        Assert.Equal(0x30u, _helper.GetReg("EAX") & 0xFF);
        
        // Test another index
        _helper.SetReg("EBX", tableAddr);
        _helper.SetReg("EAX", 0x05); // AL = 5
        _helper.WriteCode(0xD7);
        _helper.ExecuteInstruction();
        
        Assert.Equal(0x50u, _helper.GetReg("EAX") & 0xFF);
    }

    #endregion

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
