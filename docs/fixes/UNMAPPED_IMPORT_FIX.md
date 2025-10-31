# Unmapped Import Stub Fix

## Problem Description

When running `IGN_TEAS.EXE`, the emulator would crash with:
```
Calculated memory address out of range: 0xFFFFFFFD (EIP=0x0F000532)
```

The EIP shows `0x0F000532` (2 bytes after `0x0F000530`) because the CPU had already advanced past the INT3 instruction (0xCC) at `0x0F000530` and was trying to decode the next byte.

## Root Cause

The program was attempting to execute code at address `0x0F000530`, which is in the import stub address range (`0x0F000000` - `0x0FFFFFFF`) but was never mapped to an actual import.

### Import Mapping Details

- The PE loader maps imports to synthetic addresses in the `0x0F000000` range
- Each import gets a 16-byte stub at address `0x0F000000 + (index * 0x10)`
- `IGN_TEAS.EXE` has 83 imports (indices 0-82)
- These map to addresses `0x0F000000` through `0x0F000520`
- Address `0x0F000530` would be for index 83 (the 84th import), which doesn't exist
- The emulator checks for addresses in the range `[0x0F000000, 0x10000000)` (exclusive upper bound)

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

Added a safety check at the beginning of the emulator's main loop to detect when EIP is in the import stub range `[0x0F000000, 0x10000000)` but not in the `ImportAddressMap`. When this occurs:

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

This fix prevents crashes when code attempts to execute at unmapped import addresses (in the range `[0x0F000000, 0x10000000)`). It provides graceful degradation by simulating a return with a safe default value, allowing the program to continue (though functionality may be limited if the unmapped import was actually needed).

## Investigation Results (Updated)

The following improvements have been implemented to investigate and detect the root cause:

### 1. IAT Validation During PE Loading

Added comprehensive validation in `PeImageLoader.cs`:
- Tracks all IAT entry addresses to detect duplicates (potential PE corruption)
- Validates existing values at IAT entries before overwriting
- Logs summary of import mapping to detect anomalies
- Scans for unexpected data beyond the mapped import range
- Helps identify extra IAT entries that shouldn't exist

**Location:** `Win32Emu/Loader/PeImageLoader.cs` in `BuildImportMap()`

### 2. Stack Corruption Detection After Import Calls

Added runtime validation in `Emulator.cs`:
- Compares return address before and after each import call
- Detects if the return address was corrupted during API execution
- Identifies if corrupted address falls in unmapped import range
- Calculates which import index the address represents
- Provides detailed diagnostics including:
  - Whether the address is in the import stub range
  - Whether the import is actually mapped
  - Which import index it would be (e.g., index 83 when only 0-82 exist)
  - Whether this indicates a C runtime bug or array bounds issue

**Location:** `Win32Emu/Emulator.cs` in `RunNormalAsync()` syscall handling

### 3. Diagnostic Test Coverage

Created comprehensive test suites:
- **StackCorruptionDetectionTests.cs** - 6 tests validating stack corruption detection
- **IATValidationTests.cs** - 8 tests validating IAT entry processing

**Location:** `Win32Emu.Tests.Emulator/`

## Future Work

To fully resolve this issue, we need to:

1. ~~Investigate why the return address `0x0F000530` appears on the stack~~ ✅ **Detection Implemented**: Stack validation now detects when this happens and logs comprehensive diagnostics to aid investigation
2. ~~Determine if there's an IAT entry that shouldn't be there~~ ✅ **Validation Implemented**: IAT validation scans for extra entries, duplicates, and unexpected data
3. **Identify root cause of C runtime corruption** - In Progress: Validation will catch if CRT corrupts stack, providing diagnostics to identify the specific cause
4. ~~Possibly add validation of the stack after each import call to detect corruption early~~ ✅ **Validation Implemented**: Return address is validated before/after each import call

**Note:** Items 1, 2, and 4 provide detection and diagnostic capabilities to help investigate the issue. The underlying root cause (item 3) may still require fixing once identified through these diagnostics.

### Next Steps

The validation mechanisms will now provide detailed diagnostics when the issue occurs:
- Which API call caused the corruption (if any)
- Exact before/after return address values
- Whether corrupted address is in import range
- Import index and whether it's mapped
- Suggestions about possible root causes (uninitialized array, bounds error, etc.)

This will help identify whether the issue is:
- A C runtime bug with uninitialized function pointer array
- Stack corruption from earlier code
- Extra IAT entry written with wrong address
- Array bounds issue in CRT initialization code
