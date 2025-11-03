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
}
