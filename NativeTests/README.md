# Native Win32 API Test Executables

This directory contains simple C test programs for testing Win32 API functions in Win32Emu. These tests can be compiled with Visual C++ and run on both real Windows and in the Win32Emu emulator.

## Purpose

These test executables are designed to:
- Test specific Win32 API functions that are problematic or require validation
- Compare behavior between real Windows and Win32Emu implementation
- Provide simple, focused tests that are easier to debug than complex applications
- Serve as integration tests that can be run manually on both platforms

## Test Programs

### test_getmodulefilename.exe
Tests the `GetModuleFileNameA` function with various scenarios:
- Getting current module filename with NULL handle
- Testing buffer size limitations
- Getting kernel32.dll module filename
- Testing invalid handles
- Edge cases with zero buffer sizes

### test_environment.exe
Tests environment variable functions:
- `GetEnvironmentVariableA` - Reading environment variables
- `SetEnvironmentVariableA` - Setting and updating environment variables
- Testing non-existent variables
- Testing buffer size handling
- Deleting environment variables with NULL values

## Building on Windows

### Using Visual Studio
1. Open the solution file `Win32Emu.slnx` in Visual Studio
2. The projects are configured to build for Win32 (x86)
3. Build configuration: Debug or Release
4. Output directory: `EXEs/NativeTests/Debug/` or `EXEs/NativeTests/Release/`

### Using Visual Studio Command Prompt
```cmd
cd NativeTests
cl test_getmodulefilename.c /Fe:test_getmodulefilename.exe
cl test_environment.c /Fe:test_environment.exe
```

### Using MinGW-w64 on Windows
```cmd
cd NativeTests
gcc -o test_getmodulefilename.exe test_getmodulefilename.c -lkernel32
gcc -o test_environment.exe test_environment.c -lkernel32
```

## Building on Linux (Cross-compilation)

You can cross-compile these tests on Linux using MinGW-w64:

```bash
cd NativeTests
i686-w64-mingw32-gcc -o test_getmodulefilename.exe test_getmodulefilename.c -lkernel32
i686-w64-mingw32-gcc -o test_environment.exe test_environment.c -lkernel32
```

Or use the provided Makefile:
```bash
cd NativeTests
make
```

## Running the Tests

### On Real Windows
```cmd
cd EXEs\NativeTests\Release
test_getmodulefilename.exe
test_environment.exe
```

### In Win32Emu
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_getmodulefilename.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_environment.exe
```

## Expected Behavior

### GetModuleFileNameA Tests
- All tests should PASS on real Windows
- Win32Emu behavior should match Windows behavior:
  - NULL handle returns current module path
  - Small buffers return truncated paths with ERROR_INSUFFICIENT_BUFFER
  - Invalid handles return 0 with ERROR_INVALID_PARAMETER
  - Valid module handles return full paths

### Environment Variable Tests
- All tests should PASS on real Windows
- Win32Emu should use virtualized environment variables:
  - Variables set in the emulator don't affect the host OS
  - GetEnvironmentVariableA/SetEnvironmentVariableA work within the emulated environment
  - Non-existent variables return ERROR_ENVVAR_NOT_FOUND
  - Null value deletes the variable

## Comparing Results

To compare results between Windows and Win32Emu:

1. Run the tests on Windows and save output:
```cmd
test_getmodulefilename.exe > windows_getmodulefilename.txt
test_environment.exe > windows_environment.txt
```

2. Run the tests in Win32Emu and save output:
```bash
dotnet run --no-build --project Win32Emu.Gui -- --nogui test_getmodulefilename.exe > emu_getmodulefilename.txt
dotnet run --no-build --project Win32Emu.Gui -- --nogui test_environment.exe > emu_environment.txt
```

3. Compare the outputs to identify any discrepancies

## Known Differences

### Path Separators
- Windows uses backslashes (`\`) in paths
- Win32Emu may use forward slashes (`/`) or backslashes depending on configuration
- This is expected and not considered a failure

### Environment Variables
- Win32Emu uses a virtualized environment
- Default variables (like PATH, WINDIR) will have emulated values
- This is by design to isolate the emulated environment from the host

## Adding New Tests

To add a new test:
1. Create a new `.c` file in this directory
2. Create a corresponding `.vcxproj` file (copy and modify an existing one)
3. Add the project to the solution if desired
4. Update this README with test description
5. Ensure the test can run on both Windows and Win32Emu

## Related Issues

These tests were created to help validate the implementation of functions mentioned in issue #650, particularly:
- GetModuleFileNameA (and related module functions)
- GetEnvironmentVariableA / SetEnvironmentVariableA
- Other Kernel32 functions that are problematic in the current implementation
