using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for synthetic exports (functions looked up via GetProcAddress and called through function pointers)
/// </summary>
[Trait("Category", "DllModuleTests")]
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
        Assert.InRange(functionAddress, 0x0F800000u, 0x10000000u); // Synthetic exports at 0x0F800000+
    }

    [Fact]
    public void SyntheticExport_ShouldUse_SyscallMechanism()
    {
        // Arrange
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var kernel32Handle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        
        // Act
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", kernel32Handle, procNamePtr);
        
        // Assert - Synthetic exports now use the syscall mechanism (CALL/RET stubs)
        // They should be in the 0x0F800000+ range and have CALL instruction as first byte
        Assert.InRange(functionAddress, 0x0F800000u, 0x10000000u);
        
        // The stub should start with CALL instruction (0xE8)
        var firstByte = _testEnv.Memory.Read8(functionAddress);
        Assert.Equal(0xE8, firstByte);
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
    public void SyntheticExport_MemoryAt_ShouldContainCALLStub()
    {
        // Arrange
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        
        // Act
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        
        // Assert - Should have CALL (0xE8) as the first byte (CALL rel32 instruction)
        var firstByte = _testEnv.Memory.Read8(functionAddress);
        Assert.Equal(0xE8, firstByte); // CALL instruction
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
