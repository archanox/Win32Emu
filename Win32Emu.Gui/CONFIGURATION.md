# Configuration Persistence Implementation

## Overview

The Win32Emu GUI uses Config.Net for persistent storage of application settings and game library. Configuration is split into two separate files:

- **Settings file (`settings.json`)**: Portable emulator settings that can be carried across machines/platforms
- **Library file (`library.json`)**: Machine-specific game library and watched folders

This separation allows users to sync their emulator preferences across devices while keeping game libraries local to each machine.

## Storage Location

Configuration files are stored in a platform-agnostic location:
- **Windows**: 
  - Settings: `%APPDATA%\Win32Emu\settings.json`
  - Library: `%APPDATA%\Win32Emu\library.json`
- **Linux**: 
  - Settings: `~/.config/Win32Emu/settings.json`
  - Library: `~/.config/Win32Emu/library.json`
- **macOS**: 
  - Settings: `~/Library/Application Support/Win32Emu/settings.json`
  - Library: `~/Library/Application Support/Win32Emu/library.json`

## What Gets Persisted

### Emulator Settings (settings.json)
- Rendering Backend (Software, DirectDraw, Glide)
- Resolution Scale Factor (1-4)
- Reserved Memory (MB)
- Windows Version
- Debug Mode enabled/disabled
- Per-game settings overrides

### Game Library (library.json)
- Game title
- Executable path
- Description
- Times played counter
- Last played timestamp
- Watched folders list

## Per-Game Settings Overrides

The emulator settings can be overridden on a per-game basis. These overrides are stored in the settings file and allow you to customize emulator behavior for specific games.

Example use cases:
- Use DirectDraw for one game but Software rendering for others
- Allocate more memory for specific games
- Enable debug mode only for troublesome games

## Implementation Details

### ConfigurationService
The `ConfigurationService` class manages all configuration persistence:
- Loads configuration on application startup from split files
- Automatically saves changes when settings are modified
- Uses Config.Net with JSON file storage
- Supports migration from legacy `config.json` to split files
- Provides per-game settings override functionality

### Split Configuration Files
The configuration is split into two files:
1. **settings.json**: Contains emulator configuration and per-game overrides (portable)
2. **library.json**: Contains game library and watched folders (machine-specific)

### Auto-Save Behavior
- Game library (library.json) is automatically saved when:
  - Adding a game
  - Removing a game
  - Launching a game (updates play count)
  - Adding a watched folder
  
- Settings (settings.json) are automatically saved when:
  - Any setting is changed in the Settings view
  - Per-game settings are added or modified

### Legacy Migration
If a legacy `config.json` file exists and the new split files don't exist, the service will automatically migrate the configuration to the new format on startup.

### Play Counter
The `TimesPlayed` counter is automatically incremented each time a game is launched, before the emulator starts.

## Example Configuration Files

### settings.json
```json
{
  "RenderingBackend": "Software",
  "ResolutionScaleFactor": 1,
  "ReservedMemoryMB": 256,
  "WindowsVersion": "Windows 95",
  "EnableDebugMode": false,
  "PerGameSettings": {
    "C:\\Games\\game1.exe": {
      "RenderingBackend": "DirectDraw",
      "ResolutionScaleFactor": 2,
      "ReservedMemoryMb": 512
    },
    "C:\\Games\\game2.exe": {
      "EnableDebugMode": true
    }
  }
}
```

### library.json
```json
{
  "Games": [
    {
      "Title": "Game1",
      "ExecutablePath": "C:\\Games\\game1.exe",
      "Description": "Classic game",
      "TimesPlayed": 5,
      "LastPlayed": "2024-01-15T10:30:00"
    }
  ],
  "WatchedFolders": [
    "C:\\Games",
    "D:\\OldGames"
  ]
}
```

## Testing

To verify the implementation:
1. Run the application
2. Add a game to the library
3. Close the application
4. Reopen the application - the game should still be in the library
5. Launch the game - the play counter should increment
6. Change a setting - it should persist after restart
7. Verify that two separate files are created: `settings.json` and `library.json`

### Legacy Migration Testing
1. If you have an existing `config.json` file, delete `settings.json` and `library.json`
2. Start the application
3. The legacy configuration should be automatically migrated to the new split files

The configuration files can be manually inspected at the locations mentioned above.
