# PR Summary: Native Win32 API Test Executables

## Description

This PR adds small, focused native Win32 API test executables that can be compiled with Visual C++ and run on both real Windows and Win32Emu. These tests address the request in issue #650 for simple test programs to validate Win32 API implementations, particularly for `GetModuleFileNameA` and environment variable functions.

## Changes

### New Test Executables (NativeTests/)

1. **test_getmodulefilename.c** - Tests GetModuleFileNameA with 5 test cases:
   - NULL handle (current module)
   - Buffer size limitations
   - Valid module handles (kernel32.dll)
   - Zero buffer size edge case
   - Invalid handles

2. **test_environment.c** - Tests environment variable functions with 8 test cases:
   - GetEnvironmentVariableA (existing and non-existent variables)
   - SetEnvironmentVariableA (create, update, delete)
   - Buffer size handling
   - Edge cases (empty names, NULL buffers)

### Build System

- **Visual C++ Projects** (.vcxproj) - For building on Windows with Visual Studio
- **Makefile** - For cross-compilation with MinGW-w64 on Linux
- **Pre-built Executables** - Included in EXEs/NativeTests/ directory

### Documentation

- **NativeTests/README.md** - Technical documentation for the tests
- **docs/guides/NATIVE_TESTS_GUIDE.md** - User guide for running and comparing tests
- **docs/implementation/NATIVE_TESTS_IMPLEMENTATION.md** - Implementation details

### Integration

- Updated Win32Emu.slnx to include NativeTests solution folder

## Benefits

1. **Focused Testing** - Small, targeted tests for specific problematic APIs
2. **Cross-Platform** - Run on both Windows and Win32Emu for comparison
3. **Easy to Extend** - Simple pattern for adding new tests
4. **Well Documented** - Clear instructions for building, running, and interpreting results
5. **Reproducible** - Consistent test cases with PASS/FAIL indicators

## How to Use

### Building
```bash
# On Linux with MinGW
cd NativeTests
make

# On Windows with Visual Studio
cd NativeTests
cl test_getmodulefilename.c /Fe:test_getmodulefilename.exe
```

### Running on Windows
```cmd
cd EXEs\NativeTests
test_getmodulefilename.exe
test_environment.exe
```

### Running in Win32Emu
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_getmodulefilename.exe
```

## Testing

- ✅ Successfully compiled with MinGW-w64
- ✅ Valid PE32 executables created
- ✅ Emulator loads and runs the executables (output is verbose but functional)
- ⏳ Testing on real Windows (awaiting Windows environment)

## Related Issues

- Addresses #650 - Request for basic test executables for problematic functions

## Files Added/Modified

**Added:**
- NativeTests/test_getmodulefilename.c
- NativeTests/test_getmodulefilename.vcxproj
- NativeTests/test_environment.c
- NativeTests/test_environment.vcxproj
- NativeTests/Makefile
- NativeTests/README.md
- docs/guides/NATIVE_TESTS_GUIDE.md
- docs/implementation/NATIVE_TESTS_IMPLEMENTATION.md
- EXEs/NativeTests/test_getmodulefilename.exe
- EXEs/NativeTests/test_environment.exe

**Modified:**
- Win32Emu.slnx

## Notes

The test executables are intentionally kept small and simple to make debugging easier. Each test prints clear output with PASS/FAIL status, making it easy to identify discrepancies between Windows and Win32Emu behavior.

The emulator successfully loads and runs these tests, though the output includes verbose logging. Users can filter the output to see just the test results by grepping for "Testing" in the output.

## Future Enhancements

- Add more tests for other functions mentioned in #650
- Create automated comparison scripts
- Add CI integration for automated testing
- Add Unicode (W suffix) versions of tests
