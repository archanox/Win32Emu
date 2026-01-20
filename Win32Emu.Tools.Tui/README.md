# Win32Emu TUI (Terminal User Interface)

A terminal-based interface for Win32Emu, optimized for 80-column terminals and mobile SSH access.

## Features

### ✅ Backend Services Implemented

- **Game Library Service**: Complete CRUD operations with JSON persistence
  - Add, remove, and update games with full metadata
  - Play statistics tracking (count, last played timestamp)
  - Game metadata: title, developer, publisher, genre, year, description
  - Storage at `%APPDATA%\Win32Emu\game-library.json` (Windows) or `~/.config/Win32Emu/game-library.json` (Unix)
  
- **Configuration Service**: Emulator settings management
  - Backend selection (SDL, GLFW, Vulkan, Metal, Software)
  - Debug mode toggles
  - Interactive debugger enable/disable
  - GDB server configuration with port setting
  - File logging control
  - Builds argument arrays for EmulatorLauncher

- **EmulatorLauncher Integration**: Ready for game launching
  - Configuration injection prepared
  - Automatic stats tracking logic implemented
  - Launch argument building complete

- **80-Column Display**: Optimized for mobile SSH clients
  - Current static display fits within 80 columns
  - Clean, bordered layout with info bars
  - Cross-platform support (Windows, Linux, macOS)

### 🔨 In Development

- **Interactive Menu Navigation**: Arrow key navigation with visual selection
- **Game Browser UI**: Interactive list of games with launch capability
- **Add Game Form**: Input fields for adding new games
- **Settings Screen**: Toggle switches for configuration options
- **Keyboard Shortcuts**: Quick actions for power users
- **Real-time Updates**: Dynamic screen refreshing

### Current Status

**v0.2.0** - This version provides a **static display** showing the architecture and backend services. The service layer (GameLibraryService, ConfigurationService) is fully implemented and ready to be wired to interactive UI components. Interactive navigation is planned for the next update.

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

