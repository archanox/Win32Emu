# IGN_TEAS.EXE Infinite Loop Investigation

## **INVESTIGATION STATUS** - Emulation Issue Identified ⚠️

**IMPORTANT UPDATE:** Initial hypothesis about uninitialized PE data was **incorrect**.

### The Real Problem: Emulator Bug, Not PE File Issue

**Verified Facts:**
- ✅ PE file at 0x43AB91 does contain zeros (verified with xxd)
- ✅ **BUT the game works perfectly on real Windows hardware**
- ✅ Therefore: the issue is in the **emulator**, not the PE file
- ✅ Windows handles character classification dynamically at runtime

The decompiler showing `g_a43AB91[]` as empty is a **red herring** - it's just showing uninitialized .data, which Windows populates at runtime through APIs like `GetStringTypeA/GetStringTypeW` or other CRT initialization.

### Likely Root Causes (Emulation Issues)

1. **GetStringTypeA/GetStringTypeW bug** - These functions are implemented in Kernel32Module.cs but may have incorrect behavior
2. **Stack corruption from missing argBytes** - Other functions besides GetModuleFileNameA may have incorrect cleanup
3. **CPU flag handling** - CMP, TEST, and conditional jumps may not set FLAGS correctly
4. **Function return handling** - stdcall/cdecl cleanup might be wrong in some cases

### Evidence from Interactive Debugger

Using the debugger at breakpoint 0x004123B8:
- Stack state is correct after GetModuleFileNameA
- ESP/EBP values are valid
- Return address (0x004123B8) is in valid code range
- The infinite loop happens AFTER this point in C runtime code

### Evidence from PE File Analysis

```bash
$ xxd -s $((0x38F91)) -l 32 EXEs/ign_teas/IGN_TEAS.EXE
0x38F91: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
         00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
```

The .data section at RVA 0x3AB91 is indeed all zeros in the PE file. **However**, since the game runs on Windows, this is normal - Windows CRT initializes this data at runtime.

## Next Steps for Diagnosis

Focus on emulator issues, not the PE file:

1. **Test GetStringTypeA/GetStringTypeW** - Add logging to see if they're being called and returning correct values
2. **Check for other missing argBytes** - Audit all Kernel32 functions for correct parameter counts
3. **Add CPU flag logging** - Log ZF/CF/SF/OF before/after CMP/TEST instructions
4. **Step through with register inspection** - Use interactive debugger to see which instruction causes the loop

### Solution Required

The PE loader must properly initialize the `.data` section at address 0x0043AB91 with the character type table. This is typically 256 bytes containing bit flags:
- 0x01: uppercase letter
- 0x02: lowercase letter  
- 0x04: digit/whitespace/special characters
- 0x08: hex digit
- etc.

## Debugging Steps Taken

### Step 1: Used Interactive Debugger to Set Breakpoint at 0x004123B8

Created debugging script and ran:
```bash
Win32Emu.exe ./EXEs/ign_teas/IGN_TEAS.EXE --interactive-debug < debug_script.txt
```

Results:
- Breakpoint hit successfully at 0x004123B8 (return from GetModuleFileNameA)
- Registers show correct state: EAX=0x0000001C, ESP=0x001FFF6C, EBP=0x001FFFFC
- Stack looks correct
- Stepped through 10 instructions showing argument setup for function call

### Step 2: Analyzed Decompilation to Understand Code Flow

From `/Decomp/ign_teas/reko.cpp`:
- Address 0x004123B8 is in function `fn004123A0()` (C runtime initialization)
- This function calls `fn00412440()` which is a command-line parser
- The parser has character type checks that may rely on Windows APIs
- The emulator likely has a bug in how it handles these checks

### Step 3: Verified PE File Is Correct

Used `xxd` to dump the PE file at address 0x43AB91:
- The .data section does contain zeros at this location
- **However**: The game works on real Windows, so this is expected
- Windows CRT populates runtime data structures dynamically

## Recommended Next Steps

### 1. Add Comprehensive Logging to Character Classification

Add logging to `GetStringTypeA` and `GetStringTypeW` in Kernel32Module.cs to see if they're called and what they return.

### 2. Step Through with Full CPU State Logging

Use the interactive debugger to step through instructions after 0x004123B8 while logging:
- Each instruction executed
- Register values before/after
- FLAGS register state (ZF, CF, SF, OF)
- Memory reads/writes

### 3. Audit argBytes for All Functions

Check Win32Dispatcher.cs for any other functions with missing or incorrect argBytes.

### 4. Test Simple Case

Create a minimal test program that just calls GetModuleFileNameA and returns, to isolate the stack corruption issue.

---

## Original Investigation Below (Context)
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

## Recommended Actions & Status

### 1. GetStringTypeA/GetStringTypeW Investigation ✅ IN PROGRESS

**Status:** Functions ARE implemented with test coverage
- GetStringTypeA: 6 existing tests in BasicFunctionsTests.cs
- GetStringTypeW: 1 existing test in NewFunctionsTests.cs
- **Action Taken:** Added 13 comprehensive tests in GetStringTypeTests.cs

**Potential Issue:** GetStringTypeW is incomplete - missing punct, blank, cntrl, xdigit flags
If C runtime uses these functions for character classification, incomplete implementation could cause parsing failures.

### 2. argBytes Metadata Audit ✅ COMPLETED

**Findings:** Documented in ARGBYTES_INVESTIGATION.md
- StdCallArgBytesGenerator IS working correctly
- Manual overrides exist due to exception handling in Win32Dispatcher.cs
- All three problematic functions (GetAcp, GetCpInfo, GetModuleFileNameA) HAVE [DllModuleExport] attributes
- **Root Cause:** Likely case sensitivity or DLL name mismatch between generator and dispatcher lookup

### 3. Interactive Debugger Investigation 🔄 NEXT STEP

**Recommended:** Set breakpoint at 0x004123B8 with full register/FLAGS inspection to identify exact failure point in C runtime code.

### 4. CPU FLAGS Testing ⚠️ NEEDS COVERAGE

**Testing Gaps Identified:**
- CMP/TEST instruction flag setting (ZF, CF, SF, OF, PF)
- Conditional jump behavior (JE, JNE, JG, JL, etc.)
- LOOP instruction handling
- Flag propagation through instruction sequences

Character comparison loops heavily depend on correct CPU flag handling.

### 5. GetStringTypeW Completeness 🔄 NEXT STEP

**Action Required:** Add missing character type flags to match GetStringTypeA:
- CT_CTYPE1_PUNCT (0x0010)
- CT_CTYPE1_CNTRL (0x0020)
- CT_CTYPE1_BLANK (0x0040)
- CT_CTYPE1_XDIGIT (0x0080)

See ARGBYTES_INVESTIGATION.md for complete analysis and detailed recommendations.
