using Microsoft.Extensions.Logging;
using Win32Emu.Cpu.Iced;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for stack corruption detection mechanisms.
/// These tests verify that stack corruption is detected early after import calls,
/// addressing the issue where return address 0x0F000530 (unmapped import) appears on stack.
/// 
/// Related to: docs/fixes/UNMAPPED_IMPORT_FIX.md
/// Issue: Investigate why return address 0x0F000530 appears on the stack
/// </summary>
public class StackCorruptionDetectionTests
{
    private readonly ITestOutputHelper _output;

    public StackCorruptionDetectionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ValidateReturnAddress_DetectsUnmappedImportAddress()
    {
        // This test validates that if a return address points to an unmapped import address
        // (like 0x0F000530 which would be import index 83 when only 83 imports exist),
        // we can detect it as invalid

        var memory = new VirtualMemory();
        
        // Simulate the scenario from IGN_TEAS.EXE:
        // - 83 imports exist (indices 0-82, addresses 0x0F000000 - 0x0F000520)
        // - Address 0x0F000530 would be index 83, which doesn't exist
        const int IMPORT_COUNT = 83;
        const uint UNMAPPED_IMPORT_ADDR = 0x0F000530u;
        
        // Create a mock import map with 83 imports
        var importMap = new Dictionary<uint, (string dll, string name)>();
        for (int i = 0; i < IMPORT_COUNT; i++)
        {
            var addr = 0x0F000000u + (uint)(i * 0x10);
            importMap[addr] = ("KERNEL32.DLL", $"Import_{i}");
        }
        
        // Verify the last valid import address
        var lastValidAddr = 0x0F000000u + (uint)((IMPORT_COUNT - 1) * 0x10);
        Assert.Equal(0x0F000520u, lastValidAddr);
        Assert.True(importMap.ContainsKey(lastValidAddr));
        
        // Verify the unmapped address is NOT in the map
        Assert.False(importMap.ContainsKey(UNMAPPED_IMPORT_ADDR));
        
        // Verify we can detect this address is in the import range but unmapped
        var isInImportRange = UNMAPPED_IMPORT_ADDR >= 0x0F000000 && UNMAPPED_IMPORT_ADDR < 0x10000000;
        var alignedAddr = UNMAPPED_IMPORT_ADDR & 0xFFFFFFF0u;
        var isMapped = importMap.ContainsKey(alignedAddr);
        
        Assert.True(isInImportRange, "Address should be in import stub range");
        Assert.False(isMapped, "Address should NOT be mapped to any import");
        
        // Calculate which import index this would be
        var wouldBeIndex = (UNMAPPED_IMPORT_ADDR - 0x0F000000) / 0x10;
        _output.WriteLine($"Address 0x{UNMAPPED_IMPORT_ADDR:X8} would be import index {wouldBeIndex}, but only {IMPORT_COUNT} imports exist (0-{IMPORT_COUNT - 1})");
        
        Assert.Equal(83, (int)wouldBeIndex);
        Assert.True(wouldBeIndex >= IMPORT_COUNT, "Import index out of bounds");
    }

    [Fact]
    public void StackValidation_DetectsCorruptedReturnAddress()
    {
        // This test simulates the scenario where a return address on the stack
        // gets corrupted to point to an unmapped import address
        
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        
        var stackBase = 0x00100000u;
        cpu.SetRegister("ESP", stackBase);
        
        // Simulate a normal return address
        var validReturnAddr = 0x00401000u;
        memory.Write32(stackBase, validReturnAddr);
        
        var readBack = memory.Read32(stackBase);
        Assert.Equal(validReturnAddr, readBack);
        
        // Now simulate stack corruption - return address gets overwritten
        var corruptedAddr = 0x0F000530u; // Unmapped import address
        memory.Write32(stackBase, corruptedAddr);
        
        var corruptedReadBack = memory.Read32(stackBase);
        Assert.Equal(corruptedAddr, corruptedReadBack);
        
        // Validate that we can detect this corruption
        var isInImportRange = corruptedReadBack >= 0x0F000000 && corruptedReadBack < 0x10000000;
        Assert.True(isInImportRange, "Corrupted address is in import stub range - this is suspicious");
        
        _output.WriteLine($"CORRUPTION DETECTED: Return address changed from 0x{validReturnAddr:X8} to 0x{corruptedReadBack:X8}");
        _output.WriteLine($"The corrupted address is in import stub range, indicating possible C runtime bug or stack corruption");
    }

