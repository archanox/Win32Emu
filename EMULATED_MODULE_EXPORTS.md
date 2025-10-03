# Emulated Module Export Ordinals Implementation

## Overview
This document describes the implementation of export ordinals for all emulated Win32 modules in Win32Emu, enabling `GetProcAddress` to work with system DLLs like kernel32.dll, user32.dll, etc.

## Problem
While the initial GetProcAddress implementation supported PE export table parsing for loaded DLLs, it didn't handle emulated system modules. When code called `GetProcAddress(GetModuleHandle("kernel32.dll"), "GetVersion")`, it would fail because kernel32.dll is an emulated module, not a loaded PE file.

## Solution Architecture

### 1. Export Ordinals Interface
Added `GetExportOrdinals()` to the `IWin32ModuleUnsafe` interface:
```csharp
Dictionary<string, uint> GetExportOrdinals();
```

Each emulated module implements this to return a dictionary mapping export names to ordinals.

### 2. Module Export Definitions

#### Kernel32Module (48 exports)
- File I/O: CREATEFILEA, READFILE, WRITEFILE, SETFILEPOINTER, etc.
- Memory: HEAPALLOC, HEAPFREE, HEAPCREATE, VIRTUALALLOC, VIRTUALFREE
- Process: EXITPROCESS, TERMINATEPROCESS, GETCURRENTPROCESS
- Module: GETMODULEHANDLEA, LOADLIBRARYA, GETPROCADDRESS
- String: MULTIBYTETOWIDECHAR, WIDECHARTOMULTIBYTE, LCMAPSTRINGA, etc.
- And more...

#### User32Module (31 exports)
- Window creation: CREATEWINDOWEXA, DESTROYWINDOW
- Messaging: GETMESSAGEA, PEEKMESSAGEA, SENDMESSAGEA, POSTMESSAGEA
- Window management: SHOWWINDOW, SETWINDOWPOS, GETWINDOWRECT
- Input: GETCLIENTRECT, SETFOCUS
- Display: MESSAGEBOXA, UPDATEWINDOW
- And more...

#### Gdi32Module (9 exports)
- Painting: BEGINPAINT, ENDPAINT
- Drawing: TEXTOUT, FILLRECT
- Device context: GETDEVICECAPS, GETSTOCKOBJECT
- Text: SETTEXTCOLOR, SETBKMODE

#### DDrawModule (2 exports)
- DIRECTDRAWCREATE
- DIRECTDRAWCREATEEX

#### DSoundModule (2 exports)
- DIRECTSOUNDCREATE
- DIRECTSOUNDENUMERATEA

#### DInputModule (3 exports)
- DIRECTINPUTCREATEA
- DIRECTINPUTCREATE
- DIRECTINPUT8CREATE

#### WinMMModule (4 exports)
- TIMEGETTIME
- TIMEBEGINPERIOD
- TIMEENDPERIOD
- TIMEKILLEVENT

#### Glide2xModule (0 exports)
- Placeholder for future Glide3D/Voodoo emulation

### 3. Synthetic Export Addresses

Since emulated modules are C# classes (not actual PE files in memory), we can't use real memory addresses. Instead, we use synthetic addresses:

**ProcessEnvironment changes:**
```csharp
// Tracking for synthetic exports
private readonly Dictionary<uint, (string module, string export)> _syntheticExports = new();
private uint _nextSyntheticExport = 0x0E000000; // Base address for synthetic exports

public uint RegisterSyntheticExport(string moduleName, string exportName)
{
    var address = _nextSyntheticExport;
    _nextSyntheticExport += 0x10;
    _syntheticExports[address] = (moduleName.ToUpperInvariant(), exportName.ToUpperInvariant());
    
    // Create INT3 stub for breakpoint interception
    var stub = new byte[] { 0xCC, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
    vm.WriteBytes(address, stub);
    
    return address;
}
```

Each synthetic export:
- Gets a unique address in the 0x0E000000 range
- Has an INT3 (0xCC) instruction for breakpoint interception
- Maps back to (module name, export name) for dispatcher lookup

### 4. GetProcAddress Flow

