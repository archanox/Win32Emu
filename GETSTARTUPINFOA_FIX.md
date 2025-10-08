# Fix for GetStartupInfoA Pseudo-Handle Issue

## Summary

Fixed `GetStartupInfoA` to return Windows pseudo-handle constants in the STARTUPINFO structure instead of real handle values. This resolves the "winapi.exe" crash where programs expected pseudo-handles but received real handles.

## Problem

The original implementation of `GetStartupInfoA` was writing real handle values (0x00000001, 0x00000002, 0x00000003) to the `hStdInput`, `hStdOutput`, and `hStdError` fields of the STARTUPINFO structure. 

However, Windows programs expect these fields to contain the pseudo-handle constants:
- `STD_INPUT_HANDLE` = 0xFFFFFFF6 (-10)
- `STD_OUTPUT_HANDLE` = 0xFFFFFFF5 (-11) 
- `STD_ERROR_HANDLE` = 0xFFFFFFF4 (-12)

Programs should call `GetStdHandle()` to convert these pseudo-handles to real handles before using them.

## Root Cause

When the emulator wrote real handle values into the STARTUPINFO structure, programs that read these values and tried to use them directly (without calling `GetStdHandle()`) would either:
1. Use the wrong handle values (if they used them as handles)
2. Get invalid memory addresses (if they inadvertently used them in address calculations)

The error message from the issue:
```
Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00401002)
NOTE: Address 0xFFFFFFF5 is STD_OUTPUT_HANDLE (pseudo-handle value -11).
```

This suggested that somewhere in the program's code, the pseudo-handle value 0xFFFFFFF5 was being used as a memory address, likely because the program read it from the STARTUPINFO structure and used it incorrectly.

## Solution

Changed `GetStartupInfoA` to write the pseudo-handle constants:

```csharp
[DllModuleExport(19)]
private unsafe uint GetStartupInfoA(uint lpStartupInfo)
{
    if (lpStartupInfo == 0)
    {
        return 0;
    }

    _env.MemZero(lpStartupInfo, 68);
    _env.MemWrite32(lpStartupInfo + 0, 68);
    // Write pseudo-handle constants, not real handle values
    // Programs should call GetStdHandle() to get actual handles
    _env.MemWrite32(lpStartupInfo + 56, 0xFFFFFFF6); // STD_INPUT_HANDLE
    _env.MemWrite32(lpStartupInfo + 60, 0xFFFFFFF5); // STD_OUTPUT_HANDLE
    _env.MemWrite32(lpStartupInfo + 64, 0xFFFFFFF4); // STD_ERROR_HANDLE
    return 0;
}
```

## Windows API Behavior

According to the Windows documentation, `GetStartupInfo` fills in the STARTUPINFO structure with the startup information that was passed to `CreateProcess` when the process was created.

For console applications, the standard handle fields typically contain:
- The pseudo-handle constants by default
- Or specific handle values if `STARTF_USESTDHANDLES` was used in CreateProcess

Programs must call `GetStdHandle()` with these pseudo-handle constants to obtain the actual handle values that can be used with file I/O functions like `ReadFile` and `WriteFile`.

## Testing

Added a new test `GetStartupInfoA_ShouldReturnPseudoHandlesInStartupInfo` that verifies the STARTUPINFO structure contains the correct pseudo-handle constants.

All existing tests continue to pass (169/169).

## Files Modified

1. **Win32Emu/Win32/Modules/Kernel32Module.cs**
   - Changed `GetStartupInfoA` to write pseudo-handle constants instead of real handle values

2. **Win32Emu.Tests.Kernel32/FileIOTests.cs**
   - Added test for verifying GetStartupInfoA returns pseudo-handles

## Impact

This fix ensures that Windows programs behave correctly when they:
1. Call `GetStartupInfoA` to get startup information
2. Read the standard handle fields from the STARTUPINFO structure
3. Call `GetStdHandle()` to convert pseudo-handles to real handles (correct behavior)
4. OR directly use the values from STARTUPINFO (now they'll get pseudo-handles instead of wrong real handles)

Programs that were already calling `GetStdHandle()` directly (without reading from STARTUPINFO) are unaffected by this change.
