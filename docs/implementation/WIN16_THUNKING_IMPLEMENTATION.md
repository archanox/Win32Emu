# Win16 (NE) Support Implementation

## Overview

Win32Emu now includes support for Win16 (New Executable / NE format) applications through a thunking layer that translates Win16 API calls to their Win32 equivalents. This allows running classic 16-bit Windows applications on the emulator.

## Architecture

### Thunking Layer Concept

Win16 applications use the PASCAL calling convention and 16-bit parameters, while Win32 uses STDCALL/CDECL and 32-bit parameters. The thunking layer bridges this gap by:

1. **Module Name Mapping**: Maps Win16 module names to Win32 equivalents
   - `KERNEL` → `KERNEL32.DLL`
   - `USER` → `USER32.DLL`
   - `GDI` → `GDI32.DLL`
   - `KEYBOARD` → `USER32.DLL`
   - `SYSTEM` → `KERNEL32.DLL`
   - `SOUND` → `WINMM.DLL`

2. **Calling Convention Translation**: Handles differences between PASCAL and STDCALL
   - PASCAL: Arguments pushed left-to-right, callee cleans stack
   - STDCALL: Arguments pushed right-to-left, callee cleans stack
   - The base thunking layer provides utilities for both conventions

3. **Parameter Size Conversion**: Converts between 16-bit and 32-bit parameters
   - Handles (HWND, HDC, etc.): Zero-extension from 16-bit to 32-bit
   - Integers and other values: Appropriate size conversion
   - Pointers: Handled as 32-bit addresses in flat memory model

### Win16 Thunking Modules

Six Win16 thunking modules are implemented:

#### Win16KernelModule (`KERNEL` → `KERNEL32.DLL`)
Handles core system functions:
- Memory management (GlobalAlloc, LocalAlloc, etc.)
- File I/O (_lopen, _lread, _lwrite, etc.)
- String operations (lstrcpy, lstrlen, etc.)
- Module loading (LoadLibrary, GetProcAddress, etc.)
- Version information (GetVersion)

#### Win16UserModule (`USER` → `USER32.DLL`)
Handles window and UI functions:
- Window management (CreateWindow, ShowWindow, etc.)
- Message handling (GetMessage, SendMessage, etc.)
- Dialog functions (DialogBox, GetDlgItem, etc.)
- Menu operations (CreateMenu, AppendMenu, etc.)
- Input functions (GetKeyState, GetCursorPos, etc.)

#### Win16GdiModule (`GDI` → `GDI32.DLL`)
Handles graphics functions:
- Device context operations (CreateDC, GetDeviceCaps, etc.)
- Drawing primitives (LineTo, Rectangle, Ellipse, etc.)
- Text output (TextOut, DrawText, etc.)
- Pen and brush management (CreatePen, CreateBrush, etc.)
- Bitmap operations (BitBlt, StretchBlt, etc.)

#### Win16KeyboardModule (`KEYBOARD` → `USER32.DLL`)
Handles keyboard-specific functions:
- Key state queries (GetKeyState, GetAsyncKeyState, etc.)
- Keyboard configuration (GetKeyboardType, MapVirtualKey, etc.)

#### Win16SystemModule (`SYSTEM` → `KERNEL32.DLL`)
Handles system timer and configuration:
- Timer functions (GetTickCount)
- System time (GetSystemTime, GetLocalTime, etc.)

#### Win16SoundModule (`SOUND` → `WINMM.DLL`)
Handles multimedia and sound:
- Sound playback (sndPlaySound)
- Driver management (OpenDriver, CloseDriver, etc.)

## Implementation Details

### Base Thunking Layer

The `Win16ThunkingLayer` abstract base class provides common functionality:

```csharp
public abstract class Win16ThunkingLayer
{
    protected uint ConvertHandle16To32(ushort handle16);
    protected ushort ConvertHandle32To16(uint handle32);
    protected ushort Read16FromStack(ICpu cpu, VirtualMemory memory, int offset);
    protected uint Read32FromStack(ICpu cpu, VirtualMemory memory, int offset);
    protected void LogWin16Call(string export, string details = "");
    public abstract bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue);
}
```

### Module Registration

Win16 modules are automatically registered when loading NE format executables:

```csharp
if (format == ExecutableFormat.NE)
{
    // Get Win32 modules
    var kernel32 = _dispatcher.TryGetModule("KERNEL32.DLL", out var k32Module) ? k32Module! : ...;
    var user32 = _dispatcher.TryGetModule("USER32.DLL", out var u32Module) ? u32Module! : ...;
    // ... etc
    
    // Register Win16 thunking wrappers
    _dispatcher.RegisterModule(new Win16KernelModule(kernel32, _logger));
    _dispatcher.RegisterModule(new Win16UserModule(user32, _logger));
    // ... etc
}
```

