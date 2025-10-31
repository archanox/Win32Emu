# Phase 4 Implementation: Window Procedure Callbacks and Input Routing

## Overview

Phase 4 completes the Win32 message infrastructure by implementing full keyboard and mouse input routing from Avalonia windows to the Win32 message queue. This enables emulated Win32 applications to receive and process user input events.

## What Was Implemented

### 1. Window Procedure Callbacks (Already Complete from Phase 3)

Window procedure callbacks were already fully functional before this phase:

**CallWindowProcedure Method** (`Win32Emu/Win32/Modules/User32Module.cs`)
- Executes emulated window procedures with proper calling convention
- Sets up stdcall stack frame with hwnd, message, wParam, lParam parameters
- Handles return values from window procedures
- Supports unlimited execution with infinite loop detection
- Integrates with COM vtable dispatcher and Win32 import dispatcher

**Status:** ✅ Fully implemented and tested

### 2. Keyboard Input Routing

#### Win32InputMapper Utility Class

Created `Win32Emu.Gui/Utilities/Win32InputMapper.cs` to map Avalonia keyboard events to Win32 messages:

```csharp
public static class Win32InputMapper
{
    public static byte MapKeyToVirtualKeyCode(Key key)
    {
        // Maps Avalonia.Input.Key to Win32 Virtual Key codes
        // Supports 65+ keys including:
        // - Letters A-Z (0x41-0x5A)
        // - Numbers 0-9 (0x30-0x39)
        // - Function keys F1-F12 (0x70-0x7B)
        // - Special keys (Enter, Escape, Space, etc.)
        // - Arrow keys (Left, Up, Right, Down)
        // - Modifier keys (Shift, Ctrl, Alt - left and right)
        // - Numpad keys (0-9, operators)
        // - OEM keys (punctuation, brackets, etc.)
    }
    
    public static uint GetKeyModifiers(KeyModifiers modifiers);
    public static uint MakeMouseLParam(double x, double y);
}
```

**Key Mappings:**
- **Letters:** A-Z → VK 0x41-0x5A
- **Numbers:** 0-9 → VK 0x30-0x39
- **Function Keys:** F1-F12 → VK 0x70-0x7B
- **Special Keys:** Enter (0x0D), Escape (0x1B), Space (0x20), Backspace (0x08), Tab (0x09)
- **Arrow Keys:** Left (0x25), Up (0x26), Right (0x27), Down (0x28)
- **Modifiers:** Shift (0xA0/0xA1), Ctrl (0xA2/0xA3), Alt (0xA4/0xA5)
- **Numpad:** NumPad0-9 (0x60-0x69), operators (0x6A-0x6F)

#### Keyboard Event Handlers

Added to `EmulatorWindowViewModel.CreateTopLevelWindow()`:

```csharp
window.KeyDown += async (s, e) =>
{
    var virtualKey = Win32InputMapper.MapKeyToVirtualKeyCode(e.Key);
    if (virtualKey != 0)
    {
        // WM_KEYDOWN = 0x0100
        await PostMessageAsync(info.Handle, 0x0100, virtualKey, lParam);
        
        // Also send WM_CHAR = 0x0102 for character keys
        if (e.Key is letter or number or space)
        {
            await PostMessageAsync(info.Handle, 0x0102, (uint)charCode, lParam);
        }
    }
};

window.KeyUp += async (s, e) =>
{
    var virtualKey = Win32InputMapper.MapKeyToVirtualKeyCode(e.Key);
    if (virtualKey != 0)
    {
        // WM_KEYUP = 0x0101
        await PostMessageAsync(info.Handle, 0x0101, virtualKey, lParam);
    }
};
```

**Messages Generated:**
- **WM_KEYDOWN (0x0100):** Posted when key is pressed
  - wParam: Virtual key code
  - lParam: Key data (repeat count, scan code, flags)
- **WM_KEYUP (0x0101):** Posted when key is released
  - wParam: Virtual key code
  - lParam: Key data with transition state = 1
- **WM_CHAR (0x0102):** Posted for character keys
  - wParam: Character code (with shift state applied for letters)
  - lParam: Key data

### 3. Mouse Input Routing

#### Mouse Button Messages

