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

    #region REP Instruction Batching Tests
    
    /// <summary>
    /// Tests that large REP operations complete correctly across multiple batches.
    /// REP_BATCH_SIZE is 1000 for WASM and 100000 for native, but we test with values
    /// larger than WASM batch size to ensure batching works correctly.
    /// </summary>
    [Fact]
    public void REP_STOSD_LargeCount_ShouldCompleteCorrectly()
    {
        // Arrange: REP STOSD (F3 AB) with 10000 iterations
        // This exceeds WASM_BATCH_SIZE (1000) so it would require 10 batches on WASM
        const uint iterationCount = 10000;
        const uint fillValue = 0xDEADBEEF;
        
        _helper.SetReg("EDI", 0x00001000);
        _helper.SetReg("ECX", iterationCount);
        _helper.SetReg("EAX", fillValue);
        _helper.WriteCode(0xF3, 0xAB); // REP STOSD
        
        // Act: Execute multiple SingleStep calls until REP completes
        // The batching implementation keeps EIP at the REP instruction until ECX reaches 0
        uint maxIterations = 100; // Prevent infinite loop in case of bugs
        uint iterations = 0;
        while (_helper.GetReg("ECX") > 0 && iterations < maxIterations)
        {
            _helper.ExecuteInstruction();
            iterations++;
            // No need to reset EIP - the batching implementation doesn't advance it
        }
        
        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("ECX")); // ECX should be 0
        Assert.Equal(0x00001000u + (iterationCount * 4), _helper.GetReg("EDI")); // EDI should advance
        
        // Verify memory was filled correctly (check first, middle, and last DWORDs)
        Assert.Equal(fillValue, _helper.ReadMemory32(0x00001000)); // First
        Assert.Equal(fillValue, _helper.ReadMemory32(0x00001000 + 5000 * 4)); // Middle
        Assert.Equal(fillValue, _helper.ReadMemory32(0x00001000 + (iterationCount - 1) * 4)); // Last
    }

    [Fact]
    public void REP_MOVSD_LargeCount_ShouldCopyMemoryCorrectly()
    {
        // Arrange: REP MOVSD (F3 A5) with 5000 iterations
        const uint iterationCount = 5000;
        
        // Initialize source memory with pattern
        for (uint i = 0; i < iterationCount; i++)
        {
            _helper.WriteMemory32(0x00010000 + i * 4, 0x12340000 + i);
        }
        
        _helper.SetReg("ESI", 0x00010000); // Source
        _helper.SetReg("EDI", 0x00020000); // Destination
        _helper.SetReg("ECX", iterationCount);
        _helper.WriteCode(0xF3, 0xA5); // REP MOVSD
        
        // Act: Execute until complete
        uint maxIterations = 100;
        uint iterations = 0;
        while (_helper.GetReg("ECX") > 0 && iterations < maxIterations)
        {
            _helper.ExecuteInstruction();
            iterations++;
        }
        
        // Assert
        Assert.Equal(0x00000000u, _helper.GetReg("ECX"));
        Assert.Equal(0x00010000u + iterationCount * 4, _helper.GetReg("ESI"));
        Assert.Equal(0x00020000u + iterationCount * 4, _helper.GetReg("EDI"));
        
        // Verify copy was correct
        Assert.Equal(0x12340000u, _helper.ReadMemory32(0x00020000)); // First
        Assert.Equal(0x12340000u + 2500, _helper.ReadMemory32(0x00020000 + 2500 * 4)); // Middle (2500)
        Assert.Equal(0x12340000u + iterationCount - 1, _helper.ReadMemory32(0x00020000 + (iterationCount - 1) * 4)); // Last
    }

    [Fact]
    public void REP_STOSB_RegisterState_ShouldBePreservedBetweenBatches()
    {
        // Arrange: REP STOSB (F3 AA) - tests that ECX, EDI are correctly preserved
        const uint iterationCount = 5000;
        const byte fillValue = 0xAA;
        
        _helper.SetReg("EDI", 0x00001000);
        _helper.SetReg("ECX", iterationCount);
        _helper.SetReg("EAX", fillValue);
        _helper.WriteCode(0xF3, 0xAA); // REP STOSB
        
        // Act: Execute and track register state between batches
        uint maxIterations = 100;
        uint iterations = 0;
        uint lastEcx = iterationCount;
        uint lastEdi = 0x00001000;
        
        while (_helper.GetReg("ECX") > 0 && iterations < maxIterations)
        {
            _helper.ExecuteInstruction();
            iterations++;
            
            uint currentEcx = _helper.GetReg("ECX");
            uint currentEdi = _helper.GetReg("EDI");
            
            // Verify ECX is decreasing (or at 0)
            Assert.True(currentEcx <= lastEcx, $"ECX should decrease: was {lastEcx}, now {currentEcx}");
            
            // Verify EDI is increasing
            Assert.True(currentEdi >= lastEdi, $"EDI should increase: was {lastEdi}, now {currentEdi}");
            
            // Verify EDI and ECX are consistent
            uint bytesProcessed = iterationCount - currentEcx;
            Assert.Equal(0x00001000u + bytesProcessed, currentEdi);
            
            lastEcx = currentEcx;
            lastEdi = currentEdi;
        }
        
        // Final assertions
        Assert.Equal(0x00000000u, _helper.GetReg("ECX"));
        Assert.Equal(0x00001000u + iterationCount, _helper.GetReg("EDI"));
        
        // Verify memory filled correctly
        Assert.Equal(fillValue, _helper.ReadMemory8(0x00001000));
        Assert.Equal(fillValue, _helper.ReadMemory8(0x00001000 + iterationCount - 1));
    }

    [Fact]
    public void REPE_CMPSB_LargeCount_ShouldFindMismatch()
    {
        // Arrange: REPE CMPSB (F3 A6) - compare with mismatch at position 3000
        const uint stringLength = 5000;
        const uint mismatchPos = 3000;
        
        // Initialize identical strings with one mismatch
        for (uint i = 0; i < stringLength; i++)
        {
            byte value = (byte)(i & 0xFF);
            _helper.Memory.Write8(0x00010000 + i, value);
            _helper.Memory.Write8(0x00020000 + i, value);
        }
        // Insert mismatch
        _helper.Memory.Write8(0x00010000 + mismatchPos, 0xFF);
        _helper.Memory.Write8(0x00020000 + mismatchPos, 0x00);
        
        _helper.SetReg("ESI", 0x00010000);
        _helper.SetReg("EDI", 0x00020000);
        _helper.SetReg("ECX", stringLength);
        _helper.WriteCode(0xF3, 0xA6); // REPE CMPSB
        
        // Act: Execute until mismatch found or ECX exhausted
        uint maxIterations = 100;
        uint iterations = 0;
        while (_helper.GetReg("ECX") > 0 && iterations < maxIterations)
        {
            _helper.ExecuteInstruction();
            iterations++;
            // REPE stops when ZF becomes 0 (mismatch) - check after execution
            if (!_helper.IsFlagSet(CpuFlag.Zf))
            {
                break; // Mismatch found
            }
        }
        
        // Assert: Should stop at mismatch
        // After finding mismatch at position 3000, ECX = 5000 - 3001 = 1999
        // (we compared 3001 bytes: 0-2999 equal, 3000 not equal)
        Assert.Equal(stringLength - mismatchPos - 1, _helper.GetReg("ECX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.Zf)); // ZF clear indicates mismatch found
    }
    
    #endregion

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
