# Win32Emu Test Programs

Test programs for Win32Emu emulator, including DirectDraw examples.

## Building

These examples are built using MinGW cross-compiler:

```bash
make all
```

Requirements:
- mingw-w64 (i686-w64-mingw32-gcc)

## DirectDraw Examples

### simple_ddraw.exe
Basic DirectDraw test with animated XOR pattern. Tests:
- DirectDraw initialization
- Primary surface creation
- Surface locking/unlocking
- Timer-based animation (no threading)

### hugi.exe
Advanced DirectDraw example from [Hugi 16 article](https://hugi.scene.org/online/coding/hugi%2016%20-%20coddraw.htm). Tests:
- Fullscreen DirectDraw mode
- Double buffering with surface flipping
- Multi-threaded rendering
- Critical section synchronization

## Running

These executables are automatically included in the Win32Emu.Wasm frontend and can be loaded from the "📦 Sample Executables" section.

For desktop testing:
```bash
Win32Emu.Gui --nogui simple_ddraw.exe
Win32Emu.Gui --nogui hugi.exe
```

See [Win32Emu.Wasm/wwwroot/samples/README.md](../../Win32Emu.Wasm/wwwroot/samples/README.md) for detailed documentation.

## Legacy Note

The original README mentioned `cargo minibuild` which was part of a Rust-based build system (retrowin32). The current examples use standard MinGW compilation via Makefile.
