# Async Migration Plan - Removing GetAwaiter().GetResult()

## Status: IN PROGRESS

This document tracks the complete migration from synchronous wrappers with `GetAwaiter().GetResult()` to fully async architecture.

## Completed Changes

### DDrawModule.cs (Commit 23b5b75)
- ✅ Removed `DDraw_CreateSurface` synchronous wrapper (12 lines)
- ✅ Removed `DDraw_SetDisplayMode` synchronous wrapper with problematic WASM code (148 lines)
- ✅ Removed `InitializeRenderingBackendWithDimensions` synchronous wrapper (12 lines)
- **Result**: 172 lines deleted, COM vtables already use async versions

## Architecture Analysis

### Current Dual-Path System
The codebase uses a dual-path system for WASM vs non-WASM:

1. **Non-WASM (Desktop)**:
   - Methods with `[DllModuleExport]` attribute are called directly
   - These are synchronous and use `GetAwaiter().GetResult()` internally
   
2. **WASM**:
   - `TryInvokeAsync` routes to async methods, bypassing synchronous exports
   - Example from User32Module.cs lines 1732-1748:
     ```csharp
     switch (export.ToUpperInvariant())
     {
         case "GETMESSAGEA":
             return (true, await GetMessageAsync(...));
         case "TRANSLATEMESSAGE":
             return (true, await TranslateMessageAsync(...));
     }
     ```

### Implications for Full Async Migration

To fully remove `GetAwaiter().GetResult()`, we need to:
1. Make all `[DllModuleExport]` methods async
2. Update the DLL export invoker to handle async methods
3. OR remove `[DllModuleExport]` entirely and use only `TryInvokeAsync` for all platforms

## Remaining Work by Module

### User32Module.cs (~10 occurrences)
**Exports using sync wrappers:**
- Line 2230: `GetMessageA` → calls `GetMessageAsync`
- Line 2386: `TranslateMessageA` → calls `TranslateMessageAsync`  
- Line 2435: `DispatchMessageA` → calls `DispatchMessageAsync`
- Line 2994: `SendMessageA` → calls `SendMessageAsync`
- Line 3173: `UpdateWindow` → calls `UpdateWindowAsync`
- Line 3536: `PeekMessageA` → calls `PeekMessageAsync`
- Line 3608: `WaitMessage` → calls `WaitMessageAsync`
- Line 3666: `DialogBoxParamA` → calls `DialogBoxParamAsync`
- Line 5764: `EnumWindows` → calls `EnumWindowsAsync`

**Status**: All have async versions, all routed via `TryInvokeAsync` on WASM

**Options:**
1. **Simple**: Convert `[DllModuleExport]` methods to async (requires DLL export system changes)
2. **Complex**: Remove `[DllModuleExport]` and use `TryInvokeAsync` for all platforms

### DSoundModule.cs (5 occurrences)  
**Line 144**: `DirectSoundCreate` - Fire-and-forget on WASM, blocking on desktop
**Line 192**: `DirectSoundEnumerateA` wrapper → calls `DirectSoundEnumerateAAsync`
**Line 722**: `DSound_SetCooperativeLevel` - Fire-and-forget on WASM, blocking on desktop

**Required changes:**
1. Convert `DirectSoundCreate` to async (`DirectSoundCreateAsync`)
2. Convert `DSound_SetCooperativeLevel` to async
3. Update COM vtable registrations to use async versions
4. Remove `DirectSoundEnumerateA` wrapper (has export attribute)

### DInputModule.cs (2 occurrences)
**Line 175**: `DirectInputCreate` - Fire-and-forget on WASM, blocking on desktop
**Line 273**: `DirectInputCreate8` - Fire-and-forget on WASM, blocking on desktop

**Required changes:**
1. Convert initialization code to use proper `await`
2. Make `DirectInputCreate` and `DirectInputCreate8` async if they have `[DllModuleExport]`

### Shell32Module.cs (1 occurrence)
**Line 160**: `SHBrowseForFolderA` wrapper → calls `SHBrowseForFolderAAsync`

**Status**: Has `[DllModuleExport]`, routed via `TryInvokeAsync` on WASM

**Options:**
1. Convert to async with export system changes
2. Remove wrapper if export system supports async

### WinMMModule.cs (1 occurrence)
**Line 1041**: `waveOutOpen` - Fire-and-forget on WASM, blocking on desktop

**Required changes:**
1. Convert to async with proper `await`

