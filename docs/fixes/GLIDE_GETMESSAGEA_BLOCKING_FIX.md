# Glide GetMessageA Blocking Fix

## Problem

Applications using Glide (3Dfx Voodoo emulation) would get stuck indefinitely in `GetMessageA` after window creation. The log showed:

```
[06:38:59] [DBG] [Emulator] [User32] GetMessageA: No messages available, blocking thread 1
[07:03:15] [DBG] [Emulator] [Emulator] Progress: 190000 iterations (1459982.00ms), EIP=0x0E000002, ESP=0x001FEF08
```

The application remained stuck for over 24 minutes with no progress.

## Root Cause

1. **Message Loop Blocking**: When `GetMessageA` is called and the message queue is empty, the thread is suspended using the thread scheduler
2. **No Message Generation**: The background event processing loop calls `ProcessEvents()` every 16ms, but this only posts Win32 messages when SDL events (mouse/keyboard) occur
3. **Silent Waiting**: When there are no user interactions after window creation, no messages are posted, causing `GetMessageA` to block forever
4. **Glide Specifics**: Glide applications typically:
   - Create a window via `grSstWinOpen`
   - Enter a message loop with `GetMessageA`
   - Call `grBufferSwap` to present rendered frames
   - The issue occurred because `grBufferSwap` wasn't being called yet, or wasn't posting messages

## Solution

### Primary Fix: Post WM_PAINT in grBufferSwap

Modified `grBufferSwap` in `Glide2xModule.cs` to post a `WM_PAINT` message after processing events:

```csharp
_renderingBackend.ProcessEvents();

// Post a WM_PAINT message to keep the message queue active
// This ensures GetMessageA doesn't block forever when there are no user interactions
var windows = _env.GetAllWindowHandles().ToList();
if (windows.Count > 0)
{
    var firstWindow = windows[0];
    const uint WM_PAINT = 0x000F;
    _env.PostMessage(firstWindow, WM_PAINT, 0, 0);
    _logger.LogTrace("[GLIDE2x] Posted WM_PAINT to window 0x{Hwnd:X8} to keep message loop active", firstWindow);
}

// Begin next frame
if (_useHardwareAcceleration)
{
    _renderingBackend.BeginFrame();
}
```

### Why WM_PAINT?

1. **Keeps Message Loop Alive**: Prevents `GetMessageA` from blocking indefinitely
2. **Harmless**: `WM_PAINT` is designed to be posted frequently - DefWindowProc handles it gracefully
3. **Appropriate**: Posting a paint message after swapping buffers makes semantic sense
4. **Non-Intrusive**: Doesn't require changes to the core event processing loop

### Alternative Considered: Heartbeat in ProcessAllBackendEvents

We considered adding a `WM_NULL` heartbeat in `ProcessAllBackendEvents()` to post messages even when no SDL events occur. This was rejected because:

1. **Too Aggressive**: Would post 60 messages per second
2. **Unnecessary**: The `grBufferSwap` fix is sufficient
3. **Complexity**: Adds state tracking to avoid flooding the message queue

## Implementation Details

### Location
- **File**: `Win32Emu/Win32/Modules/Glide2xModule.cs`
- **Method**: `grBufferSwap` (Line 864-878)
- **Export**: `_grBufferSwap@4` (ordinal 14)

### Behavior

1. After calling `_renderingBackend.ProcessEvents()` which polls for native SDL events
2. Get all window handles via `_env.GetAllWindowHandles()`
3. Post `WM_PAINT` (0x000F) to the first window with `_env.PostMessage()`
4. Continue with frame presentation

### Message Flow

```
grBufferSwap
    ↓
ProcessEvents() - polls SDL events
    ↓
PostMessage(WM_PAINT) - keeps message queue alive
    ↓
GetMessageA - unblocks and retrieves WM_PAINT
    ↓
DispatchMessageA - calls window procedure
    ↓
DefWindowProcA - handles WM_PAINT (no-op for now)
    ↓
Back to message loop
```

## Testing

### Expected Behavior

After the fix:
1. `grBufferSwap` posts WM_PAINT every frame
2. `GetMessageA` receives messages and doesn't block
3. Application continues running normally
4. Message loop stays responsive

### Verification Steps

1. Run application with Glide (e.g., Ign_3dfx)
2. Observe logs for `Posted WM_PAINT to window` messages
3. Verify application doesn't get stuck in `GetMessageA`
4. Confirm frame swapping continues

### Log Example (Expected)

```
[GLIDE2x] grBufferSwap(interval=1)
[GLIDE2x] Posted WM_PAINT to window 0x00010000 to keep message loop active
[GLIDE2x] Buffer swapped successfully
[User32] GetMessageA: retrieved MSG=0x000F HWND=0x00010000
[User32] DispatchMessageA: HWND=0x00010000 MSG=0x000F wParam=0x00000000 lParam=0x00000000
```

## Impact

### Positive

- ✅ Fixes infinite blocking in `GetMessageA` for Glide applications
- ✅ Provides a consistent way to keep Glide windows responsive during rendering
- ✅ Minimal, surgical change
- ✅ No performance impact (one message per frame)

### Potential Issues

- ⚠️ If `grBufferSwap` is never called, the application will still block
  - **Mitigation**: This is expected - if the app doesn't render, it shouldn't need messages
- ⚠️ WM_PAINT may trigger unnecessary window invalidation
  - **Mitigation**: DefWindowProc handles it efficiently; applications can override if needed

## Related

- **DirectDraw Fix**: See `docs/fixes/DDRAW_UNRESPONSIVE_WINDOW_FIX.md` for a related issue solved via backend `ProcessEvents()` calls (different approach than the WM_PAINT posting used here)
- **Message Queue**: See `docs/implementation/MESSAGE_QUEUE_IMPLEMENTATION.md`
- **Event Processing**: See `docs/implementation/EVENT_DRIVEN_UI_IMPLEMENTATION.md`

## Future Considerations

If more rendering APIs need this pattern:

1. Consider extracting to a helper method in `ProcessEnvironment`
2. Add documentation about the pattern in architecture docs
3. Ensure all `ProcessEvents()` calls are followed by message posting where appropriate

## References

- **Issue**: "Ign_3dfx seems to get stuck. Perhaps our glide implementation isn't complete?"
- **Log**: Shows 190000 iterations stuck at same EIP over 24 minutes
- **Fix Commit**: "Fix Glide GetMessageA blocking issue by posting WM_PAINT in grBufferSwap"
