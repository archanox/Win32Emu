# Async COM Methods Implementation

## Overview

This document describes the implementation of async COM method handlers in Win32Emu, enabling proper async/await support throughout the COM call chain. This addresses the race condition issues documented in `WASM_RENDERING_RACE_CONDITION_FIX.md` by providing a more robust solution than `.GetAwaiter().GetResult()`.

## Background

### The Problem

The WASM rendering backend race condition fix document identified that using `.GetAwaiter().GetResult()` in WASM mode works but is not the most elegant solution. The issue stated:

> For a more robust solution, implement async COM method handlers

The current implementation uses `.GetAwaiter().GetResult()` in methods like `SetDisplayMode` when calling async backend initialization:

```csharp
// Current implementation (synchronous COM method)
private uint DDraw_SetDisplayMode(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
{
    // ...
    if (PlatformHelpers.IsWasm)
    {
        var success = obj.RenderingBackend.InitializeAsync((int)dwWidth, (int)dwHeight, title).GetAwaiter().GetResult();
        // ...
    }
}
```

### The Solution

With the new async COM infrastructure, methods that need async operations can now use proper async/await:

```csharp
// New implementation (async COM method)
private async Task<uint> DDraw_SetDisplayModeAsync(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
{
    // ...
    if (PlatformHelpers.IsWasm)
    {
        var success = await obj.RenderingBackend.InitializeAsync((int)dwWidth, (int)dwHeight, title);
        // ...
    }
}
```

## Infrastructure Components

### 1. ComAsyncMethodInfo Record

Located in `Win32Emu/Win32/COM/ComAsyncMethodInfo.cs`:

```csharp
public record ComAsyncMethodInfo(
    Func<ICpu, VirtualMemory, Task<uint>> AsyncHandler,
    int ArgBytes = 0  // Argument byte count for stdcall stack cleanup
);
```

This record stores metadata for async COM methods, including the async handler and argument byte count for proper stack cleanup.

### 2. FromAsyncDelegate<T> Static Method

Located in `ComVtableDispatcher.cs`:

```csharp
public static ComAsyncMethodInfo FromAsyncDelegate<TDelegate>(
    Func<ICpu, VirtualMemory, Task<uint>> asyncHandler) 
    where TDelegate : Delegate
{
    var delegateType = typeof(TDelegate);
    
    // Verify the delegate has the correct attribute
    if (!ComDelegateHelper.HasStdCallConvention(delegateType))
    {
        throw new InvalidOperationException($"Delegate type {delegateType.Name} must have [UnmanagedFunctionPointer(CallingConvention.StdCall)] attribute");
    }
    
    // Calculate argument bytes from delegate signature
    var argBytes = ComDelegateHelper.GetArgBytes(delegateType);
    
    return new ComAsyncMethodInfo(asyncHandler, argBytes);
}
```

This method:
- Automatically calculates `argBytes` from the delegate signature
- Validates the delegate has the correct calling convention
- Returns a `ComAsyncMethodInfo` ready for use in vtable creation

### 3. CreateComObjectAsyncOrdered Method

Located in `ComVtableDispatcher.cs`:

```csharp
public uint CreateComObjectAsyncOrdered(
    string interfaceName, 
    List<KeyValuePair<string, ComAsyncMethodInfo>> methods)
{
    return CreateComObjectInternal(
        interfaceName,
        methods,
        info => null,
        info => info.AsyncHandler,
        info => info.ArgBytes,
        isAsync: true);
}
```

This method creates COM objects with async vtable handlers, ensuring methods are in the correct order as required by COM interface specifications.

### 4. TryInvokeAsync Method

Already existed in `ComVtableDispatcher.cs`:

```csharp
public async Task<(bool success, uint returnValue, int argBytes)> TryInvokeAsync(
    uint address, 
    ICpu cpu, 
    VirtualMemory memory,
    CancellationToken cancellationToken = default)
{
    // ... implementation that handles both async and sync handlers
}
```

This method:
- Checks for async handlers first
- Falls back to sync handlers if no async handler exists
- Handles CPU state suspension/resumption across async boundaries
- Supports cancellation tokens

## Usage Examples

### Basic Async COM Method

Here's how to migrate a synchronous COM method to async:

**Before (Synchronous):**

