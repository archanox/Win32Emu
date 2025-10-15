# GDB Server Integration Guide

## Overview

Win32Emu now includes a built-in GDB Remote Serial Protocol server that allows you to debug emulated programs using external debugging tools like:
- **Ghidra** - NSA's reverse engineering framework
- **IDA Pro** - Commercial disassembler and debugger
- **radare2** - Open-source reverse engineering framework
- **GDB** - GNU Debugger

This allows you to step through decompiled code line by line, set breakpoints, and inspect values using familiar reverse engineering tools.

## Quick Start

> **Note**: If you see "(No debugging symbols found in ...)" when connecting, that's normal! See [GHIDRA_DEBUGGING_FAQ.md](GHIDRA_DEBUGGING_FAQ.md) for details. Ghidra's decompiler works perfectly without debug symbols.

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
- `G` - Write all general-purpose registers
- `p` - Read single register by index
- `P` - Write single register by index

### Memory Operations
- `m addr,length` - Read memory bytes
- `M addr,length:XX...` - Write memory bytes

### Execution Control
- `c` - Continue execution
- `s` - Single step
- `vCont` - Extended continue/step commands

### Breakpoints
- `Z0,addr,kind` - Insert software breakpoint
- `z0,addr,kind` - Remove software breakpoint
- `Z1,addr,kind` - Insert hardware breakpoint (treated as software)
- `z1,addr,kind` - Remove hardware breakpoint
- `Z2,addr,kind` - Insert write watchpoint
- `z2,addr,kind` - Remove write watchpoint
- `Z3,addr,kind` - Insert read watchpoint
- `z3,addr,kind` - Remove read watchpoint
- `Z4,addr,kind` - Insert access watchpoint (read/write)
- `z4,addr,kind` - Remove access watchpoint

### Remote File I/O (when VFS is initialized)
- `vFile:open:filename,flags,mode` - Open a file in the virtual filesystem
- `vFile:close:fd` - Close a file descriptor
- `vFile:pread:fd,count,offset` - Read from file at offset
- `vFile:pwrite:fd,offset,data` - Write to file at offset
- `vFile:fstat:fd` - Get file status information
- `vFile:unlink:filename` - Delete a file
- `vFile:setfs:pid` - Set filesystem (no-op, always returns success)

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

## Remote File I/O

When the Virtual File System (VFS) is initialized, the GDB server supports remote file I/O operations. This allows Ghidra and other GDB clients to access files in the emulated environment's virtual filesystem.

### Enabling Remote File I/O

Remote file I/O is automatically enabled when you initialize the VFS before starting the GDB server:

```csharp
// In your emulator setup code
var processEnv = new ProcessEnvironment(virtualMemory);

// Initialize VFS with game directory
processEnv.InitializeVirtualFileSystem(
    baseDirectory: @"C:\Games\MyGame",
    overlayDirectory: @"C:\Users\YourName\AppData\Local\Win32Emu\MyGame"
);

// Now when you start the GDB server, it will have access to VFS
emulator.LoadExecutable("game.exe", gdbServerMode: true);
```

### Supported File Operations

The GDB server supports the following file I/O operations on the virtual filesystem:

- **Open files**: Ghidra can open files from the game directory
- **Read files**: Read file contents at any offset
- **Write files**: Write to files (writes go to overlay, preserving originals)
- **Get file info**: Query file size and attributes
- **Delete files**: Delete files from the overlay
- **Close files**: Clean up file descriptors

### Use Cases

Remote file I/O is useful for:

1. **Analyzing game data files**: Open and examine configuration files, saved games, etc.
2. **Debugging file operations**: Watch what files the game accesses
3. **Extracting assets**: Copy out resources without manual file copying
4. **Scripting analysis**: Use Ghidra scripts to process game files

### Example: Reading a Config File

```python
# Ghidra Python script to read game config
import gdb

# Open a config file from the virtual filesystem
fd = gdb.execute("call (int)open(\"config.ini\", 0)", to_string=True)

# Read the contents
buffer = gdb.execute("call (void*)malloc(1024)", to_string=True)
bytes_read = gdb.execute(f"call (int)read({fd}, {buffer}, 1024)", to_string=True)

# Process the file data...
```

### Security Considerations

- Only files within the VFS base directory are accessible
- Write operations are copy-on-write (original files are never modified)
- File deletions only affect the overlay directory
- Standard POSIX permission checks apply

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

### "No debugging symbols found in .EXE"

**Problem**: GDB or Ghidra shows a warning like:
```
(No debugging symbols found in /path/to/IGN_TEAS.EXE)
```

**Why This Happens**:
This message is **expected and harmless**. It occurs because:
- PE (Portable Executable) files don't contain embedded debug symbols
- Debug information for Windows executables is stored in separate PDB (Program Database) files
- Older games and applications typically don't ship with PDB files
- Win32Emu emulates the executable but doesn't have access to the original PDB files

