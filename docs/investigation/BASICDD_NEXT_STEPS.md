# BasicDD.exe Crash Investigation - Next Steps

## Current Status

BasicDD.exe crashes at address 0x0040715A after GetAttachedSurface returns successfully. Extensive analysis has been performed (see BASICDD_CRASH_0x0040715A_ANALYSIS.md).

## Confirmed Facts

1. ✅ COM vtable ordering is correct
2. ✅ COM method argBytes are correct (verified by tests)
3. ✅ GetAttachedSurface returns successfully with correct stack cleanup
4. ✅ Crash happens in application code, not in emulator code
5. ❌ EBP is corrupted to code address 0x0040187C
6. ❌ ESP contains 0x0F000115 (middle of GetModuleHandleA import stub)
7. ❌ Crash at 0x0040715A (data section)

## Root Cause Hypothesis

The crash is caused by stack or register corruption that occurs between:
- GetAttachedSurface return (EIP=0x0040140C)
- Crash location (EIP=0x0040715A)

Most likely causes:
1. **EBP corruption** - EBP contains code address instead of stack frame pointer
2. **Stack corruption** - ESP contains value pointing into import stub
3. **Memory corruption** - Application code overwrites stack or registers

## Implementation Plan

### Phase 1: Add Detailed Execution Tracing

Create targeted instruction-level tracing that activates after specific COM calls:

```csharp
// In DDrawModule.cs Surface_GetAttachedSurface()
private uint Surface_GetAttachedSurface(ICpu cpu, VirtualMemory mem)
{
    // ... existing code ...
    
    // Enable detailed tracing for next N instructions
    if (_enableDetailedTracing)
    {
        _logger.LogInformation("[TRACE] Enabling detailed execution tracing after GetAttachedSurface");
        // Set flag to enable tracing in emulator
    }
    
    return (uint)DDResult.DD_OK;
}
```

### Phase 2: Add Stack Validation

Add validation to detect stack corruption early:

```csharp
// In CpuHelpers.InvokeWithRegisterPreservation()
// After reading return address
var retEip = memory.Read32(esp);

// Validate stack contents
for (int i = 0; i < 20; i++)
{
    var addr = esp + (uint)(i * 4);
    var val = memory.Read32(addr);
    
    // Check for suspicious values (import stub addresses)
    if (MemoryRegions.IsInImportHookRange(val) && (val & 0xF) != 0)
    {
        logger?.LogWarning("[STACK CORRUPTION DETECTED] Stack at ESP+{Offset} contains suspicious value 0x{Val:X8} (partial import stub address)", 
            i * 4, val);
    }
}
```

### Phase 3: Add EBP Corruption Detection

Add early detection of EBP corruption:

```csharp
// In IcedCpu.SingleStep() after each instruction
if (_eip >= imageBase && _eip < imageBase + imageSize)
{
    // In application code
    if (_ebp >= imageBase && _ebp < imageBase + imageSize)
    {
        // EBP points to code section - likely corruption!
        _logger.LogError("[EBP CORRUPTION] EBP=0x{Ebp:X8} points to code section at EIP=0x{Eip:X8}", _ebp, _eip);
    }
}
```

### Phase 4: Interactive Debugger Improvements

Fix the interactive debugger so it doesn't cause different crash behavior:

1. Ensure debugger mode uses same execution path as normal mode
2. Add command to enable/disable detailed tracing
3. Add command to dump registers and stack at any point

### Phase 5: Compare with retrowin32

If available, compare execution with retrowin32 emulator:

1. Run BasicDD.exe in retrowin32
2. Compare API call sequence
3. Compare register values at key points
4. Identify differences in behavior

## Testing Plan

1. Add unit tests for EBP corruption detection
2. Add unit tests for stack validation
3. Test BasicDD.exe with detailed tracing enabled
4. Analyze trace output to find exact corruption point
5. Implement fix based on findings
6. Verify fix in headless mode

## Success Criteria

- [ ] BasicDD.exe runs without crashing
- [ ] GetAttachedSurface, Flip, and other COM methods work correctly
- [ ] No EBP or stack corruption detected
- [ ] Application displays correctly

## Timeline Estimate

- Phase 1 (Tracing): 2-4 hours
- Phase 2 (Validation): 1-2 hours  
- Phase 3 (Detection): 1-2 hours
- Phase 4 (Debugger): 2-3 hours
- Phase 5 (Comparison): 2-4 hours (if retrowin32 available)
- Testing: 2-3 hours

Total: 10-18 hours

## Alternative Approaches

If detailed tracing doesn't reveal the issue:

1. **Bisect the problem**: Add logging at more points to narrow down corruption location
2. **Check other games**: See if similar pattern exists in other DirectDraw games
3. **Memory watchpoints**: Implement memory watchpoints to catch when stack is corrupted
4. **Disassemble critical functions**: Manually review assembly of FUN_00401310, FUN_00401640
5. **Check for known issues**: Search for similar issues in other x86 emulators

## References

- Main analysis: `BASICDD_CRASH_0x0040715A_ANALYSIS.md`
- Decompilation: `Decomp/BasicDD/ghidra.cpp`
- COM implementation: `Win32Emu/Win32/Modules/DDrawModule.cs`
- COM dispatcher: `Win32Emu/Win32/COM/ComVtableDispatcher.cs`
- CPU helpers: `Win32Emu/Cpu/CpuHelpers.cs`
