# GDB Server Quick Reference

## Starting the Server

```bash
# Default port (1234)
Win32Emu.exe game.exe --gdb-server

# Custom port
Win32Emu.exe game.exe --gdb-server 5678
```

## Connecting from Ghidra

1. **Debugger** → **Connect to Target**
2. Select **gdb**
3. Enter `localhost:1234`
4. Click **Connect**

## Connecting from GDB

```bash
$ gdb
(gdb) target remote localhost:1234
(gdb) continue
```

## Connecting from IDA Pro

1. **Debugger** → **Select Debugger** → **Remote GDB Debugger**
2. **Debugger** → **Process Options**
   - Hostname: `localhost`
   - Port: `1234`
3. **Debugger** → **Start Process**

## Common Commands (in GDB)

| Command | Description |
|---------|-------------|
| `c` or `continue` | Continue execution |
| `s` or `step` | Single step |
| `info registers` | Show all registers |
| `x/16x 0x401000` | Examine 16 hex words at address |
| `break *0x401234` | Set breakpoint |
| `info breakpoints` | List breakpoints |
| `delete 1` | Delete breakpoint #1 |
| `quit` | Disconnect |

## Register Names

- General: `eax`, `ebx`, `ecx`, `edx`, `esi`, `edi`
- Pointers: `esp` (stack), `ebp` (frame), `eip` (instruction)
- Flags: `eflags`

## Memory Format

- Read: `m addr,length` (returns hex bytes)
- Addresses are in hexadecimal
- Little-endian byte order

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Connection refused | Ensure Win32Emu is running with `--gdb-server` |
| Wrong port | Check port number matches on both sides |
| Breakpoints not hit | Verify address is correct and code executes |
| Disconnects | Win32Emu may have exited - check terminal |

## See Also

- [GDB_SERVER_GUIDE.md](GDB_SERVER_GUIDE.md) - Complete guide with examples
- [INTERACTIVE_DEBUGGER_GUIDE.md](INTERACTIVE_DEBUGGER_GUIDE.md) - Built-in debugger
