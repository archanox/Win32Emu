# BasicDD Window Occlusion Fix

## Issue Summary

When running BasicDD.exe in the WASM frontend, a dialog window appeared with the message "Window rendering is handled by the emulator canvas below", but this dialog completely occluded the canvas where DirectDraw rendering was actually happening. The user could not see any rendered output.

**GitHub Issue:** BasicDD - Canvas occluded by WindowComponent overlay

## Root Cause

The issue was in `Win32Emu.Wasm/Pages/Home.razor` where **all** windows created via `CreateWindowEx` were being rendered as HTML overlays using `WindowComponent`. This was incorrect because:

1. Regular windows (CreateWindowEx) render their content directly on the HTML5 canvas via DirectDraw/GDI
2. The `WindowComponent` overlay (z-index: 1500) was positioned above the canvas
3. The overlay had a solid gray background that completely hid the canvas below
4. The message "Window rendering is handled by the emulator canvas below" was misleading - the canvas was hidden behind the overlay

### Expected Behavior

On real Windows:
- BasicDD creates a window via CreateWindowEx
- BasicDD uses DirectDraw to render graphics directly into that window
- The DirectDraw content IS the window content, not below it

In the WASM frontend:
- BasicDD creates a window (tracked by the emulator)
- DirectDraw content is rendered to the HTML5 canvas
- The canvas should be visible without any HTML overlays occluding it
- Only DialogBox and MessageBox should show HTML overlays (because they use Win32 controls)

## Solution

Removed the rendering of `WindowComponent` overlays from `Home.razor`. The code now only renders:
- `DialogComponent` - for modal dialogs created via DialogBox API
- `MessageBoxComponent` - for message boxes created via MessageBox API

Regular windows are NOT rendered as HTML overlays. Their content (DirectDraw surfaces, GDI drawing, etc.) is rendered directly on the canvas by the emulator's rendering backend.

### Code Changes

**File:** `Win32Emu.Wasm/Pages/Home.razor`

**Before:**
```razor
<!-- Windows and dialogs overlay -->
@if (_activeWindows.Count > 0 || _activeDialog != null || _activeMessageBox != null)
{
    <div class="win32-overlay">
        @foreach (var windowInfo in _activeWindows.Values)
        {
            <WindowComponent Info="windowInfo" OnClose="() => HandleWindowClose(windowInfo.Handle)" />
        }
        
        @if (_activeDialog != null)
        {
            <DialogComponent Info="_activeDialog" OnResult="HandleDialogResult" />
        }
        
        @if (_activeMessageBox != null)
        {
            <MessageBoxComponent Info="_activeMessageBox" OnResult="HandleMessageBoxResult" />
        }
    </div>
}
```

**After:**
```razor
<!-- Dialogs and message boxes overlay -->
<!-- Note: WindowComponent overlays are NOT rendered because regular windows (CreateWindowEx)
     render their content directly on the canvas via DirectDraw/GDI. Only dialogs and message boxes
     need HTML overlays since they use Win32 controls that aren't rendered via DirectDraw. -->
@if (_activeDialog != null || _activeMessageBox != null)
{
    <div class="win32-overlay">
        @if (_activeDialog != null)
        {
            <DialogComponent Info="_activeDialog" OnResult="HandleDialogResult" />
        }
        
        @if (_activeMessageBox != null)
        {
            <MessageBoxComponent Info="_activeMessageBox" OnResult="HandleMessageBoxResult" />
        }
    </div>
}
```

**Key changes:**
1. Removed the `@foreach` loop that rendered `WindowComponent` for each window
2. Updated condition to only check for dialogs and message boxes
3. Added explanatory comment about why WindowComponent is not rendered

### Documentation Updates

**File:** `docs/implementation/WASM_WINDOWS_DIALOGS.md`

Updated the documentation to clarify that:
- WindowComponent.razor is not currently used for rendering
- Regular windows render their content directly on the canvas
- Window creation events are still tracked for debugging purposes

## Verification

To verify this fix:

1. Build the WASM project:
   ```bash
   dotnet build Win32Emu.Wasm/Win32Emu.Wasm.csproj --configuration Release
   ```

2. Run the WASM frontend:
   ```bash
   dotnet run --project Win32Emu.Wasm/Win32Emu.Wasm.csproj
   ```

3. Load BasicDD.exe in the browser

4. Expected result:
   - The canvas should be visible and show DirectDraw rendering
   - No gray overlay should occlude the canvas
   - If BasicDD creates a dialog or message box, THAT should show as an HTML overlay

## Impact

### Fixed
- ✅ BasicDD.exe DirectDraw content is now visible
- ✅ Canvas is no longer occluded by WindowComponent overlays
- ✅ User can see the rendered output

### Unchanged
- ✅ DialogBox still shows as HTML overlay (correct behavior)
- ✅ MessageBox still shows as HTML overlay (correct behavior)
- ✅ Window creation events are still tracked (for debugging)

### Not Implemented (Future Work)
- ⚠️ WindowComponent could potentially be used for special cases in the future
- ⚠️ Dialog and message box controls are not yet synchronized with emulator state
- ⚠️ Window decorations (title bar, borders) are not rendered

## Related Files

- `Win32Emu.Wasm/Pages/Home.razor` - Main page with canvas and overlays
- `Win32Emu.Wasm/Components/WindowComponent.razor` - Window overlay component (not used)
- `Win32Emu.Wasm/Components/DialogComponent.razor` - Dialog overlay component
- `Win32Emu.Wasm/Components/MessageBoxComponent.razor` - Message box overlay component
- `Win32Emu.Wasm/wwwroot/css/win32-ui.css` - Styling for overlays
- `docs/implementation/WASM_WINDOWS_DIALOGS.md` - Documentation

## References

- Original issue: BasicDD canvas occluded by dialog window
- Related fix: `docs/fixes/BASICDD_FIX.md` - Stack misalignment workaround
- WASM rendering backend: `Win32Emu.Wasm/Backend/WasmRenderingBackend.cs`

---

**Status:** ✅ Fixed  
**Date:** December 16, 2024  
**Affected Version:** WASM frontend  
**Executable:** BasicDD.exe from DirectX SDK
