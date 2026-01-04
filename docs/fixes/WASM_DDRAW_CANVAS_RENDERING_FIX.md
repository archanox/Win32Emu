# WASM DirectDraw Canvas Rendering Fix

## Issue Summary

The `simple_ddraw.exe` executable failed to render on the WASM frontend. The canvas remained black with diagnostic showing:
- Canvas Status: Last Update = "Never", Update Count = 0  
- Rendering Backend: Initialized = "No"

## Root Cause

The DirectDraw rendering backend initialization was failing silently on WASM due to blocking async calls.

### Technical Details

**Location**: `Win32Emu/Win32/Modules/DDrawModule.cs` line 3974 (before fix)

**Problematic Code**:
```csharp
var success = ddrawObj.RenderingBackend.InitializeAsync((int)width, (int)height, title).GetAwaiter().GetResult();
```

**Why It Failed on WASM**:
- `.GetAwaiter().GetResult()` is a blocking synchronous wait on an async operation
- WebAssembly runs in a single-threaded JavaScript environment
- Blocking the thread prevents the async operation from completing, causing a deadlock
- Throws `PlatformNotSupportedException` on WASM

## Solution

### 1. Made DirectDraw Surface Creation Async

**File**: `Win32Emu/Win32/Modules/DDrawModule.cs`

Created async version of `DDraw_CreateSurface`:

```csharp
private async Task<uint> DDraw_CreateSurfaceAsync(ICpu cpu, VirtualMemory memory)
{
    // ... surface creation logic ...
    
    if (isPrimary && ddrawObj.RenderingBackend != null && !ddrawObj.RenderingBackend.IsInitialized)
    {
        await InitializeRenderingBackendWithDimensionsAsync(ddrawObj, surfaceWidth, surfaceHeight, updateEnvironment: true);
    }
    
    return (uint)DDResult.DD_OK;
}
```

### 2. Made Backend Initialization Async

**File**: `Win32Emu/Win32/Modules/DDrawModule.cs`

Created async version of initialization:

```csharp
private async Task InitializeRenderingBackendWithDimensionsAsync(DirectDrawObject ddrawObj, uint width, uint height, bool updateEnvironment = true)
{
    // ... setup code ...
    
    var success = await ddrawObj.RenderingBackend.InitializeAsync((int)width, (int)height, title);
    
    // ... error handling ...
}
```

**Key Change**: Replaced `.GetAwaiter().GetResult()` with proper `await` keyword.

### 3. Updated COM Vtable Registrations

Both IDirectDraw vtables now call the async version:

```csharp
new("CreateSurface", ComVtableDispatcher.FromAsyncDelegate<IDirectDraw.CreateSurface>(async (cpu, mem) => await DDraw_CreateSurfaceAsync(cpu, mem))),
```

### 4. Added Backwards Compatibility

Synchronous wrappers with platform detection:

```csharp
private uint DDraw_CreateSurface(ICpu cpu, VirtualMemory memory)
{
    if (PlatformHelpers.IsWasm)
    {
        _logger.LogError("[DDraw] DDraw_CreateSurface called on WASM - this should use async path");
        return (uint)DDResult.DDERR_GENERIC;
    }
    
    return DDraw_CreateSurfaceAsync(cpu, memory).GetAwaiter().GetResult();
}
```

### 5. Enhanced Logging

**File**: `Win32Emu.Wasm/Backend/WasmRenderingBackend.cs`

Added detailed logging to help diagnose initialization:

```csharp
_logger.LogInformation("[WASM] InitializeAsync starting: ({Width}x{Height})", width, height);
_logger.LogInformation("[WASM] Calling JavaScript initializeEmulator with canvasId: {CanvasId}", _canvasId);
// ... await initialization ...
_logger.LogInformation("[WASM] Rendering backend initialized successfully - canvas ready for updates");
```

## Impact

### Before Fix
1. Backend initialization blocked waiting for async operation
2. Threw `PlatformNotSupportedException` or deadlocked
3. No frames were rendered to canvas
4. Application appeared to freeze after window creation

### After Fix
1. Backend initialization completes asynchronously
2. Canvas receives frame updates properly
3. DirectDraw applications render correctly on WASM
4. No blocking calls, no exceptions

## Testing

### Manual Testing Steps

1. Deploy WASM frontend to GitHub Pages
2. Load `simple_ddraw.exe` sample
3. Start emulation
4. Verify:
   - Canvas updates with animated pattern
   - Diagnostics show "Initialized: Yes"
   - No console errors about canvas or initialization

### Log Output (Expected)

```
[WASM] InitializeAsync starting: (640x480)
[WASM] Calling JavaScript initializeEmulator with canvasId: emulatorCanvas
[WASM] Rendering backend initialized successfully - canvas ready for updates
[WASM] Frame buffer allocated: 1228800 bytes (640x480x4)
[DDraw] Rendering backend initialized successfully with 640x480 (WASM mode)
```

## Related Files

- `Win32Emu/Win32/Modules/DDrawModule.cs` - Main fix location
- `Win32Emu.Wasm/Backend/WasmRenderingBackend.cs` - Enhanced logging
- `docs/investigation/WEB_CANVAS_RENDERING_ANALYSIS.md` - Previous investigation

## Best Practices Learned

### DO:
✅ Use `async/await` pattern for all async operations on WASM  
✅ Test on WASM platform specifically  
✅ Add detailed logging for initialization steps  
✅ Check `PlatformHelpers.IsWasm` for platform-specific code paths

### DON'T:
❌ Use `.GetAwaiter().GetResult()` on WASM  
❌ Use `.Result` on WASM  
❌ Use `.Wait()` on WASM  
❌ Block on async operations in WASM environment

## References

- [WASM Async Limitations](https://learn.microsoft.com/en-us/aspnet/core/blazor/call-dotnet-from-javascript)
- [Task.GetAwaiter() Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.getawaiter)
- Previous Fix: `docs/fixes/WASM_RENDERING_RACE_CONDITION_FIX.md`

---

**Fixed By**: GitHub Copilot  
**Date**: 2024-12-22  
**Issue**: DirectDraw canvas not rendering on WASM  
**PR**: #TBD
