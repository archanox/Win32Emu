# Win32Emu

A Windows 32-bit PE executable emulator for running classic Windows games and applications on modern systems.

## Features

- **Cross-Platform**: Runs on Windows, Linux, and macOS (both x86 and ARM)
- **Hardware-Accelerated**: Uses .NET intrinsics for CPU instruction acceleration
- **Modern CPU Support**: Automatically detects and uses SSE, AVX, and NEON instructions
- **Accurate Emulation**: Full x86 CPU and Windows API emulation

## Components

### Win32Emu (CLI)
Command-line emulator that loads and executes Windows PE executables.

**Usage:**
```bash
Win32Emu <path-to-pe> [--debug]
```

### Win32Emu.Gui
Cross-platform desktop GUI for managing your game library and emulator settings. Built with Avalonia UI.

**Features:**
- Game library with thumbnail views
- File picker for adding games
- Emulator configuration (rendering backend, resolution scaling, memory, Windows version)
- One-click game launching

See [Win32Emu.Gui/README.md](Win32Emu.Gui/README.md) for more details.

## CPU Intrinsics Support

Win32Emu leverages hardware-accelerated SIMD instructions for better performance:

- **x86 hosts**: Uses SSE, SSE2, SSE3, SSE4, AVX, AVX2 instructions
- **ARM hosts**: Uses NEON (AdvSimd) instructions
- **Automatic detection**: CPUID reports accurate host CPU capabilities
- **Fallback support**: Software implementations when intrinsics aren't available

See [INTRINSICS.md](INTRINSICS.md) for detailed documentation.

## Building

```bash
dotnet build Win32Emu.sln
```

## Running Tests

```bash
dotnet test Win32Emu.sln
```
