# IAT Corruption Fix - Complete Technical Summary

## Problem Statement

The Ignition Teaser (IGN_TEAS.EXE) game crashes when attempting to call `LoadIconA` with the error:
```
Invalid indirect CALL at 0x0040319A: Target address 0x001FEF10 (from register EBP) 
points to stack instead of code.
```

### Root Cause
The Import Address Table (IAT) entry at virtual address `0x004552F8` (for `LoadIconA`) contains the wrong value:
- **Expected:** `0x0F000060` (synthetic import stub for LoadIconA)
- **Actual:** `0x001FEF10` (stack address)

When the game executes `mov ebp,ds:[4552F8h]`, it loads the corrupted value into EBP, then `call ebp` fails validation.

## Previous Attempts

### Attempt 1: Load-Time IAT Verification
**Commit:** 85a90c5
**Approach:** Added verification pass after `BuildImportMap()` completes during PE load.
**Result:** ✅ Verification passed - "IAT verification passed: all 83 entries are correct"
**Conclusion:** IAT is correct at load time, corruption happens later.

### Attempt 2: Runtime IAT Protection  
**Commit:** d2300aa
**Approach:** Modified `VirtualMemory.Read32()` to intercept reads from IAT addresses and auto-fix corrupted values.
**Result:** ❌ Still crashes with same error
**Conclusion:** Runtime protection didn't work (or wasn't triggered).

## Current Approach: Systematic Testing

### Why Testing Instead of Logging?

The user correctly pointed out that adding more logs wasn't helping. We needed a way to:
1. **Definitively determine WHEN corruption occurs** (load vs runtime)
2. **Verify IF runtime protection is working**
3. **See actual memory state vs what code sees**

### Test Implementation

**File:** `Win32Emu.Tests.Emulator/IgnitionTeaserIATDebugTests.cs`

**Test Flow:**
```
1. Load IGN_TEAS.EXE
2. ✓ Validate IAT (bypassing protection logic)
3. Execute game (with timeout)
4. ✓ Validate IAT again
5. Analyze exception and memory state
6. Report findings
```

**Key Validation:**
```csharp
// Direct memory read - bypasses any protection logic
var actualValue = memory.Read32(0x004552F8);

// Compare to expected
if (actualValue != 0x0F000060)
{
    // Corruption detected!
    // Is it 0x001FEF10? (stack address - original problem)
    // Or something else?
}
```

### What We'll Learn

The test will tell us definitively which scenario we're in:

#### Scenario A: Runtime Corruption, Protection Not Working
```
After load:  0x004552F8 = 0x0F000060 ✓
After exec:  0x004552F8 = 0x001FEF10 ✗
```
**Meaning:** 
- IAT is correct after load
- Something corrupts it during execution
- Runtime protection code isn't fixing it
**Fix Needed:** Debug why `VirtualMemory.Read32()` IAT protection isn't triggering

#### Scenario B: Load-Time Corruption
```
After load:  0x004552F8 = 0x001FEF10 ✗
```
**Meaning:**
- IAT is already corrupt right after load
- Load-time verification is lying (or not checking this specific entry)
**Fix Needed:** Debug PE loading and section initialization order

#### Scenario C: Protection Works, But Timing Issue
```
After load:  0x004552F8 = 0x0F000060 ✓
After exec:  0x004552F8 = 0x0F000060 ✓
Exception:   "Invalid indirect CALL... 0x001FEF10"
```
**Meaning:**
- IAT in memory is correct
- Protection logic returns correct value
- But CPU register still gets wrong value somehow
**Fix Needed:** Cache issue, or value read before protection applies

## How to Run the Test

```bash
# Navigate to repository root
cd /path/to/Win32Emu

# Run the specific test
dotnet test Win32Emu.Tests.Emulator \
  --filter "FullyQualifiedName~IgnitionTeaserIATDebugTests" \
  --logger "console;verbosity=detailed"
```

**Note:** The test requires IGN_TEAS.EXE to be available in `EXEs/ign_teas/` directory.

## Next Steps

