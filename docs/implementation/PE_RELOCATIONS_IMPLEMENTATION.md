# PE Base Relocations Implementation

## Overview

This document describes the implementation of PE (Portable Executable) base relocations in Win32Emu. Base relocations allow PE images to be loaded at memory addresses different from their preferred ImageBase, which is essential for proper emulation of Windows executable loading behavior.

## Background

### What are Base Relocations?

When a PE file (EXE or DLL) is compiled, the linker assigns a preferred base address (ImageBase) where it expects to be loaded in memory. However, at runtime, the loader may need to load the image at a different address due to:

- Address conflicts with other loaded modules
- ASLR (Address Space Layout Randomization) security features
- Memory layout constraints

When this happens, all absolute memory addresses in the code and data sections need to be adjusted (relocated) to account for the difference between the preferred and actual base addresses. This process is called base relocation.

### PE Relocation Structure

The PE format stores relocation information in the `.reloc` section. Each relocation entry specifies:
- **Location**: The RVA (Relative Virtual Address) of the address that needs to be adjusted
- **Type**: The kind of relocation (e.g., 32-bit absolute, high 16 bits, low 16 bits)

## Implementation

### Location

The implementation is in `Win32Emu/Loader/PeImageLoader.cs`:
- Method: `ApplyRelocations()`
- Called from: `Load()` method, after sections are loaded but before building import maps

### Key Components

#### 1. Relocation Detection

```csharp
var delta = (long)actualBase - (long)preferredBase;
if (delta == 0)
{
    // No relocations needed - loaded at preferred base
    return;
}
```

The implementation calculates the delta (difference) between where the image is actually loaded and where it wants to be loaded.

#### 2. Processing Relocations

For each relocation in `image.Relocations`:
1. Extract the RVA from the Location (ISegmentReference)
2. Calculate the virtual address: `va = actualBase + rva`
3. Apply the relocation based on its type

#### 3. Supported Relocation Types

| Type | Value | Description | Implementation |
|------|-------|-------------|----------------|
| Absolute | 0 | No-op (padding) | Skip |
| HighLow | 3 | 32-bit absolute | Read 32-bit value, add delta, write back |
| High | 1 | High 16 bits | Read 16-bit value, add high 16 bits of delta |
| Low | 2 | Low 16 bits | Read 16-bit value, add low 16 bits of delta |
| Dir64 | 10 | 64-bit absolute | Log warning (shouldn't occur in PE32) |
| HighAdj | 4 | Complex (2-slot) | Log warning (rarely used) |

**Note**: For PE32 images, **HighLow** (type 3) is by far the most common relocation type, as it handles standard 32-bit absolute addresses.

#### 4. ISegmentReference Handling

The `Location` property in `BaseRelocation` is of type `ISegmentReference`, which is an interface. The implementation handles three concrete types:

```csharp
if (relocation.Location is SegmentReference segRef)
    rva = segRef.Rva;
else if (relocation.Location is RelativeReference relRef)
    rva = relRef.Rva;
else if (relocation.Location is VirtualAddress virtAddr)
    rva = virtAddr.Rva;
```

### Error Handling

The implementation includes comprehensive error handling:

- **Missing relocations**: Logs warning if image needs relocations but has none
- **Invalid locations**: Skips relocations with null or unsupported location types
- **Unsupported types**: Logs warnings for rarely-used or PE32+-only relocation types
- **Exceptions**: Catches and logs errors for individual relocations, continues processing
- **Summary**: Reports total applied and failed relocations

### Logging

The implementation provides detailed logging at multiple levels:

- **Debug**: When no relocations needed (loaded at preferred base)
- **Info**: Start of relocation processing, completion summary
- **Trace**: Individual relocation details (original value → new value)
- **Warning**: Missing relocations, unsupported types, null locations
- **Error**: Exceptions during relocation processing

## Testing

### Unit Tests

Tests are located in `Win32Emu.Tests.Emulator/PeImageLoaderTests.cs`:

- **Load_AppliesRelocations_WhenImageBaseChanged**: Placeholder test verifying the implementation structure exists

### Integration Testing

The relocation code is validated through integration tests with real game executables that contain base relocations. These tests verify:

1. Images load successfully when relocated
2. Code executes correctly at non-preferred base addresses
3. No crashes or memory corruption occurs

## Technical Details

### Memory Operations

All memory operations use the `VirtualMemory` interface:
- `vm.Read32(va)` / `vm.Write32(va, value)` for 32-bit relocations
- `vm.Read16(va)` / `vm.Write16(va, value)` for 16-bit relocations

### Performance Considerations

- Relocations are only applied when `delta != 0`
- Each relocation requires 1 read and 1 write operation
- Processing is O(n) where n is the number of relocations
- Typical PE files have hundreds to thousands of relocations

### AsmResolver Integration

The implementation uses the AsmResolver.PE library:
- `PEImage.Relocations` property provides the relocation list
- Each `BaseRelocation` contains Location and Type
- No need to manually parse the `.reloc` section

## Future Enhancements

Potential improvements for the future:

1. **Comprehensive Testing**: Create synthetic PE files with various relocation types for thorough testing
2. **Performance Metrics**: Track and log relocation processing time for large images
3. **Validation**: Verify that relocated addresses remain within valid memory ranges
4. **Advanced Types**: Implement support for complex relocation types like HIGHADJ if needed

## References

- [PE Format Specification - Base Relocations](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format#the-reloc-section-image-only)
- [AsmResolver Documentation](https://docs.washi.dev/asmresolver/)
- [Windows PE Loader Behavior](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format)

## Revision History

| Date | Version | Changes |
|------|---------|---------|
| 2025-01-03 | 1.0 | Initial implementation of PE base relocations |
