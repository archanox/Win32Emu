# BasicDD.exe Crash Investigation - Comprehensive Summary

## Executive Summary

BasicDD.exe from the DirectX SDK samples crashes at address 0x0040715A immediately after the `GetAttachedSurface` COM method returns successfully. Extensive analysis has been performed to diagnose the root cause.

## Investigation Timeline

### Session 1: Initial Analysis (2025-11-23)

**Goals:**
- Understand the existing analysis in `BASICDD_CRASH_0x0040715A_ANALYSIS.md`
- Reproduce the crash
- Verify existing hypotheses
- Identify root cause

**Completed:**
1. ✅ Built project successfully (Release configuration)
2. ✅ Reproduced crash consistently at 0x0040715A in headless mode
3. ✅ Confirmed crash characteristics:
   - EIP = 0x0040715A (data section)
   - ESP = 0x001FEF70 (contains 0x0F000115 - middle of GetModuleHandleA import stub)
   - EBP = 0x0040187C (code address inside FUN_00401873 - invalid frame pointer)
   - EAX = 0x00000065 (resource ID from LoadImageA call)

4. ✅ Verified COM vtable implementation:
   - Vtable ordering is correct (verified against MSDN and retrowin32)
   - All COM vtable tests pass (17/17)
   - argBytes calculation is correct (uses `ComDelegateHelper`)
   - GetAttachedSurface argBytes = 12 (3 params × 4 bytes) ✓

5. ✅ Analyzed execution flow:
   - GetAttachedSurface returns successfully to 0x0040140C
   - Stack cleanup is correct: ESP moves from 0x001FEEB0 to 0x001FEEC0 (16 bytes = 4 ret + 12 args)
   - NO API/COM calls logged between GetAttachedSurface return and crash
   - Crash happens in application code, not emulator code

6. ✅ Identified key evidence:
   - Stack pointer moved 176 bytes (0xB0) between GetAttachedSurface return and crash
   - EBP corruption to code address suggests register/stack manipulation
   - ESP contains corrupted value pointing into import stub
   - Interactive debugger shows DIFFERENT crash location (0x001FEFF9 vs 0x0040715A) - suggests debugger affects execution

7. ✅ Ruled out:
   - COM vtable ordering issues (fixed in previous work)
   - Incorrect argBytes for COM methods (verified by tests and calculation)
   - Issues in GetAttachedSurface implementation (returns correctly)
   - Missing COM method implementations (all required methods present)

## Current Understanding

### What We Know

1. **Crash Location**: 0x0040715A is in the data section (DAT_00407154 + 6 bytes)
2. **Corrupted Registers**: 
   - EBP points to code (0x0040187C) instead of stack
   - ESP contains partial import stub address (0x0F000115)
3. **Execution Sequence**:
   ```
   FUN_00401310 (DirectDraw init)
   ├─> DirectDrawCreateEx
   ├─> SetCooperativeLevel (offset 0x50)
   ├─> SetDisplayMode (offset 0x54)
   ├─> CreateSurface (offset 0x18)
   └─> GetAttachedSurface (offset 0x30) ✓ Returns successfully to 0x0040140C
   
   [APPLICATION CODE EXECUTES]
   
   CRASH at 0x0040715A ✗
   ```

4. **Missing Link**: The crash happens in application code between 0x0040140C and before the next API call. This is a "black box" - we don't have visibility into what the application is doing.

### What We Don't Know

1. **Exact instruction** that causes EIP to jump to 0x0040715A
2. **When/how** EBP gets corrupted to 0x0040187C
3. **Why** ESP contains 0x0F000115 (import stub address)
4. **What code** executes between 0x0040140C and the crash

## Technical Analysis

### EBP Corruption Analysis

EBP = 0x0040187C points inside FUN_00401873:
```c
int __cdecl FUN_00401873(int param_1)
{
  int iVar1;
  iVar1 = FUN_00401806(param_1);
  return (iVar1 != 0) - 1;
}
```

This function is called early in the initialization (line 17 of entry point). The address 0x0040187C being in EBP suggests either:
1. EBP was set to this address intentionally by application code (non-standard usage)
2. EBP was corrupted by a buffer overflow or memory corruption
3. EBP restoration logic has a bug

### ESP Corruption Analysis

ESP = 0x001FEF70 contains 0x0F000115

This value is exactly 5 bytes into the GetModuleHandleA import stub:
```
0x0F000110: GetModuleHandleA stub (16 bytes)
0x0F000115: +5 bytes (middle of CALL instruction encoding)
```

This is IMPOSSIBLE as a valid return address. Import stubs use this layout:
```
0x0F000110: FF 15 XX XX XX XX   ; CALL [syscall_addr]  (6 bytes)
0x0F000116: C3                   ; RET                 (1 byte)
0x0F000117: 90 90 ... 90         ; NOP padding
```

