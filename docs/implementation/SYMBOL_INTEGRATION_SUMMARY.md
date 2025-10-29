# Symbol Integration Implementation Summary

## Overview

This PR implements **better symbol integration for GDB/Ghidra debugging** by parsing PE import/export tables and sending symbol information via the GDB Remote Serial Protocol's `qSymbol` packets.

## Problem Statement

Previously, when debugging Win32Emu with Ghidra or GDB, debuggers had no knowledge of:
- Which Windows API functions were being called (imports)
- What functions the executable exported
- Function names for better code navigation

This made debugging more difficult as users only saw raw addresses instead of meaningful function names.

## Solution

The implementation extracts symbols from PE files and announces them to debuggers:

1. **Parse Import Tables**: Extract all imported Windows API functions
2. **Parse Export Tables**: Extract all functions exported by the executable
3. **Announce to Debugger**: Proactively send symbols via `qSymbol` packets
4. **Respond to Lookups**: Answer symbol lookup requests from debuggers

## Technical Implementation

### GdbServer.cs Changes

**New Fields:**
```csharp
// Symbol tables for import/export information
private readonly Dictionary<string, uint> _symbols = new(StringComparer.OrdinalIgnoreCase);
private readonly List<string> _symbolsToAnnounce = new();
private int _symbolAnnounceIndex;
```

**New Methods:**

1. **AddSymbols()** - Add a dictionary of symbols to the server
   ```csharp
   public void AddSymbols(Dictionary<string, uint> symbols)
   ```

2. **AddSymbolsFromLoadedImage()** - Extract symbols from a PE image
   ```csharp
   public void AddSymbolsFromLoadedImage(Loader.LoadedImage image, string moduleName)
   ```
   - Extracts exports: `MODULE!FunctionName` → actual function address
   - Extracts imports: `DLL!FunctionName` → synthetic stub address (0x0F000000 range)

3. **HandleSymbolQueryAsync()** - Process qSymbol packets
   ```csharp
   private async Task HandleSymbolQueryAsync(string args)
   ```
   - Responds to symbol lookups: `qSymbol::<hexname>`
   - Proactively announces symbols: `qSymbol:<hexaddr>:<hexname>`
   - Returns "no more symbols": `qSymbol:OK`

4. **EncodeHexString()** - Helper to hex-encode symbol names
   ```csharp
   private static string EncodeHexString(string text)
   ```

### Emulator.cs Changes

Modified `RunWithGdbServer()` to add symbols after creating the GDB server:

```csharp
// Add symbols from the loaded image for better debugging experience
if (_image != null)
{
    var moduleName = Path.GetFileNameWithoutExtension(_image.FilePath).ToUpperInvariant();
    gdbServer.AddSymbolsFromLoadedImage(_image, moduleName);
}
```

### Test Coverage

Added 2 new unit tests in `GdbServerTests.cs`:

1. **GdbServer_AddSymbols_StoresSymbolsCorrectly** - Tests AddSymbols() method
2. **GdbServer_AddSymbolsFromLoadedImage_ProcessesExportsAndImports** - Tests symbol extraction

All 7 GDB server tests pass successfully.

## Symbol Format

Symbols are announced in the format: `MODULE!FunctionName`

**Examples:**
- `KERNEL32!GetVersion` - Imported function from KERNEL32.DLL
- `USER32!MessageBoxA` - Imported function from USER32.DLL
- `MYAPP!MyFunction` - Exported function from the main executable

## Benefits

### For Users
- ✅ Function names appear in disassembly instead of raw addresses
- ✅ Better call graphs showing import/export relationships
- ✅ Easier navigation in Ghidra/IDA Pro
- ✅ Improved understanding of which Windows APIs programs use
- ✅ Enhanced debugging experience without PDB files

### For Debuggers
- ✅ Ghidra can display symbols in the Symbol Tree window
- ✅ IDA Pro can use symbols for function naming
- ✅ GDB can use symbols for breakpoint setting by name
- ✅ Better integration with reverse engineering workflows

## GDB Remote Serial Protocol Details

The implementation follows the GDB Remote Serial Protocol specification for symbol handling:

### qSymbol Packet Format

