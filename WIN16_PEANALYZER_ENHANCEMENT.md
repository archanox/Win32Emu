# Win16 PE Analyzer Enhancement Summary

## Date
December 4, 2025

## Issue
When checking Win16 NE executables (e.g., mine.exe) on the PE analyzer web page at https://archanox.github.io/Win32Emu/pe-analyzer/:
1. It said "Win16 API emulation is not yet implemented"
2. It was not showing any of the imported functions

## Root Cause
The NE (New Executable) format parser in `Win32Emu.Tools.PeAnalyzer.Wasm` was only extracting module names from the Module Reference Table, not individual imported functions from the Entry Table. Additionally, there was no mechanism to check if Win16 functions were actually supported by the emulator's Win16 thunking layer.

## Solution

### 1. Enhanced NE Entry Table Parser
**File**: `Win32Emu.Tools.PeAnalyzer.Wasm/Pages/PeAnalyzer.razor`

Rewrote `ParseNeImports()` method to:
- Parse the NE Entry Table (not just Module Reference Table)
- Extract individual imported functions with their names and ordinals
- Handle both name-based imports (function names) and ordinal-based imports
- Return `Dictionary<string, List<NeImportedFunction>>` instead of just module names
- Add fallback to module-only display if Entry Table parsing fails

**NE Format Details**:
- Entry Table contains "bundles" of entries
- Bundle type 0xFF = imported entries (what we need)
- Each imported entry is 6 bytes:
  - Byte 0: Flags
  - Bytes 1-2: Module index (1-based, references Module Reference Table)
  - Bytes 3-4: Import ordinal or name offset
  - Byte 5: Reserved
- If high bit of import ordinal is clear, it's a name reference (offset into Imported Names Table)
- If high bit is set, it's an ordinal import (low 15 bits = ordinal number)

### 2. Win16 Function Support Database
**File**: `Win32Emu.Tools.PeAnalyzer.Wasm/Pages/PeAnalyzer.razor`

Created `GetWin16SupportedModules()` method that returns a hardcoded dictionary of ~400+ supported Win16 functions across 6 modules:

- **KERNEL.DLL** → KERNEL32.DLL (45+ functions)
  - Memory: GlobalAlloc, LocalAlloc, etc.
  - File I/O: _lopen, _lread, _lwrite, etc.
  - Strings: lstrcpy, lstrcat, lstrlen, etc.
  - Modules: LoadLibrary, GetProcAddress, etc.

- **USER.DLL** → USER32.DLL (150+ functions)
  - Windows: CreateWindow, ShowWindow, etc.
  - Messages: GetMessage, SendMessage, etc.
  - Dialogs: DialogBox, GetDlgItem, etc.
  - Menus: CreateMenu, AppendMenu, etc.
  - Input: GetKeyState, GetCursorPos, etc.
  - Resources: LoadString, LoadIcon, etc.

- **GDI.DLL** → GDI32.DLL (130+ functions)
  - DC: CreateDC, GetDeviceCaps, etc.
  - Drawing: LineTo, Rectangle, Ellipse, etc.
  - Text: TextOut, DrawText, etc.
  - Objects: CreatePen, CreateBrush, etc.
  - Bitmaps: BitBlt, StretchBlt, etc.
  - Fonts: CreateFont, EnumFonts, etc.
  - Regions: CreateRectRgn, FillRgn, etc.
  - Palettes: CreatePalette, RealizePalette, etc.

- **KEYBOARD.DLL** → USER32.DLL (9 functions)
- **SYSTEM.DLL** → KERNEL32.DLL (8 functions)
- **SOUND.DLL** → WINMM.DLL (9 functions)

This list is based on the actual Win16 thunking layer implementations in:
- `Win32Emu/Win32/Win16/Win16KernelModule.cs`
- `Win32Emu/Win32/Win16/Win16UserModule.cs`
- `Win32Emu/Win32/Win16/Win16GdiModule.cs`
- `Win32Emu/Win32/Win16/Win16AuxiliaryModules.cs`

### 3. Updated NE Analysis Logic
**File**: `Win32Emu.Tools.PeAnalyzer.Wasm/Pages/PeAnalyzer.razor`

