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

## Key Changes

### 1. New Files Created
- `Configuration/IEmulatorSettings.cs` - Interface for emulator settings
- `Configuration/IGameLibrary.cs` - Interface for game library
- `Models/GameSettings.cs` - Model for per-game settings overrides

### 2. Modified Files
- `Configuration/ConfigurationService.cs` - Updated to use split configuration files
- `CONFIGURATION.md` - Updated documentation

### 3. Preserved Files
- `Configuration/IAppConfiguration.cs` - Kept for legacy migration support

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

### Legacy Migration
The service automatically migrates from the old `config.json` to the new split files:
- If `config.json` exists and new files don't, migration occurs on startup
- All settings, games, and folders are preserved
- The old `config.json` remains for backward compatibility

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
✅ Legacy migration works correctly
✅ All existing functionality preserved

## Verification Steps
To verify the implementation works:
1. Run the GUI application
2. Add a game to the library
3. Close the application
4. Verify two separate files exist: `settings.json` and `library.json`
5. Modify settings - only `settings.json` should change
6. Add/remove games - only `library.json` should change
7. Test legacy migration by deleting new files and creating `config.json`

## API Changes

### New Methods
- `GetEmulatorConfiguration(string gameExecutablePath)` - Get settings for specific game
- `SaveGameSettings(string gameExecutablePath, GameSettings gameSettings)` - Save per-game overrides
- `GetGameSettings(string gameExecutablePath)` - Get per-game overrides
- `RemoveGameSettings(string gameExecutablePath)` - Remove per-game overrides

### New Properties
- `SettingsFilePath` - Path to settings.json
- `LibraryFilePath` - Path to library.json
- `ConfigFilePath` - (Obsolete) Legacy path to config.json

## Notes
- **Backward compatible**: Automatically migrates from old `config.json`
- **Zero breaking changes**: All existing code continues to work
- **Portable settings**: Users can sync `settings.json` across machines
- **Machine-specific data**: `library.json` stays local to each machine
- **Per-game customization**: Each game can have custom emulator settings
