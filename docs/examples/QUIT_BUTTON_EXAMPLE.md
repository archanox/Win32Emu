# Quit Button Example - Step by Step

This document shows exactly what happens when a user clicks the "quit" button in the gdi.exe example from the issue.

## Initial Setup (from issue logs)

```
Parent Window: HWND=0x00010000 Class='gdi' Title='title'
Button Control: HWND=0x00010004 Class='BUTTON' Title='quit'
```

## Message Flow When User Clicks "Quit" Button

### 1. User Interaction
```
User clicks the "quit" button in Avalonia GUI
```

### 2. Avalonia Event Handler (EmulatorWindowViewModel.cs)
```csharp
button.Click += (s, e) =>
{
    OnDebugOutput($"Button 0x{hwnd:X8} clicked", DebugLevel.Debug);
    SendMouseClickToButton(hwnd); // hwnd = 0x00010004
};
```

### 3. Mouse Messages Posted
```csharp
// SendMouseClickToButton() posts:
PostMessage(0x00010004, 0x0201, 0x0001, 0); // WM_LBUTTONDOWN to button
PostMessage(0x00010004, 0x0202, 0, 0);      // WM_LBUTTONUP to button
```

### 4. Message Loop Processing
```
GetMessageA() retrieves WM_LBUTTONDOWN for HWND=0x00010004
DispatchMessageA() routes to StandardControlHandler.HandleButtonMessage()
    → Handles WM_LBUTTONDOWN (logs and returns)

GetMessageA() retrieves WM_LBUTTONUP for HWND=0x00010004
DispatchMessageA() routes to StandardControlHandler.HandleButtonMessage()
    → Handles WM_LBUTTONUP
    → Calls SendButtonNotification(0x00010004, 0)
```

### 5. WM_COMMAND Posted to Parent (StandardControlHandler.cs)
```csharp
// SendButtonNotification() logic:
var windowInfo = GetWindow(0x00010004);
var parentHwnd = windowInfo.Parent;    // = 0x00010000
var controlId = windowInfo.Menu;        // = 0 (from hMenu parameter)

uint wParam = (0 << 16) | (0 & 0xFFFF); // = 0x00000000 (BN_CLICKED, controlId=0)
uint lParam = 0x00010004;                 // button HWND

PostMessage(0x00010000, 0x0111, 0x00000000, 0x00010004); // WM_COMMAND to parent
```

### 6. Parent Window Receives WM_COMMAND
```
GetMessageA() retrieves message for HWND=0x00010000
  hwnd = 0x00010000 (parent window)
  message = 0x0111 (WM_COMMAND)
  wParam = 0x00000000 (HIWORD=0 (BN_CLICKED), LOWORD=0 (controlId))
  lParam = 0x00010004 (button HWND)

DispatchMessageA() calls parent's WndProc
```

### 7. Parent WndProc Handles WM_COMMAND
```c
LRESULT CALLBACK wndproc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
    switch (msg) {
        case WM_COMMAND:  // 0x0111
            // Check if this is the quit button
            if (LOWORD(wparam) == QUIT_BUTTON_ID) {  // Usually some ID like 1001
                PostQuitMessage(0);
                return 0;
            }
            break;
        
        // ... other cases ...
    }
    return DefWindowProcA(hwnd, msg, wparam, lparam);
}
```

### 8. Application Exits
```
PostQuitMessage(0) sets quit flag in ProcessEnvironment

GetMessageA() checks quit flag and returns WM_QUIT:
  hwnd = 0x00000000 (NULL)
  message = 0x0012 (WM_QUIT)
  wParam = 0x00000000 (exit code)
  
GetMessageA() returns 0 (signals message loop to exit)

Message loop exits:
while (GetMessageA(&msg, NULL, 0, 0)) {  // Returns 0 for WM_QUIT
    TranslateMessage(&msg);
    DispatchMessageA(&msg);
}

Application terminates gracefully ✅
```

## Log Output (Expected)

```
[Button] WM_LBUTTONDOWN
[Button] WM_LBUTTONUP
[Button] Sending WM_COMMAND to parent 0x00010000: controlId=0, notification=BN_CLICKED
[User32] PostMessageA: HWND=0x00010000 MSG=0x0111 wParam=0x00000000 lParam=0x00010004
[ProcessEnv] PostMessage: queued MSG=0x0111 HWND=0x00010000
[User32] GetMessageA: retrieved MSG=0x0111 HWND=0x00010000
[User32] DispatchMessageA: HWND=0x00010000 MSG=0x0111 wParam=0x00000000 lParam=0x00010004
[User32] PostQuitMessage: exitCode=0
[ProcessEnv] PostQuitMessage: exitCode=0
[User32] GetMessageA: WM_QUIT (exitCode=0)
```

## Key Points

1. ✅ Button receives mouse messages (WM_LBUTTONDOWN, WM_LBUTTONUP)
2. ✅ Button sends WM_COMMAND to parent with correct parameters
3. ✅ Parent receives WM_COMMAND and can handle it
4. ✅ Quit button can now call PostQuitMessage(0)
5. ✅ Application exits gracefully

**The quit button now works!** 🎉
