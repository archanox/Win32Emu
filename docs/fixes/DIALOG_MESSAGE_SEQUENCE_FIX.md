# Dialog Message Sequence Fix

## Problem
The Ignition setup.exe install window was not appearing on the WASM frontend. The dialog appeared to be stuck in a loop, waiting for messages indicating the window had been shown or had received focus.

## Root Cause
The `DialogBoxParamAsync` implementation in User32Module was incomplete. It sent `WM_INITDIALOG` to initialize the dialog controls, but failed to send the subsequent `WM_SHOWWINDOW` and `WM_SETFOCUS` messages that are part of the standard Windows dialog initialization sequence.

### Expected Windows Dialog Message Sequence
According to Microsoft Windows API documentation, when a modal dialog is created via DialogBoxParam, the following message sequence occurs:

1. **WM_INITDIALOG (0x0110)** - Sent to initialize dialog controls before display
   - wParam: 0 (or handle of control to receive focus)
   - lParam: dwInitParam (application-defined value)
   - Dialog can initialize controls, set text, load resources

2. **WM_SHOWWINDOW (0x0018)** - Sent to notify window it's about to become visible
   - wParam: TRUE (1) when being shown, FALSE (0) when being hidden
   - lParam: 0 when sent by ShowWindow call
   - Dialog can perform visibility-related operations

3. **WM_SETFOCUS (0x0007)** - Sent to notify window it's receiving keyboard focus
   - wParam: 0 (handle of window losing focus, or 0)
   - lParam: 0
   - Dialog can perform focus-related initialization

### What Was Missing
The emulator was only sending WM_INITDIALOG. Applications expecting the full message sequence (like setup.exe) would wait indefinitely for WM_SHOWWINDOW and WM_SETFOCUS, causing the dialog to appear frozen or stuck in a message loop.

## Solution
### Changes to DialogBoxParamAsync
Added the missing message sequence after WM_INITDIALOG:

```csharp
// After WM_INITDIALOG succeeds...
if (!dialogProcTimedOut && !dialogProcCancelled && !dialogProcFailed && lpDialogFunc != 0)
{
    // Send WM_SHOWWINDOW
    await CallDialogProcedureAsync(..., WM_SHOWWINDOW, 1, 0, ...);
    
    // If successful, send WM_SETFOCUS
    if (!showTimedOut && !showCancelled && !showFailed)
    {
        await CallDialogProcedureAsync(..., WM_SETFOCUS, 0, 0, ...);
    }
}
```

Key implementation details:
- Messages are sent sequentially (INITDIALOG → SHOWWINDOW → SETFOCUS)
- Each message is only sent if the previous one succeeded
- Proper error handling for timeout/cancellation/failure
- Dialog is terminated early if any initialization message fails

### Changes to ShowWindow
Enhanced the ShowWindow function to send WM_SHOWWINDOW messages when window visibility changes:

```csharp
if (shouldBeVisible && !wasPreviouslyVisible)
{
    // Send WM_SHOWWINDOW before WM_ACTIVATEAPP
    _env.SendMessageToWindow(hwnd, 0x0018, 1, 0);
    _env.SendMessageToWindow(hwnd, 0x001C, 1, 0);
}
```

This ensures that regular window show/hide operations also follow Windows API behavior.

## Files Modified
- `Win32Emu/Win32/Modules/User32Module.cs`
  - DialogBoxParamAsync: Added WM_SHOWWINDOW and WM_SETFOCUS message sending
  - ShowWindow: Added WM_SHOWWINDOW message sending for visibility changes

## Testing
- ✅ Project builds successfully
- ✅ Core emulator tests pass (no regressions)
- ✅ User32 tests pass (no regressions)
- ✅ Code review completed

## Expected Results
With this fix:
1. Dialog procedures receive the complete initialization message sequence
2. Applications can properly detect when dialogs become visible
3. Focus handling is properly communicated to dialog procedures
4. setup.exe and similar applications should no longer appear frozen

## References
- [WM_INITDIALOG - Microsoft Docs](https://learn.microsoft.com/en-us/windows/win32/dlgbox/wm-initdialog)
- [WM_SHOWWINDOW - Microsoft Docs](https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-showwindow)
- [WM_SETFOCUS - Microsoft Docs](https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-setfocus)
- [DialogBoxParam - Microsoft Docs](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-dialogboxparama)

## Related Issues
- Previous SETUP_DIALOG_FIXES.md documented UI rendering issues but didn't address the message sequence
- This complements the Avalonia UI integration by ensuring proper Win32 API compliance
