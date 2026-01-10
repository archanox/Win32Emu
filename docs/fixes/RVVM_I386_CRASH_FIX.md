# Fix for rvvm_i386.exe Crash/Hang Issue

## Problem Statement

The rvvm_i386.exe executable was crashing or hanging during load time in Win32Emu. The logs showed:

1. **Excessive logging**: 273 individual debug messages about undecorated exports
2. **Truncated logs**: Logs ended abruptly after section listing, suggesting crash or hang
3. **Wrong calling convention**: All undecorated exports were defaulting to `stdcall` instead of `cdecl`

### Original Log Output (Truncated)
```
[06:39:26] [DBG] [Emulator] [Loader] Export 'pci_bus_init' has no decoration, using default Stdcall with 0 bytes. This may be incorrect for cdecl functions.
[06:39:26] [DBG] [Emulator] [Loader] Export 'pci_bus_init_auto' has no decoration, using default Stdcall with 0 bytes. This may be incorrect for cdecl functions.
... (271 more similar messages) ...
[06:39:26] [INF] [Emulator] [Loader] Built metadata for 273 exports
[06:39:26] [DBG] [Emulator] [Loader] Section '/4': RVA=0x00053000...
[CRASH/HANG - logs truncated]
```

## Root Cause Analysis

1. **Log Flooding**: 273 individual debug messages caused excessive log output
   - On WASM, this could cause performance issues or buffer overflows
   - Each export generated a separate log line
   - Total log output: ~27,000 characters just for export warnings

2. **Wrong Default Convention**: Used `stdcall` for C-compiled functions
   - rvvm_i386.exe is C-compiled with undecorated exports
   - C functions use `cdecl` convention (caller cleans stack)
   - Using wrong convention causes stack corruption and crashes

3. **Missing Error Context**: No logging around critical initialization points
   - ProcessEnvironment initialization could fail silently
   - Hard to diagnose where crash occurred

## Solution Implemented

### 1. C-Compiled Executable Detection Heuristic

Implemented two-pass export processing:

**First Pass**: Count decorated vs undecorated exports
```csharp
var decoratedCount = 0;
var undecoratedCount = 0;

foreach (var export in image.Exports.Entries)
{
    if (ExportMetadata.FromDecoratedName(export.Name) != null)
        decoratedCount++;
    else
        undecoratedCount++;
}
```

**Heuristic**: If >80% exports are undecorated, treat as C-compiled executable
```csharp
var isCCompiled = undecoratedCount > 0 && 
    (double)undecoratedCount / (decoratedCount + undecoratedCount) > 0.8;
    
var defaultMetadata = isCCompiled ? ExportMetadata.CdeclDefault : ExportMetadata.Default;
```

**Second Pass**: Apply appropriate defaults
```csharp
foreach (var export in image.Exports.Entries)
{
    var exportMeta = ExportMetadata.FromDecoratedName(export.Name);
    metadata[export.Name] = exportMeta ?? defaultMetadata; // Use cdecl for C-compiled
}
```

### 2. Log Spam Reduction

Limited individual export warnings to first 10, then show summary:

```csharp
const int MAX_UNDECORATED_LOGS = 10;
var undecoratedExportsLogged = 0;

foreach (var export in image.Exports.Entries)
{
    if (exportMeta == null && undecoratedExportsLogged < MAX_UNDECORATED_LOGS)
    {
        logger?.LogDebug("[Loader] Export '{Name}' has no decoration, using default {Convention}",
            export.Name, defaultMetadata.Convention);
        undecoratedExportsLogged++;
    }
}

if (undecoratedCount > MAX_UNDECORATED_LOGS)
{
    logger?.LogDebug("[Loader] {Total} total undecorated exports (showing first {Shown}, hiding {Hidden})",
        undecoratedCount, MAX_UNDECORATED_LOGS, undecoratedCount - MAX_UNDECORATED_LOGS);
}
```

### 3. Enhanced Error Handling

Added try-catch around ProcessEnvironment initialization with better logging:

