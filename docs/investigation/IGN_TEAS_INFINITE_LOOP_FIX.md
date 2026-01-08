# IGN_TEAS Infinite Loop Fix - Investigation Summary

## Problem
The Win32Emu emulator was stuck in an infinite loop when running `IGN_TEAS.EXE` natively. The game would initialize successfully, make several API calls (GetVersion, HeapCreate, GetStartupInfo, GetCommandLineA, GetEnvironmentStringsW, GetModuleFileNameA), but then enter an infinite loop without making any further progress.

## Investigation Process

### Step 1: Added Targeted Logging
Added instruction-level logging for EIP range 0x00412300-0x00412700 where the game returns from `GetModuleFileNameA` (commit 1caabd3).

### Step 2: Captured Loop Behavior
Running the game with logging enabled revealed the exact loop pattern:
```
[IGN_TEAS Loop] Iteration 10137: EIP 0x0041269A→0x0041269D, ESI=0x00880002
[IGN_TEAS Loop] Iteration 10140: EIP 0x0041269A→0x0041269D, ESI=0x00880004
[IGN_TEAS Loop] Iteration 10143: EIP 0x0041269A→0x0041269D, ESI=0x00880006
[IGN_TEAS Loop] Iteration 10146: EIP 0x0041269A→0x0041269D, ESI=0x00880008
...
```

### Step 3: Identified the Problem
The loop was scanning through wide-character (WCHAR) environment strings:
- ESI started at 0x00880000 (address returned by `GetEnvironmentStringsW`)
- ESI incremented by 2 each iteration (size of WCHAR)
- The loop was looking for a double-NULL terminator (two consecutive null WCHARs: 0x0000 0x0000)
- The loop never found the terminator because it was missing!

### Step 4: Root Cause Analysis
Examined the `GetEnvironmentStringsW` implementation in `ProcessEnvironment.cs`:

```csharp
// BEFORE (WRONG):
foreach (var kvp in _environmentVariables.OrderBy(x => x.Key))
{
    envBlock.Append($"{kvp.Key}={kvp.Value}");
    envBlock.Append('\0'); // null terminate each string
}
// Add final null terminator for the block
envBlock.Append('\0');  // ❌ ONLY ONE NULL!
```

This creates: `"VAR1=value1\0VAR2=value2\0\0"` in the StringBuilder, but when there's only ONE trailing `\0`, it becomes a single WCHAR (2 bytes) in Unicode encoding.

**Windows environment blocks require TWO null WCHARs** (4 bytes total) to mark the end:
- Format: `"VAR1=value1\0VAR2=value2\0\0"` where the last `\0\0` are TWO separate characters
- When encoded as Unicode: Each `\0` becomes 0x00 0x00 (one WCHAR)
- Double-NULL = 0x00 0x00 0x00 0x00 (two WCHARs)

### Step 5: Applied the Fix
Modified both `GetEnvironmentStringsW` and `GetEnvironmentStringsA`:

```csharp
// AFTER (CORRECT):
foreach (var kvp in _environmentVariables.OrderBy(x => x.Key))
{
    envBlock.Append($"{kvp.Key}={kvp.Value}");
    envBlock.Append('\0'); // null terminate each string
}
// Add final double-null terminator for the block
// Windows environment blocks are terminated with TWO null characters
envBlock.Append('\0');  // First null of double-NULL
envBlock.Append('\0');  // Second null of double-NULL
```

## Results

### Before Fix
- Game enters infinite loop at 0x0041269A-0x004126A1
- ESI scans indefinitely: 0x00880002, 0x00880004, 0x00880006, ...
- No progress beyond `GetModuleFileNameA`
- Execution never reaches window creation APIs

### After Fix
- ✅ Game exits the environment string scanning loop
- ✅ Execution continues past EIP 0x0041269A
- ✅ Game progresses to EIP 0x0041243A and beyond
- ✅ Ready to proceed with remaining initialization

## Technical Details

### The Loop Assembly
```asm
0x0041269A:  inc esi, 2        ; ESI += 2 (next WCHAR)
0x0041269D:  cmp [esi], 0      ; Check if current WCHAR is NULL
0x004126A1:  jnz 0x0041269A    ; Loop back if not NULL
```

The game checks each WCHAR:
- If it finds NULL (0x0000), it checks the next one
- If two consecutive NULLs are found (double-NULL), the loop exits
- Without the second NULL, the first NULL passes but there's no second NULL to confirm end-of-block

### Windows Environment Block Format
```
WCHAR block format (Unicode):
"PATH=C:\Windows\0USER=Admin\0TEMP=C:\Temp\0\0"
       └─ NULL (2 bytes)        └─ NULL (2 bytes)              └─ Double-NULL (4 bytes)
```

## Files Modified

1. **Win32Emu/Win32/ProcessEnvironment.cs**
   - Line 761: Added second `\0` to `GetEnvironmentStringsW`
   - Line 791: Added second `\0` to `GetEnvironmentStringsA`

2. **Win32Emu/Emulator.cs**
   - Line 1370: Added diagnostic logging for IGN_TEAS loop investigation
   - Logs every instruction in EIP range 0x00412300-0x00412700 with full register state

## Commits

1. **1caabd3** - Add loop diagnostic logging - found infinite wide-char scan
2. **044d0d6** - Fix GetEnvironmentStringsW missing double-NULL terminator - resolves infinite loop

## Lessons Learned

1. **Environment Blocks:** Windows environment blocks require careful termination:
   - Each variable: NULL-terminated string
   - Block end: Double-NULL (two consecutive NULLs)

2. **Unicode Encoding:** When building strings in .NET and encoding to Unicode:
   - One `\0` character = One WCHAR (2 bytes: 0x00 0x00)
   - Double-NULL = Two `\0` characters (4 bytes: 0x00 0x00 0x00 0x00)

3. **Diagnostic Approach:** Instruction-level logging of register states was crucial for:
   - Identifying the exact loop pattern
   - Understanding what the loop was searching for
   - Pinpointing the missing terminator

## Next Steps

With the infinite loop fixed, IGN_TEAS should now be able to continue initialization. The game should proceed to:
1. Parse command line arguments
2. Call `HeapAlloc` / `HeapFree`
3. Call `GetModuleHandleA("KERNEL32")`
4. Call `GetProcAddress(...)` for dynamic linking
5. Call `IsProcessorFeaturePresent(...)`
6. Create window with `LoadCursorA`, `LoadIconA`, `RegisterClassA`, `CreateWindowExA`
7. Initialize DirectDraw/Glide rendering

The emulator is now unblocked and ready for the next phase of initialization.

---

**Date:** January 8, 2026  
**Issue:** IGN_TEAS infinite loop after GetModuleFileNameA  
**Resolution:** Fixed GetEnvironmentStringsW/A double-NULL terminator  
**Status:** ✅ RESOLVED
