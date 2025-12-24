# WM_CREATE Window Procedure Crash Fix (Issue #996)

## Problem Statement

When handling WM_CREATE messages, the window procedure would crash when execution jumped to a NULL address (0x00000000). This occurred when a window class was registered with a NULL window procedure pointer.

## Root Cause Analysis

The issue was in the `ExecuteStdCallProcedureAsync` method in `User32Module.cs`. The method performed EIP validation checks **after** calling `CpuHelpers.ExecuteAsync()`, which meant the CPU had already attempted to fetch and execute instructions from the invalid address before the check could abort execution.

```csharp
// BEFORE (buggy version):
while (true)
{
    var eip = cpu.GetEip();
    
    // Execute first, then check (too late!)
    var step = await CpuHelpers.ExecuteAsync(cpu, memory).ConfigureAwait(false);
    
    // Check for NULL after execution attempt
    if (eip == 0x00000000)
    {
        failed = true;
        break;
    }
}
```

The synchronous version `ExecuteStdCallProcedure` correctly checked EIP **before** calling `cpu.SingleStep()`, preventing execution at invalid addresses.

## Solution

Moved the EIP validation checks to occur **before** the `ExecuteAsync` call, matching the pattern used in the synchronous version:

```csharp
// AFTER (fixed version):
while (true)
{
    var eip = cpu.GetEip();
    
    // Check if we've returned to our marker address
    if (eip == RETURN_ADDRESS)
    {
        break;
    }

    // Check for invalid EIP BEFORE attempting to execute
    if (eip == 0x00000000)
    {
        _logger.LogWarning("[User32] {Context}: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting", contextName);
        failed = true;
        break;
    }

    // Check for other invalid low addresses BEFORE attempting to execute
    if (eip < MINIMUM_VALID_EIP && eip != RETURN_ADDRESS)
    {
        _logger.LogError("[User32] {Context}: Execution jumped to invalid low address 0x{Eip:X8}", contextName, eip);
        failed = true;
        break;
    }
    
    // Now it's safe to execute
    var step = await CpuHelpers.ExecuteAsync(cpu, memory).ConfigureAwait(false);
}
```

## Changes Made

### 1. User32Module.cs
- Reordered validation checks in `ExecuteStdCallProcedureAsync` (lines 2828-2845)
- Added clarifying comments about preventing NULL pointer execution
- No changes to `ExecuteStdCallProcedure` (already correct)

### 2. ReactOSPortedTests_WindowCreation.cs
- Added test `CreateWindowExA_WithNullWndProc_ShouldNotCrash`
- Registers a window class with WndProc = 0x00000000
- Creates a window, which posts WM_CREATE message
- Verifies the window is created successfully without crashing

## Test Results

- Window creation tests: 17/17 passed (includes new NULL WndProc test)
- CPU tests: 11/11 passed
- No regressions introduced

## Impact

This fix prevents crashes when:
1. A window class is registered with a NULL window procedure
2. CreateWindow/CreateWindowEx sends WM_CREATE to the window
3. The message dispatcher attempts to call the window procedure

The fix gracefully aborts execution with a warning log message instead of crashing, allowing the application to continue.

## Related

- Issue #996: Window procedure crash investigation
- `docs/implementation/ASYNC_WINDOW_PROCEDURE_ARCHITECTURE.md`: Async window procedure design
- `docs/implementation/MESSAGE_DISPATCHER_IMPLEMENTATION.md`: Message handling architecture
