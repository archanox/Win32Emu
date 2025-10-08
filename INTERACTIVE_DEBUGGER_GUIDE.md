# Interactive Debugger Guide

## Overview

Win32Emu now includes a built-in interactive step-through debugger similar to GDB. This allows you to:
- Set breakpoints at specific addresses
- Step through instructions one at a time
- Inspect CPU registers and memory
- Examine the call stack
- Pause and resume execution

This debugger is perfect for understanding why your emulator stops outputting logs or appears to get stuck.

## Getting Started

### Starting the Interactive Debugger

Run your executable with the `--interactive-debug` flag:

```bash
Win32Emu.exe your-program.exe --interactive-debug
```

The emulator will stop at the entry point and present you with a debugger prompt:

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
Next instruction at 0x00401000: 55

(dbg) 
```

## Available Commands

### Execution Control

#### `continue` (or `c`)
Continue execution until the next breakpoint is hit.

```
(dbg) continue
Continuing...
```

#### `step` (or `s`, `stepi`)
Execute a single instruction and break again.

```
(dbg) step
Stepping one instruction...
```

#### `next` (or `n`)
Execute a single instruction. Currently same as `step`.

```
(dbg) next
Stepping one instruction...
```

### Breakpoints

#### `break <address>` (or `b`, `breakpoint`)
Set a breakpoint at the specified address.

```
(dbg) break 0x401234
Breakpoint 1 set at 0x00401234
```

Addresses can be specified in multiple formats:
- With 0x prefix: `break 0x401234`
- Without prefix (hex): `break 401234`
- Decimal: `break 4198964`

#### `delete <breakpoint-id>` (or `d`)
Delete a breakpoint by its ID.

```
(dbg) delete 1
Breakpoint 1 deleted
```

#### `disable <breakpoint-id>`
Temporarily disable a breakpoint without deleting it.

```
(dbg) disable 1
Breakpoint 1 disabled
```

#### `enable <breakpoint-id>`
Re-enable a disabled breakpoint.

```
(dbg) enable 1
Breakpoint 1 enabled
```

#### `info breakpoints` (or `info b`)
List all breakpoints.

```
(dbg) info breakpoints
Breakpoints:
  1: 0x00401234 (enabled, hit 0 time(s))
  2: 0x00401500 (disabled, hit 3 time(s))
```

### Inspection

#### `registers` (or `r`)
Display all CPU registers.

```
(dbg) registers
Registers:
  EIP: 0x00401234
  EAX: 0x12345678  EBX: 0x00000001  ECX: 0x00000000  EDX: 0xFFFFFFFF
  ESI: 0x00400000  EDI: 0x00000000  EBP: 0x0019FF88  ESP: 0x0019FF80
  EFLAGS: 0x00000246
```

#### `info registers` (or `info r`)
Same as `registers` command.

#### `info stack` (or `info s`)
Display the top of the stack.

```
(dbg) info stack
Stack (ESP=0x0019FF80):
  0x0019FF80: 0x00401000
  0x0019FF84: 0x00000000
  0x0019FF88: 0x00000001
  0x0019FF8C: 0x7FFFFFFF
  ...
```

#### `examine <address> [count]` (or `x`)
Examine memory at the specified address. Optionally specify the number of bytes to display (default: 16).

```
(dbg) examine 0x401000 32
Memory at 0x00401000:
0x00401000: 55 8B EC 83 EC 10 C7 45 FC 00 00 00 00 8B 45 08  | U...... E....E.
0x00401010: 89 45 F8 8B 45 0C 89 45 F4 8B 45 F8 03 45 F4 89  | .E..E..E..E..E.
```

The display shows:
- Hex offset
- Hex bytes
- ASCII representation (`.` for non-printable characters)

#### `disassemble <address> [count]` (or `disas`)
Show a simple disassembly at the specified address. If no address is given, uses the current EIP.

```
(dbg) disassemble 0x401000 5
Disassembly at 0x00401000:
0x00401000: 558BEC83EC10C745FC000000
0x00401003: EC83EC10C745FC0000008B45
0x00401006: EC10C745FC0000008B450889
...
```

*Note: This is a simple hex dump of instructions. For proper disassembly, you may want to use external tools like Ghidra or IDA.*

### Other Commands

#### `quit` (or `q`, `exit`)
Exit the debugger and stop emulation.

```
(dbg) quit
Quitting debugger...

Interactive debugger session ended
```

#### `help` (or `h`, `?`)
Display help for available commands.

```
(dbg) help

