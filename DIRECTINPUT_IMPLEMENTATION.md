# DirectInput Implementation

This document describes the implementation of DirectInput support with SDL3 and GLFW input backends for Win32Emu.

## Overview

DirectInput is a Windows API for handling game controller, keyboard, and mouse input. This implementation provides:

1. **Input Backend Abstraction**: Flexible input backend system supporting SDL3 and GLFW
2. **DirectInput API Emulation**: Full emulation of DirectInput interfaces (IDirectInput, IDirectInputDevice)
3. **Controller Presets**: Pre-configured controller profiles matching Windows Server 2003 DirectInput presets
4. **Controller Mapping**: UI for mapping physical controllers to virtual controller configurations
5. **Keyboard and Mouse Support**: Full keyboard and mouse input pass-through via DirectInput

## Architecture

### Input Backend System

```
IInputBackend (interface)
├── SDL3InputBackend
│   └── Uses SDL3-CS for native input support
└── SilkInputBackend
    └── Uses Silk.NET.Input (GLFW-based)
```

#### IInputBackend Interface

The `IInputBackend` interface defines the contract for input backends:

```csharp
public interface IInputBackend : IDisposable
{
    enum DeviceType { Keyboard, Mouse, Gamepad, Joystick }
    
    bool Initialize();
    List<(uint DeviceId, string Name, DeviceType Type)> GetDevices();
    uint OpenDevice(uint deviceId, DeviceType type);
    bool CloseDevice(uint deviceId);
    bool PollDevice(uint deviceId, out InputState? state);
    void ProcessEvents();
    
    event EventHandler<UIEventArgs>? UIEvent;
    bool IsInitialized { get; }
    int DeviceCount { get; }
}
```

#### InputState Class

The `InputState` class tracks the current state of an input device:

```csharp
public class InputState
{
    // Joystick/Gamepad state
    public Dictionary<int, bool> Buttons { get; set; } = new();
    public Dictionary<int, short> Axes { get; set; } = new();
    public int PovHat { get; set; }

    // Keyboard state (256 keys for DirectInput compatibility)
    public Dictionary<int, bool> KeyStates { get; set; } = new();

    // Mouse state
    public int MouseX { get; set; }
    public int MouseY { get; set; }
    public int MouseZ { get; set; } // Scroll wheel
    public Dictionary<int, bool> MouseButtons { get; set; } = new();
}
```

### DirectInput Module

The `DInputModule` class implements the DirectInput API:

```csharp
public class DInputModule : IWin32ModuleUnsafe
{
    public string Name => "DINPUT.DLL";
    
    // Exports
    DirectInputCreateA(...)
    DirectInputCreate(...)
    DirectInput8Create(...)
}
```

#### Device Type Detection

Device types are determined from DirectInput GUIDs:

| GUID | Device Type | Description |
|------|-------------|-------------|
| `6F1D2B61-D5A0-11CF-BFC7-444553540000` | Keyboard | System keyboard |
| `6F1D2B60-D5A0-11CF-BFC7-444553540000` | Mouse | System mouse |
| `6F1D2B70-D5A0-11CF-BFC7-444553540000` | Joystick | Game controller |

#### State Conversion

The module converts backend input state to DirectInput format:

**Keyboard (256 bytes)**:
```
[0x00-0xFF] = key state (0x80 = pressed, 0x00 = not pressed)
```

**Mouse (DIMOUSESTATE structure)**:
```
struct DIMOUSESTATE {
    LONG lX;            // X-axis movement
    LONG lY;            // Y-axis movement
    LONG lZ;            // Wheel movement
    BYTE rgbButtons[4]; // Button states (0x80 = pressed)
}
```

**Joystick (DIJOYSTATE structure)**:
```
struct DIJOYSTATE {
    LONG lX, lY, lZ, lRx; // Axes (0-65535)
    DWORD rgdwPOV[4];     // POV hat states
    BYTE rgbButtons[32];  // Button states (0x80 = pressed)
}
```

### Controller Configuration System

#### ControllerPreset Model

Pre-defined controller configurations based on Windows Server 2003 DirectInput presets:

```csharp
public class ControllerPreset
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int NumberOfAxes { get; set; }
    public int NumberOfButtons { get; set; }
    public bool HasPointOfView { get; set; }
    public Dictionary<string, string> AxisMappings { get; set; }
    public Dictionary<string, string> ButtonMappings { get; set; }
}
```

