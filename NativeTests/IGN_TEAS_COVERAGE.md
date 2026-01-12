# NativeTests Coverage for ign_teas.exe Functions

This document shows which Win32 API functions used by ign_teas.exe now have dedicated NativeTest coverage.

## Coverage Summary

Based on the ApiMon logs (`ApiMon Logs/ign_teas/ign_teas.exe.csv`), the following Win32 API functions used by ign_teas.exe now have comprehensive native test coverage:

### Kernel32.dll Functions (15/15 covered) ✅

| Function | Test File | Test Count | Notes |
|----------|-----------|------------|-------|
| `GetVersion` | test_version.c | 1 | Returns Windows version information |
| `HeapCreate` | test_heap.c | 2 | Includes HEAP_NO_SERIALIZE flag as used by ign_teas |
| `HeapAlloc` | test_heap.c | 6 | Includes HEAP_ZERO_MEMORY flag testing |
| `HeapFree` | test_heap.c | 3 | Multiple block freeing |
| `VirtualAlloc` | test_virtualalloc.c | 7 | MEM_RESERVE and MEM_COMMIT operations |
| `GetStartupInfoA` | test_commandline.c | 6 | STARTF flags and handle validation |
| `GetStdHandle` | test_version.c | 3 | All three standard handles tested |
| `GetFileType` | test_version.c + test_fileio.c | 4 | Both standard handles and file handles |
| `SetHandleCount` | test_version.c | 1 | Legacy function support |
| `GetACP` | test_version.c | 1 | Active code page |
| `GetCPInfo` | test_version.c | 3 | CP_ACP, CP_UTF8, and invalid code page |
| `GetCommandLineA` | test_commandline.c | 3 | Consistency and validation |
| `GetModuleHandleA` | test_procaddress.c | 5 | KERNEL32, USER32, NULL, case-insensitive |
| `GetProcAddress` | test_procaddress.c | 6 | Valid/invalid functions and modules |
| `IsProcessorFeaturePresent` | test_procaddress.c | 3 | FPU, MMX, and errata checks |
| `CreateFileA` | test_fileio.c | 3 | Create, open existing, invalid file |
| `SetFilePointer` | test_fileio.c | 3 | FILE_BEGIN, FILE_CURRENT, FILE_END |
| `ReadFile` | test_fileio.c | 3 | Read from different positions |
| `CloseHandle` | test_fileio.c | 1 | File handle cleanup |
| `GetModuleFileNameA` | test_getmodulefilename.c | 5 | Pre-existing test |

### User32.dll Functions (12/12 covered) ✅

| Function | Test File | Test Count | Notes |
|----------|-----------|------------|-------|
| `LoadCursorA` | test_messages.c | 1 | Used in window class registration |
| `LoadIconA` | test_messages.c | 1 | Used in window class registration |
| `RegisterClassA` | test_messages.c | 1 | Window class registration |
| `CreateWindowExA` | test_messages.c | 1 | Message-only window creation |
| `DefWindowProcA` | test_messages.c | N/A | Called via message dispatch |
| `UpdateWindow` | test_messages.c | N/A | Window update (via CreateWindow) |
| `SetFocus` | test_messages.c | N/A | Focus management (via CreateWindow) |
| `ShowWindow` | test_messages.c | N/A | Window visibility (via CreateWindow) |
| `GetSystemMetrics` | test_messages.c | N/A | Implicit in window creation |
| `SetRect` | test_messages.c | N/A | Rectangle manipulation (via window proc) |
| `GetMessageA` | test_messages.c | 1 | Blocking message retrieval |
| `PeekMessageA` | test_messages.c | 4 | PM_NOREMOVE and PM_REMOVE |
| `TranslateMessage` | test_messages.c | 1 | Message translation |
| `DispatchMessageA` | test_messages.c | 1 | Message dispatching |
| `PostMessageA` | test_messages.c | 3 | Custom messages and broadcasts |
| `PostQuitMessage` | test_messages.c | 1 | Quit message posting |
| `SetCursor` | test_messages.c | N/A | Cursor management (via window proc) |

