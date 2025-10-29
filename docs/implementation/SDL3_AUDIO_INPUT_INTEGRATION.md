# SDL3 Audio and Input Integration

This document describes the SDL3 audio and input integration for DirectSound and DirectInput support in Win32Emu.

## Overview

Building on the existing SDL3 graphics integration, this implementation adds:
- **Audio support** via SDL3 Audio subsystem for DirectSound emulation
- **Input support** via SDL3 Gamepad/Joystick subsystems for DirectInput emulation

## Architecture

### SDL3AudioBackend (`Win32Emu/Rendering/SDL3AudioBackend.cs`)

The audio backend provides:

**Features:**
- SDL3 audio subsystem initialization
- Audio stream management with configurable frequency, channels, and buffer size
- Audio data queuing for playback
- Volume control per stream
- Pause/resume functionality
- Thread-safe operations

**API:**
- `Initialize()` - Initialize SDL3 audio subsystem
- `CreateAudioStream(frequency, channels, bufferSize)` - Create audio stream
- `WriteAudioData(streamId, data, offset, length)` - Queue audio data
- `DestroyAudioStream(streamId)` - Destroy audio stream
- `SetStreamVolume(streamId, volume)` - Set volume (0.0 to 1.0)
- `SetStreamPaused(streamId, paused)` - Pause/resume stream

### SDL3InputBackend (`Win32Emu/Rendering/SDL3InputBackend.cs`)

The input backend provides:

**Features:**
- SDL3 gamepad and joystick subsystem initialization
- Device enumeration (keyboard, mouse, gamepads, joysticks)
- Device opening/closing
- Input state polling (buttons, axes, POV hats)
- Hot-plug support for controllers
- Thread-safe operations

**Device Types:**
- Keyboard (always available)
- Mouse (always available)
- Gamepad (SDL gamepad API - standardized mapping)
- Joystick (SDL joystick API - raw input)

**API:**
- `Initialize()` - Initialize SDL3 input subsystems
- `GetDevices()` - Enumerate available input devices
- `OpenDevice(deviceId, type)` - Open device for reading
- `CloseDevice(deviceId)` - Close device
- `PollDevice(deviceId, out state)` - Poll current input state
- `ProcessEvents()` - Handle device hot-plug events

### DirectSound Integration (`DSoundModule`)

Located in `Win32Emu/Win32/IWin32ModuleUnsafe.cs`, provides:

**Implemented Functions:**
- `DirectSoundCreate` - Creates DirectSound interface objects
- `DirectSoundEnumerateA` - Enumerates audio devices

**Implementation Details:**
- DirectSound objects are tracked with handle management
- Audio backend is lazily initialized on first DirectSound creation
- Buffer management infrastructure ready for expansion

**Future Functions:**
- `IDirectSound::SetCooperativeLevel` - Set priority level
- `IDirectSound::CreateSoundBuffer` - Create primary/secondary buffers
- `IDirectSoundBuffer::Lock` - Lock buffer for writing
- `IDirectSoundBuffer::Unlock` - Unlock buffer
- `IDirectSoundBuffer::Play` - Start playback
- `IDirectSoundBuffer::Stop` - Stop playback
- `IDirectSoundBuffer::SetVolume` - Set buffer volume
- `IDirectSoundBuffer::SetFrequency` - Set playback frequency

### DirectInput Integration (`DInputModule`)

Located in `Win32Emu/Win32/IWin32ModuleUnsafe.cs`, provides:

**Implemented Functions:**
- `DirectInputCreate` / `DirectInputCreateA` - Creates DirectInput interface
- `DirectInput8Create` - Creates DirectInput8 interface

**Implementation Details:**
- DirectInput objects are tracked with version information
- Input backend is lazily initialized on first DirectInput creation
- Device tracking infrastructure ready for expansion

**Future Functions:**
- `IDirectInput::EnumDevices` - Enumerate input devices
- `IDirectInput::CreateDevice` - Create device interface
- `IDirectInputDevice::SetDataFormat` - Set data format
- `IDirectInputDevice::SetCooperativeLevel` - Set cooperation level
- `IDirectInputDevice::Acquire` - Acquire device for exclusive use
- `IDirectInputDevice::GetDeviceState` - Get current device state
- `IDirectInputDevice::GetDeviceData` - Get buffered device data
- `IDirectInputDevice::Unacquire` - Release device

## Integration with ProcessEnvironment

The `ProcessEnvironment` class now maintains optional references to:
- `AudioBackend` - SDL3AudioBackend instance
- `InputBackend` - SDL3InputBackend instance

These backends are created on-demand when applications call DirectSound or DirectInput APIs.

## Subsystem Management

Each SDL3 subsystem is independently managed:
- **Video** - Managed by SDL3RenderingBackend
- **Audio** - Managed by SDL3AudioBackend
- **Gamepad/Joystick** - Managed by SDL3InputBackend

This allows selective initialization based on what the emulated application uses.

## Data Flow

### DirectSound Audio Path

```
Win32 App
    ↓
DirectSoundCreate() → DSoundModule
    ↓
Initialize AudioBackend
    ↓
SDL.Init(Audio)
    ↓
CreateSoundBuffer (future) → CreateAudioStream
    ↓
Lock/Write/Unlock (future) → WriteAudioData
    ↓
Play (future) → SDL Audio Playback
```

### DirectInput Input Path

```
Win32 App
    ↓
DirectInputCreate() → DInputModule
    ↓
Initialize InputBackend
    ↓
SDL.Init(Gamepad | Joystick)
    ↓
EnumDevices (future) → GetDevices
    ↓
CreateDevice (future) → OpenDevice
    ↓
GetDeviceState (future) → PollDevice
    ↓
Return button/axis states
```

## Testing Strategy

1. **Unit Tests**: Test individual backend APIs
2. **Integration Tests**: Test DirectSound/DirectInput API implementations
3. **Sample Programs**: Create test programs using DirectSound/DirectInput
4. **Game Testing**: Test with actual legacy games that use these APIs

## Future Enhancements

### Audio
- Full SDL3 audio stream implementation with actual playback
- 3D positional audio support
- Audio effects (reverb, echo, etc.)
- Multiple simultaneous sound buffers
- Hardware-accelerated audio paths

### Input
- Force feedback support for gamepads
- Custom device mappings
- Input recording and playback
- Multiple simultaneous device support
- Keyboard and mouse state tracking

### Networking (DirectPlay)
DirectPlay networking support is not included in this implementation because:
- SDL3 does not provide networking APIs
- DirectPlay is deprecated and rarely used
- Modern alternatives (sockets) are simpler

If DirectPlay support is needed, it would require:
- Direct socket implementation using .NET networking
- Protocol implementation for DirectPlay message formats
- NAT traversal and matchmaking infrastructure

## References

- SDL3-CS: https://github.com/edwardgushchin/SDL3-CS
- SDL3 Documentation: https://wiki.libsdl.org/SDL3/
- DirectSound Documentation: MSDN DirectSound API Reference
- DirectInput Documentation: MSDN DirectInput API Reference
- Issue #19: https://github.com/archanox/Win32Emu/issues/19
