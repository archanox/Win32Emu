# Fix for fputc/fprintf/fputs Console Output Issue

## Problem
Native test executables using `fputc`, `fprintf`, `fputs`, and related functions were not producing console output. The logs showed that the functions were being called successfully with the correct parameters, but the output was not appearing in the StdOut console.

### Root Cause
The stream pointer passed to these functions (e.g., `0x0F000210`) was not being recognized as stdout or stderr. The original implementation only checked if the stream matched `_iobArrayPtr + 32` (for stdout) or `_iobArrayPtr + 64` (for stderr). However:

1. `_iobArrayPtr` was only set when `__p__iob()` was explicitly called
2. Programs that import `_iob` as a data export (common in native code) may never call `__p__iob()` directly
3. The stream pointers from data imports were in the valid range but didn't match the uninitialized `_iobArrayPtr`

## Solution
Added a `GetStandardStreamType` helper method that:

1. **Checks against known _iobArrayPtr** (if set): First tries to match against the cached `_iobArrayPtr` values
2. **Address pattern analysis**: If not matched, analyzes the stream address to detect if it could be stdout/stderr based on:
   - Offset from `_imageBase` (module base address)
   - Standard FILE structure spacing (32 bytes per stream: stdin, stdout, stderr)
3. **Auto-detection and caching**: When a stream is detected as stdout/stderr, it updates `_iobArrayPtr` so future calls will match against the cached value

### Updated Functions
- `fputc` - Write character to stream
- `fprintf` - Formatted output to stream
- `fputs` - Write string to stream
- `vfprintf` - Formatted output to stream (va_list version)
- `fwrite` - Write binary data to stream

## Testing
Added comprehensive tests in `MsvcrtStringAndIoTests.cs`:
- `Fputc_ToStdout_ReturnsCharacterWritten` - Tests with stream from `__p__iob()` call
- `Fputc_WithStreamAtImageBaseOffset_ReturnsCharacterWritten` - Tests with stream from data import scenario

All 97 MSVCRT tests pass, confirming no regressions.

## Technical Details
The detection logic works as follows:

```csharp
// Example: stream = 0x0F000210 (from logs)
// If this is stdout, then _iobArrayPtr = stream - 32 = 0x0F0001F0
// Each FILE structure is 32 bytes:
//   stdin  = _iobArrayPtr + 0  = 0x0F0001F0
//   stdout = _iobArrayPtr + 32 = 0x0F000210
//   stderr = _iobArrayPtr + 64 = 0x0F000230
```

The method checks if `(stream - 32)` or `(stream - 64)` falls within a reasonable range from `_imageBase` (within first 64KB of module), indicating it's likely a standard stream pointer.

## Impact
This fix ensures that:
- Console output from native executables appears correctly in StdOut
- Both direct `__p__iob()` calls and data import scenarios work correctly
- The fix is backward compatible with existing code
- No performance impact as detection only happens once per stream
