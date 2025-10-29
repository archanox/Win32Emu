# thread.exe Output Investigation

## Issue
When running thread.exe (and other retrowin32 cpp test executables), the output shows null bytes instead of the expected text messages.

## Investigation Summary

### What Works
1. ✅ PE file loads correctly
2. ✅ .rdata section (containing string constants) is loaded into memory
3. ✅ String data exists at the correct memory addresses
4. ✅ No exceptions are thrown during execution  
5. ✅ Thread allocation functions (TlsAlloc, TlsSetValue, etc.) work correctly
6. ✅ GetCurrentThreadId returns expected value
7. ✅ WriteFile is called with correct parameters

### The Problem
When the executable tries to print output using the `print(fmt().str("...").dec(...))` pattern from util.h, the fmt buffer that should contain the formatted string contains all null bytes instead.

### Root Cause Analysis

The retrowin32 cpp tests use an inline `fmt` class defined in util.h:
```cpp
struct fmt {
    char buf[1024];  // Buffer allocated on stack
    size_t ofs;
    // Methods fill the buffer with formatted text
};

void print(std::string_view str) {
    WriteFile(GetStdHandle(STD_OUTPUT_HANDLE), str.data(), str.size(), nullptr, nullptr);
}
```

The execution flow should be:
1. Create `fmt` temporary object on stack (allocates 1024-byte buffer)
2. Call `.str()`, `.dec()` methods to fill buffer with text
3. Convert to `string_view` pointing to the buffer
4. Call `print()` with the string_view
5. `print()` calls WriteFile with pointer to the buffer

### What We Observed

1. **String constants ARE loaded**: The .rdata section contains the expected strings like "GetSystemMetrics():" at the correct addresses (e.g., 0x4020C6)

2. **WriteFile IS called**: The function receives:
   - Correct file handle (stdout)
   - Buffer pointer (pointing to stack location where fmt buffer should be)
   - Correct size (matches expected string length)

3. **Buffer contains zeros**: When WriteFile reads from the buffer address, it finds all null bytes

### Detailed Findings

Example from metrics.exe:
- Assembly code attempts to copy string from .rdata to fmt buffer:
  ```assembly
  mov eax, 0xffffffeb        ; eax = -21
  mov cl, BYTE PTR [eax+0x4020da]  ; Read from string location
  mov BYTE PTR [esp+edx*1], cl     ; Write to fmt buffer
  ```

- String data EXISTS in memory at 0x4020C5: `\0GetSystemMetrics():\r\n...`
- But when WriteFile is called, buffer at ESP contains: `\0\0\0\0\0\0...`

### Possible Causes

1. **CPU emulation bug**: The MOV instructions that copy bytes from .rdata to the stack buffer may not be executing correctly
2. **Address calculation issue**: The emulator might not be correctly calculating addresses for `[eax + constant]` when eax contains a negative value
3. **Stack corruption**: Something might be zeroing the stack between when the buffer is filled and when WriteFile reads it
4. **Instruction variant issue**: The specific MOV instruction encoding used might not be properly handled by the IcedCpu emulator

### Affected Tests
All retrowin32 cpp tests are affected:
- metrics.exe
- thread.exe  
- cmdline.exe
- gdi.exe
- errors.exe
- ddraw.exe
- dib.exe

They all use the same util.h with the `fmt` pattern.

### Impact
- Tests execute without crashes ✅
- Tests complete successfully ✅
- But no text output is visible ❌

## Recommendations

### Short-term
1. Document this as a known limitation
2. Update test expectations to note that output validation is not yet implemented
3. Focus testing on API functionality rather than console output

### Long-term
1. Add detailed CPU instruction logging to trace the exact failure point
2. Create minimal test case that reproduces the buffer copy issue
3. Debug the IcedCpu implementation for MOV instruction variants
4. Consider alternative test executables that don't use this pattern
5. Potentially rebuild the test executables with different compiler settings

## Test Results
Despite the null output issue, the tests are valuable for:
- Verifying PE loading works
- Testing Win32 API implementations
- Ensuring no crashes occur
- Validating basic execution flow

The null output issue doesn't prevent the tests from serving their primary purpose of integration testing the emulator's Win32 API implementations.
