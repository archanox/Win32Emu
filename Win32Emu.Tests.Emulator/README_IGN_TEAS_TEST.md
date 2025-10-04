# IGN_TEAS.EXE Integration Test

## Overview

This test demonstrates running the Ignition Teaser demo (`IGN_TEAS.EXE`) using the Win32Emu emulator. The test is located in `IgnitionTeaserTests.cs` and serves as both a functional test and documentation of the emulator's behavior when running real Win32 executables.

## Test Structure

### 1. `IgnitionTeaser_ShouldLoadAndRun`
The main test that runs with minimal logging (Info level and above only). This test:
- Loads the IGN_TEAS.EXE executable
- Runs it for up to 5 seconds with a timeout
- Captures all emulator output including:
  - Debug messages
  - Error messages
  - Warning messages
  - Stdout messages
  - Windows created
- Provides a detailed summary of execution

### 2. `IgnitionTeaser_ShouldLoadAndRun_WithDebugLogging` (Skipped)
A detailed debugging version that runs with full debug logging enabled. This test is skipped by default to avoid excessive output but can be manually enabled for detailed analysis of the emulator's instruction-level execution.

## Running the Tests

### Run the basic test:
```bash
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionTeaser_ShouldLoadAndRun"
```

### Run with detailed output:
```bash
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~IgnitionTeaser_ShouldLoadAndRun" --logger "console;verbosity=detailed"
```

### Enable the debug test (remove the Skip attribute):
Edit `IgnitionTeaserTests.cs` and remove the `Skip` parameter from the `[Fact]` attribute on the debug test.

## Findings

### What Works ✓
1. **Executable Loading**: The PE32 executable loads successfully
2. **Memory Management**: Virtual memory allocation and heap creation work correctly
3. **Win32 API Calls**: Multiple KERNEL32 functions are successfully emulated:
   - `GetVersion` - Returns Windows 95 version info
   - `HeapCreate` - Creates a heap for dynamic memory
   - `VirtualAlloc` - Allocates virtual memory (multiple calls)
   - `GetStartupInfoA` - Retrieves startup information
   - `GetStdHandle` - Gets standard I/O handles
   - `GetFileType` - Checks handle types
   - `SetHandleCount` - Sets file handle count
   - `GetACP` - Gets ANSI code page
   - `GetCPInfo` - Gets code page information
4. **No Crashes**: The executable runs without throwing exceptions

### Known Issues ⚠️

1. **Infinite Loop After Initialization**
   - The game enters an infinite loop after completing basic Win32 initialization
   - No additional API calls are made after GetCPInfo
   - Test uses a 5-second timeout to prevent hanging

2. **No DirectX Calls Observed**
   - Despite importing DDRAW.dll, DINPUT.dll, DSOUND.dll, the game never attempts to call functions from these DLLs
   - The dispatcher logs "No unknown function calls recorded" - confirming no missing APIs are being called
   - This indicates the game is stuck in its own initialization code BEFORE attempting graphics/input setup

3. **Missing Metadata**
   - Warnings about missing argument byte metadata for:
     - `KERNEL32.DLL!GetACP`
     - `KERNEL32.DLL!GetCPInfo`
   - The emulator falls back to 0 bytes for these functions

4. **Root Cause Unknown**
   - The game is executing instructions continuously without calling any APIs
   - Possible causes: waiting for a specific return value, timer condition, or event that will never occur
   - Requires deeper debugging (instruction-level tracing) to identify the exact loop location

## Next Steps for Investigation

1. **Instruction-Level Debugging**: Use the debug test variant to identify the exact code location where the infinite loop occurs
2. **Check for Polling Loops**: The game may be polling for a specific condition (e.g., checking a flag or waiting for a timer)
3. **Verify API Return Values**: Some API calls might be returning unexpected values that cause the game to wait
4. **Implement Missing APIs**: Once the game progresses past this loop, it will likely call DirectX/User32/GDI32 functions
5. **Add Execution Profiling**: Track which code regions are being executed repeatedly to identify hot loops

## Test Output Example

```
=== Ignition Teaser Demo Test ===
Current directory: /path/to/bin/Debug/net9.0
Repository root: /path/to/Win32Emu
Testing executable: /path/to/EXEs/ign_teas/IGN_TEAS.EXE
File exists: True

Loading executable...
[DEBUG] [Loader] Loading PE: /path/to/EXEs/ign_teas/IGN_TEAS.EXE
[DEBUG] [Loader] Image base=0x00400000 EntryPoint=0x00410FD0 Size=0x64000
[DEBUG] [Loader] Imports mapped: 83
Starting emulation...

[API calls omitted for brevity]

Test timed out after 5 seconds - stopping emulator
[DEBUG] [Emulator] Stop requested
Emulation completed

=== Test Summary ===
Execution time: 5.08 seconds
Debug messages captured: 36
Error messages captured: 0
Warning messages captured: 0
Stdout messages captured: 0
Windows created: 0

=== Test Result ===
✓ The executable loaded and ran successfully (with timeout)
  No exceptions were thrown during execution

Note: This test documents the behavior and issues when running the game.
Check the output above for details on what was encountered.
```

## Related Files

- **Test File**: `Win32Emu.Tests.Emulator/IgnitionTeaserTests.cs`
- **Executable**: `EXEs/ign_teas/IGN_TEAS.EXE`
- **Game Info**: `EXEs/ign_teas/README.TXT`
- **Emulator Core**: `Win32Emu/Emulator.cs`
- **KERNEL32 Module**: `Win32Emu/Win32/Modules/Kernel32Module.cs`

## Contributing

If you're working on improving DirectX support or adding more Win32 API implementations, this test serves as a real-world benchmark to verify your changes work with actual games.
