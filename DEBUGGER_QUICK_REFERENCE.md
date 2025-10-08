# Interactive Debugger Quick Reference

## Common Debugging Scenarios

### Scenario 1: Investigate Why Emulator Stops Progressing

**Problem:** Game loads but stops without clear error messages.

**Solution:**
```bash
Win32Emu.exe game.exe --interactive-debug
```

```
(dbg) break 0x401000    # Entry point or main function
(dbg) continue
(dbg) step             # Step through instructions
(dbg) step
(dbg) info registers   # Check what's happening
(dbg) info stack       # See call stack
```

**What to look for:**
- Is EIP changing or stuck in a loop?
- Are registers being updated?
- Is the stack growing or stable?

---

### Scenario 2: Debug DirectX Initialization (Like IGN_TEAS.EXE)

**Based on decompilation analysis, set breakpoints at key DirectX functions:**

```
(dbg) break 0x403510    # Main initialization
(dbg) break 0x404640    # DirectDraw init
(dbg) break 0x4046F0    # DirectInput init
(dbg) continue
```

**When at DirectDrawCreate:**
```
(dbg) step
(dbg) step
(dbg) info registers
(dbg) examine 0x43CCDC 16    # Check where COM object pointer is written
```

**What to look for:**
- Is the returned value a handle (0x00700000) or a pointer?
- Can you read memory at that address?
- Does the structure have a vtable pointer at offset 0?

---

### Scenario 3: Examine Unknown Memory Access Errors

**Problem:** Emulator crashes with "memory access violation" but you don't know where.

**Solution:**
```
(dbg) break 0x403000    # Set breakpoint before suspected area
(dbg) continue
(dbg) step              # Step through slowly
(dbg) info registers    # Check EIP, EAX, EBX etc
(dbg) examine EAX 16    # Examine memory at register values
(dbg) examine EBX 16
```

**What to look for:**
- Which register contains the bad address?
- Is it reading or writing?
- What instruction is causing the fault?

---

### Scenario 4: Trace API Call Sequence

**Problem:** Need to see what Win32 APIs are being called and in what order.

**Solution:**
```bash
Win32Emu.exe game.exe --debug    # Use enhanced debug mode instead
```

Or in interactive mode:
```
(dbg) continue    # Let it run, watch console output for [Import] messages
```

**What you'll see:**
```
[Import] KERNEL32!GetVersion
[Import] Returned 0x00000004
[Import] KERNEL32!HeapCreate
[Import] Returned 0x00010000
...
```

---

### Scenario 5: Find Where a Global Variable Gets Modified

**Problem:** A global variable changes unexpectedly.

**Current limitation:** Watchpoints not yet implemented.

**Workaround:**
```
(dbg) examine 0x43CCDC 16    # Note current value
(dbg) step
(dbg) step
(dbg) step
(dbg) examine 0x43CCDC 16    # Check if changed
```

Repeat until you find the instruction that modifies it.

---

### Scenario 6: Understand Function Call Flow

**Problem:** Need to see what functions are being called.

**Solution:**
```
(dbg) break 0x401000    # Function entry
(dbg) continue
(dbg) info stack        # See return address
(dbg) step
(dbg) step              # Watch for CALL instructions in console
(dbg) break 0x402000    # Set breakpoint at called function
(dbg) continue
```

---

### Scenario 7: Quick Memory Dump at Specific Address

**Problem:** Need to see what's in memory at a specific location.

**Solution:**
```
(dbg) examine 0x43CCDC 64
```

Output shows hex and ASCII:
```
Memory at 0x0043CCDC:
0x0043CCDC: 00 00 70 00 00 00 00 00 00 00 00 00 00 00 00 00  | ..p.............
0x0043CCEC: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  | ................
...
```

---

### Scenario 8: Script Mode for Automated Debugging

**Problem:** Need to run same debugging sequence repeatedly.

**Solution:** Create a debug script file:

```bash
# debug_script.txt
break 0x403510
continue
info registers
info stack
step
step
step
examine 0x43CCDC 16
quit
```

Run it:
```bash
Win32Emu.exe game.exe --interactive-debug < debug_script.txt
```

---

## Command Cheat Sheet

| Command | Shortcut | Description |
|---------|----------|-------------|
| `continue` | `c` | Run until next breakpoint |
| `step` | `s`, `stepi` | Execute one instruction |
| `next` | `n` | Execute one instruction |
| `break <addr>` | `b` | Set breakpoint |
| `delete <id>` | `d` | Delete breakpoint |
| `disable <id>` | - | Disable breakpoint |
| `enable <id>` | - | Enable breakpoint |
| `info breakpoints` | `info b` | List breakpoints |
| `registers` | `r` | Show all registers |
| `info registers` | `info r` | Show all registers |
| `info stack` | `info s` | Show stack |
| `examine <addr> [n]` | `x` | Examine memory |
| `disassemble <addr>` | `disas` | Show instructions |
| `quit` | `q`, `exit` | Exit debugger |
| `help` | `h`, `?` | Show help |

---

## Address Formats

All commands accept multiple address formats:

- Hex with prefix: `0x401000`
- Hex without prefix: `401000`
- Decimal: `4198400`

---

## Tips

### Tip 1: Use Decompilation Outputs
Reference addresses from Ghidra, IDA, or Reko decompilation:
- Find interesting functions in decompiler
- Set breakpoints at those addresses
- Step through to understand behavior

### Tip 2: Start Broad, Then Narrow
1. Set breakpoint at main/WinMain
2. Identify which subsystem is failing (graphics, input, sound)
3. Set breakpoints at that subsystem's init functions
4. Step through to find exact failure

### Tip 3: Compare with Documentation
When debugging DirectX:
1. Check MSDN for expected behavior
2. Use debugger to see what emulator actually does
3. Compare and identify discrepancies

### Tip 4: Watch for Patterns
If you see:
- EIP stuck at same address → infinite loop
- ESP decreasing rapidly → possible stack overflow
- Many failed API calls → missing implementation

### Tip 5: Save Your Work
Document what you find:
```
(dbg) info registers > registers.txt
(dbg) examine 0x43CCDC 64 > memory.txt
```

---

## Common Addresses for IGN_TEAS.EXE

From decompilation analysis:

| Address | Function |
|---------|----------|
| 0x403140 | WinMain entry |
| 0x403510 | Main DirectX initialization |
| 0x404640 | DirectDraw initialization |
| 0x4046F0 | DirectInput initialization |
| 0x43CCDC | DirectDraw object pointer |
| 0x43CEB0 | DirectInput object pointer |

---

## Exit Codes

When debugger exits, check the reason:
- **User typed 'quit'** - Normal exit
- **Program exited** - Check if expected or premature
- **Breakpoint not hit** - Function might not be called
- **Script exhausted** - All script commands executed

---

## Limitations (Future Enhancements)

Current limitations to be aware of:
- No conditional breakpoints yet
- No watchpoints (break on memory change)
- No register modification during debug
- No step over (treat as step into)
- No proper disassembly (shows hex bytes)

See DEBUGGER_IMPLEMENTATION_SUMMARY.md for planned enhancements.

---

## Getting Help

- Full command reference: `INTERACTIVE_DEBUGGER_GUIDE.md`
- Practical example: `INTERACTIVE_DEBUGGER_EXAMPLE.md`
- Technical details: `DEBUGGER_IMPLEMENTATION_SUMMARY.md`
- IGN_TEAS analysis: `IGN_TEAS_DEBUG_REPORT.md`

In debugger: Type `help` for command list
