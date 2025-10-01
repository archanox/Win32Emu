# API Integration Architecture

This document describes the new API-based architecture for integrating the Win32Emu core with the Avalonia GUI.

## Overview

The goal is to move from a simple process-launching approach to a proper API-based integration where:
- Debug messages are routed to a dedicated debug output window with configurable levels
- Emulated stdout output goes to a DOS-like child window
- User32/GDI32 implementations use runtime-generated Avalonia windows
- DirectDraw output is rendered via SDL3 in a child window

## Current State (Implemented)

### 1. IEmulatorHost Interface

Location: `Win32Emu.Gui/Services/IEmulatorHost.cs`

This interface defines callbacks for the GUI to receive emulator events:

```csharp
public interface IEmulatorHost
{
    void OnDebugOutput(string message, DebugLevel level);
    void OnStdOutput(string output);
    void OnWindowCreate(WindowCreateInfo info);
    void OnDisplayUpdate(DisplayUpdateInfo info);
    void OnStateChanged(EmulatorState state);
}
```

**Debug Levels:**
- Trace
- Debug
- Info
- Warning
- Error

### 2. EmulatorWindow

Location: `Win32Emu.Gui/Views/EmulatorWindow.axaml`

A dedicated window for running games with:

**Debug Output Panel (Left Side)**
- Timestamps and level indicators
- Filterable by minimum debug level
- Auto-scrolling with 1000 message limit
- Toggle with F12 key

**Stdout Console (Bottom of Debug Panel)**
- DOS-like black console with monospace font
- Shows emulated program output
- 1000 line limit with auto-scrolling

**Game Display Area (Main)**
- Placeholder for SDL3/DirectDraw rendering
- Will display actual game graphics when integrated

**Status Bar**
- Shows current emulator state (Stopped/Running/Paused/Error)
- Help text for keyboard shortcuts

### 3. EmulatorWindowViewModel

Location: `Win32Emu.Gui/ViewModels/EmulatorWindowViewModel.cs`

Implements `IEmulatorHost` and manages:
- Debug message collection with level filtering
- Stdout output collection
- Emulator state tracking
- Future window creation and display updates

### 4. Updated EmulatorService

Location: `Win32Emu.Gui/Services/EmulatorService.cs`

Now supports both approaches:
- **Process-based (current):** Launches Win32Emu as external process
- **API-based (future):** Will run emulator in-process with IEmulatorHost callbacks

When an `IEmulatorHost` is provided, the service redirects process stdout/stderr to the host callbacks.

## Next Steps (To Be Implemented)

### Phase 1: Core Refactoring

1. **Create IDebugOutput interface in Win32Emu core**
   - Replace all `Console.WriteLine` calls with `debugOutput.Write(message, level)`
   - Default implementation writes to Console
   - GUI implementation routes to EmulatorWindow

2. **Create IStdOutput interface**
   - Capture emulated program stdout/stderr
   - Route to DOS-like console window

3. **Refactor Win32Emu.Program to accept IEmulatorHost**
   - Make the emulator runnable as a library, not just an executable
   - Pass debug and stdout callbacks through the host interface

### Phase 2: Window Management ✅ COMPLETED

**Implementation**: See `PHASE2_IMPLEMENTATION.md` for complete details.

When `CreateWindowExA` is called by emulated applications:
1. User32Module validates and processes the window creation request
2. ProcessEnvironment stores window information and allocates a handle
3. ProcessEnvironment calls `IEmulatorHost.OnWindowCreate()` with complete window details
4. EmulatorWindowViewModel receives the callback and creates an Avalonia Window
5. The window appears on screen with proper title, size, and position

**Status**: ✅ Fully implemented and tested
- Actual Avalonia windows are created for each User32 window request
- Windows are mapped to Win32 handles (HWND → Avalonia Window)
- Windows integrate with the host OS (taskbar, move, resize, etc.)
- Thread-safe window creation via Dispatcher.UIThread
- Proper cleanup when windows are closed

