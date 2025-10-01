# Win32Emu.Tests.Kernel32

This project contains comprehensive unit tests for the Kernel32.dll emulation in Win32Emu.

## Test Structure

### Test Categories

1. **BasicFunctionsTests** - Core system functions (32 tests)
   - GetVersion, GetLastError, SetLastError, ExitProcess, GetCurrentProcess, QueryPerformanceCounter, code page functions, wide character conversion

2. **MemoryManagementTests** - Memory allocation and management (10 tests)
   - GlobalAlloc/GlobalFree, HeapCreate/HeapAlloc/HeapFree, VirtualAlloc

3. **FileIOTests** - File and handle operations (11 tests)
   - CreateFileA, GetStdHandle, SetStdHandle, CloseHandle, GetFileType, SetHandleCount

4. **ModuleProcessTests** - Module and process functions (10 tests)
   - GetModuleHandleA, LoadLibraryA with various scenarios

5. **EnvironmentTests** - Environment and command-line functions (14 tests)
   - GetCommandLineA, GetEnvironmentStrings, GetStartupInfoA

6. **CpuMemoryAccessTests** - CPU and memory interaction tests (11 tests)
   - Stack operations, memory access patterns

7. **CpuDebuggingTests** - CPU debugging functionality (6 tests)
   - Register state tracking, debugging helpers

8. **DispatcherTests** - Win32 dispatcher functionality (5 tests)
   - Function dispatching, unknown function handling

9. **DispatcherIntegrationTests** - Integration tests (2 tests)
   - LoadLibrary integration, dispatcher summary

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

**Total: 101 tests**
- ✅ **101 passing** (100% success rate)

### Passing Test Categories
- **BasicFunctionsTests**: 32/32 (100%)
- **MemoryManagementTests**: 10/10 (100%)
- **ModuleProcessTests**: 10/10 (100%)
- **FileIOTests**: 11/11 (100%)
- **EnvironmentTests**: 14/14 (100%)
- **CpuMemoryAccessTests**: 11/11 (100%)
- **CpuDebuggingTests**: 6/6 (100%)
- **DispatcherTests**: 5/5 (100%)
- **DispatcherIntegrationTests**: 2/2 (100%)

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

None - all tests are currently passing!