# Win32Emu TUI (Terminal User Interface)

A terminal-based interface for Win32Emu, optimized for 80-column terminals and mobile SSH access.

## Features

### ✅ Implemented Features (v0.4.0)

- **Interactive Menu Navigation**: Arrow key navigation with visual selection using Hex1b List widgets
  - Main menu with 5 options: Game Library, Add Game, Settings, Help, Exit
  - Full keyboard navigation (↑/↓ arrows, Enter to select)
  - Visual feedback for selected items
  
- **Add Game Form**: Interactive form for adding new games
  - Display fields: Title, Executable Path, Developer, Publisher, Genre, Release Year, Description
  - List-based navigation through fields
  - Save and Cancel options
  - Automatic ID and date generation on save
  - Form validation (requires title and executable path)
  - Direct access from Main Menu or Game Library
  
- **Game Browser UI**: Interactive list of games with launch capability
  - Display all games with title and release year
  - "[Add New Game]" option at the bottom of the list
  - Navigate with arrow keys
  - Press Enter to view game details
  - Empty state with "Add New Game" prompt when no games exist
  
- **Game Details View**: Comprehensive game information display
  - Full metadata: title, developer, publisher, genre, year
  - Play statistics: times played, last played date
  - File path and added date
  - Game description
  - Navigation: ESC to go back
  
- **Settings Screen**: Interactive configuration management
  - Toggle switches for all boolean settings (press Enter to toggle)
  - Backend selection (cycles through SDL, GLFW, Vulkan, Metal, Software)
  - Debug Mode (ON/OFF toggle)
  - Interactive Debugger (ON/OFF toggle)
  - GDB Server (ON/OFF toggle)
  - GDB Server Port (cycles through common ports: 1234, 2345, 3456, 4567, 5678)
  - File Logging (ON/OFF toggle)
  - Real-time value updates on toggle

- **Help Screen**: Keyboard shortcuts reference
  - Complete list of navigation controls
  - Context-specific help for each screen
  - Quick reference for all features
  - Updated with Add Game and Settings toggle instructions

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
  - Live settings value updates

### 🚧 Future Enhancements

- **Text Input Fields**: Actual text entry for Add Game form (currently display-only)
- **Game Launch**: Direct game launching from TUI (requires EmulatorLauncher integration)
- **Game Removal**: Delete games from library
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
- **Enter**: Select/Activate/Toggle item
- **ESC**: Go back to previous screen
- **Ctrl+C**: Exit application

### Main Menu
- Use arrow keys to navigate between options
- Press Enter to select:
  - Game Library
  - Add Game
  - Settings  
  - Help
  - Exit

### Game Library
- **↑/↓**: Browse games and options
- **Enter**: View game details or select [Add New Game]
- **ESC**: Return to main menu

### Add Game
- **↑/↓**: Navigate form fields
- **Enter**: Edit field (display-only currently), or Save/Cancel
- **ESC**: Cancel and return to main menu
- **Note**: Text input functionality coming soon. Currently displays field structure.

### Game Details
- **ESC**: Return to game library

### Settings
- **↑/↓**: Navigate settings
- **Enter**: Toggle ON/OFF or cycle through values
  - Backend: Cycles through SDL → GLFW → Vulkan → Metal → Software
  - Debug Mode: Toggle ON/OFF
  - Interactive Debugger: Toggle ON/OFF
  - GDB Server: Toggle ON/OFF
  - GDB Server Port: Cycles through 1234 → 2345 → 3456 → 4567 → 5678
  - File Logging: Toggle ON/OFF
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
- Multi-view navigation (Main Menu, Game Library, Add Game, Details, Settings, Help)
- Game browser with selectable list
- "[Add New Game]" option in game library
- Game details display
- Add Game form with Save/Cancel functionality
- Settings screen with interactive toggles
- Toggle switches for all boolean settings
- Backend cycling (SDL/GLFW/Vulkan/Metal/Software)
- Port cycling for GDB server
- Help screen with keyboard shortcuts
- State-driven UI updates
- 80-column terminal optimization

### In Progress 🚧
- Text input fields for Add Game form (currently display-only)
- Game removal functionality

### Planned 📋
- Game launch functionality
- Search and filter functionality
- Advanced configuration options
- Theme customization
- Multi-language support

## Current Version

**v0.4.0** - Add Game form and interactive Settings toggles complete

