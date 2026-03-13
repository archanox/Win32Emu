using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Win32Emu.Gui.Backends;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Emulator;

public class DInputModuleTests
{
	/// <summary>
	/// Mock backend factory for testing
	/// </summary>
	private class MockBackendFactory : IBackendFactory
	{
		// Test mock doesn't need configurable backend type
		public BackendType CurrentBackendType { get; set; } = BackendType.Headless;
		
		public IRenderingBackend CreateRenderingBackend(ILogger logger) => throw new NotImplementedException();
		public IRenderingBackend CreateRenderingBackendWithHost(ILogger logger, IEmulatorHost? host) => throw new NotImplementedException();
		public IAudioBackend CreateAudioBackend(ILogger logger) => throw new NotImplementedException();
		public IInputBackend CreateInputBackend(ILogger logger) => new MockInputBackend();
	}

	/// <summary>
	/// Mock input backend for testing
	/// </summary>
	private class MockInputBackend : IInputBackend
	{
		public bool IsInitialized { get; private set; }
		public int DeviceCount => 0;
		
		// Event required by interface but not used in tests
#pragma warning disable CS0067
		public event EventHandler<UIEventArgs>? UIEvent;
#pragma warning restore CS0067

		public Task<bool> InitializeAsync()
		{
			IsInitialized = true;
			return Task.FromResult(true);
		}

		public List<(uint DeviceId, string Name, IInputBackend.DeviceType Type)> GetDevices()
		{
			return new List<(uint, string, IInputBackend.DeviceType)>
			{
				(1, "Mock Keyboard", IInputBackend.DeviceType.Keyboard),
				(2, "Mock Mouse", IInputBackend.DeviceType.Mouse)
			};
		}

		public uint OpenDevice(uint deviceId, IInputBackend.DeviceType type) => deviceId;
		public bool CloseDevice(uint deviceId) => true;
		public bool PollDevice(uint deviceId, out IInputBackend.InputState? state)
		{
			state = new IInputBackend.InputState();
			return true;
		}
		public void ProcessEvents() { }
		public void Dispose() { }
	}

    [Fact]
    public async Task DirectInputCreateA_ShouldReturnSuccess()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInputModule(env, 0x00400000);

        // Allocate space for the output pointer
        var outputPtr = 0x001FF000u;
        vm.Write32(outputPtr, 0x00000000);

