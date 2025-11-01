# Metal Rendering Backend

## Overview

Win32Emu now supports native Metal rendering on macOS using the [SharpMetal](https://github.com/IsaacMarovitz/SharpMetal) library. This provides hardware-accelerated graphics rendering using Apple's Metal API.

## Requirements

- **Platform**: macOS only
- **Package**: SharpMetal v1.0.0
- **Runtime**: .NET 9.0

## Usage

### Via Environment Variable

Set the `WIN32EMU_BACKEND` environment variable to enable the Metal backend:

```bash
export WIN32EMU_BACKEND=Metal
./Win32Emu [your-executable.exe]
```

### Programmatically

Set the backend type before creating rendering instances:

```csharp
using Win32Emu.Rendering;

BackendFactory.CurrentBackendType = BackendType.Metal;
var renderer = BackendFactory.CreateRenderingBackend(logger);
```

## Features

### Basic Features

- **Native Metal Rendering**: Utilizes Apple's Metal API for optimal performance on macOS
- **Full IRenderingBackend Implementation**: Compatible with all Win32Emu rendering requirements
- **Pixel Format Support**: Converts 8-bit palettized, 16-bit RGB565, and 24-bit RGB/BGR to RGBA
- **GLFW Integration**: Uses GLFW for window management and attaches Metal layers to native NSView
- **Resource Management**: Proper disposal of Metal resources (devices, command queues, textures, buffers)

### Advanced Features (NEW)

The Metal backend has been significantly enhanced with GPU-accelerated advanced features:

- **Custom Shader Support**: Load and compile Metal shaders at runtime for custom effects
- **Multiple Render Targets (MRT)**: Render to up to 8 textures simultaneously for deferred shading
- **3D Graphics Emulation**: Full 3D rendering pipeline with vertex/index buffers, depth testing, and transformations
- **Compute Shaders**: GPU-accelerated image processing with built-in kernels (blur, sharpen, edge detection, etc.)

**See**: [METAL_ADVANCED_FEATURES.md](METAL_ADVANCED_FEATURES.md) for detailed documentation and usage examples.

## Architecture

The SharpMetalRenderingBackend:
1. Creates a GLFW window for cross-platform window management
2. Retrieves the native NSView from the GLFW window using Cocoa APIs
3. Creates and attaches a CAMetalLayer to the view
4. Initializes Metal device, command queue, and rendering pipeline
5. Renders frame buffer data to the Metal layer using a fullscreen quad

## Limitations

- **macOS Only**: This backend is only available on macOS and will fail on other platforms
- **GLFW Dependency**: Requires GLFW for window creation (inherited from existing backends)
- **Experimental**: As SharpMetal is still in v1.0, there may be edge cases

## Performance

Metal provides excellent performance on macOS by:
- Direct GPU access without translation layers
- Efficient command buffer management
- Hardware-accelerated texture sampling and rendering

## Troubleshooting

### Backend fails to initialize
- Ensure you're running on macOS
- Verify Metal is supported on your Mac (most Macs since 2012)
- Check that GLFW is properly initialized

### Black screen or no rendering
- Verify frame buffer data is being provided correctly
- Check log output for Metal API errors
- Ensure the application has necessary permissions

## See Also

- [METAL_USAGE_EXAMPLES.md](METAL_USAGE_EXAMPLES.md) - Practical usage examples and code samples
- [SharpMetal Repository](https://github.com/IsaacMarovitz/SharpMetal)
- [Apple Metal Documentation](https://developer.apple.com/metal/)
- [Win32Emu Rendering Backends](../Rendering/)
