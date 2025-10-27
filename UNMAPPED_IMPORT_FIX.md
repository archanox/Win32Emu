# Unmapped Import Stub Fix

## Problem Description

When running `IGN_TEAS.EXE`, the emulator would crash with:
```
Calculated memory address out of range: 0xFFFFFFFD (EIP=0x0F000532)
```

## Root Cause

The program was attempting to execute code at address `0x0F000530`, which is in the import stub address range (`0x0F000000` - `0x0FFFFFFF`) but was never mapped to an actual import.

### Import Mapping Details

- The PE loader maps imports to synthetic addresses in the `0x0F000000` range
- Each import gets a 16-byte stub at address `0x0F000000 + (index * 0x10)`
- `IGN_TEAS.EXE` has 83 imports (indices 0-82)
- These map to addresses `0x0F000000` through `0x0F000520`
- Address `0x0F000530` would be for index 83 (the 84th import), which doesn't exist

### What Was Happening

1. During C runtime initialization, `SetHandleCount` is called successfully
2. After `SetHandleCount` returns, the calling function performs its epilogue
3. The function does a `RET` instruction that reads a return address from the stack
4. The return address on the stack is `0x0F000530` (an unmapped import address)
5. The CPU jumps to `0x0F000530` and tries to execute
6. At `0x0F000530`, there's no stub (only zeros), causing a crash when it tries to decode `00 00 00 00` as instructions

## Why This Happened

The exact cause of why `0x0F000530` ended up as a return address is unclear, but possibilities include:

1. **C Runtime Bug**: The C runtime may have a function pointer array with an uninitialized entry
2. **Stack Corruption**: Earlier code may have corrupted the stack
3. **IAT Issue**: There may be an extra IAT entry that got written with the wrong address

## The Fix

Added a safety check at the beginning of the emulator's main loop to detect when EIP is in the import stub range (`0x0F000000` - `0x0FFFFFFF`) but not in the `ImportAddressMap`. When this occurs:

1. Log an error with details about the unmapped address
2. Read the return address from the stack
3. Simulate a return by:
   - Popping the return address from the stack
   - Setting EAX to 0 (safe default return value)
   - Setting EIP to the return address
4. Continue execution

### Code Location

The fix is in `Win32Emu/Emulator.cs` in the `RunNormalAsync` method, right before calling `SingleStep()`.

## Impact

This fix prevents crashes when code attempts to execute at unmapped import addresses. It provides graceful degradation by simulating a return with a safe default value, allowing the program to continue (though functionality may be limited if the unmapped import was actually needed).

## Future Work

To fully resolve this issue, we need to:

1. Investigate why the return address `0x0F000530` appears on the stack
2. Determine if there's an IAT entry that shouldn't be there
3. Check if the C runtime initialization is correct
4. Possibly add validation of the stack after each import call to detect corruption early
