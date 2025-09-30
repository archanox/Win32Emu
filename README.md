# Win32Emu

A Windows 32-bit PE executable emulator for running classic Windows games and applications on modern systems.

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

## Building

```bash
dotnet build Win32Emu.sln
```

## Running Tests

```bash
dotnet test Win32Emu.sln
```
