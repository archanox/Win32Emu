# COM Vtable Async/Sync Handler Mismatch Fix

## Problem Statement

The emulator was showing warning messages for DirectDraw COM interface calls:
```
warn: Win32Emu.Emulator[0]
      [COM] Unhandled COM vtable call at 0x0D001140 (method: IDirectDraw::SetCooperativeLevel)
warn: Win32Emu.Emulator[0]
      [COM] Unhandled COM vtable call at 0x0D001150 (method: IDirectDraw::SetDisplayMode)
warn: Win32Emu.Emulator[0]
      [COM] Unhandled COM vtable call at 0x0D001060 (method: IDirectDraw::CreateSurface)
warn: Win32Emu.Emulator[0]
      [COM] Unhandled COM vtable call at 0x0D001050 (method: IDirectDraw::CreatePalette)
```

These warnings were followed by low EIP corruption errors (EIP=0x00000001), causing the emulated program to crash.

## Root Cause Analysis

### Investigation Steps
1. The methods ARE implemented in `DDrawModule.cs` (SetCooperativeLevel, SetDisplayMode, CreateSurface, CreatePalette)
2. The methods ARE registered when DirectDrawCreate/DirectDrawCreateEx is called
3. But `ComVtableDispatcher.TryInvoke` was logging them as "Unhandled"

### The Bug
The issue was a mismatch between handler registration and handler lookup:

**Registration (in DDrawModule.cs)**:
- DirectDraw COM objects are created with `CreateComObjectAsyncOrdered()`
- Handlers are registered in `_vtableAsyncHandlers` dictionary

**Invocation (in ComVtableDispatcher.cs)**:
- Synchronous `TryInvoke()` only checks `_vtableHandlers` dictionary
- NEVER checks `_vtableAsyncHandlers` dictionary
- Returns `false` → "Unhandled COM vtable call" warning
- Emulator continues execution with invalid state → EIP corruption

**The async path was correct**:
- `TryInvokeAsync()` checks both `_vtableAsyncHandlers` AND `_vtableHandlers` as fallback
- But sync invocation path (used by the emulator) did not have this fallback

## Solution

Modified `ComVtableDispatcher.TryInvoke()` to add a fallback path:

```csharp
// Try sync handler first
if (_vtableHandlers.TryGetValue(address, out var handler))
{
    // ... execute sync handler ...
    return true;
}

// NEW: Fall back to async handler if no sync handler exists
// Execute async handler synchronously by blocking on Result
if (_vtableAsyncHandlers.TryGetValue(address, out var asyncHandler))
{
    _logger.LogInformation("[COM] Invoking async vtable method (sync fallback): {MethodName}", methodName);
    _logger.LogWarning("[COM] PERFORMANCE WARNING: Executing async handler {MethodName} synchronously", methodName);
    
    returnValue = asyncHandler(cpu, memory).Result;  // Block on async result
    argBytes = _vtableArgBytes.GetValueOrDefault(address, 0);
    return true;
}

// Still not found - log warning
_logger.LogWarning("[COM] Unhandled COM vtable call at 0x{Address:X8} (method: {MethodName})", address, methodName);
return false;
```

## Testing

Created comprehensive unit tests in `ComVtableAsyncSyncFallbackTests.cs`:

1. **TryInvoke_WithAsyncHandlerRegistered_ShouldInvokeSuccessfully**
   - Registers async COM handlers
   - Invokes through sync `TryInvoke`
   - Verifies handler executes and returns correct value

2. **TryInvoke_WithBothSyncAndAsyncHandlers_ShouldPreferSyncHandler**
   - Verifies sync handlers are still preferred when available
   - Ensures no behavioral change for existing sync code

3. **TryInvoke_WithUnregisteredAddress_ShouldReturnFalse**
   - Verifies unregistered addresses still fail correctly
   - Ensures error path still works

**Test Results**: All 3 tests passing ✓

## Impact

### Before Fix
```
warn: [COM] Unhandled COM vtable call at 0x0D001140 (method: IDirectDraw::SetCooperativeLevel)
warn: [Emulator] EIP=0x00000001 is suspiciously low (<0x00400000)
→ Emulator crashes with EIP corruption
```

### After Fix
```
info: [COM] Invoking async vtable method (sync fallback): IDirectDraw::SetCooperativeLevel
warn: [COM] PERFORMANCE WARNING: Executing async handler IDirectDraw::SetCooperativeLevel synchronously
info: [COM] IDirectDraw::SetCooperativeLevel returned 0x00000000 (argBytes=8)
→ Method executes successfully, returns DD_OK, program continues
```

### Benefits
- DirectDraw methods now execute instead of failing
- No more "Unhandled COM vtable call" warnings for async handlers
- No more EIP corruption from failed COM calls
- Games using DirectDraw can progress past initialization
- Async-first architecture preserved (sync path is just a fallback)

### Performance Considerations
- Sync-over-async execution uses `.Result` which blocks the thread
- On WASM this could potentially cause issues
- However, this is a fallback path - proper async invocation should be preferred
- The performance warning log helps identify places where async path should be used

## Files Modified

1. **Win32Emu/Win32/COM/ComVtableDispatcher.cs**
   - Added async handler fallback in `TryInvoke()` method

2. **Win32Emu.Tests.Emulator/ComVtableAsyncSyncFallbackTests.cs** (new)
   - Comprehensive unit tests for the fix

## Related Documentation

- `UNHANDLED_COM_VTABLE_FIX.md` - Previous fix for missing error handling
- `docs/fixes/COM_VTABLE_FIX_SUMMARY.md` - Original COM vtable implementation
- `docs/implementation/ASYNC_COM_METHODS.md` - Async COM design

## Conclusion

This fix resolves the async/sync handler mismatch that was causing DirectDraw COM methods to appear unimplemented. The solution is minimal, backward-compatible, and includes comprehensive test coverage. Games using DirectDraw should now be able to successfully call SetCooperativeLevel, SetDisplayMode, CreateSurface, and CreatePalette without crashing.
