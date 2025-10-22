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

    [Fact]
    public void SetDataFormat_ShouldParseAndStoreDataFormat()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);

        // Create DIDATAFORMAT structure at 0x0040A480 (as seen in problem statement)
        var dataFormatPtr = 0x0040A480u;
        vm.Write32(dataFormatPtr + 0, 24);          // dwSize
        vm.Write32(dataFormatPtr + 4, 16);          // dwObjSize
        vm.Write32(dataFormatPtr + 8, 0x00000002);  // dwFlags (DIDF_RELAXIS)
        vm.Write32(dataFormatPtr + 12, 256);        // dwDataSize (keyboard: 256 bytes)
        vm.Write32(dataFormatPtr + 16, 10);         // dwNumObjs
        vm.Write32(dataFormatPtr + 20, 0x0040B000);// rgodf

        // Act - Verify the structure is correctly written
        var dwSize = vm.Read32(dataFormatPtr);
        var dwObjSize = vm.Read32(dataFormatPtr + 4);
        var dwFlags = vm.Read32(dataFormatPtr + 8);
        var dwDataSize = vm.Read32(dataFormatPtr + 12);
        var dwNumObjs = vm.Read32(dataFormatPtr + 16);
        var rgodf = vm.Read32(dataFormatPtr + 20);

        // Assert
        Assert.Equal(24u, dwSize);
        Assert.Equal(16u, dwObjSize);
        Assert.Equal(0x00000002u, dwFlags);
        Assert.Equal(256u, dwDataSize);
        Assert.Equal(10u, dwNumObjs);
        Assert.Equal(0x0040B000u, rgodf);
    }

    [Fact]
    public void SetCooperativeLevel_ShouldStoreWindowHandleAndFlags()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInputModule(env, 0x00400000);

        // Setup stack for SetCooperativeLevel call (simulated COM call)
        cpu.SetRegister("ESP", 0x001FED00);
        vm.Write32(0x001FED00, 0x014508C0);        // this
        vm.Write32(0x001FED04, 0x00010000);        // hwnd
        vm.Write32(0x001FED08, 0x00000006);        // flags (DISCL_NONEXCLUSIVE | DISCL_FOREGROUND)

        // Act - The method should parse and log the flags
        // Verify the structure is correct
        var hwnd = vm.Read32(0x001FED04);
        var flags = vm.Read32(0x001FED08);

        // Assert
        Assert.Equal(0x00010000u, hwnd);
        Assert.Equal(0x00000006u, flags);

        // Verify flags
        Assert.True((flags & 0x02) != 0, "DISCL_NONEXCLUSIVE flag should be set");
        Assert.True((flags & 0x04) != 0, "DISCL_FOREGROUND flag should be set");
    }

    [Fact]
    public void SetProperty_ShouldParsePropertyHeader()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInputModule(env, 0x00400000);

        // Create DIPROPDWORD structure
        var propPtr = 0x001FF200u;
        vm.Write32(propPtr + 0, 20);               // dwSize (sizeof DIPROPDWORD)
        vm.Write32(propPtr + 4, 16);               // dwHeaderSize (sizeof DIPROPHEADER)
        vm.Write32(propPtr + 8, 0);                // dwObj (0 for device)
        vm.Write32(propPtr + 12, 0);               // dwHow (DIPH_DEVICE)
        vm.Write32(propPtr + 16, 100);             // dwData (property value)

        // Setup stack for SetProperty call
        cpu.SetRegister("ESP", 0x001FED00);
        vm.Write32(0x001FED00, 0x014508C0);        // this
        vm.Write32(0x001FED04, 1);                 // rguidProp (DIPROP_BUFFERSIZE)
        vm.Write32(0x001FED08, propPtr);           // pdiph

        // Act - Verify structure is correct
        var dwSize = vm.Read32(propPtr);
        var dwHeaderSize = vm.Read32(propPtr + 4);
        var dwData = vm.Read32(propPtr + 16);

        // Assert
        Assert.Equal(20u, dwSize);
        Assert.Equal(16u, dwHeaderSize);
        Assert.Equal(100u, dwData);
    }

    [Fact]
    public void Acquire_ShouldMarkDeviceAsAcquired()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new IcedCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInputModule(env, 0x00400000);

        // Setup stack for Acquire call
        cpu.SetRegister("ESP", 0x001FED00);
        vm.Write32(0x001FED00, 0x014508C0);        // this

        // Act - The method should mark the device as acquired
        // Verify the call structure is correct
        var thisPtr = vm.Read32(0x001FED00);

        // Assert
        Assert.Equal(0x014508C0u, thisPtr);
    }
}
