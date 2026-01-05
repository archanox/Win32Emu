# DirectDraw Window-Specific Rendering

## Overview

This document describes the implementation of window-specific rendering for DirectDraw content, enabling proper display of DirectDraw output to the correct window when applications create windows via `CreateWindowEx` before initializing DirectDraw.

## Problem Statement

Previously, both WASM and Avalonia frontends rendered all DirectDraw output to a single canvas/element, regardless of which window was created by the emulated application:

- **WASM**: Windows created via `CreateWindowEx` were tracked but not rendered (effectively hidden)
- **Avalonia**: Windows were created but DirectDraw content always went to the main display
- **Issue**: Applications that create a window and then use DirectDraw for rendering to that window wouldn't display correctly

## Solution

### Core Architecture Changes

1. **Window Handle Tracking** (Already Existed)
   - `DirectDrawObject` already stored the window handle from `SetCooperativeLevel`
   - No changes needed to tracking mechanism

2. **Interface Extension**
   - Added optional `targetWindowHandle` parameter to `IRenderingBackend.UpdateFrameBuffer()`
   - Default value of `IntPtr.Zero` maintains backward compatibility
   - Signature: `bool UpdateFrameBuffer(byte[] data, int pitch, IntPtr targetWindowHandle = default)`

3. **Rendering Backend Updates**
   - Updated `DDrawModule` to pass `ddrawObj.WindowHandle` when calling `UpdateFrameBuffer`
   - Updated all rendering backend implementations to accept the new parameter
   - Backends can use the window handle to route rendering to specific surfaces

### WASM Implementation

The WASM frontend now fully supports window-specific rendering:

#### Component Changes

**Home.razor:**
```razor
<!-- Windows that have been created by the emulated application -->
@foreach (var window in _activeWindows.Values)
{
    <WindowComponent Info="window" OnClose="@(() => HandleWindowClose(window.Handle))" />
}
```

**WindowComponent.razor:**
```razor
<div class="win32-window" style="...">
    <div class="win32-window-titlebar">...</div>
    <div class="win32-window-content">
        <!-- Canvas for DirectDraw rendering to this specific window -->
        <canvas id="window-canvas-@(Info.Handle.ToString("X8"))"
                width="@Info.Width"
                height="@Info.Height"
                style="...">
        </canvas>
    </div>
</div>
```

#### Backend Changes

**WasmRenderingBackend.cs:**
```csharp
public bool UpdateFrameBuffer(byte[] data, int pitch, IntPtr targetWindowHandle = default)
{
    // Determine which canvas to render to
    var canvasId = _canvasId; // Default to main canvas
    if (targetWindowHandle != IntPtr.Zero)
    {
        // Use window-specific canvas ID format: "window-canvas-{HWND}"
        var windowHandleValue = (uint)targetWindowHandle.ToInt32();
        canvasId = $"window-canvas-{windowHandleValue:X8}";
        _logger.LogTrace("[WASM] Rendering to window-specific canvas: {CanvasId}", canvasId);
    }
    
    // ... call JavaScript updateCanvas with canvasId ...
}
```

#### JavaScript Support

The existing JavaScript function `updateCanvas(canvasId, base64Data, width, height)` already supports dynamic canvas IDs, so no JavaScript changes were needed.

### Avalonia Implementation Status

The Avalonia frontend has been updated to accept the new parameter but currently maintains backward-compatible behavior:

- ✅ Accepts `targetWindowHandle` parameter  
- ⚠️ Ignores the handle and renders to main display (existing behavior)
- ℹ️ Future enhancement: Route to specific window `WriteableBitmap` instances

This approach ensures:
- No breaking changes to existing functionality
- Foundation for future window-specific rendering
- Consistent API across all backends

## Usage Pattern

### For Emulated Applications

No changes needed - applications work as written:

```c
// Create a window
HWND hwnd = CreateWindowEx(0, "MyClass", "My Window", WS_OVERLAPPEDWINDOW, 
                           CW_USEDEFAULT, CW_USEDEFAULT, 640, 480, 
                           NULL, NULL, hInstance, NULL);

// Initialize DirectDraw
DirectDrawCreate(NULL, &lpDD, NULL);
lpDD->SetCooperativeLevel(hwnd, DDSCL_NORMAL);  // Associates with window

// Create surfaces and render
// ... content will now appear in the window created above ...
```

