# Configuration Persistence Implementation

## Overview

The Win32Emu GUI now uses Config.Net for persistent storage of application settings, game library, and watched folders. This ensures that all data persists between application launches.

## Storage Location

Configuration is stored in a platform-agnostic location:
- **Windows**: `%APPDATA%\Win32Emu\config.json`
- **Linux**: `~/.config/Win32Emu/config.json`
- **macOS**: `~/Library/Application Support/Win32Emu/config.json`

## What Gets Persisted

### Emulator Configuration
- Rendering Backend (Software, DirectDraw, Glide)
- Resolution Scale Factor (1-4)
- Reserved Memory (MB)
- Windows Version
- Debug Mode enabled/disabled

### Game Library
- Game title
- Executable path
- Description
- Times played counter
- Last played timestamp

### Watched Folders
- List of folders being monitored for games

## Implementation Details

### ConfigurationService
The `ConfigurationService` class manages all configuration persistence:
- Loads configuration on application startup
- Automatically saves changes when settings are modified
- Uses Config.Net with INI file provider for simple settings
- Uses JSON serialization for complex data (game list)

### Auto-Save Behavior
- Game library is automatically saved when:
  - Adding a game
  - Removing a game
  - Launching a game (updates play count)
  - Adding a watched folder
  
- Settings are automatically saved when:
  - Any setting is changed in the Settings view

### Play Counter
The `TimesPlayed` counter is automatically incremented each time a game is launched, before the emulator starts.

## Example Configuration File

```json
{
  "RenderingBackend": "Software",
  "ResolutionScaleFactor": 1,
  "ReservedMemoryMB": 256,
  "WindowsVersion": "Windows 95",
  "EnableDebugMode": false,
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

The configuration file can be manually inspected at the location mentioned above.
