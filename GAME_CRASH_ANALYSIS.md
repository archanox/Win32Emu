# Game Crash Analysis - IGN_TEAS.EXE

## Summary
The Ignition game (IGN_TEAS.EXE) crashes after approximately 1.2 million instructions with an INVALID instruction error at address 0x001FEC40.

## Problem Details

### Crash Information
- **Crash Location**: EIP=0x001FEC40
- **Stack Pointer**: ESP=0x001FEE64
- **Base Pointer**: EBP=0x0F000060
- **Iterations**: ~1,200,000
- **Error**: INVALID instruction (executing data as code)

### Memory Layout
- **Image Base**: 0x00400000 (typical Windows PE)
- **Stack Base**: 0x00200000
- **Stack Limit**: 0x00100000
- **Heap Base**: 0x01000000

### Root Cause
The crash occurs because the CPU is **executing code from the stack**, not from the normal code section. Analysis shows:

1. **EIP is in stack range**: The crash address 0x001FEC40 is within the stack region (0x00100000 - 0x00200000)
2. **Steadily incrementing EIP**: Before the crash, EIP progresses through stack addresses:
   - 1140000 iterations: EIP=0x001DF5A5
   - 1150000 iterations: EIP=0x001E43C5
   - 1160000 iterations: EIP=0x001E91E5
   - 1170000 iterations: EIP=0x001EE005
   - 1180000 iterations: EIP=0x001F2E25
   - 1190000 iterations: EIP=0x001F7C45
   - 1200000 iterations: EIP=0x001FCA65
   - **CRASH**: EIP=0x001FEC40

3. **Constant ESP**: The stack pointer (ESP=0x001FEE64) remains constant throughout this execution, suggesting the program isn't making normal function calls.

## Root Cause Analysis

Based on decompilation analysis in `Decomp/ign_teas/` and documentation in `docs/archive/DECOMPILATION_FINDINGS.md`:

**The stack execution at 0x001FEC40 is a SYMPTOM, not the root cause.**

### Actual Root Cause: DirectX Initialization Failure

The game's initialization sequence (from Ghidra decompilation):
1. `WinMain` calls `sub_403510()` for DirectX initialization
2. `sub_403510` calls `FUN_00404640()` to initialize DirectDraw
3. DirectDraw initialization calls `DirectDrawCreate()` ✅ (succeeds)
4. Then immediately calls COM vtable methods: ✅ (COM vtables implemented)
   - `lpDD->lpVtbl->SetCooperativeLevel()`
   - `lpDD->lpVtbl->SetDisplayMode()`
5. If DirectDraw init returns 0 (failure), `sub_403510` returns 0
6. WinMain takes error path, which likely corrupts stack/function pointers
7. Eventually execution jumps to invalid stack address 0x001FEC40 ❌

**Note:** COM vtable support was implemented (see `docs/fixes/COM_VTABLE_FIX_SUMMARY.md`), but some vtable method may be:
- Returning incorrect value
- Not preserving registers correctly
- Corrupting stack during execution

## Possible Causes (Updated)

### 1. COM Vtable Method Implementation Issue (Most Likely)
One of the DirectDraw COM methods is failing or corrupting state:
- `SetCooperativeLevel` may not handle parameters correctly
- `SetDisplayMode` may not validate/store display mode properly
- Method may not preserve EBP/EBX/ESI/EDI registers
- Return value may be incorrect (game expects S_OK/DD_OK)

### 2. Stack Corruption During WndProc or Message Handling
After DirectX init fails, the error handling path may:
- Call WndProc with corrupted parameters
- Incorrectly clean up the stack
- Leave function pointers in invalid state

### 3. Missing or Incorrect DirectX Object State
The COM objects created may be missing required state:
- DirectDraw object missing display mode information
- Surface objects not properly initialized
- Cooperative level not set correctly

### 4. Register Preservation Issue
COM method calls may not be preserving registers correctly:
- EBP corruption (documented fix in `docs/archive/EBP_COM_POINTER_FIX.md`)
- Other callee-saved registers (EBX, ESI, EDI) may be corrupted
- ESP misalignment after method return

## Recommendations for Debugging

### 1. Enable Enhanced Debugging with GDB Server
Run with GDB server to trace exact execution path:
```bash
Win32Emu.Gui --nogui IGN_TEAS.EXE --gdb-server
```
Then attach Ghidra to:
- Set breakpoint at `sub_403510` (DirectX init function)
- Trace through DirectDraw COM method calls
- Identify which method returns failure or corrupts state

### 2. Enable File Logging with Debug Mode
Capture full logs for analysis:
```bash
Win32Emu.Gui --nogui IGN_TEAS.EXE --debug --log-file
```

### 3. Add Detailed COM Method Logging
Temporarily add logging in `DDrawModule.cs` COM methods:
- Log entry to each vtable method (SetCooperativeLevel, SetDisplayMode, etc.)
- Log parameters and return values
- Log register state before/after
- Identify which method fails or returns incorrect value

### 4. Check Decompilation for Expected Behavior
Review `Decomp/ign_teas/ghidra.cpp` to understand:
Review `Decomp/ign_teas/ghidra.cpp` to understand:
- Exact sequence of DirectDraw method calls in `FUN_00404640`
- Expected parameters for each COM method
- Expected return values (should be 0 for success/DD_OK)
- What the game does if DirectX init fails

### 5. Cross-Reference with Existing Fixes
Check these documentation files for related issues:
- `docs/fixes/COM_VTABLE_FIX_SUMMARY.md` - COM vtable implementation
- `docs/archive/EBP_COM_POINTER_FIX.md` - Register preservation issues
- `docs/archive/STACK_CORRUPTION_FIX.md` - Stack corruption during WndProc
- `docs/archive/DECOMPILATION_FINDINGS.md` - Original root cause analysis

## Next Steps

1. **Add COM method call tracing** - Log every COM vtable method invocation with parameters and return values

2. **Verify DirectDraw initialization** - Check that:
   - SetCooperativeLevel is called with correct flags
   - SetDisplayMode is called with valid dimensions
   - Both methods return success (0/DD_OK)

3. **Check register preservation** - Ensure COM methods preserve:
   - EBP (base pointer)
   - EBX, ESI, EDI (callee-saved registers)
   - ESP properly aligned after return

4. **Investigate error path** - If DirectX init fails:
   - What does the game do in the error handler?
   - Does it clean up properly?
   - Does it corrupt stack or function pointers?

## Related Documentation

- `Decomp/ign_teas/ghidra.cpp` - Full decompilation showing execution flow
- `docs/archive/DECOMPILATION_FINDINGS.md` - Detailed analysis of initialization sequence
- `docs/fixes/COM_VTABLE_FIX_SUMMARY.md` - COM implementation details
- `docs/archive/EBP_COM_POINTER_FIX.md` - Register corruption fix

## File Logging Feature
To make issue reporting easier, the new file logging feature has been implemented:
- Use `--log-file` to automatically generate a log file with MD5 hash of the executable
- Format: `<executable>_<md5hash>_<timestamp>.log`
- Example: `IGN_TEAS_42aeaf49af6191400fa18ba3e3c47e48_20251107_161715.log`

This makes it easy to identify logs for specific games when reporting issues.
