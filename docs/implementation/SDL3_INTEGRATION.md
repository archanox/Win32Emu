# SDL3 Integration for DirectDraw and GDI

This document describes the SDL3 integration approach for rendering DirectDraw and GDI graphics in Win32Emu.

## Overview

SDL3 is used as the rendering backend for both DirectDraw and GDI operations. This provides:
- **Cross-platform rendering support using SDL3 GPU API**:
  - **macOS**: Metal backend (hardware accelerated)
  - **Linux**: Vulkan backend (hardware accelerated)
  - **Windows**: DirectX 12 backend (hardware accelerated)
- Hardware acceleration through native graphics APIs
- Unified rendering pipeline for different Win32 graphics APIs

## Architecture

### Rendering Backend (`SDL3RenderingBackend`)

Located in `Win32Emu/Rendering/SDL3RenderingBackend.cs`, this class provides:

- **SDL3 GPU Initialization**: Creates GPU device with auto-selected driver (Metal/Vulkan/DirectX)
- **Window Management**: Creates and manages window for GPU rendering
- **Frame Buffer Management**: Uses GPU transfer buffers to upload frame data
- **GPU Command Submission**: Uses command buffers for efficient rendering
- **Event Processing**: Handles SDL events (resize, quit, etc.)
- **Resource Management**: Proper cleanup via IDisposable

#### GPU API Usage

The rendering backend uses SDL3's modern GPU API instead of the traditional renderer API:

1. **Device Creation**: `SDL.CreateGPUDevice()` with shader format flags for Metal/SPIRV/DXIL
2. **Window Association**: `SDL.ClaimWindowForGPUDevice()` to claim window for GPU rendering
3. **Frame Upload**:
   - Create transfer buffer with `SDL.CreateGPUTransferBuffer()`
   - Map buffer, copy frame data, unmap
   - Use `SDL.UploadToGPUTexture()` to upload to GPU
4. **Presentation**:
   - Acquire swapchain texture with `SDL.AcquireGPUSwapchainTexture()`
   - Blit from frame texture to swapchain with `SDL.BlitGPUTexture()`
   - Submit command buffer with `SDL.SubmitGPUCommandBuffer()`

This approach ensures Metal is properly initialized on macOS, fixing initialization issues that occurred with the traditional video subsystem.

### DirectDraw Integration (`DDrawModule`)

The DirectDraw module in `Win32Emu/Win32/IWin32ModuleUnsafe.cs` provides:

**Implemented Functions:**
- `DirectDrawCreate` - Creates DirectDraw object
- `DirectDrawCreateEx` - Creates DirectDraw object with interface specification

**Planned Functions:**
- `IDirectDraw::SetCooperativeLevel` - Set windowed/fullscreen mode
- `IDirectDraw::SetDisplayMode` - Set resolution and color depth
- `IDirectDraw::CreateSurface` - Create primary/back buffers
- `IDirectDrawSurface::Lock` - Lock surface for direct pixel access
- `IDirectDrawSurface::Unlock` - Unlock surface after modifications
- `IDirectDrawSurface::Blt` - Blit operations (copy pixels between surfaces)
- `IDirectDrawSurface::Flip` - Flip back buffer to screen

**Implementation Strategy:**
1. DirectDraw objects maintain SDL3RenderingBackend instances
2. Surface locks provide access to SDL3 texture data
3. Blits are translated to SDL3 render operations
4. Flips trigger SDL3 present operations

### GDI Integration (`Gdi32Module`)

The GDI32 module provides basic drawing primitives:

**Implemented Functions:**
- `BeginPaint` - Start paint session, creates device context
- `EndPaint` - End paint session, releases device context
- `FillRect` - Fill rectangle with brush
- `TextOutA` - Output text at position
- `SetBkMode` - Set background mode (transparent/opaque)
- `SetTextColor` - Set text color
- `GetStockObject` - Get stock brushes/pens

**Planned Functions:**
- `LineTo` / `MoveTo` - Line drawing
- `Rectangle` / `Ellipse` - Shape drawing
- `BitBlt` - Bitmap operations
- `StretchBlt` - Scaled bitmap operations
- `SetPixel` / `GetPixel` - Pixel operations
- `CreatePen` / `CreateBrush` - GDI object creation
- `SelectObject` - Select GDI objects into DC

**Implementation Strategy:**
1. Device contexts maintain rendering state
2. Drawing operations can be:
   - Immediately rendered to SDL3 texture (software rendering)
   - Batched and rendered on EndPaint (hardware acceleration)
   - Recorded to display list for later replay

## Data Flow

### DirectDraw Rendering Path
```
Emulated App → DirectDrawCreate → DDrawModule creates SDL3RenderingBackend
           ↓
Emulated App → CreateSurface → Allocate frame buffer + SDL3 texture
           ↓
Emulated App → Lock → Return pointer to frame buffer
           ↓
Emulated App writes pixels to frame buffer
           ↓
Emulated App → Unlock → Update SDL3 texture from frame buffer
           ↓
Emulated App → Flip → SDL3 present to display
```

### GDI Rendering Path
```
Emulated App → BeginPaint → GDI32 creates device context
           ↓
Emulated App → TextOut/FillRect → Operations recorded/rendered
           ↓
Emulated App → EndPaint → Flush operations to SDL3
           ↓
         SDL3 present to display
```

## Future Enhancements

### Phase 1: Surface Management
- Complete surface creation with proper format handling
- Implement lock/unlock with pitch calculation
- Add surface blit operations

### Phase 2: Advanced DirectDraw
- Color key support for transparency
- Palette support for 8-bit modes
- Overlay surfaces
- 3D buffer chains

### Phase 3: Enhanced GDI
- Full drawing primitive support
- Font rendering
- Bitmap operations
- Region operations
- Clipping support

### Phase 4: Optimization
- Batch rendering for multiple operations
- Hardware-accelerated paths where possible
- Minimize texture uploads
- Dirty rectangle tracking

### Phase 5: Advanced Features
- SDL_gpu integration for 3D acceleration
- SDL_shadercross for shader compatibility
- Multi-monitor support
- High-DPI support

### Phase 6: Audio and Input ✅ COMPLETED
See [SDL3_AUDIO_INPUT_INTEGRATION.md](SDL3_AUDIO_INPUT_INTEGRATION.md) for details:
- SDL3 audio backend for DirectSound
- SDL3 input backend for DirectInput (gamepad/joystick)
- Process environment integration

## Testing Strategy

1. **Unit Tests**: Test individual API functions
2. **Integration Tests**: Test complete rendering paths
3. **Sample Programs**: Create test programs using DirectDraw/GDI
4. **Game Testing**: Test with actual legacy games

## References

- SDL3-CS Documentation: https://github.com/edwardgushchin/SDL3-CS
- DirectDraw Documentation: MSDN DirectDraw API Reference
- GDI Documentation: MSDN GDI API Reference
- Issue #19: https://github.com/archanox/Win32Emu/issues/19
