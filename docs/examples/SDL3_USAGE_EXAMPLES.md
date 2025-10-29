# SDL3 Audio and Input Usage Examples

This document provides usage examples for the new SDL3 audio and input backends.

## DirectSound Audio Example

```csharp
// Example: Using DirectSound API in emulated Win32 code

// Create DirectSound object
LPDIRECTSOUND lpDS;
HRESULT hr = DirectSoundCreate(NULL, &lpDS, NULL);
if (SUCCEEDED(hr)) {
    Console.WriteLine("DirectSound created successfully!");
    
    // Future: Create sound buffer, write audio data, and play
    // DSBUFFERDESC dsbdesc;
    // LPDIRECTSOUNDBUFFER lpDSBuffer;
    // hr = lpDS->CreateSoundBuffer(&dsbdesc, &lpDSBuffer, NULL);
}

// Behind the scenes:
// 1. DSoundModule.DirectSoundCreate() is called
// 2. SDL3AudioBackend is initialized if not already done
// 3. SDL.Init(SDL.InitFlags.Audio) is called
// 4. DirectSound handle is returned to the application
```

## C# Backend Usage Example

```csharp
using Win32Emu.Rendering;

// Direct usage of SDL3AudioBackend
using var audioBackend = new SDL3AudioBackend();

// Initialize audio subsystem
if (audioBackend.Initialize())
{
    Console.WriteLine("Audio backend initialized");
    
    // Create an audio stream for 44.1kHz stereo audio
    var streamId = audioBackend.CreateAudioStream(
        frequency: 44100,
        channels: 2,
        bufferSize: 4096
    );
    
    if (streamId != 0)
    {
        Console.WriteLine($"Created audio stream {streamId}");
        
        // Write audio data
        byte[] audioData = new byte[4096];
        // ... fill audioData with PCM samples ...
        audioBackend.WriteAudioData(streamId, audioData, 0, audioData.Length);
        
        // Control playback
        audioBackend.SetStreamVolume(streamId, 0.8f); // 80% volume
        audioBackend.SetStreamPaused(streamId, false); // Start playback
        
        // Later: cleanup
        audioBackend.DestroyAudioStream(streamId);
    }
}
```

## DirectInput Example

```csharp
// Example: Using DirectInput API in emulated Win32 code

// Create DirectInput object
LPDIRECTINPUT lpDI;
HRESULT hr = DirectInputCreate(hInstance, DIRECTINPUT_VERSION, &lpDI, NULL);
if (SUCCEEDED(hr)) {
    Console.WriteLine("DirectInput created successfully!");
    
    // Future: Enumerate devices, create device interface
    // lpDI->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumDevicesCallback, NULL, DIEDFL_ATTACHEDONLY);
}

// Behind the scenes:
// 1. DInputModule.DirectInputCreate() is called
// 2. SDL3InputBackend is initialized if not already done
// 3. SDL.Init(SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick) is called
// 4. DirectInput handle is returned to the application
```

## C# Input Backend Usage Example

```csharp
using Win32Emu.Rendering;

// Direct usage of SDL3InputBackend
using var inputBackend = new SDL3InputBackend();

// Initialize input subsystem
if (inputBackend.Initialize())
{
    Console.WriteLine("Input backend initialized");
    
    // Get list of available devices
    var devices = inputBackend.GetDevices();
    foreach (var device in devices)
    {
        Console.WriteLine($"Device {device.DeviceId}: {device.Name} ({device.Type})");
    }
    
    // Open a gamepad device
    var gamepadDevice = devices.FirstOrDefault(d => d.Type == SDL3InputBackend.DeviceType.Gamepad);
    if (gamepadDevice != default)
    {
        var deviceId = inputBackend.OpenDevice(gamepadDevice.DeviceId, gamepadDevice.Type);
        
        if (deviceId != 0)
        {
            Console.WriteLine($"Opened gamepad {deviceId}");
            
            // Poll input state
            if (inputBackend.PollDevice(deviceId, out var state) && state != null)
            {
                // Check button states
                foreach (var button in state.Buttons)
                {
                    if (button.Value)
                    {
                        Console.WriteLine($"Button {button.Key} is pressed");
                    }
                }
                
                // Check axis values
                foreach (var axis in state.Axes)
                {
                    Console.WriteLine($"Axis {axis.Key} = {axis.Value}");
                }
                
                // Check POV hat
                Console.WriteLine($"POV Hat = {state.PovHat}");
            }
            
            // Process hot-plug events
            inputBackend.ProcessEvents();
            
            // Later: cleanup
            inputBackend.CloseDevice(deviceId);
        }
    }
}
```