### For Backend Implementers

To support window-specific rendering in a new backend:

```csharp
public bool UpdateFrameBuffer(byte[] data, int pitch, IntPtr targetWindowHandle = default)
{
    if (targetWindowHandle != IntPtr.Zero)
    {
        // Find window-specific rendering surface
        if (TryGetWindowSurface(targetWindowHandle, out var surface))
        {
            // Render to window-specific surface
            RenderToSurface(surface, data, pitch);
            return true;
        }
        // Fall through to default if window not found
    }
    
    // Render to default surface (fullscreen or main display)
    RenderToDefaultSurface(data, pitch);
    return true;
}
```

## Behavior

### WASM Frontend

| Scenario | Behavior |
|----------|----------|
| Window created, then DirectDraw | Renders to window's canvas |
| Fullscreen DirectDraw | Renders to default `emulatorCanvas` |
| No window handle (0) | Renders to default `emulatorCanvas` |
| Window not found | Renders to default `emulatorCanvas` |

### Avalonia Frontend

| Scenario | Behavior |
|----------|----------|
| All scenarios | Renders to main `DisplayBitmap` |

## Benefits

1. **Correct Behavior**: Applications that create windows before DirectDraw now work correctly
2. **Backward Compatible**: Fullscreen and existing applications continue to work
3. **Multiple Windows**: Foundation for multi-window DirectDraw applications
4. **Standards Compliant**: Better matches native Win32 behavior
5. **Consistent API**: All backends use the same interface

## Testing

### Test Cases

1. **Window-based DirectDraw**
   - Create window via `CreateWindowEx`
   - Initialize DirectDraw with `SetCooperativeLevel(hwnd, DDSCL_NORMAL)`
   - Verify rendering appears in the window

2. **Fullscreen DirectDraw**
   - Initialize DirectDraw with `SetCooperativeLevel(NULL, DDSCL_FULLSCREEN | DDSCL_EXCLUSIVE)`
   - Verify rendering appears in default canvas

3. **Multiple Windows**
   - Create multiple windows
   - Initialize separate DirectDraw instances for each
   - Verify each renders to its own window

### Sample Applications

- **BasicDD.exe** - Creates window, then uses DirectDraw
- **IGN_TEAS.EXE** - Game with window and DirectDraw
- **Fullscreen demos** - Should continue using default canvas

## Implementation Files

### Core
- `Win32Emu/Rendering/IRenderingBackend.cs` - Interface definition
- `Win32Emu/Win32/Modules/DDrawModule.cs` - Window handle passing

### WASM
- `Win32Emu.Wasm/Backend/WasmRenderingBackend.cs` - Canvas routing logic
- `Win32Emu.Wasm/Components/WindowComponent.razor` - Window UI with canvas
- `Win32Emu.Wasm/Pages/Home.razor` - Window component rendering

### Avalonia
- `Win32Emu.Gui/Backends/AvaloniaRenderingBackend.cs` - Backend implementation
- `Win32Emu.Gui/ViewModels/EmulatorWindowViewModel.cs` - Window creation

### Other Backends
- `Win32Emu.Gui/Backends/SDL3RenderingBackend.cs`
- `Win32Emu.Gui/Backends/SilkGlfwRenderingBackend.cs`
- `Win32Emu.Gui/Backends/SilkVulkanRenderingBackend.cs`
- `Win32Emu.Gui/Backends/SharpMetalRenderingBackend.cs`
- `Win32Emu.Gui/Backends/SoftwareRenderingBackend.cs`

## Future Enhancements

### Avalonia Window Rendering
To fully implement window-specific rendering in Avalonia:

1. Add `WriteableBitmap` to each created window
2. Store window handle to bitmap mapping
3. Update `OnDisplayUpdate` to accept window handle
4. Route framebuffer updates to correct bitmap

### Additional Features
- Window dragging/resizing in WASM
- Multi-monitor support
- Window Z-order management
- Window focus tracking
- Proper window decorations (minimize, maximize buttons)

## References

- Win32 `CreateWindowEx`: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createwindowexa
- Win32 `SetCooperativeLevel`: https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/bb151620(v=vs.85)
- WASM Windows and Dialogs: [WASM_WINDOWS_DIALOGS.md](./WASM_WINDOWS_DIALOGS.md)
