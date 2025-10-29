# Stack Alignment Root Cause Investigation

## The Bandaid vs The Problem

### Current Bandaid Solutions (Lines 415-826 in Emulator.cs)

The code has THREE error recovery paths that pop only the return address (4 bytes) without knowing argBytes:

1. **Lines 432-434**: When EIP lands in unmapped import range
   ```csharp
   esp += 4; // Pop return address only
   ```

2. **Lines 790-792**: When dispatcher fails to invoke  
   ```csharp
   esp += 4; // Pop return address only.
   // WARNING: stack may become misaligned...
   ```

3. **Lines 819-820**: When CALL targets unmapped import
   ```csharp
   esp += 4; // Pop return address
   ```

### The Deeper Problem

**Question**: Why are these error recovery paths being hit at all?

**Verified Facts**:
- ✅ All 83 imports have correct argBytes metadata (verified)
- ✅ All KERNEL32 functions used by IGN_TEAS.EXE are implemented
- ✅ GetCPInfo has correct argBytes=8
- ✅ Source generator is working correctly

**The Mystery**:
The original problem statement shows the game calling 0x0F000530 (import index 83, which doesn't exist). This happens AFTER SetHandleCount returns successfully.

## Possible Root Causes

### Hypothesis 1: Function Pointer Corruption
The game might have a function pointer that got corrupted, pointing to 0x0F000530 instead of a valid import.

**How to test**: 
- Set breakpoint before the unmapped import call
- Examine the call instruction - is it `CALL [address]` (indirect through IAT)?
- Check what value is at that IAT entry

### Hypothesis 2: Off-By-One in Import Mapping
Maybe there's an off-by-one error where import index 82 should be 83, causing the last import to be unmapped.

**How to test**:
- Count the actual number of imports in IGN_TEAS.EXE PE file
- Compare to the number mapped (83 according to logs)
- Check if any import is being skipped

### Hypothesis 3: Stack Corruption from Earlier Call
Maybe a previous function call had incorrect argBytes, causing stack drift, which eventually makes the game read a corrupted return address or function pointer.

**How to test**:
- Trace ESP value through all API calls
- Calculate expected vs actual ESP after each call
- Find where they diverge

### Hypothesis 4: The Game Actually Works, But...
Maybe the game isn't actually calling 0x0F000530 in the current version. The problem statement might be from an older version before fixes were applied.

**How to test**:
- Run IGN_TEAS.EXE with current code
- Check if unmapped import error still occurs
- Verify the game actually gets stuck

## Investigation Plan

### Step 1: Reproduce the Issue
```bash
dotnet run --project Win32Emu/Win32Emu.csproj -- ./EXEs/ign_teas/IGN_TEAS.EXE 2>&1 | tee /tmp/ign_run.log
grep "unmapped" /tmp/ign_run.log
```

### Step 2: If Unmapped Import Occurs
- Note the exact address
- Note the previous API call
- Check ESP values before/after
- Identify the call instruction in game code

### Step 3: If No Unmapped Import
- The issue is already fixed
- Focus on why game gets stuck in infinite loop
- This is a different problem (missing DirectX backend implementation)

## The Real Issue vs The Symptom

**Symptom**: Stack alignment error recovery is a bandaid
**Real Issue**: Unknown - need to determine if:
- A) Unmapped imports are still being called (need to find why)
- B) Game is stuck for a different reason (DirectX backend incomplete)
- C) There's a regression from a recent "fix"

## Investigation Results

### Actual Test Run (Current Version)

**Findings**:
- ✅ **NO unmapped import errors occur**
- ✅ **NO stack alignment issues**
- ✅ All API calls complete successfully
- ✅ Game reaches USER32.LoadCursorA  
- ❌ Game gets stuck after LoadCursorA

**Output Analysis**:
```
Last API call: USER32.DLL!LoadCursorA returned 0x00017F00, argBytes=8
Total output: 273 lines
Errors found: 0
Unmapped imports: 0
```

### Conclusion

**The original problem (unmapped import 0x0F000530) is ALREADY FIXED.**

The problem statement appears to reference an older version of the code. The current codebase:
- Has all argBytes metadata correct
- Has all imports properly mapped
- Has no stack alignment issues

**The NEW problem** (different from original issue):
The game gets stuck in an infinite loop during window initialization, after LoadCursorA but before reaching DirectDraw. This is NOT a stack alignment issue - it's likely:
1. Waiting for a window message that never arrives
2. Polling for a condition that's never met
3. Missing window procedure implementation

### Recommendations

1. **Close the original issue** - unmapped import 0x0F000530 is fixed
2. **Remove the "bandaid" comments** - they're no longer accurate since the error paths aren't being hit
3. **Open a new issue** for the infinite loop after LoadCursorA
4. **Focus investigation** on why the game doesn't progress past window setup