At offset +5 (0x0F000115), we're in the middle of the CALL instruction. This can only happen through corruption.

### FUN_00401640 Analysis

This function (called after GetAttachedSurface) has unusual characteristics:
```c
undefined4 __thiscall FUN_00401640(void *this, int *param_1, 
                                   undefined4 param_2, undefined4 param_3)
{
  int unaff_retaddr;  // Ghidra detected return address access!
  
  // ... code ...
  
  if (unaff_retaddr != -1) {
    (**(code **)(*(int *)*puVar2 + 0x74))((int *)*puVar2,8,&stack0xffffff6c);
  }
  *(int *)((int)this + 0x1c) = unaff_retaddr;  // Stores return address!
}
```

The function:
1. Reads the return address from the stack
2. Compares it to -1
3. Conditionally calls SetColorKey (offset 0x74) based on the comparison
4. Stores the return address in a data structure

This is unusual but not necessarily wrong. It could be part of a callback mechanism or state tracking.

## Diagnostic Challenges

1. **Black Box Problem**: Execution between 0x0040140C and 0x0040715A is not logged
2. **Debugger Heisenbug**: Interactive debugger changes behavior (different crash location)
3. **No Instruction Trace**: Need instruction-level tracing to see exact execution path
4. **Complex Assembly**: Hand-optimized assembly code may use non-standard patterns

## Proposed Solutions

### Immediate (Phase 1)
1. **Add instruction-level tracing** after GetAttachedSurface returns
2. **Implement EBP validation** after each instruction in suspect range
3. **Add stack corruption detection** to catch invalid values early

### Medium-term (Phase 2)
1. **Fix interactive debugger** to not affect execution
2. **Add memory watchpoints** to detect when stack/registers are corrupted
3. **Compare with retrowin32** behavior (if available)

### Long-term (Phase 3)
1. **Enhance COM call logging** to include caller information
2. **Add automated corruption detection** for all register/stack operations
3. **Create regression tests** for BasicDD.exe

## Next Actions

### Critical Path
1. Enable detailed instruction tracing for addresses 0x0040140C - 0x00401700
2. Capture register and stack state after each instruction
3. Identify the exact instruction that:
   - Corrupts EBP to 0x0040187C
   - Causes execution to reach 0x0040715A
   - Leaves 0x0F000115 on the stack

### Implementation
See `BASICDD_NEXT_STEPS.md` for detailed implementation plan.

## Testing Strategy

1. **Reproduce crash** with detailed tracing enabled
2. **Analyze trace** to find corruption point
3. **Implement fix** based on findings
4. **Verify in headless mode** that crash is resolved
5. **Test with other DirectDraw games** to ensure no regressions

## Success Criteria

- [ ] BasicDD.exe runs without crashing
- [ ] All COM methods (GetAttachedSurface, Flip, Restore, etc.) work correctly
- [ ] No register or stack corruption detected
- [ ] Application displays graphics correctly (if possible in headless mode)

## References

### Documentation
- `BASICDD_CRASH_0x0040715A_ANALYSIS.md` - Original analysis
- `BASICDD_NEXT_STEPS.md` - Implementation plan
- `MESSAGE_DISPATCHER_IMPLEMENTATION.md` - Message handling
- `DEBUGGING_GUIDE.md` - Debugging features

### Source Files
- `Decomp/BasicDD/ghidra.cpp` - Decompiled application code
- `Win32Emu/Win32/Modules/DDrawModule.cs` - DirectDraw implementation
- `Win32Emu/Win32/COM/ComVtableDispatcher.cs` - COM vtable handling
- `Win32Emu/Cpu/CpuHelpers.cs` - Register/stack management
- `Win32Emu/Cpu/Iced/IcedCpu.cs` - CPU emulation

### Tests
- `Win32Emu.Tests.Emulator/ComVtableOrderingTests.cs` - COM vtable tests (17/17 passing)
- `Win32Emu.Tests.Emulator/ComVtableValidationTests.cs` - Validation tests

## Conclusion

The BasicDD.exe crash is a complex issue involving stack/register corruption in application code. While we've ruled out COM vtable issues and verified correct argBytes calculation, the root cause remains elusive without instruction-level tracing.

The next critical step is implementing detailed execution tracing to capture the exact sequence of instructions that leads to the crash. Once we have this trace, we can identify the corruption point and implement a targeted fix.

**Estimated Time to Resolution**: 10-18 hours of focused development and debugging.

---

**Last Updated**: 2025-11-23
**Status**: Investigation in progress, awaiting Phase 1 implementation
