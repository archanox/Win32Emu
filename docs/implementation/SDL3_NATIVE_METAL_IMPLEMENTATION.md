# SDL3 Native Metal Support Implementation

## Overview

This implementation adds native Metal support on macOS through the SDL3-CS backend, providing hardware-accelerated rendering using platform-native graphics APIs.

## Implementation Details

### Components Added

1. **SDL3Initializer.cs**
   - Static helper class ensuring SDL app metadata is set before any SDL initialization
   - Critical for macOS where `SetAppMetadata()` must be called before ANY `SDL.Init()` call
   - Thread-safe initialization using locks

2. **SDL3RenderingBackend.cs**
   - Implements `IRenderingBackend` interface
   - Uses SDL3 GPU API for hardware-accelerated rendering:
     - **macOS**: Metal backend
     - **Linux**: Vulkan backend
     - **Windows**: DirectX 12 backend
   - Features:
     - GPU device creation with auto-selected driver
     - Window management with GPU device claiming
     - Frame texture creation and transfer buffer management
     - Command buffer-based rendering pipeline
     - Event processing (window events, focus changes)
     - Format conversion utilities (8-bit palettized, 16-bit RGB565, 24-bit RGB)

3. **SDL3AudioBackend.cs**
   - Implements `IAudioBackend` interface
   - Native audio stream support using SDL3
   - Features:
     - Audio stream creation with custom sample rate, channels, and buffer size
     - Direct audio data writing to streams
     - Volume control and pause/resume functionality
     - Proper cleanup and disposal

4. **SDL3InputBackend.cs**
   - Implements `IInputBackend` interface
   - Keyboard, mouse, and joystick support
   - Features:
     - Device enumeration (keyboard, mouse, joysticks)
     - Joystick state polling (axes, buttons, POV hat)
     - Device lifecycle management

5. **BackendFactory.cs** (Updated)
   - SDL backend now default (changed from GLFW)
   - Factory methods for creating backend instances based on selected type
   - Environment variable support (`WIN32EMU_BACKEND`)

6. **BackendType.cs** (Updated)
   - Added `SDL` enum value
   - SDL is first in the enum to indicate it's the preferred option

### Testing

Created comprehensive test suite in `Sdl3BackendTests.cs`:
- 10 unit tests covering all three backends
- Tests gracefully handle missing SDL3 library in CI environments
- All tests passing

Test Coverage:
- SDL3AudioBackend initialization
- SDL3AudioBackend stream creation and data writing
- SDL3InputBackend initialization and device enumeration
- SDL3RenderingBackend initialization and frame buffer updates
- Proper disposal for all backends

### API Compatibility

All SDL3-CS types use:
- Pascal case for struct fields (e.g., `Type`, `Format`, `Width`, `Height`)
- Types defined in `SDL` class within `SDL3` namespace
- Proper enum values (e.g., `SDL.GPUTextureType.Texturetype2D`)
- Correct method signatures with marshaling attributes

### Documentation

Updated README.md to reflect:
- SDL as default backend
- Platform-specific backend selection (Metal/Vulkan/DirectX 12)
- SDL audio and input backend usage
- Configuration options

## Benefits

### Platform-Specific
- **macOS**: Native Metal backend for optimal performance and compatibility
- **Linux**: Native Vulkan backend for modern GPU support
- **Windows**: Native DirectX 12 backend for Windows 10/11

### General
- Hardware-accelerated rendering on all platforms
- Modern, future-proof GPU API
- Better resource management with explicit GPU command buffers
- Backward compatible API surface
- No breaking changes to existing code
- SDL is now the default, providing better out-of-box experience

## Usage

### Default (SDL Backend - Metal on macOS)
```bash
Win32Emu game.exe
```

### Explicit Backend Selection
```bash
Win32Emu game.exe --backend SDL
Win32Emu game.exe --backend GLFW
Win32Emu game.exe --backend Vulkan
```

### Environment Variable
```bash
export WIN32EMU_BACKEND=SDL
Win32Emu game.exe
```

## Build Status

- ✅ Clean build with no errors
- ✅ 10/10 SDL3 backend tests passing
- ✅ No security vulnerabilities detected (CodeQL)
- ✅ Only pre-existing warnings remain
- ✅ No new code analysis warnings introduced

## Security Summary

CodeQL analysis completed successfully with **0 alerts** found in the new SDL3 backend code. No security vulnerabilities introduced.

## Future Enhancements

Potential improvements that could be made:
1. **Shader Support**: Add custom shader support for advanced effects
2. **Multi-texture**: Support multiple render targets
3. **3D Rendering**: Leverage GPU API for 3D graphics emulation
4. **Compute Shaders**: Use compute passes for image processing
5. **HDR Support**: Add high dynamic range rendering support

## References

- SDL3-CS Repository: https://github.com/edwardgushchin/SDL3-CS
- SDL3 GPU Documentation: https://wiki.libsdl.org/SDL3/CategoryGPU
- Issue: Native Metal support on macOS
