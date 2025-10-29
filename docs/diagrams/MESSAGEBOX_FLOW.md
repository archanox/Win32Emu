# MessageBox Implementation Flow

## Call Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Game Executable                           │
│                                                                   │
│  MessageBoxA("", "Backbuffer couldn't be obtained", MB_OK)      │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Win32Emu - User32Module                       │
│                                                                   │
│  MessageBoxA(hwnd, lpText, lpCaption, uType)                    │
│    │                                                             │
│    ├─ Read strings from memory                                  │
│    ├─ Log: [User32] MessageBoxA: "" - "Backbuffer..."          │
│    │                                                             │
│    └─ Call: _host?.OnMessageBox(MessageBoxInfo)                │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Win32Emu.Gui - EmulatorWindowViewModel             │
│                                                                   │
│  OnMessageBox(MessageBoxInfo info)                              │
│    │                                                             │
│    └─ Dispatcher.UIThread.InvokeAsync(() => {                   │
│         var messageBox = new MessageBoxWindow(                  │
│             info.Caption,                                        │
│             info.Text,                                           │
│             info.Type                                            │
│         );                                                       │
│         return await messageBox.ShowMessageBoxAsync(_owner);    │
│       })                                                         │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Win32Emu.Gui - MessageBoxWindow                     │
│                                                                   │
│  ┌───────────────────────────────────────────────┐              │
│  │                                               │              │
│  │  ❌   Backbuffer couldn't be obtained        │              │
│  │                                               │              │
│  │                        ┌────────┐             │              │
│  │                        │   OK   │ ◄─── User   │              │
│  │                        └────────┘      clicks │              │
│  └───────────────────────────────────────────────┘              │
│                                                                   │
│  Returns: IDOK (1)                                               │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Game Executable                               │
│                                                                   │
│  EAX = 1 (IDOK)                                                 │
│  Execution continues with user acknowledgment                    │
└─────────────────────────────────────────────────────────────────┘
```

## Key Components

### 1. MessageBoxInfo
```csharp
public class MessageBoxInfo
{
    public uint ParentHandle { get; init; }
    public required string Text { get; init; }
    public required string Caption { get; init; }
    public uint Type { get; init; }
}
```

### 2. IEmulatorHost Interface
```csharp
public interface IEmulatorHost
{
    // ... other methods ...
    int OnMessageBox(MessageBoxInfo info);
}
```

### 3. MessageBoxWindow
- Creates appropriate button layout based on MB_* type
- Shows icon based on MB_ICON* flags
- Displays message text with word wrapping
- Returns Win32-compatible button result code

## Thread Safety

All UI operations are marshaled to the UI thread using:
```csharp
Dispatcher.UIThread.InvokeAsync(async () => {
    // UI operations here
}).Result;
```

This ensures the MessageBox can be called from the emulator thread while still displaying on the UI thread correctly.