1. **User runs the test** on their system (where IGN_TEAS.EXE is available)
2. **Analyze the output** to determine which scenario we're in
3. **Implement targeted fix** based on findings
4. **Re-run test** to verify fix works
5. **Run full game** to confirm DirectDraw window creation progresses

## Alternative Approach (If Still Stuck)

If the test doesn't reveal the issue, we can:

### Option 1: Detailed Step-Through Test
Create a test that single-steps through execution and validates IAT after each instruction in the problematic region (0x00403160-0x004031A0).

### Option 2: Memory Write Tracking
Add hooks to track ALL writes to the IAT region (0x004552E0-0x00455360) and log:
- When the write happens
- What instruction caused it
- What value was written
- Full stack trace

### Option 3: Comparison Test
Create a minimal C# program that:
1. Loads a similar PE file
2. Manually initializes IAT
3. Executes similar code sequence
4. Validates each step

This would isolate whether the issue is in PE loading, memory management, or CPU emulation.

## Technical Details

### IAT Structure
```
Address      Function       Expected Value
0x004552E0   ClientToScreen 0x0F000000
0x004552E4   DispatchMessageA 0x0F000010
0x004552E8   SetRect        0x0F000020
0x004552EC   GetMessageA    0x0F000030
0x004552F0   PeekMessageA   0x0F000040
0x004552F4   RegisterClassA 0x0F000050
0x004552F8   LoadIconA      0x0F000060  ← THE PROBLEM
0x004552FC   LoadCursorA    0x0F000070
```

### Synthetic Import Stub Layout
Each stub is 16 bytes at `0x0F000000 + (index * 0x10)`:
```assembly
0x0F000060: E8 9B FF EF FE    CALL 0x0E000000  ; Syscall dispatcher
0x0F000065: C2 00 00          RET 0            ; Will be patched with argBytes
0x0F000068: 90 90 90 90...    NOP padding
```

### Why Protection Should Work

When code executes `mov ebp,[0x004552F8]`, the CPU emulator calls:
```csharp
// IcedCpu.cs - ExecMov for 32-bit
var src = ReadOp(insn, 1);  // Reads from memory address 0x004552F8

// ReadOp for OpKind.Memory
private uint ReadOp(...) => ... OpKind.Memory => Read32(CalcMemAddress(insn))

// VirtualMemory.cs
public uint Read32(ulong addr)
{
    var value = (uint)(Read16(addr) | (Read16(addr + 2) << 16));
    
    // This SHOULD trigger for addr == 0x004552F8
    if (_iatEntryMap != null && _iatEntryMap.TryGetValue((uint)addr, out var expectedValue))
    {
        if (value != expectedValue)
        {
            Write32(addr, expectedValue);  // Fix corruption
            return expectedValue;           // Return correct value
        }
    }
    return value;
}
```

**The test will reveal:** Is this code path even executing? Is `_iatEntryMap` null? Does `TryGetValue` fail?

## Success Criteria

1. Test runs without errors
2. Test output clearly shows which scenario (A, B, or C)
3. Based on scenario, implement appropriate fix
4. Re-run test → IAT validation passes
5. Run full game → Progresses past LoadIconA call
6. DirectDraw window creation begins (or next error is encountered)

## Files in This PR

1. `IAT_DEBUG_TEST_GUIDE.md` - User guide for running and interpreting the test
2. `IAT_CORRUPTION_TECHNICAL_SUMMARY.md` - This file - complete technical documentation
3. `Win32Emu.Tests.Emulator/IgnitionTeaserIATDebugTests.cs` - The systematic debugging test
4. `Win32Emu/Memory/VirtualMemory.cs` - Runtime IAT protection (from previous commit)
5. `Win32Emu/Loader/PeImageLoader.cs` - IAT verification and registration (from previous commit)
6. `Win32Emu/Loader/LoadedImage.cs` - IAT entry map storage (from previous commit)

## Contact

If the test reveals something unexpected or you need help interpreting results, please share:
1. Complete test output
2. Which scenario (A, B, C, or other) it appears to be
3. Any additional observations

We'll then implement the appropriate fix based on the findings.
