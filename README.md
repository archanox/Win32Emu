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
Win32Emu <path-to-pe> [options]
```

**Options:**
- `--debug`: Enable enhanced debugging mode with automatic error detection
- `--interactive-debug`: Enable interactive step-through debugger (GDB-like)
- `--gdb-server [port]`: Start GDB server for remote debugging with Ghidra/IDA (default port: 1234)

**Examples:**
```bash
# Run normally
Win32Emu game.exe

# Run with enhanced debugging
Win32Emu game.exe --debug

# Run with interactive debugger for step-through debugging
Win32Emu game.exe --interactive-debug

# Run with GDB server for debugging in Ghidra or IDA
Win32Emu game.exe --gdb-server

# Run with GDB server on custom port
Win32Emu game.exe --gdb-server 5678
```

See [DEBUGGING_GUIDE.md](DEBUGGING_GUIDE.md), [INTERACTIVE_DEBUGGER_GUIDE.md](INTERACTIVE_DEBUGGER_GUIDE.md), and [GDB_SERVER_GUIDE.md](GDB_SERVER_GUIDE.md) for more details.

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
