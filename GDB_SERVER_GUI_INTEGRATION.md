# GDB Server GUI Integration

## Overview

The GDB server can now be enabled and configured directly from the Win32Emu GUI settings.

## Settings Panel

The Settings view now includes a new "GDB Server (Ghidra/IDA Integration)" section with the following controls:

### Enable GDB Server
- **Type**: Checkbox
- **Default**: Unchecked
- **Description**: Start GDB server for remote debugging with Ghidra, IDA Pro, or GDB

### GDB Server Port
- **Type**: Numeric input
- **Default**: 1234
- **Range**: 1024-65535
- **Description**: Port for GDB server to listen on
- **Enabled**: Only when "Enable GDB Server" is checked

### Pause on Start
- **Type**: Checkbox
- **Default**: Checked
- **Description**: Pause emulation at entry point and wait for debugger to connect
- **Enabled**: Only when "Enable GDB Server" is checked

### Information Panel
When GDB server is enabled, an information panel displays:
- Current port configuration
- Connection instructions for Ghidra
- Connection instructions for GDB command line

## UI Layout

```
┌─────────────────────────────────────────────────┐
│ Emulator Settings                               │
├─────────────────────────────────────────────────┤
│                                                 │
│ [... existing settings ...]                     │
│                                                 │
│ ☑ Enable Debug Mode                            │
│ Enable enhanced debugging to catch memory...   │
│                                                 │
│ ─────────────────────────────────────────────  │
│                                                 │
│ GDB Server (Ghidra/IDA Integration)            │
│                                                 │
│ ☐ Enable GDB Server                            │
│ Start GDB server for remote debugging with...  │
│                                                 │
│ GDB Server Port                                 │
│ [1234        ] ▲▼                              │
│ Port for GDB server to listen on (default...)  │
│                                                 │
│ ☑ Pause on Start                               │
│ Pause emulation at entry point and wait for... │
│                                                 │
│ ┌─────────────────────────────────────────┐   │
│ │ ℹ️ How to Connect                        │   │
│ │                                           │   │
│ │ Once the game starts, the GDB server      │   │
│ │ will be listening on port 1234.           │   │
│ │                                           │   │
│ │ Connect from Ghidra:                      │   │
│ │ Debugger → Connect → gdb → localhost:1234 │   │
│ │                                           │   │
│ │ Connect from GDB:                         │   │
│ │ target remote localhost:1234              │   │
│ └─────────────────────────────────────────┘   │
│                                                 │
└─────────────────────────────────────────────────┘
```

## Behavior

1. **When enabled**: The emulator will start a GDB server on the configured port when launching a game
2. **Pause on Start**: If checked, the game will pause at the entry point waiting for a debugger connection
3. **Port configuration**: The port is validated to be in the range 1024-65535
4. **Settings persistence**: All GDB server settings are saved to the configuration file

## Implementation Details

- Settings are stored in `EmulatorSettings` and persisted to `settings.json`
- UI is reactive - controls are enabled/disabled based on the "Enable GDB Server" checkbox
- Information panel shows dynamically based on enabled state and current port value
- Integration with `EmulatorService` to pass GDB configuration when launching games

## Files Modified

- `Win32Emu.Gui/Models/EmulatorConfiguration.cs` - Added GDB server properties
- `Win32Emu.Gui/Configuration/EmulatorSettings.cs` - Added GDB server settings
- `Win32Emu.Gui/ViewModels/SettingsViewModel.cs` - Added observable properties and change handlers
- `Win32Emu.Gui/Views/SettingsView.axaml` - Added UI controls
- `Win32Emu.Gui/Configuration/ConfigurationService.cs` - Added load/save support
- `Win32Emu.Gui/Services/EmulatorService.cs` - Pass GDB settings to emulator

## Usage

1. Open Win32Emu GUI
2. Navigate to Settings
3. Scroll down to "GDB Server (Ghidra/IDA Integration)" section
4. Check "Enable GDB Server"
5. Configure port if needed (default 1234)
6. Optionally uncheck "Pause on Start" if you want the game to run immediately
7. Launch a game from the library
8. Connect from Ghidra or GDB using the displayed connection string

The emulator will automatically start the GDB server when the game launches.
