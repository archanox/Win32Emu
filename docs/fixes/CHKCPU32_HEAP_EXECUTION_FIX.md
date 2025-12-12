# CHKCPU32 Heap Execution Fix

## Problem
CHKCPU32.exe would run indefinitely in the emulator instead of exiting normally as it does on Windows. The emulator would flood logs with thousands of warnings about EIP being in heap memory range.

## Symptoms
```
[WRN] [Emulator] LOOP START: EIP=0x115038A0 is already in suspicious range at loop start! ESP=0x001FEFFA
[WRN] [Emulator] EIP=0x115038A0 is in heap memory range (0x01000000-0x6FFFFFFF). This may indicate a bad jump or return address...
[WRN] [Emulator] LOOP START: EIP=0x115038A2 is already in suspicious range at loop start! ESP=0x001FEFFA
[WRN] [Emulator] EIP=0x115038A2 is in heap memory range (0x01000000-0x6FFFFFFF). This may indicate a bad jump or return address...
... (repeating thousands of times)
```

## Root Cause
1. EIP jumped into heap memory range (0x11503xxx) and started executing data as code
2. Two duplicate checks for heap execution both logged warnings every iteration
3. No mechanism to stop execution when stuck in heap memory
4. Test would timeout after 10 seconds

## Solution
Added heap execution detection with early termination:

1. **Removed duplicate check**: Eliminated redundant warning at loop start
2. **Added counter**: Track consecutive heap executions with `consecutiveHeapExecutions` variable
3. **Stop after 10 iterations**: Terminate emulation after 10 consecutive heap executions
4. **Reset counter**: When EIP leaves heap range, reset counter to allow legitimate heap transitions
5. **Throttled logging**: Only log warning when EIP changes to reduce spam

## Code Changes
File: `Win32Emu/Emulator.cs` in `RunNormalAsync()` method (lines ~961 and ~1186-1231)

```csharp
// Track consecutive heap executions
var consecutiveHeapExecutions = 0ul;

// ... in main loop ...

// Guard: detect execution in heap memory (likely executing data)
var isExecutingInHeapRange = eipBeforeStep >= _heapBase && eipBeforeStep < HEAP_LIMIT;
var isExecutingInSpecialRange = MemoryRegions.IsInSpecialRange(eipBeforeStep);
if (isExecutingInHeapRange && !isExecutingInSpecialRange)
{
    consecutiveHeapExecutions++;
    
    // Log warning only when EIP changes
    if (eipBeforeStep != lastHeapEipWarning)
    {
        _logger.LogWarning("EIP=0x{Eip:X8} is in heap memory range. Consecutive heap executions: {Count}", 
            eipBeforeStep, consecutiveHeapExecutions);
        lastHeapEipWarning = eipBeforeStep;
    }
    
    // Stop after 10 consecutive heap executions
    if (consecutiveHeapExecutions >= 10)
    {
        _logger.LogError("HEAP EXECUTION DETECTED: EIP has been in heap memory range for {Count} consecutive iterations. Stopping emulation.", 
            consecutiveHeapExecutions);
        break;
    }
}
else
{
    // Reset counter when not in heap
    consecutiveHeapExecutions = 0;
}
```

## Results
- ✅ Test completes in 0.29 seconds (was timing out at 10+ seconds)
- ✅ Only 10 warning messages (was thousands)
- ✅ Clear error message for debugging
- ✅ No regressions in existing tests

## Known Limitations
This fix addresses the *symptom* (infinite execution) but not the *root cause* (why EIP jumps into heap). Further investigation is needed to determine:
- Which instruction or return causes the bad jump
- Whether a missing/incorrect Win32 API implementation is responsible
- Why CHKCPU32 behaves differently on Windows vs emulator

## Test
Run: `dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~CHKCPU32_ShouldLoadAndRun"`

Expected: Test passes in < 1 second with ~10 heap execution warnings and stops with "HEAP EXECUTION DETECTED" error.

## Date
2025-12-12
