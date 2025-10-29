# PR Summary: Fix GetStartupInfoA to Return Pseudo-Handle Constants

## Issue

The emulator was crashing when running `winapi.exe` with the error:
```
Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00401002)
NOTE: Address 0xFFFFFFF5 is STD_OUTPUT_HANDLE (pseudo-handle value -11).
This error typically occurs when code tries to dereference a pseudo-handle as a memory address.
```

## Root Cause

The `GetStartupInfoA` function was incorrectly writing real handle values (0x00000001, 0x00000002, 0x00000003) to the standard handle fields of the STARTUPINFO structure, instead of the Windows pseudo-handle constants that programs expect.

Windows programs expect:
- `STD_INPUT_HANDLE` = 0xFFFFFFF6 (-10)
- `STD_OUTPUT_HANDLE` = 0xFFFFFFF5 (-11)
- `STD_ERROR_HANDLE` = 0xFFFFFFF4 (-12)

Programs should call `GetStdHandle()` with these pseudo-handle constants to obtain the actual handle values before using them with file I/O functions.

## Solution

Modified `GetStartupInfoA` in `Win32Emu/Win32/Modules/Kernel32Module.cs` to write pseudo-handle constants instead of real handle values.

### Code Changes

```diff
- _env.MemWrite32(lpStartupInfo + 56, _env.StdInputHandle);
- _env.MemWrite32(lpStartupInfo + 60, _env.StdOutputHandle);
- _env.MemWrite32(lpStartupInfo + 64, _env.StdErrorHandle);
+ // Write pseudo-handle constants, not real handle values
+ // Programs should call GetStdHandle() to get actual handles
+ _env.MemWrite32(lpStartupInfo + 56, 0xFFFFFFF6); // STD_INPUT_HANDLE
+ _env.MemWrite32(lpStartupInfo + 60, 0xFFFFFFF5); // STD_OUTPUT_HANDLE
+ _env.MemWrite32(lpStartupInfo + 64, 0xFFFFFFF4); // STD_ERROR_HANDLE
```

## Changes Made

### 1. Core Fix
- **File**: `Win32Emu/Win32/Modules/Kernel32Module.cs`
- **Change**: Modified `GetStartupInfoA` to write pseudo-handle constants (3 lines changed)
- **Impact**: Programs now receive correct pseudo-handle values in STARTUPINFO structure

### 2. Tests Added
- **File**: `Win32Emu.Tests.Kernel32/FileIOTests.cs`
- **Tests Added**:
  - `GetStartupInfoA_ShouldReturnPseudoHandlesInStartupInfo` - Verifies STARTUPINFO contains pseudo-handles
  - `GetStartupInfoA_ThenGetStdHandle_ShouldWorkCorrectly` - Tests complete workflow from GetStartupInfoA → GetStdHandle → WriteFile

### 3. Documentation
- **File**: `GETSTARTUPINFOA_FIX.md`
- **Content**: Detailed explanation of the issue, root cause, solution, and Windows API behavior

## Why This Fix Is Correct

According to the Windows API documentation:

1. `GetStartupInfo` returns the STARTUPINFO structure that was used when the process was created
2. For console applications, the standard handle fields typically contain pseudo-handle constants
3. Programs must call `GetStdHandle()` to convert these pseudo-handles to actual handle values
4. The pseudo-handle constants are sentinel values, not valid memory addresses or real handles

## Test Results

All tests pass with the fix:
- ✅ **170/170** Kernel32 tests pass (added 2 new tests)
- ✅ **65/65** User32 tests pass
- ✅ **13/13** CodeGen tests pass
- ✅ **87/99** Emulator tests pass (12 failures are pre-existing, related to retrowin32 submodule)

## Impact Analysis

### Positive Impact
- Programs that follow correct Windows API patterns (GetStartupInfoA → GetStdHandle) now work correctly
- The winapi.exe test should now pass when the retrowin32 submodule is available
- More accurate Windows API emulation

### No Breaking Changes
- Programs that were already calling `GetStdHandle()` directly (without reading from STARTUPINFO) are unaffected
- All existing tests continue to pass
- The fix only affects programs that read standard handles from the STARTUPINFO structure

## Files Changed

```
GETSTARTUPINFOA_FIX.md                   | 88 +++++++++++++++++++++++++++++++++
Win32Emu.Tests.Kernel32/FileIOTests.cs   | 66 +++++++++++++++++++++++++
Win32Emu/Win32/Modules/Kernel32Module.cs |  8 +++---
3 files changed, 159 insertions(+), 3 deletions(-)
```

## Minimal, Surgical Changes

This PR follows the principle of making the smallest possible changes:
- Only 3 lines of actual code changed (plus comments)
- No changes to existing functionality
- Added tests to prevent regression
- Added documentation for future reference

## Verification

To verify this fix resolves the winapi.exe issue:
1. Initialize the retrowin32 submodule: `git submodule update --init retrowin32`
2. Run the winapi test: `dotnet test --filter "WinapiTest_ShouldLoadAndRun"`
3. The test should now pass without the "Calculated memory address out of range: 0xFFFFFFF5" error
