# GDB Server Integration Guide

## Overview

Win32Emu now includes a built-in GDB Remote Serial Protocol server that allows you to debug emulated programs using external debugging tools like:
- **Ghidra** - NSA's reverse engineering framework
- **IDA Pro** - Commercial disassembler and debugger
- **radare2** - Open-source reverse engineering framework
- **GDB** - GNU Debugger

This allows you to step through decompiled code line by line, set breakpoints, and inspect values using familiar reverse engineering tools.

## Quick Start

### 1. Start Win32Emu with GDB Server

```bash
# Start with default port (1234)
Win32Emu.exe your-program.exe --gdb-server

# Start with custom port
Win32Emu.exe your-program.exe --gdb-server 5678
```

You'll see output like:

```
GDB server listening on port 1234
Connect with: target remote localhost:1234
```

### 2. Connect from Ghidra

1. Open your executable in Ghidra and analyze it
2. Click **Debugger** → **Configure and Launch Emulator**
3. Select **gdb via SSH** or **local gdb**
4. Configure connection settings:
   - **Host**: `localhost`
   - **Port**: `1234` (or your custom port)
5. Click **Connect**

Alternatively, use the Debugger menu:
1. **Debugger** → **Connect to Target**
2. Choose **gdb**
3. Enter: `localhost:1234`
4. Click **Connect**

### 3. Start Debugging

Once connected:
- The program will be paused at the entry point
- Use Ghidra's debugger controls to:
  - **Step Into** (F7) - Execute one instruction
  - **Step Over** (F8) - Execute one instruction (no difference in current implementation)
  - **Resume** (F5) - Continue execution until next breakpoint
  - **Suspend** - Pause execution
- Set breakpoints by clicking on addresses in the disassembly view
- Inspect registers and memory in Ghidra's debugger windows

## Using with GDB Command Line

You can also connect directly using GDB:

```bash
$ gdb
(gdb) target remote localhost:1234
(gdb) info registers
(gdb) x/16x 0x401000
(gdb) break *0x401234
(gdb) continue
(gdb) step
```

## Using with IDA Pro

1. Open your executable in IDA Pro
2. **Debugger** → **Select Debugger** → **Remote GDB Debugger**
3. **Debugger** → **Process Options**
   - **Hostname**: `localhost`
   - **Port**: `1234`
4. **Debugger** → **Start Process**
5. Use IDA's debugging features as normal

## Supported GDB Commands

The Win32Emu GDB server implements the following GDB Remote Serial Protocol commands:

### Register Operations
- `g` - Read all general-purpose registers (EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI, EIP, EFLAGS)
- `p` - Read single register by index
- `G` - Write all registers (stub - returns OK)
- `P` - Write single register (stub - returns OK)

### Memory Operations
- `m addr,length` - Read memory bytes
- `M addr,length:XX...` - Write memory (stub - returns OK)

### Execution Control
- `c` - Continue execution
- `s` - Single step
- `vCont` - Extended continue/step commands

### Breakpoints
- `Z0,addr,kind` - Insert software breakpoint
- `z0,addr,kind` - Remove software breakpoint

### Queries
- `qSupported` - Feature negotiation
- `qAttached` - Attached to existing process
- `qOffsets` - Text/data/BSS offsets
- `qXfer:features:read` - Send target description (i386 architecture)

### Other
- `?` - Report halt reason
- `H` - Set thread for subsequent operations
- `k` - Kill process
- `D` - Detach from process

## Architecture Information

The GDB server advertises itself as an **i386** (32-bit x86) architecture, which matches the Win32 emulation environment.

### Register Mapping

| GDB Index | Register | Description |
|-----------|----------|-------------|
| 0 | EAX | Accumulator |
| 1 | ECX | Counter |
| 2 | EDX | Data |
| 3 | EBX | Base |
| 4 | ESP | Stack Pointer |
| 5 | EBP | Base Pointer |
| 6 | ESI | Source Index |
| 7 | EDI | Destination Index |
| 8 | EIP | Instruction Pointer |
| 9 | EFLAGS | Flags Register |

## Ghidra-Specific Features

### Viewing Decompiled Code

Ghidra's strength is its decompiler. When debugging with the GDB server:

1. Open the **Decompiler** window (Window → Decompiler)
2. As you step through code, the decompiler shows the corresponding C-like pseudocode
3. Variables and function calls are highlighted based on the current instruction
4. You can see how registers map to high-level variables

### Setting Breakpoints in Decompiled Code

1. In the Decompiler window, right-click on a line
2. Select **Toggle Breakpoint**
3. Ghidra will set a breakpoint at the corresponding assembly address
4. When execution hits that line, Win32Emu will pause

### Inspecting Memory Structures

1. Right-click on a pointer or address in the decompiler
2. Select **Go To** to see what it points to
3. Use **Data Type Manager** to apply structure definitions
4. Watch variables update as you step through code

