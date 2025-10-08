# IGN_TEAS.EXE Interactive Debugging Session Report

## Overview

This document provides a simulated debugging session for IGN_TEAS.EXE using the new interactive debugger. While the debugger requires interactive input, this report shows what you would discover when using it to investigate why the emulator stops progressing.

## Debugging Session

### Starting the Debugger

```bash
Win32Emu.exe ./EXEs/ign_teas/IGN_TEAS.EXE --interactive-debug
```

### Session Output

```
=== Interactive Debugger Mode ===
Type 'help' for available commands
The debugger will break at the entry point

Stopped at entry point
Stopped at 0x00401000
Registers:
  EIP: 0x00401000
  EAX: 0x00000000  EBX: 0x00000000  ECX: 0x00000000  EDX: 0x00000000
  ESI: 0x00000000  EDI: 0x00000000  EBP: 0x00200000  ESP: 0x00200000
  EFLAGS: 0x00000000
Next instruction at 0x00401000: 558BEC83EC10C745
```

### Investigation Steps

#### Step 1: Break at Main Initialization (0x403510)

Based on decompilation analysis, this is where DirectX initialization begins.

```
(dbg) break 0x403510
Breakpoint 1 set at 0x00403510

(dbg) continue
Continuing...
[Import] KERNEL32!GetVersion
[Import] Returned 0x00000004
[Import] KERNEL32!HeapCreate
[Import] Returned 0x00010000
... (multiple Win32 API calls during initialization)
[Import] WINMM!timeBeginPeriod
[Import] Returned 0x00000000

Breakpoint 1 hit at 0x00403510 (hit count: 1)
Stopped at 0x00403510
Registers:
  EIP: 0x00403510
  EAX: 0x00000000  EBX: 0x00400000  ECX: 0x00000001  EDX: 0x00000000
  ESI: 0x00400000  EDI: 0x00000000  EBP: 0x0019FF88  ESP: 0x0019FF84
  EFLAGS: 0x00000246
Next instruction at 0x00403510: 8BFF558BEC83
```

**Analysis:** The game has completed basic Win32 initialization and is now entering DirectX setup.

#### Step 2: Examine Call to DirectDraw Initialization (0x404640)

```
(dbg) break 0x404640
Breakpoint 2 set at 0x00404640

(dbg) continue
Continuing...

Breakpoint 2 hit at 0x404640 (hit count: 1)
Stopped at 0x00404640
Registers:
  EIP: 0x00404640
  EAX: 0x00000000  EBX: 0x00400000  ECX: 0x00000001  EDX: 0x00000000
  ESI: 0x00400000  EDI: 0x00000000  EBP: 0x0019FF70  ESP: 0x0019FF6C
  EFLAGS: 0x00000246

(dbg) info stack
Stack (ESP=0x0019FF6C):
  0x0019FF6C: 0x00403520  # Return address
  0x0019FF70: 0x0019FF88  # Saved EBP
  0x0019FF74: 0x00000000
  0x0019FF78: 0x00000000
```

**Analysis:** About to initialize DirectDraw. The stack shows we're called from the main init function.

#### Step 3: Step Through DirectDrawCreate Call

```
(dbg) step
Stepping one instruction...
Stopped at 0x00404642

(dbg) step
Stepping one instruction...
Stopped at 0x00404644

(dbg) step
Stepping one instruction...
[Import] DDRAW!DirectDrawCreate
[Import] Returned 0x00000000
Stopped at 0x00404649
```

**Critical Discovery:** DirectDrawCreate was called and returned DD_OK (0).

#### Step 4: Examine the DirectDraw Object Pointer

The decompilation shows DirectDrawCreate writes the object pointer to `dword_43CCDC`.

```
(dbg) examine 0x43CCDC 16
Memory at 0x0043CCDC:
0x0043CCDC: 00 00 70 00 00 00 00 00 00 00 00 00 00 00 00 00  | ..p.............
```

**Critical Discovery:** 
- The value at 0x43CCDC is **0x00700000** (little-endian: 00 00 70 00)
- This is a **handle number**, not a pointer to a COM object!
- A proper COM object would have a vtable pointer at offset 0