```csharp
window.PointerPressed += async (s, e) =>
{
    var point = e.GetCurrentPoint(window);
    var pos = point.Position;
    var properties = point.Properties;
    
    uint wParam = Win32InputMapper.GetMouseButtonState(properties);
    uint lParam = Win32InputMapper.MakeMouseLParam(pos.X, pos.Y);
    
    if (properties.IsLeftButtonPressed)
        await PostMessageAsync(info.Handle, 0x0201, wParam, lParam); // WM_LBUTTONDOWN
    else if (properties.IsRightButtonPressed)
        await PostMessageAsync(info.Handle, 0x0204, wParam, lParam); // WM_RBUTTONDOWN
    else if (properties.IsMiddleButtonPressed)
        await PostMessageAsync(info.Handle, 0x0207, wParam, lParam); // WM_MBUTTONDOWN
};

window.PointerReleased += async (s, e) =>
{
    // Similar logic for WM_LBUTTONUP (0x0202), WM_RBUTTONUP (0x0205), WM_MBUTTONUP (0x0208)
};
```

**Messages Generated:**
- **WM_LBUTTONDOWN (0x0201):** Left mouse button pressed
- **WM_LBUTTONUP (0x0202):** Left mouse button released
- **WM_RBUTTONDOWN (0x0204):** Right mouse button pressed
- **WM_RBUTTONUP (0x0205):** Right mouse button released
- **WM_MBUTTONDOWN (0x0207):** Middle mouse button pressed
- **WM_MBUTTONUP (0x0208):** Middle mouse button released

**Message Parameters:**
- wParam: Button state flags (MK_LBUTTON, MK_RBUTTON, MK_MBUTTON, MK_SHIFT, MK_CONTROL)
- lParam: Mouse position (LOWORD = x, HIWORD = y)

#### Mouse Movement Messages

```csharp
window.PointerMoved += async (s, e) =>
{
    var point = e.GetCurrentPoint(window);
    var pos = point.Position;
    var properties = point.Properties;
    
    uint wParam = Win32InputMapper.GetMouseButtonState(properties);
    uint lParam = Win32InputMapper.MakeMouseLParam(pos.X, pos.Y);
    
    // WM_MOUSEMOVE = 0x0200
    await PostMessageAsync(info.Handle, 0x0200, wParam, lParam);
};
```

**Message Generated:**
- **WM_MOUSEMOVE (0x0200):** Mouse cursor moved
  - wParam: Button state flags
  - lParam: Mouse position
  - Logged at Trace level to avoid spam

#### Mouse Wheel Messages

```csharp
window.PointerWheelChanged += async (s, e) =>
{
    var point = e.GetCurrentPoint(window);
    var pos = point.Position;
    var delta = e.Delta.Y; // Vertical scroll delta
    
    // Win32 wheel delta is in units of WHEEL_DELTA (120)
    short wheelDelta = (short)(delta * 120);
    uint wParam = ((uint)wheelDelta << 16) | Win32InputMapper.GetMouseButtonState(properties);
    uint lParam = Win32InputMapper.MakeMouseLParam(pos.X, pos.Y);
    
    // WM_MOUSEWHEEL = 0x020A
    await PostMessageAsync(info.Handle, 0x020A, wParam, lParam);
};
```

**Message Generated:**
- **WM_MOUSEWHEEL (0x020A):** Mouse wheel scrolled
  - wParam: HIWORD = wheel delta (120 units per notch), LOWORD = button state
  - lParam: Mouse position

### 4. Message Flow

The complete message flow from user input to window procedure:

1. **User Input** → Avalonia window receives keyboard/mouse event
2. **Event Handler** → Avalonia event handler in `EmulatorWindowViewModel`
3. **Input Mapping** → `Win32InputMapper` converts to Win32 virtual key/button codes
4. **Message Posting** → `PostMessageAsync` posts message to Win32 message queue
5. **Message Queue** → `Channel<QueuedMessage>` in `ProcessEnvironment` stores message
6. **GetMessageA** → Emulated application retrieves message from queue
7. **TranslateMessage** → Translates keyboard messages (if needed)
8. **DispatchMessageA** → Dispatches message to window procedure
9. **CallWindowProcedure** → Executes emulated window procedure code
10. **Return** → Window procedure returns result to emulator

## Testing

### Win32InputMapperTests

Created comprehensive tests for the input mapper (`Win32Emu.Tests.Gui/Win32InputMapperTests.cs`):

