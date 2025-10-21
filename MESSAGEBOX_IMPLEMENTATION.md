# MessageBox Implementation Summary

## Problem
The emulated game was calling `MessageBoxA` to display an error message ("Backbuffer couldn't be obtained"), but the message was only being logged to the console and not shown to the user as an actual dialog box. This made it difficult for users to understand why the game was failing.

## Solution
Added proper MessageBox support to the emulator by:

1. **Extended IEmulatorHost Interface**: Added `OnMessageBox(MessageBoxInfo info)` method to allow the host to display message boxes.

2. **Updated User32Module**: Modified `MessageBoxA` implementation to:
   - Call the host's `OnMessageBox` method when a host is available
   - Log the message as an error to ensure visibility
   - Return appropriate Win32 button result codes (IDOK, IDCANCEL, etc.)
   - Fall back to returning IDOK (1) if no host is available

3. **Implemented in GUI Host**: Added `OnMessageBox` implementation in `EmulatorWindowViewModel` that:
   - Logs the message box content with Error severity level for visibility
   - Returns IDOK as the default result
   - Can be extended later to show actual Avalonia UI dialogs

4. **Updated Test Mocks**: Added stub implementations of `OnMessageBox` to all test host classes to maintain compatibility.

## Files Changed
- `Win32Emu/Emulator.cs`: Added `OnMessageBox` method to `IEmulatorHost` interface and `MessageBoxInfo` class
- `Win32Emu/Win32/Modules/User32Module.cs`: Updated `MessageBoxA` to call host when available
- `Win32Emu.Gui/ViewModels/EmulatorWindowViewModel.cs`: Implemented `OnMessageBox` with logging
- Test files: Added stub implementations to all mock host classes

## Benefits
1. **Improved User Experience**: Error messages are now visible to users through the debug output with Error severity
2. **Better Debugging**: MessageBox calls are logged with full context (caption, text, type)
3. **Extensible**: The infrastructure is in place to show actual UI dialogs in the future
4. **Backward Compatible**: Existing code continues to work with the fallback behavior

## Future Enhancements
The current implementation logs message boxes to output. Future enhancements could include:
- Showing actual Avalonia MessageBox dialogs on the UI thread
- Supporting all Win32 MessageBox button combinations (OK, OK/Cancel, Yes/No, etc.)
- Supporting MessageBox icons (Error, Warning, Information, Question)
- Implementing proper modal behavior for blocking MessageBox calls

## Win32 MessageBox Constants
The implementation supports Win32 MessageBox type flags:
- Button types (bits 0-3): MB_OK (0x00), MB_OKCANCEL (0x01), MB_YESNO (0x04), etc.
- Icon types (bits 4-7): MB_ICONERROR (0x10), MB_ICONWARNING (0x30), MB_ICONINFORMATION (0x40), etc.
- Default button (bits 8-9): MB_DEFBUTTON1 (0x000), MB_DEFBUTTON2 (0x100), etc.
- Modality (bits 12-13): MB_APPLMODAL (0x0000), MB_SYSTEMMODAL (0x1000), etc.

## Testing
All existing tests continue to pass with the new mock implementations returning IDOK (1) as the default button result.
