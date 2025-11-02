# Win32Emu

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/archanox/Win32Emu)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=archanox_Win32Emu&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=archanox_Win32Emu)
[![Codeac](https://static.codeac.io/badges/2-1063646816.svg "Codeac")](https://app.codeac.io/github/archanox/Win32Emu)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/421f38603fdf478dbffee73008830ade)](https://app.codacy.com/gh/archanox/Win32Emu/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

A Windows 32-bit PE executable emulator for running classic Windows games and applications on modern systems.

## Features

- **Cross-Platform**: Runs on Windows, Linux, and macOS (both x86 and ARM)
- **Hardware-Accelerated**: Uses .NET intrinsics for CPU instruction acceleration
- **Modern CPU Support**: Automatically detects and uses SSE, AVX, and NEON instructions
- **Accurate Emulation**: Full x86 CPU and Windows API emulation
- **JIT Caching**: Persistent JIT cache to disk for faster subsequent executions with precompilation support
- **Disc Image Support**: CHD (Compressed Hunks of Data) detection and validation for CD-ROM games

## Components

### Win32Emu.Gui
Cross-platform desktop GUI for managing your game library and emulator settings. Built with Avalonia UI.

**Note:** This is now the primary executable for Win32Emu. The standalone CLI has been integrated into this application.

**Features:**
- Game library with thumbnail views
- File picker for adding games
- Emulator configuration (rendering backend, resolution scaling, memory, Windows version)
- One-click game launching
- **CLI mode** with `--nogui` flag for headless operation

**GUI Usage:**
```bash
# Launch the GUI application
Win32Emu.Gui
```

**CLI Usage (with --nogui flag):**
```bash
# Run in command-line mode without GUI
Win32Emu.Gui --nogui <path-to-pe> [options]
```

**CLI Options:**
- `--debug`: Enable enhanced debugging mode with automatic error detection
- `--interactive-debug`: Enable interactive step-through debugger (GDB-like)
- `--gdb-server [port]`: Start GDB server for remote debugging with Ghidra/IDA (default port: 1234)
  - Supports remote file I/O when VFS is initialized (access game files from debugger)
- `--backend <SDL|GLFW|Vulkan|Metal|Software>`: Select rendering backend (default: SDL)
- `--telemetry-console`: Enable OpenTelemetry with console exporter for logging and metrics
- `--telemetry-otlp [endpoint]`: Enable OpenTelemetry with OTLP exporter (default: http://localhost:4317)

**Environment Variables:**
- `WIN32EMU_BACKEND`: Set rendering backend (SDL, GLFW, Vulkan, Metal, or Software)
- `OTEL_EXPORTER_OTLP_ENDPOINT`: OpenTelemetry OTLP endpoint (e.g., `http://localhost:4317`)
  - Automatically enables OpenTelemetry when set
  - Useful for IDE integrations like JetBrains Rider

**CLI Examples:**
```bash
# Run normally (uses SDL backend)
Win32Emu.Gui --nogui game.exe

# Run with GLFW backend (alternative if SDL has issues)
Win32Emu.Gui --nogui game.exe --backend GLFW

# Run with Vulkan backend (uses MoltenVK on macOS)
Win32Emu.Gui --nogui game.exe --backend Vulkan

# Run with Metal backend (macOS only, hardware-accelerated)
Win32Emu.Gui --nogui game.exe --backend Metal

# Run with Software backend (CPU-based, no GPU required)
Win32Emu.Gui --nogui game.exe --backend Software

# Run with enhanced debugging
Win32Emu.Gui --nogui game.exe --debug

# Run with interactive debugger for step-through debugging
Win32Emu.Gui --nogui game.exe --interactive-debug

# Run with GDB server for debugging in Ghidra or IDA
Win32Emu.Gui --nogui game.exe --gdb-server

# Run with GDB server on custom port
Win32Emu.Gui --nogui game.exe --gdb-server 5678

# Run with OpenTelemetry console exporter for observability
Win32Emu.Gui --nogui game.exe --telemetry-console

# Run with OpenTelemetry OTLP exporter (for Jaeger, Prometheus, etc.)
Win32Emu.Gui --nogui game.exe --telemetry-otlp http://localhost:4317
```

**Important Note for macOS Users:**
Running with `--nogui` ensures that rendering backends run on the main thread, which is required for proper operation of Metal, SDL, and other graphics APIs on macOS.

See [Win32Emu.Gui/README.md](Win32Emu.Gui/README.md) for more details about the GUI features.

**See Also:**
- [docs/implementation/SILK_NET_MIGRATION.md](docs/implementation/SILK_NET_MIGRATION.md) - Backend system and configuration
- [docs/guides/GHIDRA_DEBUGGING_FAQ.md](docs/guides/GHIDRA_DEBUGGING_FAQ.md) - Troubleshooting "no debugging symbols" and debugging tips
- [docs/guides/DEBUGGING_GUIDE.md](docs/guides/DEBUGGING_GUIDE.md) - Enhanced debugging mode
- [docs/guides/INTERACTIVE_DEBUGGER_GUIDE.md](docs/guides/INTERACTIVE_DEBUGGER_GUIDE.md) - Interactive debugger
- [docs/guides/GDB_SERVER_GUIDE.md](docs/guides/GDB_SERVER_GUIDE.md) - GDB server for Ghidra/IDA integration
- [docs/guides/VFS_DOCUMENTATION.md](docs/guides/VFS_DOCUMENTATION.md) - Virtual File System for game file isolation
- [docs/guides/OPENTELEMETRY_USAGE.md](docs/guides/OPENTELEMETRY_USAGE.md) - OpenTelemetry for logging, metrics, and profiling
- [docs/examples/TELEMETRY_EXAMPLE.md](docs/examples/TELEMETRY_EXAMPLE.md) - Practical examples of using OpenTelemetry
- [docs/guides/RIDER_OPENTELEMETRY_SETUP.md](docs/guides/RIDER_OPENTELEMETRY_SETUP.md) - JetBrains Rider integration guide
- [docs/implementation/JIT_CACHE_IMPLEMENTATION.md](docs/implementation/JIT_CACHE_IMPLEMENTATION.md) - JIT caching to disk for faster emulation
- [docs/examples/JIT_CACHE_EXAMPLES.md](docs/examples/JIT_CACHE_EXAMPLES.md) - JIT cache usage examples and best practices
- [docs/features/CHD_DISC_IMAGE_SUPPORT.md](docs/features/CHD_DISC_IMAGE_SUPPORT.md) - CHD disc image format support

### Win32Emu (Library)
The core emulation library that powers Win32Emu.Gui. This library provides the `Emulator` class and `EmulatorLauncher` API for embedding Win32 emulation into .NET applications.

## Backend System

Win32Emu uses pluggable backends for cross-platform multimedia support:

### Rendering Backends
- **SDL** (default): SDL3-CS - Native Metal on macOS, Vulkan on Linux, DirectX 12 on Windows. Best compatibility, hardware-accelerated
- **GLFW**: Silk.NET.GLFW + OpenGL - Alternative for systems where SDL has issues
- **Vulkan**: Silk.NET.Vulkan - Modern GPU API with cross-platform support (uses MoltenVK on macOS)
- **Metal**: SharpMetal - Native Metal backend for macOS (hardware-accelerated)
- **Software**: SDL3 software renderer - True CPU-only rendering with windowing and event support. No GPU acceleration required, ideal for macOS, debugging, or systems without GPU support

### Audio Backend
- **SDL Audio**: SDL3-CS audio when using SDL backend - Native audio support
- **OpenAL**: Silk.NET.OpenAL - Cross-platform audio support for GLFW/Vulkan backends

### Input Backend
- **SDL Input**: SDL3-CS input when using SDL backend - Keyboard, mouse, and joystick support
- **Silk.NET.Input**: Unified keyboard, mouse, and gamepad support for GLFW/Vulkan backends

**Configuration:**
- Command-line: `--backend SDL`, `--backend GLFW`, `--backend Vulkan`, `--backend Metal`, or `--backend Software`
- Environment variable: `WIN32EMU_BACKEND=SDL`, `WIN32EMU_BACKEND=GLFW`, `WIN32EMU_BACKEND=Vulkan`, `WIN32EMU_BACKEND=Metal`, or `WIN32EMU_BACKEND=Software`
- Programmatic: `BackendFactory.CurrentBackendType = BackendType.SDL;`

See [docs/implementation/SILK_NET_MIGRATION.md](docs/implementation/SILK_NET_MIGRATION.md) for detailed documentation.

## CPU Intrinsics Support

Win32Emu leverages hardware-accelerated SIMD instructions for better performance:

- **x86 hosts**: Uses SSE, SSE2, SSE3, SSE4, AVX, AVX2 instructions
- **ARM hosts**: Uses NEON (AdvSimd) instructions
- **Automatic detection**: CPUID reports accurate host CPU capabilities
- **Fallback support**: Software implementations when intrinsics aren't available

See [docs/implementation/INTRINSICS.md](docs/implementation/INTRINSICS.md) for detailed documentation.

## Event-Driven Messaging System

Win32Emu includes a DispatchR-inspired message handling system for type-safe, zero-allocation Win32 message dispatching:

- **Type-Safe Handlers**: Strongly-typed message classes with compile-time checking
- **Zero Allocation**: Lambda-based handlers avoid heap allocations
- **Extensible**: Easy to register custom message handlers
- **Testable**: Handlers can be tested independently from API implementations

**Example:**
```csharp
// Register a message handler
env.MessageDispatcher.RegisterHandler(WM.COMMAND, msg =>
{
    var cmdMsg = (CommandMessage)msg;
    Console.WriteLine($"Button {cmdMsg.ControlId} clicked!");
    return 0;
});

// Dispatch a message
var message = new CommandMessage(hwnd, wParam, lParam);
env.MessageDispatcher.Dispatch(message);
```

See [docs/implementation/MESSAGE_DISPATCHER_IMPLEMENTATION.md](docs/implementation/MESSAGE_DISPATCHER_IMPLEMENTATION.md) for detailed documentation and examples.

## Building

```bash
dotnet build Win32Emu.sln
```

## Running Tests

```bash
dotnet test Win32Emu.sln
```
