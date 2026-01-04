# Virtual File System Browser

## Overview

The Virtual File System (VFS) Browser is a component in the Win32Emu WASM frontend that allows users to browse, view, and download files from the emulator's virtual disk. This is particularly useful for viewing log files, save games, and other data files created by emulated applications like `ign_teas`.

## Features

### File Listing
- Displays all files in the VFS with their paths and sizes
- Automatically refreshes during emulation to show newly created files
- Supports sorting by path name
- Shows total file count and total size

### File Viewing
- **Text Files**: Automatically detects and displays text files with proper encoding (UTF-8, ASCII, Latin-1)
- **Binary Files**: Shows hex preview of first 256 bytes with ASCII representation
- Modal viewer with proper formatting

### File Download
- Download any file from the VFS to the local file system
- Uses browser download API for seamless file saving
- Preserves original file names

## Usage

### During Emulation

1. **Load and Start an Executable**: 
   - Load a Windows executable (e.g., `ign_teas.exe`) in the WASM frontend
   - Start the emulation

2. **View VFS Files**:
   - The VFS Browser appears at the bottom of the page
   - Files are automatically refreshed every 100ms during emulation
   - Manual refresh is available via the "Refresh" button

3. **View a File**:
   - Click the "View" button next to any file
   - Text files display with syntax highlighting
   - Binary files show a hex dump preview

4. **Download a File**:
   - Click the "Download" button next to any file
   - File is saved to your browser's download location

## Technical Details

### Architecture

```
Home.razor (Main Page)
    ↓ includes
VirtualFileSystemBrowser.razor (Component)
    ↓ calls
EmulatorService.GetVfsFiles()
    ↓ accesses
BrowserVirtualFileSystem.Files
```

### Components

#### VirtualFileSystemBrowser.razor
- Razor component for UI rendering
- Handles file viewing modal
- Implements text/binary detection
- Generates hex previews for binary files

#### EmulatorService
- `GetVfsFiles()`: Returns read-only dictionary of VFS files
- Exposes `BrowserVirtualFileSystem.Files` property

#### JavaScript Interop
- `downloadFile(fileName, base64Data)`: Triggers browser download
- Converts base64 to Blob and creates temporary download link

### Auto-Refresh

The VFS browser automatically refreshes every 100ms during emulation:

```csharp
// In Home.razor, UI update loop
while (_isRunning)
{
    // ... other updates ...
    _vfsFiles = EmulatorService.GetVfsFiles();
    await InvokeAsync(StateHasChanged);
    await Task.Delay(100); // Update UI at 10 FPS
}
```

### File Detection

Text file detection uses a heuristic approach:
- Samples first 512 bytes of file
- Counts null bytes and control characters
- Considers file text if < 5% null/control characters

### Encoding Support

Text files are decoded in this order:
1. UTF-8
2. ASCII
3. Latin-1 (fallback)

If all fail, file is treated as binary.

## Implementation Notes

### Performance
- VFS files are stored in-memory in `BrowserVirtualFileSystem`
- File access is O(1) via dictionary lookup
- Refresh rate is limited to 10 Hz to avoid UI thrashing

### Memory Management
- Files are stored as byte arrays in memory
- Modal viewer creates temporary copies for display
- Base64 encoding used for JavaScript interop

### Browser Compatibility
- Uses modern browser APIs (Clipboard, Blob, URL.createObjectURL)
- Fallback mechanisms for older browsers in clipboard operations
- Works in Chrome, Firefox, Safari, Edge

## Example: ign_teas Game

The `ign_teas` game is known to write log files during execution. To view these logs:

1. Click "IGN_TEAS Game" button to load the sample
2. Click "Start" to begin emulation
3. Wait for the game to run (it may create log files)
4. Scroll down to the "Virtual File System Browser" section
5. Look for `.txt`, `.log`, or `.ini` files
6. Click "View" to see file contents
7. Click "Download" to save locally for analysis

## Troubleshooting

### No Files Showing
- Ensure the emulator is running and has loaded an executable
- Some applications don't create files immediately
- Click "Refresh" to manually update the list

### Can't View File
- Large files may take time to load
- Binary files show hex preview instead of text
- Try downloading the file to view with external tools

### Download Not Working
- Check browser's download settings
- Ensure pop-ups/downloads are not blocked
- Try a different browser if issues persist

## VFS Persistence (IndexedDB) ✅ IMPLEMENTED

The VFS now supports persistence using browser IndexedDB storage. This allows you to save and restore VFS states across browser sessions.

### Features

- **Save VFS States**: Save the current VFS contents with a custom name
- **Load VFS States**: Restore previously saved VFS states
- **State Management**: View, delete, and manage multiple saved states
- **Storage Usage**: Monitor browser storage usage
- **Metadata**: Each state includes executable name, file count, and timestamp

### Usage

1. **Save Current State**:
   - Enter a name for your save state (e.g., "my-save-game")
   - Click "Save" to persist the current VFS to IndexedDB
   - The state is saved with metadata (executable name, file count, timestamp)

2. **View Saved States**:
   - All saved states are listed in the VFS Persistence panel
   - Shows executable name, file count, and save timestamp
   - Storage usage bar indicates how much browser storage is used

3. **Load Saved State**:
   - Click "Load" on any saved state to restore it
   - The VFS will be cleared and replaced with the saved files
   - Loaded files appear in the VFS Browser

4. **Delete States**:
   - Click the trash icon to delete individual states
   - Click "Clear All Saved States" to remove all states at once

### Technical Details

- Uses IndexedDB API for browser-local storage
- Files are stored as base64-encoded strings for JSON serialization
- Database name: `Win32EmuVFS`
- Object store: `vfs_states`
- Each state includes: id, executableName, timestamp, fileCount, files

### Storage Limits

- IndexedDB typically provides 50-100 MB of storage per origin
- Storage usage is shown as a percentage bar
- Consider deleting old states if approaching quota

## Future Enhancements

Potential improvements for the VFS browser:

1. **Search/Filter**: Add search box to filter files by name
2. **Directory Tree**: Show files in a hierarchical tree structure
3. **File Editor**: Allow editing text files and writing back to VFS
4. **File Upload**: Upload files to VFS from local system
5. ~~**IndexedDB Persistence**: Save VFS to browser storage between sessions~~ ✅ COMPLETED
6. **Export All**: Download entire VFS as a ZIP file
7. **File Metadata**: Show creation/modification timestamps
8. **Syntax Highlighting**: Add code highlighting for common file types
9. **Auto-save**: Automatic periodic saving of VFS state during emulation

## Related Files

- `/Win32Emu.Wasm/Components/VirtualFileSystemBrowser.razor` - Main VFS browser component
- `/Win32Emu.Wasm/Components/VfsPersistenceManager.razor` - IndexedDB persistence UI component
- `/Win32Emu.Wasm/Services/EmulatorService.cs` - Service layer
- `/Win32Emu.Wasm/Services/VfsPersistenceService.cs` - IndexedDB persistence service
- `/Win32Emu.Wasm/VirtualFileSystem/BrowserVirtualFileSystem.cs` - VFS implementation
- `/Win32Emu.Wasm/Pages/Home.razor` - Page integration
- `/Win32Emu.Wasm/wwwroot/index.html` - JavaScript functions (including IndexedDB API)
