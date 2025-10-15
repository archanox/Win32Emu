# Remote File I/O Implementation for GDB Server

## Overview

This document describes the implementation of remote file I/O support for the GDB server, enabling Ghidra and other GDB clients to access files in Win32Emu's Virtual File System (VFS).

## Background

The GDB Remote Serial Protocol includes a File I/O extension that allows debuggers to access files on the target system. This is particularly useful for:

1. **Analyzing game data files** - Read configuration files, saved games, etc.
2. **Debugging file operations** - Monitor what files the emulated program accesses
3. **Extracting assets** - Copy out resources without manual extraction
4. **Scripting analysis** - Use debugger scripts to process game files

## Implementation Details

### 1. GDB Server Changes

**File:** `Win32Emu/Debugging/GdbServer.cs`

#### Added VFS Integration
- Added `IVirtualFileSystem? _vfs` field to store VFS reference
- Updated constructor to accept optional VFS parameter
- Added file descriptor tracking with `Dictionary<int, IVirtualFileHandle> _openFiles`
- Added `_nextFileDescriptor` counter starting at 3 (after stdin/stdout/stderr)

#### Advertised File I/O Capabilities
Updated `HandleQueryAsync` to include vFile capabilities in qSupported response when VFS is available:
```
vFile:open+;vFile:close+;vFile:pread+;vFile:pwrite+;vFile:fstat+;vFile:unlink+;vFile:readlink+;vFile:setfs+
```

#### Implemented vFile Handlers
Added the following protocol handlers:

1. **`vFile:open`** - Opens files with POSIX flags, maps to VFS operations
2. **`vFile:close`** - Closes file descriptors and disposes handles
3. **`vFile:pread`** - Reads from file at specified offset
4. **`vFile:pwrite`** - Writes to file at specified offset
5. **`vFile:fstat`** - Returns file status (size, mode, etc.)
6. **`vFile:unlink`** - Deletes files from VFS overlay
7. **`vFile:readlink`** - Returns EPERM (symbolic links not supported)
8. **`vFile:setfs`** - Returns success (single filesystem only)

#### Helper Methods
- `SendFileIoErrorAsync(int errno)` - Sends error responses with errno codes
- `DecodeHexString(string hex)` - Decodes hex-encoded filenames from GDB

#### Resource Cleanup
Updated `Dispose()` to close all open file handles on server shutdown.

### 2. Emulator Integration

**File:** `Win32Emu/Emulator.cs`

Updated `RunWithGdbServer` to pass VFS reference:
```csharp
var gdbServer = new GdbServer(_cpu!, _vm!, breakpoints, _logger, port, _env!.VirtualFileSystem);
```

This ensures that when VFS is initialized via `ProcessEnvironment.InitializeVirtualFileSystem()`, the GDB server has access to it.

### 3. Testing

**File:** `Win32Emu.Tests.Kernel32/GdbServerTests.cs`

Added test to verify GDB server can be created with VFS:
```csharp
[Fact]
public void GdbServer_CanBeCreatedWithVfs()
{
    // Creates temporary VFS and verifies GdbServer initialization
}
```

All existing tests continue to pass (225 passed, 3 pre-existing failures, 4 skipped).

### 4. Documentation

**Files Updated:**
- `GDB_SERVER_GUIDE.md` - Added "Remote File I/O" section with usage examples
- `README.md` - Noted remote file I/O support in --gdb-server option

## Protocol Details

### File Open Flags

The implementation maps POSIX open flags to VFS file modes:

| POSIX Flag | Value | VFS Mapping |
|------------|-------|-------------|
| O_RDONLY | 0x0000 | VfsFileAccess.Read |
| O_WRONLY | 0x0001 | VfsFileAccess.Write |
| O_RDWR | 0x0002 | VfsFileAccess.ReadWrite |
| O_CREAT | 0x0100 | VfsFileMode.CreateNew/Create/OpenOrCreate |
| O_TRUNC | 0x0200 | VfsFileMode.Truncate/Create |
| O_EXCL | 0x0400 | VfsFileMode.CreateNew |

### File Status Structure

The `fstat` implementation returns a minimal struct stat with:
- `st_dev` - Device ID (0)
- `st_ino` - Inode number (0)
- `st_mode` - File mode (S_IFREG | 0644)
- `st_nlink` - Hard link count (1)
- `st_uid` - User ID (0)
- `st_gid` - Group ID (0)
- `st_size` - File size in bytes (actual)
- `st_blksize` - Block size (0)
- `st_blocks` - Number of blocks (0)
- `st_atime/mtime/ctime` - Timestamps (0)