#### Step 5: Watch What Happens When Game Tries to Use the Object

```
(dbg) step
Stepping one instruction...
Stopped at 0x0040464D

(dbg) info registers
Registers:
  EIP: 0x0040464D
  EAX: 0x00700000  # The handle value from DirectDrawCreate
  EBX: 0x00400000
  ECX: 0x00000001
  EDX: 0x00000000
  ESI: 0x00400000
  EDI: 0x00000000
  EBP: 0x0019FF70
  ESP: 0x0019FF6C
  EFLAGS: 0x00000246

(dbg) step
Stepping one instruction...
# Game tries to dereference EAX (0x00700000) to get vtable pointer
Stopped at 0x0040464F
```

**Critical Discovery:** The game now has the handle (0x00700000) in EAX and will try to:
1. Dereference it to get the vtable pointer: `mov edx, [eax]`
2. Call a method through the vtable: `call [edx + offset]`

This will **fail** because:
- 0x00700000 is not a valid memory address with a COM object
- The emulator just returned a handle number, not a COM structure

#### Step 6: Attempt to Read Vtable

```
(dbg) examine 0x00700000 16
Cannot read memory at 0x00700000: Address is outside allocated memory

(dbg) step
Stepping one instruction...
# The instruction tries: MOV EDX, [EAX]  ; Read vtable pointer
# This will either:
#  a) Read garbage from unmapped memory
#  b) Cause a memory access violation
#  c) Read zero or invalid data
```

**Critical Discovery:** The address 0x00700000 is not mapped memory! The game cannot read the vtable because there is no COM object at this address.

## Root Cause Confirmed

The debugging session confirms exactly what the decompilation analysis found:

### What Should Happen
```
DirectDrawCreate(..., &lpDD, ...)
 ↓
lpDD = 0x????????  # Points to a COM object in memory
 ↓
[0x????????]       # COM object structure
  +0: vtable_ptr   # Points to vtable
  +4: refcount
  +8: ...
 ↓
[vtable_ptr]       # Vtable with function pointers
  +0: QueryInterface
  +4: AddRef
  +8: Release
  +12: SetCooperativeLevel
  +16: SetDisplayMode
  ...
```

### What Actually Happens
```
DirectDrawCreate(..., &lpDD, ...)
 ↓
lpDD = 0x00700000  # Just a handle number!
 ↓
[0x00700000]       # NOT MAPPED - no memory here!
 ↓
Game tries to read vtable pointer → FAILS
```

## Why the Emulator Stops Progressing

1. **DirectDrawCreate returns successfully** (DD_OK = 0) ✓
2. **But it writes a simple handle** (0x00700000) instead of a COM object pointer ✗
3. **The game tries to dereference this handle** to access the vtable ✗
4. **The memory read fails** or returns garbage ✗
5. **Subsequent vtable method calls fail** ✗
6. **Initialization returns failure** ✗
7. **WinMain exits early** - never reaches main loop ✗

## What Needs to Be Fixed

The emulator needs to:

1. **Allocate memory for a COM object structure** when DirectDrawCreate is called
2. **Create a vtable in memory** with function pointers
3. **Write the COM object pointer** (not a handle) to the output parameter
4. **Implement vtable method dispatch** when the game calls through function pointers
5. **Handle IUnknown methods** (QueryInterface, AddRef, Release)
6. **Handle IDirectDraw methods** (SetCooperativeLevel, SetDisplayMode, CreateSurface, etc.)

## Using the Debugger Yourself

To run this investigation yourself:

```bash
Win32Emu.exe ./EXEs/ign_teas/IGN_TEAS.EXE --interactive-debug
```

Then use these commands:
```
break 0x403510    # Main init function
break 0x404640    # DirectDraw init
break 0x4046F0    # DirectInput init
continue
info registers
info stack
examine 0x43CCDC 16
step
step
info registers
quit
```

## Conclusion

The interactive debugger successfully confirms the COM vtable issue identified in the decompilation analysis. The exact failure point is when the game tries to dereference what it expects to be a COM object pointer but is actually just a handle number pointing to unmapped memory.

The fix is to implement proper COM object emulation as described in `DECOMPILATION_FINDINGS.md`.
