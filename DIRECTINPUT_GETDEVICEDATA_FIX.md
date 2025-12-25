# DirectInput GetDeviceData Fix for ign_teas

## Problem Summary
The game `ign_teas` was running but not responding to keyboard or mouse input. Analysis of the ApiMon logs revealed that the game was calling `IDirectInputDevice::GetDeviceData` in its main loop to poll for input events, but the function was always returning 0 events (empty).

## Root Cause
The `GetDeviceData` method in `DInputModule.cs` was a stub implementation that:
1. Validated parameters
2. Always returned 0 events with comment "For now, return 0 elements (no buffered events)"
3. Never queried the input backend or generated any events

This meant games using buffered input (the standard DirectInput pattern) would never receive keyboard or mouse events.

## Solution Implemented

### Changes to DirectInputDevice Class
Added state tracking and event buffering capabilities:
```csharp
// Previous state for detecting changes (needed for buffered events)
public Dictionary<int, bool> PreviousKeyStates { get; set; } = new();
public Dictionary<int, bool> PreviousMouseButtons { get; set; } = new();
public int PreviousMouseX { get; set; }
public int PreviousMouseY { get; set; }
public int PreviousMouseZ { get; set; }

// Buffered input events queue
public Queue<DeviceObjectData> EventQueue { get; set; } = new();
public uint EventSequence { get; set; } // Sequence counter for events
```

### New DeviceObjectData Struct
Represents a DIDEVICEOBJECTDATA structure:
```csharp
private struct DeviceObjectData
{
    public uint dwOfs;       // +0: Offset in data format (key scancode, mouse offset)
    public uint dwData;      // +4: Value (0x80=pressed, 0x00=released, or axis value)
    public uint dwTimeStamp; // +8: Timestamp from Environment.TickCount
    public uint dwSequence;  // +12: Sequence number for event ordering
}
```

### Full GetDeviceData Implementation
The method now:
1. **Validates parameters** - Checks device is acquired, validates cbObjectData size
2. **Polls input backend** - Calls `IInputBackend.PollDevice()` to get current state
3. **Detects changes** - Compares current state vs previous state
4. **Generates keyboard events**:
   - For each of 256 keys, if pressed/released state changed
   - Event offset = key scancode (0-255)
   - Event data = 0x80 (pressed) or 0x00 (released)
5. **Generates mouse button events**:
   - For each of 4 mouse buttons, if pressed/released state changed
   - Event offset = 12, 13, 14, or 15
   - Event data = 0x80 (pressed) or 0x00 (released)
6. **Generates mouse movement events**:
   - X axis: offset 0, data = X position (delta)
   - Y axis: offset 4, data = Y position (delta)
   - Z axis (wheel): offset 8, data = Z position (delta)
7. **Writes events to buffer** - Copies queued events to output buffer in DIDEVICEOBJECTDATA format
8. **Returns event count** - Updates output parameter with number of events returned

## How It Works

### Event Generation Flow
```
Game calls GetDeviceData()
  ↓
Poll input backend for current state
  ↓
Compare with previous state
  ↓
For each changed key/button/axis:
  - Create DeviceObjectData event
  - Add to EventQueue
  - Update previous state
  ↓
Dequeue events (up to requested count)
  ↓
Write to output buffer (16 bytes per event)
  ↓
Return event count to game
```

### Event Format (DIDEVICEOBJECTDATA)
Each event is 16 bytes:
- **Bytes 0-3**: dwOfs - Offset in data format
  - Keyboard: 0-255 (scancode)
  - Mouse buttons: 12-15 (button 0-3)
  - Mouse X: 0, Mouse Y: 4, Mouse Z: 8
- **Bytes 4-7**: dwData - Value
  - Keys/buttons: 0x80 (pressed) or 0x00 (released)
  - Mouse axes: Position value (delta movement)
- **Bytes 8-11**: dwTimeStamp - Environment.TickCount
- **Bytes 12-15**: dwSequence - Event sequence number

## Testing Instructions

### Prerequisites
- Win32Emu compiled with the fix
- ign_teas.exe in EXEs/ign_teas/ directory
- Input backend initialized (SDL3, GLFW, etc.)

### Test Steps
1. Run ign_teas:
   ```bash
   dotnet run --project Win32Emu.Gui -- ./EXEs/ign_teas/IGN_TEAS.EXE
   ```

