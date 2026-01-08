# Win32Emu.Tools.DecompToCS

Transpilation tool that converts decompiled C++ code (from Hex-Rays, Ghidra, etc.) into executable C# code for use with Win32Emu's JIT cache system.

## Purpose

This tool enables:
1. **Inspection of initialization flows** - Convert decompiled code to C# to understand game startup behavior
2. **Debugging support** - Step through converted C# code in Visual Studio or dnSpy
3. **JIT cache integration** - Generate cache entries that map to original x86 addresses
4. **Analysis** - Study game behavior without running x86 code

## Use Case: ign_teas Investigation

The primary motivation for this tool is to help investigate initialization issues in games like `ign_teas`. By converting the decompiled C++ to C#:
- We can trace initialization flow step-by-step
- Identify where the emulator differs from native behavior
- Set breakpoints at specific functions to inspect state
- Compare expected vs actual behavior

## Features

- **Multiple decompiler support**: Hex-Rays, Ghidra, Binary Ninja, RetDec
- **Type mapping**: Automatic conversion of C++ types to C# equivalents
- **Win32 API mapping**: Maps Win32 API calls to emulator implementations
- **Address preservation**: Maintains original x86 addresses for debugging
- **Function graph generation**: Creates call graph from decompiled code
- **Partial compilation**: Works with incomplete decompilations

## Usage

### Basic Transpilation

```bash
dotnet run --project Win32Emu.Tools.DecompToCS -- decomp.cpp --output ./CSharpOutput
```

### Specify Decompiler Format

```bash
# Hex-Rays IDA output
dotnet run --project Win32Emu.Tools.DecompToCS -- hexrays.cpp --format hexrays

# Ghidra output
dotnet run --project Win32Emu.Tools.DecompToCS -- ghidra.cpp --format ghidra

# Binary Ninja output
dotnet run --project Win32Emu.Tools.DecompToCS -- binaryninja.cpp --format binaryninja
```

### Generate JIT Cache Integration

```bash
dotnet run --project Win32Emu.Tools.DecompToCS -- hexrays.cpp --jit-cache --cache-dir ./JitCache
```

### With Original Executable (for address mapping)

```bash
dotnet run --project Win32Emu.Tools.DecompToCS -- hexrays.cpp --exe game.exe --jit-cache
```

### All Options

```bash
Win32Emu.Tools.DecompToCS <decomp-file.cpp> [options]

Options:
  --output <dir>        Output directory for C# files (default: ./CSharpOutput)
  --format <format>     Decompiler format: hexrays, ghidra, binaryninja, retdec (auto-detected)
  --exe <file>          Original executable for address mapping
  --jit-cache           Generate JIT cache integration
  --cache-dir <dir>     JIT cache directory (default: ./JitCache)
  --namespace <ns>      C# namespace (default: Win32Emu.Generated)
  --partial             Allow partial compilation (skip unresolved symbols)
  --verbose             Enable verbose logging
```

## Architecture

### Input: Decompiled C++

```cpp
int __cdecl sub_401000(int a1, int a2)
{
    HWND hWnd = CreateWindowExA(0, "MyClass", "Title", WS_OVERLAPPEDWINDOW, 
                                 100, 100, 800, 600, 0, 0, hInstance, 0);
    if (hWnd == 0)
        return 0;
    ShowWindow(hWnd, SW_SHOW);
    return 1;
}
```

### Output: Executable C#

```csharp
namespace Win32Emu.Generated
{
    /// <summary>
    /// Function at 0x00401000
    /// Decompiled from: hexrays.cpp
    /// </summary>
    public class Function_00401000
    {
        private readonly EmulatorEnvironment _env;
        
        public Function_00401000(EmulatorEnvironment env)
        {
            _env = env;
        }
        
        [OriginalAddress(0x00401000)]
        public uint Execute(uint a1, uint a2)
        {
            // CreateWindowExA
            uint hWnd = _env.CallWin32Api("User32.CreateWindowExA", 
                0u, "MyClass", "Title", 0xCF0000u /* WS_OVERLAPPEDWINDOW */,
                100, 100, 800, 600, 0u, 0u, _env.hInstance, 0u);
            
            if (hWnd == 0)
                return 0;
            
            // ShowWindow
            _env.CallWin32Api("User32.ShowWindow", hWnd, 1 /* SW_SHOW */);
            
            return 1;
        }
    }
}
```

## Transpilation Process

```
Decompiled C++ (hexrays.cpp)
    ↓
[Parser] → Parse function declarations and definitions
    ↓
[Type Mapper] → Convert C++ types to C# types
    ↓
[API Mapper] → Map Win32 API calls to emulator
    ↓
[Code Generator] → Generate C# classes
    ↓
[Address Linker] → Link to original x86 addresses
    ↓
[JIT Cache Generator] → Create cache metadata (optional)
    ↓
Output: C# source + JIT cache
```

## Type Mapping

