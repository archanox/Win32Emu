# Fix for Setup.exe Non-Responsive Buttons

## Problem Summary

Setup.exe dialog boxes were showing but buttons were not responsive. The issue was tracked down to the dialog procedure timing out during initialization, which prevented the message loop from processing button click messages.

## Root Cause

The dialog procedure was calling import functions that were not implemented by the emulator. When an import function call was detected but not implemented, the code would let execution continue to the import address (which was typically 0x00000000), causing:

1. Jump to NULL address (EIP = 0x00000000)
2. Dialog procedure timeout
3. Message loop exiting prematurely
4. Button clicks never being processed

### Log Evidence

From the problem statement logs:
```
[User32] CallDialogProcedureAsync: Step 19: EIP=0x0040113A
[User32] CallDialogProcedureAsync: Execution jumped to NULL address (0x00000000) at step 1043322
[User32] CallDialogProcedureAsync: This typically means the code called a NULL function pointer
[User32] DialogBoxParamAsync: Dialog procedure timed out, ending dialog with result 0
```

## Solution

Modified `CallDialogProcedureAsync` and `CallDialogProcedureWithTimeout` in `User32Module.cs` to handle unimplemented imports gracefully by:

1. **Detecting unimplemented imports**: When `TryInvoke` returns false (import not implemented)
2. **Simulating return**: Instead of letting execution jump to NULL, we simulate a function return
3. **Proper stack cleanup**: Use `StdCallMeta.GetArgBytes()` to get the correct parameter bytes and clean up the stack properly
4. **Default return value**: Return 0 (typically means failure or NULL) as a safe default

### Code Changes

In both `CallDialogProcedureAsync` and `CallDialogProcedureWithTimeout`, the import handling was enhanced:

```csharp
if (_dispatcher != null && _dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
{
    // Success case - dispatch handled the import
    // ... existing code ...
}
else
{
    // NEW: Import function not implemented - simulate return
    var simulatedArgBytes = 0;
    try
    {
        simulatedArgBytes = StdCallMeta.GetArgBytes(dll, name);
        _logger.LogWarning("Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes={ArgBytes}", dll, name, simulatedArgBytes);
    }
    catch
    {
        _logger.LogWarning("Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes unknown (assuming 0)", dll, name);
    }
    
    var currentEsp = cpu.GetRegister("ESP");
    var retEip = memory.Read32(currentEsp);
    
    // Pop return address + parameters (stdcall convention - callee cleans)
    currentEsp += 4 + (uint)simulatedArgBytes;
    
    cpu.SetRegister("ESP", currentEsp);
    cpu.SetRegister("EAX", 0); // Return 0 as default
    cpu.SetEip(retEip);
    
    // Restore callee-saved registers
    CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved);
    
    RestoreEbpFromStack(cpu, memory, currentEsp);
}
```

## Expected Behavior After Fix

With this fix in place:

1. **WM_INITDIALOG completes successfully**: Even if the dialog procedure calls unimplemented imports, they return gracefully with a default value
2. **Message loop runs normally**: Since the dialog procedure doesn't timeout, the message loop can process events
3. **Button clicks are processed**: When a user clicks a button:
   - The Avalonia UI posts a WM_COMMAND message to the emulator
   - The message loop retrieves the message
   - The message is dispatched to the dialog procedure
   - The dialog procedure handles the button click and calls EndDialog if appropriate
   - The dialog closes with the appropriate result

## Testing

All existing dialog tests continue to pass:
- `DialogState_InitializeAndEnd_ShouldWorkCorrectly`
- `EndDialog_WithValidDialog_ShouldReturnTrue`
- `EndDialog_WithInvalidDialog_ShouldReturnFalse`
- `GetDlgItem_ShouldReturnSyntheticHandle`
- `SetDlgItemTextA_ShouldStoreText`
- And 4 more tests

## Notes

- The fix assumes that returning 0 (NULL/failure) is an acceptable default for unimplemented functions
- Functions with side effects that are critical for the dialog initialization may still cause issues
- The proper long-term solution is to implement all required import functions
- This fix provides graceful degradation when imports are missing
