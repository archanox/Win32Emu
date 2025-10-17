# Silk.NET Backend Migration

## Overview

Win32Emu has migrated from SDL3-CS to Silk.NET for multimedia backends. This change provides:

- **Better cross-platform compatibility**: Silk.NET has excellent support for macOS, Linux, and Windows
- **Multiple backend options**: Choose between SDL and GLFW for windowing/rendering
- **Consistent audio**: OpenAL for all platforms
- **Flexible input**: Unified input abstraction across backends

## Architecture

### Backend Interfaces

Three main interfaces define the backend contracts:

1. **IRenderingBackend** - Window management and frame buffer rendering
2. **IAudioBackend** - Audio stream management (DirectSound emulation)
3. **IInputBackend** - Input device enumeration and polling (DirectInput emulation)

### Available Backends

#### Rendering Backends

- **SilkSdlRenderingBackend** - Uses Silk.NET.SDL for windowing and rendering
  - Best compatibility with older systems
  - Hardware-accelerated rendering
  - Default backend

- **SilkGlfwRenderingBackend** - Uses Silk.NET.GLFW + OpenGL
  - Modern OpenGL rendering pipeline
  - Good for systems where SDL has issues
  - Alternative for macOS

#### Audio Backend

- **SilkOpenAlAudioBackend** - Uses Silk.NET.OpenAL
  - Cross-platform audio support
  - Used for all rendering backend types

#### Input Backend

- **SilkInputBackend** - Uses Silk.NET.Input
  - Provides keyboard and mouse support
  - Extensible for gamepad/joystick support

## Configuration

### Backend Selection Priority

Backends are selected in this order:

1. **Explicit setting** in code: `BackendFactory.CurrentBackendType = BackendType.GLFW;`
2. **Command-line argument**: `--backend GLFW`
3. **Environment variable**: `WIN32EMU_BACKEND=GLFW`
4. **Default**: SDL

### Command-Line Usage

```bash
# Use default SDL backend
Win32Emu game.exe

# Use GLFW backend
Win32Emu game.exe --backend GLFW

# Use SDL backend explicitly
Win32Emu game.exe --backend SDL
```

### Environment Variable

```bash
# Set backend for all runs
export WIN32EMU_BACKEND=GLFW
Win32Emu game.exe

# Or on Windows
set WIN32EMU_BACKEND=GLFW
Win32Emu game.exe
```

### Programmatic Configuration

```csharp
using Win32Emu.Rendering;

// Set backend before creating any backends
BackendFactory.CurrentBackendType = BackendType.GLFW;

// Backends are created automatically by modules
// DDrawModule, DSoundModule, and DInputModule use the factory
```

## Migration from SDL3

### Changes for Users

- **No breaking changes** for most users - the default SDL backend works the same way
- **New option**: Try `--backend GLFW` if SDL has issues on your system
- **Environment variable**: Set `WIN32EMU_BACKEND` for persistent configuration

### Changes for Developers

#### Before (SDL3-CS)

```csharp
using SDL3;

var backend = new Sdl3RenderingBackend(logger);
backend.Initialize(640, 480);
```

#### After (Silk.NET)

```csharp
using Win32Emu.Rendering;

// Factory creates the appropriate backend based on configuration
var backend = BackendFactory.CreateRenderingBackend(logger);
backend.Initialize(640, 480);
```

### Removed Classes

- `Sdl3RenderingBackend` → Use `BackendFactory.CreateRenderingBackend()`
- `Sdl3AudioBackend` → Use `BackendFactory.CreateAudioBackend()`
- `Sdl3InputBackend` → Use `BackendFactory.CreateInputBackend()`
- `Sdl3Initializer` → No longer needed

### New Classes

- `IRenderingBackend` - Interface for rendering backends
- `IAudioBackend` - Interface for audio backends
- `IInputBackend` - Interface for input backends
- `BackendType` - Enumeration of backend types
- `BackendFactory` - Factory for creating backends
- `SilkSdlRenderingBackend` - SDL implementation
- `SilkGlfwRenderingBackend` - GLFW implementation
- `SilkOpenAlAudioBackend` - OpenAL implementation
- `SilkInputBackend` - Input implementation

## Dependencies

### NuGet Packages

The following Silk.NET packages are now used:

- `Silk.NET.SDL` (2.22.0) - SDL3 bindings for windowing and rendering
- `Silk.NET.GLFW` (2.22.0) - GLFW bindings for windowing
- `Silk.NET.OpenGL` (2.22.0) - OpenGL bindings for rendering
- `Silk.NET.OpenAL` (2.22.0) - OpenAL bindings for audio
- `Silk.NET.Input` (2.22.0) - Input abstraction

### Removed Packages

- `SDL3-CS` (3.2.24)
- `SDL3-CS.Native` (3.2.24)

## Testing

All backend tests have been updated and pass:

```bash
dotnet test --filter "FullyQualifiedName~SilkBackendTests"
```

Test coverage includes:
- Backend initialization
- Backend disposal
- Audio stream creation
- Audio data writing
- Input device enumeration
- Graceful handling of missing libraries (CI environments)

## Troubleshooting

### SDL Backend Issues on macOS

If you experience issues with the SDL backend on macOS:

1. Try the GLFW backend: `--backend GLFW`
2. Ensure you have the latest Silk.NET.SDL native binaries
3. Check console output for initialization errors

### Missing Native Libraries

If you see `DllNotFoundException` or `FileNotFoundException`:

1. Install required native libraries:
   - **SDL**: Install SDL3 for your platform
   - **OpenAL**: Install OpenAL for your platform
   - **GLFW**: Install GLFW3 for your platform

2. On Linux:
   ```bash
   sudo apt-get install libsdl3-dev libopenal-dev libglfw3-dev
   ```

3. On macOS:
   ```bash
   brew install sdl3 openal-soft glfw
   ```

### Backend Selection Not Working

Verify backend selection order:

1. Check if explicitly set in code
2. Check command-line arguments
3. Check environment variable
4. Default (SDL) will be used if none specified

## Future Enhancements

Potential future improvements:

- Additional rendering backends (e.g., Vulkan, Metal)
- Enhanced gamepad/joystick support in input backend
- Configuration file support for backend selection
- Runtime backend switching
- Per-application backend preferences

## References

- [Silk.NET Documentation](https://github.com/dotnet/Silk.NET)
- [SDL3 Documentation](https://wiki.libsdl.org/SDL3)
- [GLFW Documentation](https://www.glfw.org/documentation.html)
- [OpenAL Documentation](https://www.openal.org/documentation/)
