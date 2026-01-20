# Win32Emu TUI (Terminal User Interface)

A terminal-based interface for Win32Emu, optimized for 80-column terminals and mobile SSH access.

## Features

### ✅ Implemented Features (v0.3.0)

- **Interactive Menu Navigation**: Arrow key navigation with visual selection using Hex1b List widgets
  - Main menu with 4 options: Game Library, Settings, Help, Exit
  - Full keyboard navigation (↑/↓ arrows, Enter to select)
  - Visual feedback for selected items
  
- **Game Browser UI**: Interactive list of games with launch capability
  - Display all games with title and release year
  - Navigate with arrow keys
  - Press Enter to view game details
  - Empty state message when no games exist
  
- **Game Details View**: Comprehensive game information display
  - Full metadata: title, developer, publisher, genre, year
  - Play statistics: times played, last played date
  - File path and added date
  - Game description
  - Navigation: ESC to go back
  
- **Settings Screen**: Configuration management interface
  - Backend selection list (SDL, GLFW, Vulkan, Metal, Software)
  - Display current configuration values
  - Debug mode status
  - Interactive debugger status  
  - GDB server status with port
  - File logging status
  - Navigation: Select backend from list, ESC to go back

- **Help Screen**: Keyboard shortcuts reference
  - Complete list of navigation controls
  - Context-specific help for each screen
  - Quick reference for all features

- **Backend Services**: Complete CRUD operations with JSON persistence
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

- **80-Column Display**: Optimized for mobile SSH clients
  - All views fit within 80 columns
  - Clean, bordered layout with info bars
  - Cross-platform support (Windows, Linux, macOS)

- **Real-time Updates**: Dynamic screen refreshing based on state changes
  - Immediate view switching on selection
  - State-driven UI updates
  - Responsive navigation

### 🚧 Future Enhancements

- **Add Game Form**: Multi-field input form for adding new games
- **Toggle Switches**: Interactive settings controls (planned for v0.4.0)
- **Game Launch**: Direct game launching from TUI (requires EmulatorLauncher integration)
- **Search/Filter**: Find games quickly in large libraries
- **Themes**: Customizable color schemes

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

## Keyboard Shortcuts

### Navigation
- **↑/↓**: Navigate lists
- **Enter**: Select/Activate item
- **ESC**: Go back to previous screen
- **Ctrl+C**: Exit application

### Main Menu
- Use arrow keys to navigate between options
- Press Enter to select:
  - Game Library
  - Settings  
  - Help
  - Exit

### Game Library
- **↑/↓**: Browse games
- **Enter**: View game details
- **ESC**: Return to main menu

### Game Details
- **ESC**: Return to game library

### Settings
- **↑/↓**: Navigate backend options
- **Enter**: Select backend
- **ESC**: Return to main menu

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
  - Current view tracking (MainMenu, GameLibrary, GameDetails, Settings, Help)
  - Selection state
  - Form state for adding games

- **GameEntry**: Game metadata model
  - All metadata fields
  - Play statistics
  - Custom settings per game

### Views
- **Main Menu**: Primary navigation hub
- **Game Library**: Browse and select games
- **Game Details**: View comprehensive game information
- **Settings**: Configure emulator options
- **Help**: Keyboard shortcuts reference

### Integration
- **EmulatorLauncher API**: Direct integration for game launching
- **Hex1b Framework**: Modern terminal UI framework for .NET

## Requirements

- .NET 10.0 or later
- Terminal with Unicode support
- Interactive TTY for navigation features
- Minimum 80 columns width recommended

## Technology

Built with [Hex1b](https://hex1b.dev/) - a modern terminal UI framework for .NET.

## Development Roadmap

### Completed ✅
- Backend service architecture
- JSON persistence for game library
- Configuration management system
- EmulatorLauncher argument building
- Interactive menu navigation with List widgets
- Multi-view navigation (Main Menu, Game Library, Details, Settings, Help)
- Game browser with selectable list
- Game details display
- Settings screen with backend selection
- Help screen with keyboard shortcuts
- State-driven UI updates
- 80-column terminal optimization

### In Progress 🚧
- Add Game form with TextBox inputs
- Toggle switches for boolean settings
- Game launch functionality

### Planned 📋
- Search and filter functionality
- Advanced configuration options
- Theme customization
- Multi-language support

## Current Version

**v0.3.0** - Interactive UI complete with full navigation, game browsing, and settings management

