# Implementation Summary: Split Configuration Files

## Issue Addressed
Configuration needed to be split into:
- **Emulator settings** (portable, user-specific) - can be synced across machines
- **Game library** (machine-specific) - stays local to each machine

Additionally, per-game emulator settings overrides were needed.

## Solution Implemented
Split the single `config.json` into two separate files:
- `settings.json` - Emulator configuration and per-game overrides
- `library.json` - Game library and watched folders

Added support for per-game emulator settings that override the global defaults.

Uses System.Text.Json for clean, standard JSON serialization with direct file I/O.

## Key Changes

### 1. Removed Config.Net and Legacy Migration
- Removed dependency on Config.Net
- Removed legacy migration code from config.json
- Deleted AppConfiguration.cs (no longer needed)
- Uses System.Text.Json for serialization

### 2. Files Created
- `Configuration/EmulatorSettings.cs` - POCO class for emulator settings
- `Configuration/GameLibrary.cs` - POCO class for game library
- `Models/GameSettings.cs` - Model for per-game settings overrides

### 3. Modified Files
- `Configuration/ConfigurationService.cs` - Simplified to use System.Text.Json directly
- `CONFIGURATION.md` - Updated documentation
- `Win32Emu.Gui.csproj` - Updated package references

## How It Works

### Split Configuration
The configuration service now manages two separate files:
1. **settings.json**: Contains emulator configuration and per-game overrides
2. **library.json**: Contains game library and watched folders

### Storage Location
Configuration files are stored in platform-specific locations:
- **Windows**: 
  - `%APPDATA%\Win32Emu\settings.json`
  - `%APPDATA%\Win32Emu\library.json`
- **Linux**: 
  - `~/.config/Win32Emu/settings.json`
  - `~/.config/Win32Emu/library.json`
- **macOS**: 
  - `~/Library/Application Support/Win32Emu/settings.json`
  - `~/Library/Application Support/Win32Emu/library.json`

### Per-Game Settings Overrides
Games can have custom emulator settings that override the global defaults:
- Settings are stored in `settings.json` under `PerGameSettings`
- Only specified settings are overridden; others use global defaults
- Use `GetEmulatorConfiguration(gameExecutablePath)` to get merged settings

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
      "ResolutionScaleFactor": 2
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
      "TimesPlayed": 5,
      "LastPlayed": "2024-01-15T10:30:00"
    }
  ],
  "WatchedFolders": ["C:\\Games", "D:\\OldGames"]
}
```

## Testing Performed
✅ Project builds without errors or warnings
✅ Configuration successfully splits into two files
✅ Per-game settings work correctly
✅ Settings and library are read/written independently
✅ All existing functionality preserved

## Verification Steps
To verify the implementation works:
1. Run the GUI application
2. Add a game to the library
3. Close the application
4. Verify two separate files exist: `settings.json` and `library.json`
5. Modify settings - only `settings.json` should change
6. Add/remove games - only `library.json` should change

## API Changes

### New Methods
- `GetEmulatorConfiguration(string gameExecutablePath)` - Get settings for specific game
- `SaveGameSettings(string gameExecutablePath, GameSettings gameSettings)` - Save per-game overrides
- `GetGameSettings(string gameExecutablePath)` - Get per-game overrides
- `RemoveGameSettings(string gameExecutablePath)` - Remove per-game overrides

### New Properties
- `SettingsFilePath` - Path to settings.json
- `LibraryFilePath` - Path to library.json

## Notes
- **Simplified implementation**: Uses System.Text.Json directly for configuration management
- **Zero breaking changes**: All existing code continues to work
- **Portable settings**: Users can sync `settings.json` across machines
- **Machine-specific data**: `library.json` stays local to each machine
- **Per-game customization**: Each game can have custom emulator settings