**Test Coverage (37 tests, all passing):**
- Key to virtual key code mapping for letters and numbers
- Special keys (F-keys, Enter, Escape, Space, etc.)
- Arrow keys (Left, Up, Right, Down)
- Modifier keys (Shift, Ctrl, Alt - both left and right)
- Numpad keys and operations
- Keyboard modifier state encoding
- Mouse position encoding (lParam format)
- Coordinate clamping for valid range

**Example Tests:**
```csharp
[Theory]
[InlineData(Key.A, 0x41)]
[InlineData(Key.Z, 0x5A)]
[InlineData(Key.D0, 0x30)]
[InlineData(Key.F1, 0x70)]
public void MapKeyToVirtualKeyCode_WithVariousKeys_ReturnsCorrectCode(Key key, byte expectedVK)
{
    var result = Win32InputMapper.MapKeyToVirtualKeyCode(key);
    Assert.Equal(expectedVK, result);
}
```

### InputRoutingTests

Created tests for message queue routing (`Win32Emu.Tests.User32/InputRoutingTests.cs`):

**Test Coverage (5 tests, all passing):**
- Keyboard message posting and retrieval (WM_KEYDOWN)
- Mouse message posting and retrieval (WM_LBUTTONDOWN)
- Multiple input messages queued in order
- Mouse move message with position encoding
- Character message posting (WM_CHAR)

**Example Tests:**
```csharp
[Fact]
public void PostMessage_WithKeyboardMessage_ShouldQueueCorrectly()
{
    const uint WM_KEYDOWN = 0x0100;
    const uint virtualKeyCode = 0x41; // 'A' key
    
    _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_KEYDOWN, virtualKeyCode, lParam);
    
    // Retrieve and verify message
    var msgAddr = _testEnv.AllocateMemory(28);
    _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);
    
    var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
    var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
    
    Assert.Equal(WM_KEYDOWN, retrievedMsg);
    Assert.Equal(virtualKeyCode, retrievedWParam);
}
```

### Test Results

**All Tests Passing:**
- ✅ Win32InputMapperTests: 37/37 passed
- ✅ InputRoutingTests: 5/5 passed
- ✅ No regressions in existing tests
- ✅ Build successful with no errors

## Benefits

### 1. Full Input Support

Applications can now receive and process all user input:
- Keyboard input (65+ keys mapped)
- Mouse button clicks (left, right, middle)
- Mouse movement and tracking
- Mouse wheel scrolling

### 2. Standard Win32 Message Pattern

The implementation follows standard Win32 message patterns:
- Proper message codes (WM_KEYDOWN, WM_LBUTTONDOWN, etc.)
- Correct parameter encoding (wParam = key/button, lParam = position)
- Message queue ordering preserved

### 3. Interactive Applications

Enables emulated applications to:
- Respond to keyboard shortcuts
- Handle button clicks and menus
- Track mouse cursor position
- Implement drag-and-drop
- Scroll content with mouse wheel

### 4. Complete Message Loop

The full Windows message loop is now operational:
```c
MSG msg;
while (GetMessageA(&msg, NULL, 0, 0) > 0) {
    TranslateMessage(&msg);  // Translates keyboard messages
    DispatchMessageA(&msg);  // Calls window procedure
}
```

Applications receive input events through their window procedure:
```c
LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    switch (msg) {
        case WM_KEYDOWN:
            // Handle key press
            if (wParam == VK_ESCAPE) {
                PostQuitMessage(0);
            }
            break;
        case WM_LBUTTONDOWN:
            // Handle mouse click
            int x = LOWORD(lParam);
            int y = HIWORD(lParam);
            break;
    }
    return DefWindowProcA(hwnd, msg, wParam, lParam);
}
```

## Current Limitations

### 1. Simplified Character Translation

The current implementation uses a simplified approach for WM_CHAR generation:
- Only basic character keys are handled (A-Z, 0-9, Space)
- Keyboard layout is not considered
- Shift state is only applied for letters
- Special character combinations (e.g., Shift+2 for @) are not handled

**Future Enhancement:** Implement full keyboard layout translation using Windows keyboard layout APIs.

### 2. No Keyboard State Tracking

The implementation doesn't maintain global keyboard state:
- GetKeyState() and GetAsyncKeyState() not implemented
- GetKeyboardState() not implemented
- Caps Lock, Num Lock, Scroll Lock states not tracked

**Future Enhancement:** Add keyboard state tracking in ProcessEnvironment.

### 3. No Mouse Capture

The implementation doesn't support mouse capture:
- SetCapture() / ReleaseCapture() not implemented
- Mouse messages outside window bounds not tracked