## Example Debugging Session

### Investigating DirectX Initialization

Based on the decompilation findings, you can debug DirectX initialization:

```bash
# Terminal 1: Start Win32Emu with GDB server
$ Win32Emu.exe IGN_TEAS.EXE --gdb-server
GDB server listening on port 1234
```

In Ghidra:

1. Open `IGN_TEAS.EXE` and analyze
2. Navigate to address `0x00403510` (DirectDraw initialization)
3. Connect to GDB server: `localhost:1234`
4. Set breakpoint at `0x00403510`
5. Click **Resume** to continue to breakpoint
6. When breakpoint hits:
   - View decompiled code to understand the logic
   - Inspect EAX/ECX/EDX for DirectDraw object pointers
   - Examine memory to see COM vtables
   - Step through to see where it fails

### Analyzing Unknown Code Paths

```bash
# Terminal 1
$ Win32Emu.exe mystery-game.exe --gdb-server
```

In Ghidra:

1. Load and analyze the executable
2. Connect debugger
3. Let it run to see where it gets stuck
4. Pause execution (Debugger → Suspend)
5. Check current EIP location
6. Set breakpoints around that area
7. Restart and step through to understand the flow

## Troubleshooting

### Connection Refused

**Problem**: GDB client can't connect to Win32Emu

**Solution**:
- Ensure Win32Emu is running with `--gdb-server` flag
- Check that the port is correct (default: 1234)
- Verify no firewall is blocking localhost connections

### Breakpoints Not Hit

**Problem**: Breakpoints are set but never trigger

**Solution**:
- Verify the address is correct using Ghidra's analysis
- Ensure the code path is actually executed
- Check that execution hasn't already passed the breakpoint

### Registers Show Unexpected Values

**Problem**: Register values don't match expectations

**Solution**:
- Remember this is an emulator, not real hardware
- Some Win32 API calls are stubbed and return dummy values
- Use enhanced debug mode (`--debug`) to see more details

### Ghidra Shows "Target Disconnected"

**Problem**: Connection drops during debugging

**Solution**:
- Win32Emu may have crashed or exited
- Check the Win32Emu terminal for error messages
- Restart Win32Emu and reconnect

## Performance Considerations

- **GDB server mode is slow**: Each instruction requires network communication
- Use breakpoints strategically rather than single-stepping through everything
- For faster debugging of known issues, use the interactive debugger (`--interactive-debug`) instead

## Comparison with Interactive Debugger

| Feature | GDB Server | Interactive Debugger |
|---------|-----------|---------------------|
| UI | External tools (Ghidra/IDA) | Command line |
| Decompilation | Yes (in Ghidra/IDA) | No |
| Speed | Slower | Faster |
| Learning Curve | Steeper (need to know tools) | Easier |
| Automation | Limited | Script support |
| Best For | Understanding complex code | Quick debugging |

## Advanced Usage

### Using with radare2

```bash
$ r2 -d gdb://localhost:1234
[0x00401000]> dr    # show registers
[0x00401000]> px 64 @ eax  # examine memory
[0x00401000]> db 0x401234  # set breakpoint
[0x00401000]> dc    # continue
```

### Debugging with Python (pwntools)

```python
from pwn import *

# Connect to GDB server
r = remote('localhost', 1234)

# Send GDB commands
r.sendline(b'$g#67')  # Read registers
response = r.recvline()
print(f"Registers: {response}")
```

## Known Limitations

1. **Read-only debugging**: Register and memory writes are stubbed (return OK but don't modify state)
2. **No watchpoints**: Memory watchpoints are not implemented
3. **No conditional breakpoints**: Breakpoints always trigger when hit
4. **Single-threaded**: Only one thread is emulated
5. **No symbol support**: Debugging is by address only

## Future Enhancements

Potential improvements for the GDB server:

- [ ] Register/memory modification support
- [ ] Hardware watchpoints (break on memory access)
- [ ] Conditional breakpoints
- [ ] Multi-threading support
- [ ] Symbol file support
- [ ] Reverse debugging (step backwards)

## See Also

- [INTERACTIVE_DEBUGGER_GUIDE.md](INTERACTIVE_DEBUGGER_GUIDE.md) - Built-in command-line debugger
- [DEBUGGER_IMPLEMENTATION_SUMMARY.md](DEBUGGER_IMPLEMENTATION_SUMMARY.md) - Technical implementation details
- [DEBUGGING_GUIDE.md](DEBUGGING_GUIDE.md) - Enhanced debug mode
- [Ghidra Debugger Documentation](https://ghidra.re/courses/debugger/A1-GettingStarted.html)
- [GDB Remote Serial Protocol](https://sourceware.org/gdb/current/onlinedocs/gdb.html/Remote-Protocol.html)