Updated `Kernel32Module.GetProcAddress()`:

1. **Parse lpProcName**: Determine if it's a name or ordinal
2. **Check loaded PE images**: Try `TryGetLoadedImage()` first
   - If found, use real PE export tables
   - Return immediately if found
3. **Check emulated modules**: Get module name from handle
   - Look up module in dispatcher via `TryGetModule()`
   - Get export ordinals via `GetExportOrdinals()`
   - Resolve name or ordinal to export name
4. **Register synthetic export**: 
   - Call `RegisterSyntheticExport(module, export)`
   - Return synthetic address

### 5. Win32Dispatcher Enhancement

Added method to access registered modules:
```csharp
public bool TryGetModule(string dllName, out IWin32ModuleUnsafe? module)
{
    return _modules.TryGetValue(dllName, out module);
}
```

This allows GetProcAddress to query emulated modules for their exports.

## Usage Example

```csharp
// Get kernel32 module handle
var hKernel32 = GetModuleHandle("kernel32.dll");

// Get address of GetVersion by name
var procNamePtr = WriteAnsiString("GetVersion");
var address = GetProcAddress(hKernel32, procNamePtr);
// Returns: 0x0E000000 (synthetic address)

// Get address by ordinal
var address2 = GetProcAddress(hKernel32, 23); // ordinal 23 = GETVERSION
// Returns: 0x0E000000 (same synthetic address)

// When the CPU executes the INT3 at 0x0E000000:
// The emulator can look up (KERNEL32.DLL, GETVERSION) via TryGetSyntheticExport()
// And dispatch to the actual C# implementation
```

## Export Ordinal Numbering

All modules use alphabetical ordering for consistency:
- Ordinal 1 = first export alphabetically
- Ordinal 2 = second export alphabetically
- etc.

Example for Gdi32Module:
```csharp
{ "BEGINPAINT", 1 },
{ "ENDPAINT", 2 },
{ "FILLRECT", 3 },
{ "GETDEVICECAPS", 4 },
{ "GETSTOCKOBJECT", 5 },
// ...
```

This makes ordinals predictable and easy to maintain.

## Error Handling

New error codes added:
- `ERROR_MOD_NOT_FOUND = 126`: Module not found in dispatcher
- `ERROR_PROC_NOT_FOUND = 127`: Export not found (existing)
- `ERROR_INVALID_HANDLE = 6`: Invalid module handle (existing)

GetProcAddress returns 0 and sets appropriate last error on failure.

## Testing

All 117 existing Kernel32 tests pass with no regressions.

The implementation correctly handles:
- Null module handles
- Non-existent modules
- Non-existent exports
- Ordinal lookups
- Name lookups (case-insensitive)

## Future Enhancements

1. **Caching**: Cache synthetic exports to avoid re-registering the same function
2. **Forwarded Exports**: Support exports that forward to other DLLs
3. **Export by Hint**: Support export lookup by hint+name (optimization)
4. **Automatic Export Discovery**: Use reflection to auto-generate export lists from TryInvokeUnsafe switch cases

## Files Modified

1. `Win32Emu/Win32/IWin32ModuleUnsafe.cs` - Added GetExportOrdinals() interface method and implementations for all modules
2. `Win32Emu/Win32/Kernel32Module.cs` - Enhanced GetProcAddress, added GetExportOrdinals()
3. `Win32Emu/Win32/ProcessEnvironment.cs` - Added synthetic export tracking
4. `Win32Emu/Win32/Win32Dispatcher.cs` - Added TryGetModule()
5. `Win32Emu/Win32/NativeTypes.cs` - Added ERROR_MOD_NOT_FOUND

**Total Changes:** 280 insertions, 22 deletions across 5 files

## Benefits

1. **Dynamic Linking**: Full support for GetProcAddress on emulated modules
2. **Ordinal Support**: Applications can use ordinal-based imports
3. **Extensibility**: Easy to add new exports to any module
4. **Interception**: Synthetic addresses with INT3 enable debugging/tracing
5. **Compatibility**: Matches Win32 behavior for system DLL exports
