# BasicDD Stack Corruption - Detailed Hypothesis

## Problem Summary

String pointer 0x004070A4 ("Basic DD") appears at stack location 0x001FEF4C where a return address should be, causing the application to jump to data section and crash at 0x0040715A.

## Confirmed Facts

1. **String Usage**: 0x004070A4 is used in window creation:
   - `RegisterClassA(&wndClass)` where `wndClass.lpszClassName = 0x004070A4`
   - `CreateWindowExA(..., 0x004070A4, 0x004070A4, ...)` - used as both class name and window title

2. **Stack Layout in FUN_00401310**:
   - EBP = 0x001FEFFC (constant throughout function)
   - [EBP-0xB0] = 0x001FEF4C (local variable `apiStack_b0`)
   - Value at 0x001FEF4C = 0x004070A4 (the corruption!)

3. **Function Epilogue** (at 0x00401410):
   ```assembly
   ADD ESP, 0x8C    ; ESP: 0x001FEEC0 → 0x001FEF4C
   RET              ; Pops [0x001FEF4C] = 0x004070A4 and jumps there
   ```

4. **Timing**: Value 0x004070A4 exists at 0x001FEF4C BEFORE any DirectDraw functions are called.

## Most Likely Root Causes

### Hypothesis 1: Stack Cleanup Error in CreateWindowExA

**Theory**: CreateWindowExA has incorrect argBytes calculation, leaving parameters on stack.

**Evidence**:
- CreateWindowExA takes 12 parameters (48 bytes on x86)
- String pointer 0x004070A4 is passed TWICE (as lpClassName and lpWindowName)
- If argBytes is wrong (e.g., 44 instead of 48), one parameter remains on stack

**How to verify**:
```csharp
// In User32Module.cs CreateWindowExA
_logger.LogWarning("[User32] CreateWindowExA called with argBytes check");
// Add explicit argBytes logging in Win32Dispatcher when this function returns
```

**Expected fix**:
Ensure DllModuleExport automatically calculates correct argBytes for CreateWindowExA (12 params × 4 bytes = 48).

### Hypothesis 2: RegisterClassA Corruption

**Theory**: RegisterClassA writes WNDCLASSA structure incorrectly, corrupting stack.

**Evidence**:
- RegisterClassA receives pointer to WNDCLASSA structure
- Structure contains lpszClassName = 0x004070A4
- If we read/write structure incorrectly, could corrupt nearby stack memory

**How to verify**:
- Add memory range checking in RegisterClassA
- Verify we don't write beyond structure boundaries
- Check if ref struct wrapper (WndClassARef) has correct size/layout

### Hypothesis 3: Window Procedure (WndProc) Setup

**Theory**: Setting up window procedure callback corrupts stack.

**Evidence**:
- `local_28.lpfnWndProc = (WNDPROC)&LAB_004012d0` sets callback
- If we store this callback incorrectly or call it with wrong convention, could corrupt stack

**How to verify**:
- Check WndProc callback mechanism in ProcessEnvironment
- Verify calling convention matches (stdcall for WndProc)
- Check if we're preserving stack correctly during callback

### Hypothesis 4: Early Function Stack Frame Corruption

**Theory**: FUN_00401310 prologue or early code writes to wrong stack location.

**Evidence**:
- Local variable at [EBP-0xB0] somehow gets 0x004070A4
- This happens BEFORE any DirectDraw calls
- Could be during function entry or early initialization

**How to verify**:
- Trace from FUN_00401310 entry (address 0x00401310)
- Watch memory writes to 0x001FEF4C
- Check if application code itself writes this value

## Recommended Investigation Steps

### Step 1: Add Memory Write Watchpoint

```csharp
// In Emulator.cs, add to SingleStep:
if (step.IsMemoryWrite && step.WriteAddress == 0x001FEF4C)
{
    _logger.LogError("[WATCHPOINT] Write to 0x001FEF4C! Value=0x{Value:X8}, EIP=0x{Eip:X8}",
        step.WriteValue, _cpu.GetEip());
    // Capture stack trace
}
```

### Step 2: Verify CreateWindowExA ArgBytes

```csharp
// In Win32Dispatcher.cs, add logging:
if (functionName == "CreateWindowExA")
{
    _logger.LogWarning("[Win32] CreateWindowExA: argBytes={ArgBytes}, ESP before={EspBefore:X8}, ESP after={EspAfter:X8}",
        argBytes, espBefore, espAfter);
}
```

### Step 3: Trace from Program Entry

Enable tracing from WinMain entry to capture ALL initialization:
```csharp
// In Emulator.cs:
if (eipAfter >= 0x00401040 && eipAfter < 0x00401060 && !_traceEnabled)
{
    _instructionTraceCount = 5000; // Trace entire initialization
    _traceEnabled = true;
}
```

### Step 4: Compare with Retrowin32

If available, run BasicDD.exe in retrowin32 and compare:
- Stack state after CreateWindowExA
- Stack state at FUN_00401310 entry
- Memory at 0x001FEF4C throughout execution

## Expected Outcome

Once we identify where 0x004070A4 gets written to 0x001FEF4C, we can:

1. **If it's our bug**: Fix the argBytes or calling convention in the responsible API
2. **If it's application bug triggered by our behavior**: Understand what Windows does differently that prevents the bug
3. **If it's intentional application behavior**: Understand why it works on Windows but not in our emulator

## Technical Notes

### Function Epilogue Analysis

The epilogue `ADD ESP, 0x8C; RET` is valid IF:
- Function doesn't use frame pointer (no PUSH EBP)
- Local variables size = 0x8C
- Stack frame layout: [return addr][local vars 0x8C bytes]

But from our trace, EBP = 0x001FEFFC is set, suggesting frame pointer IS used. This means prologue should be:
```assembly
PUSH EBP
MOV EBP, ESP
SUB ESP, 0x8C
```

And epilogue should be:
```assembly
MOV ESP, EBP  (or ADD ESP, 0x8C)
POP EBP
RET
```

But we only see `ADD ESP, 0x8C; RET` without POP EBP! This suggests either:
- Ghidra disassembly is incomplete
- Function uses non-standard epilogue
- Some compiler optimization or exception handling code

### Stack Layout Calculation

Given:
- EBP = 0x001FEFFC
- ESP after epilogue = 0x001FEF4C
- Delta = 0xB0 = 176 bytes

This matches the function having 176 bytes of local variables and stack-based parameters.

## References

- Main analysis: `BASICDD_CRASH_0x0040715A_ANALYSIS.md`
- Investigation summary: `BASICDD_INVESTIGATION_SUMMARY.md`
- Next steps: `BASICDD_NEXT_STEPS.md`
- Tutorial: https://www.codeproject.com/articles/Introduction-to-DirectDraw-and-Surface-Blitting
