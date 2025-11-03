using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

public class VirtualAllocTests
{
    private const uint MEM_COMMIT   = 0x00001000;
    private const uint MEM_RESERVE  = 0x00002000;
    private const uint PAGE_READWRITE = 0x04;

    [Fact]
    public void VirtualAlloc_Reserve4MB_DoesNotOverflow()
    {
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm, 0x01000000, null, NullLogger.Instance);

        uint addr = env.VirtualAlloc(0, 0x00400000, MEM_RESERVE, PAGE_READWRITE);
        Assert.NotEqual(0u, addr);

        // Ensure we can probe start and end-4 without throwing
        Assert.Equal(0u, vm.Read32(addr));
        Assert.Equal(0u, vm.Read32(addr + 0x00400000 - 4));
    }

    [Fact]
    public void VirtualAlloc_RequestCrossingUpperBoundary_FailsGracefully()
    {
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm, 0x01000000, null, NullLogger.Instance);

        uint hint = 0xFFF30000; // near the end of 32-bit space
        uint addr = env.VirtualAlloc(hint, 0x00400000, MEM_RESERVE, PAGE_READWRITE);
        Assert.Equal(0u, addr);
    }

    [Fact]
    public void VirtualAlloc_ReserveAndCommit_AlignedProperly()
    {
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm, 0x01000000, null, NullLogger.Instance);

        // Reserve 1 byte -> should round up to 64KB region
        uint r = env.VirtualAlloc(0, 1, MEM_RESERVE, PAGE_READWRITE);
        Assert.NotEqual(0u, r);

        // Commit 1 page at reserved address -> should succeed
        uint c = env.VirtualAlloc(r, 0x1000, MEM_COMMIT, PAGE_READWRITE);
        Assert.Equal(r, c);

        // Write within the committed page should be fine
        vm.Write32(c, 0xDEADBEEF);
        Assert.Equal(0xDEADBEEFu, vm.Read32(c));
    }

    [Fact]
    public void HeapAlloc_And_HeapFree_Reuses_Memory()
    {
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm, 0x01000000, null, NullLogger.Instance);

        // Create a heap
        const uint HEAP_NO_SERIALIZE = 0x00000001;
        uint heap = env.HeapCreate(HEAP_NO_SERIALIZE, 0x1000, 0);
        Assert.NotEqual(0u, heap);

        // Allocate 100 blocks of 1KB each
        var allocations = new uint[100];
        for (int i = 0; i < 100; i++)
        {
            allocations[i] = env.HeapAlloc(heap, 0x400); // 1KB
            Assert.NotEqual(0u, allocations[i]);
        }

        // Free all allocations
        for (int i = 0; i < 100; i++)
        {
            uint result = env.HeapFree(heap, allocations[i]);
            Assert.Equal(1u, result);
        }

        // Allocate again - should reuse freed memory and not exhaust address space
        for (int i = 0; i < 100; i++)
        {
            uint addr = env.HeapAlloc(heap, 0x400); // 1KB
            Assert.NotEqual(0u, addr);
        }
    }

    [Fact]
    public void HeapAlloc_Without_Heap_Uses_VirtualAlloc_And_Can_Be_Freed()
    {
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm, 0x01000000, null, NullLogger.Instance);

        // Allocate without a valid heap handle - should use VirtualAlloc fallback
        uint fakeHeapHandle = 0xDEADBEEF;
        uint addr1 = env.HeapAlloc(fakeHeapHandle, 0x1000);
        Assert.NotEqual(0u, addr1);

        // Free it - should call VirtualFree internally
        uint result = env.HeapFree(fakeHeapHandle, addr1);
        Assert.Equal(1u, result);

        // Allocate again - should be able to reuse the freed memory
        uint addr2 = env.HeapAlloc(fakeHeapHandle, 0x1000);
        Assert.NotEqual(0u, addr2);
        
        // The second allocation might reuse the freed block
        // We can't assert they're equal because VirtualAlloc might allocate elsewhere,
        // but we verify that allocation succeeded
    }

    [Fact]
    public void HeapAlloc_ManyAllocationsAndFrees_DoesNotExhaustAddressSpace()
    {
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm, 0x01000000, null, NullLogger.Instance);

        // Create a heap
        const uint HEAP_NO_SERIALIZE = 0x00000001;
        uint heap = env.HeapCreate(HEAP_NO_SERIALIZE, 0x1000, 0x10000); // Small heap with 64KB limit

        // Allocate and free many blocks - this should not exhaust address space
        // because freed memory should be reused
        for (int i = 0; i < 1000; i++)
        {
            // Allocate 64KB - this will exceed heap limit and use VirtualAlloc fallback
            uint addr = env.HeapAlloc(heap, 0x10000);
            Assert.NotEqual(0u, addr);
            
            // Free it immediately
            uint result = env.HeapFree(heap, addr);
            Assert.Equal(1u, result);
        }

        // If the fix is working, we should still be able to allocate after 1000 iterations
        // Without the fix, we would have exhausted the address space
        uint finalAddr = env.HeapAlloc(heap, 0x10000);
        Assert.NotEqual(0u, finalAddr);
    }
}
