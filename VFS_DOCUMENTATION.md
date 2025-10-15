# Virtual File System (VFS) Documentation

## Overview

Win32Emu includes a Virtual File System (VFS) that provides isolation and copy-on-write semantics for file I/O operations. This allows games to run without modifying original installation files, while maintaining per-game save data and configurations independently.

## Architecture

The VFS uses a layered approach with two layers:

1. **Base Layer (Read-Only)**: Contains original game files
2. **Overlay Layer (Read-Write)**: Contains modified files and new files specific to this game instance

When a file is accessed:
- **Read operations**: Check overlay first, then fall back to base
- **Write operations**: Automatically copy file from base to overlay (copy-on-write), then modify
- **Delete operations**: Remove from overlay (base files remain intact)
- **Move operations**: Move within overlay (copies from base if needed)

## Usage

### Initializing VFS

```csharp
// In your emulator setup code
var processEnv = new ProcessEnvironment(virtualMemory);

// Initialize VFS with game directory as base and optional overlay directory
processEnv.InitializeVirtualFileSystem(
    baseDirectory: @"C:\Games\MyGame",
    overlayDirectory: @"C:\Users\YourName\AppData\Local\Win32Emu\MyGame"
);
```

If `overlayDirectory` is `null`, a temporary directory will be used.

### Per-Game Isolation

Each game instance can have its own overlay:

```csharp
// Game 1
var processEnv1 = new ProcessEnvironment(vm1);
processEnv1.InitializeVirtualFileSystem(
    @"C:\Games\SharedGame",
    @"C:\Users\YourName\AppData\Local\Win32Emu\Game1_SaveData"
);

// Game 2 - Same base, different overlay
var processEnv2 = new ProcessEnvironment(vm2);
processEnv2.InitializeVirtualFileSystem(
    @"C:\Games\SharedGame",
    @"C:\Users\YourName\AppData\Local\Win32Emu\Game2_SaveData"
);
```

Now both game instances can modify files independently without affecting each other or the original installation.

## Supported Operations

All Win32 file I/O APIs automatically use VFS when initialized:

- `CreateFileA` - Opens/creates files through VFS
- `ReadFile` - Reads from VFS handles
- `WriteFile` - Writes to VFS handles (with copy-on-write)
- `DeleteFileA` - Deletes files from overlay
- `MoveFileA` - Moves files within overlay
- `SetFilePointer` - Seeks within VFS files
- `FlushFileBuffers` - Flushes VFS file buffers
- `SetEndOfFile` - Truncates VFS files
- `FindFirstFileA` - Enumerates files from both layers
- `CloseHandle` - Closes VFS handles

## Path Handling

The VFS automatically normalizes paths:

```csharp
// These are all equivalent:
"config.ini"
"./config.ini"
@"C:\config.ini"
@"C:\Games\config.ini"  // Relative to VFS root
```

Paths are normalized to remove drive letters and leading slashes, making them relative to the VFS root.

## Copy-on-Write Semantics

When you modify a file that exists in the base layer:

1. File is automatically copied from base to overlay
2. Modifications are made to the overlay copy
3. Base file remains unchanged
4. Subsequent reads/writes use the overlay version

Example:

```csharp
// Base layer has: config.ini
vfs.OpenFile("config.ini", VfsFileMode.Open, VfsFileAccess.Write);
// -> File is copied from base to overlay
// -> Overlay now has: config.ini (writable)
// -> Base still has: config.ini (unchanged)
```

## Use Cases

### 1. Game Installation Preservation

Keep game installations pristine while allowing games to save data:

```csharp
processEnv.InitializeVirtualFileSystem(
    baseDirectory: gameInstallPath,
    overlayDirectory: Path.Combine(saveDataPath, gameId)
);
```

### 2. Multiple Save Slots

Each save slot can have its own overlay:

```csharp
// Save Slot 1
var slot1Overlay = Path.Combine(saveDataPath, gameId, "slot1");
processEnv1.InitializeVirtualFileSystem(gameInstallPath, slot1Overlay);

// Save Slot 2
var slot2Overlay = Path.Combine(saveDataPath, gameId, "slot2");
processEnv2.InitializeVirtualFileSystem(gameInstallPath, slot2Overlay);
```

### 3. Mod Testing

Test mods without affecting the original installation:

