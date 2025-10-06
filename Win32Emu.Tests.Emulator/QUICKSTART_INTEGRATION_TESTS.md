# Quick Start: Running Integration Tests

## Overview

This guide shows how to run the integration tests for the Ignition game executables and understand the results.

## Running Tests

### Run All Integration Tests

```bash
cd /path/to/Win32Emu
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionGameTests"
```

### Run Individual Tests

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

- **Test Code**: `Win32Emu.Tests.Emulator/IgnitionGameTests.cs`
- **Results Documentation**: `Win32Emu.Tests.Emulator/README_INTEGRATION_TEST_RESULTS.md`
- **Original IGN_TEAS Test**: `Win32Emu.Tests.Emulator/IgnitionTeaserTests.cs`

## Executables Tested

Located in the `EXEs` directory:

- `EXEs/ign_demo/IGN_DEMO.EXE` - Ignition playable demo
- `EXEs/ign_win/Ign_win.exe` - Ignition main executable
- `EXEs/ign_win/Ign_win_fix.exe` - Fixed version of Ign_win.exe
- `EXEs/ign_win/ign_3dfx.exe` - 3dfx Glide version
- `EXEs/ign_teas/IGN_TEAS.EXE` - Ignition teaser demo (original test)

## Current Test Results Summary

| Executable | Status | Main Issue |
|-----------|--------|------------|
| IGN_DEMO.EXE | ✓ Loads | Infinite loop after init |
| Ign_win.exe | ✓ Loads | Infinite loop after init |
| Ign_win_fix.exe | ✓ Loads | Infinite loop after init |
| ign_3dfx.exe | ✓ Loads | Invalid instructions |
| IGN_TEAS.EXE | ✓ Loads | Infinite loop after init |

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

Ensure the EXE files exist in the `EXEs` directory. The tests automatically search for executables in:
- `EXEs/ign_demo/`
- `EXEs/ign_win/`
- `EXEs/ign_dos/`
- `EXEs/ign_teas/`

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
