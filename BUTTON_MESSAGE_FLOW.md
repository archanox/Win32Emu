# Button Click Message Flow

## Before the Fix
```
User clicks Avalonia button
    ↓
Avalonia Click Event
    ↓
SendWmCommand() tries to send WM_COMMAND
    ↓
❌ Button doesn't process the click properly
```

## After the Fix
```
User clicks Avalonia button
    ↓
Avalonia Click Event
    ↓
SendMouseClickToButton() posts WM_LBUTTONDOWN & WM_LBUTTONUP
    ↓
GetMessageA() retrieves WM_LBUTTONDOWN
    ↓
DispatchMessageA() → StandardControlHandler.HandleButtonMessage()
    ↓
Handles WM_LBUTTONDOWN (capture mouse, set state)
    ↓
GetMessageA() retrieves WM_LBUTTONUP
    ↓
DispatchMessageA() → StandardControlHandler.HandleButtonMessage()
    ↓
Handles WM_LBUTTONUP → SendButtonNotification()
    ↓
Posts WM_COMMAND to parent window
    ↓
GetMessageA() retrieves WM_COMMAND
    ↓
DispatchMessageA() → Parent window's WndProc
    ↓
✅ Parent processes button click (e.g., calls PostQuitMessage for quit button)
```

## Key Win32 Message Codes
- `WM_LBUTTONDOWN = 0x0201` - Left mouse button pressed
- `WM_LBUTTONUP = 0x0202` - Left mouse button released
- `BM_CLICK = 0x00F1` - Programmatic button click
- `WM_COMMAND = 0x0111` - Menu/control notification message
- `BN_CLICKED = 0` - Button clicked notification code

## WM_COMMAND Message Format
```
wParam = MAKEWPARAM(controlId, notificationCode)
       = (notificationCode << 16) | (controlId & 0xFFFF)
       
lParam = controlHwnd (handle of the button control)
```

For a button with ID 1001:
```
wParam = 0x00001001  (HIWORD=0 (BN_CLICKED), LOWORD=1001 (control ID))
lParam = 0x00010004  (button's HWND)
```
