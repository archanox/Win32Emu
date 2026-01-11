# Fix for fputc/fprintf/fputs Console Output Issue

## Problem
Native test executables using `fputc`, `fprintf`, `fputs`, and related functions were not producing console output. The logs showed that the functions were being called successfully with the correct parameters, but the output was not appearing in the StdOut console.

### Root Cause
The stream pointer passed to these functions (e.g., `0x0F000210`) was not being recognized as stdout or stderr. The original implementation checked against the cached `_iobArrayPtr` and would immediately return -1 (not a standard stream) if the stream didn't match.

The issue occurred when:
1. The executable's `_iob` array was allocated at 0x00448000
2. MSVCRT.DLL's own `_iob` array was at 0x0F0001F0 
3. `fputc` was called with stream=0x0F000210 (MSVCRT.DLL's stdout pointer)
4. The cached `_iobArrayPtr` was 0x00448000 (from the executable)
5. Since the stream didn't match the cached array, detection failed

This happens because:
- Programs that link against MSVCRT.DLL may import `_iob` as a data export
- MSVCRT.DLL itself has its own internal `_iob` array
- Both arrays are valid, but at different memory addresses
- The old code would only recognize ONE array at a time

## Solution
Modified `GetStandardStreamType` to support detection of multiple `_iob` arrays:

1. **Still checks cached `_iobArrayPtr` first**: For performance, if stream matches the cached array, return immediately
2. **Falls through to heuristic detection**: If stream doesn't match cached array, don't give up - try to detect if it could be from a different `_iob` array
3. **Address pattern analysis**: Analyzes the stream address to detect if it could be stdout/stderr based on:
   - Offset from potential `_iob` base address (subtract FILE_STRUCTURE_SIZE for stdout check)
   - Standard FILE structure spacing (32 bytes per stream: stdin, stdout, stderr)
   - Reasonable address range (>= 0x10000 && < 0xFFFF0000)
4. **Preserves first detected array**: Only updates `_iobArrayPtr` if it wasn't already set, to maintain backwards compatibility

### Updated Behavior
The detection now works as follows:

```csharp
// Example: stream = 0x0F000210 (MSVCRT.DLL's stdout)
// Cached _iobArrayPtr = 0x00448000 (executable's _iob array)

// Step 1: Check against cached array
// 0x0F000210 != 0x00448000 + 32, so doesn't match - continue

// Step 2: Try heuristic detection
// potentialIobBase = 0x0F000210 - 32 = 0x0F0001F0
// This is a valid address range, so detect it as stdout
// Log: "Found additional _iob array at 0x0F0001F0 (different from cached 0x00448000)"
// Return 1 (stdout)
```

### Updated Functions
The fix applies to all functions that use `GetStandardStreamType`:
- `fputc` - Write character to stream
- `fprintf` - Formatted output to stream  
- `fputs` - Write string to stream
- `vfprintf` - Formatted output to stream (va_list version)
- `fwrite` - Write binary data to stream

## Testing
All existing tests in `MsvcrtStringAndIoTests.cs` still pass (24/24):
- `Fputc_ToStdout_ReturnsCharacterWritten` - Tests with stream from `__p__iob()` call
- `Fputc_WithStreamAtImageBaseOffset_ReturnsCharacterWritten` - Tests with stream from data import scenario
- `Fputc_ToStderr_ReturnsCharacterWritten` - Tests stderr detection

Manual testing with native executables confirms console output now appears correctly:
- `test_environment.exe` - Outputs "Testing Environment Variable Functions" and test results
- `test_getmodulefilename.exe` - Outputs "Testing GetModuleFileNameA" and test results

## Impact
This fix ensures that:
- Console output from native executables appears correctly in StdOut
- Multiple `_iob` arrays (from executable and MSVCRT.DLL) are supported
- Both direct `__p__iob()` calls and data import scenarios work correctly  
- The fix is backward compatible with existing code
- No performance impact as cached array is checked first