```csharp
// In DirectDrawCreate or similar
var vtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
{
    new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>(
        (cpu, mem) => ComQueryInterface(cpu, mem))),
    new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>(
        (cpu, mem) => ComAddRef(cpu, mem))),
    new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>(
        (cpu, mem) => ComRelease(cpu, mem))),
    new("SetDisplayMode", ComVtableDispatcher.FromDelegate<IDirectDraw.SetDisplayMode>(
        (cpu, mem) => DDraw_SetDisplayMode(cpu, mem, ddrawHandle))),
};

var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDraw", vtableMethods);
```

**After (Asynchronous):**

```csharp
// In DirectDrawCreate or similar
var vtableMethods = new List<KeyValuePair<string, ComAsyncMethodInfo>>
{
    new("QueryInterface", ComVtableDispatcher.FromAsyncDelegate<IDirectDraw.QueryInterface>(
        async (cpu, mem) => await ComQueryInterfaceAsync(cpu, mem))),
    new("AddRef", ComVtableDispatcher.FromAsyncDelegate<IDirectDraw.AddRef>(
        async (cpu, mem) => await ComAddRefAsync(cpu, mem))),
    new("Release", ComVtableDispatcher.FromAsyncDelegate<IDirectDraw.Release>(
        async (cpu, mem) => await ComReleaseAsync(cpu, mem))),
    new("SetDisplayMode", ComVtableDispatcher.FromAsyncDelegate<IDirectDraw.SetDisplayMode>(
        async (cpu, mem) => await DDraw_SetDisplayModeAsync(cpu, mem, ddrawHandle))),
};

var comObjectAddr = _env.ComDispatcher.CreateComObjectAsyncOrdered("IDirectDraw", vtableMethods);
```

### Converting Handler Methods

**Before (Synchronous Handler):**

```csharp
private uint DDraw_SetDisplayMode(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
{
    var args = new StackArgs(cpu, memory);
    var thisPtr = args.UInt32(0);
    var dwWidth = args.UInt32(1);
    var dwHeight = args.UInt32(2);
    var dwBPP = args.UInt32(3);
    
    // ... validation and setup ...
    
    if (PlatformHelpers.IsWasm)
    {
        // Using .GetAwaiter().GetResult() to block on async operation
        var success = obj.RenderingBackend.InitializeAsync(
            (int)dwWidth, (int)dwHeight, title).GetAwaiter().GetResult();
        // ...
    }
    
    return (uint)DDResult.DD_OK;
}
```

**After (Async Handler):**

```csharp
private async Task<uint> DDraw_SetDisplayModeAsync(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
{
    var args = new StackArgs(cpu, memory);
    var thisPtr = args.UInt32(0);
    var dwWidth = args.UInt32(1);
    var dwHeight = args.UInt32(2);
    var dwBPP = args.UInt32(3);
    
    // ... validation and setup ...
    
    if (PlatformHelpers.IsWasm)
    {
        // Using proper async/await
        var success = await obj.RenderingBackend.InitializeAsync(
            (int)dwWidth, (int)dwHeight, title);
        // ...
    }
    
    return (uint)DDResult.DD_OK;
}
```

### Mixed Sync/Async Approach

Not all COM methods need to be async. For methods that don't perform async operations, you can continue using the synchronous approach. The `TryInvokeAsync` method in `ComVtableDispatcher` automatically falls back to synchronous handlers when no async handler is registered.

However, when migrating an interface to async, it's recommended to make all methods async for consistency, even if they just use `await Task.CompletedTask` or `return await Task.FromResult(value)`:

```csharp
// Simple methods that don't need async can still be async for consistency
private async Task<uint> ComAddRefAsync(ICpu cpu, VirtualMemory memory)
{
    _refCount++;
    _logger.LogDebug("[COM] AddRef: refCount={RefCount}", _refCount);
    return await Task.FromResult((uint)_refCount);
}

// Or use synchronous code with async signature
private async Task<uint> ComQueryInterfaceAsync(ICpu cpu, VirtualMemory memory)
{
    var args = new StackArgs(cpu, memory);
    var thisPtr = args.UInt32(0);
    var riid = args.UInt32(1);
    var ppvObject = args.UInt32(2);
    
    // Synchronous logic
    _logger.LogDebug("[COM] QueryInterface not implemented");
    memory.Write32(ppvObject, 0);
    
    return await Task.FromResult((uint)0x80004002); // E_NOINTERFACE
}
```

## When to Use Async COM Methods

Use async COM methods when:

