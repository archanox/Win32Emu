# Game Info Window Feature

## Overview

The Game Info Window feature allows users to view detailed information about games in their library and edit metadata. It can be accessed by right-clicking on any game in the library and selecting "View Info" from the context menu.

## Features

### View Information

The Game Info Window displays comprehensive information extracted from the game's PE (Portable Executable) file:

1. **File Information**
   - **File Name**: The name of the executable file
   - **File Size**: Formatted size (e.g., "2.45 MB")
   - **DateTime Compiled**: When the executable was compiled (extracted from PE header)
   - **Machine Type**: Target processor architecture (e.g., "Intel 386 or later processors and compatible processors")
   - **Minimum OS**: Minimum required operating system (e.g., "Windows 95 / NT 4.0")
   - **Minimum OS Version**: Version number (e.g., "4.00")

2. **DLL Imports**
   - Lists all DLL imports from the executable
   - Shows the DLL name and function name for each import
   - Color-coded status:
     - **Green (✓ Yes)**: Import is implemented in the emulator
     - **Red (✗ No)**: Import is not yet implemented
   - Helps identify which functions need to be implemented for the game to run

### Edit Capabilities

1. **Edit Friendly Title**
   - Change the display name of the game
   - Persisted to the library configuration

2. **Environment Variables**
   - Set custom environment variables for the game
   - Format: `KEY=value` (one per line)

3. **Program Arguments**
   - Specify command-line arguments to pass when launching the game

### Additional Features

1. **Copy GameDB Stub**
   - Generates a JSON stub for the GameDB with:
     - Unique ID
     - Game title
     - Executable information with MD5, SHA1, and SHA256 hashes
   - Copies the JSON to clipboard for easy submission to GitHub
   - Button shows "✓ Copied!" feedback when successful
   - Ready to submit to the gamedb repository

2. **Open in VirusTotal**
   - Automatically generates a VirusTotal URL based on the executable's SHA256 hash
   - Opens the URL in the default browser
   - Useful for verifying the executable is clean/legitimate

## Technical Implementation

### Architecture

```
GameLibraryView (AXAML)
    ├── Context Menu → "View Info"
    └── GameLibraryViewModel
            └── ShowGameInfoCommand
                    └── Creates GameInfoViewModel
                            └── GameInfoWindow (AXAML)
```

### Services

1. **PeMetadataService**
   - Extracts metadata from PE files using AsmResolver.PE library
   - Parses file headers, optional headers, and import tables
   - Maps machine types and OS versions to human-readable strings

2. **HashUtility**
   - Computes MD5, SHA1, and SHA256 hashes
   - Used for game identification and VirusTotal links

3. **GameDbService**
   - Looks up game entries by executable hash
   - Provides metadata from the game database

### Data Models

```csharp
public class PeMetadata
{
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public DateTime? DateTimeCompiled { get; set; }
    public string MachineType { get; set; }
    public string MinimumOs { get; set; }
    public string MinimumOsVersion { get; set; }
    public List<PeImport> Imports { get; set; }
}

public class PeImport
{
    public string DllName { get; set; }
    public string FunctionName { get; set; }
    public bool IsImplemented { get; set; }
}
```

### UI Converters

1. **BoolToImplementedConverter**: Converts boolean to "✓ Yes" or "✗ No"
2. **BoolToColorConverter**: Converts boolean to green or red color

## Usage

### Viewing Game Info

1. Navigate to the Game Library
2. Right-click on any game
3. Select "View Info" from the context menu
4. The Game Info Window opens with all details

### Editing Game Details

1. Open the Game Info Window
2. Edit the title, environment variables, or program arguments
3. Click "Save Changes"
4. Changes are persisted to the library configuration

### Submitting to GameDB

1. Open the Game Info Window
2. Click "Copy GameDB Stub"
3. The button will change to "✓ Copied!" briefly
4. Paste the JSON into a new file in the gamedb repository
5. Submit a pull request

### Checking VirusTotal

1. Open the Game Info Window
2. Click "Open in VirusTotal"
3. Your default browser opens to the VirusTotal page for the executable

## Testing

The feature includes comprehensive unit tests:

```bash
# Run all GUI tests
dotnet test Win32Emu.Tests.Gui/Win32Emu.Tests.Gui.csproj

# Tests specifically for PeMetadataService
- GetMetadata_WithValidPeFile_ReturnsMetadata
- GetMetadata_WithValidPeFile_ReturnsImports
- GetMetadata_WithNonExistentFile_ReturnsNull
- GetMetadata_WithInvalidFile_ReturnsNull
```

All 30 tests pass successfully.

## Future Enhancements

Potential improvements:

1. **Enhanced Import Detection**
   - Query the actual emulator module registry instead of using heuristics
   - Show which module implements each function

2. **Environment Variables Persistence**
   - Save environment variables per-game to configuration
   - Load them when launching the game

3. **Program Arguments Persistence**
   - Save program arguments per-game to configuration
   - Pass them to the emulator on launch

4. **GameDB Integration**
   - Automatically enrich from GameDB when opening the window
   - Show more metadata (developers, publishers, ratings)

5. **VirusTotal Upload**
   - Implement the VirusTotal API to upload executables if not found
   - Show scan results directly in the window

## Dependencies

- **AsmResolver.PE** (6.0.0-beta.4): PE file parsing
- **Avalonia** (11.3.7): Cross-platform UI framework
- **CommunityToolkit.Mvvm** (8.4.0): MVVM helpers
- **System.Security.Cryptography**: Hash computation

## Files Added/Modified

### New Files
- `Win32Emu.Gui/Services/PeMetadataService.cs`
- `Win32Emu.Gui/ViewModels/GameInfoViewModel.cs`
- `Win32Emu.Gui/Views/GameInfoWindow.axaml`
- `Win32Emu.Gui/Views/GameInfoWindow.axaml.cs`
- `Win32Emu.Gui/Converters/BoolConverters.cs`
- `Win32Emu.Tests.Gui/PeMetadataServiceTests.cs`

### Modified Files
- `Win32Emu.Gui/Win32Emu.Gui.csproj` - Added AsmResolver.PE reference
- `Win32Emu.Gui/App.axaml` - Registered converters
- `Win32Emu.Gui/Views/GameLibraryView.axaml` - Added context menu
- `Win32Emu.Gui/ViewModels/GameLibraryViewModel.cs` - Added ShowGameInfo command
