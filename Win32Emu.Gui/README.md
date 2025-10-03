# Win32Emu GUI

A cross-platform desktop GUI for Win32Emu, built with Avalonia UI.

## Features

- **Game Library**: Manage your collection of Windows games with an intuitive card-based layout
- **Persistent Storage**: Your game library and settings automatically persist between application launches
- **Add Games**: Easily add games using a file picker to select Windows executables (.exe files)
- **Folder Scanning**: Add entire folders to automatically discover Windows executables
- **Launch Games**: Run games directly from the library with a single click
- **Play Tracking**: Automatically tracks how many times each game has been played
- **Emulator Settings**: Configure emulator options including:
  - Rendering Backend (Software, DirectDraw, Glide)
  - Resolution Scale Factor (1x - 4x)
  - Reserved Memory (64MB - 2048MB)
  - Windows Version (95, 98, ME, NT 4.0, 2000, XP)
  - Debug Mode toggle

## Configuration Storage

All settings, games, and watched folders are automatically saved to a platform-specific location:
- **Windows**: `%APPDATA%\Win32Emu\config.json`
- **Linux**: `~/.config/Win32Emu/config.json`
- **macOS**: `~/Library/Application Support/Win32Emu/config.json`

See [CONFIGURATION.md](CONFIGURATION.md) for more details about the configuration system.

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Win32Emu executable in your PATH or in the same directory as the GUI

### Running the Application

```bash
dotnet run --project Win32Emu.Gui
```

### Adding Games

1. Click the "Add Game" button in the library view
2. Select a Windows executable (.exe) file
3. The game will be added to your library with a default title (based on the filename)

### Launching Games

1. Click the "Launch" button on any game card
2. The emulator will start with your configured settings
3. Game play statistics (times played, last played) will be tracked automatically

## Architecture

The application follows the MVVM (Model-View-ViewModel) pattern:

- **Models**: Game, EmulatorConfiguration, GameLibrary
- **ViewModels**: MainWindowViewModel, GameLibraryViewModel, SettingsViewModel
- **Views**: MainWindow, GameLibraryView, SettingsView
- **Services**: EmulatorService (handles launching the emulator)
- **Configuration**: ConfigurationService (manages persistent storage using Config.Net)

## Technology Stack

- **Avalonia UI**: Cross-platform XAML-based UI framework
- **CommunityToolkit.Mvvm**: MVVM helpers and source generators
- **Config.Net**: Configuration management with INI file persistence

## Future Enhancements

- Game metadata fetching from online databases
- Custom thumbnails for games
- Save state management
- Controller configuration
- Game-specific settings overrides