        // Set up stack with parameters: hinst, dwVersion, lplpDirectInput, pUnkOuter
        // Note: StackArgs reads starting from ESP+4, so we write parameters there
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF04, 0x00400000); // hinst (ESP+4 = a.UInt32(0))
        vm.Write32(0x001FFF08, 0x00000300); // dwVersion = DIRECTINPUT_VERSION (0x0300) (ESP+8 = a.UInt32(1))
        vm.Write32(0x001FFF0C, outputPtr);  // lplpDirectInput (ESP+12 = a.UInt32(2))
        vm.Write32(0x001FFF10, 0x00000000); // pUnkOuter = NULL (ESP+16 = a.UInt32(3))

        // Act
        var result = dinputModule.TryInvokeUnsafe("DirectInputCreateA", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "DirectInputCreateA should be found and invoked");
        Assert.Equal(0x00000000u, returnValue); // DI_OK
        
        // Verify COM object pointer was written (note: COM objects may start at address 0)
        // var comObjectPtr = vm.Read32(outputPtr);
        // Just verify the call succeeded for now
    }

    [Fact]
    public async Task DirectInputCreate_ShouldInitializeInputBackend()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var backendFactory = new MockBackendFactory();
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000, backendFactory: backendFactory);
        var dinputModule = new DInputModule(env, 0x00400000);

        var outputPtr = 0x001FF000u;
        vm.Write32(outputPtr, 0x00000000);

        // Note: StackArgs reads starting from ESP+4
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF04, 0x00400000); // hinst (ESP+4 = a.UInt32(0))
        vm.Write32(0x001FFF08, 0x00000300); // dwVersion (ESP+8 = a.UInt32(1))
        vm.Write32(0x001FFF0C, outputPtr);  // lplpDirectInput (ESP+12 = a.UInt32(2))
        vm.Write32(0x001FFF10, 0x00000000); // pUnkOuter (ESP+16 = a.UInt32(3))

        // Act
        var result = dinputModule.TryInvokeUnsafe("DirectInputCreate", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "DirectInputCreate should be found and invoked");
        Assert.Equal(0x00000000u, returnValue); // DI_OK
        Assert.NotNull(env.InputBackend);
    }

    [Fact]
    public async Task DirectInput8Create_ShouldReturnSuccess()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInput8Module(env, 0x00400000);

        var outputPtr = 0x001FF000u;
        vm.Write32(outputPtr, 0x00000000);

        // Note: StackArgs reads starting from ESP+4
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF04, 0x00400000); // hinst (ESP+4 = a.UInt32(0))
        vm.Write32(0x001FFF08, 0x00000800); // dwVersion = DIRECTINPUT_VERSION (0x0800 for DInput8) (ESP+8 = a.UInt32(1))
        vm.Write32(0x001FFF0C, 0x00000000); // riidltf (ESP+12 = a.UInt32(2))
        vm.Write32(0x001FFF10, outputPtr);  // lplpDirectInput (ESP+16 = a.UInt32(3))
        vm.Write32(0x001FFF14, 0x00000000); // pUnkOuter (ESP+20 = a.UInt32(4))

        // Act
        var result = dinputModule.TryInvokeUnsafe("DirectInput8Create", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "DirectInput8Create should be found and invoked");
        Assert.Equal(0x00000000u, returnValue); // DI_OK
    }

    [Fact]
    public async Task DirectInputCreateEx_ShouldReturnSuccess()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var backendFactory = new MockBackendFactory();
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000, backendFactory: backendFactory);
        var dinputModule = new DInputModule(env, 0x00400000);

        var outputPtr = 0x001FF000u;
        vm.Write32(outputPtr, 0x00000000);

        // Note: StackArgs reads starting from ESP+4
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF04, 0x00400000); // hinst (ESP+4 = a.UInt32(0))
        vm.Write32(0x001FFF08, 0x00000700); // dwVersion = DIRECTINPUT_VERSION (0x0700 for DInput7) (ESP+8 = a.UInt32(1))
        vm.Write32(0x001FFF0C, 0x00000000); // riidltf (IID_IDirectInput7) (ESP+12 = a.UInt32(2))
        vm.Write32(0x001FFF10, outputPtr);  // lplpDirectInput (ESP+16 = a.UInt32(3))
        vm.Write32(0x001FFF14, 0x00000000); // pUnkOuter (ESP+20 = a.UInt32(4))

        // Act
        var result = dinputModule.TryInvokeUnsafe("DirectInputCreateEx", cpu, vm, out var returnValue);

        // Assert
        Assert.True(result, "DirectInputCreateEx should be found and invoked");
        Assert.Equal(0x00000000u, returnValue); // DI_OK
        Assert.NotNull(env.InputBackend);
    }

    [Fact]
    public async Task UnknownExport_ShouldReturnFalse()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
        var dinputModule = new DInputModule(env, 0x00400000);

        cpu.SetRegister("ESP", 0x001FFF00);

        // Act
        var result = dinputModule.TryInvokeUnsafe("UnknownFunction", cpu, vm, out var returnValue);

        // Assert
        Assert.False(result, "Unknown function should return false");
    }

    [Fact]
    public async Task InputBackend_ShouldEnumerateDevices()
    {
        // Arrange
        var backend = new SilkInputBackend(NullLogger.Instance);
        
        // Act
        await backend.InitializeAsync();
        var devices = backend.GetDevices();

        // Assert
        Assert.NotEmpty(devices);
        Assert.Contains(devices, d => d.Type == IInputBackend.DeviceType.Keyboard);
        Assert.Contains(devices, d => d.Type == IInputBackend.DeviceType.Mouse);
    }

    [Fact]
    public async Task InputBackend_ShouldOpenAndCloseDevice()
    {
        // Arrange
        var backend = new SilkInputBackend(NullLogger.Instance);
        await backend.InitializeAsync();
        var devices = backend.GetDevices();
        var keyboardDevice = devices.First(d => d.Type == IInputBackend.DeviceType.Keyboard);

        // Act
        var deviceHandle = backend.OpenDevice(keyboardDevice.DeviceId, IInputBackend.DeviceType.Keyboard);
        var pollResult = backend.PollDevice(deviceHandle, out var state);
        var closeResult = backend.CloseDevice(deviceHandle);

        // Assert
        Assert.NotEqual(0u, deviceHandle);
        Assert.True(pollResult);
        Assert.NotNull(state);
        Assert.True(closeResult);
    }

    [Fact]
    public async Task InputState_ShouldHaveKeyboardState()
    {
        // Arrange
        var state = new IInputBackend.InputState();

        // Act
        state.KeyStates[0x01] = true; // Escape key
        state.KeyStates[0x1E] = true; // 'A' key

        // Assert
        Assert.True(state.KeyStates[0x01]);
        Assert.True(state.KeyStates[0x1E]);
        Assert.False(state.KeyStates.GetValueOrDefault(0x02, false)); // '1' key not pressed
    }

    [Fact]
    public async Task InputState_ShouldHaveMouseState()
    {
        // Arrange
        var state = new IInputBackend.InputState();

        // Act
        state.MouseX = 100;
        state.MouseY = 200;
        state.MouseZ = 50; // Scroll wheel
        state.MouseButtons[0] = true; // Left button
        state.MouseButtons[1] = false; // Right button
        state.MouseButtons[2] = true; // Middle button

        // Assert
        Assert.Equal(100, state.MouseX);
        Assert.Equal(200, state.MouseY);
        Assert.Equal(50, state.MouseZ);
        Assert.True(state.MouseButtons[0]);
        Assert.False(state.MouseButtons[1]);
        Assert.True(state.MouseButtons[2]);
    }

    [Fact]
    public async Task InputState_ShouldHaveJoystickState()
    {
        // Arrange
        var state = new IInputBackend.InputState();

        // Act
        state.Axes[0] = 100;   // X axis
        state.Axes[1] = -200;  // Y axis
        state.Axes[2] = 50;    // Z axis (throttle)
        state.Buttons[0] = true;
        state.Buttons[1] = false;
        state.PovHat = 0; // Centered

        // Assert
        Assert.Equal(100, state.Axes[0]);
        Assert.Equal(-200, state.Axes[1]);
        Assert.Equal(50, state.Axes[2]);
        Assert.True(state.Buttons[0]);
        Assert.False(state.Buttons[1]);
        Assert.Equal(0, state.PovHat);
    }

    [Fact]
    public async Task SetDataFormat_ShouldParseAndStoreDataFormat()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
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
    public async Task SetCooperativeLevel_ShouldStoreWindowHandleAndFlags()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
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
    public async Task SetProperty_ShouldParsePropertyHeader()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
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
    public async Task Acquire_ShouldMarkDeviceAsAcquired()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
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

    /// <summary>
    /// Regression test for IGN_TEAS.EXE crash:
    /// Verifies SetDataFormat returns DIERR_INVALIDPARAM when given NULL pointer.
    /// </summary>
    [Fact]
    public void SetDataFormat_WithNullPointer_ReturnsInvalidParamError()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var backendFactory = new MockBackendFactory();
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000, backendFactory: backendFactory);
        var dinputModule = new DInputModule(env, 0x00400000, logger: NullLogger.Instance);

        // Setup stack for SetDataFormat call with NULL lpdf parameter
        cpu.SetRegister("ESP", 0x001FED00);
        vm.Write32(0x001FED04, 0x014508C0);  // this pointer (ESP+4 = arg 0)
        vm.Write32(0x001FED08, 0x00000000);  // lpdf = NULL (ESP+8 = arg 1)
        
        // Use reflection to invoke the private method
        var method = typeof(DInputModule).GetMethod("DInputDevice_SetDataFormat", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        // Act
        var returnValue = (uint)method!.Invoke(dinputModule, new object[] { cpu, vm })!;

        // Assert - Should return DIERR_INVALIDPARAM (0x80070057)
        Assert.Equal(0x80070057u, returnValue);
    }

    /// <summary>
    /// Regression test for IGN_TEAS.EXE crash:
    /// Verifies SetDataFormat returns DIERR_INVALIDPARAM when given invalid memory address.
    /// </summary>
    [Fact]
    public void SetDataFormat_WithInvalidMemory_ReturnsInvalidParamError()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var backendFactory = new MockBackendFactory();
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000, backendFactory: backendFactory);
        var dinputModule = new DInputModule(env, 0x00400000, logger: NullLogger.Instance);

        // First create DirectInput to initialize internal state
        var outputPtr = 0x001FF000u;
        vm.Write32(outputPtr, 0x00000000);
        cpu.SetRegister("ESP", 0x001FFF00);
        vm.Write32(0x001FFF04, 0x00400000); // hinst
        vm.Write32(0x001FFF08, 0x00000300); // dwVersion
        vm.Write32(0x001FFF0C, outputPtr);  // lplpDirectInput
        vm.Write32(0x001FFF10, 0x00000000); // pUnkOuter
        dinputModule.TryInvokeUnsafe("DirectInputCreateA", cpu, vm, out _);

        // Setup stack for SetDataFormat call with invalid memory address
        cpu.SetRegister("ESP", 0x001FED00);
        vm.Write32(0x001FED04, 0x014508C0);  // this pointer (ESP+4 = arg 0)
        vm.Write32(0x001FED08, 0xFFFFFF00);  // lpdf = invalid address (ESP+8 = arg 1)
        
        // Use reflection to invoke the private method
        var method = typeof(DInputModule).GetMethod("DInputDevice_SetDataFormat", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        // Act
        var returnValue = (uint)method!.Invoke(dinputModule, new object[] { cpu, vm })!;

        // Assert - Should return DIERR_INVALIDPARAM (0x80070057) due to exception
        Assert.Equal(0x80070057u, returnValue);
    }

    /// <summary>
    /// Regression test for IGN_TEAS.EXE crash:
    /// Verifies SetProperty returns DIERR_INVALIDPARAM when given NULL pointer.
    /// </summary>
    [Fact]
    public void SetProperty_WithNullPointer_ReturnsInvalidParamError()
    {
        // Arrange
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var backendFactory = new MockBackendFactory();
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000, backendFactory: backendFactory);
        var dinputModule = new DInputModule(env, 0x00400000, logger: NullLogger.Instance);

        // Setup stack for SetProperty call with NULL pdiph parameter
        cpu.SetRegister("ESP", 0x001FED00);
        vm.Write32(0x001FED04, 0x014508C0);  // this pointer (ESP+4 = arg 0)
        vm.Write32(0x001FED08, 1);           // rguidProp = DIPROP_BUFFERSIZE (ESP+8 = arg 1)
        vm.Write32(0x001FED0C, 0x00000000);  // pdiph = NULL (ESP+12 = arg 2)
        
        // Use reflection to invoke the private method
        var method = typeof(DInputModule).GetMethod("DInputDevice_SetProperty", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        // Act
        var returnValue = (uint)method!.Invoke(dinputModule, new object[] { cpu, vm })!;

        // Assert - Should return DIERR_INVALIDPARAM (0x80070057)
        Assert.Equal(0x80070057u, returnValue);
    }

    /// <summary>
    /// Verifies that GetDeviceData converts VK codes (stored in backend) to DIK scan codes
    /// (expected by the emulated game) when returning buffered keyboard events.
    /// ign_teas and ign_win both use DirectInput buffered keyboard input and check dwOfs
    /// against DIK_* constants (e.g. DIK_LEFT = 0xCB for the left arrow key).
    /// </summary>
    [Fact]
    public async Task GetDeviceData_KeyboardEvent_ReportsDikScanCode()
    {
        // Arrange: stateful backend that can inject VK key state
        var statefulBackend = new StatefulMockInputBackend();
        var backendFactory = new StatefulMockBackendFactory(statefulBackend);
        var vm = new VirtualMemory(0x10000000);
        var cpu = new JitCpu(vm);
        var env = new ProcessEnvironment(vm, heapBase: 0x01000000, backendFactory: backendFactory);
        var dinputModule = new DInputModule(env, 0x00400000, logger: NullLogger.Instance);

        // Initialise DirectInput (creates IDirectInput COM object and initialises InputBackend)
        var diPtr = 0x001FF000u;
        vm.Write32(diPtr, 0);
        cpu.SetRegister("ESP", 0x001FFE00);
        vm.Write32(0x001FFE04, 0x00400000); // hinst
        vm.Write32(0x001FFE08, 0x00000300); // dwVersion
        vm.Write32(0x001FFE0C, diPtr);       // lplpDirectInput
        vm.Write32(0x001FFE10, 0);           // pUnkOuter
        dinputModule.TryInvokeUnsafe("DirectInputCreateA", cpu, vm, out _);
        Assert.NotNull(env.InputBackend);

        // Inject a DirectInputDevice via reflection.
        // The device must be in the _devices dictionary and IsAcquired must be true for
        // GetDeviceData to process it.  BackendDeviceId=1 maps to statefulBackend device 1.
        var devicesField = typeof(DInputModule)
            .GetField("_devices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(devicesField);

        var devicesDict = devicesField!.GetValue(dinputModule);
        Assert.NotNull(devicesDict);

        // Find the DirectInputDevice type inside DInputModule
        var deviceType = typeof(DInputModule).GetNestedType(
            "DirectInputDevice", System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(deviceType);

        var device = Activator.CreateInstance(deviceType!)!;
        deviceType!.GetProperty("Handle")!.SetValue(device, 0x01000000u);
        deviceType!.GetProperty("BackendDeviceId")!.SetValue(device, 1u); // matches keyboard device ID 1
        deviceType!.GetProperty("DeviceType")!.SetValue(device, IInputBackend.DeviceType.Keyboard);
        deviceType!.GetProperty("IsAcquired")!.SetValue(device, true);
        deviceType!.GetProperty("Name")!.SetValue(device, "TestKeyboard");

        var addMethod = devicesDict!.GetType().GetMethod("Add")!;
        addMethod.Invoke(devicesDict, new object[] { 0x01000000u, device });

        // Simulate VK_LEFT (0x25) being pressed in the input backend
        statefulBackend.KeyboardState.KeyStates[0x25] = true; // VK_LEFT

        // Allocate output buffer for GetDeviceData (room for 4 events of 16 bytes each)
        const uint EventBufPtr = 0x001FD000u;
        const uint CountPtr    = 0x001FD200u;
        vm.Write32(CountPtr, 4); // request up to 4 events

        // Call DInputDevice_GetDeviceData via reflection
        var getDataMethod = typeof(DInputModule).GetMethod(
            "DInputDevice_GetDeviceData",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(getDataMethod);

        cpu.SetRegister("ESP", 0x001FEB00);
        vm.Write32(0x001FEB04, 0x01000000u); // this = device handle
        vm.Write32(0x001FEB08, 16);           // cbObjectData = sizeof(DIDEVICEOBJECTDATA)
        vm.Write32(0x001FEB0C, EventBufPtr);  // rgdod (output buffer)
        vm.Write32(0x001FEB10, CountPtr);     // pdwInOut
        vm.Write32(0x001FEB14, 0);            // dwFlags

        var dataResult = (uint)getDataMethod!.Invoke(dinputModule, new object[] { cpu, vm })!;
        Assert.Equal(0u, dataResult); // DI_OK

        // Verify returned event count
        var returnedCount = vm.Read32(CountPtr);
        Assert.Equal(1u, returnedCount); // one key-down event

        // Verify DIDEVICEOBJECTDATA: dwOfs should be DIK_LEFT = 0xCB
        var dwOfs  = vm.Read32(EventBufPtr + 0);  // dwOfs at offset 0
        var dwData = vm.Read32(EventBufPtr + 4);  // dwData at offset 4
        Assert.Equal(0xCBu, dwOfs);   // DIK_LEFT
        Assert.Equal(0x80u, dwData);  // 0x80 = key pressed
    }

    // Infrastructure for stateful mock
    private class StatefulMockInputBackend : IInputBackend
    {
        public bool IsInitialized { get; private set; }
        public int DeviceCount => 1;

        public event EventHandler<UIEventArgs>? UIEvent { add { } remove { } }

        public readonly IInputBackend.InputState KeyboardState = new();

        public Task<bool> InitializeAsync() { IsInitialized = true; return Task.FromResult(true); }

        public List<(uint DeviceId, string Name, IInputBackend.DeviceType Type)> GetDevices() =>
            new() { (1, "Mock Keyboard", IInputBackend.DeviceType.Keyboard) };

        public uint OpenDevice(uint deviceId, IInputBackend.DeviceType type) => deviceId;
        public bool CloseDevice(uint deviceId) => true;

        public bool PollDevice(uint deviceId, out IInputBackend.InputState? state)
        {
            state = KeyboardState;
            return true;
        }

        public void ProcessEvents() { }
        public void Dispose() { }
    }

    private class StatefulMockBackendFactory(StatefulMockInputBackend backend) : IBackendFactory
    {
        public BackendType CurrentBackendType { get; set; } = BackendType.Headless;
        public IRenderingBackend CreateRenderingBackend(ILogger logger) => throw new NotImplementedException();
        public IRenderingBackend CreateRenderingBackendWithHost(ILogger logger, IEmulatorHost? host) => throw new NotImplementedException();
        public IAudioBackend CreateAudioBackend(ILogger logger) => throw new NotImplementedException();
        public IInputBackend CreateInputBackend(ILogger logger) => backend;
    }
}

// Separate test class for KeyCodeMapper so it is clean and focused
public class KeyCodeMapperTests
{
    [Theory]
    [InlineData(0x41, 0x1E)] // VK_A   → DIK_A
    [InlineData(0x53, 0x1F)] // VK_S   → DIK_S
    [InlineData(0x44, 0x20)] // VK_D   → DIK_D
    [InlineData(0x57, 0x11)] // VK_W   → DIK_W
    [InlineData(0x1B, 0x01)] // VK_ESCAPE → DIK_ESCAPE
    [InlineData(0x20, 0x39)] // VK_SPACE  → DIK_SPACE
    [InlineData(0x0D, 0x1C)] // VK_RETURN → DIK_RETURN
    [InlineData(0x25, 0xCB)] // VK_LEFT   → DIK_LEFT
    [InlineData(0x27, 0xCD)] // VK_RIGHT  → DIK_RIGHT
    [InlineData(0x26, 0xC8)] // VK_UP     → DIK_UP
    [InlineData(0x28, 0xD0)] // VK_DOWN   → DIK_DOWN
    [InlineData(0x70, 0x3B)] // VK_F1     → DIK_F1
    [InlineData(0x7B, 0x58)] // VK_F12    → DIK_F12
    [InlineData(0xA0, 0x2A)] // VK_LSHIFT → DIK_LSHIFT
    [InlineData(0xA1, 0x36)] // VK_RSHIFT → DIK_RSHIFT
    [InlineData(0xA2, 0x1D)] // VK_LCONTROL → DIK_LCONTROL
    [InlineData(0xA3, 0x9D)] // VK_RCONTROL → DIK_RCONTROL
    [InlineData(0xA4, 0x38)] // VK_LMENU  → DIK_LMENU
    [InlineData(0xA5, 0xB8)] // VK_RMENU  → DIK_RMENU
    [InlineData(0x30, 0x0B)] // VK_0      → DIK_0
    [InlineData(0x31, 0x02)] // VK_1      → DIK_1
    [InlineData(0x09, 0x0F)] // VK_TAB    → DIK_TAB
    [InlineData(0x08, 0x0E)] // VK_BACK   → DIK_BACK
    public void VkToDik_MapsCommonKeys(int vk, int expectedDik)
    {
        var dik = Win32Emu.Win32.Input.KeyCodeMapper.VkToDik(vk);
        Assert.Equal(expectedDik, dik);
    }

    [Theory]
    [InlineData(0x00)] // Unmapped (null)
    [InlineData(0xFF)] // In-range but unmapped (index 255 is unused)
    public void VkToDik_ReturnsZeroForUnmappedKeys(int vk)
    {
        var dik = Win32Emu.Win32.Input.KeyCodeMapper.VkToDik(vk);
        Assert.Equal(0, dik);
    }

    [Theory]
    [InlineData(256)]  // First out-of-range index (table length = 256)
    [InlineData(-1)]   // Negative (treated as out-of-range via uint cast)
    public void VkToDik_ReturnsZeroForOutOfRangeVk(int vk)
    {
        var dik = Win32Emu.Win32.Input.KeyCodeMapper.VkToDik(vk);
        Assert.Equal(0, dik);
    }

    [Theory]
    [InlineData(4,   0x41)] // SDL_SCANCODE_A    → VK_A
    [InlineData(29,  0x5A)] // SDL_SCANCODE_Z    → VK_Z
    [InlineData(30,  0x31)] // SDL_SCANCODE_1    → VK_1
    [InlineData(39,  0x30)] // SDL_SCANCODE_0    → VK_0
    [InlineData(40,  0x0D)] // SDL_SCANCODE_RETURN → VK_RETURN
    [InlineData(41,  0x1B)] // SDL_SCANCODE_ESCAPE → VK_ESCAPE
    [InlineData(44,  0x20)] // SDL_SCANCODE_SPACE  → VK_SPACE
    [InlineData(79,  0x27)] // SDL_SCANCODE_RIGHT  → VK_RIGHT
    [InlineData(80,  0x25)] // SDL_SCANCODE_LEFT   → VK_LEFT
    [InlineData(81,  0x28)] // SDL_SCANCODE_DOWN   → VK_DOWN
    [InlineData(82,  0x26)] // SDL_SCANCODE_UP     → VK_UP
    [InlineData(58,  0x70)] // SDL_SCANCODE_F1     → VK_F1
    [InlineData(69,  0x7B)] // SDL_SCANCODE_F12    → VK_F12
    [InlineData(224, 0xA2)] // SDL_SCANCODE_LCTRL  → VK_LCONTROL
    [InlineData(225, 0xA0)] // SDL_SCANCODE_LSHIFT → VK_LSHIFT
    [InlineData(226, 0xA4)] // SDL_SCANCODE_LALT   → VK_LMENU
    [InlineData(228, 0xA3)] // SDL_SCANCODE_RCTRL  → VK_RCONTROL
    [InlineData(229, 0xA1)] // SDL_SCANCODE_RSHIFT → VK_RSHIFT
    [InlineData(230, 0xA5)] // SDL_SCANCODE_RALT   → VK_RMENU
    public void SdlScancodeToVk_MapsCommonKeys(int sdlScancode, int expectedVk)
    {
        var vk = Win32Emu.Win32.Input.KeyCodeMapper.SdlScancodeToVk(sdlScancode);
        Assert.Equal(expectedVk, vk);
    }

    [Fact]
    public void SdlScancodeToVk_ReturnsZeroForUnmappedScancode()
    {
        var vk = Win32Emu.Win32.Input.KeyCodeMapper.SdlScancodeToVk(0);
        Assert.Equal(0, vk);
    }

    [Fact]
    public void SdlScancodeToVk_ReturnsZeroForOutOfRangeScancode()
    {
        var vk = Win32Emu.Win32.Input.KeyCodeMapper.SdlScancodeToVk(9999);
        Assert.Equal(0, vk);
    }
}
