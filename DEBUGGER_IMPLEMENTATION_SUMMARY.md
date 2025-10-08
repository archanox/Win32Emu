# Interactive Step-Through Debugger Implementation Summary

## Overview

This PR implements a comprehensive interactive step-through debugger for Win32Emu, addressing the issue raised in the problem statement: "Can we integrate some sort of step through debugger? A gdb server to integrate with ghidra or Ida or reko? Or can we decompile straight in our application, where we can set breakpoints, inspect values and pause execution with stepping?"

## What Was Implemented

### Core Components

1. **BreakpointManager.cs** - Manages breakpoints with full CRUD operations
   - Add/remove breakpoints by address
   - Enable/disable breakpoints
   - Track hit counts
   - Query breakpoints

2. **InteractiveDebugger.cs** - Interactive debugger with GDB-like interface
   - Command-line interface for debugging
   - Execution control (step, continue, quit)
   - Memory examination
   - Register inspection
   - Stack viewing
   - Breakpoint management commands

3. **Integration with Emulator.cs**
   - New `RunWithInteractiveDebugger()` mode
   - Automatic break at entry point
   - Seamless integration with import handling and COM dispatch

4. **Program.cs Updates**
   - New `--interactive-debug` command-line flag
   - Updated help text

### Available Debugger Commands

#### Execution Control
- `continue` (c) - Continue until next breakpoint
- `step` (s, stepi) - Execute one instruction
- `next` (n) - Execute one instruction (same as step for now)
- `quit` (q, exit) - Exit debugger

#### Breakpoints
- `break <address>` (b, breakpoint) - Set breakpoint
- `delete <id>` (d) - Delete breakpoint
- `disable <id>` - Disable breakpoint
- `enable <id>` - Enable breakpoint
- `info breakpoints` - List all breakpoints

#### Inspection
- `registers` (r) - Show CPU registers
- `info registers` - Show CPU registers
- `info stack` - Show stack contents
- `examine <addr> [count]` (x) - Examine memory with hex/ASCII dump
- `disassemble <addr> [count]` (disas) - Show instruction bytes

#### Help
- `help` (h, ?) - Show command help

### Documentation

1. **INTERACTIVE_DEBUGGER_GUIDE.md** - Comprehensive user guide
   - Getting started
   - Complete command reference
   - Examples and tips
   - Differences from GDB

2. **INTERACTIVE_DEBUGGER_EXAMPLE.md** - Practical walkthrough
   - Step-by-step investigation of IGN_TEAS.EXE
   - How to use the debugger to confirm the COM vtable issue
   - Key addresses to watch
   - What to expect

3. **Updated README.md** - Added debugger to main documentation

### Tests

**InteractiveDebuggerTests.cs** - 13 comprehensive unit tests covering:
- Breakpoint creation and management
- Enable/disable functionality
- Hit count tracking
- Duplicate breakpoint handling
- Clear all functionality
- Breakpoint querying

All tests pass ✅

## How to Use

### Basic Usage

```bash
# Run with interactive debugger
dotnet run --project Win32Emu -- ./path/to/game.exe --interactive-debug

# Or using the built executable
Win32Emu.exe game.exe --interactive-debug
```

### Example Session

```
=== Interactive Debugger Mode ===
Type 'help' for available commands
The debugger will break at the entry point

Stopped at entry point
Stopped at 0x00401000

(dbg) break 0x403510
Breakpoint 1 set at 0x00403510

(dbg) continue
Continuing...
Breakpoint 1 hit at 0x00403510 (hit count: 1)

(dbg) info registers
Registers:
  EIP: 0x00403510
  EAX: 0x00000000  EBX: 0x00000000  ...

(dbg) step
Stepping one instruction...

(dbg) examine 0x43CCDC 16
Memory at 0x0043CCDC:
0x0043CCDC: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  | ....

(dbg) quit
Quitting debugger...
```

## Key Features

### 1. Breakpoint Support
- Set unlimited breakpoints at any address
- Enable/disable without deleting
- Track how many times each breakpoint is hit
- List all breakpoints with status

### 2. Step-Through Execution
- Single-step through instructions
- Full control over execution flow
- Break at any point

### 3. State Inspection
- View all CPU registers
- Examine memory with hex/ASCII dump
- View stack contents
- See current instruction bytes

### 4. Address Flexibility
Multiple address formats supported:
- Hexadecimal with 0x prefix: `0x401000`
- Hexadecimal without prefix: `401000`
- Decimal: `4198400`

