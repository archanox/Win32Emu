using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Integration test that simulates the Hot Wheels scenario:
/// 1. GetModuleHandleA("KERNEL32")
/// 2. GetProcAddress(hModule, "IsProcessorFeaturePresent")
/// 3. Call the returned function pointer
/// </summary>
public class SyntheticExportIntegrationTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public SyntheticExportIntegrationTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void HotWheelsScenario_GetProcAddress_And_Call_IsProcessorFeaturePresent()
    {
        // This test simulates the exact scenario from the Hot Wheels issue log:
        // 1. Game calls GetModuleHandleA("KERNEL32")
        // 2. Game calls GetProcAddress(hModule, "IsProcessorFeaturePresent")
        // 3. Game calls the returned function pointer
        
        // Step 1: GetModuleHandleA("KERNEL32")
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        Assert.NotEqual(0u, moduleHandle);
        
        // Step 2: GetProcAddress(hModule, "IsProcessorFeaturePresent")
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        Assert.NotEqual(0u, functionAddress);
        Assert.InRange(functionAddress, 0x0E000000u, 0x0F000000u);
        
        // Step 3: Create a real CPU (IcedCpu) to test the call through the function pointer
        var cpu = new IcedCpu(_testEnv.Memory, NullLogger.Instance);
        
        // Set up a simple stack
        const uint stackBase = 0x00200000;
        cpu.SetRegister("ESP", stackBase);
        cpu.SetRegister("EBP", stackBase);
        
        // Write a return address on the stack
        const uint returnAddress = 0x00401000;
        _testEnv.Memory.Write32(stackBase, returnAddress);
        
        // Write the function parameter (processorFeature = 0) on the stack
        _testEnv.Memory.Write32(stackBase + 4, 0);
        
        // Write a CALL instruction that jumps to the function address
        const uint callInstructionAddress = 0x00400000;
        cpu.SetEip(callInstructionAddress);
        
        // Write: PUSH 0; CALL [functionAddress]; (we'll simulate by setting EIP directly)
        // Since we're testing the INT3 hook, we'll just jump to the synthetic export directly
        cpu.SetEip(functionAddress);
        
        // Execute one instruction - this should hit the INT3 and mark it as a call
        var result = cpu.SingleStep(_testEnv.Memory);
        
        // Verify that the CPU recognized this as a call to a synthetic export
        Assert.True(result.IsCall);
        Assert.Equal(functionAddress, result.CallTarget);
        
        // Verify that we can look up the synthetic export
        var found = _testEnv.ProcessEnv.TryGetSyntheticExport(functionAddress, out var moduleName, out var exportName);
        Assert.True(found);
        Assert.Equal("KERNEL32.DLL", moduleName);
        Assert.Equal("ISPROCESSORFEATUREPRESENT", exportName);
        
        // Verify that the dispatcher can invoke it
        var success = _testEnv.Dispatcher.TryInvoke(moduleName, exportName, cpu, _testEnv.Memory, out var retValue, out var argBytes);
        Assert.True(success);
        Assert.Equal(4, argBytes); // IsProcessorFeaturePresent has 1 uint parameter (4 bytes)
        
        // The return value should be a valid result (0 or 1 for FALSE/TRUE)
        Assert.True(retValue is 0 or 1);
    }

    [Fact]
    public void SyntheticExport_INT3_ShouldBeInRange()
    {
        // Arrange - Get a synthetic export address
        var kernel32Name = _testEnv.WriteString("KERNEL32");
        var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", kernel32Name);
        var procNamePtr = _testEnv.WriteString("IsProcessorFeaturePresent");
        var functionAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);
        
        // Assert - Should be in synthetic export range and have INT3 stub
        Assert.InRange(functionAddress, 0x0E000000u, 0x0F000000u);
        Assert.Equal(0xCC, _testEnv.Memory.Read8(functionAddress));
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
