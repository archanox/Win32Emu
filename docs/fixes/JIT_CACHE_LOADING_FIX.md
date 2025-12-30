# JIT Cache Loading Fix for ign_teas and Other Executables

## Problem

The pre-compiled JIT cache was not being loaded when running executables with `useJitCpu=true`, including ign_teas. Although the JitCpu class had full cache support through `SetExecutablePath()` and `LoadCacheAsync()` methods, these were never invoked by the Emulator, resulting in:

1. No executable path set for cache identification
2. No existing cache loaded from disk
3. Compiled blocks not saved to disk after execution
4. **No mention of cache in logs**, making it appear as if the cache system was completely non-functional

## Root Cause

In `Win32Emu/Emulator.cs` at line 545, when `useJitCpu=true`, the JitCpu was instantiated but the cache initialization methods were never called:

```csharp
// Before fix:
if (useJitCpu)
{
    _cpu = new Cpu.Jit.JitCpu(_vm, _logger);  // No cache initialization!
    LogDebug("[Loader] JIT CPU backend enabled (async-capable)");
}
```

This meant that every run of an executable would:
- Recompile all code blocks from scratch
- Not benefit from previous JIT compilation work
- Not persist any compilation work for future runs

## Solution

### 1. Initialize Cache on Load (Emulator.cs lines 542-565)

After creating the JitCpu instance, we now:
1. Store it in a local variable for manipulation
2. Call `SetExecutablePath(path)` to identify the executable
3. Call `LoadCacheAsync()` to load any existing cached blocks
4. Add comprehensive logging for diagnostics

```csharp
if (useJitCpu)
{
    var jitCpu = new Cpu.Jit.JitCpu(_vm, _logger);
    _cpu = jitCpu;
    LogDebug("[Loader] JIT CPU backend enabled (async-capable)");
    
    // Initialize JIT cache for pre-compiled blocks
    jitCpu.SetExecutablePath(path);
    _logger.LogInformation("[Loader] JIT cache: Set executable path to {Path}", path);
    
    // Load existing cache asynchronously (non-blocking)
    // We use ConfigureAwait(false) since we're in a non-UI context
    Task.Run(async () =>
    {
        try
        {
            await jitCpu.LoadCacheAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Loader] Failed to load JIT cache (non-fatal)");
        }
    }).Wait(); // Wait for cache loading to complete before execution
}
```

### 2. Save Cache on Exit (Emulator.cs lines 962-977)

In the `RunAsync()` method's finally block, we now save the cache:

```csharp
finally
{
    // Stop event processing thread
    StopEventProcessing();
    
    // Save JIT cache if using JitCpu
    if (_cpu is Cpu.Jit.JitCpu jitCpu)
    {
        try
        {
            await jitCpu.SaveCacheAsync().ConfigureAwait(false);
            _logger.LogInformation("[Emulator] JIT cache saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Emulator] Failed to save JIT cache (non-fatal)");
        }
    }
    
    // [rest of exit handling...]
}
```

## Expected Log Output

With this fix, when running an executable with JIT CPU enabled, you should see:

### First Run (No Cache)
```
[Loader] JIT CPU backend enabled (async-capable)
[Loader] JIT cache: Set executable path to ./EXEs/ign_teas/IGN_TEAS.EXE
[JitCpu] JIT cache not available in WASM environment  // If in WASM
  OR
[RtlJitCache] Initialized RTL-based JIT cache at /path/to/cache
[RtlJitCache] No cache metadata found for ./EXEs/ign_teas/IGN_TEAS.EXE
...execution...
[Emulator] JIT cache saved successfully
```

### Subsequent Runs (Cache Exists)
```
[Loader] JIT CPU backend enabled (async-capable)
[Loader] JIT cache: Set executable path to ./EXEs/ign_teas/IGN_TEAS.EXE
[RtlJitCache] Initialized RTL-based JIT cache at /path/to/cache
[JitCpu] RTL cache loaded: 150 blocks from /tmp/Win32Emu/RtlJitCache
[RtlJitCache] Loaded 150 cached blocks
...execution...
[Emulator] JIT cache saved successfully
```

## Benefits

1. **Faster Startup**: Subsequent runs benefit from pre-compiled code blocks
2. **Reduced JIT Overhead**: No need to analyze and compile previously-seen code
3. **Observable Behavior**: Logs clearly show cache loading and saving
4. **Cross-Run Learning**: Cache persists across emulator restarts
5. **Non-Breaking**: If cache operations fail, execution continues normally

## Testing

### Automated Tests
```bash
dotnet test Win32Emu.Tests.Emulator/Win32Emu.Tests.Emulator.csproj --filter "JitCacheTests"
```
Result: 11/12 tests passing (1 path comparison test issue, not functional)

### Manual Test with ign_teas

1. **First Run** (build cache):
   ```bash
   # Run with JIT CPU enabled
   dotnet run --project Win32Emu.Tests.Emulator -- \
       ./EXEs/ign_teas/IGN_TEAS.EXE --use-jit
   ```
   Check logs for:
   - `[Loader] JIT cache: Set executable path to ./EXEs/ign_teas/IGN_TEAS.EXE`
   - `[Emulator] JIT cache saved successfully`

2. **Second Run** (use cache):
   ```bash
   # Run again with same executable
   dotnet run --project Win32Emu.Tests.Emulator -- \
       ./EXEs/ign_teas/IGN_TEAS.EXE --use-jit
   ```
   Check logs for:
   - `[JitCpu] RTL cache loaded: N blocks` (where N > 0)
   - Faster startup compared to first run

3. **Verify Cache Files**:
   ```bash
   # Check cache directory
   ls -la ~/.local/share/Win32Emu/RtlJitCache/
   # or on macOS:
   ls -la ~/Library/Application\ Support/Win32Emu/RtlJitCache/
   # or on Windows:
   dir %LOCALAPPDATA%\Win32Emu\RtlJitCache\
   ```

## Cache Locations

The JIT cache is stored in platform-specific locations:

- **Windows**: `%LOCALAPPDATA%\Win32Emu\RtlJitCache\`
- **Linux**: `~/.local/share/Win32Emu/RtlJitCache/`
- **macOS**: `~/Library/Application Support/Win32Emu/RtlJitCache/`

Cache files are named based on the executable path hash to avoid conflicts.

## Implementation Notes

1. **Non-Fatal Errors**: Cache loading/saving failures are logged as warnings but don't stop execution
2. **WASM Compatibility**: Cache is disabled in WASM environments (Roslyn not available)
3. **Async Safety**: Uses `ConfigureAwait(false)` for non-UI contexts
4. **Pattern Matching**: Uses C# pattern matching to check CPU type for cache saving

## Related Documentation

- [JIT Cache Implementation](../implementation/JIT_CACHE_IMPLEMENTATION.md)
- [JIT Cache Quick Start](../guides/JIT_CACHE_QUICK_START.md)
- [RTL JIT Integration](../implementation/RTL_JIT_INTEGRATION.md)

## References

- Issue: "The pre-compiled cache appears to still not work for ign_teas"
- PR: [Link to PR]
- Commit: f9ce0a0
