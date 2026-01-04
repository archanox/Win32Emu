# WASM Cache Filename Fix

**Issue**: Pre-compiled cache not loading for ign_teas on WASM frontend  
**Date**: 2025-12-30  
**Status**: ✅ Fixed

## Problem

The pre-compiled cache for `ign_teas` was not loading when running on the WASM frontend with IcedCpu. The issue was not visible in logs because the cache file had an incorrect name and couldn't be found.

## Root Cause

The cache file was named `ign_tease.wasm-cache.json` (typo: "tease" instead of "teas"), but the EmulatorService correctly constructs the cache filename from the executable name:

```csharp
// EmulatorService.cs line 241
var cacheFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.wasm-cache.json";
```

When loading `IGN_TEAS.EXE`, this produces `IGN_TEAS.wasm-cache.json`, but the actual file was `ign_tease.wasm-cache.json`.

## Solution

### 1. Fixed Cache Filename
- Renamed: `ign_tease.wasm-cache.json` → `IGN_TEAS.wasm-cache.json`
- Generated real cache from actual IGN_TEAS.EXE executable

### 2. Fixed CI Workflow
Updated `.github/workflows/cpu-test-results.yml`:
```yaml
--output Win32Emu.Wasm/wwwroot/cache/IGN_TEAS.wasm-cache.json
```

### 3. Enhanced Logging
Added `[Cache]` prefix and improved visibility in EmulatorService.cs:
```csharp
_logger.LogInformation("Attempting to load pre-compiled cache: {CacheUrl}", cacheUrl);
EmitDebugOutput($"[Cache] Attempting to load pre-compiled cache: {cacheUrl}");
// ...
_logger.LogInformation("Pre-compiled cache loaded successfully: {CacheFileName}", cacheFileName);
EmitDebugOutput($"[Cache] ✓ Pre-compiled cache loaded successfully: {cacheFileName}");
```

### 4. Added Tests
Created `WasmCacheFilenameTests.cs` with 6 tests to verify:
- Cache filename preserves case from executable name
- IGN_TEAS.EXE correctly produces IGN_TEAS.wasm-cache.json
- Different executable names produce correct cache filenames

## How Cache Loading Works

### WASM Frontend Flow
1. User loads executable (e.g., `IGN_TEAS.EXE`)
2. EmulatorService calls `LoadExecutableFromBytesAsync()`
3. If `useCache=true` and `IcedCpu` is active:
   - Constructs cache filename from executable name
   - Fetches cache via HTTP from `wwwroot/cache/{filename}.wasm-cache.json`
   - Calls `IcedCpu.LoadCacheFromJsonAsync()` with JSON content
4. IcedCpu loads pre-analyzed block metadata
5. Execution starts with cached metadata

### Cache File Format
```json
{
  "version": 1,
  "executablePath": "IGN_TEAS.EXE",
  "timestamp": "2025-12-30T06:20:49Z",
  "blocks": [
    {
      "startAddress": 4263888,
      "instructionCount": 6,
      "byteLength": 12,
      "codeHash": "...",
      "endsWithCall": false,
      "endsWithReturn": true
    }
  ]
}
```

## Preventing Similar Issues

### Naming Convention
Cache files **MUST** follow this exact pattern:
```
{ExecutableName}.wasm-cache.json
```

Where `{ExecutableName}` is `Path.GetFileNameWithoutExtension()` of the actual executable, **preserving case**.

### CI Workflow Pattern
When generating cache files in CI:
```bash
dotnet run --project Win32Emu.Tools.WasmCacheGenerator -- \
  EXEs/path/to/EXECUTABLE.EXE \
  --output Win32Emu.Wasm/wwwroot/cache/EXECUTABLE.wasm-cache.json
```

Note: Use the **exact case** of the executable name (usually uppercase for Win32 executables).

### Verification
Use the new tests to verify cache filename logic:
```bash
dotnet test --filter "WasmCacheFilenameTests"
```

## Debug Checklist

If cache is not loading in WASM:

1. **Check log output** - Look for `[Cache]` messages:
   - `[Cache] Attempting to load pre-compiled cache: cache/FILENAME.wasm-cache.json`
   - `[Cache] ✓ Pre-compiled cache loaded successfully` (success)
   - `[Cache] No pre-compiled cache file found` (file missing)

2. **Verify filename** matches executable:
   - If executable is `IGN_TEAS.EXE`, cache must be `IGN_TEAS.wasm-cache.json`
   - Case matters! `ign_teas.wasm-cache.json` ≠ `IGN_TEAS.wasm-cache.json`

3. **Check cache file exists**:
   ```
   Win32Emu.Wasm/wwwroot/cache/FILENAME.wasm-cache.json
   ```

4. **Verify cache is enabled**:
   - Must use `IcedCpu` (not JIT CPU in WASM)
   - `useCache` parameter must be `true`

5. **Check browser console** for HTTP 404 errors

## Related Files
- `Win32Emu.Wasm/Services/EmulatorService.cs` - Cache loading logic
- `Win32Emu/Cpu/Iced/IcedCpu.cs` - `LoadCacheFromJsonAsync()` method
- `Win32Emu.Tools.WasmCacheGenerator/Program.cs` - Cache generator
- `.github/workflows/cpu-test-results.yml` - CI cache generation

## See Also
- [JIT_CACHE_IMPLEMENTATION.md](../implementation/JIT_CACHE_IMPLEMENTATION.md) - Full cache system docs
- [JIT_CPU_WASM_COMPATIBILITY.md](../implementation/JIT_CPU_WASM_COMPATIBILITY.md) - WASM architecture
- [WasmCacheGenerator README](../../Win32Emu.Tools.WasmCacheGenerator/README.md) - Cache generator tool
