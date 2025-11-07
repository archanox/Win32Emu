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

## Possible Causes

### 1. Self-Modifying Code or JIT Compilation
Some games, especially those with anti-debugging or copy protection, generate code on the stack and execute it. This is a legitimate technique but requires:
- Stack execution permissions (DEP/NX must be disabled)
- Proper code generation

### 2. Corrupted Return Address
A return address on the stack may have been overwritten, causing the CPU to jump into stack memory:
- Buffer overflow
- Stack corruption
- Incorrect pointer arithmetic

### 3. Unimplemented or Incorrectly Emulated Win32 API
The game may be calling a Win32 API function that:
- Returns an incorrect value
- Modifies memory incorrectly
- Doesn't set up the stack properly

### 4. Timing or Threading Issues
The game may be sensitive to timing:
- Race conditions between threads
- Events not being processed in time
- Message pump issues

## Recommendations for Debugging

### 1. Enable Enhanced Debugging
Run with `--debug` flag to catch issues earlier:
```bash
Win32Emu.Gui --nogui IGN_TEAS.EXE --debug
```

### 2. Enable File Logging
Capture full logs for analysis:
```bash
Win32Emu.Gui --nogui IGN_TEAS.EXE --debug --log-file
```

### 3. Use GDB Server for Detailed Inspection
Connect a debugger to examine the execution:
```bash
Win32Emu.Gui --nogui IGN_TEAS.EXE --gdb-server
```

Then connect with Ghidra or IDA Pro to:
- Set breakpoints before the crash
- Examine the stack contents
- Trace the execution flow
- Identify what called the stack address

### 4. Check Stack Execution Protection
The emulator may need to allow stack execution for this game. Look for:
- DEP/NX settings
- Memory page permissions
- Stack protection flags

### 5. Examine the Last Valid Code Address
Before jumping to the stack, the EIP was likely in the normal code section. Check logs around iteration 1,130,000 to see:
- What function was being executed
- What Win32 APIs were being called
- Any error or warning messages

## Next Steps

1. **Re-enable the "suspicious low memory" logging** - This was temporarily disabled but may provide valuable insights about when execution enters unexpected memory ranges.

2. **Add memory access logging** - Enable detailed logging of:
   - Memory writes to the stack
   - Indirect jumps and calls
   - Return address modifications

3. **Check for self-modifying code patterns** - Look for:
   - Writes to memory addresses that are later executed
   - VirtualProtect calls changing memory permissions
   - Code unpacking or decryption routines

4. **Compare with API Monitor logs** - If available, compare the emulator's API calls with those captured from API Monitor on a real Windows system to identify discrepancies.

## File Logging Feature
To make issue reporting easier, the new file logging feature has been implemented:
- Use `--log-file` to automatically generate a log file with MD5 hash of the executable
- Format: `<executable>_<md5hash>_<timestamp>.log`
- Example: `IGN_TEAS_42aeaf49af6191400fa18ba3e3c47e48_20251107_161715.log`

This makes it easy to identify logs for specific games when reporting issues.
