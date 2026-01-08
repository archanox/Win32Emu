# JIT Integration Usage

## Overview

This project contains 128 transpiled C# functions that can be integrated with Win32Emu's JIT system.

## Loading Transpiled Functions

```csharp
using Win32Emu;
using IgNTeas.Generated;

// Create the function loader
var loader = new TranspiledFunctionLoader(logger);

// Check if a function is available
if (loader.HasFunction(0x004032A0))
{
    Console.WriteLine("Initialization function available");
}

// Execute a transpiled function
if (loader.TryExecuteFunction(0x004032A0, env, Array.Empty<object>(), out var result))
{
    Console.WriteLine($"Function returned: {result}");
}
```

## Integration with JitCpu

To integrate with the JIT CPU, you can modify the emulator to check for transpiled functions before JIT compiling:

```csharp
// In your emulator initialization
var transpiledLoader = new IgNTeas.Generated.TranspiledFunctionLoader(logger);

// Before executing a block at an address:
if (transpiledLoader.HasFunction(eip))
{
    // Execute the transpiled C# version instead of JIT compiling
    if (transpiledLoader.TryExecuteFunction(eip, env, Array.Empty<object>(), out var result))
    {
        // Update CPU state based on result
        // Set EIP to return address, etc.
        return;
    }
}

// Otherwise, proceed with normal JIT compilation
await cpu.ExecuteBlockAsync(memory);
```

## Compiled Assembly

You can also compile this project into a DLL and load it dynamically:

```bash
# Build the transpiled functions
dotnet build -c Release

# Reference the DLL in your emulator project
# Or load it dynamically at runtime
```

## Benefits

- **Debugging**: Step through C# code in Visual Studio/dnSpy
- **Performance**: Pre-compiled C# executes faster than JIT compilation
- **Inspection**: Understand game logic without reverse engineering
- **Modification**: Easily modify behavior for testing/patching

## Limitations

- Global variables (dword_XXXXXX) need to be mapped to emulator memory
- Function calls to other transpiled functions need proper integration
- Complex pointer operations may need manual refinement
- Win32 API calls are routed through EmulatorEnvironment
