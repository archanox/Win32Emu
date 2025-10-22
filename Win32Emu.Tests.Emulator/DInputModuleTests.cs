using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

public class DInputModuleTests
{
    [Fact]
    public void DirectInputCreateA_ShouldReturnSuccess()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInputModule(env, 0x00400000);

        // Allocate space for the output pointer
        var outputPtr = 0x001FF000u;
        vm.Write32(outputPtr, 0x00000000);

        // Set up stack with parameters: hinst, dwVersion, lplpDirectInput, pUnkOuter
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF00, 0x00400000); // hinst
        vm.Write32(0x001FFF04, 0x00000300); // dwVersion = DIRECTINPUT_VERSION (0x0300)
        vm.Write32(0x001FFF08, outputPtr);  // lplpDirectInput
        vm.Write32(0x001FFF0C, 0x00000000); // pUnkOuter = NULL

        // Act
        var result = dinputModule.TryInvokeUnsafe("DirectInputCreateA", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "DirectInputCreateA should be found and invoked");
        Assert.Equal(0x00000000u, returnValue); // DI_OK
    }

    [Fact]
    public void UnknownExport_ShouldReturnFalse()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInputModule(env, 0x00400000);

        cpu.SetRegister("ESP", 0x001FFF00);

        // Act
        var result = dinputModule.TryInvokeUnsafe("UnknownFunction", cpu, vm, out var returnValue);

        // Assert
        Assert.False(result, "Unknown function should return false");
    }
}