**Standard Presets**:
- 2-axis, 2-button joystick
- 2-axis, 4-button joystick
- 3-axis, 2-button joystick
- 3-axis, 4-button joystick
- 4-button gamepad
- CH Flightstick / Flightstick Pro / Virtual Pilot
- Gravis Analog Joystick / Analog Pro / Gamepad
- Logitech ThunderPad / WingMan / WingMan Extreme / WingMan Light
- Microsoft SideWinder (Freestyle Pro, game pad, Precision Pro)
- Thrustmaster Flight Control System / Formula T1/T2

#### ControllerConfiguration Model

Stores the mapping between physical and virtual controllers:

```csharp
public class ControllerConfiguration
{
    public uint PhysicalControllerId { get; set; }
    public string PhysicalControllerName { get; set; }
    public string SelectedPreset { get; set; }
    public ControllerPreset? CustomConfiguration { get; set; }
    public Dictionary<int, int> AxisMappings { get; set; }
    public Dictionary<int, int> ButtonMappings { get; set; }
    public int PovHatMapping { get; set; }
}
```

## Configuration

### EmulatorSettings

The emulator settings now include input backend selection:

```csharp
public class EmulatorSettings
{
    public string RenderingBackend { get; set; } = "GLFW";
    public string InputBackend { get; set; } = "GLFW"; // SDL or GLFW
    public Dictionary<string, ControllerConfiguration> ControllerConfigurations { get; set; }
    // ... other settings
}
```

### Backend Selection

The input backend can be selected in the GUI settings:

1. Open **Settings** view
2. Select **Input Backend** (SDL or GLFW)
3. Changes are saved automatically

### Controller Mapping

To map a controller:

1. Open **Controller Mapping** view
2. Select a **Controller Preset** (or choose "Custom")
3. If custom, configure the virtual controller:
   - Controller Type (Joystick, Flight yoke/stick, Game pad, Race car controller)
   - Number of Axes (2-4)
   - Number of Buttons (0-4+)
   - Point of View control (yes/no)
4. Select a **Physical Controller** from the dropdown
5. Map each virtual axis to a physical axis:
   - Click "Map" next to the axis
   - Move the physical axis to assign
6. Map each virtual button to a physical button:
   - Click "Map" next to the button
   - Press the physical button to assign
7. Test the mapping with **Test Controller**
8. Save the configuration with **Save Configuration**

## API Coverage

### Implemented Functions

- ✅ `DirectInputCreateA` - Create DirectInput object (ANSI version)
- ✅ `DirectInputCreate` - Create DirectInput object (Unicode version)
- ✅ `DirectInput8Create` - Create DirectInput8 object

### IDirectInput Interface

- ✅ `QueryInterface` - Query COM interface
- ✅ `AddRef` - Increment reference count
- ✅ `Release` - Decrement reference count
- ✅ `CreateDevice` - Create input device
- 🔄 `EnumDevices` - Enumerate devices (stub)
- 🔄 `GetDeviceStatus` - Get device status (stub)
- 🔄 `RunControlPanel` - Open control panel (stub)
- 🔄 `Initialize` - Initialize DirectInput (stub)

### IDirectInputDevice Interface

- ✅ `QueryInterface` - Query COM interface
- ✅ `AddRef` - Increment reference count
- ✅ `Release` - Decrement reference count
- ✅ `GetCapabilities` - Get device capabilities
- ✅ `EnumObjects` - Enumerate device objects
- ✅ `GetProperty` - Get device property
- ✅ `SetProperty` - Set device property
- ✅ `Acquire` - Acquire device for input
- ✅ `Unacquire` - Release device
- ✅ `GetDeviceState` - Get current device state (wired to backend)
- ✅ `GetDeviceData` - Get buffered device data
- ✅ `SetDataFormat` - Set data format
- ✅ `SetEventNotification` - Set event notification
- ✅ `SetCooperativeLevel` - Set cooperative level
- ✅ `GetObjectInfo` - Get object information
- ✅ `GetDeviceInfo` - Get device information
- 🔄 `RunControlPanel` - Open control panel (stub)
- 🔄 `Initialize` - Initialize device (stub)

Legend: ✅ Implemented | 🔄 Stub | ❌ Not implemented

## Usage Example

### Game Code (Emulated)

