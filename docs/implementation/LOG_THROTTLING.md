# Log Throttling Implementation

## Problem

The emulator was generating thousands of repetitive warning messages that made it extremely difficult to identify actual errors. In particular:

1. **"LOOP START: EIP is in suspicious range"** - Logged on EVERY iteration when EIP was in heap memory range
2. **"EIP is in heap memory range"** - Logged on EVERY iteration when executing from heap
3. **"IAT entry already contains value"** - Logged for every import entry during PE loading (normal behavior)

Example from actual log file:
```
warn: Win32Emu.Emulator[0]
      [Emulator] LOOP START: EIP=0x0F000400 is already in suspicious range at loop start! ESP=0x001FEF7C
warn: Win32Emu.Emulator[0]
      [Emulator] EIP=0x0F000400 is in heap memory range...
```

This pattern repeated thousands of times, hiding the actual error:
```
fail: Win32Emu.Emulator[0]
      Invalid indirect CALL at 0x0040319A: Target address 0x001FEF10 (from register EBP) 
      points to stack instead of code.
```

## Solution

Implemented throttling for repetitive warnings using tracking variables:

### Emulator.cs Changes

Added two tracking variables to `RunNormalAsync()`:
```csharp
// Throttle noisy warning logs to reduce spam
var lastSuspiciousEipWarning = 0u;
var lastHeapEipWarning = 0u;
```

Modified warning conditions to only log when EIP changes:

**Before:**
```csharp
if (eipAtLoopStart >= _heapBase && eipAtLoopStart < HEAP_LIMIT)
{
    _logger.LogWarning("...", eipAtLoopStart, esp);
}
```

**After:**
```csharp
if (eipAtLoopStart >= _heapBase && eipAtLoopStart < HEAP_LIMIT && 
    eipAtLoopStart != lastSuspiciousEipWarning)
{
    _logger.LogWarning("...", eipAtLoopStart, esp);
    lastSuspiciousEipWarning = eipAtLoopStart;
}
```

### PeImageLoader.cs Changes

Modified IAT entry validation to only log unusual values:

**Before:**
```csharp
if (existingValue != 0)
{
    logger?.LogDebug("...already contains value... This is normal for some loaders.");
}
```

**After:**
```csharp
// Only log if value seems unexpected (outside normal stub/thunk ranges)
if (existingValue != 0 && existingValue < IMAGE_BASE_THRESHOLD)
{
    logger?.LogDebug("...contains unusual value...");
}
```

## Benefits

1. **Dramatically reduced log noise** - Warnings now appear once per unique EIP instead of thousands of times
2. **Easier debugging** - Actual errors are no longer buried in repetitive warnings
3. **Still catches issues** - Warnings still appear when execution moves to different suspicious addresses
4. **Minimal code changes** - Simple throttling logic, no complex changes to core emulation

## Behavior

With these changes:
- If EIP stays at `0x0F000400` for 1000 iterations, only 1 warning is logged instead of 1000
- If EIP moves to `0x0F000410`, a new warning is logged for the new address
- IAT debug messages are reduced from ~80 messages to only messages for truly unusual values

This maintains diagnostic capability while making logs actually usable for debugging.
