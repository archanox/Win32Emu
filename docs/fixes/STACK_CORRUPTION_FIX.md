# Stack Corruption Fix in CallWindowProcedure

## Problem Description

When `CallWindowProcedure` in `User32Module.cs` encountered an exception during WndProc execution, it would restore CPU registers (EIP, ESP, EBP) but leave stack memory in a corrupted state. This caused subsequent function calls to read garbage data from the stack, leading to execution jumping to invalid memory addresses.

## Symptoms Observed

From the error logs:
```
warn: Win32Emu.Emulator[0]
      [IcedCpu] Unhandled mnemonic INVALID at 0x001FFC40
warn: Win32Emu.Emulator[0]
      [IcedCpu] Unhandled mnemonic INVALID at 0x001FFC42
fail: Win32Emu.Emulator[0]
      Calculated memory address out of range: 0x52290043 (EIP=0x001FFC65)
```

The EIP jumped to stack addresses (0x001FFC40) which contained invalid instructions, then to completely invalid memory (0x52290043).

## Root Cause Analysis

### Call Setup Process

When `CallWindowProcedure` sets up a call to a WndProc:

1. Saves current CPU state (EIP, ESP, EBP)
2. Pushes return address (0xDEADBEEF marker) onto stack
3. Pushes 4 parameters (hwnd, message, wParam, lParam) onto stack
4. Sets EIP to WndProc address and executes

This writes 5 dwords (20 bytes) to addresses: `(savedEsp - 20)` through `(savedEsp - 1)`

### Exception Handling Issue

When an exception occurred:

1. Exception was caught and `executionSuccessful` set to false
2. CPU registers restored: `cpu.SetRegister("ESP", savedEsp)`
3. **Stack memory NOT cleaned up**

The result: ESP is restored to original value, but the 20 bytes of data (including the 0xDEADBEEF return address) remain in memory below ESP.

### Subsequent Call Corruption

On the next call to `CallWindowProcedure`:

1. New call setup writes data to the same stack locations
2. If any data remains from previous failed call, it can be read by the WndProc
3. WndProc might call a function or return, reading garbage as a return address
4. Execution jumps to invalid address (e.g., 0x001FFC40 on stack or 0x52290043 in invalid memory)

## Solution Implemented

Added stack memory cleanup when execution is unsuccessful:

```csharp
if (!executionSuccessful)
{
    // Clear the stack memory region that was used for the call
    // This includes the return address and parameters (5 dwords = 20 bytes)
    var stackDataSize = 20u; // Return address (4) + hwnd (4) + message (4) + wParam (4) + lParam (4)
    for (uint i = 0; i < stackDataSize; i += 4)
    {
        memory.Write32(savedEsp - stackDataSize + i, 0);
    }
    _logger.LogDebug("[User32] CallWindowProcedure: Cleaned up {Size} bytes of stack memory after failed execution", stackDataSize);
}
```

This writes zeros to all 20 bytes that were used during the call setup, ensuring no garbage data remains that could corrupt subsequent calls.

## Impact

- **Performance**: Minimal - cleanup only runs on failed executions (exceptions or timeouts)
- **Correctness**: Prevents stack corruption and execution jumping to invalid addresses
- **Security**: Prevents potential exploitation of stack corruption for arbitrary code execution

## Testing

- Build verified: No compilation errors
- CodeQL security analysis: 0 vulnerabilities found
- Manual verification: Stack cleanup addresses match pushed data locations

## Files Modified

- `Win32Emu/Win32/Modules/User32Module.cs`: Added stack cleanup in `CallWindowProcedure` exception handling
