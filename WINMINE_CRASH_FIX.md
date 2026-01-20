# Winmine.exe Crash Fix

## Problem Statement

Winmine.exe (Windows Minesweeper) from Windows 3.1 crashes during initialization when loading in the Win32Emu emulator. The crash occurs during Win16 module registration, with the log cutting off at:

```
[09:43:36] [DBG] [Emulator] [Loader] Creating Win16 KE...
```

## Root Cause Analysis

The investigation revealed the following issues:

1. **Missing Module Mapping**: Winmine.exe is a 16-bit Windows NE (New Executable) format binary that imports functions from the "SHELL" module (specifically ordinal 22).

2. **Incorrect Module Resolution**: The NE loader was mapping "SHELL" to "SHELL.DLL" using the default case in the module mapping switch, but the emulator only had "SHELL32.DLL" registered in the dispatcher.

3. **Missing Thunking Layer**: There was no Win16ShellModule to handle the translation between Win16 SHELL calls and Win32 SHELL32.DLL functions.

### Evidence from Logs

```
[09:43:36] [DBG] [Emulator] [NE Loader] Import by ordinal: SHELL!22 -> 0x0F040058
...
[09:43:36] [DBG] [Emulator] [NE Loader] Mapping Win16 module SHELL to SHELL.DLL
```

The loader was trying to map to "SHELL.DLL" but only "SHELL32.DLL" existed, causing the module lookup to fail.

## Solution Implemented

### 1. Added SHELL Module Mapping (NeImageLoader.cs)

**File**: `Win32Emu/Loader/NeImageLoader.cs`

Added explicit mapping from Win16 "SHELL" to Win32 "SHELL32.DLL":

```csharp
var win32Module = normalizedModule switch
{
    "KERNEL" => "KERNEL32.DLL",
    "USER" => "USER32.DLL",
    "GDI" => "GDI32.DLL",
    "KEYBOARD" => "USER32.DLL",
    "SOUND" => "WINMM.DLL",
    "SYSTEM" => "KERNEL32.DLL",
    "SHELL" => "SHELL32.DLL",      // Added this line
    _ => normalizedModule + ".DLL"
};
```

### 2. Created Win16ShellModule (Win16AuxiliaryModules.cs)

**File**: `Win32Emu/Win32/Win16/Win16AuxiliaryModules.cs`

Created a new Win16ShellModule class that acts as a thunking layer between Win16 SHELL calls and Win32 SHELL32.DLL:

```csharp
internal class Win16ShellModule : Win16ThunkingLayer, IWin32ModuleAsync
{
    public Win16ShellModule(IWin32ModuleUnsafe shell32Module, ILogger logger)
        : base(shell32Module, logger)
    {
    }

    public string Name => "SHELL";
    
    // Handles 30+ SHELL function mappings including:
    // - ShellAbout, ShellExecute, ShellExecuteEx
    // - DragAcceptFiles, DragFinish, DragQueryFile
    // - ExtractIcon, SHBrowseForFolder, SHFileOperation
    // - SHGetFileInfo, SHGetMalloc, SHGetPathFromIDList
    // - SHGetSpecialFolderLocation, SHGetDesktopFolder
    // And many more...
}
```

### 3. Registered Win16ShellModule (Emulator.cs)

**File**: `Win32Emu/Emulator.cs`

Added SHELL32.DLL lookup and Win16ShellModule registration:

```csharp
_logger.LogDebug("[Loader] Looking up SHELL32.DLL module");
if (!_dispatcher.TryGetModule("SHELL32.DLL", out var shell32Module) || shell32Module == null)
{
    throw new InvalidOperationException("SHELL32.DLL not found in dispatcher...");
}
var shell32 = shell32Module;

// ... other Win16 modules ...

_logger.LogDebug("[Loader] Creating Win16 SHELL module");
_dispatcher.RegisterModule(new Win32.Win16.Win16ShellModule(shell32, _logger));
```

## Testing Results

### Build Status
✅ Build succeeded with no errors

### Test Results
✅ All Win16 thunking tests passed (9/9)
- Win16KeyboardModule_GetKeyState_ForwardsToUser32
- Win16KernelModule_GetVersion_ForwardsToKernel32
- Win16KernelModule_UnknownFunction_ReturnsFalse
- Win16GdiModule_GetDeviceCaps_ForwardsToGdi32
- Win16SystemModule_GetTickCount_ForwardsToKernel32
- Win16Modules_HaveCorrectNames
- Win16UserModule_MessageBeep_ForwardsToUser32
- Win16SoundModule_SndPlaySound_ForwardsToWinMM
- SolExe_LoadsSuccessfully_WithWin16ModuleRegistration

## Expected Behavior After Fix

With this fix applied, winmine.exe should:
1. Successfully load all Win16 module mappings including SHELL
2. Complete the Win16 thunking module registration without crashing
3. Proceed to execute the entry point at 0x0001228C
4. Run successfully in the emulator

## Technical Details

### Win16 to Win32 Thunking Architecture

The Win32Emu emulator uses a thunking layer to translate 16-bit Windows API calls to 32-bit equivalents:

```
NE Executable (16-bit)
    ↓
Win16 Module (e.g., SHELL)
    ↓
Win16ThunkingLayer (Win16ShellModule)
    ↓
Win32 Module (SHELL32.DLL)
```

The thunking layer:
- Converts function names to uppercase
- Forwards calls to the appropriate Win32 module
- Handles parameter translation if needed
- Provides proper error handling and logging

### Module Registration Order

Win16 modules are registered in the following order:
1. KERNEL → KERNEL32.DLL
2. USER → USER32.DLL
3. GDI → GDI32.DLL
4. KEYBOARD → USER32.DLL
5. SYSTEM → KERNEL32.DLL
6. SOUND → WINMM.DLL
7. SHELL → SHELL32.DLL (newly added)

## Files Changed

1. `Win32Emu/Loader/NeImageLoader.cs` - Added SHELL module mapping
2. `Win32Emu/Win32/Win16/Win16AuxiliaryModules.cs` - Created Win16ShellModule class
3. `Win32Emu/Emulator.cs` - Added SHELL32.DLL lookup and Win16ShellModule registration

## Related Issues

This fix addresses the crash reported in the issue where winmine.exe failed to load due to missing SHELL module support in the Win16 thunking layer.
