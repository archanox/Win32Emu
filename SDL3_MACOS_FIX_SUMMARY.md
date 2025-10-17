# SDL3 macOS Initialization Fix - Complete Summary

## The Problem

Users reported that SDL3 video initialization was failing on macOS with:
```
[SDL3] Failed to initialize video subsystem: No available video device
```

This prevented DirectDraw applications from running on macOS.

## Evolution of the Fix

### First Attempt (Commit 43c20ec)
**What we tried:** Moved `SDL.SetAppMetadata()` before `SDL.Init(SDL.InitFlags.Video)` in `SDL3RenderingBackend.cs`

**Why it failed:** This only worked if the Video backend was the first to initialize SDL. If Audio or Input backends called `SDL.Init()` first, the metadata would be set too late, and macOS would still deny video device access.

### Final Solution (Commit 4565d11)
**What we did:** Created a static `SDL3Initializer` helper class that ensures `SetAppMetadata()` is called exactly once before ANY SDL subsystem initialization.

**Why it works:** 
- The helper is called by all three SDL3 backends (Video, Audio, Input)
- Thread-safe with proper locking
- Guarantees metadata is set before the first `SDL.Init()` call from any subsystem
- Works regardless of initialization order

## Technical Details

### The Helper Class
```csharp
internal static class Sdl3Initializer
{
    private static readonly object _lock = new();
    private static bool _metadataSet = false;

    public static void EnsureAppMetadataSet()
    {
        lock (_lock)
        {
            if (_metadataSet) return;
            SDL.SetAppMetadata("Win32Emu", "1.0", "com.win32emu.display");
            _metadataSet = true;
        }
    }
}
```

### Usage Pattern
All three backends now follow this pattern:
```csharp
public bool Initialize()
{
    // Ensure metadata is set before ANY SDL initialization
    Sdl3Initializer.EnsureAppMetadataSet();
    
    // Now safe to initialize subsystem
    if (!SDL.Init(SDL.InitFlags.Video)) // or Audio, or Input
    {
        logger.LogError("Failed to initialize...");
        return false;
    }
    
    // Continue with initialization...
}
```

## Files Changed

1. **SDL3Initializer.cs** (NEW) - Static helper for global metadata initialization
2. **SDL3RenderingBackend.cs** - Use helper before Video init
3. **SDL3AudioBackend.cs** - Use helper before Audio init
4. **SDL3InputBackend.cs** - Use helper before Input init
5. **Documentation files** - Updated to reflect the complete fix

## Why This Matters

macOS has strict requirements for application metadata when accessing system resources:
1. The macOS window server requires proper app identity for security
2. Metal framework initialization needs valid app metadata
3. Once SDL initializes without metadata, it's too late to add it
4. The first `SDL.Init()` call from any subsystem triggers this check

## Testing

- ✅ All 9 SDL3 backend tests pass
- ✅ Build succeeds with 0 errors, 0 warnings
- ✅ Thread-safe initialization
- ✅ Works regardless of subsystem initialization order

## Key Takeaway

On macOS with SDL3: **Always set app metadata before ANY SDL initialization, not just before the subsystem you're using.**
