# IAT Debugging Test - Usage Guide

## Overview

The `IgnitionTeaserIATDebugTests.cs` test provides a systematic way to debug the IAT (Import Address Table) corruption issue in IGN_TEAS.EXE.

## Running the Test

```bash
cd /path/to/Win32Emu
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionTeaserIATDebugTests" --logger "console;verbosity=detailed"
```

## What the Test Does

### Phase 1: Load Executable
- Loads IGN_TEAS.EXE into the emulator
- Does NOT start execution yet

### Phase 2: Validate IAT After Load
- Reads critical IAT entries directly from memory
- Checks three key entries:
  - `0x004552E0` (ClientToScreen) → should be `0x0F000000`
  - `0x004552F8` (LoadIconA) → should be `0x0F000060` ⚠️ **This is the problematic one**
  - `0x004552FC` (LoadCursorA) → should be `0x0F000070`

### Phase 3: Execute
- Runs the game with a 2-second timeout
- Captures any exceptions that occur

### Phase 4: Final Validation
- Re-checks IAT entries after execution attempt
- Compares before/after states

### Phase 5: Analysis
- If the expected error occurs, analyzes what's in memory
- Determines if runtime IAT protection worked

## Expected Output Scenarios

### Scenario 1: IAT Correct After Load, Corrupted During Execution
```
=== Phase 2: Validate IAT After Load ===
  0x004552F8 (LoadIconA): 0x0F000060 (expected 0x0F000060) ✓ OK

=== Phase 4: Final IAT Validation ===
  0x004552F8 (LoadIconA): 0x001FEF10 (expected 0x0F000060) ✗ CORRUPTED
  WARNING: Value points to stack region!

!!! CONFIRMED: IAT corruption detected !!!
ERROR: Runtime IAT protection did NOT fix the corruption!
```
**Conclusion:** Corruption happens at runtime, and the runtime protection isn't working.

### Scenario 2: IAT Corrupted After Load
```
=== Phase 2: Validate IAT After Load ===
  0x004552F8 (LoadIconA): 0x001FEF10 (expected 0x0F000060) ✗ CORRUPTED
```
**Conclusion:** Corruption happens during PE load phase. The load-time verification isn't working.

### Scenario 3: Runtime Protection Works
```
=== Phase 2: Validate IAT After Load ===
  0x004552F8 (LoadIconA): 0x0F000060 (expected 0x0F000060) ✓ OK

=== Phase 4: Final IAT Validation ===  
  0x004552F8 (LoadIconA): 0x0F000060 (expected 0x0F000060) ✓ OK

!!! CONFIRMED: IAT corruption detected !!!
UNEXPECTED: IAT value is correct, but error was still thrown
```
**Conclusion:** Runtime protection IS working (fixes the value), but there's a timing issue or the fix happens after the value was already read.

## Debugging the Runtime Protection

The test directly reads from VirtualMemory to see what's actually stored, bypassing any protection logic. This tells us if:
1. The corruption is in actual memory (physical storage)
2. The corruption is in the read path (logic issue)

## Next Steps Based on Results

**If Scenario 1:** 
- Fix: Improve runtime IAT protection timing
- Maybe hook at a different point (before register write vs after memory read)

**If Scenario 2:**
- Fix: Improve load-time IAT verification
- Check if sections are overwriting IAT after it's initialized

**If Scenario 3:**
- Fix: Cache the IAT values in registers or ensure fixes happen before CPU reads

## Integration Test vs Unit Test

This is an integration test because:
- It loads real executable (IGN_TEAS.EXE)
- It exercises full emulator stack
- It validates end-to-end behavior

This approach is better than unit tests because the IAT corruption issue involves interaction between:
- PE loader
- Memory system  
- CPU emulation
- Import stub dispatch

A unit test wouldn't catch the interaction bugs.

## Assertions

The test intentionally does NOT fail on corrupted IAT during debugging. It reports the state and continues. Once we understand the issue, we can add proper assertions.
