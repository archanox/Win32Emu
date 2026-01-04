# NE Parser: Specification Compliance Fixes

## Problem

During audit of the NE parser implementation against the osdev.org/NE specification, two critical bugs were discovered that caused valid NE executables to be misparsed. These bugs made it appear that test executables were corrupted, when in fact the parser was not correctly implementing the NE specification.

## Key Finding

The test executables (including Windows ME calc.exe) are **NOT corrupted**. The NE parser had specification compliance bugs that caused valid files to be incorrectly parsed.

## Bug #1: Incorrect Module Reference Table Format Detection

### Issue
The `ParseImportModuleTable` function attempted to auto-detect between two formats:
1. "Standard format": Array of 2-byte offsets into Imported Names Table
2. "Inline format": Direct Pascal strings in the Module Reference Table

According to the NE specification from osdev.org/NE, there is **NO "inline format"**. The Module Reference Table is always an array of 2-byte offsets into the Imported Names Table, where the actual module names (as Pascal strings) are stored.

### Root Cause
The code contained this logic:
```csharp
// The Module Reference Table format varies between NE implementations:
// Standard format: Array of 2-byte offsets into the Imported Names Table
// Alternative format (Windows ME and some others): Inline Pascal strings

// Try to detect which format by checking if the first entry looks like an offset or a string
```

This was **incorrect**. The NE format specification is clear: the Module Reference Table contains 2-byte offsets, not inline strings.

### Impact
When the heuristic detection failed (which it often did for valid NE files), the parser would try to read Pascal strings directly from the Module Reference Table instead of following the offsets to the Imported Names Table. This caused:
- Incorrect module names
- Parsing failures
- Files appearing "corrupted" when they were actually valid

### Fix
Removed all "inline format" detection logic and implemented the standard NE specification:

```csharp
private static List<string> ParseImportModuleTable(byte[] bytes, NeHeader header)
{
    var modules = new List<string>();
    var moduleTableOffset = header.BaseOffset + header.ModuleReferenceTableOffset;
    var importNamesOffset = header.BaseOffset + header.ImportedNamesTableOffset;
    
    var moduleCount = header.ModuleReferenceCount;
    
    // According to NE specification, Module Reference Table is an array of 2-byte offsets
    // into the Imported Names Table (which contains Pascal strings)
    // Each offset is relative to the start of the Imported Names Table
    
    for (var i = 0; i < moduleCount; i++)
    {
        var offset = moduleTableOffset + (i * NE_MODULE_REF_ENTRY_SIZE);
        var nameOffset = BitConverter.ToUInt16(bytes, offset);
        
        // The offset is relative to the Imported Names Table start
        var actualOffset = importNamesOffset + nameOffset;
        
        var nameLength = bytes[actualOffset];
        var moduleName = Encoding.ASCII.GetString(bytes, actualOffset + 1, nameLength);
        modules.Add(moduleName);
    }
    
    return modules;
}
```

## Bug #2: Incorrect Segment Length Handling

### Issue
In `ParseSegmentTable`, the code had this logic for handling 0-length segments:

```csharp
// If length is 0, it means full 64KB segment
uint length = lengthRaw;
if (length == 0 && minAllocation > 0)
{
    length = 0x10000; // 64KB full segment
}
```

According to the NE specification, a segment length of 0 **always** means 64KB, not conditionally based on `minAllocation`.

### Impact
Segments with length=0 but minAllocation=0 would be treated as having 0 length instead of 64KB, causing:
- Incorrect memory allocation
- Missing segment data
- Apparent "corruption" of valid executables

### Fix
```csharp
// If length is 0, it means full 64KB segment
// According to NE specification, a length of 0 indicates 64KB
uint length = lengthRaw;
if (length == 0)
{
    length = 0x10000; // 64KB full segment
}
```

## Related Changes

### Reverted: NE Loader Exception Handling
Earlier commits added exception handling to the NE loader for "corrupted segments". This was reverted because:
1. The test executables are NOT corrupted
2. The parser bugs were the root cause, not file corruption
3. Adding exception handling masked the real issues in the parser
4. Valid files should not need exception handling

The NE loader now works with the corrected parser without needing try-catch blocks for normal operation.

## Testing

All NE tests continue to pass with the corrected parser:
- `IsNE_WithNonExistentFile_ReturnsFalse`
- `IsNE_WithTextFile_ReturnsFalse`
- `IsNE_WithInvalidNEHeader_ReturnsFalse`
- `IsNE_WithValidNESignature_ReturnsTrue`
- `IsNE_WithByteArray_ValidNESignature_ReturnsTrue`
- `IsNE_WithByteArray_InvalidSignature_ReturnsFalse`
- `DetectFormat_WithNEFile_ReturnsNE`
- `DetectFormat_WithByteArray_NEFile_ReturnsNE`
- `DetectFormat_WithInvalidFile_ReturnsUnknown`
- `LoadFromBytes_WithMinimalNEFile_CreatesLoadedImage`
- `LoadFromBytes_WithMinimalNEFile_SetsHeaderEndRvaToZero`
- `LoadFromBytes_WithImportModules_ParsesModuleNamesCorrectly`
- `LoadFromBytes_WithExtraSpaceAfterModuleTable_OnlyReadsSpecifiedCount`

## References

- **NE Specification**: https://wiki.osdev.org/NE
- **Module Reference Table**: Always an array of 2-byte offsets into Imported Names Table
- **Segment Length**: 0 means 64KB unconditionally
- **Imported Names Table**: Contains Pascal strings (length byte + string data)

## Related Files

- `Win32Emu.NeParser/NeParser.cs`: Parser bug fixes
- `Win32Emu/Loader/NeImageLoader.cs`: Reverted unnecessary exception handling
- `Win32Emu.Tests.Emulator/NeImageLoaderTests.cs`: Test coverage (all passing)

## Conclusion

The NE parser now correctly implements the NE specification from osdev.org/NE. The bugs that made valid executables appear corrupted have been fixed. No exception handling for "corrupted" files is needed because the files were never corrupted - the parser was simply not following the specification correctly.
