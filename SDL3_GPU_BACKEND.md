# SDL3 GPU Backend Implementation

## Overview

Win32Emu now uses SDL3's modern GPU API for hardware-accelerated rendering. This ensures proper graphics initialization across all platforms, particularly addressing Metal backend requirements on macOS.

## Platform Support

The SDL3 GPU API automatically selects the appropriate graphics backend for each platform:

| Platform | Graphics Backend | Status |
|----------|-----------------|--------|
| **macOS** | Metal | ✅ Fully Supported |
| **Linux** | Vulkan | ✅ Fully Supported |
| **Windows** | DirectX 12 | ✅ Fully Supported |

## Changes from Traditional Rendering

### Previous Implementation (Problematic on macOS)
```csharp
// Old approach - could fail on macOS
SDL.Init(SDL.InitFlags.Video);
SDL.CreateWindowAndRenderer(title, width, height, flags, out window, out renderer);
SDL.CreateTexture(renderer, format, access, width, height);
```

### New GPU-Based Implementation
```csharp
// Modern GPU API - works reliably on macOS with Metal
var gpuDevice = SDL.CreateGPUDevice(shaderFormats, debug, driverName);
var window = SDL.CreateWindow(title, width, height, flags);
SDL.ClaimWindowForGPUDevice(gpuDevice, window);
var texture = SDL.CreateGPUTexture(gpuDevice, textureCreateInfo);

// Rendering uses command buffers
var commandBuffer = SDL.AcquireGPUCommandBuffer(gpuDevice);
var swapchainTexture = SDL.AcquireGPUSwapchainTexture(commandBuffer, window);
// ... render operations ...
SDL.SubmitGPUCommandBuffer(commandBuffer);
```

## Benefits

1. **Reliable macOS Support**: Metal backend initialization is handled properly by SDL3 GPU API
2. **Hardware Acceleration**: Native graphics API on each platform (Metal/Vulkan/DirectX)
3. **Modern Architecture**: Command buffer-based rendering is more efficient
4. **Future-Proof**: GPU API is the recommended path for SDL3 applications

## Technical Details

### GPU Device Creation

The GPU device is created with support for multiple shader formats:
- **SPIRV**: For Vulkan (Linux)
- **MSL**: For Metal (macOS)
- **DXIL**: For DirectX 12 (Windows)

SDL3 automatically selects the appropriate backend based on the platform.

### Frame Upload Process

1. Create a transfer buffer for CPU-to-GPU data transfer
2. Map the buffer to CPU memory
3. Copy frame data to mapped memory
4. Unmap the buffer
5. Issue upload command to GPU
6. Blit uploaded texture to swapchain for display

### Resource Management

All GPU resources are properly tracked and released:
- GPU device is destroyed on shutdown
- Textures are released before device destruction
- Windows are released from GPU device before cleanup
- GPU idle wait ensures all operations complete before cleanup

## Troubleshooting

### macOS-Specific Issues

**Problem**: "Failed to create GPU device"
- **Solution**: Ensure macOS version supports Metal (10.11+)
- **Check**: GPU device driver name should report "metal"

**Problem**: Black screen on window creation
- **Solution**: Verify window was successfully claimed by GPU device
- **Check**: Enable debug mode in CreateGPUDevice for detailed errors

### General Issues

**Problem**: "SDL3 library not found"
- **Solution**: Ensure SDL3-CS.Native package is properly installed
- **Package**: SDL3-CS.Native version 3.2.24 or higher

## References

- [SDL3 GPU API Documentation](https://wiki.libsdl.org/SDL3/CategoryGPU)
- [SDL3-CS Bindings](https://github.com/edwardgushchin/SDL3-CS)
- [Issue #XX: SDL3 Initialisation issues on macOS](https://github.com/archanox/Win32Emu/issues/XX)

## Implementation Files

- `Win32Emu/Rendering/SDL3RenderingBackend.cs` - Main GPU backend implementation
- `Win32Emu.Tests.Emulator/SDL3BackendTests.cs` - Test coverage
- `SDL3_INTEGRATION.md` - Integration documentation