**Request from GDB:**
- `qSymbol::` - Initial request (ready to receive symbols)
- `qSymbol::<hexname>` - Lookup specific symbol by name (hex-encoded)

**Response from Server:**
- `qSymbol:<hexaddr>:<hexname>` - Provide symbol address
- `qSymbol:OK` - No more symbols to announce

### Symbol Announcement Flow

1. GDB connects and sends `qSupported` with symbol support
2. GDB sends `qSymbol::` to indicate readiness
3. Server responds with first symbol: `qSymbol:<addr>:<name>`
4. GDB acknowledges and sends `qSymbol::` again
5. Server sends next symbol, repeating until all symbols announced
6. Server sends `qSymbol:OK` when done

## Documentation Updates

Updated `GDB_SERVER_GUIDE.md` with:

1. **New "Symbol Integration" section** explaining:
   - How it works
   - Symbol format
   - Benefits
   - How to view symbols in Ghidra
   - Technical details

2. **Updated "Supported GDB Commands"** to include:
   - `qSymbol` - Symbol lookup and announcement

3. **Moved to "Recent Enhancements"**:
   - Marked as ✅ IMPLEMENTED
   - Added to list of completed features

4. **Updated "Future Enhancements"**:
   - Marked "Better symbol integration via qSymbol" as complete

## Testing

### Build Status
✅ Build succeeded with no errors

### Test Results
✅ All 7 GDB server tests pass:
- GdbServer_CanBeCreated
- GdbServer_CanBeCreatedWithVfs
- GdbServer_ShouldBreak_ReturnsFalseWhenNoBreakpoint
- GdbServer_ShouldBreak_ReturnsTrueWhenBreakpointSet
- GdbServer_ShouldBreak_RecordsHitCount
- GdbServer_AddSymbols_StoresSymbolsCorrectly ⭐ NEW
- GdbServer_AddSymbolsFromLoadedImage_ProcessesExportsAndImports ⭐ NEW

## Files Changed

| File | Lines Changed | Description |
|------|---------------|-------------|
| Win32Emu/Debugging/GdbServer.cs | +118 | Symbol tracking and qSymbol handling |
| Win32Emu/Emulator.cs | +7 | Call AddSymbolsFromLoadedImage() |
| Win32Emu.Tests.Kernel32/GdbServerTests.cs | +77 | New symbol integration tests |
| GDB_SERVER_GUIDE.md | +57 | Documentation updates |
| **Total** | **+259** | **4 files modified** |

## Usage Example

### Starting Win32Emu with Symbol Support

```bash
# Start with GDB server (symbols are automatically loaded)
Win32Emu.exe your-program.exe --gdb-server

# Connect from Ghidra and see symbols in disassembly!
```

### Viewing Symbols in Ghidra

1. Connect to Win32Emu's GDB server (localhost:1234)
2. Open **Window** → **Debugger** → **Symbols**
3. See imported/exported functions listed:
   - `KERNEL32!GetVersion`
   - `USER32!MessageBoxA`
   - etc.

### Using Symbols in GDB

```bash
$ gdb
(gdb) target remote localhost:1234
(gdb) info symbols KERNEL32
# Shows all KERNEL32 symbols
(gdb) break KERNEL32!GetVersion
# Set breakpoint by symbol name
```

## Future Enhancements

Potential improvements:
- Add support for DLL symbols (currently only main executable)
- Support for forwarded exports
- Symbol versioning for multiple loaded modules
- Type information from exports

## References

- [GDB Remote Serial Protocol - qSymbol](https://sourceware.org/gdb/current/onlinedocs/gdb.html/General-Query-Packets.html#qSymbol)
- [PE Format Specification](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format)
- [Ghidra Debugger Documentation](https://ghidra.re/courses/debugger/)

## Conclusion

This implementation successfully addresses the issue "Better symbol integration (ghidra/gdb)" by:

✅ Parsing import/export tables from PE files
✅ Sending symbols via qSymbol packets to debuggers
✅ Providing meaningful function names in disassembly
✅ Enhancing the debugging experience in Ghidra, IDA, and GDB

The feature is fully implemented, tested, and documented.
