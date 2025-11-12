# Registry Persistence Implementation - Summary

## Issue Resolution

This PR resolves the issue where registry files were not stored in the virtual disk. Previously, when users mounted a VHD file, they could only see game files but no registry files (SYSTEM, SOFTWARE, NTUSER.DAT).

## Changes Overview

### Core Implementation

1. **RegistryHive.cs** - Enhanced registry management:
   - `CreateOrLoadHive()`: Now loads existing registry hives from VFS if available
   - `SaveHives()`: Persists all registry hives to VFS using DiscUtils
   - `SaveHiveToVfs()`: Writes individual hives to the virtual filesystem
   - `EnsureDirectoryExists()`: Creates necessary directory structure for registry files
   - Helper methods for path management and hive name mapping

2. **DiskVirtualFileSystem.cs** - Added directory creation:
   - `CreateDirectory()`: Public method to create directories in the virtual disk
   - Supports the FAT filesystem used by VHD files

3. **VfsStream.cs** - NEW FILE:
   - Stream wrapper around `IVirtualFileHandle`
   - Enables Stream-based APIs (like DiscUtils.Registry) to work with VFS
   - Implements all required Stream methods (Read, Write, Seek, etc.)

4. **ProcessEnvironment.cs** - Added cleanup:
   - `Cleanup()`: New method to properly dispose resources
   - Calls `RegistryHive.Dispose()` which triggers `SaveHives()`

5. **Emulator.cs** - Lifecycle management:
   - Updated `Dispose()` to call `ProcessEnvironment.Cleanup()`
   - Ensures registry is saved when emulator shuts down

### Testing

**RegistryPersistenceTests.cs** - Comprehensive test suite:
- ✅ Registry saves to VHD with proper file structure
- ✅ Registry loads from existing VHD
- ✅ Registry works without VFS (in-memory fallback)
- ✅ Files are created in Windows-standard locations

### Documentation

**REGISTRY_PERSISTENCE.md** - User guide:
- How registry persistence works
- Usage examples with VHD files
- Instructions for mounting and inspecting VHD files
- Troubleshooting guide
- Technical implementation details

## Registry File Locations

Registry hives are stored in standard Windows locations:

```
C:\
├── Windows\
│   └── System32\
│       └── Config\
│           ├── SYSTEM       (HKEY_LOCAL_MACHINE\SYSTEM)
│           └── SOFTWARE     (HKEY_LOCAL_MACHINE\SOFTWARE)
└── Users\
    └── User\
        └── NTUSER.DAT       (HKEY_CURRENT_USER)
```

## How It Works

1. **On Startup**: 
   - When VFS is initialized, `CreateOrLoadHive()` checks for existing registry files
   - If found, loads them into memory using DiscUtils.Registry
   - If not found, creates new empty hives

2. **During Execution**:
   - Registry operations work in-memory for performance
   - All changes are tracked in the DiscUtils.Registry hives

3. **On Shutdown**:
   - `ProcessEnvironment.Cleanup()` is called from `Emulator.Dispose()`
   - `RegistryHive.Dispose()` calls `SaveHives()`
   - Each hive is saved to its corresponding file in VFS
   - Directories are created automatically if needed

## Backward Compatibility

- ✅ Works without VFS (in-memory mode) - no breaking changes
- ✅ Works with existing VHD files - will create registry files on first save
- ✅ Read-only VHD files are handled gracefully (registry stays in-memory)
- ✅ All existing tests pass

## Benefits

1. **User Experience**:
   - Registry changes now persist between runs
   - Users can inspect registry files by mounting the VHD
   - Game settings and preferences are preserved

2. **Authenticity**:
   - Registry files use Windows format (compatible with Windows tools)
   - Stored in standard Windows locations
   - Mimics real Windows behavior more closely

3. **Debugging**:
   - Registry contents can be examined externally
   - Registry files can be backed up or shared
   - Easier to diagnose registry-related issues

## Testing Notes

- All tests are in `Win32Emu.Tests.Kernel32/RegistryPersistenceTests.cs`
- Tests create temporary VHD files and verify:
  - Registry files are created in correct locations
  - Registry changes persist after cleanup
  - Registry loads correctly from existing VHD
  - In-memory fallback works when VFS is not available

## Potential Edge Cases Handled

1. **VFS not available**: Falls back to in-memory registry
2. **Read-only VHD**: Registry stays in-memory, no errors thrown
3. **Directory doesn't exist**: Automatically created before writing files
4. **Existing registry files**: Loaded and merged with defaults
5. **Multiple startups**: Registry accumulates changes across runs

## Future Enhancements (Out of Scope)

- Registry compaction/optimization
- Registry transaction support
- Registry backup/restore functionality
- Support for additional registry hives (SAM, SECURITY, etc.)

## Security Considerations

- Registry files contain no sensitive data (only game settings)
- Files are stored in standard FAT filesystem (readable by any tool)
- No encryption or access control (not needed for emulation)

## Performance Impact

- **Minimal**: Registry is always in-memory during execution
- **Load time**: Small overhead to read existing registry files on startup (~milliseconds)
- **Save time**: Small overhead to write registry files on shutdown (~milliseconds)
- **Memory**: Registry hives are small (typically < 1 MB total)

## Rollback Plan

If issues are found, the changes can be easily reverted:
- Registry will continue to work in-memory
- All existing functionality is preserved
- No data loss (registry was never persisted before)
