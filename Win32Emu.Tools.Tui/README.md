# Win32Emu TUI (Terminal User Interface)

A terminal-based interface for Win32Emu, optimized for 80-column terminals and mobile SSH access.

## Features

### ✅ Implemented

- **Game Library Management**: Browse, add, and manage games with metadata
  - JSON-based persistent storage
  - Play statistics tracking (count, last played timestamp)
  - Game metadata: title, developer, publisher, genre, year, description
  
- **Settings Configuration UI**: Complete emulator configuration
  - Backend selection (SDL, GLFW, Vulkan, Metal, Software)
  - Debug mode toggles
  - Interactive debugger enable/disable
  - GDB server configuration with port setting
  - File logging control

- **Game Launching**: Launch games directly from TUI
  - Integration with EmulatorLauncher API
  - Automatic stats tracking
  - Configuration injection (backend, debug modes, etc.)

- **80-Column Optimized**: Perfect for mobile SSH clients
  - All screens fit within 80 columns
  - Clean, bordered layout with info bars
  - Automatic text truncation

- **Cross-Platform**: Works on Windows, Linux, and macOS
  - Lightweight, terminal-based
  - No GUI dependencies

### 🔨 Coming Soon

- **Interactive Menu Navigation**: Arrow key navigation with visual selection
- **Input Forms**: Edit game metadata directly in TUI
- **Real-time Updates**: Dynamic screen refreshing
- **Keyboard Shortcuts**: Quick actions for power users

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

## Architecture

### Services
- **GameLibraryService**: Manages game library with JSON persistence
  - Location: `%APPDATA%\Win32Emu\game-library.json` (Windows)
  - Location: `~/.config/Win32Emu/game-library.json` (Linux/macOS)

- **ConfigurationService**: Manages emulator configuration
  - Backend selection
  - Debug modes
  - GDB server settings
  - Builds argument arrays for EmulatorLauncher

### Models
- **AppState**: Application state management
  - Current view tracking
  - Selection state
  - Form state for adding games

- **GameEntry**: Game metadata model
  - All metadata fields
  - Play statistics
  - Custom settings per game

### Integration
- **EmulatorLauncher API**: Direct integration for game launching
- **Hex1b Framework**: Modern terminal UI framework for .NET

## Requirements

- .NET 10.0 or later
- Terminal with Unicode support
- Interactive TTY (for navigation features)
- Minimum 80 columns width recommended

## Technology

Built with [Hex1b](https://hex1b.dev/) - a modern terminal UI framework for .NET.

## Current Version

**v0.2.0** - Interactive features implemented, navigation coming soon

