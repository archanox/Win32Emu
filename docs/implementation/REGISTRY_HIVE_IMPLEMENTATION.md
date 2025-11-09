# Registry Hive Emulation Implementation

## Overview

This implementation adds Windows registry hive emulation to Win32Emu using the DiscUtils.Registry library. The registry provides proper storage for environment variables and other Windows configuration data.

## Architecture

### Components

1. **RegistryHive Class** (`Win32Emu.Win32.Registry.RegistryHive`)
   - Manages Windows registry hives (SYSTEM, SOFTWARE, NTUSER.DAT)
   - Provides APIs for opening, creating, querying, and setting registry keys/values
   - Supports in-memory storage with hooks for future VFS persistence
   - Enumeration APIs for UI support (EnumerateSubKeyNames, EnumerateValueNames)

2. **ProcessEnvironment Integration**
   - Initializes registry hive on construction
   - Synchronizes environment variables with registry storage
   - Maintains backward compatibility with legacy virtual registry
   - Exposes RegistryHive property for UI access

3. **Advapi32 Module Updates**
   - `RegQueryValueExA`: Properly serializes and returns registry data with correct types
   - `RegSetValueExA`: Properly deserializes and stores registry data
   - Full support for REG_SZ, REG_DWORD, REG_BINARY, and REG_EXPAND_SZ types

4. **Registry Viewer UI** (`Win32Emu.Gui`)
   - Interactive Avalonia-based registry editor
   - Tree view for browsing registry hierarchy
   - Data grid for viewing/editing values
   - Accessible from EmulatorWindow via "Registry" button

## Registry Structure

The implementation follows the standard Windows registry structure:

### System Environment Variables
Path: `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment`

Default values:
- PATH
- PATHEXT
- TEMP/TMP
- WINDIR
- SystemRoot
- ComSpec
- OS

### User Environment Variables
Path: `HKEY_CURRENT_USER\Environment`

Default values:
- TEMP/TMP (user-specific)

## Usage

### Using the Registry Viewer UI

When the emulator is running, click the "Registry" button (📋) in the EmulatorWindow status bar to open the registry viewer. The viewer allows you to:

1. **Browse Keys**: Navigate the registry hierarchy using the tree view on the left
2. **View Values**: See all values in the selected key in the data grid on the right
3. **Edit Values**: Double-click a value or use the "Edit Value" button (coming soon)
4. **Add/Delete**: Use toolbar buttons to add or remove keys and values

Environment variables can be viewed and edited at:
- System: `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment`
- User: `HKEY_CURRENT_USER\Environment`

### Reading Environment Variables

Environment variables are automatically synchronized from the registry to the in-memory dictionary. Use standard environment variable APIs:

```csharp
var path = env.GetEnvironmentVariable("PATH");
```

### Setting Environment Variables

When setting environment variables, both the in-memory dictionary and registry are updated:

```csharp
env.SetEnvironmentVariable("MY_VAR", "MyValue");
```

This updates:
1. The in-memory `_environmentVariables` dictionary
2. The registry at `HKCU\Environment\MY_VAR`

### Direct Registry Access

Use the Win32 registry APIs via Advapi32:

```csharp
// Open key
var handle = RegOpenKeyExA(HKEY_LOCAL_MACHINE, "SYSTEM\\...", ...);

// Query value
RegQueryValueExA(handle, "PATH", ...);

// Set value
RegSetValueExA(handle, "MyValue", REG_SZ, ...);

// Close key
RegCloseKey(handle);
```

## Features

### Implemented
- ✅ In-memory registry hive storage
- ✅ Proper registry key/value management
- ✅ Support for multiple data types (REG_SZ, REG_DWORD, REG_BINARY, REG_EXPAND_SZ)
- ✅ Environment variable integration
- ✅ Backward compatibility with legacy virtual registry
- ✅ Standard Windows registry paths

### Future Enhancements
- [ ] VFS persistence (requires custom stream wrapper for IVirtualFileHandle)
- [ ] RegEnumKeyExA implementation with actual enumeration
- [ ] RegEnumValueA implementation with actual enumeration
- [ ] RegDeleteKeyA/RegDeleteValueA with actual deletion
- [ ] HKEY_CLASSES_ROOT merged view implementation
- [ ] Registry security descriptors
- [ ] Registry change notifications

## Testing

### Test Coverage
- `RegistryEnvironmentTests`: Verifies registry integration with environment variables
  - Registry hive initialization
  - Environment variable updates to registry
  - Default values from registry
  
- `EnvironmentTests`: Existing tests for environment variable APIs
  - GetEnvironmentStringsA/W
  - SetEnvironmentVariableA
  - FreeEnvironmentStringsA/W

All tests pass successfully.

## Technical Details

### Data Type Mapping

| Registry Type | C# Type | Notes |
|--------------|---------|-------|
| REG_SZ | string | Null-terminated ASCII string |
| REG_EXPAND_SZ | string | Expandable string (environment vars) |
| REG_DWORD | uint / int | 32-bit integer |
| REG_BINARY | byte[] | Raw binary data |

### Key Handle Management

Registry key handles are allocated starting from `0x80000000` (matching Windows predefined keys). Handles are tracked in the `_openKeys` dictionary and must be closed with `RegCloseKey`.

### Backward Compatibility

The implementation maintains backward compatibility by:
1. Checking the new RegistryHive first for registry operations
2. Falling back to the legacy virtual registry if needed
3. Keeping the legacy `VirtualRegistryKey` class (marked obsolete)

## Dependencies

- `LTRData.DiscUtils.Registry` v1.0.69
- Compatible with existing DiscUtils packages (Core, Fat, Iso9660, etc.)

## Performance

The in-memory implementation provides fast registry operations. Future VFS persistence may add some overhead but will enable registry state to survive emulator restarts.

## Security

- ✅ No security vulnerabilities detected (CodeQL scan clean)
- Registry data is properly validated before use
- Buffer sizes are checked to prevent overflows
- Type conversions are handled safely

## References

- [Windows Registry Structure](https://learn.microsoft.com/en-us/windows/win32/sysinfo/registry)
- [Registry Value Types](https://learn.microsoft.com/en-us/windows/win32/sysinfo/registry-value-types)
- [DiscUtils.Registry Documentation](https://github.com/LTRData/DiscUtils)
