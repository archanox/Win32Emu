# Register Preservation Fix for Hooked Win32 API Functions

## Problem

When running the Ignition game (Ign_win.exe), the emulator crashed with the following error:

```
fail: Win32Emu.Emulator[0]
      Calculated memory address out of range: 0xBDB81551 (EIP=0x00401005) size=0x10000000; 
      ESP=0x001FFF10 EBP=0x0F000610 EAX=0x00000000 EBX=0x00000065 ECX=0xFFFFFFFF EDX=0x00000001 
      ESI=0x00000001 EDI=0x00000005
```

## Root Cause

The issue was that the EBP register was corrupted, containing the value `0x0F000610`, which is the address of a hooked Win32 API function (USER32.DLL!LoadIconA) rather than a valid frame pointer.

In x86 calling conventions:
- **Caller-saved registers**: EAX, ECX, EDX (can be modified by the callee)
- **Callee-saved registers**: EBX, ESI, EDI, EBP (must be preserved by the callee)

When we hook Win32 API functions using INT3 breakpoints, we intercept the call and dispatch to our implementation. However, we were not preserving the callee-saved registers (EBX, ESI, EDI, EBP) across these hooked function calls. This caused register corruption, leading to invalid memory address calculations when the emulated code tried to use these registers.

## Solution

The fix saves the callee-saved registers before invoking a hooked function and restores them after the function returns. This is implemented in all execution modes:

1. `RunNormal()` - Normal execution mode
2. `RunWithEnhancedDebugging()` - Enhanced debugging mode
3. `RunWithInteractiveDebugger()` - Interactive debugger mode
4. `RunWithGdbServer()` - GDB server mode

### Implementation

```csharp
// Before calling hooked function:
var savedEbx = _cpu.GetRegister("EBX");
var savedEsi = _cpu.GetRegister("ESI");
var savedEdi = _cpu.GetRegister("EDI");
var savedEbp = _cpu.GetRegister("EBP");

// Call hooked function
if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm, out var ret, out var argBytes))
{
    // ... handle return ...
    
    // After returning, restore callee-saved registers:
    _cpu.SetRegister("EBX", savedEbx);
    _cpu.SetRegister("ESI", savedEsi);
    _cpu.SetRegister("EDI", savedEdi);
    _cpu.SetRegister("EBP", savedEbp);
}
```

This same pattern is applied to both Win32 API imports and COM vtable method calls.

## Testing

- Added `RegisterPreservationTests.cs` to document the fix
- Verified that 117 tests pass in the emulator test suite
- The fix prevents the "Calculated memory address out of range" error that was occurring with Ign_win.exe

## Impact

This fix ensures that:
1. Registers are correctly preserved across hooked function calls
2. The emulated code sees consistent register values before and after Win32 API calls
3. Frame pointer (EBP) based addressing works correctly
4. x86 calling conventions are properly maintained in the emulator

## Files Modified

- `Win32Emu/Emulator.cs` - Added register save/restore logic in all run methods
- `Win32Emu.Tests.Emulator/RegisterPreservationTests.cs` - Added test documenting the fix