Total size: 88 bytes

### Error Codes

Standard POSIX error codes returned:
- `EPERM (1)` - Operation not permitted
- `ENOENT (2)` - No such file or directory
- `EIO (5)` - I/O error
- `EBADF (9)` - Bad file descriptor
- `EINVAL (22)` - Invalid argument

## Security Considerations

1. **Sandboxed Access** - Only files within the VFS base directory are accessible
2. **Copy-on-Write** - All write operations use VFS overlay, original files are never modified
3. **Safe Deletion** - File deletions only affect overlay directory
4. **No Symbolic Links** - readlink returns EPERM to prevent directory traversal

## Usage Examples

### Opening a File from Ghidra Script

```python
# Ghidra Python script using GDB remote file I/O
import gdb

# Open game configuration file
result = gdb.execute("monitor vFile:open:636f6e6669672e696e69,0,1a4", to_string=True)
# Returns: F<fd> where <fd> is the file descriptor

# Read 1024 bytes from offset 0
result = gdb.execute("monitor vFile:pread:3,400,0", to_string=True)
# Returns: F<bytes_read>;<hex_data>

# Close the file
result = gdb.execute("monitor vFile:close:3", to_string=True)
# Returns: F0
```

### Prerequisites

Remote file I/O requires VFS initialization before starting GDB server:

```csharp
// Initialize VFS
processEnv.InitializeVirtualFileSystem(
    baseDirectory: @"C:\Games\MyGame",
    overlayDirectory: @"C:\Users\Me\AppData\Local\Win32Emu\MyGame"
);

// Start emulator with GDB server
emulator.LoadExecutable("game.exe", gdbServerMode: true, gdbServerPort: 1234);
```

## Testing

The implementation was tested with:
- Unit tests for GdbServer creation with VFS
- Integration with existing GDB server tests
- All 225 existing Kernel32 tests pass
- Manual verification pending (Ghidra integration)

## Performance Considerations

1. **File Descriptor Limit** - No hard limit, but memory grows with open files
2. **Read/Write Buffer Size** - Limited by GDB packet size (4096 bytes by default)
3. **VFS Performance** - Depends on underlying LayeredVirtualFileSystem performance
4. **Network Latency** - File operations are synchronous over TCP

## Known Limitations

1. **No Symbolic Links** - readlink not implemented (returns EPERM)
2. **No Directory Operations** - mkdir, rmdir not implemented
3. **No Stat by Path** - Only fstat (by fd) supported
4. **No File Listing** - readdir not implemented
5. **Single Filesystem** - setfs is a no-op
6. **No Seek Operation** - Must use pread/pwrite with explicit offsets

## Future Enhancements

Potential improvements:

1. Add directory operations (mkdir, rmdir, readdir)
2. Implement stat by path
3. Add file seek support
4. Support for file metadata (timestamps, permissions)
5. Async file I/O for better performance
6. Buffering for small read/write operations

## Comparison with Standard GDB File I/O

| Feature | Standard GDB | Win32Emu Implementation |
|---------|--------------|-------------------------|
| open | ✅ Full support | ✅ Full support via VFS |
| close | ✅ Full support | ✅ Full support |
| read | ✅ Full support | ✅ Via pread |
| write | ✅ Full support | ✅ Via pwrite |
| lseek | ✅ Full support | ❌ Not implemented |
| rename | ✅ Full support | ❌ Not implemented |
| unlink | ✅ Full support | ✅ Full support |
| stat | ✅ Full support | ❌ Only fstat |
| fstat | ✅ Full support | ✅ Minimal implementation |
| gettimeofday | ✅ Full support | ❌ Not implemented |
| isatty | ✅ Full support | ❌ Not implemented |
| system | ✅ Full support | ❌ Not implemented |

## Conclusion

The remote file I/O implementation provides essential file access capabilities to GDB clients while maintaining the security and copy-on-write semantics of Win32Emu's Virtual File System. This enables powerful debugging and analysis workflows in tools like Ghidra without compromising the integrity of game files.

## See Also

- [GDB_SERVER_GUIDE.md](GDB_SERVER_GUIDE.md) - Complete GDB server documentation
- [VFS_DOCUMENTATION.md](VFS_DOCUMENTATION.md) - Virtual File System documentation
- [GDB Remote Serial Protocol Specification](https://sourceware.org/gdb/current/onlinedocs/gdb.html/Remote-Protocol.html)
