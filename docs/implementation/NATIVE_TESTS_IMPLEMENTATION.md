# Native Test Executables Implementation Summary

## Overview

This document summarizes the implementation of native Win32 API test executables for Win32Emu, created in response to the request for small, focused test programs similar to winetests but simpler.

## What Was Created

### 1. Test Executables

Two C test programs were created in the `NativeTests/` directory:

#### test_getmodulefilename.c
Tests `GetModuleFileNameA` with various scenarios:
- NULL handle (gets current module path)
- Small buffer size (tests truncation and ERROR_INSUFFICIENT_BUFFER)
- Valid module handles (kernel32.dll)
- Zero buffer size edge case
- Invalid handle (0xFFFFFFFF) with ERROR_INVALID_PARAMETER

**Key Features:**
- 5 comprehensive test cases
- Clear PASS/FAIL indicators
- Detailed output showing parameters, results, and error codes
- ~80 lines of clean, documented C code

#### test_environment.c
Tests environment variable functions:
- `GetEnvironmentVariableA` - Read existing and non-existent variables
- `SetEnvironmentVariableA` - Create, update, and delete variables
- Buffer size handling (insufficient buffer tests)
- NULL buffer to query required size
- Edge cases (empty variable names)

**Key Features:**
- 8 comprehensive test cases
- Tests virtualization (Win32Emu shouldn't affect host environment)
- Error code validation (ERROR_ENVVAR_NOT_FOUND, ERROR_INSUFFICIENT_BUFFER)
- ~150 lines of clean, documented C code

### 2. Build System

#### Visual C++ Projects
Created `.vcxproj` files for both tests:
- Compatible with Visual Studio 2022 (v143 toolset)
- Configurable for Debug and Release builds
- Target: Win32 (32-bit) platform
- Output: `EXEs/NativeTests/Debug/` or `EXEs/NativeTests/Release/`

#### Cross-Compilation Makefile
Created `Makefile` for building on Linux with MinGW-w64:
- Cross-compiles to 32-bit Windows executables
- Simple targets: `make`, `make clean`, `make help`
- Automatic output directory creation
- Builds successfully on Ubuntu 24.04 with `i686-w64-mingw32-gcc`

### 3. Documentation

#### NativeTests/README.md
Comprehensive documentation covering:
- Purpose and goals of the test executables
- Building on Windows (Visual Studio, cl.exe, MinGW)
- Building on Linux (cross-compilation)
- Running tests on Windows and in Win32Emu
- Expected behavior for each test
- Known differences between platforms
- How to compare results
- Instructions for adding new tests

#### docs/guides/NATIVE_TESTS_GUIDE.md
User guide for working with the tests:
- Step-by-step instructions for running tests
- Output interpretation guide
- Comparing Windows vs Win32Emu results
- Troubleshooting common issues
- Best practices for writing new tests
- Examples of expected output

### 4. Integration

#### Solution File Updates
Updated `Win32Emu.slnx` to include:
- New solution folder: `Solution Items/NativeTests/`
- All test source files (`.c`)
- Visual C++ project files (`.vcxproj`)
- README and Makefile

### 5. Compiled Executables

Pre-built executables included in `EXEs/NativeTests/`:
- `test_getmodulefilename.exe` (~235 KB)
- `test_environment.exe` (~236 KB)
- Built with MinGW-w64 on Linux
- PE32 format, 32-bit Intel 80386 executables
- Console subsystem

## Technical Details

### Compilation
Successfully compiled with:
- **Compiler**: `i686-w64-mingw32-gcc` (MinGW-w64)
- **Flags**: `-Wall -Wextra -O2 -m32`
- **Linking**: `-lkernel32`
- **Architecture**: PE32 (32-bit Windows)

### File Sizes
- Source files: ~2-5 KB each
- Executables: ~235-240 KB each (includes MinGW C runtime)
- Project files: ~4 KB each
- Documentation: ~5-6 KB total

### Testing Status
- ✅ Compilation successful on Linux
- ✅ Executables created as valid PE32 files
- ⏳ Win32Emu testing (emulator runs but output verbose - needs filtering)
- ⏳ Real Windows testing (requires Windows system)

## How to Use

### On Windows
```cmd
cd EXEs\NativeTests
test_getmodulefilename.exe
test_environment.exe
```

### In Win32Emu
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_getmodulefilename.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/test_environment.exe
```

### Building from Source
```bash
cd NativeTests
make
```

Or on Windows with Visual Studio installed:
```cmd
cd NativeTests
cl test_getmodulefilename.c /Fe:test_getmodulefilename.exe
cl test_environment.c /Fe:test_environment.exe
```

## Benefits

### For Win32Emu Development
1. **Focused Testing** - Small, targeted tests for specific APIs
2. **Easy Debugging** - Simple code paths make issues easier to trace
3. **Cross-Platform** - Run on both Windows and Win32Emu for comparison
4. **Reproducible** - Consistent test cases with clear outputs
5. **Extensible** - Easy to add new tests following the same pattern

### For Issue #650
Addresses the request by providing:
- Test executables for problematic functions (GetModuleFileNameA, environment variables)
- Ability to run on real Windows for comparison
- Visual C++ projects for building on Windows
- Small, focused tests unlike complex winetests
- Clear documentation for usage and interpretation

## Addressing Issue #650 Concerns

The issue specifically mentioned:
- ✅ "Can we create some basic test executables, like the winetests, but smaller"
- ✅ "Can you focus on the functions on #650" (GetModuleFileNameA, environment variables)
- ✅ "I'd like to be able to run them on a real windows system too"
- ✅ "Maybe just create them in the solution as visual C/C++ projects"
- ✅ "It'll help paint any possible issues we have with our implementation"

All requirements met with:
- Small, focused test executables (80-150 lines each)
- Tests for GetModuleFileNameA and environment variable functions
- Can run on real Windows
- Visual C++ project files included
- Clear output showing PASS/FAIL and parameters for debugging

## Future Enhancements

Potential additions:
1. More test functions from issue #650 (if there are others)
2. Unicode (W suffix) versions of the tests
3. Automated comparison scripts
4. CI integration to run tests automatically
5. Performance benchmarking
6. Coverage for more edge cases

## Files Created/Modified

### New Files
- `NativeTests/test_getmodulefilename.c` - Test source code
- `NativeTests/test_getmodulefilename.vcxproj` - Visual C++ project
- `NativeTests/test_environment.c` - Test source code
- `NativeTests/test_environment.vcxproj` - Visual C++ project
- `NativeTests/Makefile` - Build automation
- `NativeTests/README.md` - Technical documentation
- `docs/guides/NATIVE_TESTS_GUIDE.md` - User guide
- `EXEs/NativeTests/test_getmodulefilename.exe` - Compiled executable
- `EXEs/NativeTests/test_environment.exe` - Compiled executable

### Modified Files
- `Win32Emu.slnx` - Added NativeTests solution folder

## Conclusion

This implementation provides a solid foundation for testing Win32 API functions in Win32Emu by creating small, focused test executables that can run on both Windows and the emulator. The tests are well-documented, easy to build, and designed to help identify implementation issues through comparison of behavior between the two platforms.

The approach is extensible, allowing for easy addition of new tests as needed, and provides clear, actionable output that helps developers understand what's working and what needs attention.