```c
// Create DirectInput object
LPDIRECTINPUT8 pDI;
DirectInput8Create(hInstance, DIRECTINPUT_VERSION, 
                   IID_IDirectInput8, (VOID**)&pDI, NULL);

// Create keyboard device
LPDIRECTINPUTDEVICE8 pKeyboard;
pDI->CreateDevice(GUID_SysKeyboard, &pKeyboard, NULL);

// Set data format
pKeyboard->SetDataFormat(&c_dfDIKeyboard);
pKeyboard->SetCooperativeLevel(hwnd, DISCL_FOREGROUND | DISCL_NONEXCLUSIVE);

// Acquire the device
pKeyboard->Acquire();

// Poll keyboard state
BYTE keyState[256];
pKeyboard->GetDeviceState(sizeof(keyState), keyState);

// Check if Escape is pressed
if (keyState[DIK_ESCAPE] & 0x80) {
    // Handle escape key
}
```

### Backend Integration (Win32Emu)

```csharp
// Initialize input backend (done automatically by DInputModule)
env.InputBackend = BackendFactory.CreateInputBackend(logger);
env.InputBackend.Initialize();

// Get available devices
var devices = env.InputBackend.GetDevices();

// Poll device state
if (env.InputBackend.PollDevice(deviceId, out var state))
{
    // State contains keyboard, mouse, or joystick data
    foreach (var (key, pressed) in state.KeyStates)
    {
        Console.WriteLine($"Key {key}: {(pressed ? "pressed" : "released")}");
    }
}
```

## Testing

Comprehensive tests are included in `Win32Emu.Tests.Emulator/DInputModuleTests.cs`:

```bash
# Run all DInput tests
dotnet test --filter "FullyQualifiedName~DInputModuleTests"
```

**Test Coverage**:
- DirectInputCreateA/Create/8Create success
- Input backend initialization
- Device enumeration
- Device open/close operations
- Keyboard state tracking
- Mouse state tracking
- Joystick state tracking

## Backend Comparison

| Feature | SDL3InputBackend | SilkInputBackend |
|---------|------------------|------------------|
| Keyboard | ✅ Event-based | ✅ Cached state |
| Mouse | ✅ Event-based | ✅ Cached state |
| Joystick | ✅ Direct polling | 🔄 Requires window context |
| Gamepad | ✅ Native support | 🔄 Via GLFW |
| Hot-plug | ✅ Yes | ❌ Limited |
| Platform | All | All |
| Performance | Excellent | Good |

**Recommendations**:
- Use **SDL3InputBackend** (default) for best compatibility and performance
- Use **SilkInputBackend** when GLFW rendering backend is required

## Future Enhancements

1. **Force Feedback**: Implement force feedback effects for joysticks
2. **Action Mapping**: High-level action mapping system (e.g., "Jump" → Space/Button A)
3. **Profile Management**: Save/load controller profiles per game
4. **Controller Testing UI**: Real-time visual feedback for controller testing
5. **Buffered Input**: Implement GetDeviceData with event buffering
6. **Enumeration**: Complete implementation of EnumDevices callback
7. **Multiple Controllers**: Support for multiple controllers simultaneously

## References

- [DirectInput Documentation (MSDN)](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416842(v=vs.85))
- [Windows Server 2003 DirectInput Presets](https://github.com/selfrender/Windows-Server-2003/tree/5c6fe3db626b63a384230a1aa6b92ac416b0765f/multimedia/directx/dinput/ihvmap)
- [SDL3 Input Handling](https://wiki.libsdl.org/SDL3/CategoryGamepad)
- [GLFW Input Guide](https://www.glfw.org/docs/latest/input_guide.html)

## Troubleshooting

### Controller not detected

1. Check that the input backend is initialized (should happen automatically)
2. Verify the physical controller is connected and recognized by the OS
3. Try switching between SDL and GLFW backends in Settings

### Keyboard/Mouse not working

1. Ensure the game window has focus
2. Check that SetCooperativeLevel was called with appropriate flags
3. Verify the device was acquired with Acquire()

### Incorrect button mappings

1. Open Controller Mapping view
2. Re-map the buttons using the UI
3. Test the configuration with Test Controller
4. Save the configuration

### Performance issues

1. Switch to SDL3InputBackend for better performance
2. Reduce polling frequency if custom polling is used
3. Check for input event processing bottlenecks