**This Does NOT Prevent Debugging**:
You can still debug effectively without symbols because:
- ✅ **Ghidra's disassembler works perfectly** - It analyzes the binary and creates its own function names
- ✅ **You can set breakpoints by address** - Use addresses from Ghidra's analysis
- ✅ **Import functions are visible** - Win32Emu logs all API calls with their names (like `KERNEL32!CreateFileA`)
- ✅ **Memory and registers are accessible** - Full inspection capabilities
- ✅ **Ghidra's decompiler works** - Shows C-like pseudocode even without debug symbols

**What You Can Do**:

1. **Use Ghidra's Analysis** (Recommended):
   - Open the .EXE in Ghidra and run Auto-Analysis
   - Ghidra will identify functions, strings, and data structures
   - These appear in the debugger as you step through code
   - You can rename functions in Ghidra to make debugging easier

2. **Check Win32Emu's Logs**:
   - Win32Emu logs all Win32 API calls with their names
   - Example: `[Import] KERNEL32!GetVersion`
   - This helps you understand what the program is doing

3. **Use Enhanced Debug Mode**:
   ```bash
   Win32Emu game.exe --debug
   ```
   - Provides detailed execution logs
   - Shows register states and function calls
   - Helps identify where the program is in its execution flow

4. **Ignore the Warning**:
   - Simply continue debugging
   - The warning doesn't affect functionality
   - It's just informing you that symbolic debugging won't be available

**Understanding the Limitation**:
- **With symbols**: You could see variable names like `playerHealth` or `screenBuffer`
- **Without symbols**: You see addresses like `0x00405060` and `0x00406000`
- **Ghidra helps**: Its analysis creates names like `FUN_00405060` and identifies data structures
- **Win32Emu helps**: Import names show which Windows APIs are being called

**For the Curious**:
If you really want symbol information, you would need:
- The original PDB file from the developer (rarely available for old games)
- Source code to compile with debug symbols (usually not available)
- Or use Ghidra's analysis to create your own symbol annotations

### Protocol Errors or Memory Access Errors

**Problem**: You see errors like:
```
Protocol error: QStartNoAckMode (noack) conflicting enabled responses.
```
or
```
Python Exception <class 'gdb.MemoryError'>: Cannot access memory at address 0x...
```

**Solution**:
These issues have been fixed in the latest version:
- **QStartNoAckMode conflict**: The GDB server now properly handles the `QStartNoAckMode` command
- **Memory access errors**: Invalid memory reads are handled gracefully with better error messages

If you're still experiencing these errors:
- Make sure you're using the latest version of Win32Emu
- Check the Win32Emu console for warning messages about invalid memory access
- The memory errors are normal if the debugger tries to read beyond allocated memory
- Use Ghidra's memory map to understand which addresses are valid

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

1. ~~**Read-only debugging**: Register and memory writes are stubbed (return OK but don't modify state)~~ ✅ **IMPLEMENTED**
2. ~~**No watchpoints**: Memory watchpoints are not implemented~~ ✅ **IMPLEMENTED**
3. **No conditional breakpoints**: Breakpoints always trigger when hit
4. **Single-threaded**: Only one thread is emulated
5. **No PDB/DWARF symbols**: PE files don't have embedded debug symbols (this is normal - use Ghidra's analysis instead)
6. **Watchpoint checking**: Watchpoints are not automatically triggered during execution - they're registered but need manual checking in emulator loop

## Recent Enhancements

### ✅ Register and Memory Modification (Implemented)
You can now modify register and memory values during debugging:
- Set register values with `P` (single) or `G` (all registers)
- Write memory with `M addr,length:data`
- Full read/write debugging capability

### ✅ Hardware Watchpoints (Implemented)
Break on memory access at specific addresses:
- Write watchpoints: Break when memory is written
- Read watchpoints: Break when memory is read
- Access watchpoints: Break on any read or write
- Set with `Z2`, `Z3`, `Z4` commands in GDB/Ghidra

**Note**: Watchpoints are registered but require integration into the emulator's execution loop for automatic triggering.

## Future Enhancements

Potential improvements for the GDB server:

- [ ] Conditional breakpoints
- [ ] Multi-threading support
- [ ] Reverse debugging (step backwards)
- [ ] Automatic watchpoint triggering in emulator loop
- [ ] Tracepoints for non-intrusive data collection
- [ ] Better symbol integration via qSymbol responses

## See Also

- [GHIDRA_DEBUGGING_FAQ.md](GHIDRA_DEBUGGING_FAQ.md) - **START HERE** - Answers common questions about "No debugging symbols" and effective debugging without PDB files
- [INTERACTIVE_DEBUGGER_GUIDE.md](INTERACTIVE_DEBUGGER_GUIDE.md) - Built-in command-line debugger
- [DEBUGGER_IMPLEMENTATION_SUMMARY.md](DEBUGGER_IMPLEMENTATION_SUMMARY.md) - Technical implementation details
- [DEBUGGING_GUIDE.md](DEBUGGING_GUIDE.md) - Enhanced debug mode
- [Ghidra Debugger Documentation](https://ghidra.re/courses/debugger/A1-GettingStarted.html)
- [GDB Remote Serial Protocol](https://sourceware.org/gdb/current/onlinedocs/gdb.html/Remote-Protocol.html)
