# Using the Interactive Debugger to Investigate IGN_TEAS.EXE

This document provides a practical example of using the new interactive debugger to investigate why the IGN_TEAS.EXE emulation stops progressing.

## Problem Statement

Based on the decompilation analysis, we know that:
1. The game calls DirectDrawCreate and DirectInputCreate
2. It expects COM objects with vtables to be returned
3. The emulator currently returns simple handles instead of proper COM objects
4. The game tries to call methods through these vtables and fails

## Starting the Debug Session

```bash
cd /home/runner/work/Win32Emu/Win32Emu
dotnet run --project Win32Emu -- ./EXEs/ign_teas/IGN_TEAS.EXE --interactive-debug
```

## Step-by-Step Investigation

### 1. Break at the Critical Initialization Function

From the decompilation analysis (see `Decomp/ign_teas/ANALYSIS.md`), we know that function `sub_403510` at address `0x403510` is the critical initialization point.

```
=== Interactive Debugger Mode ===
Type 'help' for available commands
The debugger will break at the entry point

Stopped at entry point
Stopped at 0x00401XXX

(dbg) break 0x403510
Breakpoint 1 set at 0x00403510

(dbg) continue
Continuing...
```

### 2. When the Breakpoint Hits

```
Breakpoint 1 hit at 0x00403510 (hit count: 1)
Stopped at 0x00403510
Registers:
  EIP: 0x00403510
  EAX: 0x00000000  EBX: 0x00000000  ECX: 0x00000001  EDX: 0x00000000
  ESI: 0x00400000  EDI: 0x00000000  EBP: 0x0019FF88  ESP: 0x0019FF84
  EFLAGS: 0x00000246

(dbg) info stack
Stack (ESP=0x0019FF84):
  0x0019FF84: 0x00401XXX  # Return address
  0x0019FF88: 0x00000000
  ...
```

### 3. Step Through DirectDraw Initialization

From the decompilation, we know `sub_404640` is called for DirectDraw initialization:

```
(dbg) break 0x404640
Breakpoint 2 set at 0x00404640

(dbg) continue
Continuing...

Breakpoint 2 hit at 0x00404640 (hit count: 1)
Stopped at 0x00404640

(dbg) step
Stepping one instruction...
...
```

### 4. Examine Where DirectDrawCreate Returns

When DirectDrawCreate is called, it should write a COM object pointer to the output parameter. Let's examine what actually gets written:

```
# After DirectDrawCreate returns, the output parameter should point to a COM object
# The decompilation shows it's stored at dword_43CCDC

(dbg) examine 0x43CCDC 16
Memory at 0x0043CCDC:
0x0043CCDC: XX XX XX XX 00 00 00 00 00 00 00 00 00 00 00 00  | ....

# This shows the handle value that was written
# A proper COM object would have a vtable pointer at offset 0
```

### 5. Check When the Game Tries to Use the Vtable

The decompilation shows the game immediately tries to call methods via the vtable. We can set a breakpoint at the first vtable call:

```
# From the decompilation, after DirectDrawCreate returns,
# the game does: lpDD->lpVtbl->SetCooperativeLevel(...)
# This dereferences the object pointer to get the vtable

(dbg) step    # Step through several instructions
(dbg) step
(dbg) registers  # Check what's about to happen
(dbg) info stack
```

### 6. Observe the Failure

When the game tries to dereference what it thinks is a COM object pointer, it will either:
1. Read invalid memory (if the handle value is treated as an address)
2. Jump to an invalid address (if it tries to call through a bad function pointer)
3. Get stuck in a loop waiting for something that never happens

You can observe this by:

```
(dbg) step
(dbg) registers
(dbg) step
(dbg) registers
# Repeat to see if EIP is changing or stuck
```

## Expected Observations

### What You'll See

1. **DirectDrawCreate is called** and returns success (EAX = 0)
2. **A handle value (like 0x70000000) is written** to the output parameter
3. **The game tries to dereference this handle** as if it were a pointer to a COM object
4. **The dereference reads invalid memory** or gets a garbage vtable pointer
5. **Subsequent execution fails** or loops indefinitely

### Key Addresses to Watch

From the decompilation analysis:

- `0x403510` - Main initialization function
- `0x404640` - DirectDraw initialization  
- `0x4046F0` - DirectInput initialization
- `0x43CCDC` - Global variable storing DirectDraw object pointer
- `0x43CEB0` - Global variable storing DirectInput object pointer

## What This Reveals

Using the debugger confirms what the decompilation analysis found:

1. ✅ The emulator successfully calls DirectDrawCreate
2. ✅ DirectDrawCreate returns DD_OK (0)
3. ❌ DirectDrawCreate writes a simple handle instead of a COM object
4. ❌ The game expects a proper COM object structure with vtable
5. ❌ When the game tries to call vtable methods, it fails

## Next Steps

The debugger has helped us confirm the root cause. To fix the issue:

1. Implement proper COM object creation in `DDrawModule.cs`
2. Create vtable structures in emulator memory
3. Populate vtables with function pointers to emulated DirectX methods
4. Use the debugger to verify the fix works

## Tips for Your Own Debugging

### Setting Multiple Breakpoints

You can set breakpoints at all critical functions at once:

```
(dbg) break 0x403510
(dbg) break 0x404640
(dbg) break 0x4046F0
(dbg) info breakpoints
```

### Examining Function Parameters

Before a function call, check the stack to see parameters:

```
(dbg) info stack
# The stack shows: return address, then parameters
```

### Following Execution Flow

Use `step` repeatedly and watch EIP to see the execution path:

```
(dbg) step
(dbg) registers
(dbg) step
(dbg) registers
# etc.
```

### Quitting Early

If you've seen enough:

```
(dbg) quit
```

## Comparing with Enhanced Debug Mode

The `--debug` flag gives you automatic error detection:

```bash
dotnet run --project Win32Emu -- ./EXEs/ign_teas/IGN_TEAS.EXE --debug
```

This is good for catching crashes, but `--interactive-debug` is better for:
- Understanding execution flow
- Setting precise breakpoints
- Examining state at specific points
- Stepping through problematic code

## Advanced: Automating Breakpoint Setup

You could modify `InteractiveDebugger.cs` to auto-set breakpoints based on a configuration file:

```json
{
  "breakpoints": [
    { "address": "0x403510", "description": "Main init" },
    { "address": "0x404640", "description": "DirectDraw init" },
    { "address": "0x4046F0", "description": "DirectInput init" }
  ]
}
```

This would be a future enhancement to the debugger.

## Conclusion

The interactive debugger is a powerful tool for understanding exactly what's happening during emulation. Combined with the decompilation analysis, it provides clear confirmation of the COM vtable issue and helps guide the implementation of the fix.

For the next phase of development, implement the COM vtable support as described in `DECOMPILATION_FINDINGS.md` and use the debugger to verify that vtable methods are being called correctly.
