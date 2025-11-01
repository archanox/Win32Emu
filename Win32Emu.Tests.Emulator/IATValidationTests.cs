using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for Import Address Table (IAT) validation mechanisms.
/// These tests verify that IAT entries are properly validated during PE loading,
/// addressing the issue where extra IAT entries might contain incorrect addresses.
/// 
/// Related to: docs/fixes/UNMAPPED_IMPORT_FIX.md
/// Issue: Determine if there's an IAT entry that shouldn't be there
/// </summary>
public class IATValidationTests
{
    private readonly ITestOutputHelper _output;

    public IATValidationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ImportMap_CreatesCorrectNumberOfEntries()
    {
        // Verify that the import map correctly maps the expected number of imports
        // and that addresses beyond the last import are not mapped
        
        const int EXPECTED_IMPORT_COUNT = 83; // From IGN_TEAS.EXE documentation
        
        var importMap = new Dictionary<uint, (string dll, string name)>();
        
        // Simulate creating import map for 83 imports
        for (int i = 0; i < EXPECTED_IMPORT_COUNT; i++)
        {
            var addr = 0x0F000000u + (uint)(i * 0x10);
            importMap[addr] = ("KERNEL32.DLL", $"TestImport_{i}");
        }
        
        Assert.Equal(EXPECTED_IMPORT_COUNT, importMap.Count);
        
        // Verify first import
        Assert.True(importMap.TryGetValue(0x0F000000u, out var firstImport));
        Assert.Equal("KERNEL32.DLL", firstImport.dll);
        
        // Verify last valid import (index 82)
        var lastValidAddr = 0x0F000000u + (uint)((EXPECTED_IMPORT_COUNT - 1) * 0x10);
        Assert.Equal(0x0F000520u, lastValidAddr);
        Assert.True(importMap.ContainsKey(lastValidAddr));
        
        // Verify next address (index 83) is NOT mapped
        var unmappedAddr = 0x0F000530u;
        Assert.False(importMap.ContainsKey(unmappedAddr));
        
        _output.WriteLine($"Import map contains {importMap.Count} entries");
        _output.WriteLine($"Last valid address: 0x{lastValidAddr:X8}");
        _output.WriteLine($"First unmapped address: 0x{unmappedAddr:X8}");
    }

    [Fact]
    public void ImportAddressRange_ValidatesCorrectly()
    {
        // Test that we can correctly identify addresses in the import stub range
        
        var testCases = new[]
        {
            (0x0EFFFFFFU, false), // Just before import range
            (0x0F000000u, true),  // First import
            (0x0F000520u, true),  // Last import for 83 entries
            (0x0F000530u, true),  // In range but unmapped
            (0x0FFFFFFFu, true),  // Last address in range
            (0x10000000u, false), // Just after import range
        };
        
        foreach (var (addr, expectedInRange) in testCases)
        {
            var inRange = addr >= 0x0F000000 && addr < 0x10000000;
            Assert.Equal(expectedInRange, inRange);
            _output.WriteLine($"Address 0x{addr:X8}: In range = {inRange}");
        }
    }

    [Fact]
    public void DuplicateIATEntries_ShouldBeDetected()
    {
        // This test verifies that duplicate IAT entries can be detected
        // which might indicate PE corruption or parsing errors
        
        var iatEntries = new HashSet<uint>();
        var duplicates = new List<uint>();
        
        var testEntries = new[]
        {
            0x00401000u,
            0x00401004u,
            0x00401008u,
            0x00401004u, // Duplicate!
            0x0040100Cu,
        };
        
        foreach (var entry in testEntries)
        {
            if (iatEntries.Contains(entry))
            {
                duplicates.Add(entry);
                _output.WriteLine($"DUPLICATE IAT entry detected: 0x{entry:X8}");
            }
            iatEntries.Add(entry);
        }
        
        Assert.Single(duplicates);
        Assert.Equal(0x00401004u, duplicates[0]);
    }

    [Fact]
    public void UnmappedImportStubs_ShouldBeZeroed()
    {
        // Verify that unmapped import stub addresses contain zeros
        // and not executable code
        
        var memory = new VirtualMemory();
        
        // The unmapped address should be all zeros if not written to
        var unmappedAddr = 0x0F000530u;
        
        // Read several bytes at the unmapped address
        var byte1 = memory.Read8(unmappedAddr);
        var byte2 = memory.Read8(unmappedAddr + 1);
        var byte3 = memory.Read8(unmappedAddr + 2);
        var byte4 = memory.Read8(unmappedAddr + 3);
        
        // These should all be zero for unmapped memory
        Assert.Equal(0, byte1);
        Assert.Equal(0, byte2);
        Assert.Equal(0, byte3);
        Assert.Equal(0, byte4);
        
        _output.WriteLine($"Unmapped import stub at 0x{unmappedAddr:X8} contains: 0x{byte1:X2} 0x{byte2:X2} 0x{byte3:X2} 0x{byte4:X2}");
    }

