# Heap Base Calculation Fix

## Problem
The emulator would incorrectly flag legitimate code execution as "heap execution" and terminate the emulation after 10 instructions. This was a false positive caused by an incorrect heap base address calculation.

## Symptoms
```
[22:21:30] [WRN] [Emulator] EIP=0x01012310 is in heap memory range (0x01000000-0x6FFFFFFF). This may indicate a bad jump or return address. Consecutive heap executions: 1
[22:21:30] [WRN] [Emulator] EIP=0x01012311 is in heap memory range (0x01000000-0x6FFFFFFF). This may indicate a bad jump or return address. Consecutive heap executions: 2
...
[22:21:30] [ERR] [Emulator] HEAP EXECUTION DETECTED: EIP has been in heap memory range for 10 consecutive iterations. EIP=0x01012330, ESP=0x0013EF84. Stopping emulation.
```

The entry point address (0x01012310) is in the .text (code) section, not heap memory, but was being flagged as heap execution.

## Root Cause
The `CalculateHeapBase()` function was hardcoded to return `0x01000000`, which is the default image base address for Win32 PE executables. This caused the following problem:

1. PE executable loads at image base: `0x01000000`
2. .text section starts at image base + RVA: `0x01000000 + 0x1000 = 0x01001000`
3. Entry point is within .text section: `0x01012310`
4. Heap base was hardcoded to: `0x01000000`
5. Heap range check: `if (eip >= 0x01000000 && eip < 0x70000000)` → **TRUE** (false positive!)

The code section was incorrectly considered to be in the "heap memory range" because the heap base overlapped with the image base.

## Solution
Changed `CalculateHeapBase()` to calculate the heap base dynamically based on the loaded image:

1. **Calculate heap base after image**: `heapBase = image.BaseAddress + image.ImageSize`
2. **Align to 64KB boundary**: Align to `0x10000` (standard Windows allocation granularity)
3. **Pass LoadedImage parameter**: Modified function signature to accept the loaded image

Example calculation for calc.exe:
- Image base: `0x01000000`
- Image size: `0x18000` (from log: "Size=0x18000")
- Initial heap base: `0x01000000 + 0x18000 = 0x01018000`
- Aligned heap base: `0x01020000` (aligned to 64KB boundary)

Now the heap range is `[0x01020000, 0x70000000)` and the code section `[0x01001000, 0x010133AA)` is correctly excluded from heap checks.

## Code Changes
File: `Win32Emu/Emulator.cs`

### 1. Updated `CalculateHeapBase()` method (lines ~2840-2851):
```csharp
private static uint CalculateHeapBase(LoadedImage image)
{
    // Calculate heap base as image base + image size
    var heapBase = image.BaseAddress + image.ImageSize;
    
    // Align to 64KB boundary (0x10000) - standard Windows allocation granularity
    const uint ALLOCATION_GRANULARITY = 0x10000;
    heapBase = (heapBase + ALLOCATION_GRANULARITY - 1) & ~(ALLOCATION_GRANULARITY - 1);
    
    return heapBase;
}
```

### 2. Updated call sites (lines 476 and 577):
```csharp
// Line 476: ProcessEnvironment initialization
_env = new ProcessEnvironment(_vm, CalculateHeapBase(_image), _host, _logger, _backendFactory);

// Line 577: Store heap base for emulation loop checks
_heapBase = CalculateHeapBase(_image);
```

## Results
- ✅ No false positive heap execution warnings for legitimate code
- ✅ Entry point and code sections correctly identified as code, not heap
- ✅ Heap detection still works for actual heap execution bugs
- ✅ All existing tests pass (Win32Emu.Tests.Emulator)
- ✅ Build successful with no errors

## Memory Layout After Fix
For a typical PE executable like calc.exe:
```
0x00000000 - 0x00000FFF: NULL page (protected)
0x00001000 - 0x00FFFFFF: Reserved/unmapped
0x01000000 - 0x01000FFF: PE headers
0x01001000 - 0x010133AA: .text section (code) ✅ NO LONGER FLAGGED AS HEAP
0x01014000 - 0x01014FFF: .data section (data)
0x01015000 - 0x0101FFFF: Unused (part of image)
0x01020000 - 0x6FFFFFFF: Heap region (VirtualAlloc, HeapAlloc, GlobalAlloc)
0x70000000 - 0x7FFFFFFF: Reserved (future expansion)
...
0x0D000000 - 0x0FFFFFFF: Emulator infrastructure (COM vtables, syscall dispatcher, import stubs)
```

## Related Issues
This fix addresses the root cause that was previously worked around in `CHKCPU32_HEAP_EXECUTION_FIX.md`. The CHKCPU32 fix added detection and termination logic for heap execution, but this fix ensures legitimate code is not incorrectly flagged as heap execution in the first place.

## Test
Any PE executable should now run without false positive heap execution warnings:
```bash
dotnet run --project Win32Emu -- --file path/to/calc.exe
```

Expected: No heap execution warnings unless the program actually has a bug causing code to jump into data memory.

## Date
2025-12-16
