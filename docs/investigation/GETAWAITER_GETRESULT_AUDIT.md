# GetAwaiter().GetResult() Audit and Remediation Plan

## Overview
This document provides a comprehensive analysis of all `GetAwaiter().GetResult()` usages in the Win32Emu codebase and recommendations for addressing them.

## Summary
- **Total occurrences found**: 53 in C# code files
- **Locations**: DDrawModule, User32Module, DSoundModule, DInputModule, Emulator.cs, ProcessEnvironment.cs, and others
- **Categories**: 4 distinct patterns identified

## Analysis by Category

### Category 1: Synchronous Wrapper Methods (Safe for Desktop, Not Called on WASM)
**Status**: Generally safe - these are designed for non-WASM platforms only

**Pattern**:
```csharp
private uint SyncMethod(params) 
{
    if (PlatformHelpers.IsWasm)
    {
        _logger.LogError("Should use async path");
        return ERROR;
    }
    return AsyncMethod(params).GetAwaiter().GetResult();
}

private async Task<uint> AsyncMethod(params)
{
    var result = await SomethingAsync();
    return result;
}
```

**Examples**:
- `DDrawModule.cs:855` - `DDraw_CreateSurface` (has check, calls `DDraw_CreateSurfaceAsync`)
- `DDrawModule.cs:4006` - `InitializeRenderingBackendWithDimensions` (has check, calls async version)
- `User32Module.cs:2230` - `GetMessageA` wrapper (calls `GetMessageAsync`)
- `User32Module.cs:2386` - `TranslateMessageA` wrapper (calls `TranslateMessageAsync`)
- `User32Module.cs:2435` - `DispatchMessageA` wrapper (calls `DispatchMessageAsync`)
- `User32Module.cs:2994` - `SendMessageA` wrapper (calls `SendMessageAsync`)
- `User32Module.cs:3173` - `UpdateWindow` wrapper (calls `UpdateWindowAsync`)
- `User32Module.cs:3536` - `PeekMessageA` wrapper (calls `PeekMessageAsync`)
- `User32Module.cs:3608` - `WaitMessage` wrapper (calls `WaitMessageAsync`)
- `User32Module.cs:3666` - `DialogBoxParamA` wrapper (calls `DialogBoxParamAsync`)
- `User32Module.cs:5764` - `EnumWindows` wrapper (calls `EnumWindowsAsync`)
- `Shell32Module.cs:160` - `SHBrowseForFolderA` wrapper (calls `SHBrowseForFolderAAsync`)
- `DSoundModule.cs:192` - `DirectSoundEnumerateA` wrapper (calls async version)

**Recommendation**: ✅ No action needed - these are properly designed with WASM checks and async alternatives

### Category 2: WASM-Specific Code in Synchronous Methods (Problematic)
**Status**: ❌ MUST FIX - WASM code should not be in synchronous methods

**Problem**: The synchronous `DDraw_SetDisplayMode` method has a WASM-specific code path (lines 3343-3368) that uses `GetAwaiter().GetResult()`. This is wrong because:
1. Synchronous methods should NEVER be called on WASM (vtable uses async version)
2. The async version `DDraw_SetDisplayModeAsync` already properly uses `await` (line 3503)
3. Having WASM code in sync method is confusing and creates maintenance burden

**Location**:
- `DDrawModule.cs:3353` - Inside `if (PlatformHelpers.IsWasm)` block in `DDraw_SetDisplayMode`
- `DDrawModule.cs:3372` - Inside `else` block (non-WASM path) in same method

**Fix Required**:
```csharp
// CURRENT (WRONG):
private uint DDraw_SetDisplayMode(...)
{
    // ... validation code ...
    
    if (PlatformHelpers.IsWasm)
    {
        // WASM-specific initialization code - SHOULD NOT BE HERE!
        var success = obj.RenderingBackend.InitializeAsync(...).GetAwaiter().GetResult();
        // ... more WASM code ...
    }
    else
    {
        var success = obj.RenderingBackend.InitializeAsync(...).GetAwaiter().GetResult();
        // ... non-WASM code ...
    }
}

// SHOULD BE:
private uint DDraw_SetDisplayMode(...)
{
    if (PlatformHelpers.IsWasm)
    {
        _logger.LogError("[DDraw] DDraw_SetDisplayMode called on WASM - should use async path");
        return (uint)DDResult.DDERR_GENERIC;
    }
    
    // ... validation code ...
    
    // Only non-WASM initialization code
    var success = obj.RenderingBackend.InitializeAsync(...).GetAwaiter().GetResult();
    // ... handle result ...
}
```

**Verification**: Check vtable registration confirms async version is used:
```csharp
// Line 210 in DDrawModule.cs:
new("SetDisplayMode", ComVtableDispatcher.FromAsyncDelegate<IDirectDraw.SetDisplayMode>(
    async (cpu, mem) => await DDraw_SetDisplayModeAsync(cpu, mem, ddrawHandle))),
```