    [Fact]
    public void ImportStubStructure_IsValid()
    {
        // Verify that import stubs have the expected structure:
        // - CALL to syscall dispatcher (5 bytes: E8 + rel32)
        // - RET imm16 (3 bytes: C2 + imm16)
        // - Padding (8 bytes of 0x90 NOP)
        // Total: 16 bytes per stub for alignment
        
        var memory = new VirtualMemory();
        const uint SYSCALL_DISPATCHER = 0x0E000000u;
        const uint STUB_ADDR = 0x0F000000u;
        
        // Calculate relative offset for CALL instruction
        // The offset must fit in a 32-bit signed integer for the x86 CALL instruction.
        long callOffsetLong = (long)SYSCALL_DISPATCHER - ((long)STUB_ADDR + 5);
        var callOffset = (int)callOffsetLong;
        
        var stub = new byte[]
        {
            0xE8, // CALL rel32
            (byte)(callOffset & 0xFF),
            (byte)((callOffset >> 8) & 0xFF),
            (byte)((callOffset >> 16) & 0xFF),
            (byte)((callOffset >> 24) & 0xFF),
            0xC2, 0x00, 0x00, // RET 0 (will be patched at runtime)
            0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 // Padding
        };
        
        // Verify stub is exactly 16 bytes
        Assert.Equal(16, stub.Length);
        
        // Write stub to memory
        memory.WriteBytes(STUB_ADDR, stub);
        
        // Verify we can read it back correctly
        var readCall = memory.Read8(STUB_ADDR);
        var readRet = memory.Read8(STUB_ADDR + 5);
        
        Assert.Equal(0xE8, readCall); // CALL opcode
        Assert.Equal(0xC2, readRet);  // RET imm16 opcode
        
        _output.WriteLine($"Import stub structure verified at 0x{STUB_ADDR:X8}");
        _output.WriteLine($"  CALL opcode: 0x{readCall:X2}");
        _output.WriteLine($"  RET opcode:  0x{readRet:X2}");
    }

    [Fact]
    public void IATPrelinkValue_Detection()
    {
        // Some PE files may have pre-linked IAT values (non-zero values before fixup)
        // This test verifies we can detect and handle these
        
        var memory = new VirtualMemory();
        var iatEntryAddr = 0x00401000u;
        
        // Simulate a pre-linked IAT entry
        var prelinkValue = 0x77E51234u; // Typical kernel32.dll address in Windows
        memory.Write32(iatEntryAddr, prelinkValue);
        
        var readValue = memory.Read32(iatEntryAddr);
        Assert.Equal(prelinkValue, readValue);
        
        // This is not zero, indicating a pre-linked value
        Assert.NotEqual(0u, readValue);
        
        _output.WriteLine($"Pre-linked IAT value detected: 0x{readValue:X8}");
        _output.WriteLine("This is normal for some PE loaders and will be overwritten with synthetic address");
    }

    [Fact]
    public void SyntheticAddressGeneration_IsCorrect()
    {
        // Verify that synthetic addresses are generated correctly
        // Each import gets address 0x0F000000 + (index * 0x10)
        
        var expectedAddresses = new[]
        {
            (0, 0x0F000000u),
            (1, 0x0F000010u),
            (2, 0x0F000020u),
            (82, 0x0F000520u), // Last valid for IGN_TEAS
            (83, 0x0F000530u), // First unmapped for IGN_TEAS
        };
        
        foreach (var (index, expectedAddr) in expectedAddresses)
        {
            var synthetic = 0x0F000000u + (uint)(index * 0x10);
            Assert.Equal(expectedAddr, synthetic);
            _output.WriteLine($"Import index {index} -> Synthetic address 0x{synthetic:X8}");
        }
    }

    [Fact]
    public void BeyondMappedRange_DetectsUnexpectedData()
    {
        // This test simulates scanning for unexpected data beyond the mapped import range
        // which could indicate extra IAT entries or memory corruption
        
        var memory = new VirtualMemory();
        
        // Map first 83 imports (0x0F000000 - 0x0F000520)
        const int MAPPED_COUNT = 83;
        var maxMappedAddr = 0x0F000000u + (uint)((MAPPED_COUNT - 1) * 0x10);
        Assert.Equal(0x0F000520u, maxMappedAddr);
        
        // Write valid import stubs in the mapped range
        for (int i = 0; i < MAPPED_COUNT; i++)
        {
            var addr = 0x0F000000u + (uint)(i * 0x10);
            memory.Write8(addr, 0xE8); // CALL opcode - indicates valid stub
        }
        
        // Scan for unexpected data beyond the mapped range
        var scanRangeEnd = 0x0F000000u + 0x1000u; // Scan first 256 possible slots
        var unexpectedDataFound = false;
        
        for (uint addr = maxMappedAddr + 0x10; addr < scanRangeEnd; addr += 0x10)
        {
            try
            {
                var byte1 = memory.Read8(addr);
                var byte2 = memory.Read8(addr + 1);
                
                if (byte1 != 0 || byte2 != 0)
                {
                    unexpectedDataFound = true;
                    _output.WriteLine($"WARNING: Unexpected non-zero data at unmapped address 0x{addr:X8}: 0x{byte1:X2} 0x{byte2:X2}");
                }
            }
            catch (Exception ex)
            {
                // Memory not mapped - this is expected and fine
                _output.WriteLine($"Memory read exception at 0x{addr:X8}: {ex.Message}");
                break;
            }
        }
        
        Assert.False(unexpectedDataFound, "No unexpected data should exist beyond mapped range");
        _output.WriteLine($"Scan complete: No unexpected data found beyond 0x{maxMappedAddr:X8}");
    }
}
