# DirectDraw Test Examples for Win32Emu

This directory contains simple DirectDraw test executables that can be used to test Win32Emu's DirectDraw emulation.

## Examples

### simple_ddraw.exe
A basic DirectDraw example that demonstrates:
- DirectDraw initialization
- Primary surface creation
- Direct surface access and locking
- Animated pattern rendering using a timer
- Proper cleanup and resource management

Features:
- Simpler than the Hugi example (no threading)
- Uses WM_TIMER for frame updates
- Press ESC to exit
- Good for testing basic DirectDraw functionality

### hugi.exe
Based on the classic Hugi 16 article "Coding DirectDraw" by Submissive/Cubic.
Original article: https://hugi.scene.org/online/coding/hugi%2016%20-%20coddraw.htm

Demonstrates:
- Advanced DirectDraw setup with fullscreen mode
- Double buffering with backbuffer flipping
- Multi-threaded rendering
- Critical sections for thread synchronization
- Surface restoration on loss
- Animated XOR pattern (320x240 @ 8-bit color)

Features:
- More complex than simple_ddraw.exe
- Uses a separate rendering thread
- Tests thread synchronization
- Runs for 3200 frames then exits
- Press any key to exit early

## Building

The examples are built using MinGW cross-compiler:

```bash
cd /path/to/Win32Emu/retrowin32/exe/cpp
make all
```

Requirements:
- mingw-w64 (i686-w64-mingw32-gcc)
- DirectDraw headers and libraries

## Running in Win32Emu

### Desktop (Win32Emu.Gui)

```bash
Win32Emu.Gui --nogui simple_ddraw.exe
Win32Emu.Gui --nogui hugi.exe
```

### WASM Frontend

1. Navigate to the WASM frontend in your browser
2. Click on the "📦 Sample Executables" section
3. Select one of the sample buttons to load it
4. Click "Start" to run the emulator

The samples are automatically included in the WASM build and served from the `wwwroot/samples/` directory.

## Testing DirectDraw Implementation

These examples are useful for testing:
- ✅ DirectDraw initialization (DirectDrawCreate, QueryInterface)
- ✅ Cooperative level setting
- ✅ Surface creation (primary, backbuffer)
- ✅ Surface locking/unlocking
- ✅ Pixel format handling
- ✅ Surface flipping
- ✅ Surface restoration
- ✅ Memory pitch handling
- ✅ Thread synchronization (hugi.exe only)

## Technical Notes

### Compilation Details
- Target: 32-bit Windows PE executables (i686)
- Compiler: MinGW-w64 GCC
- Linked libraries: kernel32, user32, gdi32, ddraw, uuid, dxguid
- Subsystem: Windows GUI

### Memory Layout
Both examples use direct memory access to surface buffers, which tests:
- Proper lpSurface pointer handling
- Correct pitch calculation
- Memory boundary checking
- Pixel format conversion (if applicable)

### Threading (hugi.exe)
The Hugi example uses Windows threading APIs:
- CreateThread for render thread
- Critical sections for synchronization
- TerminateThread for cleanup

This is useful for testing Win32Emu's thread emulation capabilities.

## Adding More Examples

To add more DirectDraw examples:

1. Create a new .c or .cpp file in this directory
2. Add the target to the Makefile:
   ```makefile
   EXAMPLES = hugi.exe simple_ddraw.exe yourexample.exe
   
   yourexample.exe: yourexample.c
       $(CC) $(CFLAGS) $< -o $@ $(LDFLAGS) $(LIBS)
   ```
3. Build with `make all`
4. Copy to WASM wwwroot: `cp yourexample.exe ../../Win32Emu.Wasm/wwwroot/samples/`
5. Update Home.razor to add a button for your example

## Resources

- [DirectDraw Documentation (MSDN)](https://learn.microsoft.com/en-us/windows/win32/directdraw/directdraw)
- [Hugi 16 - Coding DirectDraw](https://hugi.scene.org/online/coding/hugi%2016%20-%20coddraw.htm)
- [Win32Emu DirectDraw Implementation](../../Win32Emu/Win32/DirectDraw/)

## License

These examples are provided as test cases for Win32Emu. The Hugi example is based on the article by Submissive/Cubic from Hugi 16.
