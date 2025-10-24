using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for synthetic exports (functions looked up via GetProcAddress and called through function pointers)
/// </summary>
public class SyntheticExportTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public SyntheticExportTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void GetProcAddress_ForIsProcessorFeaturePresent_ShouldReturnValidAddress()
    {
        // Arrange - Get handle to KERNEL32 system DLL
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        Assert.NotEqual(0u, moduleHandle);
        
        // Look up IsProcessorFeaturePresent function
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        
        // Act
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        
        // Assert - Should return a non-zero function address in the synthetic export range
        Assert.NotEqual(0u, functionAddress);
        Assert.InRange(functionAddress, 0x0E000000u, 0x0F000000u);
    }

    [Fact]
    public void SyntheticExport_ShouldBeRegisteredInProcessEnvironment()
    {
        // Arrange
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        
        // Act
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        
        // Assert - Should be able to look up the synthetic export
        var found = _testEnv.ProcessEnv.TryGetSyntheticExport(functionAddress, out var moduleName, out var exportName);
        Assert.True(found);
        Assert.Equal("KERNEL32.DLL", moduleName);
        Assert.Equal("ISPROCESSORFEATUREPRESENT", exportName);
    }

    [Fact]
    public void SyntheticExport_DifferentFunctions_ShouldReturnDifferentAddresses()
    {
        // Arrange
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        
        var procName1Ptr = _testEnv.WriteString("IsProcessorFeaturePresent");
        var procName2Ptr = _testEnv.WriteString("GetVersion");
        
        // Act
        var address1 = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procName1Ptr);
        var address2 = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procName2Ptr);
        
        // Assert - Different functions should have different addresses
        Assert.NotEqual(address1, address2);
    }

    [Fact]
    public void SyntheticExport_MemoryAt_ShouldContainINT3Stub()
    {
        // Arrange
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        
        // Act
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        
        // Assert - Should have INT3 (0xCC) as the first byte
        var firstByte = _testEnv.Memory.Read8(functionAddress);
        Assert.Equal(0xCC, firstByte);
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
