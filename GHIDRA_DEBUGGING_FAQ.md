# Ghidra Debugging FAQ

## "No debugging symbols found in .EXE" - What does this mean?

### The Quick Answer

**This warning is completely normal and expected.** You can ignore it and continue debugging. Ghidra's disassembler will work perfectly without debug symbols.

### Why You're Seeing This

When you connect Ghidra's debugger to Win32Emu using the GDB server, Ghidra (via GDB) tries to load debugging symbols from the executable file. The warning appears because:

1. **PE files don't have embedded debug symbols** - Unlike Linux executables (which use DWARF), Windows executables store debug information in separate `.pdb` files
2. **Old games don't ship with PDB files** - Developers typically don't distribute debug symbols with released games
3. **Win32Emu emulates the binary** - The emulator runs the executable code but doesn't have access to the original PDB files (which probably don't exist anyway)

### What This Means for Debugging

**The good news**: You lose almost nothing!

| What You CAN Do | What You CAN'T Do (but don't really need) |
|----------------|-------------------------------------------|
| ✅ Set breakpoints by address | ❌ See original variable names (like `playerHealth`) |
| ✅ Step through instructions | ❌ See original function names (like `DrawSprite`) |
| ✅ View registers and memory | ❌ See source file names and line numbers |
| ✅ Use Ghidra's decompiler | ❌ Have call stack with symbol names |
| ✅ See all import function calls | |
| ✅ Analyze the program structure | |

### How Ghidra Compensates

Ghidra doesn't actually need debug symbols because it performs **static analysis**:

1. **Auto-Analysis**: When you open the .EXE in Ghidra, it:
   - Identifies all functions automatically
   - Creates names like `FUN_00401000`, `FUN_00402550`, etc.
   - Finds strings and data references
   - Detects function parameters and return values

2. **Decompilation**: The decompiler shows C-like pseudocode:
   ```c
   void FUN_00401550(void) {
     int iVar1;
     iVar1 = GetVersion();
     if (iVar1 < 5) {
       MessageBoxA(0, "Windows XP required", "Error", 0x10);
       ExitProcess(1);
     }
     return;
   }
   ```

3. **You can rename things**: Double-click any function or variable in Ghidra to rename it:
   - `FUN_00401550` → `CheckWindowsVersion`
   - `DAT_00405060` → `g_DirectDrawObject`

4. **Win32Emu shows API names**: The emulator logs show actual function names:
   ```
   [Import] KERNEL32!GetVersion
   [Import] USER32!MessageBoxA
   [Import] KERNEL32!ExitProcess
   ```

### Step-by-Step: Effective Debugging Without Symbols

#### 1. Analyze in Ghidra First

```bash
# Before debugging, analyze the executable in Ghidra
1. Open Ghidra
2. Create a new project
3. Import your .EXE file
4. Run Auto-Analysis (Analysis → Auto Analyze)
5. Wait for analysis to complete
```

This creates a database of functions, strings, and structures.

#### 2. Start Win32Emu with GDB Server

```bash
$ Win32Emu your-game.exe --gdb-server
GDB server listening on port 1234
```

#### 3. Connect Ghidra's Debugger

```
1. In Ghidra: Debugger → Configure and Launch
2. Choose: "gdb via SSH" or "local gdb"  
3. Set host: localhost, port: 1234
4. Click Connect
5. You'll see: "(No debugging symbols found in ...)"
   → Ignore this and click OK
```

#### 4. Debug Normally

```
1. Set breakpoints by clicking addresses in the Listing window
2. Use Resume (F5) to run until breakpoint
3. Use Step Into (F7) to step through instructions
4. View the Decompiler window to see C-like code
5. Watch the Win32Emu terminal for API call logs
```

### Real-World Example: Debugging IGN_TEAS.EXE

Let's say you want to debug `IGN_TEAS.EXE`:

#### What You See in GDB:

```
GNU gdb (GDB) 16.3
...
(No debugging symbols found in /path/to/IGN_TEAS.EXE)
```

#### What You Do:

1. **Analyze in Ghidra first** - This finds 147 functions
2. **Connect debugger** - Ignore the warning
3. **Find DirectDraw initialization**:
   - Ghidra shows it at `0x00403510`
   - Function name: `FUN_00403510` (you can rename to `InitDirectX`)
