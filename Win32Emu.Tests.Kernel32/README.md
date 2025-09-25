# Win32Emu.Tests.Kernel32

This project contains comprehensive unit tests for the Kernel32.dll emulation in Win32Emu.

## Test Structure

### Test Categories

1. **BasicFunctionsTests** - Core system functions (6 tests)
   - GetVersion, GetLastError, SetLastError, ExitProcess, GetCurrentProcess

2. **MemoryManagementTests** - Memory allocation and management (10 tests)
   - GlobalAlloc/GlobalFree, HeapCreate/HeapAlloc/HeapFree, VirtualAlloc

3. **FileIOTests** - File and handle operations (11 tests)
   - CreateFileA, GetStdHandle, SetStdHandle, CloseHandle, GetFileType, SetHandleCount

4. **ModuleProcessTests** - Module and process functions (3 tests)
   - GetModuleHandleA with various scenarios

## Test Infrastructure

### MockCpu
Provides a mock implementation of the `ICpu` interface for testing Win32 API calls without requiring actual CPU emulation.

### TestEnvironment
Complete test setup that includes:
- VirtualMemory simulation
- MockCpu instance
- ProcessEnvironment setup
- Kernel32Module instance
- Utility methods for memory allocation and string handling

## Test Status

**Total: 30 tests**
- ✅ **25 passing** (83% success rate)
- ❌ **5 failing** (file I/O related, due to implementation differences)

### Passing Test Categories
- **BasicFunctionsTests**: 6/6 (100%)
- **MemoryManagementTests**: 10/10 (100%)
- **ModuleProcessTests**: 3/3 (100%)
- **FileIOTests**: 6/11 (55%)

## Usage

Run all tests:
```bash
dotnet test Win32Emu.Tests.Kernel32
```

Run specific test category:
```bash
dotnet test Win32Emu.Tests.Kernel32 --filter "BasicFunctionsTests"
```

## Known Issues

Some file I/O tests fail due to differences between the emulated implementation and expected Win32 behavior:
- CreateFileA with invalid filenames returns 0 instead of INVALID_HANDLE_VALUE
- File handle operations may not work exactly like real Win32
- Some functions return different error codes than expected

These failures document the current behavior and can be used to track improvements to the emulator.