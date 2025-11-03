# PE Header Values Implementation Status

This document details which PE (Portable Executable) header values are properly respected by the Win32Emu PE loader, addressing the requirement: "I assume we're respecting the EntryPoint, ImageBase plus all the other magic numbers we have in the PE headers? The section header locations and flags? The import dll thunk values? The import function hints?"

## Summary

✅ **FULLY RESPECTED** - Value is loaded and used correctly  
⚠️ **PARTIALLY RESPECTED** - Value is loaded but not fully enforced  
📝 **DOCUMENTED** - Value is accessible but not actively used  

## DOS Header

| Field | Status | Notes |
|-------|--------|-------|
| e_magic (MZ signature) | ✅ | Validated during load, present in memory |
| e_lfanew (PE offset) | ✅ | Used to locate PE signature, present in memory |

## PE File Header

| Field | Status | Notes |
|-------|--------|-------|
| Machine | ✅ | Must be IMAGE_FILE_MACHINE_I386 (0x014C) |
| NumberOfSections | ✅ | Used to parse section table |
| TimeDateStamp | 📝 | Present in memory, not actively used |
| PointerToSymbolTable | 📝 | Present in memory, not actively used (deprecated) |
| NumberOfSymbols | 📝 | Present in memory, not actively used (deprecated) |
| SizeOfOptionalHeader | ✅ | Used to parse optional header |
| Characteristics | 📝 | Present in memory, not actively validated |

## PE Optional Header

### Standard Fields

| Field | Status | Notes |
|-------|--------|-------|
| Magic | ✅ | Must be PE32 (0x010B), PE32+ not supported |
| MajorLinkerVersion | 📝 | Present in memory, not actively used |
| MinorLinkerVersion | 📝 | Present in memory, not actively used |
| SizeOfCode | 📝 | Present in memory, not actively used |
| SizeOfInitializedData | 📝 | Present in memory, not actively used |
| SizeOfUninitializedData | 📝 | Present in memory, not actively used |
| **AddressOfEntryPoint** | ✅ | **Used to determine program start address** |
| BaseOfCode | 📝 | Present in memory, not actively used |
| BaseOfData | 📝 | Present in memory, not actively used |

### Windows-Specific Fields

| Field | Status | Notes |
|-------|--------|-------|
| **ImageBase** | ✅ | **Used as base address for loading** |
| SectionAlignment | ✅ | Sections loaded at correct aligned RVAs |
| FileAlignment | ✅ | Sections loaded from correct file offsets |
| MajorOperatingSystemVersion | 📝 | Present in memory, not actively used |
| MinorOperatingSystemVersion | 📝 | Present in memory, not actively used |
| MajorImageVersion | 📝 | Present in memory, not actively used |
| MinorImageVersion | 📝 | Present in memory, not actively used |
| MajorSubsystemVersion | 📝 | Present in memory, not actively used |
| MinorSubsystemVersion | 📝 | Present in memory, not actively used |
| Win32VersionValue | 📝 | Present in memory, not actively used |
| **SizeOfImage** | ✅ | **Stored in LoadedImage for validation** |
| **SizeOfHeaders** | ✅ | **Headers loaded into memory** |
| CheckSum | 📝 | Present in memory, not validated |
| **Subsystem** | ✅ | **Stored in LoadedImage (GUI vs CUI)** |
| DllCharacteristics | 📝 | Present in memory, not actively enforced |
| **SizeOfStackReserve** | ✅ | **Stored in LoadedImage for stack setup** |
| **SizeOfStackCommit** | ✅ | **Stored in LoadedImage for stack setup** |
| SizeOfHeapReserve | 📝 | Present in memory, not actively used |
| SizeOfHeapCommit | 📝 | Present in memory, not actively used |
| LoaderFlags | 📝 | Present in memory, not actively used (deprecated) |
| NumberOfRvaAndSizes | ✅ | Used to parse data directories |

## Data Directories

