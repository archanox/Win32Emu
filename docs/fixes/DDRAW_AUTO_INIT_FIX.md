# DirectDraw Auto-Initialization Fix

## Problem

Some Windows applications create DirectDraw surfaces without calling `SetDisplayMode` first. This caused the rendering backend to never initialize, resulting in:
- Canvas never showing any content in WASM frontend
- Diagnostic panel showing "Backend Initialized: No"
- Frames being buffered but never displayed
- Canvas update count remaining at 0

## Root Cause

The rendering backend was only initialized in `DDraw_SetDisplayMode`. Applications that skip this call (like ign_teas) would:
1. Create DirectDraw object successfully
2. Create primary surface successfully
3. Lock and Unlock surfaces (drawing content)
4. Never initialize the rendering backend
5. Result: all frames buffered but never shown

## Solution

Added auto-initialization logic in `DDraw_CreateSurface` that:
1. Detects when a primary surface with valid dimensions is created
2. Checks if rendering backend exists but is not yet initialized
3. Automatically initializes the backend using the surface dimensions
4. Sets up frame buffering queue for WASM mode
5. Subscribes to UI events after successful initialization

## Code Changes

**File:** `Win32Emu/Win32/Modules/DDrawModule.cs`

**Location:** After line 1068 (after creating COM object for surface)

**Key Logic:**
```csharp
// Auto-initialize rendering backend when primary surface is created
if (isPrimary && ddrawObj.RenderingBackend != null && !ddrawObj.RenderingBackend.IsInitialized)
{
    if (surfaceWidth > 0 && surfaceHeight > 0)
    {
        // Initialize backend with surface dimensions
        // Handle both WASM and non-WASM platforms
        // Set up frame buffering for WASM mode
        // Subscribe to UI events
    }
}
```

## Behavior Changes

### Before Fix
1. Application creates DirectDraw object ✅
2. Application creates primary surface ✅
3. Application locks and unlocks surfaces ✅
4. **Backend never initialized ❌**
5. **Nothing rendered to canvas ❌**

### After Fix
1. Application creates DirectDraw object ✅
2. Application creates primary surface ✅
3. **Backend auto-initialized using surface dimensions ✅**
4. Application locks and unlocks surfaces ✅
5. **Content rendered to canvas ✅**

## Compatibility

This change is **backward compatible** and doesn't affect applications that properly call `SetDisplayMode`:

- **Applications calling SetDisplayMode:** Backend initialized in `SetDisplayMode` (existing behavior)
- **Applications skipping SetDisplayMode:** Backend auto-initialized in `CreateSurface` (new behavior)

## Testing

### Manual Testing with ign_teas
1. Build WASM frontend: `dotnet build Win32Emu.Wasm/Win32Emu.Wasm.csproj --configuration Release`
2. Run local server and load ign_teas executable
3. Verify diagnostic panel shows:
   - "Backend Initialized: Yes" (changes from "No")
   - "Canvas Update Count" > 0 (changes from 0)
   - "Last Update: X seconds ago" (changes from "Never")
4. Verify canvas shows game graphics

### Expected Log Messages
When auto-initialization triggers, you should see:
```
[DDraw] Auto-initializing rendering backend for primary surface (640x480)
[DDraw] Set display mode dimensions from primary surface: 640x480
[DDraw] Initialized frame buffering for WASM mode (auto-init)
[DDraw] Rendering backend auto-initialized successfully with 640x480 (WASM mode)
[DDraw] Subscribed to UI events from rendering backend (auto-init)
```

## Future Improvements

1. **Async Initialization:** Consider making `CreateSurface` async to properly await backend initialization without blocking
2. **Dimension Detection:** Improve dimension detection for edge cases where surface descriptor doesn't specify size
3. **Performance:** Monitor impact of auto-initialization on surface creation performance

## Related Issues

- Fixes canvas not showing content in WASM frontend
- Resolves issue where applications skip SetDisplayMode
- Addresses frame buffering without display problem

## References

- DirectX 7 SDK Documentation: CreateSurface method
- MSDN: DirectDraw Surface Creation
- Win32Emu Issue: "Nothing shown on canvas in WASM frontend"
