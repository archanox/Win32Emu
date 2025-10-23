# Win32Emu

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/archanox/Win32Emu)

A Windows 32-bit PE executable emulator for running classic Windows games and applications on modern systems.

## Features

- **Cross-Platform**: Runs on Windows, Linux, and macOS (both x86 and ARM)
- **Hardware-Accelerated**: Uses .NET intrinsics for CPU instruction acceleration
- **Modern CPU Support**: Automatically detects and uses SSE, AVX, and NEON instructions
- **Accurate Emulation**: Full x86 CPU and Windows API emulation
- **JIT Caching**: Persistent JIT cache to disk for faster subsequent executions with precompilation support

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
  - Supports remote file I/O when VFS is initialized (access game files from debugger)
- `--backend <SDL|GLFW|Vulkan>`: Select rendering backend (default: SDL)
- `--telemetry-console`: Enable OpenTelemetry with console exporter for logging and metrics
- `--telemetry-otlp [endpoint]`: Enable OpenTelemetry with OTLP exporter (default: http://localhost:4317)

**Environment Variables:**
- `WIN32EMU_BACKEND`: Set rendering backend (SDL, GLFW, or Vulkan)
- `OTEL_EXPORTER_OTLP_ENDPOINT`: OpenTelemetry OTLP endpoint (e.g., `http://localhost:4317`)
  - Automatically enables OpenTelemetry when set
  - Useful for IDE integrations like JetBrains Rider

**Examples:**
```bash
# Run normally (uses SDL backend)
Win32Emu game.exe

# Run with GLFW backend (alternative if SDL has issues)
Win32Emu game.exe --backend GLFW

# Run with Vulkan backend (uses MoltenVK on macOS)
Win32Emu game.exe --backend Vulkan

# Run with enhanced debugging
Win32Emu game.exe --debug

# Run with interactive debugger for step-through debugging
Win32Emu game.exe --interactive-debug

# Run with GDB server for debugging in Ghidra or IDA
Win32Emu game.exe --gdb-server

# Run with GDB server on custom port
Win32Emu game.exe --gdb-server 5678

# Run with OpenTelemetry console exporter for observability
Win32Emu game.exe --telemetry-console

# Run with OpenTelemetry OTLP exporter (for Jaeger, Prometheus, etc.)
Win32Emu game.exe --telemetry-otlp http://localhost:4317
```

**See Also:**
- [SILK_NET_MIGRATION.md](SILK_NET_MIGRATION.md) - Backend system and configuration
- [GHIDRA_DEBUGGING_FAQ.md](GHIDRA_DEBUGGING_FAQ.md) - Troubleshooting "no debugging symbols" and debugging tips
- [DEBUGGING_GUIDE.md](DEBUGGING_GUIDE.md) - Enhanced debugging mode
- [INTERACTIVE_DEBUGGER_GUIDE.md](INTERACTIVE_DEBUGGER_GUIDE.md) - Interactive debugger
- [GDB_SERVER_GUIDE.md](GDB_SERVER_GUIDE.md) - GDB server for Ghidra/IDA integration
- [VFS_DOCUMENTATION.md](VFS_DOCUMENTATION.md) - Virtual File System for game file isolation
- [OPENTELEMETRY_USAGE.md](OPENTELEMETRY_USAGE.md) - OpenTelemetry for logging, metrics, and profiling
- [TELEMETRY_EXAMPLE.md](TELEMETRY_EXAMPLE.md) - Practical examples of using OpenTelemetry
- [RIDER_OPENTELEMETRY_SETUP.md](RIDER_OPENTELEMETRY_SETUP.md) - JetBrains Rider integration guide
- [JIT_CACHE_IMPLEMENTATION.md](JIT_CACHE_IMPLEMENTATION.md) - JIT caching to disk for faster emulation
- [JIT_CACHE_EXAMPLES.md](JIT_CACHE_EXAMPLES.md) - JIT cache usage examples and best practices

### Win32Emu.Gui
Cross-platform desktop GUI for managing your game library and emulator settings. Built with Avalonia UI.

**Features:**
- Game library with thumbnail views
- File picker for adding games
- Emulator configuration (rendering backend, resolution scaling, memory, Windows version)
- One-click game launching

See [Win32Emu.Gui/README.md](Win32Emu.Gui/README.md) for more details.

## Backend System

Win32Emu uses pluggable backends for cross-platform multimedia support:

### Rendering Backends
- **SDL** (default): SDL3-CS - Native Metal on macOS, Vulkan on Linux, DirectX 12 on Windows. Best compatibility, hardware-accelerated
- **GLFW**: Silk.NET.GLFW + OpenGL - Alternative for systems where SDL has issues
- **Vulkan**: Silk.NET.Vulkan - Modern GPU API with cross-platform support (uses MoltenVK on macOS)

### Audio Backend
- **SDL Audio**: SDL3-CS audio when using SDL backend - Native audio support
- **OpenAL**: Silk.NET.OpenAL - Cross-platform audio support for GLFW/Vulkan backends

### Input Backend
- **SDL Input**: SDL3-CS input when using SDL backend - Keyboard, mouse, and joystick support
- **Silk.NET.Input**: Unified keyboard, mouse, and gamepad support for GLFW/Vulkan backends

**Configuration:**
- Command-line: `--backend SDL`, `--backend GLFW`, or `--backend Vulkan`
- Environment variable: `WIN32EMU_BACKEND=SDL`, `WIN32EMU_BACKEND=GLFW` or `WIN32EMU_BACKEND=Vulkan`
- Programmatic: `BackendFactory.CurrentBackendType = BackendType.SDL;`

See [SILK_NET_MIGRATION.md](SILK_NET_MIGRATION.md) for detailed documentation.

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
