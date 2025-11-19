# Editing Registry When Game Is Not Running

Win32Emu now persists registry hives to the virtual disk, allowing you to inspect and edit registry settings when the game is not running.

## Registry File Locations

Registry hives are stored in Windows-standard locations within the virtual disk:

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

## Mounting the Virtual Disk

### On Windows

1. **Using Disk Management:**
   ```powershell
   # Mount VHD
   Mount-VHD -Path "C:\path\to\your\game.vhd"
   
   # After editing, unmount
   Dismount-VHD -Path "C:\path\to\your\game.vhd"
   ```

2. **Using File Explorer:**
   - Right-click the `.vhd` file
   - Select "Mount"
   - After editing, right-click the drive and select "Eject"

### On Linux

```bash
# Install guestfs-tools if not already installed
sudo apt-get install guestfs-tools

# Mount the VHD
sudo guestmount -a game.vhd -i --rw /mnt/game-vhd

# After editing, unmount
sudo guestunmount /mnt/game-vhd
```

### On macOS

```bash
# Install qemu if not already installed
brew install qemu

# Convert VHD to DMG for mounting
qemu-img convert -f vpc -O dmg game.vhd game.dmg

# Mount the DMG
hdiutil attach game.dmg

# After editing, unmount
hdiutil detach /Volumes/VOLUME_NAME
```

## Editing Registry Files

### Option 1: Using Windows Registry Editor (Windows only)

1. Mount the VHD as described above
2. Open Registry Editor (`regedit.exe`)
3. Select `HKEY_LOCAL_MACHINE`
4. Go to File → Load Hive
5. Navigate to the mounted drive and load the registry file:
   - For SYSTEM: `D:\Windows\System32\Config\SYSTEM` (where D: is your mounted drive)
   - For SOFTWARE: `D:\Windows\System32\Config\SOFTWARE`
6. Provide a key name (e.g., "GameSystem")
7. Edit the values under your loaded hive
8. When done, select the hive and go to File → Unload Hive
9. Unmount the VHD

### Option 2: Using DiscUtils.Registry (Cross-platform .NET tool)

```csharp
using DiscUtils.Registry;
using DiscUtils.Vhd;
using System.IO;

// Open the VHD
using var disk = new Disk("game.vhd", FileAccess.ReadWrite);
var volume = disk.Partitions[0].Open();
var fs = new DiscUtils.Fat.FatFileSystem(volume);

// Open a registry hive
using var hiveStream = fs.OpenFile(@"\Windows\System32\Config\SYSTEM", FileMode.Open, FileAccess.ReadWrite);
var hive = new RegistryHive(hiveStream);

// Navigate and edit values
var key = hive.Root.OpenSubKey(@"CurrentControlSet\Control\Session Manager\Environment");
key.SetValue("MY_VAR", "MyValue");

// Save changes
hive.Dispose();
hiveStream.Dispose();
```

### Option 3: Using hivexregedit (Linux)

```bash
# Install hivex if not already installed
sudo apt-get install libhivex-bin

# Mount the VHD
sudo guestmount -a game.vhd -i --rw /mnt/game-vhd

# Export a key to a .reg file
hivexregedit --export /mnt/game-vhd/Windows/System32/Config/SYSTEM 'CurrentControlSet\Control\Session Manager\Environment' > env.reg

# Edit the .reg file with a text editor
nano env.reg

# Import the changes back
hivexregedit --merge /mnt/game-vhd/Windows/System32/Config/SYSTEM env.reg

# Unmount
sudo guestunmount /mnt/game-vhd
```

## Common Registry Modifications

### Changing Environment Variables

**HKEY_CURRENT_USER\Environment** (User variables):
- `TEMP` - Temporary files location
- `TMP` - Temporary files location

**HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment** (System variables):
- `PATH` - System path
- `WINDIR` - Windows directory
- `SystemRoot` - System root directory

