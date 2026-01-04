# Input System Implementation for WASM Frontend

## Overview

This document describes the implementation of keyboard and mouse input for the Win32Emu WASM frontend, including a virtual keyboard for mobile devices.

**Date**: December 21, 2024
**Status**: ✅ Complete

## Problem Statement

The WASM frontend had all the necessary C# infrastructure (`WasmInputBackend.cs` with `[JSInvokable]` methods) but lacked the JavaScript bridge code to capture browser events and forward them to the C# backend. This prevented any user input from reaching the emulator, making games unplayable despite the rendering pipeline being fully functional.

## Solution Architecture

### Components

1. **JavaScript Event System** (`index.html`)
   - Event listeners for keyboard, mouse, and touch events
   - Win32 Virtual Key code mapping table
   - DotNetObjectReference bridge for C# callbacks

2. **C# Input Backend** (`WasmInputBackend.cs`)
   - DotNetObjectReference registration
   - Async initialization with JavaScript
   - JSInvokable methods for event callbacks

3. **Virtual Keyboard Component** (`VirtualKeyboard.razor`)
   - On-screen QWERTY keyboard for mobile
   - Function keys, arrow keys, special keys
   - Collapsible design

4. **Styling** (`app.css`)
   - Modern dark theme
   - Responsive mobile layout
   - Smooth animations

## Implementation Details

### JavaScript Event Listeners

**Location**: `Win32Emu.Wasm/wwwroot/index.html`

```javascript
// Initialize input system
window.initializeInput = function(canvasId, dotNetRef) {
    const canvas = document.getElementById(canvasId);
    
    // Keyboard events
    canvas.addEventListener('keydown', (e) => {
        e.preventDefault();
        const vkCode = mapKeyCode(e.code);
        if (vkCode > 0) {
            dotNetRef.invokeMethodAsync('OnKeyDown', vkCode);
        }
    });
    
    // Mouse events
    canvas.addEventListener('mousemove', (e) => {
        const rect = canvas.getBoundingClientRect();
        const x = Math.floor((e.clientX - rect.left) * (canvas.width / rect.width));
        const y = Math.floor((e.clientY - rect.top) * (canvas.height / rect.height));
        dotNetRef.invokeMethodAsync('OnMouseMove', x, y);
    });
    
    // Touch events (mobile)
    canvas.addEventListener('touchstart', (e) => {
        e.preventDefault();
        // Convert touch to mouse event
        dotNetRef.invokeMethodAsync('OnMouseDown', 0, x, y);
    });
}
```

### Win32 Virtual Key Code Mapping

**Key Insight**: Browser `KeyboardEvent.code` values (e.g., "KeyA", "Digit1") must be mapped to Win32 virtual key codes (e.g., VK_A=0x41, VK_1=0x31) for the emulator to understand them.

**Mapping Table** (excerpt):
```javascript
const VK_MAP = {
    // Letters
    'KeyA': 0x41, 'KeyB': 0x42, 'KeyC': 0x43, // ...
    
    // Numbers
    'Digit0': 0x30, 'Digit1': 0x31, // ...
    
    // Special keys
    'Escape': 0x1B, 'Space': 0x20, 'Enter': 0x0D,
    'ArrowLeft': 0x25, 'ArrowUp': 0x26, // ...
    
    // Function keys
    'F1': 0x70, 'F2': 0x71, // ...
};
```

**Reference**: [Microsoft Virtual Key Codes](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes)

### C# Backend Updates

**Location**: `Win32Emu.Wasm/Backend/WasmInputBackend.cs`

**Key Changes**:
1. Store `DotNetObjectReference<WasmInputBackend>` for JavaScript access
2. Pass reference to JavaScript during initialization
3. Proper disposal to prevent memory leaks

```csharp
private DotNetObjectReference<WasmInputBackend>? _dotNetRef;

public async Task<bool> InitializeAsync()
{
    // Create DotNetObjectReference to pass to JavaScript
    _dotNetRef = DotNetObjectReference.Create(this);
    
    // Initialize input system in JavaScript
    var success = await _jsRuntime.InvokeAsync<bool>("initializeInput", "emulatorCanvas", _dotNetRef);
    
    return success;
}

public void Dispose()
{
    _dotNetRef?.Dispose();
    _dotNetRef = null;
}
```

### Virtual Keyboard Component

**Location**: `Win32Emu.Wasm/Components/VirtualKeyboard.razor`

