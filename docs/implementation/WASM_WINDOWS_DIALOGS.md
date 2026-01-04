# Windows and Dialogs Support in WASM Frontend

## Overview

This implementation adds support for rendering Win32 windows, dialogs, and message boxes in the WASM frontend. When an emulated application creates a window, dialog, or displays a message box, the WASM UI will now show these UI elements overlaid on the emulator canvas using Blazor components styled to look like classic Windows 95/98 UI.

## Architecture

### Components

The implementation consists of three main Blazor components located in `Win32Emu.Wasm/Components/`:

1. **MessageBoxComponent.razor** - Renders Win32 MessageBox dialogs
   - Supports all standard button combinations (OK, OK/Cancel, Yes/No, Yes/No/Cancel, etc.)
   - Returns appropriate result codes (IDOK, IDCANCEL, IDYES, IDNO, etc.)
   - Modal overlay with Windows 95/98 styling

2. **DialogComponent.razor** - Renders Win32 DialogBox with controls
   - Supports common dialog controls: Button, Edit, Static, ListBox, ComboBox
   - Handles control visibility, disabled state, and styles
   - Two-way binding for edit controls
   - Returns control ID on button clicks

3. **WindowComponent.razor** - (Not currently used)
   - Originally designed to show window metadata as overlays
   - **Not rendered** to avoid occluding the canvas where DirectDraw content is displayed
   - Regular windows render their content directly on the canvas via DirectDraw/GDI
   - Window creation events are still tracked for debugging purposes

### Event Flow

1. **Emulator creates UI element** → User32Module calls IEmulatorHost methods
2. **WasmEmulatorHost receives call** → Raises event with TaskCompletionSource
3. **Home.razor handles event** → Sets state to show component
4. **User interacts with component** → Calls callback with result
5. **Callback completes task** → Returns result to emulator

### Key Classes

- **WasmEmulatorHost** (`Win32Emu.Wasm/Services/WasmEmulatorHost.cs`)
  - Implements `IEmulatorHost` interface
  - Uses `TaskCompletionSource<int>` for async dialog/messagebox results
  - Exposes events: `WindowCreated`, `DialogCreateRequested`, `MessageBoxRequested`, `DialogEnded`

- **EmulatorService** (`Win32Emu.Wasm/Services/EmulatorService.cs`)
  - Exposes `Host` property to allow event subscriptions
  - Manages emulator lifecycle

- **Home.razor** (`Win32Emu.Wasm/Pages/Home.razor`)
  - Subscribes to host events in `OnInitialized`
  - Maintains state for active windows, dialogs, and message boxes
  - Renders components in overlay on canvas

## Styling

Windows 95/98 style CSS is provided in `Win32Emu.Wasm/wwwroot/css/win32-ui.css`:

- Classic beveled borders (outset/inset)
- Windows blue title bar gradient
- MS Sans Serif font
- Gray (#C0C0C0) background color
- Proper button, edit, and control styling

## Usage

When an emulated application calls Win32 APIs like:

```c
// Message box
MessageBoxA(NULL, "Hello World", "Title", MB_OK);

// Dialog box
DialogBoxParam(hInstance, "IDD_DIALOG", hwndParent, DialogProc, 0);

// Create window
CreateWindowEx(0, "ClassName", "Window Title", WS_OVERLAPPEDWINDOW, ...);
```

The WASM UI will:
1. Display the appropriate UI component overlaid on the canvas
2. Wait for user interaction
3. Return the result to the emulator (IDOK, IDCANCEL, control ID, etc.)
4. Continue emulation with the result

## Supported Features

✅ **MessageBox**
- All button combinations (OK, OK/Cancel, Yes/No, Yes/No/Cancel, Abort/Retry/Ignore, Retry/Cancel)
- Modal behavior with backdrop
- Returns correct button ID (IDOK=1, IDCANCEL=2, IDYES=6, IDNO=7, etc.)

✅ **Dialog Controls**
- Button (regular and default)
- Edit (single-line and multi-line)
- Static text
- ListBox
- ComboBox
- Visibility and disabled state handling

⚠️ **Windows (Not Rendered as HTML Overlays)**
- Regular windows (CreateWindowEx) render their content directly on the canvas via DirectDraw/GDI
- WindowComponent overlays are intentionally NOT rendered to avoid occluding the canvas
- Window creation events are still tracked for debugging purposes

## Limitations and Future Work

❌ **Not Yet Implemented**
- Dialog control state synchronization (emulator → UI)
  - Text changes in emulator won't update UI automatically
  - Bitmap updates for picture controls
  - Dynamic visibility/enabled state changes
- Window content rendering (rendered in canvas by emulator)
- Keyboard shortcuts (Alt+F4, Escape, Enter for default button)
- Window dragging/moving
- Focus management
- Tab order navigation

## Testing

To test the implementation:

1. Build the WASM project:
   ```bash
   dotnet build Win32Emu.Wasm/Win32Emu.Wasm.csproj
   ```

2. Run the WASM frontend:
   ```bash
   dotnet run --project Win32Emu.Wasm/Win32Emu.Wasm.csproj
   ```

3. Load an executable that creates windows, dialogs, or message boxes

4. Observe the UI overlays appearing on the canvas

## Code Examples

### Subscribing to Events in Blazor Component

```csharp
protected override void OnInitialized()
{
    if (EmulatorService.Host is WasmEmulatorHost wasmHost)
    {
        wasmHost.WindowCreated += OnWindowCreated;
        wasmHost.DialogCreateRequested += OnDialogCreateRequested;
        wasmHost.MessageBoxRequested += OnMessageBoxRequested;
    }
}

private async void OnMessageBoxRequested(object? sender, WasmEmulatorHost.MessageBoxEventArgs e)
{
    await InvokeAsync(() =>
    {
        _activeMessageBox = e.Info;
        _activeMessageBoxEventArgs = e;
        StateHasChanged();
    });
}

private void HandleMessageBoxResult(int result)
{
    if (_activeMessageBoxEventArgs != null)
    {
        _activeMessageBoxEventArgs.CompletionSource.SetResult(result);
        _activeMessageBox = null;
        _activeMessageBoxEventArgs = null;
        StateHasChanged();
    }
}
```

### Rendering Components

```razor
@if (_activeMessageBox != null)
{
    <MessageBoxComponent Info="_activeMessageBox" OnResult="HandleMessageBoxResult" />
}

@if (_activeDialog != null)
{
    <DialogComponent Info="_activeDialog" OnResult="HandleDialogResult" />
}
```

## Related Files

- Components:
  - `Win32Emu.Wasm/Components/MessageBoxComponent.razor`
  - `Win32Emu.Wasm/Components/DialogComponent.razor`
  - `Win32Emu.Wasm/Components/WindowComponent.razor`

- Services:
  - `Win32Emu.Wasm/Services/WasmEmulatorHost.cs`
  - `Win32Emu.Wasm/Services/EmulatorService.cs`

- UI:
  - `Win32Emu.Wasm/Pages/Home.razor`
  - `Win32Emu.Wasm/wwwroot/css/win32-ui.css`
  - `Win32Emu.Wasm/wwwroot/index.html`

- Core Interface:
  - `Win32Emu/IEmulatorHost.cs`
  - `Win32Emu/WindowCreateInfo.cs`
  - `Win32Emu/DialogCreateInfo.cs`
  - `Win32Emu/MessageBoxInfo.cs`

## References

- Win32 MessageBox API: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messagebox
- Win32 DialogBox API: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-dialogbox
- Win32 CreateWindow API: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createwindowexa