2. Test keyboard input:
   - Press keys on keyboard
   - Game should respond to input
   - Check logs for "[DInput COM] Returned X events" messages

3. Test mouse input:
   - Move mouse
   - Click mouse buttons
   - Game should respond to mouse movement and clicks

4. Verify in logs:
   - Look for: "IDirectInputDevice::GetDeviceData"
   - Should see: "Returned X events, Y remaining"
   - X should be > 0 when keys/mouse are active

### Expected Behavior
**Before fix:**
```
[DInput COM] IDirectInputDevice::GetDeviceData(...)
[DInput COM]   Requested elements: 16
[DInput COM]   Returned 0 events, 0 remaining
```

**After fix:**
```
[DInput COM] IDirectInputDevice::GetDeviceData(...)
[DInput COM]   Requested elements: 16
[DInput COM]   Returned 3 events, 0 remaining   <-- Events are returned!
```

## Technical Details

### Keyboard Event Example
When user presses the 'A' key (scancode 30):
```
dwOfs = 30          // Key scancode
dwData = 0x80       // Pressed (0x00 would be released)
dwTimeStamp = 12345 // Environment.TickCount
dwSequence = 42     // Incremental sequence number
```

### Mouse Movement Event Example
When mouse moves right by 10 pixels:
```
dwOfs = 0           // X axis offset
dwData = 10         // Delta X movement
dwTimeStamp = 12345
dwSequence = 43
```

## Compatibility

### What Games Benefit
- **ign_teas** - Fixed, now receives input
- Any game using buffered DirectInput (GetDeviceData)
- Games that check input in their main loop

### What Games Are NOT Affected
- Games using immediate mode (GetDeviceState only)
- Games using Win32 message loop for input (GetMessage/PeekMessage)
- Games that don't use DirectInput at all

## Performance Considerations

### Event Queue Size
- Events are queued until GetDeviceData is called
- Queue grows if game doesn't poll frequently enough
- Consider adding a maximum queue size if memory is a concern

### Change Detection Overhead
- Compares 256 keys + 4 mouse buttons + 3 axes per poll
- Only generates events when state actually changes
- Minimal overhead for typical game input polling rates (60-120 Hz)

## Debugging Tips

### Enable Verbose Logging
Set log level to Information to see:
- When GetDeviceData is called
- How many events are requested
- How many events are returned
- Which keys/buttons changed

### Check Input Backend
If still no input after fix:
1. Verify `_env.InputBackend` is not null
2. Verify `device.BackendDeviceId != 0`
3. Check `PollDevice` returns true
4. Check InputState has data in KeyStates/MouseButtons dictionaries

### Common Issues
- **Backend not initialized**: Check DirectInputCreateA logs for "Input subsystem initialized"
- **Device not acquired**: Check for "Device acquired successfully" log
- **Wrong device type**: Verify GUID detection (0x6F1D2B61 = keyboard, 0x6F1D2B60 = mouse)

## References

### DirectInput Documentation
- [IDirectInputDevice8::GetDeviceData](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ee416637(v=vs.85))
- [DIDEVICEOBJECTDATA](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ee416628(v=vs.85))
- [Buffered vs Immediate Data](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ee416854(v=vs.85))

### Wine Implementation
For reference implementation patterns, see:
- [Wine dinput/device.c](https://gitlab.winehq.org/wine/wine/-/blob/master/dlls/dinput/device.c)
- Focus on `IDirectInputDevice8AImpl_GetDeviceData` function

### ApiMon Logs
The fix was developed based on analysis of:
- `ApiMon Logs/ign_teas/ign_teas.exe.csv` - Shows GetDeviceData being called in game loop
- Game loop pattern: PeekMessage → GetDeviceData → GetCurrentPosition → Lock → Unlock → Flip

## Conclusion

The DirectInput GetDeviceData fix enables proper buffered input handling for ign_teas and similar games. The implementation:
- ✅ Polls input backend for current state
- ✅ Detects changes in keyboard/mouse state
- ✅ Generates properly formatted DIDEVICEOBJECTDATA events
- ✅ Returns events to calling code
- ✅ Maintains previous state for change detection
- ✅ Follows Win32 DirectInput API specification

Games using buffered input should now work correctly with keyboard and mouse input.
