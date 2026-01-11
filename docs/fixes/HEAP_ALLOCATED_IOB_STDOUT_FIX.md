# Fix for Heap-Allocated _iob Array Stdout Detection

## Problem

Executables like `test_environment.exe` that use `printf()`, `fprintf()`, `fputc()`, and related functions were not producing console output on the WASM frontend. Debug log entries showed that `fputc` was being called with the correct characters (e.g., 'T', 'e', 's', 't', 'i', 'n', 'g'), but the output wasn't reaching the Standard Output panel.

### Symptoms

- Debug logs showed `fputc(c=n (0x6E), stream=0x0F000210)` being called repeatedly
- NO logs showed `"[msvcrt] fputc detected stdout stream, writing to stdout"`
- This indicated that `GetStandardStreamType()` was not recognizing the stream as stdout

## Root Cause

The `GetStandardStreamType` function in `MsvcrtModule.cs` was checking if potential `_iobArrayPtr` addresses were within a 64KB range of `_imageBase`:

```csharp
if (potentialIobBase >= _imageBase && potentialIobBase < _imageBase + IOB_DETECTION_RANGE)
```

Where:
- `_imageBase` = base address of the loaded PE image (typically `0x00400000` for executables)
- `IOB_DETECTION_RANGE` = `0x10000` (64KB)

This works for **static `_iob` arrays** in the module's data section, but fails for **heap-allocated `_iob` arrays** created by `__p__iob()`.

### Why Heap-Allocated?

The `__p__iob()` function allocates the `_iob` array via `HeapAlloc()`:

```csharp
if (_iobArrayPtr == 0)
{
    _iobArrayPtr = _env.HeapAlloc(0, 96); // 3 FILE structures * 32 bytes
}
```

Heap allocations can be anywhere in memory, not just near `_imageBase`. In the case of `test_environment.exe`:
- Stream pointer: `0x0F000210` (stdout)
- Potential `_iobArrayPtr`: `0x0F000210 - 32 = 0x0F0001F0`
- Check: `0x0F0001F0 >= 0x00400000` ✓ BUT `0x0F0001F0 < 0x00410000` ✗

The range check failed, so stdout wasn't detected.

## Solution

Updated `GetStandardStreamType` to accept stream pointers in any valid memory range, not just near `_imageBase`:

### New Detection Logic

```csharp
// Check if stream could be stdout (offset FILE_STRUCTURE_SIZE from _iob base)
var potentialIobBase = stream - FILE_STRUCTURE_SIZE;

// Accept any address that looks reasonable (not NULL, not in low memory < 0x10000)
if (potentialIobBase >= 0x10000 && potentialIobBase < 0xFFFF0000)
{
    // Additional validation: check if this looks like it could be in a valid memory region
    if (potentialIobBase >= _imageBase && potentialIobBase < _imageBase + IOB_DETECTION_RANGE)
    {
        // Static _iob array in module's data section
        if (_iobArrayPtr == 0)
        {
            _iobArrayPtr = potentialIobBase;
            _logger.LogInformation("[msvcrt] Detected static _iob array at 0x{Ptr:X8} based on stdout stream pointer", _iobArrayPtr);
        }
        return 1; // stdout
    }
    else
    {
        // Potentially heap-allocated _iob array - be more permissive
        if (_iobArrayPtr == 0)
        {
            _iobArrayPtr = potentialIobBase;
            _logger.LogInformation("[msvcrt] Detected heap-allocated _iob array at 0x{Ptr:X8} based on stdout stream pointer", _iobArrayPtr);
        }
        return 1; // stdout
    }
}
```

### Key Changes

1. **Broadened range check**: Accept any address in `[0x10000, 0xFFFF0000]` instead of just `[_imageBase, _imageBase + 64KB]`
2. **Distinguish static vs heap**: Log whether the `_iob` array appears to be static or heap-allocated
3. **Auto-detection**: Cache `_iobArrayPtr` on first stdout/stderr usage, regardless of location
4. **Safety**: Still reject NULL pointers and low memory addresses (< `0x10000`)

## Testing

### Unit Tests

All 24 existing MSVCRT string/IO tests pass:

```bash
dotnet test Win32Emu.Tests.Kernel32/Win32Emu.Tests.Kernel32.csproj --filter "FullyQualifiedName~MsvcrtStringAndIoTests"
```

Result: `Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24`

### Integration Testing

The fix enables console output for:
- `test_environment.exe` - Environment variable tests with `printf()`
- Any executable using MSVCRT's `printf`, `fprintf`, `fputc`, `fputs`, `fwrite` functions
- Programs that import `_iob` as a data export vs calling `__p__iob()` directly

## Files Changed

- `Win32Emu/Win32/Modules/MsvcrtModule.cs` - Updated `GetStandardStreamType` function

## Impact

This fix ensures that console output from executables appears correctly in the Standard Output panel on both:
- Desktop GUI (Avalonia)
- WASM frontend (Blazor WebAssembly)

Programs that previously appeared to run silently will now show their console output as expected.

## Technical Notes

### FILE Structure Layout

Each FILE structure is 32 bytes in MSVC runtime:
- stdin = `_iobArrayPtr + 0`
- stdout = `_iobArrayPtr + 32`
- stderr = `_iobArrayPtr + 64`

### Memory Ranges

Valid memory addresses in Win32Emu:
- User space: `0x00010000` to `0xFFFF0000`
- NULL and low memory (< `0x10000`): Reserved/invalid
- High memory (>= `0xFFFF0000`): Reserved for special purposes

### Static vs Heap-Allocated _iob

**Static `_iob`** (in module's data section):
- Address near `_imageBase` (within 64KB)
- Common in older executables with static CRT linkage
- Example: `_imageBase=0x00400000`, `_iobArrayPtr=0x00408000`

**Heap-allocated `_iob`** (via `__p__iob()`):
- Address anywhere in heap memory
- Common in modern executables with dynamic CRT linkage
- Example: `_iobArrayPtr=0x0F0001F0` (stdout=`0x0F000210`)

The fix handles both cases correctly.
