# DirectSound Implementation with Silk.NET OpenAL

## Overview

This document describes the implementation of DirectSound (dsound.dll) support using Silk.NET OpenAL as the audio backend, inspired by the dsoal project (https://github.com/kcat/dsoal).

## Architecture

### Components

1. **DSoundModule** (`Win32Emu/Win32/Modules/DSoundModule.cs`)
   - Implements DirectSound COM interfaces (IDirectSound, IDirectSoundBuffer)
   - Manages sound buffers and their state
   - Translates DirectSound calls to OpenAL operations

2. **SilkOpenAlAudioBackend** (`Win32Emu/Rendering/SilkOpenAlAudioBackend.cs`)
   - Provides cross-platform audio playback via OpenAL
   - Manages OpenAL sources and buffers
   - Handles audio streaming

3. **IAudioBackend** (`Win32Emu/Rendering/IAudioBackend.cs`)
   - Abstract interface for audio backends
   - Allows switching between SDL3 Audio and OpenAL

## Implementation Details

### DirectSound Buffer Lifecycle

1. **Buffer Creation** (`CreateSoundBuffer`)
   - Parses `DSBUFFERDESC` structure to get buffer size and flags
   - Parses `WAVEFORMATEX` structure to get audio format (channels, frequency, bits per sample)
   - Creates a `DirectSoundBuffer` object with allocated memory
   - Returns a COM object with vtable for `IDirectSoundBuffer` methods

2. **Buffer Locking** (`Lock`)
   - Allocates memory region in emulated address space
   - Copies current buffer data to the locked region
   - Returns pointer and size to the application
   - Supports entire buffer locking via `DSBLOCK_ENTIREBUFFER` flag

3. **Buffer Unlocking** (`Unlock`)
   - Copies data from locked memory region back to buffer
   - Updates buffer content with application-provided audio data
   - Handles wraparound for circular buffers

4. **Playback** (`Play`)
   - Creates OpenAL audio stream if not already created
   - Writes buffer data to OpenAL backend
   - Starts audio playback
   - Supports looping via `DSBPLAY_LOOPING` flag

5. **Stopping** (`Stop`)
   - Pauses the OpenAL audio stream
   - Updates buffer state

### Audio Format Support

The implementation supports PCM audio formats with the following parameters:
- **Channels**: Mono (1) or Stereo (2)
- **Sample Rate**: Any rate (typically 22050Hz, 44100Hz, or 48000Hz)
- **Bits Per Sample**: 8-bit or 16-bit

### Volume Control

DirectSound uses a logarithmic volume scale in hundredths of decibels:
- `0` = Full volume (0 dB)
- `-10000` = Silence (-100 dB)

The implementation converts this to a linear 0.0-1.0 scale for OpenAL:
```csharp
var normalizedVolume = lVolume >= 0 ? 1.0f : Math.Max(0.0f, 1.0f + (lVolume / 10000.0f));
```

### COM Interface Implementation

DirectSound uses COM interfaces for object-oriented API design. The implementation:

1. **IDirectSound** vtable methods:
   - QueryInterface, AddRef, Release (standard COM methods)
   - CreateSoundBuffer (creates sound buffers)
   - GetCaps, SetCooperativeLevel, Compact (device management)
   - GetSpeakerConfig, SetSpeakerConfig (audio configuration)
   - Initialize (device initialization)

2. **IDirectSoundBuffer** vtable methods:
   - QueryInterface, AddRef, Release (standard COM methods)
   - Lock, Unlock (buffer access)
   - Play, Stop (playback control)
   - GetFormat, SetFormat (audio format)
   - GetVolume, SetVolume (volume control)
   - GetPan, SetPan (stereo positioning)
   - GetFrequency, SetFrequency (sample rate control)
   - GetCurrentPosition (playback cursor)
   - GetStatus (buffer state)
   - GetCaps (buffer capabilities)

## State Tracking

Each DirectSound buffer maintains the following state:

```csharp
public sealed class DirectSoundBuffer
{
    public uint Handle { get; set; }           // Internal buffer handle
    public uint AudioStreamId { get; set; }    // OpenAL stream ID
    public int Size { get; set; }              // Buffer size in bytes
    public byte[]? Data { get; set; }          // Audio data
    public bool IsPrimary { get; set; }        // Primary vs secondary buffer
    public int Frequency { get; set; }         // Sample rate (Hz)
    public int Channels { get; set; }          // 1 = mono, 2 = stereo
    public int BitsPerSample { get; set; }     // 8 or 16 bits
    public int Volume { get; set; }            // Volume in dB*100
    public int Pan { get; set; }               // Pan (-10000 to 10000)
    public uint PlayCursor { get; set; }       // Current play position
    public uint WriteCursor { get; set; }      // Current write position
    public bool IsPlaying { get; set; }        // Playback state
    public bool IsLooping { get; set; }        // Loop state
}
```

## Backend Selection

The audio backend is selected at runtime based on the rendering backend:

```csharp
BackendFactory.CreateAudioBackend(logger)
```

- **SDL backend**: Uses SDL3 native audio
- **GLFW/Vulkan backends**: Uses Silk.NET OpenAL

## Testing

The implementation includes unit tests in `Win32Emu.Tests.User32/MultimediaTests.cs`:

1. **DirectSoundCreate_ShouldReturnSuccess**
   - Tests basic DirectSound object creation
   - Verifies COM object pointer is returned

2. **DirectSoundEnumerateA_ShouldReturnSuccess**
   - Tests device enumeration with null callback
   - Verifies proper return value

## Compatibility

The implementation is compatible with applications that use:
- DirectSound 7 and earlier APIs
- PCM audio formats (8-bit and 16-bit)
- Mono and stereo audio
- Standard sample rates (22050Hz, 44100Hz, 48000Hz)

## Known Limitations

1. **Primary Buffers**: Primary buffers are created but don't require actual audio output
2. **3D Audio**: 3D positioning, velocity, and distance attenuation are not yet implemented
3. **Effects**: DirectSound effects (reverb, chorus, etc.) are not implemented
4. **Capture**: DirectSoundCapture APIs are stubbed out but not functional
5. **Pan Control**: Pan settings are stored but not applied to OpenAL (requires stereo source positioning)

## References

- [dsoal Project](https://github.com/kcat/dsoal) - DirectSound to OpenAL wrapper that inspired this implementation
- [DirectSound Documentation](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ee416960(v=vs.85)) - Official Microsoft DirectSound documentation
- [Silk.NET OpenAL](https://github.com/dotnet/Silk.NET) - .NET bindings for OpenAL

## Future Improvements

1. Implement 3D audio positioning using OpenAL's spatial audio features
2. Add support for DirectSound effects via OpenAL effects extension
3. Implement DirectSoundCapture for audio recording
4. Add support for compressed audio formats
5. Improve pan control with proper stereo positioning
6. Add performance optimizations for streaming audio

## Summary

This implementation provides a functional DirectSound to OpenAL translation layer, allowing Win32 applications that use DirectSound for audio playback to run on modern cross-platform systems. The architecture is modular and extensible, making it easy to add additional features and optimizations in the future.
