# Configuration Persistence Implementation

## Overview

The Win32Emu GUI uses Microsoft.Extensions.Configuration with System.Text.Json for persistent storage of application settings and game library. Configuration is split into two separate files:

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
- Loads configuration on application startup from split files using Microsoft.Extensions.Configuration
- Uses configuration binding with `Get<T>()` for all properties (leveraging source generation where available)
- Uses SHA256 hashing of executable paths as keys for per-game settings to avoid path delimiter conflicts
- Automatically saves changes when settings are modified using System.Text.Json
- Provides per-game settings override functionality with transparent hash-based key management

### SHA256 Hashing for Per-Game Settings
To avoid conflicts with Microsoft.Extensions.Configuration's `:` path delimiter in Windows file paths:
- Executable paths are hashed using SHA256 before being used as keys
- The `GamePathMapping` dictionary maintains a reference from hash to original path
- This allows pure Microsoft.Extensions.Configuration binding without hybrid approaches

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
    "1a67ffbc5ebaf4417fb6b2c135a8c64e77904a4fc5d24291f434c34e3f6b91c2": {
      "RenderingBackend": "DirectDraw",
      "ResolutionScaleFactor": 2,
      "ReservedMemoryMb": 512
    },
    "8f3e9a2b7c1d4e5f6a9b8c7d6e5f4a3b2c1d9e8f7a6b5c4d3e2f1a9b8c7d6e5f": {
      "EnableDebugMode": true
    }
  },
  "GamePathMapping": {
    "1a67ffbc5ebaf4417fb6b2c135a8c64e77904a4fc5d24291f434c34e3f6b91c2": "C:\\Games\\game1.exe",
    "8f3e9a2b7c1d4e5f6a9b8c7d6e5f4a3b2c1d9e8f7a6b5c4d3e2f1a9b8c7d6e5f": "C:\\Games\\game2.exe"
  }
}
```

Note: The keys in `PerGameSettings` are SHA256 hashes of the executable paths. The `GamePathMapping` provides a reference to see which hash corresponds to which game.

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

The configuration files can be manually inspected at the locations mentioned above.
