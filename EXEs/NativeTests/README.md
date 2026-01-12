# Pre-built Native Test Executables

This directory contains pre-built 32-bit Windows executables for testing Win32 API functions used by ign_teas.exe.

## Executables

All executables are PE32 (32-bit Windows) format, compiled with MinGW-w64 (GCC 13-win32) with -O2 optimization:

| Executable | Size | Tests | Functions Tested |
|------------|------|-------|------------------|
| `test_heap.exe` | ~242KB | 10 | HeapCreate, HeapAlloc, HeapFree, HeapDestroy |
| `test_virtualalloc.exe` | ~242KB | 10 | VirtualAlloc, VirtualFree, VirtualQuery |
| `test_fileio.exe` | ~243KB | 12 | CreateFileA, ReadFile, WriteFile, SetFilePointer, GetFileType, CloseHandle |
| `test_version.exe` | ~242KB | 10 | GetVersion, GetACP, GetCPInfo, GetStdHandle, SetHandleCount |
| `test_commandline.exe` | ~242KB | 8 | GetCommandLineA, GetStartupInfoA |
| `test_procaddress.exe` | ~241KB | 14 | GetModuleHandleA, GetProcAddress, IsProcessorFeaturePresent |
| `test_messages.exe` | ~244KB | 12 | RegisterClassA, CreateWindowExA, PeekMessageA, GetMessageA, etc. |
| `test_multimedia.exe` | ~242KB | 10 | timeBeginPeriod, timeEndPeriod, timeGetTime |
| `test_getmodulefilename.exe` | ~240KB | 5 | GetModuleFileNameA |
| `test_environment.exe` | ~242KB | 8 | GetEnvironmentVariableA, SetEnvironmentVariableA |

## Running on Windows

Simply double-click any executable or run from command prompt:
```cmd
test_heap.exe
test_virtualalloc.exe
test_fileio.exe
```

## Running in Win32Emu

```bash
# From repository root
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_heap.exe
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_virtualalloc.exe
```

## Running on Mobile/ARM Windows

These 32-bit x86 executables should work on ARM-based Windows devices with x86 emulation support (like Windows 11 on ARM).

## Running with Wine (Linux/macOS)

```bash
wine EXEs/NativeTests/test_heap.exe
wine EXEs/NativeTests/test_virtualalloc.exe
```

## Expected Output

Each test outputs PASS/FAIL status for individual test cases:
```
Testing Heap Functions
======================

Test 1: HeapCreate with default settings
  Result: 0x0b8f0000
  LastError: 0
  Status: PASS

Test 2: HeapAlloc - allocate 1024 bytes
  Result: 0x0b8f0498
  LastError: 0
  Status: PASS

...
```

## Rebuilding

If you want to rebuild these executables, see the Makefile in the parent directory:
```bash
cd ../NativeTests
make clean
make
```

## Source Code

Source code for all tests is in `/NativeTests/`:
- `test_heap.c`
- `test_virtualalloc.c`
- `test_fileio.c`
- `test_version.c`
- `test_commandline.c`
- `test_procaddress.c`
- `test_messages.c`
- `test_multimedia.c`
- `test_getmodulefilename.c`
- `test_environment.c`

## Documentation

- `/NativeTests/README.md` - Comprehensive documentation for all tests
- `/NativeTests/IGN_TEAS_COVERAGE.md` - Coverage matrix showing function coverage
