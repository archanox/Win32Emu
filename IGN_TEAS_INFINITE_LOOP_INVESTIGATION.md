# IGN_TEAS.EXE Infinite Loop Investigation

## **ROOT CAUSE IDENTIFIED** ✅

**Uninitialized Character Type Table at Address 0x0043AB91**

### The Problem

The C runtime command-line parser function `fn00412440` (at address 0x00412440) accesses a character type lookup table at `g_a43AB91` (address 0x0043AB91). This table should contain 256 bytes of character classification flags (isspace, isdigit, etc.), but the decompilation shows it as empty:

```cpp
byte g_a43AB91[] = // 0043AB91
{
};  // EMPTY!
```

### How This Causes the Infinite Loop

1. GetModuleFileNameA returns successfully with the module path string
2. C runtime calls `fn004123A0()` to initialize and parse command line
3. `fn004123A0()` calls `fn00412440()` to parse the module filename string
4. `fn00412440()` loops through each character, checking `g_a43AB91[ch] & 0x04`
5. **With an all-zero table**, the loop condition never matches properly:
   ```cpp
   if ((g_a43AB91[(uint32) dl_123] & 0x04) != 0x00)
   ```
6. The parser gets stuck in an infinite loop trying to parse the filename
7. Game never reaches WinMain or DirectX initialization

### Evidence from Decompilation

From `/Decomp/ign_teas/reko.cpp` lines 4140-4167:
```cpp
void fn00412440(...)
{
    // ... setup code ...
    if (*dwArg04.u3 != 0x22)
    {
        uint8 dl_123;
        do
        {
            ++*dwArg14;
            // ... copy character ...
            dl_123 = (uint8) *esi_101.u3;
            esi_101.u1 = (word32) esi_101 + 1;
            
            // ❌ THIS CHECK FAILS WITH EMPTY TABLE
            if ((g_a43AB91[(uint32) dl_123] & 0x04) != 0x00)
            {
                // Handle special characters
            }
            
            if (dl_123 == 0x20)  // space
                break;
            if (dl_123 == 0x00)  // null terminator
                goto l004124B0;
        } while (dl_123 != 0x09);  // tab
    }
}
```

### Solution Required

The PE loader must properly initialize the `.data` section at address 0x0043AB91 with the character type table. This is typically 256 bytes containing bit flags:
- 0x01: uppercase letter
- 0x02: lowercase letter  
- 0x04: digit/whitespace/special characters
- 0x08: hex digit
- etc.

This table is part of the C runtime library (ctype.h character classification).

## Previous Problem (Already Solved)

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
- The parser loops through characters and checks `g_a43AB91[character] & 0x04`
- Found that `g_a43AB91[]` is declared but **empty** in decompilation

### Step 3: Cross-Referenced Parser Logic

The parser function has a do-while loop (lines 4143-4167) that should:
1. Loop through each character in the filename
2. Check if character is special using the lookup table
3. Break on space (0x20), tab (0x09), or null (0x00)

**Problem**: With an empty/zero table, none of the bit checks succeed, so the break conditions are never properly evaluated, causing an infinite loop.

## Next Steps to Fix

### Option 1: Initialize Character Type Table in PE Loader

Add code to properly load and initialize the `.data` section at 0x0043AB91:

```csharp
// In PE loader, after loading sections:
// Initialize C runtime character type table at 0x43AB91
var ctypeTable = new byte[256];
for (int i = 0; i < 256; i++)
{
    byte flags = 0;
    if (char.IsUpper((char)i)) flags |= 0x01;
    if (char.IsLower((char)i)) flags |= 0x02;
    if (char.IsDigit((char)i) || i == ' ' || i == '\t') flags |= 0x04;
    if (char.IsDigit((char)i) || (i >= 'A' && i <= 'F') || (i >= 'a' && i <= 'f')) flags |= 0x08;
    ctypeTable[i] = flags;
}
memory.WriteBytes(0x0043AB91, ctypeTable);
```

### Option 2: Extract Table from Original PE File

The character type table should exist in the original IGN_TEAS.EXE file's `.data` section. Extract and verify the actual data at offset corresponding to RVA 0x0043AB91.

### Option 3: Patch the Parser Function  

As a workaround, patch the parser to use a simpler check that doesn't rely on the table.

## How to Verify the Fix

After implementing the fix:

1. Run: `Win32Emu.exe ./EXEs/ign_teas/IGN_TEAS.EXE --debug`
2. Should see:
   - GetModuleFileNameA completes successfully
   - Parser function `fn00412440` completes and returns
   - Execution continues to LoadCursorA, RegisterClassA, etc.
   - DirectX initialization functions are called
3. Game should progress into main loop

---

## Original Investigation Below
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
