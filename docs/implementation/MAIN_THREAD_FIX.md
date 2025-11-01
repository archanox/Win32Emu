# Main Thread Architecture Fix

## Problem

Some graphics backends (particularly on macOS) require initialization and operation on the main thread. Previously, Win32Emu had two separate executables:

1. **Win32Emu** - CLI executable that ran the emulator
2. **Win32Emu.Gui** - Avalonia GUI application

When the GUI launched games, it used `Task.Run()` to run the emulator on a background thread. This caused issues with backends like SDL, Metal, and GLFW on macOS, which require main thread execution.

## Solution

The architecture has been refactored to ensure proper main thread usage:

### Changes Made

1. **Win32Emu converted to Library**
   - Changed from `OutputType=Exe` to `OutputType=Library`
   - Removed standalone executable functionality
   - Created public `EmulatorLauncher` class for programmatic access

2. **Win32Emu.Gui is now the single executable**
   - Added `--nogui` flag for CLI mode
   - When `--nogui` is specified, runs emulator directly on main thread
   - When GUI mode is used, leverages `AvaloniaRenderingBackend` which doesn't require main thread

3. **EmulatorLauncher API**
   - New public class: `Win32Emu.EmulatorLauncher`
   - Provides `Launch(string[] args)` method for CLI emulation
   - Can be called from main thread to ensure backend compatibility

## Usage

### CLI Mode (Main Thread Execution)

```bash
# Run with --nogui flag to execute on main thread
Win32Emu.Gui --nogui game.exe

# All previous CLI options are supported
Win32Emu.Gui --nogui game.exe --backend SDL --debug
```

### GUI Mode

```bash
# Launch the GUI application
Win32Emu.Gui

# The GUI uses AvaloniaRenderingBackend which doesn't require main thread
```

## Technical Details

### Main Thread Execution Path

When `--nogui` is used:
1. `Program.Main()` in Win32Emu.Gui checks for `--nogui` flag
2. Calls `EmulatorLauncher.Launch()` directly on the main thread
3. Emulator runs synchronously, ensuring backends initialize on main thread
4. Process exits when emulation completes

### GUI Execution Path

When GUI mode is used:
1. `Program.Main()` starts Avalonia application
2. Main thread becomes Avalonia UI thread
3. Games launched from GUI use `AvaloniaRenderingBackend`
4. This backend doesn't create separate windows or use native graphics APIs
5. Frame buffers are passed to Avalonia controls via callbacks
6. No main thread requirement for graphics

### Backend Compatibility

| Backend | CLI (--nogui) | GUI Mode | Notes |
|---------|---------------|----------|-------|
| SDL | ✅ Main thread | ❌ Not used | GUI uses Avalonia backend |
| GLFW | ✅ Main thread | ❌ Not used | GUI uses Avalonia backend |
| Vulkan | ✅ Main thread | ❌ Not used | GUI uses Avalonia backend |
| Metal | ✅ Main thread | ❌ Not used | GUI uses Avalonia backend |
| Software | ✅ Main thread | ❌ Not used | GUI uses Avalonia backend |
| Avalonia | ❌ N/A | ✅ Used | Automatically selected in GUI mode |

## macOS Specific Benefits

On macOS, many graphics APIs (Cocoa, Metal, OpenGL) have strict main thread requirements:

1. **NSApplication** must run on the main thread
2. **Metal** device creation and command submission should be on main thread
3. **SDL** window creation requires main thread

The new architecture ensures:
- CLI mode (`--nogui`) runs all backends on main thread ✅
- GUI mode uses Avalonia backend, avoiding native graphics APIs ✅

## Migration Guide

### For CLI Users

Old:
```bash
Win32Emu game.exe --backend SDL
```

New:
```bash
Win32Emu.Gui --nogui game.exe --backend SDL
```

### For Developers

Old (embedding emulator):
```csharp
var emulator = new Emulator(null, logger, telemetryService);
emulator.LoadExecutable(path, args, debug, interactive, memory, gdb, port);
emulator.Run();
```

New (same API works):
```csharp
// Option 1: Use EmulatorLauncher API (loggerFactory is optional - pass null to use default console logger)
EmulatorLauncher.Launch(args, loggerFactory: null);

// Option 2: Direct emulator usage still works
var emulator = new Emulator(null, logger, telemetryService);
emulator.LoadExecutable(path, args, debug, interactive, memory, gdb, port);
emulator.Run();
```

## Future Improvements

Potential enhancements:
1. **Detached window mode for GUI**: Allow GUI to use SDL/GLFW backends in separate windows
2. **Thread affinity management**: Explicit control over which thread runs emulation
3. **Multi-process architecture**: Run emulator in separate process with IPC

## References

- [SDL3 Documentation](https://wiki.libsdl.org/SDL3/FrontPage)
- [Metal Threading Best Practices](https://developer.apple.com/documentation/metal/performing_calculations_on_a_gpu)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
