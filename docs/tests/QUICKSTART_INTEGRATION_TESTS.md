# Quick Start: Running Integration Tests

## Overview

This guide shows how to run the integration tests for both the Ignition game executables and the retrowin32 test executables, and understand the results.

## Test Suites

### 1. Ignition Game Tests (`IgnitionGameTests.cs`)
Full game executables from the Ignition racing game series.

### 2. Retrowin32 Tests (`Retrowin32Tests.cs`)
Purpose-built test programs from the [retrowin32 project](https://github.com/evmar/retrowin32) that exercise specific Win32 API functionality.

## Running Tests

### Prerequisites

The retrowin32 test executables require the git submodule to be initialized:

```bash
cd /path/to/Win32Emu
git submodule update --init retrowin32
```

### Run All Integration Tests

```bash
cd /path/to/Win32Emu
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionGameTests"
```

### Run All Retrowin32 Tests

```bash
cd /path/to/Win32Emu
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~Retrowin32Tests"
```

### Run All Integration Tests (Both Suites)

```bash
cd /path/to/Win32Emu
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionGameTests|FullyQualifiedName~Retrowin32Tests"
```

### Run Individual Tests

**Ignition Game Tests:**
```bash
# Test IGN_DEMO.EXE
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionDemo_ShouldLoadAndRun"

# Test Ign_win.exe
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionWin_ShouldLoadAndRun"

# Test Ign_win_fix.exe
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionWinFix_ShouldLoadAndRun"

# Test ign_3dfx.exe
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~Ignition3dfx_ShouldLoadAndRun"
```

**Retrowin32 Tests:**
```bash
# Test GDI window creation
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~GdiTest_ShouldLoadAndRun"

# Test command line handling
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~CmdLineTest_ShouldLoadAndRun"

# Test threading
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~ThreadTest_ShouldLoadAndRun"

# Test DirectDraw
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~DDrawTest_ShouldLoadAndRun"

# See Retrowin32Tests.cs for all available tests
```

### Run with Detailed Output

```bash
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionGameTests" --logger "console;verbosity=detailed"
```

### Run Including IGN_TEAS.EXE Test

```bash
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~Ignition"
```

## Understanding Test Results

### Successful Test (Expected Behavior)

All tests currently "pass" but timeout after 5 seconds. This is expected behavior documenting current emulator limitations:

```
✓ ign_3dfx.exe loaded and ran successfully (with timeout)
  No exceptions were thrown during execution
```

### What to Look For in Output

1. **API Calls**: Check which Win32 APIs are being called
   ```
   [DEBUG] [Import] KERNEL32.DLL!GetVersion
   [DEBUG] [Import] KERNEL32.DLL!HeapCreate
   ```

2. **Warnings**: Look for issues or missing functionality
   ```
   [WARNING]  [IcedCpu] Unhandled mnemonic INVALID at 0x000004DD
   [WARNING]  No arg bytes metadata for KERNEL32.DLL!GetACP
   ```

3. **Test Summary**: Shows execution statistics
   ```
   Execution time: 5.03 seconds
   Debug messages captured: 42
   Error messages captured: 0
   Warning messages captured: 0
   Windows created: 0
   ```

## Test Files

- **Ignition Game Tests**: `Win32Emu.Tests.Emulator/IgnitionGameTests.cs`
- **Retrowin32 Tests**: `Win32Emu.Tests.Emulator/Retrowin32Tests.cs`
- **Results Documentation**: `Win32Emu.Tests.Emulator/README_INTEGRATION_TEST_RESULTS.md`
- **Original IGN_TEAS Test**: `Win32Emu.Tests.Emulator/IgnitionTeaserTests.cs`

## Executables Tested

### Ignition Games (in `EXEs` directory)

- `EXEs/ign_demo/IGN_DEMO.EXE` - Ignition playable demo
- `EXEs/ign_win/Ign_win.exe` - Ignition main executable
- `EXEs/ign_win/Ign_win_fix.exe` - Fixed version of Ign_win.exe
- `EXEs/ign_win/ign_3dfx.exe` - 3dfx Glide version
- `EXEs/ign_teas/IGN_TEAS.EXE` - Ignition teaser demo (original test)

### Retrowin32 Test Programs (in `retrowin32/exe` submodule)

- `retrowin32/exe/cpp/gdi.exe` - GDI window creation and painting
- `retrowin32/exe/cpp/cmdline.exe` - Command line argument handling
- `retrowin32/exe/cpp/metrics.exe` - GetSystemMetrics API
- `retrowin32/exe/cpp/thread.exe` - CreateThread and threading
- `retrowin32/exe/cpp/errors.exe` - Error handling scenarios
- `retrowin32/exe/cpp/ddraw.exe` - DirectDraw functionality
- `retrowin32/exe/cpp/dib.exe` - Bitmap drawing APIs
- `retrowin32/exe/winapi/winapi.exe` - General Win32 API
- `retrowin32/exe/ops/ops.exe` - Math and FPU operations
- `retrowin32/exe/callback/callback.exe` - Callback functionality
- `retrowin32/exe/trace/trace.exe` - Execution tracing

## Current Test Results Summary

### Ignition Game Tests

| Executable | Status | Main Issue |
|-----------|--------|------------|
| IGN_DEMO.EXE | ✓ Loads | Infinite loop after init |
| Ign_win.exe | ✓ Loads | Infinite loop after init |
| Ign_win_fix.exe | ✓ Loads | Infinite loop after init |
| ign_3dfx.exe | ✓ Loads | Invalid instructions |
| IGN_TEAS.EXE | ✓ Loads | Infinite loop after init |

### Retrowin32 Test Programs

| Executable | Status | Description |
|-----------|--------|-------------|
| gdi.exe | ✓ Loads | GDI window creation and painting |
| cmdline.exe | ✓ Loads | Command line argument handling |
| metrics.exe | ✓ Loads | GetSystemMetrics API |
| thread.exe | ✓ Loads | CreateThread and threading |
| errors.exe | ✓ Loads | Error handling scenarios |
| ddraw.exe | ✓ Loads | DirectDraw functionality |
| dib.exe | ✓ Loads | Bitmap drawing APIs |
| winapi.exe | ✓ Loads | General Win32 API |
| ops.exe | ✓ Loads | Math and FPU operations |
| callback.exe | ✓ Loads | Callback functionality |
| trace.exe | ✓ Loads | Execution tracing |

**Total**: 16 integration tests available (15 passed, 1 skipped)

## Next Steps

See `README_INTEGRATION_TEST_RESULTS.md` for:
- Detailed analysis of issues
- Recommendations for fixes
- Missing APIs that need implementation
- Prioritized action items

## Troubleshooting

### Test Executable Not Found

If you see:
```
System.IO.FileNotFoundException: Test executable not found
```

**For Ignition games**: Ensure the EXE files exist in the `EXEs` directory. The tests automatically search for executables in:
- `EXEs/ign_demo/`
- `EXEs/ign_win/`
- `EXEs/ign_dos/`
- `EXEs/ign_teas/`

**For retrowin32 tests**: Initialize the git submodule:
```bash
git submodule update --init retrowin32
```

If you see:
```
InvalidOperationException: retrowin32 submodule not initialized
```

Run the command above to initialize the submodule.

### Build Failures

If tests don't build:
```bash
# Clean and rebuild
dotnet clean
dotnet build Win32Emu.Tests.Emulator
```

### Tests Hang Indefinitely

Tests have a 5-second timeout built in. If they hang longer:
1. Press Ctrl+C to stop
2. Check for emulator deadlock issues
3. Verify Stop() method works correctly

## Contributing

When adding new test executables:

### For Ignition or other game executables:

1. Add executable to appropriate `EXEs/` subdirectory
2. Add test method to `IgnitionGameTests.cs`:
   ```csharp
   [Fact]
   public void NewExecutable_ShouldLoadAndRun()
   {
       var exePath = FindExecutable("newexec.exe");
       RunExecutableTest(exePath, "newexec.exe");
   }
   ```
3. Run test and document findings in `README_INTEGRATION_TEST_RESULTS.md`

### For retrowin32 test executables:

The retrowin32 submodule is managed externally. To add tests:
1. Ensure submodule is up to date: `git submodule update --remote retrowin32`
2. Add test method to `Retrowin32Tests.cs`:
   ```csharp
   [Fact]
   public void NewTest_ShouldLoadAndRun()
   {
       var exePath = FindRetrowin32Executable("subdir/newtest.exe");
       RunExecutableTest(exePath, "newtest.exe", "Description of test");
   }
   ```
3. Run test and document findings
