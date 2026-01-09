# Guide: Using Native Test Executables

This guide explains how to use the native Win32 API test executables to compare behavior between real Windows and Win32Emu.

## Overview

The native test executables in the `NativeTests/` directory are simple C programs that test specific Win32 API functions. They can be run on both real Windows systems and in Win32Emu to compare behavior and identify discrepancies.

## Available Tests

### test_getmodulefilename.exe
Tests the `GetModuleFileNameA` function:
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

## Running Tests

### On Windows

#### Console/Command Prompt
```cmd
cd EXEs\NativeTests
test_getmodulefilename.exe
test_environment.exe
```

#### PowerShell
```powershell
cd EXEs\NativeTests
.\test_getmodulefilename.exe
.\test_environment.exe
```

#### Save Output to File
```cmd
test_getmodulefilename.exe > windows_getmodulefilename.txt 2>&1
test_environment.exe > windows_environment.txt 2>&1
```

### In Win32Emu

#### From the Repository Root
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_getmodulefilename.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_environment.exe
```

#### Save Output (filtering emulator logs)
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_getmodulefilename.exe 2>&1 | \
  grep -A 10000 "Testing GetModuleFileNameA" > emu_getmodulefilename.txt

dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_environment.exe 2>&1 | \
  grep -A 10000 "Testing Environment" > emu_environment.txt
```

## Comparing Results

### Side-by-Side Comparison
1. Run tests on Windows and save output
2. Run tests in Win32Emu and save output
3. Use a diff tool to compare:
```bash
diff windows_getmodulefilename.txt emu_getmodulefilename.txt
```

### Expected Differences

Some differences are expected and normal:

#### Path Formats
- **Windows**: Uses backslashes (`C:\Program Files\App.exe`)
- **Win32Emu**: May use forward slashes or backslashes depending on configuration

#### Environment Variables
- **Windows**: Returns actual system environment variables
- **Win32Emu**: Uses virtualized environment with emulated values (e.g., `WINDIR=C:\WINDOWS`)

#### Module Paths
- **Windows**: Returns actual file system paths
- **Win32Emu**: Returns paths within the virtual file system (VFS)

## Interpreting Test Results

Each test prints:
- Test name and description
- Function call parameters
- Return value
- LastError code (if applicable)
- Status: PASS or FAIL

### Example Output
```
Test 1: GetModuleFileNameA with NULL handle
  Result: 42
  Buffer: C:\MyProgram\test.exe
  LastError: 0
  Status: PASS
```

### What to Look For

#### Success Indicators
- Return values match expected behavior
- Error codes are correct (ERROR_INSUFFICIENT_BUFFER, ERROR_INVALID_PARAMETER, etc.)
- String outputs are properly null-terminated
- Buffer handling works correctly

#### Failure Indicators
- Unexpected return values (e.g., 0 when success expected)
- Wrong error codes
- Crashes or hangs
- Truncated or corrupted strings

## Troubleshooting

### Tests Don't Run in Win32Emu
1. Ensure Win32Emu is built: `dotnet build Win32Emu.Gui -c Release`
2. Check that test executables exist in `EXEs/NativeTests/`
3. Try with `--debug` flag for more verbose output

### Tests Crash or Hang
1. Run with debugger enabled: `--interactive-debug`
2. Check emulator logs for error messages
3. Compare with Windows behavior to identify the problematic API call

### Output is Too Verbose
When running in Win32Emu, filter the output to just the test results:
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui test.exe 2>&1 | \
  grep -A 10000 "Testing"
```

## Adding Your Own Tests

To create a new test executable:

1. **Create a C source file** in `NativeTests/`:
```c
#include <windows.h>
#include <stdio.h>

int main(void) {
    printf("Testing MyFunction\n");
    printf("==================\n\n");
    
    // Your test code here
    
    return 0;
}
```

2. **Create a .vcxproj file** (copy and modify an existing one):
   - Update ProjectGuid
   - Update RootNamespace
   - Update source file reference

3. **Update the Makefile** to include your new test:
```makefile
SOURCES = test_getmodulefilename.c test_environment.c your_new_test.c
```

4. **Build and test**:
```bash
cd NativeTests
make
```

5. **Run on both platforms** and compare results

## Best Practices

### Writing Tests
- **Keep tests simple and focused** - one function or scenario per test
- **Print clear output** - include test names, parameters, and expected vs actual
- **Test edge cases** - NULL pointers, zero sizes, invalid handles
- **Use PASS/FAIL** - make it easy to see what succeeded

### Comparing Results
- **Document expected differences** - note why paths or values differ
- **Focus on behavior** - the API should behave the same even if values differ
- **Report discrepancies** - file issues for unexpected differences

### Cross-Platform Testing
- **Test on real Windows** - this is your ground truth
- **Test in Win32Emu** - compare behavior
- **Use a VM if needed** - for testing on Linux/macOS without Windows access

## Related Documentation

- [NativeTests README](../NativeTests/README.md) - Detailed information about the test executables
- [Win32Emu README](../README.md) - Main project documentation
- [Testing Guide](../README.Tests.md) - Overall testing strategy

## Contributing

When adding new tests:
1. Follow the existing pattern and style
2. Update the README files
3. Test on both Windows and Win32Emu
4. Document any expected differences
5. Submit a pull request with your changes