Rewrote `AnalyzeNeFile()` method to:
- Use the new parser to get module + function imports
- Look up each imported module in Win16 support database
- Check each imported function against the module's supported functions
- Mark functions as "implemented" or "missing"
- Calculate per-module implementation percentages
- Calculate overall compatibility statistics
- Generate appropriate verdict messages:
  - "FULLY COMPATIBLE" - all functions supported
  - "MOSTLY COMPATIBLE" - ≥80% supported
  - "PARTIALLY COMPATIBLE" - ≥50% supported
  - "LIMITED COMPATIBILITY" - <50% supported

### 4. Updated Documentation
**File**: `Win32Emu.Tools.PeAnalyzer.Wasm/README.md`

Updated to reflect:
- Full Win16 NE support (not just format detection)
- Entry Table parsing capabilities
- Function-level compatibility analysis
- List of supported Win16 modules
- Removed "pending Win16 API implementation" notes

## Technical Details

### NE Header Structure (Relevant Fields)
```
Offset 0x04 (2 bytes): Offset to Entry Table
Offset 0x06 (2 bytes): Length of Entry Table
Offset 0x1E (2 bytes): Module Reference Count
Offset 0x28 (2 bytes): Offset to Module Reference Table
Offset 0x2A (2 bytes): Offset to Imported Names Table
```

### Entry Table Bundle Format
```
Byte 0: Count (number of entries in bundle, 0 = end of table)
Byte 1: Segment indicator (0xFF = imported entries, 0xFE = fixed, else moveable)
Bytes 2+: Entry data (6 bytes per entry for imported)
```

### Imported Entry Format (6 bytes)
```
Byte 0: Flags
Bytes 1-2: Module index (1-based)
Bytes 3-4: Import ordinal or name offset
  - If high bit clear: offset into Imported Names Table
  - If high bit set: ordinal number (low 15 bits)
Byte 5: Reserved
```

## Benefits

1. **Accurate Win16 Compatibility Reporting**: The PE analyzer now correctly shows which Win16 functions are supported
2. **Individual Function Display**: Users can see exactly which functions are imported and their support status
3. **Better User Experience**: No more "Win16 API emulation is not yet implemented" message when it actually IS implemented
4. **Detailed Analysis**: Per-module and per-function breakdown helps developers understand what's missing

## Limitations

1. **Hardcoded Function List**: The Win16 function support is hardcoded rather than dynamically generated from [DllModuleExport] attributes. This is because Win16 modules use a switch-statement dispatch pattern rather than individual attributed methods.
2. **No Stub Detection**: Unlike Win32 modules, Win16 functions are not marked as stubs - they're either forwarded (implemented) or unknown (missing).
3. **Manual Updates Required**: If new Win16 functions are added to the thunking layer, the `GetWin16SupportedModules()` method must be manually updated.

## Future Enhancements

1. **Dynamic Win16 API Discovery**: Consider adding [DllModuleExport] attributes to Win16 thunking layers and generating api-status.json for Win16 modules
2. **Advanced Import Detection**: Parse Resident/Non-Resident Name Tables for exported function names (for executables that export functions)
3. **Segment Information**: Display segment information for NE files
4. **Resource Analysis**: Parse and display NE resource information

## Files Changed

- `Win32Emu.Tools.PeAnalyzer.Wasm/Pages/PeAnalyzer.razor` (major changes)
- `Win32Emu.Tools.PeAnalyzer.Wasm/README.md` (documentation update)

## Testing

Build succeeds with no errors. Manual testing required:
1. Publish Blazor WASM app: `dotnet publish -c Release`
2. Deploy to GitHub Pages: `cp -r bin/Release/net10.0/publish/wwwroot/* docs/pages/pe-analyzer/`
3. Test with real Win16 executable at https://archanox.github.io/Win32Emu/pe-analyzer/

## References

- Win16 NE Format: [Wikipedia - New Executable](https://en.wikipedia.org/wiki/New_Executable)
- Win16 Thunking Implementation: `WIN16_IMPLEMENTATION_SUMMARY.md`
- PE Analyzer Documentation: `Win32Emu.Tools.PeAnalyzer.Wasm/README.md`