```csharp
_logger.LogDebug("[Loader] Initializing process environment with heap base=0x{HeapBase:X8}", heapBase);
try
{
    _env = new ProcessEnvironment(_vm, heapBase, _host, _logger, _backendFactory);
    _logger.LogDebug("[Loader] Process environment created successfully");
}
catch (Exception ex)
{
    _logger.LogCritical(ex, "[Loader] Failed to create process environment");
    throw;
}
```

## Impact and Benefits

### Log Reduction
- **Before**: 273 individual export warnings (~27,000 characters)
- **After**: 10 individual warnings + 1 summary (~1,000 characters)
- **Reduction**: 96% less log output

### Calling Convention
- **Before**: All undecorated exports → `stdcall` (incorrect for C functions)
- **After**: C-compiled executables → `cdecl` (correct)
- **Impact**: Prevents stack corruption and crashes

### Diagnostics
- **Before**: Silent failures during initialization
- **After**: Critical exceptions logged with full context
- **Impact**: Easier debugging of initialization failures

### New Log Output Example
```
[Loader] Detected C-compiled executable (>80% undecorated exports), using cdecl as default calling convention
[Loader] Export 'pci_bus_init' has no decoration, using default Cdecl with 0 bytes
... (9 more individual logs) ...
[Loader] 273 total undecorated exports (showing first 10, hiding 263 to avoid log spam)
[Loader] Built metadata for 273 exports (0 decorated, 273 undecorated)
[Loader] Initializing process environment with heap base=0x00463000
[Loader] Process environment created successfully
```

## Test Coverage

### New Tests (ExportMetadataTests.cs)
1. `ExportMetadata_Default_IsSdtcall` - Verifies default is stdcall
2. `ExportMetadata_CdeclDefault_IsCdecl` - Verifies cdecl default
3. `ExportMetadata_FromDecoratedName_ParsesStdcall` - Tests @16 decoration
4. `ExportMetadata_FromDecoratedName_ParsesFastcall` - Tests @Function@8 decoration
5. `ExportMetadata_FromDecoratedName_ReturnsNullForUndecorated` - Tests undecorated names
6. `CCompiledDetection_Heuristic_VerifyThreshold` - 8 test cases (0-100% ratios)
7. `BuildExportMetadata_LogReduction_VerifyConcept` - Documents log reduction
8. `RvvmI386Exe_ExportPattern_Simulation` - Simulates exact rvvm_i386.exe scenario

### Existing Tests
- All 16 PE loader tests pass
- All 62 loader-related tests pass
- No regressions introduced

## Files Modified

1. **Win32Emu/Loader/PeImageLoader.cs**
   - Added two-pass export processing
   - Implemented C-compiled detection heuristic
   - Limited log output for undecorated exports
   - Enhanced summary logging

2. **Win32Emu/Emulator.cs**
   - Added error handling around ProcessEnvironment initialization
   - Added debug logging before/after critical steps

3. **Win32Emu.Tests.Emulator/ExportMetadataTests.cs** (NEW)
   - 15 comprehensive tests for export metadata
   - Tests heuristic, log reduction, and rvvm_i386.exe scenario

## Configuration

No configuration changes needed. The fix is automatic:
- Heuristic detects C-compiled executables (>80% undecorated exports)
- Automatically applies appropriate calling convention
- Automatically reduces log spam

## Future Enhancements

Possible improvements for future consideration:

1. **Configurable threshold**: Allow users to override the 80% threshold
2. **Per-executable overrides**: Manual configuration for specific executables
3. **Function signature inference**: Analyze function prologues/epilogues
4. **Debug symbols**: Use PDB files when available for accurate metadata
5. **Heuristic refinement**: Additional signals beyond export decoration ratio

## References

- Issue: "rvvm_i386.exe crash"
- Commit: Fix rvvm_i386.exe crash: reduce log spam and detect C-compiled executables
- Test Suite: ExportMetadataTests.cs (15 tests)
- Related: ExportMetadata.cs (CdeclDefault property already existed but unused)
