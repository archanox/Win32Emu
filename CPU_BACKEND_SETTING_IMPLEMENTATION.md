# CPU Emulator Backend UI Setting - Implementation Summary

## Overview
This implementation adds a new UI setting that allows users to select which CPU emulation backend to use for running Win32 applications.

## Available CPU Backends

### IcedCPU (Default)
- **Type**: Interpreter-based CPU emulator
- **Status**: Stable, production-ready
- **Features**: 
  - Complete x86 instruction set implementation
  - FPU (x87) support
  - Instruction analyzer support
  - Legacy instruction decoding support
- **Performance**: Good for compatibility
- **Use Case**: Default choice for maximum compatibility

### JitCPU
- **Type**: JIT (Just-In-Time) compiler
- **Status**: Experimental
- **Features**:
  - Compiles x86 code to .NET IL for better performance
  - Async/await support (IAsyncCpu interface)
  - JIT cache with persistent storage
  - Block compilation and optimization
- **Performance**: Potentially faster for compute-intensive applications
- **Use Case**: Experimental performance testing

### Unicorn CPU
- **Type**: Reference emulator (testing only)
- **Status**: Not available as a runtime option
- **Purpose**: Used only for CPU instruction conformance testing
- **Note**: Not included in UI options

## UI Changes

### Settings View Location
The CPU Backend setting is located in the Settings panel between:
- **Input Backend** (above)
- **Resolution Scale Factor** (below)

### UI Elements
```
┌─────────────────────────────────────────┐
│ CPU Emulator Backend                    │
│ ┌─────────────────────────────────────┐ │
│ │ IcedCPU                          ▼ │ │
│ └─────────────────────────────────────┘ │
│ Select the CPU emulation backend.      │
│ IcedCPU is the stable interpreter      │
│ (default). JitCPU is the experimental  │
│ JIT compiler with async support for    │
│ better performance.                     │
└─────────────────────────────────────────┘
```

### Dropdown Options
1. **IcedCPU** - Default, stable interpreter
2. **JitCPU** - Experimental JIT compiler

## Technical Implementation

### Files Modified

1. **Win32Emu.Gui/Models/EmulatorConfiguration.cs**
   - Added `CpuBackend` property with default value "IcedCPU"

2. **Win32Emu.Gui/Configuration/EmulatorSettings.cs**
   - Added `CpuBackend` property with default value "IcedCPU"

3. **Win32Emu.Gui/ViewModels/SettingsViewModel.cs**
   - Added `CpuBackend` observable property
   - Added `CpuBackends` collection with available options
   - Added `OnCpuBackendChanged` handler for auto-save

4. **Win32Emu.Gui/Views/SettingsView.axaml**
   - Added CPU Backend ComboBox with binding
   - Added descriptive text for user guidance

5. **Win32Emu.Gui/Services/EmulatorService.cs**
   - Added logic to detect JitCPU selection
   - Pass `useJitCpu` parameter to `Emulator.LoadExecutable`

6. **Win32Emu.Gui/Configuration/ConfigurationService.cs**
   - Updated `GetEmulatorConfiguration()` to include CpuBackend
   - Updated `SaveEmulatorConfiguration()` to persist CpuBackend
   - Fixed missing properties: InputBackend, EnableInstructionAnalyzer, EnableLegacyInstructionDecoding

### Files Created

1. **Win32Emu.Tests.Gui/CpuBackendSettingsTests.cs**
   - Comprehensive test suite with 9 tests
   - Tests for default values, persistence, and ViewModel behavior
   - All tests passing ✅

## Data Flow

```
User selects CPU backend in Settings UI
         ↓
SettingsViewModel.CpuBackend property changes
         ↓
OnCpuBackendChanged() handler triggered
         ↓
EmulatorConfiguration.CpuBackend updated
         ↓
ConfigurationService.SaveEmulatorConfiguration() called
         ↓
EmulatorSettings.CpuBackend persisted to settings.json
         ↓
When launching a game:
EmulatorService reads CpuBackend from configuration
         ↓
Determines useJitCpu = (CpuBackend == "JitCPU")
         ↓
Calls Emulator.LoadExecutable(useJitCpu: bool)
         ↓
Emulator creates IcedCpu or JitCpu instance
```

## Configuration Storage

The CPU backend setting is stored in the user's application data directory:

**Location**: `%APPDATA%/Win32Emu/settings.json` (Windows)
             `~/.config/Win32Emu/settings.json` (Linux)
             `~/Library/Application Support/Win32Emu/settings.json` (macOS)

**JSON Structure**:
```json
{
  "RenderingBackend": "SDL",
  "InputBackend": "SDL",
  "CpuBackend": "IcedCPU",
  "ResolutionScaleFactor": 1,
  "ReservedMemoryMB": 256,
  "WindowsVersion": "Windows 95",
  "EnableDebugMode": false,
  "EnableGdbServer": false,
  "GdbServerPort": 1234,
  "GdbPauseOnStart": true,
  "EnableInstructionAnalyzer": false,
  "EnableLegacyInstructionDecoding": false,
  "EnableOpenTelemetry": false,
  "UseConsoleExporter": false,
  "UseOtlpExporter": false,
  "OtlpEndpoint": "http://localhost:4317",
  "PerGameSettings": {},
  "ControllerConfigurations": {}
}
```

## Testing

### Test Coverage
- ✅ Default value verification
- ✅ Configuration persistence (save/load)
- ✅ ViewModel observability
- ✅ Configuration service integration
- ✅ Support for all backend types (IcedCPU, JitCPU)
- ✅ ViewModel initialization from configuration
- ✅ Configuration updates on property changes

### Test Results
```
Total tests: 9
Passed: 9
Failed: 0
```

## User Experience

### Before Launch
1. User opens Settings panel
2. User sees CPU Emulator Backend dropdown (default: IcedCPU)
3. User can select between IcedCPU or JitCPU
4. Selection is automatically saved

### During Launch
1. User launches a game
2. EmulatorService reads the selected CPU backend
3. Appropriate CPU emulator is instantiated
4. Game runs with selected backend

### Switching Backends
- Settings are persistent across application restarts
- No need to manually save - changes are auto-saved
- Can be changed at any time (takes effect on next game launch)

## Backward Compatibility

- ✅ Existing configurations without `CpuBackend` property will default to "IcedCPU"
- ✅ No breaking changes to existing code
- ✅ Falls back gracefully if property is missing

## Future Enhancements

Potential future improvements:
1. Per-game CPU backend selection (via GameSettings)
2. Performance metrics comparison between backends
3. Auto-detection of best backend for specific games
4. Advanced JitCPU configuration options (cache size, precompilation settings)

## Notes

- The JitCPU backend is marked as experimental and may have incomplete instruction support
- For maximum compatibility, IcedCPU remains the recommended default
- JitCPU can provide performance benefits for compute-intensive games
- The setting change takes effect on the next game launch (not applied to currently running games)