**Features**:
- Full QWERTY layout with 6 rows
- Function keys (F1-F5, ESC)
- Number keys (0-9)
- Letter keys (A-Z)
- Special keys (SPACE, ENTER, SHIFT, CTRL, ALT)
- Arrow keys (←↑↓→)
- Collapsible header to save screen space

**Usage**:
```csharp
private async Task TapKey(int vkCode)
{
    await JS.InvokeVoidAsync("tapVirtualKey", vkCode);
}
```

The `tapVirtualKey` JavaScript function simulates a key press by sending both down and up events with a 50ms delay.

### Integration with Home Page

**Location**: `Win32Emu.Wasm/Pages/Home.razor`

Added after the canvas container:
```razor
<!-- Virtual Keyboard for Mobile -->
<VirtualKeyboard />
```

The virtual keyboard appears at the bottom of the screen and can be toggled with the ⌨️/▼ button in its header.

## Initialization Flow

1. **Page Load**: Blazor loads, EmulatorService is created
2. **User Loads Executable**: EmulatorService.LoadExecutableAsync() is called
3. **Backend Factory Created**: WasmBackendFactory is instantiated
4. **Game Calls DirectInput**: DInputModule creates and initializes InputBackend
5. **InputBackend.InitializeAsync()**: 
   - Creates DotNetObjectReference
   - Calls `window.initializeInput()`
   - JavaScript registers event listeners
   - Canvas receives focus
6. **User Interacts**: 
   - Desktop: Physical keyboard/mouse → JS events → C# callbacks → Emulator
   - Mobile: Virtual keyboard buttons → JS → C# callbacks → Emulator

## Event Flow Diagram

```
User Input (Desktop)
    ↓
Physical Keyboard/Mouse
    ↓
Browser Event (keydown/mousemove)
    ↓
JavaScript Event Listener
    ↓
mapKeyCode() / Calculate Canvas Coords
    ↓
dotNetRef.invokeMethodAsync('OnKeyDown', vkCode)
    ↓
WasmInputBackend.OnKeyDown(vkCode)
    ↓
UIEvent?.Invoke(EventType.KeyDown, keyCode)
    ↓
Emulator / Win32 API Handlers
    ↓
Game Receives Input

---

User Input (Mobile)
    ↓
Virtual Keyboard Button Tap
    ↓
@onclick="() => TapKey(0x41)"
    ↓
JS.InvokeVoidAsync("tapVirtualKey", 0x41)
    ↓
JavaScript tapVirtualKey()
    ↓
OnKeyDown(0x41) then OnKeyUp(0x41) after 50ms
    ↓
[Same as desktop flow from here]
```

## Key Design Decisions

### 1. DotNetObjectReference Instead of Static Methods

**Why**: Allows multiple input backend instances and proper scoping. The JavaScript code holds a reference to the specific C# object instance.

### 2. Canvas-Relative Coordinates

**Why**: Mouse events use `getBoundingClientRect()` to convert client coordinates to canvas pixel coordinates, accounting for CSS scaling.

```javascript
const rect = canvas.getBoundingClientRect();
const x = Math.floor((e.clientX - rect.left) * (canvas.width / rect.width));
const y = Math.floor((e.clientY - rect.top) * (canvas.height / rect.height));
```

### 3. preventDefault() on All Events

**Why**: Prevents browser default behavior (like scrolling, shortcuts) from interfering with game controls.

### 4. Lazy Initialization

**Why**: InputBackend is only created when the game calls DirectInput APIs, avoiding unnecessary initialization for games that don't use input.

### 5. Virtual Keyboard as Separate Component

**Why**: Keeps UI modular and reusable. Can be shown/hidden independently of emulator state.

## Mobile Considerations

### Touch Event Handling

Touch events are converted to mouse events for compatibility:
- `touchstart` → `OnMouseDown(button=0)`
- `touchmove` → `OnMouseMove(x, y)`
- `touchend` → `OnMouseUp(button=0)`

### Virtual Keyboard Layout

The keyboard is optimized for mobile screens:
- Smaller keys on mobile (32px vs 40px)
- Responsive font sizes
- Compact layout with gap spacing
- Max height constrained to prevent covering canvas

### Screen Real Estate

The virtual keyboard is collapsible:
- Default state: Hidden (max-height: 0)
- Expanded state: Visible (max-height: 350px on desktop, 280px on mobile)
- Toggle button in header for easy access

## Testing Recommendations

### Desktop Testing