### Changing Game Settings

Game-specific settings are typically stored in:
- `HKEY_CURRENT_USER\Software\<GameName>`
- `HKEY_LOCAL_MACHINE\SOFTWARE\<GameName>`

Common settings:
- Graphics options (resolution, quality settings)
- Audio settings (volume, audio device)
- Control bindings
- License keys or activation data

## Safety Tips

1. **Always backup your VHD before making registry changes:**
   ```bash
   cp game.vhd game.vhd.backup
   ```

2. **Don't edit registry while the game is running:**
   - Close the emulator completely before mounting the VHD
   - Changes made while the game is running will be overwritten

3. **Be careful with data types:**
   - Strings: `REG_SZ`
   - Numbers: `REG_DWORD`
   - Binary: `REG_BINARY`

4. **Validate registry changes:**
   - After making changes, start the emulator and verify the game still works
   - If something breaks, restore from backup

## Troubleshooting

### VHD Won't Mount
- Ensure the emulator is completely closed
- Check if another process has the VHD file open
- Verify the VHD file isn't corrupted

### Registry Changes Don't Persist
- Make sure you unmounted the VHD properly
- Verify the VHD file isn't read-only
- Check file permissions

### Game Doesn't Start After Registry Edit
- Restore from backup: `cp game.vhd.backup game.vhd`
- Review what you changed and try a smaller change
- Check emulator logs for error messages

## Example: Changing Game Resolution

1. Mount the VHD
2. Load `SOFTWARE` hive in regedit
3. Navigate to `HKEY_LOCAL_MACHINE\SOFTWARE\MyGame\Settings`
4. Change `ScreenWidth` to `1920` (DWORD)
5. Change `ScreenHeight` to `1080` (DWORD)
6. Unload hive and unmount VHD
7. Start the game

## Advanced: Automating Registry Edits

You can create scripts to automate common registry modifications:

```python
#!/usr/bin/env python3
import subprocess
import sys

def get_hive_file_path(hive):
    """Map registry hive paths to their file system locations."""
    hive_map = {
        'HKEY_LOCAL_MACHINE\\SYSTEM': '/mnt/game/Windows/System32/Config/SYSTEM',
        'HKEY_LOCAL_MACHINE\\SOFTWARE': '/mnt/game/Windows/System32/Config/SOFTWARE',
        'HKEY_CURRENT_USER': '/mnt/game/Users/User/NTUSER.DAT',
    }
    return hive_map.get(hive)

def set_registry_value(vhd_path, hive, key_path, value_name, value):
    # Mount VHD
    subprocess.run(['sudo', 'guestmount', '-a', vhd_path, '-i', '--rw', '/mnt/game'])
    
    # Create .reg file
    reg_content = f'''Windows Registry Editor Version 5.00

[{hive}\\{key_path}]
"{value_name}"="{value}"
'''
    with open('/tmp/edit.reg', 'w') as f:
        f.write(reg_content)
    
    # Apply changes
    hive_file = get_hive_file_path(hive)
    if hive_file:
        subprocess.run(['hivexregedit', '--merge', hive_file, '/tmp/edit.reg'])
    
    # Unmount
    subprocess.run(['sudo', 'guestunmount', '/mnt/game'])

if __name__ == '__main__':
    set_registry_value('game.vhd', 
                      'HKEY_LOCAL_MACHINE\\SYSTEM',
                      'CurrentControlSet\\Control\\Session Manager\\Environment',
                      'MY_VAR',
                      'MyValue')
```

## Further Reading

- [Windows Registry Structure](https://learn.microsoft.com/en-us/windows/win32/sysinfo/registry)
- [DiscUtils Documentation](https://github.com/DiscUtils/DiscUtils)
- [Hivex Tools Documentation](http://libguestfs.org/hivex.1.html)
- [VHD File Format Specification](https://www.microsoft.com/en-us/download/details.aspx?id=23850)
