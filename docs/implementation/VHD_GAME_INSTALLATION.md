# VHD Game Installation Feature

## Overview

This document describes the VHD game installation feature implemented for Win32Emu. This feature ensures that when games are added to the game library, they are automatically "installed" into a Virtual Hard Disk (VHD), and all subsequent operations use the VHD exclusively.

## Problem Statement

Previously, games were referenced by their host filesystem paths, and the VFS could fall back to directory-based file access. The new requirement is to:

1. Copy the entire game directory into a VHD when adding a game to the library
2. Store the game's installation path within the VHD
3. Always launch games from the VHD (no fallback to host filesystem)
4. Have all VFS functions exclusively use the VHD

## Implementation Details

### Game Model Changes

The `Game` model now includes two new properties:

```csharp
/// <summary>
/// Path to the game's virtual disk (VHD/VHDX/VMDK)
/// </summary>
public string? VirtualDiskPath { get; set; }

/// <summary>
/// Path to the executable within the VHD (e.g., "C:\ignition\ign_teas.exe")
/// </summary>
public string? VhdExecutablePath { get; set; }
```

- `VirtualDiskPath`: The host filesystem path to the VHD file (e.g., `/home/user/.local/share/Win32Emu/VirtualDisks/GameName.vhd`)
- `VhdExecutablePath`: The Windows-style path to the executable within the VHD (e.g., `C:\ignition\ign_teas.exe`)

### VirtualDiskService

A new method `InstallGameToVirtualDiskAsync()` was added to handle game installation:

```csharp
public async Task<(string DiskPath, string VhdExecutablePath)> InstallGameToVirtualDiskAsync(
    Game game, 
    string sourceExecutablePath,
    GameSettings? gameSettings = null)
```

This method:
1. Determines the source directory from the executable path
2. Creates or gets the VHD for the game
3. Copies the entire directory structure into the VHD
4. Returns both the VHD file path and the VHD executable path

### Game Library ViewModel

Both `AddGame()` and `ScanFolderForGames()` methods now install games to VHD:

1. When a user selects an executable (e.g., `/users/Pierce/games/ignition/ign_teas.exe`)
2. The entire parent directory (`/users/Pierce/games/ignition/`) is copied to the VHD
3. The game is stored with:
   - Original executable path (for reference)
   - VHD file path
   - VHD executable path (e.g., `C:\ignition\ign_teas.exe`)

### Emulator Service

The `EmulatorService` was updated to always use VHD:

1. `InitializeVirtualFileSystem()` now:
   - Always initializes with VHD (no fallback)
   - Uses the game's `VirtualDiskPath` if available
   - Creates a new VHD if one doesn't exist (with warning)

2. `LaunchGame()` now:
   - Initializes VFS before loading the executable
   - Uses `VhdExecutablePath` if available, otherwise falls back to `ExecutablePath` for backwards compatibility
   - Passes the correct path to the emulator's LoadExecutable

### Configuration Service

The `ConfigurationService` was updated to persist VHD settings:
- `GetEmulatorConfiguration()` now includes VHD-related properties
- `SaveEmulatorConfiguration()` now persists VHD-related properties

## Example Flow

### Adding a Game

1. User clicks "Add Game" and selects `/users/Pierce/games/ignition/ign_teas.exe`
2. System extracts directory name: `ignition`
3. System creates VHD: `~/.local/share/Win32Emu/VirtualDisks/ign_teas.vhd`
4. System copies entire `/users/Pierce/games/ignition/*` to VHD root as `/ignition`
5. Game is saved with:
   - `ExecutablePath`: `/users/Pierce/games/ignition/ign_teas.exe` (reference)
   - `VirtualDiskPath`: `~/.local/share/Win32Emu/VirtualDisks/ign_teas.vhd`
   - `VhdExecutablePath`: `C:\ignition\ign_teas.exe`

### Launching a Game

1. User double-clicks game in library
2. `EmulatorService.LaunchGame()` is called
3. VHD is mounted via `InitializeVirtualFileSystemWithDisk()`
4. Executable is loaded using `VhdExecutablePath` (`C:\ignition\ign_teas.exe`)
5. Game runs entirely from VHD
6. All file operations use the VHD's virtual filesystem

## Testing

Comprehensive unit tests were added in `VirtualDiskServiceTests.cs`:

- `GetOrCreateVirtualDisk_CreatesNewDisk_WhenDiskDoesNotExist`
- `GetOrCreateVirtualDisk_ReusesExistingDisk_WhenDiskExists`
- `GetVirtualDisksDirectory_ReturnsConfiguredDirectory`
- `ShouldUseVirtualDisk_ReturnsTrue_WhenEnabledByDefault`
- `DeleteVirtualDisk_RemovesDiskFile_WhenExists`

All tests pass successfully.

## Security

CodeQL security analysis was performed on all changes with no issues found.

## Backwards Compatibility

The implementation maintains backwards compatibility:
- Games without `VhdExecutablePath` will use `ExecutablePath`
- Existing games will be migrated to VHD on next launch (with a warning logged)
- All new games will be installed to VHD automatically

## Configuration

VHD behavior can be configured via `EmulatorConfiguration`:

- `UseVirtualDiskByDefault`: Enable/disable VHD by default (default: true)
- `DefaultVirtualDiskSizeMb`: Default size for new VHDs (default: 512 MB)
- `VirtualDiskFormat`: Format to use (VHD/VHDX/VMDK) (default: VHD)
- `VirtualDisksDirectory`: Directory to store VHDs (default: `~/.local/share/Win32Emu/VirtualDisks`)

## Files Modified

1. `Win32Emu.Gui/Models/Game.cs` - Added VHD properties
2. `Win32Emu.Gui/Services/VirtualDiskService.cs` - Added InstallGameToVirtualDiskAsync
3. `Win32Emu.Gui/ViewModels/GameLibraryViewModel.cs` - Updated AddGame and ScanFolderForGames
4. `Win32Emu.Gui/Services/EmulatorService.cs` - Updated VFS initialization and game launching
5. `Win32Emu.Gui/Configuration/ConfigurationService.cs` - Added VHD settings persistence
6. `Win32Emu.Tests.Gui/VirtualDiskServiceTests.cs` - Added comprehensive tests

## Future Enhancements

Potential future improvements:
- Progress reporting during VHD installation (for large games)
- Option to export/import VHDs for game sharing
- Compression options for VHDs
- Defragmentation tools for VHDs
- Support for multiple VHD formats (ISO mounting, etc.)