| Directory | Status | Notes |
|-----------|--------|-------|
| Export Table | ✅ | Parsed and stored in LoadedImage.ExportsByName/Ordinal |
| **Import Table** | ✅ | **Parsed, IAT populated with synthetic addresses** |
| Resource Table | ⚠️ | Accessible via PeResourceReader, not automatically loaded |
| Exception Table | 📝 | Present in memory, not actively used |
| Certificate Table | 📝 | Present in memory, not validated |
| **Base Relocation Table** | ✅ | **Applied when image base differs from preferred** |
| Debug Directory | 📝 | Present in memory, not actively used |
| Architecture | 📝 | Present in memory, not actively used |
| Global Ptr | 📝 | Present in memory, not actively used |
| TLS Table | 📝 | Present in memory, not yet implemented |
| Load Config Table | 📝 | Present in memory, not actively used |
| Bound Import | 📝 | Present in memory, not actively used |
| IAT | ✅ | Overwritten with synthetic addresses for syscall interception |
| Delay Import Descriptor | 📝 | Not supported (will throw error if present) |
| CLR Runtime Header | ❌ | Not supported (.NET assemblies not supported) |

## Section Headers

| Field | Status | Notes |
|-------|--------|-------|
| Name | ✅ | Used for logging and debugging |
| VirtualSize | ✅ | Sections allocated to VirtualSize, zero-filled beyond raw data |
| **VirtualAddress (RVA)** | ✅ | **Sections loaded at correct RVAs** |
| SizeOfRawData | ✅ | Used to determine how much data to read from file |
| PointerToRawData | ✅ | Used to locate section data in file |
| PointerToRelocations | 📝 | Present in memory, not actively used (for OBJ files) |
| PointerToLinenumbers | 📝 | Present in memory, not actively used (deprecated) |
| NumberOfRelocations | 📝 | Present in memory, not actively used (for OBJ files) |
| NumberOfLinenumbers | 📝 | Present in memory, not actively used (deprecated) |
| **Characteristics** | ⚠️ | **Logged but not enforced** (read/write/execute flags) |

### Section Characteristics Details

Section characteristics (flags) are:
- ✅ **Preserved in memory** - Available for inspection
- ✅ **Logged during load** - Visible in debug output
- ⚠️ **Not enforced** - No memory protection applied based on flags
- 📝 **Available for future use** - Could be used to implement memory protection

This is acceptable for emulation because:
1. The emulated x86 code expects to be able to read/write/execute based on its own assumptions
2. Memory protection enforcement would require implementing a full page protection system
3. Programs can still inspect their own section flags by reading PE headers from memory

## Import Table Details

### Import Address Table (IAT)

| Aspect | Status | Notes |
|--------|--------|-------|
| **IAT RVA** | ✅ | **IAT entries written at correct addresses** |
| **IAT Thunk Values** | ✅ | **Overwritten with synthetic addresses (0x0F000000 range)** |
| **Import by Name** | ✅ | **Function name stored in import map** |
| **Import by Ordinal** | ✅ | **Ordinal used when name unavailable** |

### Import Lookup Table (ILT)

| Aspect | Status | Notes |
|--------|--------|-------|
| ILT RVA | ✅ | Preserved in memory as part of PE headers |
| **ILT Original Data** | ✅ | **Preserved - IAT is overwritten, but ILT remains** |

### Import Hints

| Aspect | Status | Notes |
|--------|--------|-------|
| Hint Values | ✅ | Accessible from ImportedSymbol.Hint property |
| Hint Usage | 📝 | Not used for optimization (acceptable for emulation) |
| **Ordinal vs Hint** | ✅ | **FIXED: Now correctly uses Ordinal for imports by ordinal** |

**Important Fix**: The loader previously incorrectly used `sym.Hint` instead of `sym.Ordinal` when constructing names for imports by ordinal. This has been corrected:
- **Hint**: Suggested index into export name table (optimization, may be 0 or incorrect)
- **Ordinal**: Actual function identifier for import by ordinal (must be correct)

## Base Relocations