Available commands:
  continue (c)           - Continue execution
  step (s, stepi)        - Execute one instruction
  ...
```

## Example Debugging Session

Here's a typical debugging session to investigate why an emulator stops progressing:

```bash
$ Win32Emu.exe IGN_TEAS.EXE --interactive-debug
```

```
=== Interactive Debugger Mode ===
...
Stopped at entry point
Stopped at 0x00401000

(dbg) info registers
Registers:
  EIP: 0x00401000
  ...

(dbg) break 0x403510
Breakpoint 1 set at 0x00403510

(dbg) continue
Continuing...
Breakpoint 1 hit at 0x00403510 (hit count: 1)
Stopped at 0x00403510

(dbg) step
Stepping one instruction...
Stopped at 0x00403512

(dbg) registers
Registers:
  EIP: 0x00403512
  EAX: 0x00000000  ...

(dbg) examine 0x43CCDC 4
Memory at 0x0043CCDC:
0x0043CCDC: 00 00 00 00  | ....

(dbg) info stack
Stack (ESP=0x0019FF80):
  0x0019FF80: 0x00403520
  0x0019FF84: 0x00000000
  ...

(dbg) continue
Continuing...
```

## Tips and Best Practices

### Finding the Right Breakpoint

1. **Use decompilation analysis**: Refer to the decompilation files in `Decomp/ign_teas/` to identify key addresses
2. **Start broad, then narrow**: Set breakpoints at major initialization functions first
3. **Watch for imports**: Pay attention to DirectX, User32, and Kernel32 API calls

### Investigating Hangs

If your emulator appears to hang:

1. Run with `--interactive-debug`
2. Let it run for a few seconds, then press Ctrl+C (if supported) or set a timer
3. Use `step` repeatedly to see if you're in a loop
4. Examine registers to see if they're changing
5. Check the stack to see the call depth

### Understanding DirectX Issues

Based on the decompilation findings, DirectX initialization is a common issue:

```
(dbg) break 0x404640    # DirectDraw initialization
(dbg) break 0x4046F0    # DirectInput initialization
(dbg) continue
```

When you hit these breakpoints:
1. Step through the COM object creation
2. Examine memory where COM vtables should be
3. Check if function pointers are valid

## Differences from GDB

While inspired by GDB, this debugger is tailored for Win32 emulation:

- **Simplified**: Focuses on the most common debugging tasks
- **Integrated**: Works directly with the emulator's internal state
- **Win32-aware**: Understands import tables and COM objects
- **No symbols**: Works with raw addresses (use decompilation for context)

## Advanced Usage

### Debugging COM Vtable Issues

From the decompilation analysis, we know COM vtables are problematic:

```
# Break when DirectDrawCreate returns
(dbg) break 0x404645

# When hit, examine the returned object pointer
(dbg) examine EAX 16    # (Note: address parsing from registers not yet supported)

# Set breakpoint at first vtable call
(dbg) break 0x404650

# Continue and inspect when vtable method is called
(dbg) continue
(dbg) info stack
```

### Automated Breakpoint Scripts

You can prepare a list of breakpoints based on decompilation analysis:

```
break 0x403510    # Main initialization
break 0x404640    # DirectDraw init
break 0x4046F0    # DirectInput init
break 0x404B00    # Setup function
continue
```

## Troubleshooting

### "Cannot read memory at 0xXXXXXXXX"
The address is outside allocated memory. This is expected for unmapped regions.

### Breakpoint Not Hitting
1. Verify the address is correct (check decompilation)
2. Ensure the breakpoint is enabled (`info breakpoints`)
3. The code path may not be executed

### Debugger Exits Immediately
Check if the program is exiting normally. Use `step` from the entry point to trace execution.

## Future Enhancements

Potential improvements for the debugger:

- [ ] Conditional breakpoints
- [ ] Watchpoints (break on memory access)
- [ ] Symbol support (if debug info available)
- [ ] Register modification
- [ ] Memory modification
- [ ] Call stack unwinding
- [ ] Integration with decompilation databases
- [ ] Remote debugging (GDB server protocol)

## See Also

- [DEBUGGING_GUIDE.md](DEBUGGING_GUIDE.md) - Enhanced debugging mode
- [DECOMPILATION_FINDINGS.md](DECOMPILATION_FINDINGS.md) - Analysis of IGN_TEAS.EXE
- [Decomp/ign_teas/INDEX.md](Decomp/ign_teas/INDEX.md) - Decompilation index
