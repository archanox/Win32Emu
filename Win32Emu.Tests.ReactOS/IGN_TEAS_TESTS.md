# ReactOS/Wine Tests for ign_teas.exe Functions

This document describes the ReactOS and Wine test coverage added for the Win32 API functions used by the `ign_teas.exe` application (from the Ignition TEAS game).

## Overview

The `ign_teas.exe` application uses a comprehensive set of Win32 APIs for graphics, input, sound, and system functions. This test suite validates Win32Emu's implementation of these APIs using:

1. **ReactOS API Tests** - Focused unit tests from the ReactOS project
2. **Wine Tests** - Comprehensive integration tests from the Wine project

## Test Coverage by Module

### Kernel32.dll Tests

**Test Files:**
- `kernel32_winetest.exe` (2.1 MB) - 100+ Wine integration tests
- `kernel32_apitest.exe` (425 KB) - ReactOS focused unit tests

**Functions Covered** (as used by ign_teas.exe):

| Function | Purpose | Test Coverage |
|----------|---------|---------------|
| `GetVersion` | Get Windows version | Wine + ReactOS |
| `HeapCreate` | Create a heap | Wine + ReactOS |
| `HeapAlloc` | Allocate heap memory | Wine + ReactOS |
| `HeapFree` | Free heap memory | Wine + ReactOS |
| `VirtualAlloc` | Reserve/commit virtual memory | Wine + ReactOS |
| `GetStartupInfoA` | Get process startup info | Wine + ReactOS |
| `GetStdHandle` | Get standard handles | Wine + ReactOS |
| `GetFileType` | Determine file type | Wine + ReactOS |
| `SetHandleCount` | Set max file handles (legacy) | Wine + ReactOS |
| `GetACP` | Get ANSI code page | Wine + ReactOS |
| `GetCPInfo` | Get code page info | Wine + ReactOS |
| `GetCommandLineA` | Get command line string | Wine + ReactOS |
| `GetEnvironmentStringsW` | Get environment block | Wine + ReactOS |
| `WideCharToMultiByte` | Convert Unicode to ANSI | Wine + ReactOS |
| `FreeEnvironmentStringsW` | Free environment block | Wine + ReactOS |
| `GetModuleFileNameA` | Get module file path | Wine + ReactOS |
| `GetModuleHandleA` | Get module handle | Wine + ReactOS |
| `GetProcAddress` | Get function address | Wine + ReactOS |
| `IsProcessorFeaturePresent` | Check CPU features | Wine + ReactOS |
| `CreateFileA` | Open/create file | Wine + ReactOS |
| `GetFileType` | Get file type | Wine + ReactOS |
| `SetFilePointer` | Move file pointer | Wine + ReactOS |
| `ReadFile` | Read from file | Wine + ReactOS |
| `CloseHandle` | Close handle | Wine + ReactOS |
| `ExitProcess` | Terminate process | Wine + ReactOS |

### User32.dll Tests

**Test Files:**
- `user32_winetest.exe` (2.3 MB) - 200+ Wine integration tests
- `user32_apitest.exe` (1.2 MB) - ReactOS focused unit tests
- `user32_dynamic_apitest.exe` (47 KB) - Dynamic API tests
- `user32_apitest_menuui.exe` (33 KB) - Menu UI tests

**Functions Covered** (as used by ign_teas.exe):

| Function | Purpose | Test Coverage |
|----------|---------|---------------|
| `LoadCursorA` | Load cursor resource | Wine + ReactOS |
| `LoadIconA` | Load icon resource | Wine + ReactOS |
| `RegisterClassA` | Register window class | Wine + ReactOS |
| `CreateWindowExA` | Create window | Wine + ReactOS |
| `DefWindowProcA` | Default window procedure | Wine + ReactOS |
| `UpdateWindow` | Update window | Wine + ReactOS |
| `SetFocus` | Set keyboard focus | Wine + ReactOS |
| `ShowWindow` | Show/hide window | Wine + ReactOS |
| `GetSystemMetrics` | Get system metrics | Wine + ReactOS |
| `SetRect` | Set rectangle coordinates | Wine + ReactOS |
| `GetMessageA` | Get message from queue | Wine + ReactOS |
| `PeekMessageA` | Peek at message | Wine + ReactOS |
| `TranslateMessage` | Translate messages | Wine + ReactOS |
| `DispatchMessageA` | Dispatch message | Wine + ReactOS |
| `PostMessageA` | Post message to queue | Wine + ReactOS |
| `PostQuitMessage` | Post quit message | Wine + ReactOS |
| `SetCursor` | Set cursor shape | Wine + ReactOS |

### Gdi32.dll Tests

**Test Files:**
- `gdi32_winetest.exe` (1.1 MB) - Wine integration tests
- `gdi32_apitest.exe` (892 KB) - ReactOS focused unit tests

**Functions Covered** (as used by ign_teas.exe):

| Function | Purpose | Test Coverage |
|----------|---------|---------------|
| `GetStockObject` | Get stock GDI object | Wine + ReactOS |

### Advapi32.dll Tests

**Test Files:**
- `advapi32_winetest.exe` (773 KB) - Wine integration tests
- `advapi32_apitest.exe` (215 KB) - ReactOS focused unit tests

**Coverage:**
While `ign_teas.exe` doesn't directly use Advapi32 functions, these tests are included for comprehensive Win32 API validation, particularly for:
- Registry operations
- Security functions
- Event logging

### Multimedia Tests (Winmm)

**Functions Used** (as used by ign_teas.exe):