| C++ Type | C# Type | Notes |
|----------|---------|-------|
| `int` | `int` | 32-bit signed |
| `unsigned int` | `uint` | 32-bit unsigned |
| `__int16` | `short` | 16-bit signed |
| `DWORD` | `uint` | Windows type |
| `HWND` | `uint` | Handle (as pointer) |
| `LPSTR` | `string` | Null-terminated string |
| `void*` | `uint` | Pointer as 32-bit address |
| `_BYTE*` | `uint` | Byte pointer as address |

## API Call Mapping

Win32 API calls are converted to emulator API calls:

```cpp
// C++ decompiled code
CreateWindowExA(0, "MyClass", "Title", WS_OVERLAPPEDWINDOW, ...);
```

```csharp
// Generated C#
_env.CallWin32Api("User32.CreateWindowExA", 0u, "MyClass", "Title", 0xCF0000u, ...);
```

## Debugging Workflow

### Step 1: Transpile Decompilation

```bash
cd /path/to/Win32Emu
dotnet run --project Win32Emu.Tools.DecompToCS -- \
    Decomp/ign_teas/hexrays.cpp \
    --exe EXEs/ign_teas.exe \
    --jit-cache \
    --output ./IgNTeasCS
```

### Step 2: Build Generated Code

```bash
cd IgNTeasCS
dotnet build
```

### Step 3: Run Emulator with Generated Cache

```bash
Win32Emu.Gui --nogui EXEs/ign_teas.exe --cache-dir ./IgNTeasCS/JitCache --debug
```

### Step 4: Debug in Visual Studio

1. Open `IgNTeasCS/Win32Emu.Generated.sln`
2. Attach to running `Win32Emu.Gui` process
3. Set breakpoints in generated `Function_XXXXXXXX.cs` files
4. Inspect state when functions execute

## Example: ign_teas Initialization Flow

From `Decomp/ign_teas/hexrays.cpp`:

```cpp
int __stdcall WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
    // Function at 0x00403070
    if (!sub_4032A0())  // Initialization
        return 0;
    
    MSG msg;
    while (GetMessageA(&msg, 0, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessageA(&msg);
    }
    return msg.wParam;
}
```

After transpilation, we can:
1. Set a breakpoint in `Function_004032A0.Execute()`
2. Step through initialization code
3. Compare values with native execution
4. Identify where emulator behavior diverges

## Limitations

### Current Limitations
- **No complex pointer arithmetic**: Limited support for pointer manipulation
- **No inline assembly**: ASM blocks are not converted
- **No templates**: C++ templates require manual conversion
- **No operator overloading**: Operator overloading not preserved
- **Single-threaded**: No multi-threading support yet

### Decompiler-Specific Issues
- **Hex-Rays**: Best supported, cleanest output
- **Ghidra**: Good support, may need manual cleanup
- **Binary Ninja**: Moderate support, function signatures vary
- **RetDec**: Limited support, verbose output

### Accuracy Considerations
- Decompilation is **approximate** - not exact source reconstruction
- Generated C# may not behave identically to x86 in all cases
- Use for **analysis and debugging**, not production execution
- **Validate behavior** against actual x86 execution in emulator

## Integration with Existing Tools

### Works with JIT Cache System
The transpiled C# can generate JIT cache entries compatible with:
- `Win32Emu.Tools.AotCompiler` - Pre-compiled blocks
- `Win32Emu.Rtl.RtlJitCache` - Runtime cache
- `Win32Emu.Cpu.Jit.JitCache` - JIT CPU cache

### Complements Debugging Tools
Use with existing debugging infrastructure:
- `--debug` mode - Enhanced debugging
- `--interactive-debug` - Step-through debugger
- `--gdb-server` - GDB remote debugging
- OpenTelemetry tracing

## Performance

| Metric | Value |
|--------|-------|
| Parsing speed | ~1000-5000 functions/second |
| C# generation | ~500-2000 functions/second |
| Output size | ~10-50KB per function |
| Compilation time | ~1-5 seconds per 100 functions |

## Future Enhancements

1. **Improved type inference** - Better C++ to C# type mapping
2. **Control flow preservation** - Maintain exact control flow
3. **Data structure mapping** - Convert C++ structs to C# classes
4. **Multi-file support** - Handle multiple decompilation files
5. **Interactive mode** - Manual refinement of generated code
6. **Roundtrip verification** - Compare generated code with original x86

## See Also

- [Win32Emu.Tools.AotCompiler](../Win32Emu.Tools.AotCompiler/README.md) - Pre-compilation tool
- [JIT_CACHE_IMPLEMENTATION.md](../docs/implementation/JIT_CACHE_IMPLEMENTATION.md) - JIT caching
- [DEBUGGING_GUIDE.md](../docs/guides/DEBUGGING_GUIDE.md) - Debugging techniques
- [GDB_SERVER_GUIDE.md](../docs/guides/GDB_SERVER_GUIDE.md) - GDB integration

---

**Status**: 🚧 In Development  
**Primary Use Case**: ign_teas initialization flow investigation  
**License**: Same as Win32Emu
