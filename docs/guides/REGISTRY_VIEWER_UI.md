# Registry Viewer UI

## Overview

The Registry Viewer is an Avalonia-based UI component that provides an interactive way to view and edit the Windows registry during emulation.

## Features

- **Hierarchical Key Navigation**: Browse registry keys using a tree view (HKEY_LOCAL_MACHINE, HKEY_CURRENT_USER, etc.)
- **Value Display**: View all values in a selected key with their names, types, and data
- **Real-time Updates**: Changes are immediately reflected in the emulated environment
- **Environment Variable Management**: View and edit environment variables stored at proper registry paths

## Opening the Registry Viewer

### From EmulatorWindow

When a game is running, click the "Registry" button (📋) in the status bar at the bottom of the EmulatorWindow.

### Programmatically

```csharp
var registryWindow = new RegistryViewerWindow
{
    DataContext = new RegistryViewerViewModel(processEnvironment)
};
registryWindow.Show();
```

## UI Layout

### Left Panel: Registry Key Tree
- Shows the registry hierarchy
- Root keys: HKEY_LOCAL_MACHINE, HKEY_CURRENT_USER, HKEY_CLASSES_ROOT, HKEY_USERS
- Click to expand and navigate subkeys
- Selected key's values appear in the right panel

### Right Panel: Values Data Grid
Displays all values in the selected key with columns:
- **Name**: Value name (or "(Default)" for unnamed value)
- **Type**: Registry value type (String, Dword, Binary, etc.)
- **Value**: Formatted display of the value data

### Toolbar
- **Refresh**: Reload the current key's subkeys and values
- **Add Key**: Create a new subkey (coming soon - requires dialog)
- **Delete Key**: Remove the selected key
- **Add Value**: Create a new value (coming soon - requires dialog)
- **Edit Value**: Modify the selected value (coming soon - requires dialog)
- **Delete Value**: Remove the selected value

### Status Bar
Shows feedback messages about operations (loaded keys, errors, etc.)

## Environment Variable Locations

The registry viewer provides direct access to environment variables:

### System Environment Variables
Navigate to: `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment`

Contains system-wide environment variables like:
- PATH
- PATHEXT
- WINDIR
- SystemRoot
- TEMP
- TMP

### User Environment Variables
Navigate to: `HKEY_CURRENT_USER\Environment`

Contains user-specific environment variables like:
- TEMP (user override)
- TMP (user override)
- Custom user variables

## Integration with GameInfoWindow

The GameInfoWindow (accessed via game library) provides a "Environment Variables" text box for configuring variables **before** launching a game. These are saved to the game's configuration file (`GameSettings.json`) and represent the initial environment setup.

The Registry Viewer shows the **runtime** state after the emulator has started and can reflect:
- Default environment variables initialized by the registry hive
- Variables set by the emulated application
- Manual changes made through the registry APIs

Both UIs work together:
1. **Pre-launch**: Use GameInfoWindow to configure initial environment variables
2. **Runtime**: Use Registry Viewer to see and modify the live registry state

## Technical Details

### ViewModel: RegistryViewerViewModel
- `RootKeys`: ObservableCollection of top-level registry keys
- `SelectedKey`: Currently selected key in the tree
- `Values`: ObservableCollection of values in the selected key
- Commands: Refresh, AddKey, DeleteKey, AddValue, EditValue, DeleteValue

### View: RegistryViewerWindow.axaml
- TreeView with TreeDataTemplate for hierarchical display
- DataGrid for value list
- Toolbar with command buttons
- Status bar for user feedback

### Backend: RegistryHive Class
The UI communicates with the `RegistryHive` class which provides:
- `OpenKey(path)`: Open a registry key by path
- `EnumerateSubKeyNames(handle)`: List subkeys
- `EnumerateValueNames(handle)`: List values
- `QueryValue(handle, name)`: Get value data and type
- `SetValue(handle, name, value, type)`: Update value
- `DeleteValue(handle, name)`: Remove value
- `DeleteSubKey(path)`: Remove key

## Future Enhancements

- [ ] Input dialogs for Add Key / Add Value / Edit Value operations
- [ ] Import/Export registry files (.reg format)
- [ ] Search functionality to find keys/values
- [ ] Support for more registry value types (MultiString, QWord, etc.)
- [ ] Refresh on Focus (auto-reload when window receives focus)
- [ ] Change notifications (real-time updates when registry changes)
- [ ] Bookmarks/Favorites for frequently accessed keys

## Screenshot

*(Screenshot to be added - shows the registry viewer with tree view on left and value grid on right)*

## Known Limitations

- Add/Edit operations currently require input dialogs (not yet implemented)
- No undo/redo functionality
- Changes are not persisted to disk (in-memory only, VFS persistence planned)
- Some advanced registry features not yet supported (permissions, security descriptors)