4. **Set breakpoint**: Click on address `0x00403510`
5. **Resume**: Click F5
6. **When it breaks**:
   - Decompiler shows: `DirectDrawCreate(0, &lpDD, 0)`
   - Win32Emu logs: `[Import] DDRAW!DirectDrawCreate`
   - Registers window shows: `EAX = 0x00700000` (the DirectDraw object pointer)

#### What You Learn:

Even without symbols, you can see:
- Where DirectDraw initialization happens (address `0x00403510`)
- What parameters are passed (from decompiled code)
- What the function returns (EAX register)
- Which Win32 APIs are called (from logs)

This is **more than enough** to debug effectively!

### Comparing Symbol vs No-Symbol Debugging

#### With Debug Symbols (requires .pdb file):

```
Breakpoint 1, InitializeDirectX (hwnd=0x00010001, width=640, height=480) at main.cpp:127
127         hr = DirectDrawCreate(NULL, &g_pDD, NULL);
(gdb) print g_pDD
$1 = (LPDIRECTDRAW) 0x00700000
```

#### Without Debug Symbols (what you have):

```
Breakpoint 1, 0x00403510
(gdb) x/i $eip
=> 0x403510: push   0x0
```

In Ghidra's decompiler:
```c
void FUN_00403510(void) {
  DirectDrawCreate(0, &DAT_00405060, 0);
  ...
}
```

Win32Emu terminal:
```
[Import] DDRAW!DirectDrawCreate
```

**The difference**: Slightly harder to read, but you have ALL the information you need.

### Pro Tips

1. **Use Ghidra's "Set Label" feature**:
   - Right-click on `FUN_00403510` → Rename to `InitDirectX`
   - Now you have your own symbols!

2. **Watch Win32Emu's output**:
   - Run with `--debug` flag for even more detail
   - Logs show which APIs are called and their return values

3. **Use Ghidra's comments**:
   - Add comments in Ghidra at important addresses
   - These persist across debugging sessions

4. **Create your own symbol map**:
   - Document important addresses in a text file
   - Example:
     ```
     0x00403510 - InitDirectX
     0x00403650 - InitDirectInput  
     0x00403780 - MainGameLoop
     ```

5. **Combine multiple tools**:
   - Static analysis: Ghidra
   - Dynamic debugging: Win32Emu + Ghidra debugger
   - Execution logs: Win32Emu's console output

### When You WOULD Need Symbols

Debug symbols are really only critical when:
- Debugging your own code during development
- You have the source code and want to step through it line-by-line
- You need to see exact variable names from the original source

For reverse engineering or debugging closed-source binaries (like old games), **you never have symbols anyway**, so this is the normal way to work!

### Summary

| Question | Answer |
|----------|--------|
| Is the warning bad? | No, it's expected and harmless |
| Can I still debug? | Yes, fully! |
| What am I missing? | Original variable/function names from source code |
| Does Ghidra work? | Yes, perfectly! Ghidra creates its own analysis |
| Should I try to fix this? | No, just continue debugging |
| Any workarounds? | Use Ghidra's analysis and Win32Emu's logs |

### Need More Help?

- See [GDB_SERVER_GUIDE.md](GDB_SERVER_GUIDE.md) for general debugging guide
- See [INTERACTIVE_DEBUGGER_GUIDE.md](INTERACTIVE_DEBUGGER_GUIDE.md) for command-line debugging
- See [IGN_TEAS_DEBUG_REPORT.md](IGN_TEAS_DEBUG_REPORT.md) for a real debugging example
- Check GitHub issues for specific problems

## Other Common Questions

### "Why does Ghidra show 'Target Disconnected'?"

This means Win32Emu stopped or crashed. Check the Win32Emu terminal for error messages.

### "Can I modify registers or memory?"

Currently no - the GDB server supports read-only debugging. This is listed as a known limitation.

### "Why is debugging so slow?"

GDB server mode requires network communication for every instruction. Use breakpoints strategically instead of single-stepping through large sections of code.

### "Can I use IDA Pro instead of Ghidra?"

Yes! The same GDB server works with IDA Pro's remote GDB debugger. The same "no symbols" warning will appear, and you can ignore it the same way.

### "What about command-line GDB?"

You can use GDB directly:
```bash
$ gdb
(gdb) target remote localhost:1234
(No debugging symbols found in ...)  ← Ignore this
(gdb) info registers
(gdb) x/16x $eip
```

Works fine without symbols - you just use addresses instead of names.
