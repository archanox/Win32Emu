# TLS Callbacks Implementation

## Overview

This document describes the implementation of Thread Local Storage (TLS) callbacks in Win32Emu. TLS callbacks are special functions in PE executables that are executed **before** the main entry point and on thread attach/detach events.

## What are TLS Callbacks?

TLS callbacks are initialization functions specified in a PE file's TLS directory. They are called by the Windows loader in the following scenarios:

1. **DLL_PROCESS_ATTACH (1)**: When the process starts, **BEFORE** the main entry point
2. **DLL_THREAD_ATTACH (2)**: When a new thread is created
3. **DLL_THREAD_DETACH (3)**: When a thread terminates
4. **DLL_PROCESS_DETACH (0)**: When the process terminates

For emulation purposes, we primarily focus on **DLL_PROCESS_ATTACH** to ensure any TLS initialization code runs before the application's main entry point.

## Implementation Details

### 1. LoadedImage Extension

The `LoadedImage` record has been extended to include TLS callback addresses:

```csharp
public record LoadedImage(
    // ... existing fields ...
    uint[] TlsCallbacks  // Array of TLS callback function virtual addresses
);
```

### 2. PeImageLoader Enhancement

The `PeImageLoader.Load()` method now extracts TLS callbacks from the PE file:

```csharp
private static uint[] ExtractTlsCallbacks(PEImage image, uint imageBase, VirtualMemory vm, ILogger? logger)
{
    // 1. Check if PE has a TLS directory
    var tlsDirectory = image.TlsDirectory;
    if (tlsDirectory == null)
        return Array.Empty<uint>();
    
    // 2. Get callback functions from TLS directory
    var callbackFunctions = tlsDirectory.CallbackFunctions;
    
    // 3. Convert RVAs to VAs by adding image base
    foreach (var callback in callbackFunctions)
    {
        if (callback != null && callback.IsBounded)
        {
            var callbackVa = imageBase + callback.Rva;
            callbacks.Add(callbackVa);
        }
    }
    
    return callbacks.ToArray();
}
```

### 3. Emulator Execution

The `Emulator.LoadExecutable()` method now executes TLS callbacks after module registration but before the main entry point:

```csharp
public void LoadExecutable(string path, ...)
{
    // 1. Load PE file
    // 2. Initialize memory and CPU
    // 3. Register Win32 modules
    // 4. Initialize main thread
    
    // 5. Execute TLS callbacks (NEW)
    ExecuteTlsCallbacks();
}
```

The `ExecuteTlsCallbacks()` method:

1. Sets up the stack with callback parameters:
   - `DllHandle`: Image base address (hModule)
   - `Reason`: `DLL_PROCESS_ATTACH` (1)
   - `Reserved`: NULL (0)

2. Sets EIP to each callback address in sequence

3. Executes each callback until it returns

4. Restores original EIP and ESP after all callbacks complete

### Callback Signature

TLS callbacks use the stdcall convention with this signature:

```c
void NTAPI TlsCallback(
    PVOID DllHandle,   // Base address of the image
    DWORD Reason,      // Why callback is being called
    PVOID Reserved     // Always NULL for process attach
);
```

## Testing

Tests are provided in `Win32Emu.Tests.Emulator/TlsCallbackTests.cs`:

1. **LoadedImage_WithNoTlsCallbacks_ShouldHaveEmptyArray**: Verifies that images without TLS callbacks have an empty array
2. **LoadedImage_WithTlsCallbacks_ShouldStoreAddresses**: Verifies that TLS callback addresses are properly stored
3. **PeImageLoader_WithNoTlsDirectory_ShouldReturnEmptyCallbackArray**: Documents expected behavior for PE files without TLS

## Usage Examples

Most PE files do not have TLS callbacks. When present, they are typically used for:

1. **Anti-debugging techniques**: Some malware and protected software use TLS callbacks to detect debuggers before the main entry point
2. **Early initialization**: Code that needs to run before C runtime initialization
3. **Thread-local state setup**: Initialize thread-local storage before any thread code runs

## Limitations

Current implementation:

- ✅ Executes TLS callbacks with `DLL_PROCESS_ATTACH` on process start
- ✅ Properly sets up stack and parameters
- ✅ Restores CPU state after callback execution
- ⚠️ Does not execute callbacks on thread attach/detach events
- ⚠️ Does not execute callbacks on process detach

These limitations match the most common use cases and can be extended in the future if needed.

## Security Considerations

TLS callbacks are often used by malware and anti-debugging software:

- They execute **before** the main entry point, making them harder to detect
- They can perform environment checks, anti-debugging, or anti-VM detection
- The emulator now properly executes these callbacks, improving compatibility with protected software

## References

- [Microsoft PE/COFF Specification - TLS Directory](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format#the-tls-directory)
- [AsmResolver Documentation - TLS](https://docs.washi.dev/asmresolver/)
- Thread Local Storage on Windows: https://docs.microsoft.com/en-us/windows/win32/procthread/thread-local-storage
