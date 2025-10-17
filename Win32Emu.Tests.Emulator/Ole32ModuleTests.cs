using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

public class Ole32ModuleTests
{
    [Fact]
    public void CoInitialize_ShouldReturnSuccess()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm);
        var ole32Module = new Ole32Module(env, 0x00400000);

        // Set up stack with parameter (pvReserved = 0)
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF00, 0x00000000); // pvReserved = NULL

        // Act
        var result = ole32Module.TryInvokeUnsafe("CoInitialize", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "CoInitialize should be found and invoked");
        Assert.Equal(0x00000000u, returnValue); // S_OK
    }

    [Fact]
    public void CoInitialize_CalledTwice_ShouldReturnSFalse()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm);
        var ole32Module = new Ole32Module(env, 0x00400000);

        // Set up stack with parameter
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF00, 0x00000000);

        // Act - First call
        ole32Module.TryInvokeUnsafe("CoInitialize", cpu, vm, out var returnValue1);
        
        // Act - Second call
        var result = ole32Module.TryInvokeUnsafe("CoInitialize", cpu, vm, out var returnValue2);

        // Assert
        Assert.True(result);
        Assert.Equal(0x00000000u, returnValue1); // S_OK on first call
        Assert.Equal(0x00000001u, returnValue2); // S_FALSE on second call
    }

    [Fact]
    public void CoUninitialize_ShouldSucceed()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm);
        var ole32Module = new Ole32Module(env, 0x00400000);

        // Set up stack
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF00, 0x00000000);

        // Initialize COM first
        ole32Module.TryInvokeUnsafe("CoInitialize", cpu, vm, out _);

        // Act
        var result = ole32Module.TryInvokeUnsafe("CoUninitialize", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "CoUninitialize should be found and invoked");
        Assert.Equal(0x00000000u, returnValue); // Returns 0 (void function)
    }

    [Fact]
    public void CoUninitialize_WithoutInitialize_ShouldSucceed()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm);
        var ole32Module = new Ole32Module(env, 0x00400000);

        // Set up stack
        cpu.SetRegister("ESP", 0x001FFF00);

        // Act - Call without CoInitialize
        var result = ole32Module.TryInvokeUnsafe("CoUninitialize", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "CoUninitialize should still succeed");
        Assert.Equal(0x00000000u, returnValue);
    }

    [Fact]
    public void UnknownExport_ShouldReturnFalse()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm);
        var ole32Module = new Ole32Module(env, 0x00400000);

        cpu.SetRegister("ESP", 0x001FFF00);

        // Act
        var result = ole32Module.TryInvokeUnsafe("UnknownFunction", cpu, vm, out var returnValue);

        // Assert
        Assert.False(result, "Unknown function should return false");
    }
}
