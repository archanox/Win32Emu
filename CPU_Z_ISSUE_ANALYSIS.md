# CPU-Z Issue Analysis and Diagnostics

## Issue Summary

The original issue (#265) showed CPU-Z execution logs that were truncated during a `WideCharToMultiByte` call. The log stopped abruptly after logging "Read 257 chars (specified count)" with the next line showing only "[Di..." (truncated Dispatcher log).

## Investigation

### Log Analysis

From the truncated log, we observed:
```
info: Win32Emu.Emulator[0]
      Dispatching KERNEL32.DLL!WideCharToMultiByte at EIP=0x0F000460 ESP=0x001FF574 stack=A4 28 57 00 E9 FD 00 00 20 02 00 00 98 F5 1F 00
info: Win32Emu.Emulator[0]
      [Kernel32] WideCharToMultiByte: CP=Utf8 cchWide=257 lpWide=0x001FF598 cbMulti=256 lpMulti=0x001FFD1C
info: Win32Emu.Emulator[0]
      [Kernel32] WideCharToMultiByte: Read 257 chars (specified count)
info: Win32Emu.Emulator[0]
      [Di...
```

### Key Observations

1. **Parameters**: The call attempted to convert 257 wide characters to UTF-8
2. **Buffer Size**: Only 256 bytes of output buffer were provided
3. **Expected Behavior**: When converting 257 ASCII characters to UTF-8, the result requires at least 257 bytes (one byte per character)
4. **Problem**: 257 bytes > 256 bytes = buffer too small

### Root Cause Analysis

The `WideCharToMultiByte` implementation correctly handles this scenario:
1. Reads 257 wide characters from memory
2. Converts them to UTF-8 encoding
3. Checks if the result (257 bytes) fits in the buffer (256 bytes)
4. Since it doesn't fit, returns 0 and sets `ERROR_INSUFFICIENT_BUFFER`

This is the correct Windows API behavior according to MSDN documentation.

### Why the Log Was Truncated

The log truncation could be due to:
1. **Log output truncation**: The user may have copied only part of the log
2. **Emulator continuation**: The emulator continues after returning 0, and subsequent logs weren't included
3. **Application behavior**: CPU-Z may not handle `ERROR_INSUFFICIENT_BUFFER` correctly

The truncation at "[Di..." suggests the Dispatcher was about to log the return value (0) when the log was cut off.

## Changes Made

### 1. Enhanced Diagnostic Logging

Added comprehensive logging to `WideCharToMultiByte` function to help diagnose issues:

```csharp
// Before encoding
_logger.LogDebug("[Kernel32] WideCharToMultiByte: Converting with code page {ActualCodePage}", actualCodePage);

// After encoding
_logger.LogDebug("[Kernel32] WideCharToMultiByte: Conversion complete, got {BytesLength} bytes", multiByteBytes.Length);

// Buffer too small case
_logger.LogWarning("[Kernel32] WideCharToMultiByte: Buffer too small - need {NeedSize} bytes but only have {CbMultiByte}", multiByteBytes.Length, cbMultiByte);

// Before writing to buffer
_logger.LogDebug("[Kernel32] WideCharToMultiByte: Writing {BytesLength} bytes to 0x{LpMultiByteStr:X8}", multiByteBytes.Length, lpMultiByteStr);

// On success
_logger.LogInformation("[Kernel32] WideCharToMultiByte: Success, returning {BytesLength} bytes", (uint)multiByteBytes.Length);
```

### 2. New Test Case

Added `WideCharToMultiByte_WithInsufficientBuffer_ShouldReturnZero` test that:
- Creates a 257-character ASCII string
- Provides only 256 bytes of output buffer
- Verifies the function returns 0
- Verifies `GetLastError()` returns `ERROR_INSUFFICIENT_BUFFER` (122)

This test validates the exact scenario observed in the CPU-Z logs.

## Testing Results

- ✅ All 251 existing Kernel32 tests pass
- ✅ New insufficient buffer test passes
- ✅ CodeQL security scan: 0 vulnerabilities found
- ✅ Build succeeds with no errors

## Conclusion

The `WideCharToMultiByte` implementation is **working correctly** according to Windows API specifications. When the output buffer is too small:
1. Function returns 0
2. Sets last error to `ERROR_INSUFFICIENT_BUFFER` (122)
3. Does not write to the output buffer

### What This Means for CPU-Z

If CPU-Z crashes or hangs, the issue is likely in how CPU-Z's code handles the `ERROR_INSUFFICIENT_BUFFER` return. The correct handling should be:
1. Call `WideCharToMultiByte` with `cbMultiByte=0` to get required size
2. Allocate a buffer of the required size
3. Call `WideCharToMultiByte` again with the properly-sized buffer

### Benefits of This PR

1. **Better Diagnostics**: Enhanced logging will show exactly where execution is in the function
2. **Validated Behavior**: Test confirms the implementation handles buffer overflow correctly
3. **No Security Issues**: CodeQL scan confirms no vulnerabilities introduced
4. **No Regressions**: All existing tests continue to pass

If the issue persists when running CPU-Z, the enhanced logging will help pinpoint whether:
- The emulator crashes during encoding
- The emulator crashes after returning from the function
- The issue is in CPU-Z's handling of the error condition
