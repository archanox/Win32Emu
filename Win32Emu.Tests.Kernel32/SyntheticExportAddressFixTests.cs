using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Test to verify the synthetic export address collision fix
/// </summary>
public class SyntheticExportAddressFixTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public SyntheticExportAddressFixTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void SyntheticExport_ShouldNotCollideWithSyscallDispatcher()
    {
        // The syscall dispatcher is at 0x0E000000
        // Synthetic exports should start at 0x0E000010 to avoid collision
        
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        
        // Verify synthetic export is NOT at 0x0E000000 (syscall dispatcher)
        Assert.NotEqual(0x0E000000u, functionAddress);
        
        // Verify it's at 0x0E000010 or later (with 16-byte alignment)
        Assert.True(functionAddress >= 0x0E000010u);
        Assert.Equal(0u, functionAddress % 16);
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