## Integration with ProcessEnvironment

```csharp
using Win32Emu.Win32;
using Win32Emu.Memory;

// Create process environment
var vm = new VirtualMemory(0x100000000); // 4GB address space
var env = new ProcessEnvironment(vm);

// Audio and input backends are created on-demand
// when DirectSound or DirectInput APIs are called

// Example: When emulated code calls DirectSoundCreate()
// env.AudioBackend is automatically initialized by DSoundModule

// You can also pre-initialize them if needed
env.AudioBackend = new Win32Emu.Rendering.SDL3AudioBackend();
env.AudioBackend.Initialize();

env.InputBackend = new Win32Emu.Rendering.SDL3InputBackend();
env.InputBackend.Initialize();

// Now emulated code can use DirectSound and DirectInput
```

## Multi-Subsystem Usage

```csharp
// SDL3 subsystems can be used together
using var renderBackend = new SDL3RenderingBackend();
using var audioBackend = new SDL3AudioBackend();
using var inputBackend = new SDL3InputBackend();

// Initialize all subsystems
renderBackend.Initialize(800, 600, "My Game");
audioBackend.Initialize();
inputBackend.Initialize();

// Each subsystem manages its own SDL init flags
// Video: SDL.InitFlags.Video
// Audio: SDL.InitFlags.Audio
// Input: SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick

// Game loop
while (true)
{
    // Process input
    inputBackend.ProcessEvents();
    
    // Update game logic based on input
    if (inputBackend.PollDevice(deviceId, out var state))
    {
        // Handle input
    }
    
    // Play audio
    audioBackend.WriteAudioData(streamId, audioData, 0, audioData.Length);
    
    // Render graphics
    renderBackend.UpdateFrameBuffer(frameBuffer, pitch);
    renderBackend.ProcessEvents();
    
    // Check for quit
    // ...
}
```

## Error Handling

```csharp
using Win32Emu.Rendering;

try
{
    using var audioBackend = new SDL3AudioBackend();
    
    if (!audioBackend.Initialize())
    {
        Console.WriteLine("Failed to initialize audio backend");
        // SDL3 audio subsystem not available or failed to initialize
        // This can happen if:
        // - SDL3 native library is not installed
        // - No audio devices are available on the system
        // - Audio subsystem is already in use
        return;
    }
    
    // Use audio backend...
}
catch (DllNotFoundException ex)
{
    Console.WriteLine($"SDL3 library not found: {ex.Message}");
    // SDL3-CS bindings require native SDL3 library to be installed
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Platform Considerations

### Windows
- SDL3 provides DirectX, WASAPI, and WinMM audio backends
- SDL3 supports XInput and DirectInput for gamepad support
- Native SDL3.dll must be in PATH or application directory

### Linux
- SDL3 provides PulseAudio, ALSA, and JACK audio backends
- SDL3 supports Linux gamepad/joystick subsystems
- Native libSDL3.so must be in LD_LIBRARY_PATH or standard library paths

### macOS
- SDL3 provides CoreAudio backend
- SDL3 supports macOS gamepad/joystick subsystems
- Native libSDL3.dylib must be in DYLD_LIBRARY_PATH or standard library paths

## Performance Tips

1. **Lazy Initialization**: Only initialize subsystems when needed
2. **Buffer Size**: Use appropriate audio buffer sizes (2048-4096 bytes typical)
3. **Polling Frequency**: Poll input at consistent intervals (60Hz typical)
4. **Event Processing**: Call ProcessEvents() regularly to handle hot-plug
5. **Cleanup**: Always dispose backends when done to free resources
