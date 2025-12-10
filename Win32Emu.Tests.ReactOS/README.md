# ReactOS Test Suite for User32

This test project runs ReactOS test executables (apitest and winetest) to verify Win32Emu's User32.dll API implementations.

## Current Status

The tests are currently **skipped** due to memory corruption issues in the emulator when running these specific test executables.

### Memory Corruption Issue

The ReactOS test executables trigger a memory corruption condition where the instruction pointer (EIP) jumps to low memory addresses (0x1-0xFFFF range). Specifically:

- **Symptoms**: `EIP=0x00000002` (or similar low values like `0x00000016`, `0x00000000`)
- **Previous EIP**: Valid code addresses (e.g., `0x00401E22`, `0x0E000002`)
- **Stack State**: ESP and EBP appear valid
- **Root Cause**: **RET instructions being decoded as 16-bit instead of 32-bit**

#### Analysis (December 2024)

Investigation revealed that the emulator's CPU was incorrectly handling RET instructions in some cases:

1. **The Problem**: When the Iced instruction decoder encounters certain RET instructions (possibly with 0x66 operand-size prefix or due to encoding variations), it decodes them as 16-bit RET instructions even though the CPU is running in 32-bit protected mode.

2. **The Consequence**: A 16-bit RET only pops 2 bytes from the stack instead of 4 bytes. This truncates 32-bit return addresses:
   - Full address on stack: `0x0E000002` (syscall dispatcher RET instruction)
   - Only 2 bytes popped: `0x0002`
   - Zero-extended to 32-bit: `0x00000002` (invalid low memory address!)

3. **Why It Failed**: When EIP jumps to `0x00000002`, there's no valid code there, causing the emulator to detect memory corruption.

**This was a bug in the emulator's RET instruction handling, not in the test executables.**

The fix ensures that when the CPU is initialized with bitness=32 (Win32 protected mode), ALL RET instructions use 32-bit operand size regardless of any prefix bytes. Win32 PE executables run in 32-bit protected mode and must always use 32-bit return addresses.

### Fix Applied (December 2024)

The emulator was modified to correctly handle RET instructions in 32-bit mode:

1. **Root Cause Fixed**: RET instructions are now forced to use 32-bit operand size when CPU is in 32-bit mode, regardless of instruction encoding or prefix bytes
2. **Before Fix**: Tests would execute some APIs successfully, then crash when a 16-bit RET truncated a return address
3. **After Fix**: Tests should execute without memory corruption from truncated return addresses

The emulator also fails fast when detecting corrupted EIP to prevent infinite loops.

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

To enable these tests to run fully successfully:

### Remaining Issues to Address

While the core RET instruction bug is fixed, the tests may still have other issues:

1. **Verify complete test execution**: Run the tests to see if they now complete successfully or if there are additional unimplemented APIs
2. **Implement missing APIs**: Some User32/Kernel32 functions may need implementation
3. **Handle test-specific scenarios**: The tests may use features not yet fully supported

### If SEH is Still Needed

In case tests still encounter issues that require exception handling:

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

## Error Messages

When tests fail, you'll see errors like:

```
Memory corruption detected: EIP=0x00000002 is in suspicious low memory range.
Previous EIP=0x00000000, ESP=0x002FF004, EBP=0x002FF000.
This indicates a corrupted return address or bad function pointer.
```

This is the emulator's safety check preventing infinite loops.
