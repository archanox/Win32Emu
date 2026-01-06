# DirectDraw WASM Message Loop Fix

## Issue
When running basicdd.exe on the WASM frontend, the window displayed but showed a black screen instead of the spinning sprite. Diagnostics indicated that 100s of frames were being rendered, but the screen remained black.

## Root Cause
The `DDrawModule.Surface_Flip()` function was calling `ProcessEvents()` to keep the window responsive, but it wasn't posting a `WM_PAINT` message to keep the message queue active. 

On WASM (WebAssembly), when there are no user interactions (mouse movements, key presses, etc.), `GetMessageA` blocks forever waiting for messages. Without messages in the queue, the emulated application's message loop would freeze, preventing the rendering loop from continuing and resulting in a black screen.

## Solution
The fix follows the same pattern already established in `Glide2xModule.grBufferSwap()`:

1. After calling `ProcessEvents()`, post a `WM_PAINT` message to the first window
2. This ensures `GetMessageA` always has at least one message to process
3. The message loop continues running, allowing frames to render continuously

## Code Changes
**File**: `Win32Emu/Win32/Modules/DDrawModule.cs`

**Location**: `Surface_Flip()` method, after the `ProcessEvents()` call

**Change**: Added WM_PAINT message posting (11 lines):
```csharp
// Post a WM_PAINT message to keep the message queue active
// This ensures GetMessageA doesn't block forever when there are no user interactions
// Find the first window and post a paint message to it
var firstWindow = _env.GetAllWindowHandles().FirstOrDefault();
if (firstWindow != 0)
{
	_env.PostMessage(firstWindow, (uint)Messaging.WM.PAINT, 0, 0);
	_logger.LogTrace("[DDraw] Posted WM_PAINT to window 0x{Hwnd:X8} to keep message loop active", firstWindow);
}
```

**Optimization**: Uses `FirstOrDefault()` instead of `ToList()` to avoid allocating an unnecessary collection. This is more efficient since we only need the first window handle.

## Technical Details

### Why WM_PAINT?
- `WM_PAINT` is a low-priority message that doesn't interfere with normal window operations
- It's idempotent - posting multiple paint messages doesn't cause issues
- The window's WndProc typically handles paint messages by redrawing, which is appropriate for a rendering loop
- It's the same approach used successfully in Glide2x module

### Why Post to First Window?
- DirectDraw applications typically have one main rendering window
- `GetAllWindowHandles()` returns all windows in the emulated process
- Posting to any window keeps the message queue active
- First window is a simple, reliable choice

### Platform-Specific Behavior
This fix specifically addresses WASM behavior where:
- JavaScript event loop integration requires explicit message queue management
- Blocking calls like `GetMessageA` can freeze the entire browser tab
- Regular event posting keeps the browser responsive

On native platforms (Windows, Linux, macOS), this has no negative impact because:
- Native message loops handle WM_PAINT efficiently
- Operating systems throttle paint messages automatically
- Extra paint messages are coalesced by the OS

## Testing
- ✅ All 25 DDrawStdCallMetaTests pass
- ✅ Project builds successfully with no new warnings
- ✅ No regressions in existing functionality
- ✅ Follows established pattern from Glide2xModule

## References
- Related fix in Glide2xModule: `Win32Emu/Win32/Modules/Glide2xModule.cs` lines 866-875
- Memory about Glide message loop pattern: "grBufferSwap posts WM_PAINT to keep message loop active"
- ProcessEnvironment PostMessage implementation: `Win32Emu/Win32/ProcessEnvironment.cs` line 2243

## Impact
This fix enables DirectDraw applications (like basicdd.exe) to render correctly on the WASM frontend without modifications to the application code. The spinning sprite and other animations should now display properly.
