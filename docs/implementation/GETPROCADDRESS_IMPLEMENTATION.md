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
- Added `ForwardedExportsByName` dictionary: Maps forwarded export names to forwarder strings
- Added `ForwardedExportsByOrdinal` dictionary: Maps forwarded export ordinals to forwarder strings

**Purpose:** Store parsed export information for quick lookup during `GetProcAddress` calls, including forwarded exports.

#### 2. PeImageLoader (`Win32Emu/Loader/PeImageLoader.cs`)
**Changes:**
- Added `using AsmResolver.PE.Exports` to access export parsing functionality
- Implemented `BuildExportMaps()` method to parse PE export directory
- Updated `Load()` method to call `BuildExportMaps()` and include results in LoadedImage

**Export Parsing Logic:**
```csharp
private (Dictionary<string, uint> byName, Dictionary<uint, uint> byOrdinal, 
         Dictionary<string, string> forwardedByName, Dictionary<uint, string> forwardedByOrdinal) 
    BuildExportMaps(PEImage image, uint imageBase)
{
    // Parse image.Exports.Entries
    // For each export:
    //   - If forwarded: Store in forwarded dictionaries
    //   - Else: Calculate virtual address = imageBase + RVA
    //           Add to byOrdinal and byName dictionaries
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
  - Resolve forwarded exports recursively
  - Return virtual address or 0 if not found
- Added `ResolveForwardedExport()` method to handle export forwarding
- Added proper error handling with specific error codes

**Export Lookup Flow:**
1. Parse `lpProcName` parameter (ordinal vs. string pointer)
2. Look up module handle to get LoadedImage
3. Query `ExportsByName` or `ExportsByOrdinal` dictionary
4. If not found, check `ForwardedExportsByName` or `ForwardedExportsByOrdinal`
5. If forwarded, recursively resolve via `ResolveForwardedExport()`
6. Return VA on success, 0 on failure with appropriate error code

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
Forwarded exports (exports that redirect to another DLL) are fully supported. The implementation:
- Detects forwarded exports during PE parsing using `export.IsForwarder`
- Stores forwarder strings (e.g., "NTDLL.RtlAllocateHeap") in separate dictionaries
- Resolves forwarding chains at runtime via `ResolveForwardedExport()` method
- Supports both "DLL.Export" and "DLL.DLL.Export" forwarder formats
- Recursively loads target DLLs and resolves final export addresses

**Forwarder Resolution:**
```csharp
// Example: kernel32.HeapAlloc -> NTDLL.RtlAllocateHeap
// 1. Parse forwarder string to extract target DLL and export name
// 2. Load target DLL via LoadModule()
// 3. Recursively call GetProcAddress() on target DLL
// 4. Return final resolved address
```

## Current Status

### Completed Features
1. ✅ **Forwarded Exports**: Fully implemented with recursive resolution
2. ✅ **LoadLibraryA Integration**: Uses PeImageLoader for local DLLs
3. ✅ **Export Lookup Optimization**: Dictionary-based O(1) lookups (better than PE hints)
4. ✅ **Integration Tests**: Comprehensive tests with real PE files (8 tests in PeExportIntegrationTests.cs)
5. ✅ **Case-Insensitive Lookups**: Export names matched case-insensitively
6. ✅ **Ordinal Support**: Full support for ordinal-based export lookups
7. ✅ **Error Handling**: Proper Win32 error codes (ERROR_PROC_NOT_FOUND, ERROR_INVALID_HANDLE)

### Performance Optimizations
The implementation uses `Dictionary<string, uint>` for export lookups, providing O(1) average-case performance. This is superior to the PE format's hint-based approach, which is designed for linear searches through the export name table. No additional hint optimization is needed.

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

All Kernel32 tests pass, including:

**Basic Tests:**
- `GetProcAddress_WithNonLoadedModule_ShouldReturnZero`
- `GetProcAddress_WithNullModule_ShouldReturnZero`
- `GetProcAddress_ByOrdinal_WithNonLoadedModule_ShouldReturnZero`

**Integration Tests (PeExportIntegrationTests.cs):**
- `LoadLibraryA_WithRealPeDll_ShouldLoadSuccessfully`
- `GetProcAddress_WithRealPeDll_ByName_ShouldResolveExport`
- `GetProcAddress_WithRealPeDll_ByOrdinal_ShouldResolveExport`
- `GetProcAddress_WithRealPeDll_NonExistentExport_ShouldReturnZero`
- `LoadLibraryA_AndGetProcAddress_EndToEnd_ShouldWork`
- `GetProcAddress_WithForwardedExport_ShouldResolveCorrectly`
- `PeImageLoader_ShouldParseExportTable_WithAllExportTypes`
- `GetProcAddress_CaseInsensitiveNameLookup_ShouldWork`

## Impact on Issue #17

This implementation marks `GetProcAddress` as ✅ Implemented in the Issue #17 tracking document. It provides the foundation for dynamic function resolution, which is critical for proper Win32 DLL emulation.

## Files Modified

### Initial Implementation
1. [docs/archive/ISSUE_17_IMPLEMENTATION.md](../archive/ISSUE_17_IMPLEMENTATION.md) - Historical Issue #17 status (archived)
2. `Win32Emu.Tests.Kernel32/NewFunctionsTests.cs` - Enhanced tests
3. `Win32Emu/Loader/LoadedImage.cs` - Added export dictionaries and forwarded export support
4. `Win32Emu/Loader/PeImageLoader.cs` - Added export parsing with forwarded export handling
5. `Win32Emu/Win32/Modules/Kernel32Module.cs` - Implemented GetProcAddress with ResolveForwardedExport
6. `Win32Emu/Win32/NativeTypes.cs` - Added error codes
7. `Win32Emu/Win32/ProcessEnvironment.cs` - Added TryGetLoadedImage

### Enhancement (Current)
8. `Win32Emu.Tests.Kernel32/PeExportIntegrationTests.cs` - Comprehensive integration tests with real PE files
9. `docs/implementation/GETPROCADDRESS_IMPLEMENTATION.md` - Updated documentation to reflect current state
