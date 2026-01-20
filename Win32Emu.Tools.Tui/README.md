# Win32Emu TUI (Terminal User Interface)

A terminal-based interface for Win32Emu, optimized for 80-column terminals and mobile SSH access.

## Features

- **Game Library Management**: Browse, add, and manage games with metadata
- **80-Column Optimized**: Perfect for mobile SSH clients
- **Interactive Debugger**: Launch games in debug mode
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Lightweight**: Terminal-based, no GUI dependencies

## Installation

```bash
dotnet build Win32Emu.Tools.Tui
```

## Usage

```bash
dotnet run --project Win32Emu.Tools.Tui
```

Or after building:

```bash
./Win32Emu.Tools.Tui
```

## Requirements

- .NET 10.0 or later
- Terminal with Unicode support
- Minimum 80 columns width recommended

## Game Library

Games are stored in JSON format at:
- **Windows**: `%APPDATA%\Win32Emu\game-library.json`
- **Linux/macOS**: `~/.config/Win32Emu/game-library.json`

## Features (Coming Soon)

- Interactive menu navigation
- Game launching from TUI
- Configuration management
- Debug mode integration

## Technology

Built with [Hex1b](https://hex1b.dev/) - a modern terminal UI framework for .NET.
