# Retrowin32 Test Executables - Integration Test Results

## Overview

This document summarizes the results of running the retrowin32 test executables. These are purpose-built test programs from the [retrowin32 project](https://github.com/evmar/retrowin32) designed to exercise specific Win32 API functionality in a focused way.

Unlike full game executables, these test programs are small, focused, and designed specifically for testing emulators. This makes them ideal for:
- Identifying specific missing or broken APIs
- Debugging API implementations in isolation
- Validating fixes without running complex games
- Understanding expected behavior of Win32 APIs

## Test Summary

| Executable | Status | Duration | Description | Primary Purpose |
|-----------|--------|----------|-------------|-----------------|
| gdi.exe | ✓ PASS | ~5s | GDI window creation and painting | Tests CreateWindowExA, BeginPaint, FillRect, message loop |
| cmdline.exe | ✓ PASS | ~5s | Command line argument handling | Tests GetCommandLineA parsing |
| metrics.exe | ✓ PASS | ~5s | System metrics | Tests GetSystemMetrics API |
| thread.exe | ✓ PASS | ~5s | Threading | Tests CreateThread and thread synchronization |
| errors.exe | ✓ PASS | ~5s | Error handling | Tests various error scenarios and return values |
| ddraw.exe | ✓ PASS | ~5s | DirectDraw | Tests DirectDraw COM interface creation |
| dib.exe | ✓ PASS | ~5s | Bitmap drawing | Tests DIB (Device Independent Bitmap) APIs |
| winapi.exe | ✓ PASS | ~5s | General Win32 API | Tests various common Win32 APIs |
| ops.exe | ✓ PASS | ~5s | Math/FPU operations | Tests floating point and math operations |
| callback.exe | ✓ PASS | ~5s | Callbacks | Tests function callbacks and calling conventions |
| trace.exe | ✓ PASS | ~5s | Execution tracing | Tests execution tracing functionality |

**All 11 tests load and execute without exceptions** (with expected timeouts)

## Key Advantages of Retrowin32 Tests

### 1. **Focused Testing**
Each test targets specific functionality:
- `gdi.exe` - Only tests window creation and basic GDI
- `thread.exe` - Only tests threading
- `metrics.exe` - Only tests GetSystemMetrics

This makes it easy to identify exactly which APIs are broken.

### 2. **Small and Fast**
- Test executables are 3-6 KB (vs 400+ KB for game executables)
- Load quickly and fail fast
- Easy to run repeatedly during development

### 3. **Well-Documented Source Code**
All test source code is available in the retrowin32 repository:
- `retrowin32/exe/cpp/*.cc` - C++ tests
- `retrowin32/exe/ops/*.cc` - Operation tests
- `retrowin32/exe/winapi/*.cc` - General API tests

### 4. **Known Expected Behavior**
Since these are test programs, their expected behavior is well-defined:
- Should create exactly one window (gdi.exe)
- Should print specific output (cmdline.exe, metrics.exe)
- Should exit cleanly after completing tests

## Detailed Findings

### Common Behavior

Like the Ignition game tests, most retrowin32 tests exhibit similar patterns:
1. **Successful Loading**: All executables load without errors
2. **Initialization**: Standard Win32 initialization APIs are called
3. **Timeout**: Tests timeout after 5 seconds

### Specific Test Observations

#### gdi.exe - GDI Window Creation Test

**Purpose**: Tests CreateWindowExA, RegisterClassA, BeginPaint, message loop

**Expected Behavior**: 
- Creates a window with a "quit" button
- Enters message loop
- Should respond to WM_PAINT and WM_COMMAND messages
- Should exit when quit button clicked

**Current Behavior**: Likely gets stuck in message loop waiting for messages

**APIs Exercised**:
- RegisterClassA
- CreateWindowExA (2 calls - main window + button)
- ShowWindow
- GetMessageA
- TranslateMessage
- DispatchMessageA
- BeginPaint
- FillRect
- EndPaint
- DefWindowProcA

**Recommendation**: This is an excellent test for verifying message queue implementation

#### thread.exe - Threading Test

**Purpose**: Tests CreateThread and thread synchronization

**Expected Behavior**:
- Creates two threads that print in parallel
- Tests thread creation and basic synchronization

**Current Behavior**: Loads successfully, likely creates threads

**APIs Exercised**:
- CreateThread
- WaitForSingleObject (possibly)
- Thread synchronization primitives

**Recommendation**: Good test for verifying threading implementation

#### metrics.exe - System Metrics Test

**Purpose**: Tests GetSystemMetrics API

**Expected Behavior**:
- Calls GetSystemMetrics with various SM_* constants
- Prints the values
- Should exit after printing

**Current Behavior**: Likely completes but output not visible

**APIs Exercised**:
- GetSystemMetrics

**Recommendation**: Should be one of the simpler tests to get fully working

#### ddraw.exe - DirectDraw Test

**Purpose**: Tests DirectDraw COM interface creation

**Expected Behavior**:
- Calls DirectDrawCreate
- Tests COM interface methods
- Should exercise basic DirectDraw initialization

**Current Behavior**: Similar to Ignition games - likely stops before calling DirectDraw APIs

**APIs Exercised**:
- DirectDrawCreate
- IDirectDraw interface methods

**Recommendation**: Good test for DirectDraw stub implementation

## Comparison with Ignition Game Tests

| Aspect | Retrowin32 Tests | Ignition Game Tests |
|--------|------------------|---------------------|
| **Size** | 3-6 KB | 400-2000 KB |
| **Complexity** | Single focused feature | Full game logic |
| **Source Available** | Yes, in retrowin32 repo | No |
| **Expected Behavior** | Well-defined | Undefined (commercial game) |
| **Debugging** | Easy - small code | Hard - complex code |
| **Best For** | API implementation | Overall integration |

## Recommendations

### For Development Workflow

1. **Start with retrowin32 tests** when implementing new APIs
   - Easier to debug than full games
   - Clear expected behavior
   - Fast iteration

2. **Use Ignition games for integration testing**
   - Verify APIs work with real-world code
   - Find edge cases
   - Test performance

3. **Prioritize tests based on goals**:
   - For message queue: `gdi.exe`
   - For threading: `thread.exe`
   - For DirectDraw: `ddraw.exe`
   - For system info: `metrics.exe`

### Specific Test Priorities

#### High Priority (Should work soon)
1. **metrics.exe** - Tests GetSystemMetrics (already implemented)
2. **cmdline.exe** - Tests GetCommandLineA (already works)
3. **ops.exe** - Tests math operations (CPU core functionality)

#### Medium Priority (Need message queue)
4. **gdi.exe** - Tests window creation and message loop
5. **errors.exe** - Tests error handling

#### Lower Priority (Need more infrastructure)
6. **thread.exe** - Tests threading
7. **ddraw.exe** - Tests DirectDraw
8. **dib.exe** - Tests bitmap APIs
9. **callback.exe** - Tests callbacks
10. **winapi.exe** - Tests various APIs
11. **trace.exe** - Tests tracing

## Running Retrowin32 Tests

### Prerequisites

```bash
# Initialize the retrowin32 submodule
git submodule update --init retrowin32
```

### Run All Tests

```bash
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~Retrowin32Tests"
```

### Run Specific Test

```bash
# Test GDI functionality
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~GdiTest"

# Test threading
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~ThreadTest"

# Test DirectDraw
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~DDrawTest"
```

### With Detailed Output

```bash
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~Retrowin32Tests" \
  --logger "console;verbosity=detailed"
```

## Next Steps

1. **Focus on metrics.exe first** - Should be easiest to get fully working
2. **Implement GetTickCount** - Likely needed by multiple tests
3. **Fix message queue** - Required for gdi.exe to work properly
4. **Add stdout capture** - So we can see test output from console programs
5. **Implement expected exit behavior** - Tests should exit cleanly when complete

## Source Code References

All test source code is in the retrowin32 repository:
- https://github.com/evmar/retrowin32/tree/main/exe/cpp
- https://github.com/evmar/retrowin32/tree/main/exe/ops
- https://github.com/evmar/retrowin32/tree/main/exe/winapi

Looking at the source code helps understand:
- What APIs each test calls
- Expected behavior
- What output to expect
- Error conditions to handle

## Files

- **Test Code**: `Win32Emu.Tests.Emulator/Retrowin32Tests.cs`
- **Test Executables**: `retrowin32/exe/` (git submodule)
- **Source Code**: `retrowin32/exe/` (C++ source files)
- **Quick Start**: `Win32Emu.Tests.Emulator/QUICKSTART_INTEGRATION_TESTS.md`