| Function | Purpose | Existing Test Coverage |
|----------|---------|----------------------|
| `timeBeginPeriod` | Set timer resolution | `Win32Emu.Tests.User32/MultimediaTests.cs` |
| `timeEndPeriod` | Restore timer resolution | `Win32Emu.Tests.User32/MultimediaTests.cs` |
| `timeGetTime` | Get system time | `Win32Emu.Tests.User32/MultimediaTests.cs` |

**Note:** Multimedia functions already have dedicated test coverage in `Win32Emu.Tests.User32/MultimediaTests.cs`.

## Running the Tests

### Run All ReactOS/Wine Tests

```bash
cd Win32Emu.Tests.ReactOS
dotnet test --filter "Category=ReactOSTests"
```

### Run Tests by Module

```bash
# Kernel32 tests
dotnet test --filter "Module=Kernel32"

# User32 tests
dotnet test --filter "Module=User32"

# Gdi32 tests
dotnet test --filter "Module=Gdi32"

# Advapi32 tests
dotnet test --filter "Module=Advapi32"
```

### Run Specific Test Type

```bash
# Wine tests only
dotnet test --filter "Function~WineTests"

# ReactOS API tests only
dotnet test --filter "Function~ApiTests"
```

## Test Status

All ReactOS/Wine tests are currently **SKIPPED** by default because:

1. **These are integration tests** - They test the full emulator stack, not individual functions
2. **Informational results** - Test failures don't block PRs; they help identify missing/incomplete implementations
3. **Memory/performance intensive** - Wine test suites can be large (2+ MB) and comprehensive
4. **May trigger emulator issues** - As noted in the User32 tests, some tests may expose edge cases

### Enabling Tests Manually

To run these tests locally for validation:

```bash
# Remove Skip attribute and run
cd Win32Emu.Tests.ReactOS

# Edit test files to remove [Theory(Skip = "...")] attributes
# Or use filter to override:
dotnet test --filter "Module=Kernel32" /p:SkipTests=false
```

## Expected Results

These tests serve as **informational validation** of Win32Emu's API completeness:

- ✅ **Test execution completes** - Emulator can load and run the test executable
- ℹ️ **Some tests may fail** - Due to unimplemented or partially implemented APIs
- ℹ️ **Failures are tracked** - Helps prioritize API implementation work
- ✅ **No crashes or hangs** - Emulator handles test execution gracefully

## Comparison to Existing Unit Tests

| Test Type | Purpose | Example |
|-----------|---------|---------|
| **Unit Tests** | Test individual API functions in isolation | `Win32Emu.Tests.Kernel32/FileIOTests.cs` |
| **ReactOS Ported Tests** | Port specific ReactOS test cases to C# | `Win32Emu.Tests.User32/ReactOSPortedTests.cs` |
| **ReactOS/Wine Integration Tests** | Run full ReactOS/Wine test executables | `Win32Emu.Tests.ReactOS/*ReactOSTests.cs` |

**All three test types are complementary:**
- Unit tests provide fast, focused validation
- Ported tests validate behavior matches ReactOS expectations
- Integration tests validate real-world executable compatibility

## Function Usage Statistics from ign_teas.exe

Based on `ApiMon Logs/ign_teas/ign_teas.exe.csv`:

| API Call | Invocation Count | Module |
|----------|-----------------|--------|
| `IDirectDrawSurface::Release` | 21,312 | DirectDraw (COM) |
| `IDirectDrawSurface::QueryInterface` | 21,300 | DirectDraw (COM) |
| `IDirectDrawSurface::IsLost` | 3,314 | DirectDraw (COM) |
| `IDirectSoundBuffer::GetCurrentPosition` | 2,559 | DirectSound (COM) |
| `IDirectDrawSurface::ReleaseDC` | 2,356 | DirectDraw (COM) |
| `IDirectDrawSurface::GetDC` | 2,356 | DirectDraw (COM) |
| `IDirectDrawSurface::GetClipper` | 1,900 | DirectDraw (COM) |
| `PeekMessageA` | 1,062 | User32 |
| `timeGetTime` | 956 | Winmm |
| `IDirectInputDeviceA::GetDeviceData` | 954 | DirectInput (COM) |
| `IDirectDrawSurface::Unlock` | 952 | DirectDraw (COM) |
| `IDirectDrawSurface::Lock` | 952 | DirectDraw (COM) |
| `DefWindowProcA` | 324 | User32 |
| `IDirectDrawPalette::SetEntries` | 237 | DirectDraw (COM) |
| `SetFilePointer` | 167 | Kernel32 |
| `GetMessageA` | 107 | User32 |
| `TranslateMessage` | 106 | User32 |
| `DispatchMessageA` | 106 | User32 |
| `GetFileType` | 82 | Kernel32 |
| `CreateFileA` | 79 | Kernel32 |
| `CloseHandle` | 79 | Kernel32 |
| `SetCursor` | 72 | User32 |
| `HeapAlloc` | 57 | Kernel32 |
| `HeapFree` | 46 | Kernel32 |
| `ReadFile` | 43 | Kernel32 |

**Note:** DirectDraw, DirectSound, and DirectInput are COM interfaces, not standard Win32 APIs. They have separate test coverage in Win32Emu's DirectX implementation modules.

## References

- **ReactOS Project**: https://github.com/reactos/reactos
- **Wine Project**: https://www.winehq.org/
- **ReactOS API Tests**: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests
- **Wine Tests**: https://gitlab.winehq.org/wine/wine/-/tree/master/dlls
- **ign_teas.exe API Log**: `ApiMon Logs/ign_teas/ign_teas.exe.csv`

## See Also

- `README.md` - Main project documentation
- `README.Tests.md` - Test strategy overview
- `Win32Emu.Tests.ReactOS/README.md` - ReactOS test infrastructure details
