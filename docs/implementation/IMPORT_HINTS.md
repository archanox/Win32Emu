# Import Hints in PE Files

## Overview

This document explains how Win32Emu handles DLL import hints in PE (Portable Executable) files and why we don't need to implement hint-based optimization.

## Background: PE Import Structure

A PE file's Import Directory contains information about which DLLs and functions the executable needs to load at runtime. For each imported function, the PE format stores:

1. **Import Name** - The ASCII name of the function (e.g., "CreateFileA")
2. **Hint** - A 16-bit optimization value suggesting the index in the export table where this function might be found
3. **Ordinal** - For ordinal-based imports, the actual ordinal number that identifies the function
4. **Import Address Table (IAT) Entry** - Location where the function's address will be written

## Hint vs Ordinal: Key Differences

### Hint
- **Type**: 16-bit unsigned integer
- **Purpose**: Performance optimization for the Windows PE loader
- **Meaning**: Suggests the index in the DLL's export name table where this function's name might be found
- **Accuracy**: MAY be correct, but is NOT guaranteed to be accurate
- **When Used**: Only used by the Windows loader as a starting point for binary search in export table
- **Example**: If "CreateFileA" is at index 42 in KERNEL32.DLL's export table, the hint might be 42

### Ordinal
- **Type**: 16-bit unsigned integer  
- **Purpose**: Unique identifier for functions exported by ordinal
- **Meaning**: The actual ordinal number assigned to the function in the DLL's export table
- **Accuracy**: MUST be correct for ordinal-based imports
- **When Used**: When a function is imported by ordinal instead of by name (rare, but occurs in some DLLs)
- **Example**: A function might be exported as ordinal 15, meaning you can import it as "DLL!Ordinal_15"

### Import by Name vs Import by Ordinal

#### Import by Name (Most Common)
```
Symbol:
  Name = "CreateFileA"
  Hint = 42 (suggestion where to look in export table)
  Ordinal = 0 (may be 0 or the actual ordinal if known)
  IsImportByOrdinal = false
```

The loader:
1. Checks export table at index 42 (the hint)
2. If the name matches "CreateFileA", done (fast path)
3. If not, searches the entire export table for "CreateFileA" (slow path)

#### Import by Ordinal (Less Common)
```
Symbol:
  Name = null
  Hint = 0 (irrelevant)
  Ordinal = 15 (the actual ordinal number)
  IsImportByOrdinal = true
```

The loader:
1. Looks up function directly by ordinal 15
2. No name comparison needed
3. Function name is synthetic: "Ordinal_15"

## Why Win32Emu Doesn't Use Hints

### Traditional Windows PE Loader Flow
1. Load DLL into memory
2. Parse DLL's export table
3. For each import with a hint, check export table at hint index
4. If name matches, use that address (fast)
5. If name doesn't match, binary search entire export table (slow)

### Win32Emu Emulator Flow
1. Parse import symbols during PE load
2. **Intercept ALL imports** with synthetic addresses (0x0F000000 range)
3. Map synthetic address → (DLL name, function name)
4. When synthetic address is called, dispatch to emulated implementation
5. **No export table searching ever happens**

Since we intercept all imports at load time and never search export tables, hints provide **zero benefit**.

## Implementation Status

### ✅ What We DO Support

1. **Reading hints from PE files** - AsmResolver library provides `symbol.Hint` property
2. **Distinguishing hints from ordinals** - Correctly use `symbol.Ordinal` for ordinal-based imports
3. **Preserving PE header data** - Import tables remain intact in emulated memory
4. **Testing hint accessibility** - `PeHeaderRespectTests.ImportHints_AreAccessible()` verifies hints can be read

### ❌ What We DON'T Support (and don't need to)

1. **Using hints for optimization** - Not applicable since we don't search export tables
2. **Validating hint accuracy** - Not relevant for emulation
3. **Hint-based export table lookup** - We don't load real DLLs or parse their exports

## Code Locations

### Correct Usage
- **`Win32Emu/Loader/PeImageLoader.cs`** (lines 264, 277, 320)
  - Uses `sym.Ordinal` for ordinal-based imports
  - Uses `sym.Name` for named imports
  - Never uses `sym.Hint`

- **`Win32Emu.Tests.Emulator/PeHeaderRespectTests.cs`**
  - `ImportHints_AreAccessible()` - Verifies hints are readable
  - `ImportNameConsistency_OrdinalVsHint()` - Verifies correct ordinal usage
  - Documents that hints are not used for optimization

### Incorrect Usage (Fixed)
- **`Win32Emu.Gui/Services/PeMetadataService.cs`** (lines 133, 139)
  - ❌ **WAS**: `symbol.Hint` used incorrectly for ordinal value
  - ✅ **NOW**: `symbol.Ordinal` used correctly

## PE Format Specification References

From the Microsoft PE/COFF specification:

> "The Hint/Name Table consists of an array of Hint/Name entries. Each entry has a 2-byte Hint field that 
> is an index into the Export Name Pointer Table. The hint is not guaranteed to be correct; it is merely an 
> optimization to speed up the lookup of the function name."

> "The Hint field is a suggested starting point for searching the Export Name Pointer Table. If the function 
> name is not found at the suggested location, a binary search of the Export Name Pointer Table is performed."

## Conclusion

**Do we need to support/implement DLL import hints?**

**Answer: No.** Import hints are already accessible via AsmResolver (`symbol.Hint`) for diagnostic purposes, but implementing hint-based optimization is unnecessary because:

1. We intercept all imports with synthetic addresses at load time
2. We never search DLL export tables (we emulate the functions instead)
3. Hints are only useful for optimizing export table searches
4. All existing tests pass and verify correct handling

The only bug found was in the GUI where `Hint` was confused with `Ordinal`. This has been fixed to use the correct `Ordinal` value for ordinal-based imports.