| Field | Status | Notes |
|-------|--------|-------|
| Relocation Blocks | ✅ | Processed when image base != preferred base |
| Relocation Types | ✅ | HIGHLOW (32-bit), HIGH (16-bit), LOW (16-bit) supported |
| Relocation RVAs | ✅ | Addresses correctly calculated and patched |

## Memory Layout

| Aspect | Status | Notes |
|--------|--------|-------|
| **DOS Headers in Memory** | ✅ | **MZ header at image base** |
| **PE Headers in Memory** | ✅ | **NT headers loaded at offset indicated by e_lfanew** |
| **Section Headers in Memory** | ✅ | **Section table present in memory** |
| **Sections at Correct RVAs** | ✅ | **Each section loaded at ImageBase + RVA** |
| **Zero-filled BSS** | ✅ | **Uninitialized data regions are zero** |

## Export Table

| Field | Status | Notes |
|-------|--------|-------|
| Export by Name | ✅ | Stored in LoadedImage.ExportsByName (case-insensitive) |
| Export by Ordinal | ✅ | Stored in LoadedImage.ExportsByOrdinal |
| Forwarded Exports | ✅ | Stored in LoadedImage.ForwardedExportsByName/Ordinal |
| Export RVAs | ✅ | Converted to VAs (ImageBase + RVA) |

## Validation and Testing

All critical PE header values are validated through comprehensive tests in `PeHeaderRespectTests.cs`:

1. ✅ `EntryPoint_IsRespected_FromOptionalHeader` - Verifies entry point calculation
2. ✅ `ImageBase_IsRespected_FromOptionalHeader` - Verifies base address
3. ✅ `SectionHeaders_LocationsAreRespected` - Verifies sections loaded at correct RVAs
4. ✅ `SectionHeaders_CharacteristicsAreAccessible` - Verifies section flags available
5. ✅ `ImportHints_AreAccessible` - Verifies hint values accessible
6. ✅ `ImportAddressTable_IsProperlyPopulated` - Verifies IAT thunk values
7. ✅ `ImportThunks_AreProperlyHandled` - Verifies import mapping correct
8. ✅ `SizeOfImage_IsRespected` - Verifies image size
9. ✅ `SizeOfHeaders_IsRespected` - Verifies headers loaded
10. ✅ `StackSizes_AreRespected` - Verifies stack reserve/commit
11. ✅ `Subsystem_IsRespected` - Verifies subsystem type
12. ✅ `PEHeaders_AreLoadedIntoMemory` - Verifies headers in memory
13. ✅ `ImportNameConsistency_OrdinalVsHint` - Verifies ordinal vs hint usage
14. ✅ `BaseRelocations_AreAccessible` - Verifies relocation data

Additionally, `PeLoaderValidationTests.cs` cross-validates AsmResolver against PeNet library for critical values.

## Conclusion

**All critical PE header values are properly respected.** The loader:

1. ✅ **Correctly loads the image at ImageBase**
2. ✅ **Correctly identifies the EntryPoint**
3. ✅ **Correctly loads all sections at their specified RVAs**
4. ✅ **Correctly preserves section characteristics** (though not enforced)
5. ✅ **Correctly processes import thunks** (IAT written, ILT preserved)
6. ✅ **Correctly handles import ordinals and hints**
7. ✅ **Correctly applies base relocations when needed**
8. ✅ **Correctly loads PE headers into memory** (for self-inspection)
9. ✅ **Correctly stores stack sizes** (for thread creation)
10. ✅ **Correctly identifies subsystem** (GUI vs CUI)

The implementation is suitable for accurate PE emulation. Values marked as "not actively used" are still present in memory and accessible if needed by the emulated program or for future enhancements.

## References

- PE Format Specification: https://learn.microsoft.com/en-us/windows/win32/debug/pe-format
- AsmResolver Documentation: https://docs.washi.dev/asmresolver/
- Implementation: `Win32Emu/Loader/PeImageLoader.cs`
- Tests: `Win32Emu.Tests.Emulator/PeHeaderRespectTests.cs`
- Validation Tests: `Win32Emu.Tests.Emulator/PeLoaderValidationTests.cs`
