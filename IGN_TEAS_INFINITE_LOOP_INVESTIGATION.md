# IGN_TEAS.EXE Infinite Loop Investigation

## Problem
The game hangs in an infinite loop after GetModuleFileNameA during C runtime initialization, never reaching WinMain or DirectX code.

## Investigation Steps

### 1. Confirmed Infinite Loop Location
- Last successful Win32 API call: `GetModuleFileNameA`  
- After this, the CPU executes ~2 million instructions in 3 seconds
- No further Win32 API calls are made
- The game never reaches LoadCursorA, RegisterClassA, timeBeginPeriod, or DirectX initialization

### 2. Suspicious Register Detection
Running with `--debug` flag shows continuous warnings:
```
[Debug] [Instruction 2011615] Suspicious registers detected
[Debug] [Instruction 2011616] Suspicious registers detected
...
```

This indicates **EBP or ESP registers are below 0x1000**, suggesting:
- Stack pointer corruption
- Invalid stack setup
- Incorrect return from GetModuleFileNameA

### 3. Fix Attempted: argBytes for GetModuleFileNameA

**Problem Found:** GetModuleFileNameA had missing `argBytes` metadata, causing incorrect stack cleanup.

**Fix Applied:**
- Added `GetModuleFileNameA` to hardcoded argBytes in `Win32Dispatcher.cs` (12 bytes for 3 parameters)
- Added detailed logging in `Kernel32Module.cs` to track the function call
- Added detailed logging in `Emulator.cs` to track stack state before/after adjustment

**Results:**
```
[Kernel32] GetModuleFileNameA called: h=0x00000000 lp=0x00452760 n=260
[Kernel32] GetModuleFileNameA returning 28
[Dispatcher] KERNEL32.DLL!GetModuleFileNameA returned 0x0000001C, argBytes=12
[Emulator] Before stack adjustment: ESP=0x001FFF5C EBP=0x001FFFFC RetAddr=0x004123B8 ArgBytes=12
[Emulator] After stack adjustment: ESP=0x001FFF6C EBP=0x001FFFFC NewEIP=0x004123B8
[Emulator] GetModuleFileNameA complete - execution continuing at 0x004123B8
```

**Analysis:**
- Stack adjustment is correct: ESP increased by 16 bytes (4 for return address + 12 for args)
- Return address (0x004123B8) looks valid
- EBP (0x001FFFFC) looks reasonable
- Function returns successfully

**Conclusion:** The argBytes fix is correct, but **did NOT solve the infinite loop**. The game still executes ~2M instructions after GetModuleFileNameA with "Suspicious registers detected", then stops without calling any more Win32 APIs.

### 4. Decompilation Analysis
According to ghidra.cpp (lines 7575-7620), after GetModuleFileNameA, the C runtime should:

1. Call `parse_cmdline()` to parse command line arguments
2. Call `_malloc()` to allocate memory for argv
3. Call `parse_cmdline()` again to populate argv
4. Continue C runtime initialization
5. Eventually call WinMain

The game is stuck somewhere in this sequence, likely in an infinite loop with corrupted stack pointers.

### 4. Potential Root Causes

#### A. Stack Corruption After GetModuleFileNameA
- GetModuleFileNameA might not be correctly restoring ESP
- The function signature or calling convention might be wrong
- Return value handling could be incorrect

#### B. Unimplemented or Incorrectly Implemented C Runtime Function
- `parse_cmdline` might call an unimplemented function
- The loop could be waiting for something that never completes
- An internal C runtime function might have infinite recursion

#### C. CPU Emulation Issue
- Incorrect instruction emulation causing wrong branch
- Loop condition never being met due to flag calculation error
- Division by zero or other exception not being handled

## Next Steps to Debug

### Option 1: Use Interactive Debugger (Recommended)
Set a breakpoint right after GetModuleFileNameA returns and step through:

```bash
# Create debug script
cat > /tmp/debug_after_getmodule.txt << 'EOF'
break 0x4123B8
continue
step
step
step
step
step
info registers
info stack
examine ESP 32
quit
EOF

Win32Emu.exe ./EXEs/ign_teas/IGN_TEAS.EXE --interactive-debug < /tmp/debug_after_getmodule.txt
```

Replace `0x4123B8` with the actual return address from GetModuleFileNameA (found by examining the stack).

### Option 2: Add Detailed Logging to GetModuleFileNameA
Modify `Kernel32Module.cs` to log:
- Entry parameters (hModule, lpFilename, nSize)
- Calculated return address from stack
- ESP value before and after
- Return value

### Option 3: Check parse_cmdline Implementation
The game likely has its own parse_cmdline function. Need to:
- Find its address in the decompilation
- Set breakpoint at that address
- See if it ever gets called or loops internally

### Option 4: Examine Stack State
Create a script to examine stack right after GetModuleFileNameA:
- Check if return address is valid
- Check if ESP is pointing to valid stack memory
- Verify stack frame is properly set up

## Findings Summary

**The COM vtable fixes are NOT the issue.** The game never reaches DirectX initialization. The problem is:

1. ✅ GetModuleFileNameA is called and returns successfully
2. ❌ Something goes wrong immediately after (stack corruption or infinite loop)
3. ❌ C runtime never completes initialization
4. ❌ WinMain is never called
5. ❌ No DirectX functions are ever attempted

The infinite loop appears to be related to **stack pointer corruption** indicated by the "Suspicious registers" warnings. This could be:
- Bug in GetModuleFileNameA implementation
- Incorrect calling convention or argument cleanup
- Issue with how the emulator handles function returns

## Recommended Action

1. Add enhanced logging to `GetModuleFileNameA` in `Kernel32Module.cs`
2. Check the stack state before and after the call
3. Verify ESP is correctly adjusted for the stdcall/cdecl convention
4. Use interactive debugger to step through the instructions after the return
5. Compare against what a real Windows system would do

The interactive debugger I created should be perfect for this investigation, but requires manual stepping since the loop happens very quickly.