1. Load ign_teas.exe in WASM frontend
2. Click canvas to focus
3. Press keyboard keys → should see input events in debug log
4. Move mouse over canvas → should see mouse move events
5. Verify game responds to input

### Mobile Testing

1. Load ign_teas.exe on mobile browser
2. Tap virtual keyboard toggle button
3. Tap keys → should see input events
4. Tap on canvas → should register as mouse clicks
5. Verify game responds to virtual keyboard

### Touch Testing

1. Use Chrome DevTools mobile emulation
2. Verify touch events convert to mouse events
3. Test multitouch scenarios (if applicable)

## Known Limitations

### 1. No Key Repeat

Current implementation doesn't handle key repeat (holding a key down). Each key press requires a separate tap on virtual keyboard.

**Workaround**: Physical keyboard on desktop does support key repeat through browser's native repeat events.

### 2. No Modifier Key States

Virtual keyboard doesn't maintain SHIFT/CTRL/ALT pressed state. Each tap is immediate down+up.

**Impact**: Keyboard shortcuts (like Ctrl+C) may not work as expected on mobile.

**Future Enhancement**: Add "sticky" modifier keys that stay pressed until tapped again.

### 3. Limited Key Set

Virtual keyboard doesn't include all possible keys (Tab, PgUp/PgDn, etc.) to save space.

**Workaround**: Most games only need the keys provided (QWERTY, arrows, function keys).

### 4. No IME Support

International keyboard input (Chinese, Japanese, Korean) is not yet supported.

## Performance Considerations

### Event Rate Limiting

No explicit rate limiting is implemented. Browser's native event throttling is sufficient for most cases.

**Consideration**: For games with very high input demands, may want to throttle mouse move events to ~60Hz.

### Memory Management

Proper disposal is critical to prevent memory leaks:
- `DotNetObjectReference` is disposed when InputBackend is disposed
- Event listeners remain on canvas (acceptable since canvas persists)

### WASM-Specific Optimizations

- Events use `invokeMethodAsync` (async) to avoid blocking browser
- No synchronous `.GetAwaiter().GetResult()` calls in event handlers
- Coordinates are pre-calculated in JavaScript before C# invocation

## Future Enhancements

### 1. Gamepad Support

Add support for USB/Bluetooth gamepads:
```javascript
window.addEventListener('gamepadconnected', (e) => {
    // Map gamepad buttons to keyboard/mouse
});
```

### 2. Customizable Key Bindings

Allow users to remap keys:
```razor
<KeyBindingEditor @bind-Bindings="_keyBindings" />
```

### 3. Virtual Joystick

Add on-screen joystick for mobile:
```razor
<VirtualJoystick OnMove="HandleJoystickMove" />
```

### 4. Input Recording/Replay

Capture input sequences for testing/debugging:
```csharp
public class InputRecorder
{
    public void Record(UIEventArgs e) { /* ... */ }
    public void Replay() { /* ... */ }
}
```

## Troubleshooting

### Input Not Working

**Symptom**: Keys pressed but game doesn't respond

**Checks**:
1. Is canvas focused? (Click canvas first)
2. Are events reaching JavaScript? (Check browser console)
3. Is DotNetObjectReference initialized? (Check debug log)
4. Is game using DirectInput? (May need to call InitializeAsync manually)

### Virtual Keyboard Not Appearing

**Symptom**: Toggle button doesn't show keyboard

**Checks**:
1. Is VirtualKeyboard component in Home.razor?
2. Are styles loaded? (Check app.css)
3. Is `.visible` class toggling? (Inspect element)

### Wrong Keys Detected

**Symptom**: Pressing 'A' but game sees 'B'

**Checks**:
1. Check VK_MAP in index.html
2. Verify key codes in debug log
3. Test on different browsers (key codes may vary)

## References

- [Win32 Virtual Key Codes](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes)
- [MDN KeyboardEvent.code](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/code)
- [Blazor JavaScript Interop](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability)
- [HTML5 Canvas Events](https://developer.mozilla.org/en-US/docs/Web/API/Canvas_API)
- [Touch Events API](https://developer.mozilla.org/en-US/docs/Web/API/Touch_events)

## Conclusion

The input system implementation successfully bridges the gap between browser events and the Win32 emulator, enabling full interactivity on both desktop and mobile platforms. The virtual keyboard provides an essential mobile-first experience, making Win32 games playable on touch devices.

**Key Achievement**: Transformed the WASM frontend from a passive viewer to a fully interactive emulator with cross-platform input support.
