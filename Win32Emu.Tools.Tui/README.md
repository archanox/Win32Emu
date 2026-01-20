# Win32Emu.Tools.Tui

Terminal User Interface (TUI) for Win32Emu, built with [Hex1b](https://hex1b.dev/).

## Features

- **80-Column Mode**: Optimized for SSH access from mobile devices
- **Game Library Management**: Browse, add, and organize your Windows game collection
- **Interactive Debugger Integration**: Launch games with built-in step-through debugging
- **Keyboard-Driven Navigation**: Efficient terminal-based interface
- **Configuration Management**: Configure emulator settings (backends, debug modes, etc.)
- **Play Statistics**: Track play count and last played date for each game

## Installation

```bash
cd Win32Emu.Tools.Tui
dotnet build
```

## Usage

### Starting the TUI

```bash
dotnet run --project Win32Emu.Tools.Tui
```

Or after building:

```bash
./Win32Emu.Tools.Tui
```

### Navigation

- **↑/↓ Arrows**: Navigate menus and lists
- **Enter**: Select item or confirm action
- **ESC**: Go back to previous screen
- **Q**: Quit application (from main menu)
- **Tab**: Next field (when adding games)

### Main Menu Options

1. **Game Library**: Browse and launch games from your collection
2. **Add Game**: Add a new game with metadata (title, developer, year, etc.)
3. **Settings**: Configure emulator options
4. **Interactive Debugger**: Launch a game with step-through debugging
5. **Help**: View detailed help information
6. **Exit**: Close the application

## Game Library

The game library is stored in JSON format at:
- **Windows**: `%APPDATA%\Win32Emu\game-library.json`
- **Linux/macOS**: `~/.config/Win32Emu/game-library.json`

Each game entry includes:
- Title (required)
- Executable path (required)
- Developer, Publisher, Genre (optional)
- Release year (optional)
- Play statistics (play count, last played)
- Custom emulator settings per game

## Settings

Configure default emulator options:

- **Default Backend**: Choose rendering backend (SDL, GLFW, Vulkan, Metal, Software)
- **Debug Mode**: Enable detailed logging
- **Interactive Debugger**: Launch games with debugger by default
- **GDB Server**: Enable GDB server for remote debugging
- **File Logging**: Save logs to file

## Interactive Debugger

Launch games in debug mode to:
- Set breakpoints at specific addresses
- Step through instructions
- Inspect CPU registers and memory
- View call stack
- Pause and resume execution

See the [Interactive Debugger Guide](../../docs/guides/INTERACTIVE_DEBUGGER_GUIDE.md) for detailed commands.

## 80-Column Mode

The TUI is designed to fit within 80 columns, making it perfect for:
- SSH access from mobile devices
- Small terminal windows
- Traditional terminal constraints
- Screen readers and accessibility tools

All text is automatically truncated or wrapped to fit the 80-column width.

## Requirements

- .NET 10.0 or later
- Terminal with ANSI escape sequence support
- For best experience: Color terminal with 80+ columns and 24+ rows

## Examples

### Adding a Game

1. Select "Add Game" from main menu
2. Enter game title (required)
3. Enter executable path (required)
4. Optionally enter developer, publisher, genre, and release year
5. Press Enter to save

### Launching a Game

1. Select "Game Library" from main menu
2. Use ↑/↓ to select a game
3. Press Enter to view details
4. Press Enter again to launch

### Using the Debugger

1. Select "Interactive Debugger" from main menu
2. Enter the executable path
3. Press Enter to launch
4. Use debugger commands (type 'help' for list)

## Architecture

The TUI is built using:
- **Hex1b**: Modern declarative TUI framework for .NET
- **EmulatorLauncher**: Public API for running the emulator
- **MVVM Pattern**: Separation of UI and business logic
- **Service Layer**: Game library and configuration management

## See Also

- [Hex1b Documentation](https://hex1b.dev/guide/)
- [Win32Emu Main README](../../README.md)
- [Interactive Debugger Guide](../../docs/guides/INTERACTIVE_DEBUGGER_GUIDE.md)
- [Debugging Guide](../../docs/guides/DEBUGGING_GUIDE.md)
