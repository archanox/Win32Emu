# MSVCRT Nested Syscall Fix

## Problem

When running `simple_ddraw.exe` on the WASM frontend, the `_initterm` function in MSVCRT.DLL was aborting when callbacks tried to call other Win32 API functions (nested syscalls). This prevented the executable from initializing properly.

### Error Symptoms

```
[WRN] [Emulator] [msvcrt] _initterm: Callback attempted nested syscall at step 31 (EIP=0x0E000002) - aborting callback execution
[WRN] [Emulator] [msvcrt] _initterm: Initializer #1 at 0x00401010 failed to execute
[INF] [Emulator] [msvcrt] _initterm: Executed 0/1 initializers successfully
```

After this, the emulator would get stuck in an infinite loop at JIT-compiled addresses.

## Root Cause

The `_initterm` function executes initialization callbacks from the executable. These callbacks often need to call other Win32 API functions (like `GetModuleHandleA`, `LoadLibraryA`, etc.). However, the `ExecuteCallback` method in `MsvcrtModule` was detecting these nested syscalls (INT 0x80 instructions) and aborting execution, treating them as unsupported operations.

The comment in the original code even acknowledged this limitation:
```csharp
// Nested syscalls from within callbacks are not supported here
// Proper handling would require recursive syscall dispatching similar to User32Module.HandleComAndImportCalls
```

## Solution

The fix implements proper nested syscall handling in `MsvcrtModule` by following the same pattern used in `User32Module`:

### 1. Add Dispatcher and Loaded Image Support

```csharp
// Dispatcher for handling nested syscalls in callbacks
private Win32Dispatcher? _dispatcher;

// Loaded image for import validation
private LoadedImage? _image;

public void SetDispatcher(Win32Dispatcher dispatcher)
{
    _dispatcher = dispatcher;
}

public void SetLoadedImage(LoadedImage image)
{
    _image = image;
}
```

### 2. Implement Nested Syscall Handler

Added `HandleNestedSyscalls` method that:
- Detects import calls (calls to addresses in the import address table)
- Dispatches them through the `Win32Dispatcher`
- Handles both implemented and unimplemented imports gracefully
- Validates return addresses to ensure proper execution flow
- Saves and restores callee-saved registers (EBX, ESI, EDI, EBP)

### 3. Update Callback Execution

Modified `ExecuteCallback` to call `HandleNestedSyscalls` instead of aborting:
```csharp
// Handle nested syscalls (import calls) from within callbacks
// This allows callbacks to call other Win32 API functions
if (HandleNestedSyscalls(step, _cpu, _env.Memory, logContext, out var stepDesc, out var shouldBreak))
{
    if (shouldBreak)
    {
        _logger.LogError("[msvcrt] {LogContext}: Nested syscall handler indicated execution should stop", logContext);
        return false;
    }
    // Successfully handled nested syscall, continue to next instruction
}
else if (step.IsSyscall)
{
    // Syscall was not handled (dispatcher or image not available)
    // This maintains backward compatibility with tests that don't set dispatcher
    _logger.LogWarning("[msvcrt] {LogContext}: Callback attempted nested syscall at step {Steps} (EIP=0x{Eip:X8}) but dispatcher not available - aborting callback execution", logContext, steps, _cpu.GetEip());
    return false;
}
```

### 4. Update Emulator Initialization

Modified `Emulator.cs` to set the dispatcher and loaded image on `MsvcrtModule`:
```csharp
var msvcrtModule = new MsvcrtModule(_env, _image.BaseAddress, peLoader, _logger);
msvcrtModule.SetDispatcher(_dispatcher);
msvcrtModule.SetLoadedImage(_image);
_dispatcher.RegisterModule(msvcrtModule);
```

## Benefits

1. **Allows Full Initialization**: Executables can now properly initialize using `_initterm` callbacks that call Win32 APIs
2. **Backward Compatible**: Tests that don't set a dispatcher continue to work with a warning
3. **Consistent Architecture**: Uses the same pattern as `User32Module` for handling nested calls
4. **Proper Error Handling**: Validates return addresses and handles both implemented and unimplemented imports

## Testing

To verify this fix works with `simple_ddraw.exe`:

1. Build the WASM frontend
2. Load `simple_ddraw.exe`
3. Verify that `_initterm` callbacks execute successfully
4. Check that the application initializes without getting stuck in an infinite loop

Expected log output should show:
```
[DBG] [Emulator] [msvcrt] _initterm: Calling initializer #1 at 0x00401010
[DBG] [Emulator] [msvcrt] _initterm: Nested import call KERNEL32.DLL!GetModuleHandleA at 0x...
[DBG] [Emulator] [msvcrt] _initterm: Nested import KERNEL32.DLL!GetModuleHandleA returned 0x...
[DBG] [Emulator] [msvcrt] _initterm: Callback returned successfully after N steps
[DBG] [Emulator] [msvcrt] _initterm: Initializer #1 completed successfully
[INF] [Emulator] [msvcrt] _initterm: Executed 1/1 initializers successfully
```

## Related Files

- `Win32Emu/Win32/Modules/MsvcrtModule.cs` - Main implementation
- `Win32Emu/Emulator.cs` - Initialization code
- `Win32Emu/Win32/Modules/User32Module.cs` - Reference implementation for `HandleComAndImportCalls`

## Date

2026-01-05