**Future Enhancement:** Implement mouse capture using Avalonia's pointer capture APIs.

### 4. No Input Focus Management

The implementation has basic focus handling:
- SetFocus() / GetFocus() partially implemented
- Tab key navigation not implemented
- Focus rectangles not drawn

**Future Enhancement:** Implement full focus management for controls.

### 5. Trace Logging for Mouse Move

Mouse move events are logged at Trace level to avoid spam:
- May make debugging mouse tracking difficult
- No configuration for logging level per message type

**Future Enhancement:** Add configurable message logging levels.

## Phase 4 Completion Status

**Phase 4 Goals:**
1. ✅ **Window Procedure Callbacks** - Already complete from Phase 3
2. ✅ **Real Message Queue** - Already complete using Channel<QueuedMessage>
3. ✅ **Input Routing** - Keyboard and mouse input now fully routed

**What's Working:**
- ✅ Window procedure callback execution
- ✅ Message queue with GetMessageA, PostMessageA, DispatchMessageA
- ✅ Keyboard input (65+ keys) routing to message queue
- ✅ Mouse button input (left, right, middle) routing
- ✅ Mouse movement tracking
- ✅ Mouse wheel scrolling
- ✅ Message ordering preserved
- ✅ All event handlers properly async with PostMessageAsync
- ✅ Debug logging for all input events
- ✅ 42 tests (37 mapper + 5 routing) all passing

**Phase 4 is Complete! ✅**

The Win32 message infrastructure is now fully functional with complete input routing from the Avalonia GUI to emulated Win32 applications. Emulated programs can receive keyboard and mouse input through the standard Windows message loop pattern.

## Next Steps (Future Phases)

### Phase 5: Enhanced GDI32 Drawing

1. **Complete Drawing Primitives**
   - LineTo, Rectangle, Ellipse
   - Polygon, PolyLine
   - Pen and brush support
   
2. **Bitmap Operations**
   - BitBlt, StretchBlt
   - CreateBitmap, LoadBitmap
   - Bitmap rendering to SDL3

3. **Device Contexts**
   - GetDC, ReleaseDC
   - CreateCompatibleDC
   - SelectObject for pens, brushes, bitmaps

### Phase 6: DirectDraw Integration

1. **Surface Management**
   - CreateSurface with format handling
   - Lock/Unlock for pixel access
   - Blt operations

2. **Page Flipping**
   - Front/back buffer management
   - Vertical sync
   - Double/triple buffering

3. **SDL3 Rendering**
   - Connect DirectDraw to SDL3 renderer
   - Hardware acceleration
   - Frame rate management

### Phase 7: Advanced Input

1. **Keyboard State**
   - GetKeyState, GetAsyncKeyState
   - Keyboard layout support
   - IME support

2. **Mouse Capture**
   - SetCapture, ReleaseCapture
   - Track mouse outside window
   - Coordinate translation

3. **Joystick/Controller**
   - DirectInput integration
   - Controller mapping configuration
   - Force feedback support

## References

### Documentation
- **API_INTEGRATION.md** - Overall GUI integration architecture
- **PHASE2_IMPLEMENTATION.md** - Window creation implementation
- **PHASE3_IMPLEMENTATION.md** - Message loop implementation

### Win32 API Documentation
- [Keyboard Input](https://docs.microsoft.com/en-us/windows/win32/inputdev/keyboard-input)
- [Mouse Input](https://docs.microsoft.com/en-us/windows/win32/inputdev/mouse-input)
- [Virtual-Key Codes](https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes)
- [Window Messages](https://docs.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues)
- [WM_KEYDOWN](https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keydown)
- [WM_LBUTTONDOWN](https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondown)
- [WM_MOUSEMOVE](https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousemove)
- [WM_MOUSEWHEEL](https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousewheel)

### Implementation Files
- `Win32Emu.Gui/Utilities/Win32InputMapper.cs` - Input mapping utility
- `Win32Emu.Gui/ViewModels/EmulatorWindowViewModel.cs` - Event handlers
- `Win32Emu/Win32/Modules/User32Module.cs` - Window procedure callbacks
- `Win32Emu/Win32/ProcessEnvironment.cs` - Message queue
- `Win32Emu.Tests.Gui/Win32InputMapperTests.cs` - Mapper tests
- `Win32Emu.Tests.User32/InputRoutingTests.cs` - Routing tests
