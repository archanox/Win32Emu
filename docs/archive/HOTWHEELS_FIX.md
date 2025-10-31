# Fix for Hot Wheels - Micro Racers Launch Issue

## Problem

Hot Wheels - Micro Racers was crashing during startup with the error:
```
[IcedCpu] INT3 breakpoint at 0x0E000000
```

The game was calling `GetProcAddress` to look up `IsProcessorFeaturePresent`, receiving a synthetic export address at 0x0E000000, and then calling that address. However, the emulator wasn't handling calls to synthetic exports in the range 0x0E000000-0x0F000000.

## Root Cause

The issue had two parts:

1. **IcedCpu.cs**: The INT3 handler only recognized import stubs (0x0F000000-0x10000000) and COM vtable stubs (0x0D000000-0x0E000000), but not synthetic exports (0x0E000000-0x0F000000).

2. **Emulator.cs**: The main execution loop checked for COM vtable calls and import calls, but didn't check for synthetic export calls before dispatching.

## Solution

### 1. Added INT3 handling for synthetic exports (IcedCpu.cs)

Added a new condition to recognize INT3 instructions in the synthetic export range:

```csharp
else if (oldEip is >= 0x0E000000 and < 0x0F000000)
{
    // This is a synthetic export stub - signal this as a call
    isCall = true;
    callTarget = oldEip;
    _logger.LogInformation("[IcedCpu] INT3 (0xCC) hooking synthetic export stub at address 0x{OldEip:X8}", oldEip);
}
```

### 2. Added synthetic export dispatch (Emulator.cs)

Added dispatch logic in all execution paths (RunNormalAsync, RunWithEnhancedDebugging, RunGdbServerAsync, RunWithInteractiveDebugger):

```csharp
else if (step.IsCall && _env.TryGetSyntheticExport(step.CallTarget, out var moduleName, out var exportName))
{
    _logger.LogInformation("[SyntheticExport] Hooked function: {ModuleName}!{ExportName} at address 0x{CallTarget:X8}", moduleName, exportName, step.CallTarget);
    
    // Save callee-saved registers and invoke the function via dispatcher
    var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);
    
    if (_dispatcher!.TryInvoke(moduleName, exportName, _cpu, _vm!, out var ret, out var argBytes))
    {
        // Handle return, clean up stack, restore registers
        ...
    }
}
```

## Testing

Created comprehensive tests:

1. **SyntheticExportTests.cs** (4 tests):
   - Verifies GetProcAddress returns valid addresses in the synthetic export range
   - Verifies synthetic exports are registered in ProcessEnvironment
   - Verifies different functions get different addresses
   - Verifies INT3 stub is written at synthetic export addresses

2. **SyntheticExportIntegrationTests.cs** (2 tests):
   - Simulates the exact Hot Wheels scenario (GetModuleHandleA → GetProcAddress → Call)
   - Verifies the complete flow works end-to-end

All tests pass (6/6), and no existing tests were broken (304/313 passing, same failures as before).

## Impact

This fix enables games to dynamically look up and call Win32 API functions via GetProcAddress, which is a common pattern in Windows applications. Previously, only statically imported functions and forwarded exports worked correctly.

## Files Changed

- `Win32Emu/Cpu/Iced/IcedCpu.cs` - Added INT3 handling for synthetic export range
- `Win32Emu/Emulator.cs` - Added synthetic export dispatch in 4 execution paths
- `Win32Emu.Tests.Kernel32/SyntheticExportTests.cs` - New test file
- `Win32Emu.Tests.Kernel32/SyntheticExportIntegrationTests.cs` - New test file
