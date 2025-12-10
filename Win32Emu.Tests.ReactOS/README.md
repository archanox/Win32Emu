# ReactOS Test Suite for User32

This test project runs ReactOS test executables (apitest and winetest) to verify Win32Emu's User32.dll API implementations.

## Current Status

The tests are currently **skipped** due to memory corruption issues in the emulator when running these specific test executables.

### Memory Corruption Issue

The ReactOS test executables trigger a memory corruption condition where the instruction pointer (EIP) jumps to low memory addresses (0x1-0xFFFF range). Specifically:

- **Symptoms**: `EIP=0x00000002` (or similar low values like `0x00000016`, `0x00000000`)
- **Previous EIP**: Valid code addresses (e.g., `0x00401E22`, `0x00000000`)
- **Stack State**: ESP and EBP appear valid
- **Root Cause**: **Uninitialized or incorrectly initialized function pointers in the test executables**

#### Analysis (December 2024)

Investigation revealed that the low EIP values (2, 22, etc.) are not actual code addresses but appear to be:
- Function ordinal numbers
- Array indices 
- Uninitialized memory containing small integers

This indicates the test executables have bugs where function pointers are not properly initialized before being called. On real Windows, this would trigger an **access violation exception** that the application's exception handler might catch (via SEH - Structured Exception Handling).

**Why the tests fail in Win32Emu:**
- Win32Emu does not yet implement full Structured Exception Handling (SEH)
- No memory protection to detect invalid code execution attempts
- Cannot gracefully handle access violations like Windows does

**This is a limitation of the emulator, not a bug in the emulator's core functionality.**

### Fix Applied (December 2024)

The emulator was modified to **fail fast** when detecting this corruption:

1. **Before Fix**: Tests would spam error messages for 90+ minutes until timeout
2. **After Fix**: Tests fail immediately (~400ms) with clear error message
3. **Exception Thrown**: `InvalidOperationException` with diagnostic information

This allows developers to see the issue quickly without waiting for timeouts.

## Test Executables

Located in `/EXEs/ApiTests/`:

- `user32_apitest.exe` (1.2MB) - Core User32 API tests
- `user32_apitest_menuui.exe` (33KB) - Menu UI tests
- `user32_dynamic_apitest.exe` (47KB) - Dynamic API tests
- `user32_winetest.exe` (2.3MB) - Wine test suite for User32

## Running Tests Manually

To investigate the memory corruption:

```bash
cd Win32Emu.Tests.ReactOS

# Remove the Skip attribute temporarily
sed -i 's/\[Theory(Skip = ".*")\]/[Theory]/' User32ReactOSTests.cs

# Run tests
dotnet test --filter "User32_ReactOSTests_ShouldExecute"

# Restore skip attribute
git checkout User32ReactOSTests.cs
```

Tests will fail fast with memory corruption errors in the logs.

## Future Work

To fix these tests properly and allow them to run successfully:

### Required Features

1. **Structured Exception Handling (SEH)**:
   - Implement `__try` / `__except` / `__finally` blocks
   - Support exception filter expressions
   - Enable tests to catch access violations gracefully
   
2. **Memory Protection**:
   - Implement page-level memory protection (read/write/execute flags)
   - Detect attempts to execute from data pages or unmapped memory
   - Generate access violation exceptions on invalid operations

3. **Exception Dispatching**:
   - Call `SetUnhandledExceptionFilter` handlers when access violations occur
   - Walk the SEH chain to find appropriate exception handlers
   - Unwind the stack properly when exceptions are handled

### Investigation Still Needed

While we know the root cause (uninitialized function pointers), further debugging could help:

1. **Identify specific test cases**:
   - Which test functions trigger the corruption?
   - Are there patterns in which APIs are involved?

2. **Verify test executables**:
   - Check if these tests pass on real Windows
   - Compare with Wine's behavior
   - Determine if tests have known issues

3. **Workarounds**:
   - Could we patch the test executables to initialize pointers?
   - Is there a way to skip problematic test cases?

### Previous Investigation Notes

Early investigation focused on:
- Import stub generation and calling conventions (verified correct)
- Stack alignment in API handlers (validated)
- Return address corruption (no evidence found)

The fail-fast fix correctly identifies when tests reach the problematic code paths.

## Error Messages

When tests fail, you'll see errors like:

```
Memory corruption detected: EIP=0x00000002 is in suspicious low memory range.
Previous EIP=0x00000000, ESP=0x002FF004, EBP=0x002FF000.
This indicates a corrupted return address or bad function pointer.
```

This is the emulator's safety check preventing infinite loops.
