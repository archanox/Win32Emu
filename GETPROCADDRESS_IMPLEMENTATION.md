# GetProcAddress Implementation Summary

## Overview
This document describes the implementation of full PE export table parsing for the `GetProcAddress` Win32 API function in Win32Emu.

## Problem Statement
Previously, `GetProcAddress` was a stub that always returned 0. The infrastructure was in place, but the actual PE export table parsing was not implemented. This prevented dynamic function resolution from loaded DLLs.

## Implementation Details

### Components Modified

#### 1. LoadedImage Record (`Win32Emu/Loader/LoadedImage.cs`)
**Changes:**
- Added `ExportsByName` dictionary: Maps export names (case-insensitive) to virtual addresses
- Added `ExportsByOrdinal` dictionary: Maps export ordinals to virtual addresses

**Purpose:** Store parsed export information for quick lookup during `GetProcAddress` calls.

#### 2. PeImageLoader (`Win32Emu/Loader/PeImageLoader.cs`)
**Changes:**
- Added `using AsmResolver.PE.Exports` to access export parsing functionality
- Implemented `BuildExportMaps()` method to parse PE export directory
- Updated `Load()` method to call `BuildExportMaps()` and include results in LoadedImage

**Export Parsing Logic:**
```csharp
private (Dictionary<string, uint> byName, Dictionary<uint, uint> byOrdinal) BuildExportMaps(PEImage image, uint imageBase)
{
    // Parse image.Exports.Entries
    // For each export:
    //   - Skip forwarded exports (no RVA)
    //   - Calculate virtual address = imageBase + RVA
    //   - Add to byOrdinal dictionary
    //   - Add to byName dictionary if export has a name
}
```

#### 3. ProcessEnvironment (`Win32Emu/Win32/ProcessEnvironment.cs`)
**Changes:**
- Added `TryGetLoadedImage()` method to retrieve LoadedImage by module handle

**Purpose:** Allow GetProcAddress to access the export maps for a loaded PE image.

#### 4. Kernel32Module (`Win32Emu/Win32/Kernel32Module.cs`)
**Changes:**
- Completely rewrote `GetProcAddress()` implementation
- Added proper export resolution logic:
  - Retrieve LoadedImage from ProcessEnvironment
  - Distinguish between ordinal and name lookups
  - Query appropriate export dictionary
  - Return virtual address or 0 if not found
- Added proper error handling with specific error codes

**Export Lookup Flow:**
1. Parse `lpProcName` parameter (ordinal vs. string pointer)
2. Look up module handle to get LoadedImage
3. Query `ExportsByName` or `ExportsByOrdinal` dictionary
4. Return VA on success, 0 on failure with appropriate error code

#### 5. NativeTypes (`Win32Emu/Win32/NativeTypes.cs`)
**Changes:**
- Added `ERROR_INVALID_HANDLE = 6`
- Added `ERROR_PROC_NOT_FOUND = 127`

**Purpose:** Support proper Win32 error codes for GetProcAddress failures.

#### 6. Tests (`Win32Emu.Tests.Kernel32/NewFunctionsTests.cs`)
**Changes:**
- Updated existing test with accurate comments
- Added two new tests:
  - `GetProcAddress_WithNullModule_ShouldReturnZero`
  - `GetProcAddress_ByOrdinal_WithNonLoadedModule_ShouldReturnZero`

**Test Coverage:**
- Null module handle validation
- Non-loaded module handling (returns 0)
- Ordinal-based lookup validation

## Technical Approach

### AsmResolver Integration
The implementation leverages the AsmResolver library's PE parsing capabilities:
- `PEImage.Exports` provides access to export directory
- `ExportedSymbol` entries contain Name, Ordinal, and Address information
- Address.Rva provides the Relative Virtual Address for calculating absolute addresses

### Export Address Calculation
```
Virtual Address = Image Base Address + Export RVA
```

### Ordinal vs Name Lookup
Windows `GetProcAddress` supports two lookup modes:
1. **By Name**: `lpProcName` is a pointer to an ANSI string
2. **By Ordinal**: `lpProcName` has high word = 0, low word = ordinal

The implementation checks `(lpProcName & 0xFFFF0000) == 0` to distinguish between these modes.

### Forwarded Exports
Forwarded exports (exports that redirect to another DLL) are skipped during parsing since they have no RVA. These would require additional implementation to resolve the forwarding chain.

## Limitations and Future Work

### Current Limitations
1. **Forwarded Exports**: Not supported - these exports are skipped
2. **LoadLibraryA Integration**: GetModuleHandleA returns imageBase which may not be a LoadedImage
3. **Test Coverage**: Current tests only verify non-loaded module behavior

### Suggested Enhancements
1. Implement forwarded export resolution
2. Enhance LoadLibraryA to use PeImageLoader for DLL loading
3. Add integration tests with actual PE files containing exports
4. Support export name hints for optimization

## Usage Example

When a PE image is loaded via `ProcessEnvironment.LoadPeImage()`:
```csharp
// Load a DLL
var handle = processEnv.LoadPeImage("library.dll", peLoader);

// Later, resolve an export
var procNamePtr = WriteString("MyFunction");
var address = GetProcAddress(handle, procNamePtr);

// Or by ordinal
var address = GetProcAddress(handle, ordinal);
```

## Testing Results

All 117 Kernel32 tests pass, including:
- `GetProcAddress_WithNonLoadedModule_ShouldReturnZero`
- `GetProcAddress_WithNullModule_ShouldReturnZero`
- `GetProcAddress_ByOrdinal_WithNonLoadedModule_ShouldReturnZero`

## Impact on Issue #17

This implementation marks `GetProcAddress` as ✅ Implemented in the Issue #17 tracking document. It provides the foundation for dynamic function resolution, which is critical for proper Win32 DLL emulation.

## Files Modified

1. `ISSUE_17_IMPLEMENTATION.md` - Updated status
2. `Win32Emu.Tests.Kernel32/NewFunctionsTests.cs` - Enhanced tests
3. `Win32Emu/Loader/LoadedImage.cs` - Added export dictionaries
4. `Win32Emu/Loader/PeImageLoader.cs` - Added export parsing
5. `Win32Emu/Win32/Kernel32Module.cs` - Implemented GetProcAddress
6. `Win32Emu/Win32/NativeTypes.cs` - Added error codes
7. `Win32Emu/Win32/ProcessEnvironment.cs` - Added TryGetLoadedImage

**Total Changes:** 133 insertions, 17 deletions across 7 files
