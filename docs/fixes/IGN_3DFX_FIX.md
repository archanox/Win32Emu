# Fix for ign_3dfx.exe Crash

## Problem

The emulator crashed when running `ign_3dfx.exe` with the following error:

```
fail: Win32Emu.Emulator[0]
      Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x001FFF2E)
      NOTE: Address 0xFFFFFFF5 is STD_OUTPUT_HANDLE (pseudo-handle value -11).
      System.IndexOutOfRangeException: Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x001FFF2E)
```

## Root Cause Analysis

### Sequence of Events

1. Program calls `GetStdHandle(STD_INPUT_HANDLE)` which returns NULL (correct for GUI apps without console)
2. EBP register contains import hook address `0x0F0000A0` (from indirect call pattern: `MOV EBP, [IAT]; CALL EBP`)
3. After `GetStdHandle` returns, `RestoreEbpFromStack` tries to restore EBP from stack
4. Restoration fails because `[ESP]` after stack cleanup doesn't contain a valid frame pointer
5. **BUG**: Code sets `EBP = ESP` as a "safe fallback" (line 1206 in Emulator.cs)
6. This corrupts the frame pointer
7. When caller's function epilogue executes:
   ```asm
   mov esp, ebp  ; ESP = wrong value (0x001FFF2C)
   pop ebp       ; EBP = [ESP], ESP += 4
   ret           ; EIP = [ESP] = 0x001FFF2E (random stack value!)
   ```
8. Execution jumps to stack address `0x001FFF2E`, causing crash

### Why Setting EBP = ESP Was Wrong

The issue is that `RestoreEbpFromStack` is called with the ESP value **after** stack cleanup:

```csharp
// In Emulator.cs, lines 481-493:
var esp = _cpu.GetRegister("ESP");      // ESP = 0x001FFF24 (at call entry)
var retEip = _vm!.Read32(esp);          // Read return address from [ESP]

esp += 4 + (uint)argBytes;              // Clean up stack: ESP = 0x001FFF2C

_cpu.SetRegister("ESP", esp);
_cpu.SetEip(retEip);

RestoreEbpFromStack(esp);               // Called with cleaned-up ESP!
```

After cleanup, `[ESP]` no longer points to meaningful data - it's pointing to whatever was above the arguments on the stack. Reading `[0x001FFF2C]` returned `0x00000000`, which failed validation as a frame pointer.

Setting `EBP = ESP` seemed "safe" but actually caused:
- EBP pointing to cleaned-up stack location
- When caller does `mov esp, ebp`, ESP jumps to wrong location
- Return address read from wrong stack location
- Execution jumps to random address (in this case, the stack itself)

## Solution

Changed `RestoreEbpFromStack` in `Emulator.cs` to **leave EBP unchanged** when restoration fails, rather than setting it to ESP.

### Code Change

**Before** (lines 1203-1208):
```csharp
else
{
    // Can't restore from stack, set to ESP as a safe fallback
    _cpu!.SetRegister("EBP", esp);
    _logger.LogDebug("[Emulator] Reset EBP to ESP (was import hook 0x{OldEBP:X8}, stack restoration failed)", currentEbp);
}
```

**After**:
```csharp
else
{
    // Can't restore from stack - leave EBP unchanged
    // Setting EBP=ESP would corrupt the frame pointer if the caller relies on it
    // The caller code that used EBP for the indirect call will handle restoring it
    _logger.LogDebug("[Emulator] Cannot restore EBP from stack (was import hook 0x{OldEBP:X8}), leaving unchanged", currentEbp);
}
```

### Rationale

1. **Consistency**: This aligns with the pattern used elsewhere in the same function (lines 1230-1255) where EBP is left unchanged when it appears to be intentionally holding a non-frame-pointer value

2. **Caller Responsibility**: When code uses the indirect call pattern `MOV EBP, [IAT]; CALL EBP`, it's explicitly using EBP to hold the function pointer. The caller is responsible for managing EBP restoration, not the emulator.

3. **No Corruption**: Leaving EBP as an import hook address (0x0F0000A0) is preferable to setting it to a wrong value (ESP). If the caller tries to use EBP incorrectly, it will be caught, but it won't cause silent stack corruption.

## Testing

### New Tests Added

Two tests were added to `FileIOTests.cs` to prevent regression:

1. **`GetStdHandle_WithImportHookInEBP_ShouldNotCorruptEBP`**
   - Verifies that EBP is not set to ESP when restoration fails
   - Ensures EBP remains unchanged (still contains import hook address)

2. **`GetFileType_AfterGetStdHandle_ShouldNotCrashWithCorruptedStack`**
   - Tests the exact API call sequence that caused the crash
   - Verifies stack remains valid after sequential calls

### Test Results

```
Passed!  - Failed:     0, Passed:   314, Skipped:     4, Total:   318
```

All existing tests pass, plus 2 new tests added.

### Security Scan

CodeQL security scan: **0 alerts found**

## Impact

This fix prevents crashes in applications that:
- Use GUI subsystem without console
- Call `GetStdHandle` which returns NULL
- Use indirect call patterns with EBP (`MOV EBP, [IAT]; CALL EBP`)
- Have function epilogues that restore ESP from EBP

The fix is minimal and surgical, only changing the EBP restoration behavior when it would have corrupted the frame pointer, maintaining compatibility with existing behavior.

## Files Modified

1. **Win32Emu/Emulator.cs** - Fixed EBP restoration logic
2. **Win32Emu.Tests.Kernel32/FileIOTests.cs** - Added regression tests

## Related Documentation

- `EBP_COM_POINTER_FIX.md` - Previous fix that introduced the problematic EBP=ESP pattern (this fix refines that approach by leaving EBP unchanged instead)
- `PSEUDO_HANDLE_ERROR_FIX.md` - Enhanced diagnostics for pseudo-handle errors
