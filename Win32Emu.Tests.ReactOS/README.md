# ReactOS Test Suite for User32

This test project runs ReactOS test executables (apitest and winetest) to verify Win32Emu's User32.dll API implementations.

## Current Status

The tests are currently **skipped** due to memory corruption issues in the emulator when running these specific test executables.

### Memory Corruption Issue

The ReactOS test executables trigger a memory corruption condition where the instruction pointer (EIP) jumps to low memory addresses (0x1-0xFFFF range). Specifically:

- **Symptoms**: `EIP=0x00000002` (or similar low values like `0x00000016`)
- **Previous EIP**: Valid code addresses (e.g., `0x00401E22`)
- **Stack State**: ESP and EBP appear valid
- **Root Cause**: Unknown - likely a bug in import stub handling, calling convention mismatch, or stack corruption

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

To fix these tests properly:

1. **Debug the corruption source**:
   - Enable instruction tracing around the corruption point
   - Check import stub generation and calling conventions
   - Verify stack alignment in API handlers

2. **Possible root causes**:
   - Incorrect stack cleanup in API stubs
   - Calling convention mismatch (stdcall vs cdecl)
   - Function pointer corruption in vtables
   - Return address overwrite

3. **Investigation tools**:
   - Enable `debugMode: true` in test runner
   - Use GDB server mode for detailed debugging
   - Add memory watchpoints on stack regions

## Error Messages

When tests fail, you'll see errors like:

```
Memory corruption detected: EIP=0x00000002 is in suspicious low memory range.
Previous EIP=0x00000000, ESP=0x002FF004, EBP=0x002FF000.
This indicates a corrupted return address or bad function pointer.
```

This is the emulator's safety check preventing infinite loops.
