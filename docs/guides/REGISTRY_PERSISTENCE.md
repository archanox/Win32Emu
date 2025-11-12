# Registry Persistence in Win32Emu

## Overview

Win32Emu now supports persistent registry storage in virtual disk files (VHD/VHDX). When you run an emulated application with a virtual disk, registry changes are automatically saved to the disk and will be preserved across runs.

## How It Works

### Registry Storage Locations

Registry hives are stored in standard Windows locations within the virtual disk:

- **HKEY_LOCAL_MACHINE\SYSTEM** → `C:\Windows\System32\Config\SYSTEM`
- **HKEY_LOCAL_MACHINE\SOFTWARE** → `C:\Windows\System32\Config\SOFTWARE`
- **HKEY_CURRENT_USER** → `C:\Users\User\NTUSER.DAT`

### Automatic Persistence

When you use a virtual disk (VHD/VHDX), the emulator will:

1. **On startup**: Load existing registry files from the virtual disk (if they exist)
2. **During execution**: Store registry changes in memory
3. **On shutdown**: Save all registry hives back to the virtual disk

This means that any changes made by emulated applications (game settings, saved preferences, etc.) will persist between runs.

## Usage Examples

### Using a Virtual Disk

```bash
# Run an application with a virtual disk
win32emu --vhd my-game.vhd game.exe

# On subsequent runs, registry changes will be preserved
win32emu --vhd my-game.vhd game.exe
```

### Creating a New Virtual Disk

The emulator can create a new VHD automatically if it doesn't exist:

```bash
# The emulator will create my-game.vhd if it doesn't exist
win32emu --vhd my-game.vhd --vhd-size 500M game.exe
```

### Inspecting Registry Files

You can mount the VHD file on your host system to inspect the registry files:

**On Windows:**
```powershell
# Mount the VHD
Mount-VHD -Path my-game.vhd

# Browse to the mounted drive and look in:
# \Windows\System32\Config\
# \Users\User\
```

**On Linux (with qemu-nbd):**
```bash
# Load the nbd module
sudo modprobe nbd max_part=8

# Connect the VHD as a block device
sudo qemu-nbd --connect=/dev/nbd0 my-game.vhd

# Mount the partition
sudo mount /dev/nbd0p1 /mnt

# Browse to /mnt/Windows/System32/Config/
```

## In-Memory Mode

If you don't specify a virtual disk, the registry will work in-memory only:

```bash
# Registry changes will NOT persist between runs
win32emu game.exe
```

This is useful for testing or when you don't need to preserve settings.

## Technical Details

### File Format

Registry hives are stored using the Windows registry hive file format (as implemented by DiscUtils.Registry). This is the same format used by actual Windows systems.

### Directory Structure

When registry files are saved for the first time, the emulator will automatically create the necessary directory structure:

```
C:\
├── Windows\
│   └── System32\
│       └── Config\
│           ├── SYSTEM
│           └── SOFTWARE
└── Users\
    └── User\
        └── NTUSER.DAT
```

### Default Values

The registry is initialized with default Windows environment variables and settings:

- **PATH**: Standard Windows system paths
- **WINDIR**: `C:\WINDOWS`
- **TEMP/TMP**: `C:\TEMP` (system), `C:\Users\User\AppData\Local\Temp` (user)
- **ComSpec**: `C:\WINDOWS\system32\cmd.exe`

These values are set in the registry on first initialization and can be modified by emulated applications.

## Troubleshooting

### Registry files not appearing in VHD

Make sure you're:
1. Using a VHD/VHDX file (not running in-memory mode)
2. Allowing the emulator to shut down cleanly (registry is saved on disposal)
3. Using a writable VHD (not read-only)

### Registry changes not persisting

Check if:
1. The VHD file has write permissions
2. There's enough free space in the VHD
3. The emulator is being terminated properly (not killed forcefully)

### Looking at logs

Enable detailed logging to see registry operations:

```bash
win32emu --debug --vhd my-game.vhd game.exe
```

Look for log entries containing `[RegistryHive]` to see when registry is loaded/saved.

## Implementation Notes

For developers working on Win32Emu:

- Registry persistence is implemented in `Win32Emu/Win32/Registry/RegistryHive.cs`
- The `SaveHives()` method is called from `ProcessEnvironment.Cleanup()`
- Directory creation is handled by `DiskVirtualFileSystem.CreateDirectory()`
- Tests are available in `Win32Emu.Tests.Kernel32/RegistryPersistenceTests.cs`
