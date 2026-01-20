# Win32Emu TUI Usage Guide

## Overview

Win32Emu TUI is a terminal-based interface for managing and running classic Windows games. It's designed for 80-column terminals, making it perfect for SSH access from mobile devices.

## Installation

### Building from Source

```bash
cd Win32Emu.Tools.Tui
dotnet build
```

### Running

```bash
dotnet run --project Win32Emu.Tools.Tui
```

Or after building:

```bash
./Win32Emu.Tools.Tui
```

## Features

### 1. Game Library Management

The game library allows you to:
- Browse your collection of Windows games
- View game details (developer, publisher, genre, release year)
- Track play statistics (play count, last played date)
- Launch games with one keypress
- Delete games from the library

**How to use:**
1. Select "Game Library" from main menu
2. Browse through your games using arrow keys
3. Press Enter to view details and launch

### 2. Adding Games

Add new games to your library with metadata:
- Title (required)
- Executable path (required)
- Developer, Publisher, Genre (optional)
- Release year (optional)

**How to add:**
1. Select "Add Game" from main menu
2. Enter game details when prompted
3. Game is automatically saved to library

### 3. Settings Configuration

Configure emulator options:
- **Default Backend**: Choose rendering backend (SDL, GLFW, Vulkan, Metal, Software)
- **Debug Mode**: Enable detailed logging
- **Interactive Debugger**: Launch games with debugger by default
- **GDB Server**: Enable GDB server for remote debugging
- **GDB Port**: Configure GDB server port (default: 1234)
- **File Logging**: Save logs to file

**How to configure:**
1. Select "Settings" from main menu
2. Choose setting to change
3. Follow prompts to modify values

### 4. Interactive Debugger

Launch games in debug mode to:
- Set breakpoints at specific addresses
- Step through instructions one at a time
- Inspect CPU registers and memory
- Examine the call stack
- Pause and resume execution

**How to use:**
1. Select "Interactive Debugger" from main menu
2. Enter path to executable
3. Use debugger commands (type 'help' in debugger for list)

**Available debugger commands:**
- `continue` (c) - Continue execution
- `step` (s, stepi) - Execute one instruction
- `break <address>` (b) - Set breakpoint
- `delete <id>` (d) - Delete breakpoint
- `registers` (r) - Display CPU registers
- `examine <address> [count]` (x) - Examine memory
- `info breakpoints` - List all breakpoints
- `help` (h, ?) - Show help

### 5. Help

Access built-in help documentation from the main menu.

## 80-Column Mode

The TUI is optimized for 80-column terminals:
- All text automatically truncated or wrapped
- Tables designed to fit within 80 characters
- Perfect for SSH access from mobile devices
- Clean, readable interface

## SSH Access from Mobile

### iOS (using Termius or similar SSH client)

1. Install Termius from App Store
2. Connect to your server
3. Run: `dotnet run --project Win32Emu.Tools.Tui`
4. Use touchscreen to navigate menus

### Android (using JuiceSSH or Termux)

1. Install JuiceSSH or Termux from Play Store
2. Connect to your server
3. Run: `dotnet run --project Win32Emu.Tools.Tui`
4. Use on-screen keyboard and gestures to navigate

### Tips for Mobile SSH

- Use landscape orientation for wider display
- Enable virtual keyboard shortcuts in your SSH client
- Adjust font size for better readability
- Use Tab key for field navigation when adding games

## Game Library Storage

The game library is stored in JSON format at:
- **Windows**: `%APPDATA%\Win32Emu\game-library.json`
- **Linux/macOS**: `~/.config/Win32Emu/game-library.json`

You can manually edit this file if needed, or backup/restore it across systems.

## Examples

### Example 1: Adding and Launching a Game

```
1. Start TUI: dotnet run --project Win32Emu.Tools.Tui
2. Select "Add Game"
3. Enter:
   - Title: Age of Empires
   - Path: /games/aoe/empires.exe
   - Developer: Ensemble Studios
   - Year: 1997
4. Game is added to library
5. Select "Game Library"
6. Select "Age of Empires"
7. Choose "Launch Game"
8. Game starts in emulator
```

### Example 2: Debugging a Game

```
1. Start TUI: dotnet run --project Win32Emu.Tools.Tui
2. Select "Interactive Debugger"
3. Enter path: /games/testgame.exe
4. Debugger starts and breaks at entry point
5. Type: break 0x401000
6. Type: continue
7. Breakpoint hits at 0x401000
8. Type: registers (to inspect CPU state)
9. Type: step (to execute one instruction)
10. Type: quit (to exit debugger)
```

### Example 3: Configuring for Remote Debugging

```
1. Start TUI: dotnet run --project Win32Emu.Tools.Tui
2. Select "Settings"
3. Select "Toggle GDB Server" (enables it)
4. Select "Change GDB Port" (if needed, default is 1234)
5. Select "Back"
6. Now games will run with GDB server enabled
7. Connect Ghidra/IDA to localhost:1234 for remote debugging
```

## Troubleshooting

### TUI doesn't display correctly

- Ensure your terminal supports ANSI escape sequences
- Try resizing terminal to at least 80 columns wide
- Use a modern terminal emulator (Windows Terminal, iTerm2, etc.)

### Game won't launch

- Verify executable path is correct
- Check file permissions
- Enable debug mode in Settings for more information
- Check game compatibility with PeAnalyzer tool

### Library file not found

- The library is created automatically on first game addition
- Ensure write permissions for %APPDATA% or ~/.config directories
- Check file exists at expected location

## Advanced Features

### Custom Backend Selection

Different games may work better with different backends:
- **SDL** (default): Best compatibility, hardware-accelerated
- **GLFW**: Alternative for systems where SDL has issues
- **Vulkan**: Modern GPU API, use MoltenVK on macOS
- **Metal**: Native macOS backend, hardware-accelerated
- **Software**: CPU-only rendering, no GPU required

### GDB Server Integration

Enable GDB server for advanced debugging:
1. Configure in Settings
2. Launch game (server starts automatically)
3. Connect from Ghidra/IDA/GDB
4. Full debugging capabilities with symbol resolution

See [GDB Server Guide](../../docs/guides/GDB_SERVER_GUIDE.md) for details.

## See Also

- [Win32Emu Main README](../../README.md)
- [Interactive Debugger Guide](../../docs/guides/INTERACTIVE_DEBUGGER_GUIDE.md)
- [Debugging Guide](../../docs/guides/DEBUGGING_GUIDE.md)
- [GDB Server Guide](../../docs/guides/GDB_SERVER_GUIDE.md)
- [Backend Configuration](../../docs/implementation/SILK_NET_MIGRATION.md)