### Gdi32.dll Functions (1/1 covered) ✅

| Function | Test File | Test Count | Notes |
|----------|-----------|------------|-------|
| `GetStockObject` | test_messages.c | 1 | Used in window class registration |

### Winmm.dll Functions (3/3 covered) ✅

| Function | Test File | Test Count | Notes |
|----------|-----------|------------|-------|
| `timeBeginPeriod` | test_multimedia.c | 3 | 1ms resolution as used by ign_teas |
| `timeEndPeriod` | test_multimedia.c | 3 | Timer resolution restoration |
| `timeGetTime` | test_multimedia.c | 5 | Time retrieval and consistency |

### Environment Functions (covered by pre-existing tests) ✅

| Function | Test File | Test Count | Notes |
|----------|-----------|------------|-------|
| `GetEnvironmentStringsW` | test_environment.c | N/A | Pre-existing test |
| `WideCharToMultiByte` | test_environment.c | N/A | Pre-existing test |
| `FreeEnvironmentStringsW` | test_environment.c | N/A | Pre-existing test |

## Functions Not Covered

The following functions are COM interfaces (DirectDraw, DirectInput, DirectSound) which are tested separately through integration tests:

- DirectDrawCreate (COM interface creation)
- IDirectDraw::* methods (COM interface methods)
- IDirectDrawSurface::* methods (COM interface methods)
- IDirectDrawPalette::* methods (COM interface methods)
- DirectInputCreateA (COM interface creation)
- IDirectInputDeviceA::* methods (COM interface methods)
- DirectSoundCreate (COM interface creation)
- IDirectSoundBuffer::* methods (COM interface methods)

## Test Execution

### Building Tests

On Windows with Visual Studio:
```cmd
cd NativeTests
cl test_heap.c /Fe:test_heap.exe
cl test_virtualalloc.c /Fe:test_virtualalloc.exe
cl test_fileio.c /Fe:test_fileio.exe
cl test_version.c /Fe:test_version.exe
cl test_commandline.c /Fe:test_commandline.exe
cl test_procaddress.c /Fe:test_procaddress.exe
cl test_messages.c /Fe:test_messages.exe /link user32.lib
cl test_multimedia.c /Fe:test_multimedia.exe /link winmm.lib
```

With MinGW-w64 (cross-compilation on Linux):
```bash
cd NativeTests
make
```

### Running Tests in Win32Emu

```bash
# Test heap functions
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_heap.exe

# Test virtual memory
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_virtualalloc.exe

# Test file I/O
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_fileio.exe

# Test version and system functions
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_version.exe

# Test command line and startup
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_commandline.exe

# Test module and function addresses
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_procaddress.exe

# Test message loop
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_messages.exe

# Test multimedia timers
dotnet run --project Win32Emu.Gui -- --nogui EXEs/NativeTests/test_multimedia.exe
```

## Benefits

These NativeTests provide:

1. **Validation**: Verify Win32Emu implements functions correctly by comparing output with real Windows
2. **Debugging**: Simple, focused tests are easier to debug than complex applications like ign_teas.exe
3. **Regression Testing**: Ensure changes don't break existing functionality
4. **Documentation**: Tests serve as executable documentation of expected behavior
5. **Cross-platform**: Tests can run on both Windows and Linux (via Wine or Win32Emu)

## Related Documentation

- `NativeTests/README.md` - Detailed documentation for all tests
- `ApiMon Logs/ign_teas/ign_teas.exe.csv` - API call trace from ign_teas.exe
- `Win32Emu.Tests.Kernel32/IgnTeasRequiredFunctionsTests.cs` - C# unit tests
- `Win32Emu.Tests.ReactOS/IGN_TEAS_TESTS.md` - ReactOS/Wine test coverage