**What's Working**:
- Window creation from CreateWindowExA
- Window positioning and sizing
- Window title display
- Window handle tracking
- Window closing and cleanup

**What's Next**: Phase 4 - Window procedure callbacks and real message queue

### Phase 3: Message Loop and Window Display ✅ COMPLETED

**Implementation**: See `PHASE3_IMPLEMENTATION.md` for complete details.

Implemented the Windows message loop infrastructure that allows emulated applications to:
1. Display windows with ShowWindow
2. Run proper message loops with GetMessageA, TranslateMessage, DispatchMessageA
3. Handle quit messages with PostQuitMessage
4. Send messages directly with SendMessageA
5. Provide default message handling with DefWindowProcA

**Functions Implemented:**
- **ShowWindow** - Makes windows visible with show commands
- **GetMessageA** - Retrieves messages from queue, returns 0 for WM_QUIT
- **TranslateMessage** - Translates virtual-key messages
- **DispatchMessageA** - Dispatches messages to window procedures
- **DefWindowProcA** - Default window procedure
- **PostQuitMessage** - Posts WM_QUIT to exit message loop
- **SendMessageA** - Sends messages directly to windows

**Status**: ✅ Fully implemented and tested
- Message loop pattern fully supported
- MSG structure (28 bytes) properly handled
- Quit message flow working correctly
- All 155 tests pass

**What's Working:**
- Standard Windows message loop: `while(GetMessageA(&msg, NULL, 0, 0) > 0) { TranslateMessage(&msg); DispatchMessageA(&msg); }`
- PostQuitMessage triggers WM_QUIT and exits loop
- ShowWindow logs visibility changes
- Message logging and debugging

**What's Next**: Phase 4 - Window procedure callbacks, real message queue with input routing

### Phase 4: Display Rendering (Future)

1. **Integrate SDL3 for DirectDraw**
   - When DDraw surface is created, initialize SDL3 rendering
   - Route DDraw blits to SDL3 texture updates
   - Embed SDL3 window in EmulatorWindow's display area

2. **Implement OnDisplayUpdate**
   - Call when SDL3/DDraw frame is ready
   - Update Avalonia image/surface with new frame buffer

### Phase 4: Additional Features

1. **Input Routing**
   - Route Avalonia keyboard/mouse input to emulated program
   - Use controller mapping configuration for joystick input

2. **Audio**
   - Route DSound output to system audio
   - Synchronize with video rendering

## Migration Strategy

To maintain backward compatibility during migration:

1. **Dual Mode Support**
   - Keep process-based launching as fallback
   - Add feature flag to enable API-based mode
   - Gradually migrate components

2. **Testing**
   - Test process-based mode continues working
   - Test API-based mode with simple programs
   - Add integration tests for both modes

3. **Documentation**
   - Update README with new architecture
   - Add developer guide for contributing
   - Document API usage examples

## Benefits of API-Based Approach

1. **Better Debugging**
   - Centralized debug output with filtering
   - Real-time visibility into emulator operations
   - Easier troubleshooting

2. **Better User Experience**
   - Native window integration
   - Proper stdout/stderr handling
   - Responsive UI during emulation

3. **Better Performance**
   - No process spawning overhead
   - Direct memory sharing
   - Faster window creation

4. **Better Extensibility**
   - Easy to add new features
   - Plugin architecture possible
   - Better testing infrastructure

## Example Usage (Future)

```csharp
// Create emulator host
var host = new EmulatorWindowViewModel();
var window = new EmulatorWindow { DataContext = host };

// Create emulator service with host
var service = new EmulatorService(configuration, host);

// Launch game in-process
await service.LaunchGameInProcess(game);

// Show emulator window
window.Show();
```

## References

- `IEmulatorHost.cs` - Main interface definition
- `EmulatorWindow.axaml` - UI for emulator window
- `EmulatorWindowViewModel.cs` - View model implementing IEmulatorHost
- `EmulatorService.cs` - Service layer for launching games
