# GetModuleFileNameA Fix

## Problem
GetModuleFileNameA was returning 0 (error) when called with the main executable's module handle (0x00400000), causing subsequent operations to fail.

## Root Cause
The main executable loaded by `PeImageLoader.Load()` in `Emulator.cs` was not being registered in the `_loadedImages` dictionary in `ProcessEnvironment`. This meant that when `GetModuleFileNameForHandle()` was called with the main executable's base address, it couldn't find the module and returned null.

## Solution

### 1. Added RegisterMainExecutable Method
Added a new method to `ProcessEnvironment.cs`:

```csharp
/// <summary>
/// Registers the main executable's LoadedImage so it can be found by GetModuleFileNameA.
/// This should be called after loading the main executable.
/// </summary>
public void RegisterMainExecutable(LoadedImage image, string imagePath)
{
    var normalizedName = Path.GetFileName(imagePath).ToUpperInvariant();
    _loadedModules[normalizedName] = image.BaseAddress;
    _loadedImages[normalizedName] = image;
    _logger.LogInformation("[ProcessEnv] Registered main executable: {ImagePath} at 0x{BaseAddress:X8}", imagePath, image.BaseAddress);
}
```

### 2. Called RegisterMainExecutable in Emulator
Updated `Emulator.cs` to call the new method immediately after loading the executable:

```csharp
_env = new ProcessEnvironment(_vm, 0x01000000, _host, _logger);
// Register the main executable so GetModuleFileNameA can find it
_env.RegisterMainExecutable(_image, path);
// Convert path to Windows-style backslashes for proper parsing by C runtime
_env.InitializeStrings(path, programArgs ?? []);
```

## How It Works

### Before Fix
1. Main executable loaded via `PeImageLoader.Load(path)` in Emulator
2. LoadedImage created but NOT registered in ProcessEnvironment
3. GetModuleFileNameA calls GetModuleFileNameForHandle(0x00400000)
4. GetModuleFileNameForHandle searches _loadedImages - NOT FOUND
5. GetModuleFileNameForHandle searches _loadedModules - NOT FOUND
6. Returns null → GetModuleFileNameA returns 0 (error)

### After Fix
1. Main executable loaded via `PeImageLoader.Load(path)` in Emulator
2. LoadedImage created AND registered via RegisterMainExecutable
3. GetModuleFileNameA calls GetModuleFileNameForHandle(0x00400000)
4. GetModuleFileNameForHandle searches _loadedImages - FOUND!
5. Returns LoadedImage.FilePath
6. GetModuleFileNameA successfully returns the path length

## Testing
The fix has been validated through:
- Successful build with no errors
- Kernel32 tests pass (227/230, with 3 pre-existing unrelated failures)
- Code review confirms the logic flow is correct

## Related Code
- `Win32Emu/Win32/ProcessEnvironment.cs` - RegisterMainExecutable method
- `Win32Emu/Emulator.cs` - Call to RegisterMainExecutable
- `Win32Emu/Win32/Modules/Kernel32Module.cs` - GetModuleFileNameA implementation