1. **WASM Backend Operations**: Methods that initialize or interact with WASM rendering backends (e.g., `SetDisplayMode`, `CreateSurface`)
2. **I/O Operations**: Methods that perform file I/O, network operations, or other I/O-bound work
3. **Long-Running Operations**: Methods that take significant time and would benefit from yielding control
4. **Future Extensibility**: When you anticipate needing async in the future for a particular interface

Continue using synchronous COM methods when:
1. **Simple Operations**: Methods like `AddRef`, `Release`, getter methods
2. **CPU-Bound Logic**: Pure computation without I/O
3. **Already Working Well**: Methods using `.GetAwaiter().GetResult()` that work correctly

## Benefits

### 1. Better WASM Support

Proper async/await allows the browser event loop to continue processing during async operations, preventing UI freezes and improving responsiveness.

### 2. No Blocking

Eliminates the need for `.GetAwaiter().GetResult()` which can cause issues in some async contexts.

### 3. Better Error Handling

Async/await provides better exception propagation and stack traces compared to synchronous blocking.

### 4. Cancellation Support

The infrastructure supports `CancellationToken` for graceful cancellation of long-running operations.

### 5. Future-Proof

Provides a foundation for more complex async scenarios as the emulator evolves.

## Migration Strategy

### Phase 1: Infrastructure ✅
- [x] Add `FromAsyncDelegate<T>` method
- [x] Add `CreateComObjectAsyncOrdered` method
- [x] Add unit tests for async infrastructure

### Phase 2: High-Priority Interfaces (Optional)
- [ ] IDirectDraw (especially `SetDisplayMode`)
- [ ] IDirectDrawSurface (especially `Lock`/`Unlock` if needed)
- [ ] Other interfaces with async backend operations

### Phase 3: Full Migration (Future)
- [ ] Migrate all remaining COM interfaces
- [ ] Update documentation and examples
- [ ] Performance testing and optimization

## Testing

Unit tests are provided in `Win32Emu.Tests.Emulator/ComDelegateHelperTests.cs`:

- `FromAsyncDelegate_CreatesComAsyncMethodInfoWithCorrectArgBytes`: Verifies async delegate creation
- `FromAsyncDelegate_ThrowsException_ForNonStdCallDelegate`: Validates calling convention
- `FromAsyncDelegate_AndFromDelegate_ProduceSameArgBytes`: Ensures consistency between sync and async

Run tests:
```bash
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~ComDelegateHelperTests"
```

## Related Documentation

- `docs/fixes/WASM_RENDERING_RACE_CONDITION_FIX.md` - Original issue that motivated this implementation
- `docs/implementation/ASYNC_CALLBACK_MIGRATION.md` - Related async patterns in the codebase
- `docs/implementation/COM_VTABLE_COMPARISON.md` - COM vtable implementation details
- `Win32Emu/Win32/COM/ComVtableDispatcher.cs` - Main implementation
- `Win32Emu/Win32/COM/ComAsyncMethodInfo.cs` - Async method metadata

## Performance Considerations

1. **Async Overhead**: Async/await has minimal overhead (typically negligible compared to emulation costs)
2. **State Machine**: The C# compiler generates a state machine for async methods, adding a small amount of memory overhead
3. **CPU State Management**: The infrastructure properly handles CPU state suspension/resumption across async boundaries
4. **Cancellation**: When using cancellation tokens, check frequently enough but not too often to balance responsiveness and performance

## Limitations and Caveats

1. **All or Nothing**: When migrating an interface, all methods in the vtable should use the same approach (sync or async) for consistency
2. **Stack Args**: The `StackArgs` helper works the same for both sync and async methods
3. **Register Preservation**: Both sync and async paths properly preserve callee-saved registers (EBX, ESI, EDI, EBP)
4. **Emulator Integration**: The emulator's INT3 handler needs to support async invocation (already implemented via `TryInvokeAsync`)

## Conclusion

The async COM method infrastructure provides a robust foundation for handling asynchronous operations in COM interfaces, particularly for WASM scenarios. The infrastructure is complete and tested, making it easy to migrate existing COM interfaces when needed.

For most existing code, the current synchronous approach with `.GetAwaiter().GetResult()` works fine. Async migration should be done on a case-by-case basis when:
- WASM responsiveness needs improvement
- Long-running operations cause issues
- Future features require async capabilities

The infrastructure is ready for use, and migration can proceed incrementally as needed.
