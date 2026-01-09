# Quick Start: Native Test Executables

This is a quick reference for using the native Win32 API test executables.

## What Are These Tests?

Small C programs that test Win32 API functions. They can run on:
- Real Windows systems
- Win32Emu emulator

Perfect for comparing behavior and finding bugs!

## Available Tests

| Test | What It Tests |
|------|--------------|
| `test_getmodulefilename.exe` | `GetModuleFileNameA` - Getting module file paths |
| `test_environment.exe` | Environment variable functions (`Get/SetEnvironmentVariableA`) |

## Quick Commands

### Build the Tests (Linux)
```bash
cd NativeTests
make
```

### Run on Windows
```cmd
cd EXEs\NativeTests
test_getmodulefilename.exe
test_environment.exe
```

### Run in Win32Emu (Linux/Mac)
```bash
# First, build the emulator if not already built
dotnet build Win32Emu.Gui -c Release

# Then run the tests
cd /path/to/Win32Emu
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_getmodulefilename.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_environment.exe
```

### Filter Emulator Output
The emulator is verbose. To see just the test output:
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_getmodulefilename.exe 2>&1 | grep -A 1000 "Testing GetModuleFileNameA"
```

## Reading Test Output

Each test shows:
```
Test 1: [Description]
  Result: [Number]         ← Return value from the function
  Buffer: [String]         ← Output buffer content
  LastError: [Number]      ← Error code (if any)
  Status: PASS or FAIL     ← Did it work as expected?
```

**PASS** = Test worked correctly  
**FAIL** = Something went wrong

## Common Issues

### "No such file or directory"
The executable isn't in the right place. Make sure you're in the correct directory or use the full path.

### Emulator Output Too Verbose
Use `grep` to filter:
```bash
dotnet run ... 2>&1 | grep -A 1000 "Testing"
```

### Tests Don't Build on Linux
Install MinGW:
```bash
sudo apt-get install gcc-mingw-w64-i686
```

## What to Look For

When comparing Windows vs Win32Emu:

### Same (Good!)
- Return values
- Error codes
- Pass/Fail status
- General behavior

### Different (Expected)
- Exact file paths (Windows: `C:\Windows\...` vs Emulator: virtual paths)
- Environment variable values (Emulator uses virtualized environment)

### Different (Bad - File a Bug!)
- Wrong return values
- Crashes
- Incorrect error codes
- Functions that should pass but fail

## More Information

- **Detailed Guide**: [docs/guides/NATIVE_TESTS_GUIDE.md](../guides/NATIVE_TESTS_GUIDE.md)
- **Test Documentation**: [NativeTests/README.md](../../NativeTests/README.md)
- **Implementation Details**: [docs/implementation/NATIVE_TESTS_IMPLEMENTATION.md](../implementation/NATIVE_TESTS_IMPLEMENTATION.md)

## Adding Your Own Test

1. Copy an existing test (e.g., `test_getmodulefilename.c`)
2. Modify it to test your function
3. Add it to the Makefile
4. Run `make` to build
5. Test on both Windows and Win32Emu

Keep it simple! The goal is focused, easy-to-debug tests.

## Help

If you find a bug or have questions:
1. Check the detailed documentation (links above)
2. File an issue on GitHub
3. Include both Windows and Win32Emu output for comparison