```csharp
// Original game
processEnv.InitializeVirtualFileSystem(gameInstallPath, null);

// With mods - overlay contains modified files
var moddedOverlay = Path.Combine(tempPath, "modded");
processEnvModded.InitializeVirtualFileSystem(gameInstallPath, moddedOverlay);
```

## Implementation Details

### VFS Interfaces

```csharp
public interface IVirtualFileSystem
{
    IVirtualFileHandle? OpenFile(string path, VfsFileMode mode, VfsFileAccess access);
    bool DeleteFile(string path);
    bool MoveFile(string existingPath, string newPath);
    bool FileExists(string path);
    string[] GetFiles(string directory, string pattern);
}

public interface IVirtualFileHandle : IDisposable
{
    int Read(byte[] buffer, int offset, int count);
    void Write(byte[] buffer, int offset, int count);
    long Seek(long offset, SeekOrigin origin);
    long Position { get; }
    void SetLength(long length);
    void Flush();
}
```

### File Modes

```csharp
public enum VfsFileMode
{
    CreateNew,      // Create new file, fail if exists
    Create,         // Create new or truncate existing
    Open,           // Open existing, fail if doesn't exist
    OpenOrCreate,   // Open existing or create new
    Truncate        // Open and truncate existing
}
```

### File Access

```csharp
public enum VfsFileAccess
{
    Read,       // Read-only access
    Write,      // Write-only access
    ReadWrite   // Read and write access
}
```

## Performance Considerations

1. **First Write Penalty**: First write to a base file incurs a copy operation
2. **Subsequent Operations**: Fast - work directly with overlay files
3. **Memory Usage**: Each copied file duplicates storage (base + overlay)
4. **Disk Space**: Overlay directory grows with modified/new files

## Backwards Compatibility

VFS is optional. If not initialized, file operations fall back to direct filesystem access:

```csharp
// Without VFS - uses direct filesystem
var processEnv = new ProcessEnvironment(vm);
// CreateFileA, ReadFile, etc. work normally

// With VFS - uses layered filesystem
processEnv.InitializeVirtualFileSystem(baseDir, overlayDir);
// CreateFileA, ReadFile, etc. use VFS
```

## Example: Complete Setup

```csharp
using Win32Emu;
using Win32Emu.Memory;
using Win32Emu.Win32;

// Create emulator components
var memory = new VirtualMemory();
var processEnv = new ProcessEnvironment(memory);

// Set up VFS for the game
var gameInstallDir = @"C:\Games\Ignition";
var saveDataDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Win32Emu",
    "Ignition",
    "SaveSlot1"
);

// Initialize VFS
processEnv.InitializeVirtualFileSystem(gameInstallDir, saveDataDir);

// Now all file operations are virtualized
// The game can modify files without touching the installation
```

## Testing

The VFS includes comprehensive tests:

- **Unit Tests**: 12 tests for core VFS functionality
- **Integration Tests**: 9 tests for Kernel32 API integration
- **Coverage**: CreateFile, ReadFile, WriteFile, DeleteFile, MoveFile, etc.

Run tests:

```bash
dotnet test Win32Emu.Tests.Kernel32 --filter "FullyQualifiedName~VirtualFileSystem"
dotnet test Win32Emu.Tests.Kernel32 --filter "FullyQualifiedName~VfsIntegration"
```

## Future Enhancements

Potential improvements:

1. **Virtual Registry**: Similar VFS approach for registry operations
2. **Compression**: Compress overlay files to save disk space
3. **Deduplication**: Share unchanged files between overlays
4. **Snapshots**: Save/restore overlay state
5. **Merge**: Ability to merge overlay changes back to base (with caution)
6. **Quota Management**: Limit overlay size per game
7. **Performance Monitoring**: Track copy-on-write operations
8. **Directory Operations**: Support for CreateDirectory, RemoveDirectory

## Troubleshooting

### Files Not Found

If VFS reports files not found:

1. Check base directory path is correct
2. Verify files exist in base directory
3. Check file permissions on base directory
4. Ensure paths are using correct separators (handled automatically)

### Overlay Growing Large

If overlay directory becomes too large:

1. Review which files have been modified
2. Consider deleting overlay to start fresh
3. Implement game-specific cleanup logic
4. Use separate overlays for different game sessions

### Permission Issues

If encountering permission errors:

1. Ensure base directory is readable
2. Ensure overlay directory is writable
3. Run emulator with appropriate permissions
4. Check anti-virus isn't blocking file operations
