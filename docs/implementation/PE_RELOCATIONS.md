# PE Base Relocations Implementation

## Overview

This document describes the implementation of PE base relocations in Win32Emu, which allows executables and DLLs to be loaded at addresses different from their preferred ImageBase.

## Background

### What are Base Relocations?

Base relocations (also called fixups) are entries in a PE file that indicate which absolute addresses in the code and data sections need to be adjusted if the image is loaded at a different base address than its preferred ImageBase.

When a PE file is compiled, the linker assumes it will be loaded at a specific address (the ImageBase). All absolute addresses in the code and data are calculated based on this assumption. However, if the image must be loaded at a different address (due to address conflicts, ASLR, etc.), these absolute addresses need to be "relocated" (adjusted) to account for the difference.

### Why are Relocations Important?

1. **DLL Loading**: Multiple DLLs may have the same preferred ImageBase, causing conflicts
2. **Memory Allocator Flexibility**: The memory allocator may need to load images at specific addresses
3. **ASLR Support**: Address Space Layout Randomization requires loading at random addresses
4. **Correctness**: Without relocations, absolute address references will be incorrect

## Implementation

### Key Components

1. **PeImageLoader.cs**:
   - `Load(string path, uint baseAddress)` - Load at custom base address
   - `LoadFromBytes(byte[] bytes, uint baseAddress)` - Load from memory at custom base
   - `ApplyRelocations()` - Apply fixups to loaded image

2. **Relocation Types Supported**:
   - `IMAGE_REL_BASED_ABSOLUTE` (0): No-op (padding)
   - `IMAGE_REL_BASED_HIGH` (1): High 16 bits
   - `IMAGE_REL_BASED_LOW` (2): Low 16 bits
   - `IMAGE_REL_BASED_HIGHLOW` (3): Full 32 bits (most common for PE32)
   - `IMAGE_REL_BASED_HIGHADJ` (4): Complex relocation (logged as unsupported)
   - `IMAGE_REL_BASED_DIR64` (10): 64-bit relocation (logged as warning in PE32)

### Relocation Process

1. **Calculate Delta**: `delta = actualBase - preferredBase`
2. **Skip if Delta is Zero**: No relocations needed if loaded at preferred base
3. **Process Each Relocation Entry**:
   - Read the current value at the relocation address
   - Add the delta to the value
   - Write the adjusted value back to memory
4. **Handle Different Relocation Types**: Apply appropriate bit manipulations

### Code Flow

```csharp
// Load at custom base address
var customBase = 0x10000000;
var loader = new PeImageLoader(memory, logger);
var loadedImage = loader.Load("myapp.exe", customBase);

// Relocations are automatically applied during load:
// 1. LoadFromImage() uses customBase instead of preferredBase
// 2. ApplyRelocations() is called with delta = customBase - preferredBase
// 3. Each relocation entry is processed and memory is patched
```

## Testing

### Test Coverage

1. **PeRelocationTests.cs**:
   - `Load_AppliesRelocations_WhenLoadedAtDifferentBase()` - Verifies loading at custom base
   - `Load_NoRelocationsApplied_WhenLoadedAtPreferredBase()` - Verifies no-op when delta is 0
   - `Load_CorrectlyPatchesHighLowRelocations()` - Verifies HIGHLOW relocation correctness
   - `Load_HandlesRelocationsInCodeSection()` - Verifies code sections remain accessible
   - `Load_FailsGracefully_WhenRelocationsAreMissing()` - Verifies behavior without relocations
   - `Load_WithRelocationVerification_ActualMemoryPatching()` - Comprehensive integration test that verifies relocations are actually applied to memory by reading and comparing values before and after relocation

2. **PeHeaderRespectTests.cs**:
   - `BaseRelocations_AreAccessible()` - Verifies relocation data is accessible

### Test Strategy

- Use real PE files from `TestData/` and `retrowin32/exe/` directories
- Test with different base addresses (preferred + 0x10000, preferred + 0x20000)
- Verify entry points are correctly adjusted
- Verify code sections remain accessible after relocation

## API Usage

### Default Behavior (No Relocations)

```csharp
// Load at preferred base address (no relocations applied)
var loader = new PeImageLoader(memory, logger);
var image = loader.Load("myapp.exe");
// image.BaseAddress == preferredImageBase from PE header
```

### Custom Base Address (Relocations Applied)

```csharp
// Load at custom base address (relocations applied)
var customBase = 0x10000000;
var loader = new PeImageLoader(memory, logger);
var image = loader.Load("myapp.exe", customBase);
// image.BaseAddress == customBase
// All absolute addresses in code/data adjusted by (customBase - preferredBase)
```

### Loading from Memory

```csharp
// Load from byte array at custom base
byte[] peBytes = File.ReadAllBytes("myapp.exe");
var customBase = 0x10000000;
var loader = new PeImageLoader(memory, logger);
var image = loader.LoadFromBytes(peBytes, customBase);
```

## Limitations and Known Issues

1. **HIGHADJ Relocations**: Not fully supported, logged as warning
2. **DIR64 Relocations**: Logged as warning in PE32 files (should not occur)
3. **No Validation**: Does not validate that relocation addresses are within valid sections
4. **Page Alignment**: Does not enforce page alignment requirements (caller's responsibility)

## Future Enhancements

1. **Automatic Base Address Selection**: Choose non-conflicting addresses automatically
2. **Relocation Validation**: Verify relocations point to valid memory regions
3. **Performance Optimization**: Cache relocation calculations for repeated loads
4. **HIGHADJ Support**: Implement full support for complex relocations

## References

- Microsoft PE/COFF Specification: [docs.microsoft.com/en-us/windows/win32/debug/pe-format](https://docs.microsoft.com/en-us/windows/win32/debug/pe-format)
- AsmResolver Documentation: [docs.washi.dev/asmresolver/](https://docs.washi.dev/asmresolver/)
- PE Format - Base Relocations: Section 6.6 of PE/COFF specification

## Related Files

- `Win32Emu/Loader/PeImageLoader.cs` - Main implementation
- `Win32Emu.Tests.Emulator/PeRelocationTests.cs` - Unit tests
- `Win32Emu.Tests.Emulator/PeHeaderRespectTests.cs` - Integration tests
