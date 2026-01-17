# Fix for Unhandled COM Vtable Calls

## Problem
When DirectDraw COM interface methods were called but not yet implemented, the emulator would crash with an "INVALID instruction" error instead of gracefully handling the unimplemented method.

### Symptom
```
warn: Win32Emu.Emulator[0]
      [COM] Unhandled COM vtable call at 0x0D001140 (method: IDirectDraw::SetCooperativeLevel)
...
fail: Win32Emu.Emulator[0]
      [JitCpu] INVALID instruction at EIP=0x0043EB7D. Bytes: FF FF 00 FF FF FF 00 FF FF FF 00 FF FF FF 00 FF
```

### Root Cause
When `_env.ComDispatcher.TryInvoke()` returned `false` for an unimplemented COM method, the code would continue execution into the INT3 stub instructions instead of properly returning to the caller. This caused the CPU to eventually jump to invalid memory addresses, leading to crashes.

## Solution
The fix replaces manual register handling with `CpuHelpers.InvokeWithRegisterPreservation`, which:

1. **Handles Success Case**: Properly returns with the method's return value in EAX
2. **Handles Failure Case**: Returns EAX=0 (error) and cleans up the stack gracefully

### Code Change
**Location**: `Win32Emu/Emulator.cs`, line 2792

**Before** (missing failure handling):
```csharp
if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var ret, out var comArgBytes))
{
    // Handle success...
}
// BUG: No else clause - execution continues into INT3 stub!
```

**After** (proper error handling):
```csharp
CpuHelpers.InvokeWithRegisterPreservation(
    _cpu, _vm!,
    () => {
        var success = _env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var returnValue, out var argBytes);
        return (success, returnValue, argBytes);
    },
    _vm!.Size, _logger, "COM vtable", _image);
// HandleFailedInvocation is called automatically on failure
```

## Impact
- Games using unimplemented DirectDraw methods will now return gracefully with error codes
- The CPU will continue normal execution instead of crashing
- Games can potentially handle errors or fall back to alternative code paths

## Testing
All four COM call handling locations in `Emulator.cs` have been verified to use proper error handling:
- Line 2344: Async mode (uses `InvokeWithRegisterPreservationAsync`)
- Line 2626: Sync mode (uses `InvokeWithRegisterPreservation`)
- Line 2792: Sync mode (FIXED - now uses `InvokeWithRegisterPreservation`)
- Line 2886+: GDB debug mode (uses `InvokeWithRegisterPreservationAsync`)

## Related Files
- `Win32Emu/Emulator.cs` - Main fix location
- `Win32Emu/Cpu/CpuHelpers.cs` - Helper function that handles failures
- `Win32Emu/Win32/COM/ComVtableDispatcher.cs` - COM method dispatcher