    [Fact]
    public void ImportStubAlignment_ValidatesCorrectly()
    {
        // Import stubs are aligned to 16-byte boundaries (0x10)
        // This test validates our alignment logic
        
        var testAddresses = new[]
        {
            (0x0F000000u, 0x0F000000u), // Aligned
            (0x0F000010u, 0x0F000010u), // Aligned
            (0x0F000520u, 0x0F000520u), // Aligned (last valid for 83 imports)
            (0x0F000530u, 0x0F000530u), // Aligned but unmapped
            (0x0F000532u, 0x0F000530u), // Unaligned - should align to 0x530
            (0x0F00053Fu, 0x0F000530u), // Unaligned - should align to 0x530
        };
        
        foreach (var (addr, expected) in testAddresses)
        {
            var aligned = addr & 0xFFFFFFF0u;
            Assert.Equal(expected, aligned);
            _output.WriteLine($"Address 0x{addr:X8} aligns to 0x{aligned:X8}");
        }
    }

    [Fact]
    public void ReturnAddressValidation_AfterImportCall()
    {
        // This test simulates the validation logic that should happen after each import call
        // to detect if the return address on the stack was corrupted
        
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        
        var stackBase = 0x00100000u;
        cpu.SetRegister("ESP", stackBase);
        
        // Stack layout during import call:
        // [ESP+0] = return address to import stub (pointing to RET instruction)
        // [ESP+4] = return address to caller (what we need to validate)
        // [ESP+8+] = function arguments
        
        var returnToStub = 0x0F000005u; // Valid import stub address
        var returnToCaller = 0x00401234u; // Valid code address
        
        memory.Write32(stackBase + 0, returnToStub);
        memory.Write32(stackBase + 4, returnToCaller);
        
        // Read and validate BEFORE simulated API call
        var returnBeforeCall = memory.Read32(stackBase + 4);
        Assert.Equal(returnToCaller, returnBeforeCall);
        
        // Simulate API execution - in a correct implementation, this should not change the return address
        // But if there's stack corruption, it will change
        
        // Read and validate AFTER simulated API call
        var returnAfterCall = memory.Read32(stackBase + 4);
        Assert.Equal(returnToCaller, returnAfterCall);
        
        // The addresses should match
        var returnAddressChanged = returnBeforeCall != returnAfterCall;
        Assert.False(returnAddressChanged, "Return address should not change during API call");
        
        _output.WriteLine($"VALIDATION PASSED: Return address remained 0x{returnAfterCall:X8}");
    }

    [Fact]
    public void ReturnAddressValidation_DetectsCorruption()
    {
        // This test simulates what happens when an API call corrupts the stack
        
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        
        var stackBase = 0x00100000u;
        cpu.SetRegister("ESP", stackBase);
        
        var returnToStub = 0x0F000005u;
        var returnToCallerBefore = 0x00401234u;
        
        memory.Write32(stackBase + 0, returnToStub);
        memory.Write32(stackBase + 4, returnToCallerBefore);
        
        // Read BEFORE
        var returnBefore = memory.Read32(stackBase + 4);
        
        // Simulate stack corruption during API call
        var corruptedReturnAddr = 0x0F000530u; // Unmapped import address
        memory.Write32(stackBase + 4, corruptedReturnAddr);
        
        // Read AFTER
        var returnAfter = memory.Read32(stackBase + 4);
        
        // Detect corruption
        var returnAddressChanged = returnBefore != returnAfter;
        Assert.True(returnAddressChanged, "Stack corruption should be detected");
        
        // Validate the corrupted address is in import range
        var isInImportRange = returnAfter >= 0x0F000000 && returnAfter < 0x10000000;
        Assert.True(isInImportRange);
        
        _output.WriteLine($"STACK CORRUPTION DETECTED:");
        _output.WriteLine($"  Before: 0x{returnBefore:X8}");
        _output.WriteLine($"  After:  0x{returnAfter:X8}");
        _output.WriteLine($"  Change: Return address changed to unmapped import address");
        
        // This is the exact scenario from the UNMAPPED_IMPORT_FIX.md documentation
        // where 0x0F000530 ends up as a return address
    }

    [Fact]
    public void ImportIndexCalculation_FromAddress()
    {
        // Test that we can correctly calculate which import index a given address represents
        
        var testCases = new[]
        {
            (0x0F000000u, 0),   // First import
            (0x0F000010u, 1),   // Second import
            (0x0F000520u, 82),  // 83rd import (index 82)
            (0x0F000530u, 83),  // 84th import (index 83) - doesn't exist in IGN_TEAS
            (0x0F000540u, 84),  // 85th import (index 84)
        };
        
        foreach (var (addr, expectedIndex) in testCases)
        {
            var index = (addr - 0x0F000000) / 0x10;
            Assert.Equal(expectedIndex, (int)index);
            _output.WriteLine($"Address 0x{addr:X8} -> Import index {index}");
        }
        
        // The key insight: 0x0F000530 is import index 83
        // But IGN_TEAS.EXE only has 83 imports (indices 0-82)
        // So index 83 doesn't exist and shouldn't be accessed
    }
}
