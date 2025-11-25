using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Test to verify synthetic exports use the syscall mechanism
/// </summary>
[Trait("Category", "DllModuleTests")]
public class SyntheticExportAddressFixTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public SyntheticExportAddressFixTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void SyntheticExport_ShouldUseSyscallMechanism()
    {
        // Synthetic exports now use the syscall mechanism (CALL/RET stubs) like import stubs
        // They should be in the 0x0F800000 range, distinct from import stubs at 0x0F000000
        
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        
        // Verify synthetic export is NOT at 0x0E000000 (syscall dispatcher)
        Assert.NotEqual(0x0E000000u, functionAddress);
        
        // Verify it's in the synthetic export range (0x0F800000+) with 16-byte alignment
        Assert.True(functionAddress >= 0x0F800000u);
        Assert.Equal(0u, functionAddress % 16);
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
