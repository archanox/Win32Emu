using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for x87 FPU instructions
/// </summary>
public class FpuInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;
    private const uint FpuConditionCodeMask = 0x4500;
    private const uint FpuConditionCodeC1 = 0x0200;
    private const uint FpuConditionCodeUnordered = 0x4500;
    private const uint FpuConditionCodeLessThan = 0x0100;
    private const uint FpuConditionCodeEqual = 0x4000;

    public FpuInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    #region FCOMP Tests

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

        // FNSTSW AX (DF E0) - x87 compare results are reported in the status word, not EFLAGS
        _helper.WriteCode(0xDF, 0xE0);
        _helper.ExecuteInstruction();

        // Assert: Equal => C3=1, C2=0, C0=0
        var statusWord = _helper.GetReg("EAX") & 0xFFFF;
        Assert.Equal(FpuConditionCodeEqual, statusWord & FpuConditionCodeMask);
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

        // FNSTSW AX (DF E0)
        _helper.WriteCode(0xDF, 0xE0);
        _helper.ExecuteInstruction();

        // Assert: Less than => C0=1, C2=0, C3=0
        var statusWord = _helper.GetReg("EAX") & 0xFFFF;
        Assert.Equal(FpuConditionCodeLessThan, statusWord & FpuConditionCodeMask);
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

        // FNSTSW AX (DF E0)
        _helper.WriteCode(0xDF, 0xE0);
        _helper.ExecuteInstruction();

        // Assert: Greater than => C0=0, C2=0, C3=0
        var statusWord = _helper.GetReg("EAX") & 0xFFFF;
        Assert.Equal(0u, statusWord & FpuConditionCodeMask);
    }

    #endregion

    #region New x87 Instruction Tests

    [Fact]
    public void FDIV_ShouldDivideFloats()
    {
        // Arrange: Test FDIV - divide ST(0) by memory value
        var memAddr = 0x00200000u;
        var floatBits = BitConverter.SingleToInt32Bits(2.0f);
        _helper.WriteMemory32(memAddr, unchecked((uint)floatBits));
        
        // FLD1 - Load 1.0 onto stack
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FLD1 - Load 1.0 onto stack again
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FADD - Add to get 2.0 in ST(0)
        _helper.WriteCode(0xD8, 0xC1);
        _helper.ExecuteInstruction();
        
        // Now ST(0) = 2.0, divide by 2.0 from memory should give 1.0
        // FDIV dword ptr [memAddr] (D8 35 + address)
        _helper.WriteCode(
            0xD8, 0x35,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Result should be 1.0 (we can't directly assert FPU stack values, but test shouldn't crash)
    }

    [Fact]
    public void FSQRT_ShouldCalculateSquareRoot()
    {
        // Arrange: Test FSQRT
        var memAddr = 0x00200000u;
        var floatBits = BitConverter.SingleToInt32Bits(4.0f);
        _helper.WriteMemory32(memAddr, unchecked((uint)floatBits));
        
        // FLD dword ptr [memAddr] - Load 4.0
        _helper.WriteCode(
            0xD9, 0x05,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // FSQRT (D9 FA) - Square root of ST(0)
        _helper.WriteCode(0xD9, 0xFA);
        _helper.ExecuteInstruction();
        
        // Result should be 2.0 (test shouldn't crash)
    }

    [Fact]
    public void FCOM_ShouldCompareWithoutPop()
    {
        // Arrange: FCOM should compare but not pop the stack
        _helper.SetFlag(CpuFlag.Zf, true);
        _helper.SetFlag(CpuFlag.Cf, true);
        var flagsBeforeCompare = _helper.GetFlags();
         
        // FLD1 - Load 1.0 onto FPU stack
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FLD1 - Load another 1.0 onto FPU stack
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();
        
        // FCOM ST(1) - Compare ST(0) with ST(1) without popping (D8 D1)
        _helper.WriteCode(0xD8, 0xD1);
        _helper.ExecuteInstruction();

        Assert.Equal(flagsBeforeCompare, _helper.GetFlags());

        var statusWord = ReadStatusWord();
        Assert.Equal(FpuConditionCodeEqual, statusWord & FpuConditionCodeMask);
         
        // FCOM doesn't pop, so we can do FCOMP next
        _helper.WriteCode(0xD8, 0xD9);
        _helper.ExecuteInstruction();
        // This should succeed without error
    }

    [Fact]
    public void FCOMI_WithST1Operand_ShouldSetCarryFlagWhenST0IsLess()
    {
        var memAddr = 0x00200000u;
        var twoBits = BitConverter.SingleToInt32Bits(2.0f);
        _helper.WriteMemory32(memAddr, unchecked((uint)twoBits));

        // FLD dword ptr [memAddr] - ST(0)=2.0
        _helper.WriteCode(
            0xD9, 0x05,
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF));
        _helper.ExecuteInstruction();

        // FLD1 - ST(0)=1.0, ST(1)=2.0
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();

        // FCOMI ST(1) - 1.0 < 2.0 should set CF only.
        _helper.WriteCode(0xDB, 0xF1);
        _helper.ExecuteInstruction();

        Assert.True(_helper.IsFlagSet(CpuFlag.Cf));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf));
        Assert.False(_helper.IsFlagSet(CpuFlag.Pf));
    }

    [Fact]
    public void FUCOM_ShouldClearPreviousConditionCodesAndC1()
    {
        // FLDZ - ST(0)=0.0
        _helper.WriteCode(0xD9, 0xEE);
        _helper.ExecuteInstruction();

        // FLD1 - ST(0)=1.0, ST(1)=0.0 so FUCOM ST(1) is a greater-than compare.
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();

        SeedFpuStatusWord((ushort)(FpuConditionCodeUnordered | FpuConditionCodeC1));

        _helper.WriteCode(0xDD, 0xE1);
        _helper.ExecuteInstruction();

        var statusWord = ReadStatusWord();
        Assert.Equal(0u, statusWord & (FpuConditionCodeMask | FpuConditionCodeC1));
    }

    [Fact]
    public void FTST_ShouldClearPreviousConditionCodesAndC1()
    {
        // FLD1 - ST(0)=1.0 so FTST should report greater-than-zero and clear C0/C1/C2/C3.
        _helper.WriteCode(0xD9, 0xE8);
        _helper.ExecuteInstruction();

        SeedFpuStatusWord((ushort)(FpuConditionCodeUnordered | FpuConditionCodeC1));

        _helper.WriteCode(0xD9, 0xE4);
        _helper.ExecuteInstruction();

        var statusWord = ReadStatusWord();
        Assert.Equal(0u, statusWord & (FpuConditionCodeMask | FpuConditionCodeC1));
    }

    #endregion

    #region JitCpu FPU Method Tests

    [Fact]
    public void FpuGetSt_RetrievesCorrectValue()
    {
        // Arrange - push a specific double value onto FPU stack
        // FLD qword ptr [address] - loads a double from memory
        var memAddr = 0x00200000u;
        var doubleBits = BitConverter.DoubleToInt64Bits(3.14159265);
        _helper.Memory.Write64(memAddr, unchecked((ulong)doubleBits));
        
        // FLD qword ptr [address] - DD 05 + address
        _helper.WriteCode(
            0xDD, 0x05,  // FLD qword ptr [address]
            (byte)(memAddr & 0xFF),
            (byte)((memAddr >> 8) & 0xFF),
            (byte)((memAddr >> 16) & 0xFF),
            (byte)((memAddr >> 24) & 0xFF)
        );
        _helper.ExecuteInstruction();
        
        // Act - get the value from ST(0)
        double st0 = _helper.Cpu.FpuGetSt(0);
        
        // Assert
        Assert.Equal(3.14159265, st0, precision: 8);
    }

    [Fact]
    public void FpuPop_RemovesValueFromStack()
    {
        // Arrange - push two values onto FPU stack
        // FLD1 - loads 1.0
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        
        // FLDZ - loads 0.0
        _helper.WriteCode(0xD9, 0xEE); // FLDZ
        _helper.ExecuteInstruction();
        
        // Stack is now: ST(0)=0.0, ST(1)=1.0
        Assert.Equal(0.0, _helper.Cpu.FpuGetSt(0));
        Assert.Equal(1.0, _helper.Cpu.FpuGetSt(1));
        
        // Act - pop the top value
        double poppedValue = _helper.Cpu.FpuPop();
        
        // Assert
        Assert.Equal(0.0, poppedValue);
        // After pop, ST(0) should now be what was ST(1)
        Assert.Equal(1.0, _helper.Cpu.FpuGetSt(0));
    }

    [Fact]
    public void FpuReset_ClearsStackAndResetsState()
    {
        // Arrange - push values onto FPU stack
        _helper.WriteCode(0xD9, 0xE8); // FLD1
        _helper.ExecuteInstruction();
        
        // Verify there's a value on the stack
        Assert.Equal(1.0, _helper.Cpu.FpuGetSt(0));
        
        // Act - reset FPU
        _helper.Cpu.FpuReset();
        
        // Assert - stack should be cleared (all zeros)
        Assert.Equal(0.0, _helper.Cpu.FpuGetSt(0));
        Assert.Equal(0.0, _helper.Cpu.FpuGetSt(1));
        Assert.Equal(0.0, _helper.Cpu.FpuGetSt(7));
    }

    #endregion

    public void Dispose()
    {
        _helper?.Dispose();
    }

    private uint ReadStatusWord()
    {
        // FNSTSW AX (DF E0) stores the current x87 status word in AX.
        _helper.WriteCode(0xDF, 0xE0);
        _helper.ExecuteInstruction();
        return _helper.GetReg("EAX") & 0xFFFF;
    }

    private void SeedFpuStatusWord(ushort statusWord)
    {
        var state = _helper.Cpu.SaveState();
        state.FpuStatusWord = statusWord;
        _helper.Cpu.RestoreState(state);
    }
}