### Msacm32Module.cs (1 occurrence)
**Line 170**: `acmDriverOpen` - Fire-and-forget on WASM, blocking on desktop

**Required changes:**
1. Convert to async with proper `await`

### Glide2xModule.cs (1 occurrence)
**Line 1549**: `grSstWinOpen` - Uses `GetAwaiter().GetResult()`

**Required changes:**
1. Convert to async with proper `await`

### ProcessEnvironment.cs (2 occurrences)
**Line 2257**: `SendMessage` implementation
**Line 2444**: `GetMessage` implementation

**Status**: Need investigation - these are internal to ProcessEnvironment

### Emulator.cs (5 occurrences)
**Status**: Core execution loop, intentionally synchronous for desktop

**Decision**: LEAVE AS-IS (documented in audit)

## Recommended Implementation Strategy

### Phase 1: Low-Hanging Fruit (COMPLETED)
- ✅ Remove dead synchronous wrappers where async versions exist and are already registered
- ✅ DDrawModule complete

### Phase 2: Backend Initialization (IN PROGRESS)
Convert fire-and-forget patterns to proper async/await:
1. DSoundModule - DirectSoundCreate, SetCooperativeLevel
2. DInputModule - DirectInputCreate, DirectInputCreate8
3. WinMMModule - waveOutOpen
4. Msacm32Module - acmDriverOpen
5. Glide2xModule - grSstWinOpen

**Pattern to replace:**
```csharp
// OLD (fire-and-forget on WASM, blocking on desktop):
if (PlatformHelpers.IsWasm)
{
    _ = _env.AudioBackend.InitializeAsync()
        .ContinueWith(t => { /* log result */ });
}
else
{
    _env.AudioBackend.InitializeAsync().GetAwaiter().GetResult();
}

// NEW (async everywhere):
var success = await _env.AudioBackend.InitializeAsync();
if (success)
{
    _logger.LogInformation("Backend initialized successfully");
}
else
{
    _logger.LogWarning("Backend initialization failed");
}
```

### Phase 3: DLL Export System (COMPLEX)
Two options:

**Option A: Make DLL Export System Async-Aware**
- Update DLL invoker to detect async methods  
- Call async methods with proper await
- Maintain `[DllModuleExport]` attributes on async methods

**Option B: Remove DLL Exports, Use TryInvokeAsync Only**
- Remove all `[DllModuleExport]` attributes
- Ensure all modules implement `TryInvokeAsync`
- Route all platforms through `TryInvokeAsync`

**Recommendation**: Option A is less invasive

### Phase 4: ProcessEnvironment (COMPLEX)
- Investigate `SendMessage` and `GetMessage` in ProcessEnvironment.cs
- Determine if they can/should be async
- May require broader architectural changes

## Testing Strategy

After each phase:
1. **Build verification**: Ensure project compiles
2. **Desktop testing**: Run test suite to verify no regressions
3. **WASM testing**: Test in browser with sample executables
4. **Integration testing**: Run dd_image.exe and other DirectDraw samples

## Risks and Mitigation

### Risk: Breaking Desktop Platform
- **Mitigation**: Comprehensive testing after each change
- **Fallback**: Git history allows reverting individual changes

### Risk: Performance Impact
- **Mitigation**: Async overhead is minimal in .NET, especially for I/O-bound operations
- **Measurement**: Benchmark before/after if concerns arise

### Risk: Complex Refactoring
- **Mitigation**: Incremental approach, one module at a time
- **Documentation**: This plan tracks all changes

## Timeline Estimate

- **Phase 1**: COMPLETE (1 hour actual)
- **Phase 2**: 2-4 hours (convert 6 modules' backend initialization)
- **Phase 3**: 4-8 hours (DLL export system changes)
- **Phase 4**: 2-4 hours (ProcessEnvironment investigation/changes)

**Total**: 8-17 hours for complete migration

## Decision Points

1. **Phase 3 approach**: Option A (async-aware exports) vs Option B (TryInvokeAsync only)
   - Awaiting user decision
   
2. **ProcessEnvironment**: Keep synchronous vs convert to async
   - Needs architectural review

3. **Emulator.cs core loop**: Confirmed to leave as-is (synchronous by design)

## References

- Initial audit: `docs/investigation/GETAWAITER_GETRESULT_AUDIT.md`
- Commit 23b5b75: DDrawModule sync wrapper removal
- User request: Comment #3691993200 - "go ahead and do both, migrate everything to be async, clean up synchronous wrappers"
