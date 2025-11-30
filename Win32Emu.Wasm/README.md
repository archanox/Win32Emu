# Win32Emu.Wasm

A Blazor WebAssembly frontend for the Win32Emu emulator, providing an interactive web-based interface for running classic Windows applications in the browser.

## Overview

This project provides a proof-of-concept web interface for Win32Emu with the following features:

- **Interactive Display**: HTML5 Canvas for rendering DirectDraw output
- **Audio Output**: Web Audio API integration (placeholder)
- **Dual Output Panels**: 
  - Standard Output panel for application console output
  - Debug Output panel for emulator diagnostics
- **File Upload**: Upload Windows PE executables directly in the browser
- **Status Monitoring**: Real-time display of instructions executed, FPS, and audio status

## Current Status

This is a **proof-of-concept** implementation. The Win32Emu core library is designed for native platforms with dependencies on:
- SDL3 for rendering and input
- Silk.NET for various backends (OpenGL, Vulkan, etc.)
- Native CPU intrinsics for performance
- UnicornEngine for CPU emulation fallback

These dependencies do not support WebAssembly, so full emulation is not yet available in the browser.

## Future Work

To enable full web-based emulation, the following work is needed:

1. **WASM-Compatible Rendering Backend**
   - Implement `IWasmRenderingBackend` using HTML5 Canvas and JavaScript interop
   - Handle DirectDraw surface management through canvas contexts
   - Implement pixel format conversions for browser compatibility

2. **Web Audio Integration**
   - Create `WasmAudioBackend` implementing `IAudioBackend`
   - Use Web Audio API for DirectSound emulation
   - Handle audio buffer management and streaming

3. **CPU Emulation Adaptation**
   - Provide WASM-compatible fallbacks for SIMD intrinsics
   - Handle threading limitations in WASM
   - Optimize JIT compilation for browser environment

4. **File System Abstraction** ✅ COMPLETED
   - Browser-based VFS (`BrowserVirtualFileSystem`) for in-memory file storage
   - Case-insensitive file access (Windows compatibility)
   - File/folder upload support via HTML5 File API
   - Future: IndexedDB for persistent storage

## Building

```bash
# Build the project
dotnet build Win32Emu.Wasm.csproj

# Publish for deployment
dotnet publish Win32Emu.Wasm.csproj --configuration Release

# The output will be in bin/Release/net10.0/publish/wwwroot/
```

## Deployment

This project is automatically deployed to GitHub Pages as part of the main documentation site:

- **Live URL**: https://archanox.github.io/Win32Emu/emulator/

The deployment is handled by the `.github/workflows/cpu-test-results.yml` workflow.

## Development

To run locally during development:

```bash
dotnet watch run
```

This will start a development server with hot reload enabled.

## Architecture

The project follows Blazor WASM patterns:

- `Pages/Home.razor`: Main emulator page with canvas, controls, and output panels
- `Layout/`: Navigation and page layout components
- `wwwroot/`: Static assets (CSS, JS, icons)
  - `wwwroot/index.html`: Entry point with JavaScript interop functions

JavaScript interop functions in `index.html`:
- `initializeEmulator(canvasId)`: Initialize the HTML5 canvas for rendering
- `initializeAudio()`: Initialize Web Audio API context (placeholder)

## Testing with ign_teas

The primary goal is to support running `ign_teas.exe` with:
- DirectDraw window rendering on canvas
- Audio output through Web Audio API
- Console and debug output in the respective panels

Once the backend integration is complete, this will enable testing classic Windows games directly in the browser, including on mobile devices.

## Contributing

Contributions are welcome! Key areas where help is needed:

1. Implementing WASM-compatible rendering backend
2. Web Audio API integration for DirectSound
3. Performance optimization for browser environment
4. Mobile touch input handling
5. UI/UX improvements

## License

This project is part of Win32Emu and follows the same license terms.
