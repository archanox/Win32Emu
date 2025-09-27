# Enhanced CPU Debugging for Win32Emu

## Quick Start

The enhanced debugging is now **built directly into Program.cs**! No code changes needed.

To debug the `0xFFFFFFFD` memory access error, simply run your program with the `--debug` flag:

```bash
Win32Emu.exe your-program.exe --debug
```

## Usage

### Normal Mode (Default)
```bash
Win32Emu.exe your-program.exe
```
Runs exactly as before - no debugging overhead.

### Enhanced Debug Mode  
```bash
Win32Emu.exe your-program.exe --debug
```
Enables comprehensive debugging that will:
- **Catch the issue BEFORE it crashes**
- **Show exactly when EBP gets corrupted** 
- **Provide detailed analysis of the root cause**
- **Give execution history to trace the problem**

## What Enhanced Debug Mode Shows

When you run with `--debug`, you'll see output like:

```
[Debug] Enhanced debugging enabled - will catch 0xFFFFFFFD errors
[Debug] Monitoring for suspicious register values
[Debug] Use --debug argument to enable this mode

WARNING: EBP suspiciously small: 0x00000002 at EIP=0x0F000510

*** FOUND PROBLEMATIC EIP AT INSTRUCTION 1234 ***
EIP=0x0F000512 EBP=0x00000002 ESP=0x00200000
EAX=0x12345678 EBX=0x00000000 ECX=0x00000001 EDX=0x00000000
ESI=0x00000000 EDI=0x00000000
Instruction bytes: 0345FD
*** STOPPING BEFORE CRASH ***
ANALYSIS: EBP=0x00000002 is extremely small - any negative displacement will wrap around!
This will likely cause the 0xFFFFFFFD error.
[Debug] Stopping execution to prevent crash
```

## Enhanced Error Handling

If the crash still occurs, you'll get detailed diagnostics:

```
[Debug] *** CAUGHT MEMORY ACCESS VIOLATION AT INSTRUCTION 1234 ***
[Debug] Exception: Calculated memory address out of range: 0xFFFFFFFD (EIP=0x0F000512)
[Debug] Execution trace has 156 entries
[Debug] Found 3 suspicious register states

[Debug] First suspicious state occurred at:
[Debug]   EIP=0x0F000100 EBP=0x00000002 ESP=0x00200000

[Debug] Last 5 instructions:
[Debug]   0x0F00050E: 8945FC (EBP=0x00000002)
[Debug]   0x0F000511: 8B45F8 (EBP=0x00000002) 
[Debug]   0x0F000512: 0345FD (EBP=0x00000002)
[Debug]   0x0F000515: 8945F4 (EBP=0x00000002)
[Debug]   0x0F000518: C3 (EBP=0x00000002)

*** LIKELY CAUSE ANALYSIS ***
→ CAUSE: Frame pointer (EBP) corrupted to small value  
  SOLUTION: Check for buffer overflows or stack corruption in previous functions

To debug further:
1. Add breakpoint just before this EIP
2. Examine the call stack to see how EBP got corrupted  
3. Check the previous 10-20 instructions for buffer operations
4. Verify calling conventions match between caller/callee

[Debug] Final execution summary:
[Debug]   Total traced instructions: 156
[Debug]   Suspicious register states: 3
```

## Understanding the Analysis

The enhanced debugger identifies:

- **When**: Exact instruction number where EBP gets corrupted
- **What**: The instruction trying to access invalid memory  
- **Why**: Address calculation wraparound (small EBP + negative displacement)
- **Where**: Execution trace showing the sequence leading to the problem

## Features

### 🔍 **Suspicious Register Detection**
- Automatically detects when EBP or ESP become suspiciously small
- Warns before the crash occurs
- Configurable threshold (default: values ≤ 0x1000)

### 🎯 **Problematic EIP Detection**  
- Automatically stops at the exact problematic address `0x0F000512`
- Shows register state before the crash
- Prevents the crash from occurring

### 📊 **Execution Tracing**
- Records last 1000 instructions  
- Shows instruction bytes and register states
- Identifies patterns leading to corruption

### 🧠 **Root Cause Analysis**
- Automatic analysis of likely causes
- Specific suggestions for fixes
- Pattern detection (e.g., many recent CALLs = possible stack overflow)

## Common Issues Detected

The enhanced debugging will identify:

1. **Uninitialized EBP**: Function forgets `PUSH EBP; MOV EBP, ESP`
2. **Buffer Overflow**: Local array overflow corrupts saved EBP  
3. **Stack Corruption**: Wrong calling convention or return address corruption
4. **Missing Function Prologue**: Assembly code that skips frame setup

## No Performance Impact in Normal Mode

When you run without `--debug`, there's **zero** performance overhead - the program runs exactly as before. The debugging code is only active when explicitly requested.

## Integration Benefits

✅ **No code changes required** - just add `--debug` argument  
✅ **Zero performance impact** in normal mode  
✅ **Seamless switching** between normal and debug modes  
✅ **Full compatibility** with existing import handling  
✅ **Automatic crash prevention** and detailed analysis  

This integrated approach gives you powerful debugging capabilities exactly when you need them, without any changes to your existing workflow.