### Category 3: Backend Initialization Without Async Versions
**Status**: ⚠️ Should be migrated to async pattern

**Pattern** - Fire-and-forget on WASM, blocking on desktop:
```csharp
if (PlatformHelpers.IsWasm)
{
    _ = _env.AudioBackend.InitializeAsync();  // Fire and forget
}
else
{
    _env.AudioBackend.InitializeAsync().GetAwaiter().GetResult();  // Blocking
}
```

**Locations**:
- `DInputModule.cs:175` - DirectInputCreate (no async version exists)
- `DInputModule.cs:273` - DirectInputCreate8 (no async version exists)  
- `DSoundModule.cs:144` - DirectSoundCreate (no async version exists)
- `DSoundModule.cs:722` - DirectSoundCreate in SetCooperativeLevel (no async version)
- `Msacm32Module.cs:170` - acmDriverOpen (no async version)
- `WinMMModule.cs:1041` - waveOutOpen (no async version)
- `Glide2xModule.cs:1549` - grSstWinOpen (no async version)

**Recommendation**: Create async versions of these methods following the DDraw pattern:
1. Create `DirectSoundCreateAsync` that properly awaits backend initialization
2. Register async version in vtable for WASM
3. Keep synchronous version for desktop with WASM check

**Example Template**:
```csharp
// Synchronous version (desktop only)
private uint DirectSoundCreate(uint lpGuid, uint lplpDs, uint pUnkOuter)
{
    if (PlatformHelpers.IsWasm)
    {
        _logger.LogError("[DSound] DirectSoundCreate called on WASM - should use async path");
        return E_FAIL;
    }
    return DirectSoundCreateAsync(lpGuid, lplpDs, pUnkOuter).GetAwaiter().GetResult();
}

// Async version (WASM and future)
private async Task<uint> DirectSoundCreateAsync(uint lpGuid, uint lplpDs, uint pUnkOuter)
{
    // ... setup code ...
    
    if (PlatformHelpers.IsWasm)
    {
        await _env.AudioBackend.InitializeAsync();
    }
    else
    {
        await _env.AudioBackend.InitializeAsync();
    }
    
    // ... rest of implementation ...
}
```

### Category 4: Core Emulator Execution Loop
**Status**: ✅ Leave as-is - designed for synchronous desktop execution

**Locations**:
- `Emulator.cs:694` - `ExecuteTlsCallbacksAsync().GetAwaiter().GetResult()`
- `Emulator.cs:1000` - `RunAsync().GetAwaiter().GetResult()`  
- `Emulator.cs:1548` - `RunNormalAsync().GetAwaiter().GetResult()`
- `Emulator.cs:1738` - `HandleDosInterruptAsync().GetAwaiter().GetResult()`
- `Emulator.cs:2430` - `HandleSyscallAsync().GetAwaiter().GetResult()`

**Reason**: These are part of the core emulator execution loop which runs synchronously on desktop. The async versions exist for WASM, but desktop deliberately uses blocking calls for performance and simplicity.

**Recommendation**: ✅ No action needed

### Category 5: ProcessEnvironment Message Handling
**Status**: ⚠️ Review needed

**Locations**:
- `ProcessEnvironment.cs:2257` - `SendMessage` implementation
- `ProcessEnvironment.cs:2444` - `GetMessage` implementation

**Context**: These have platform checks but still use `GetAwaiter().GetResult()` in non-WASM paths.

**Recommendation**: Verify these have async alternatives for WASM or document why blocking is acceptable.

## Implementation Priority

### High Priority (Must Fix)
1. **Remove WASM code from `DDraw_SetDisplayMode` synchronous method** (DDrawModule.cs:3343-3368)
   - Impact: Eliminates confusing dead code
   - Risk: Low - WASM already uses async version
   - Effort: 1-2 hours

### Medium Priority (Should Fix)
2. **Create async versions for backend initialization methods**
   - DirectSound, DirectInput, WinMM, Glide2x modules
   - Impact: Proper async/await throughout WASM
   - Risk: Medium - requires testing
   - Effort: 4-8 hours

### Low Priority (Optional)
3. **Document exceptions** for core emulator and sync wrappers
   - Add code comments explaining why `GetAwaiter().GetResult()` is acceptable
   - Impact: Better code maintainability
   - Risk: None
   - Effort: 1 hour

## Testing Strategy

After making changes:
1. **Desktop regression testing**: Ensure all `GetAwaiter().GetResult()` paths still work
2. **WASM testing**: Verify async paths are used and no deadlocks occur
3. **Module-specific tests**: Test DirectSound, DirectInput after creating async versions

## References

- Async COM Methods: `docs/implementation/ASYNC_COM_METHODS.md`
- WASM Rendering Fix: `docs/fixes/WASM_DDRAW_CANVAS_RENDERING_FIX.md`
- Async Threading: `docs/implementation/ASYNC_THREADING_IMPLEMENTATION.md`