### Function Forwarding

Most Win16 functions are directly compatible and forward to Win32 implementations:

```csharp
case "GLOBALALLOC":
case "GLOBALFREE":
    LogWin16Call(export, "forwarding to KERNEL32");
    return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);
```

## Limitations

### Current Limitations

1. **Simplified Thunking**: The current implementation uses a simplified approach where most functions with compatible parameters are forwarded directly to Win32 implementations.

2. **Parameter Translation**: Some Win16 functions may have different parameter semantics that aren't fully translated:
   - Segment:offset far pointers (not yet mapped to flat addresses)
   - Certain structure layouts that differ between Win16 and Win32
   - Functions requiring explicit 16-bit to 32-bit conversions

3. **PASCAL Calling Convention**: While the infrastructure supports PASCAL convention detection, the actual stack cleanup and parameter ordering is handled by the underlying Win32 implementations.

4. **Handle Mapping**: Handle conversion uses simple zero-extension/truncation. Complex handle types (especially for GDI objects, windows) may need dedicated mapping tables.

### Compatible Functions

Functions that work well with the current thunking approach:
- Memory allocation/deallocation (handles are compatible)
- File I/O operations (file handles and basic parameters compatible)
- String functions (ANSI strings compatible)
- Simple window operations (window handles compatible with zero-extension)
- Message passing (message IDs and basic parameters compatible)
- Basic GDI operations (drawing primitives, device contexts)

### Functions Requiring Additional Work

Functions that may need enhanced thunking:
- Complex structures with different layouts (LOGFONT, WNDCLASS, etc.)
- Far pointer conversions (segment:offset to flat addresses)
- Functions with Win16-specific semantics (selectors, local/global heaps)
- Advanced GDI functions with complex parameter marshalling

## Usage

### Loading NE Executables

Win16 NE executables are automatically detected and loaded:

```bash
# Run a Win16 application
dotnet run --project Win32Emu -- path/to/win16app.exe

# The loader will:
# 1. Detect NE format
# 2. Register Win16 thunking modules
# 3. Map Win16 imports to Win32 equivalents
# 4. Execute the application
```

### Debugging Win16 Applications

Enable enhanced logging to see Win16 thunking in action:

```bash
# Enable debug logging
dotnet run --project Win32Emu -- --debug path/to/win16app.exe

# You'll see logs like:
# [Win16 Thunk] GLOBALALLOC - forwarding to KERNEL32
# [Win16 Thunk] CREATEWINDOW - forwarding to USER32
```

## Testing

Comprehensive tests verify Win16 thunking functionality:

```bash
# Run Win16 thunking tests
dotnet test --filter "Win16Thunking"

# Tests include:
# - Module name correctness (KERNEL, USER, GDI, etc.)
# - Function forwarding to Win32 modules
# - Unknown function rejection
# - Each Win16 module's core functionality
```

## Future Enhancements

Potential improvements for more complete Win16 support:

1. **Advanced Parameter Translation**
   - Implement full structure marshalling for complex types
   - Add segment:offset to flat address conversion
   - Create handle mapping tables for GDI objects

2. **PASCAL Convention Handling**
   - Explicit PASCAL calling convention support
   - Stack reordering for left-to-right parameter passing
   - Proper stack cleanup tracking

3. **Win16-Specific Features**
   - Local/global heap emulation
   - Selector management for memory segments
   - Task management and multitasking

4. **Compatibility Database**
   - Track known compatible and incompatible functions
   - Provide workarounds for specific applications
   - Document application-specific issues

## References

### Win16 to Win32 Thunking Resources

- **Microsoft MSDN**: Original Win32 thunking documentation
- **win3mu**: Open-source Win16 emulator - https://github.com/skochinsky/win3mu
- **winevdm**: Wine-based Win16 on Win64 - https://github.com/otya128/winevdm
- **win16test**: Win16 testing tools - https://github.com/BackupGGCode/win16test

### Win16 API Documentation

- Win16 API reference (MSDN archives)
- "Programmer's Guide to Windows" (Charles Petzold)
- Win16 SDK documentation

## Contributing

To add support for additional Win16 functions:

1. Identify the Win16 module (KERNEL, USER, GDI, etc.)
2. Check if the function parameters are compatible with Win32
3. Add the function name to the appropriate thunking module switch statement
4. For incompatible functions, implement custom parameter translation
5. Add test cases to verify the function works correctly

Example:

```csharp
// In Win16UserModule.cs
case "MYFUNCTION":
    LogWin16Call(export, "forwarding to USER32");
    return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);
```