### 5. Integration with Existing Features
- Works seamlessly with import handling
- Compatible with COM vtable dispatch
- Doesn't interfere with normal/debug modes

## Technical Details

### Architecture

```
User Input → InteractiveDebugger → BreakpointManager
                    ↓
         Emulator.RunWithInteractiveDebugger()
                    ↓
         CPU.SingleStep() + State Inspection
                    ↓
         Import/COM Handling (unchanged)
```

### Memory Examination

The `examine` command provides both hex and ASCII views:
```
0x00401000: 55 8B EC 83 EC 10 C7 45 FC 00 00 00 00 8B 45 08  | U...... E....E.
```

### Breakpoint Detection

On each instruction:
1. Check if current EIP matches any enabled breakpoint
2. If yes, record hit and enter interactive mode
3. Wait for user command
4. Resume execution based on command

## Addressing the Problem Statement

The problem statement asked:
> "Can we integrate some sort of step through debugger? A gdb server to integrate with ghidra or Ida or reko? Or can we decompile straight in our application, where we can set breakpoints, inspect values and pause execution with stepping?"

This implementation provides:

✅ **Step-through debugging** - Full single-step execution control
✅ **Breakpoints** - Set at any address, enable/disable, track hits
✅ **Inspect values** - Registers, memory, stack all inspectable
✅ **Pause execution** - Interactive prompt with full control
✅ **Integrated into application** - No external tools needed
✅ **Works with decompilation** - Use addresses from Ghidra/IDA/Reko output

### Why This Approach?

Instead of implementing a GDB server protocol (complex, requires external client), we:
- Built the debugger directly into the emulator
- Created a simple, familiar command interface
- Made it immediately usable without external tools
- Kept it focused on Win32 emulation needs

The debugger can be used with decompilation output from any tool - just use the addresses from the decompilation in your breakpoint commands.

## Use Cases

### 1. Investigating Emulator Hangs
Set breakpoints at key functions and step through to see where execution gets stuck.

### 2. Understanding DirectX Initialization
As shown in the example doc, you can set breakpoints at DirectDraw/DirectInput calls and examine the COM object structures.

### 3. Debugging API Call Issues
Break before/after important API calls and inspect parameters/return values.

### 4. Analyzing Control Flow
Step through the program to understand the execution path.

### 5. Memory Inspection
Examine memory regions to understand data structures and COM vtables.

## Future Enhancements

Potential additions (not in this PR):

- Conditional breakpoints (break if register == value)
- Watchpoints (break on memory access)
- Call stack unwinding
- Register/memory modification
- Symbol support (if debug info available)
- Scripting support (automate common debugging tasks)
- GDB remote protocol (for integration with external tools)

## Testing

- **13 unit tests** for BreakpointManager and basic debugger functionality
- All existing tests still pass
- Builds successfully on .NET 9

## Compatibility

- Works alongside existing `--debug` mode
- No performance impact when not enabled
- Compatible with GUI (GUI doesn't use interactive mode)
- No breaking changes to existing code

## Files Changed

### New Files
- `Win32Emu/Debugging/BreakpointManager.cs` (125 lines)
- `Win32Emu/Debugging/InteractiveDebugger.cs` (520 lines)
- `INTERACTIVE_DEBUGGER_GUIDE.md` (339 lines)
- `INTERACTIVE_DEBUGGER_EXAMPLE.md` (300 lines)
- `Win32Emu.Tests.Kernel32/InteractiveDebuggerTests.cs` (195 lines)

### Modified Files
- `Win32Emu/Emulator.cs` - Added RunWithInteractiveDebugger() method
- `Win32Emu/Program.cs` - Added --interactive-debug flag
- `Win32Emu.Gui/Services/EmulatorService.cs` - Updated LoadExecutable call
- `README.md` - Added debugger documentation

## Conclusion

This implementation provides a powerful, integrated debugging solution that directly addresses the problem statement. Users can now set breakpoints, step through execution, and inspect state without needing external tools or complex setups.

The debugger is particularly useful for investigating the IGN_TEAS.EXE issue mentioned in the problem statement - you can now set breakpoints at the DirectX initialization functions (0x403510, 0x404640, 0x4046F0) and step through to see exactly where and why the emulator stops progressing.

Combined with the existing decompilation analysis, this gives developers complete visibility into what's happening during emulation.
