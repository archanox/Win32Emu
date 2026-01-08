# Decompilation to C# Transpilation - Implementation Notes

## Overview

This document describes the implementation of `Win32Emu.Tools.DecompToCS`, a tool that transpiles decompiled C++ code from various decompilers (Hex-Rays IDA, Ghidra, Binary Ninja, etc.) into executable C# code that can be used with Win32Emu's JIT cache system.

## Problem Statement

The original request was:

> "is it possible we could use the decomp of ign_tease to recompile the game into c#? We could use it in conjunction with the jit cache to allow us to inspect and examine the initialisation flow we are having ongoing issues with"

### Use Case: ign_teas Investigation

The ign_teas game has initialization flow issues that are difficult to debug in the emulator. By converting the decompiled C++ code to C#, we can:

1. **Understand initialization** - Step through the startup code in a C# debugger
2. **Compare behavior** - Run side-by-side with emulated execution to find divergences
3. **Set breakpoints** - Stop at specific functions to inspect state
4. **Trace calls** - See the exact call sequence during initialization
5. **Validate logic** - Verify that our emulator produces the same results as the original code

## Architecture

### Input: Decompiled C++ Code

The tool accepts decompiled C++ code from various decompilers. We have 8 different decompilations of ign_teas in `Decomp/ign_teas/`:

- **hexrays.cpp** (12,555 lines) - Hex-Rays IDA Pro output (best quality)
- **ghidra.cpp** (15,268 lines) - Ghidra decompiler output
- **binaryninja.cpp** (23,734 lines) - Binary Ninja output
- **retdec.cpp** (34,125 lines) - RetDec output
- **snowman.cpp** (37,954 lines) - Snowman decompiler
- **boomerang.cpp** (19,831 lines) - Boomerang decompiler
- **recstudio.cpp** (17,264 lines) - Rec Studio output
- **reko.cpp** (14,703 lines) - Reko decompiler

Example from `hexrays.cpp`:

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

### Output: Executable C# Code

The tool generates C# classes that can be:
1. Compiled into DLLs
2. Debugged in Visual Studio or dnSpy
3. Integrated with the emulator via JIT cache
4. Used for analysis and comparison

Example generated output:

```csharp
namespace Win32Emu.Generated
{
    /// <summary>
    /// Function at 0x00403070 (WinMain)
    /// Decompiled from: hexrays.cpp
    /// </summary>
    public class Function_00403070
    {
        private readonly EmulatorEnvironment _env;
        
        [OriginalAddress(0x00403070)]
        public int Execute(uint hInstance, uint hPrevInstance, string lpCmdLine, int nShowCmd)
        {
            // Call initialization function
            var initResult = CallFunction(0x004032A0);
            if (initResult == 0)
                return 0;
            
            // Message loop
            while (true)
            {
                var msg = new MSG();
                var result = _env.CallWin32Api("User32.GetMessageA", ref msg, 0, 0, 0);
                if (result == 0)
                    break;
                    
                _env.CallWin32Api("User32.TranslateMessage", ref msg);
                _env.CallWin32Api("User32.DispatchMessageA", ref msg);
            }
            
            return msg.wParam;
        }
    }
}
```

## Implementation Phases

### Phase 1: Function Declaration Parsing (Current Implementation)

**Status**: ✅ Complete

The tool currently parses function declarations from decompiled C++ code:

```csharp
var functionPattern = @"(\w+)\s+(?:__(?:cdecl|stdcall|fastcall))?\s*(\w+)\s*\((.*?)\)";
```

This extracts:
- Return type
- Calling convention
- Function name
- Parameters
- Original address (from function name like `sub_401000`)

**Example parsing result:**

From: `int __stdcall sub_401000(int a1, int a2)`

To: 
```csharp
FunctionInfo {
    Name = "sub_401000",
    ReturnType = "int",
    Parameters = "int a1, int a2",
    Address = 0x00401000
}
```

### Phase 2: C# Code Generation (Current Implementation)

**Status**: ✅ Basic version complete

The tool generates skeleton C# classes:

```csharp
public class Function_00401000
{
    private readonly EmulatorEnvironment _env;
    
    [OriginalAddress(0x00401000)]
    public int Execute(int a1, int a2)
    {
        throw new NotImplementedException("Function implementation not yet transpiled");
    }
}
```

### Phase 3: Function Body Transpilation (To Be Implemented)

**Status**: 🚧 Planned

This phase would parse the actual function implementation and convert it to C#:

**Challenges:**
1. **Complex pointer arithmetic** - C++ pointers need to map to memory addresses
2. **Type inference** - Decompilers often use generic types like `int`, `_BYTE*`
3. **Control flow** - `goto`, nested loops, complex conditionals
4. **Win32 API calls** - Need to map to emulator API calls
5. **Data structures** - C++ structs to C# classes

**Approach:**

1. Parse function body into AST (Abstract Syntax Tree)
2. Identify Win32 API calls and map to emulator
3. Convert pointer operations to memory access
4. Map C++ types to C# equivalents
5. Generate executable C# code

**Example transformation:**

```cpp
// C++ decompiled code
int sub_401000(int a1)
{
    HWND hWnd = CreateWindowExA(0, "MyClass", "Title", WS_OVERLAPPEDWINDOW, 
                                 100, 100, 800, 600, 0, 0, hInstance, 0);
    if (hWnd == 0)
        return 0;
    ShowWindow(hWnd, SW_SHOW);
    return 1;
}
```

```csharp
// Generated C#
public int Execute(int a1)
{
    uint hWnd = _env.CallWin32Api<uint>("User32.CreateWindowExA", 
        0u, "MyClass", "Title", 0xCF0000u, 
        100, 100, 800, 600, 0u, 0u, _env.hInstance, 0u);
    
    if (hWnd == 0)
        return 0;
    
    _env.CallWin32Api("User32.ShowWindow", hWnd, 1 /* SW_SHOW */);
    return 1;
}
```

### Phase 4: JIT Cache Integration (Partially Implemented)

**Status**: 🚧 Partial

Currently generates metadata files. Full integration would:

1. Generate JIT cache entries with address mappings
2. Allow emulator to call generated C# code at specific addresses
3. Enable breakpoints in Visual Studio that trigger when x86 code reaches that address
4. Provide seamless debugging experience

## Current Limitations

### What Works Now

✅ Parse function declarations from decompiled code  
✅ Extract function addresses from names  
✅ Generate skeleton C# classes  
✅ Create compilable project structure  
✅ Detect decompiler format automatically  
✅ Generate metadata files  

### What Doesn't Work Yet

❌ Function body transpilation (manual implementation needed)  
❌ Complex pointer arithmetic conversion  
❌ Automatic Win32 API mapping  
❌ Data structure conversion  
❌ Full JIT cache integration  
❌ Debugging integration with running emulator  

## Usage Workflow

### Current Usage (Manual Implementation)

1. **Generate skeleton code:**
   ```bash
   dotnet run --project Win32Emu.Tools.DecompToCS -- \
       Decomp/ign_teas/hexrays.cpp \
       --output ./IgNTeasCS \
       --exe EXEs/ign_teas.exe
   ```

2. **Manually implement function bodies:**
   - Open generated `Function_XXXXXXXX.cs` files
   - Look at corresponding function in `hexrays.cpp`
   - Manually translate C++ logic to C#
   - Use `_env.CallWin32Api()` for Win32 calls

3. **Build and debug:**
   ```bash
   cd IgNTeasCS
   dotnet build
   ```

4. **Integrate with emulator** (future):
   - Load C# DLLs at runtime
   - Map addresses to C# functions
   - Call C# instead of emulating when address is hit

### Future Usage (Automated)

Once Phase 3 is complete:

1. **Fully automated transpilation:**
   ```bash
   dotnet run --project Win32Emu.Tools.DecompToCS -- \
       Decomp/ign_teas/hexrays.cpp \
       --output ./IgNTeasCS \
       --jit-cache \
       --cache-dir ./JitCache
   ```

2. **Run with cache:**
   ```bash
   Win32Emu.Gui --nogui EXEs/ign_teas.exe \
       --cache-dir ./JitCache \
       --debug
   ```

3. **Debug in Visual Studio:**
   - Set breakpoint in `Function_004032A0.Execute()`
   - Breakpoint hits when game reaches 0x004032A0
   - Step through C# code
   - Inspect emulator state

## Integration with Existing Tools

### Works With

- **Win32Emu.Tools.AotCompiler** - Pre-compilation from x86 to RTL to C#
- **Win32Emu.Cpu.Jit.JitCache** - JIT cache system
- **Win32Emu.Rtl.RtlJitCache** - RTL cache
- **Debugging tools** - `--debug`, `--interactive-debug`, `--gdb-server`

### Comparison with AotCompiler

| Feature | AotCompiler | DecompToCS |
|---------|-------------|------------|
| Input | x86 PE executable | Decompiled C++ code |
| Analysis | x86 instruction level | High-level C++ logic |
| Output | RTL intermediate code | C# source code |
| Debugging | Limited (RTL level) | Full (C# debugger) |
| Accuracy | Exact x86 behavior | Approximate (decompiler quality) |
| Use case | Performance | Understanding/debugging |

**Key difference**: AotCompiler maintains exact x86 behavior, while DecompToCS provides human-readable code for understanding logic.

## Benefits for ign_teas Investigation

### Why This Helps

1. **Initialization flow visibility**
   - See exact sequence of calls during startup
   - Understand what functions are called when
   - Identify initialization prerequisites

2. **State inspection**
   - Set breakpoints at critical points
   - Inspect variable values during init
   - Compare with emulator state

3. **Logic verification**
   - Verify emulator produces same results
   - Find discrepancies in behavior
   - Validate Win32 API implementations

4. **Documentation**
   - Generated C# serves as documentation
   - Easy to understand game logic
   - Can add comments and annotations

### Specific ign_teas Issues

From `IGN_TEAS_INVESTIGATION_SUMMARY.md`, we know ign_teas has issues with:
- Initialization timing
- DirectX component setup
- Message loop behavior

With transpiled C# code, we can:
1. Step through initialization function `sub_4032A0`
2. See exact DirectX calls being made
3. Compare with emulator's DirectX implementation
4. Identify where behavior diverges

## Future Enhancements

### Short Term (Next Steps)

1. **Implement basic function body parsing**
   - Simple statements (assignments, returns)
   - Function calls
   - Basic control flow (if, while)

2. **Win32 API call detection and mapping**
   - Recognize common Win32 APIs
   - Generate correct `_env.CallWin32Api()` calls
   - Handle different calling conventions

3. **Improve type mapping**
   - Better C++ to C# type conversion
   - Handle pointers properly
   - Support structs and classes

### Long Term (Advanced Features)

1. **Full AST-based transpilation**
   - Use Clang or similar to parse C++
   - Generate accurate C# from AST
   - Preserve semantics exactly

2. **Interactive refinement**
   - UI for manual corrections
   - Learn from corrections
   - Improve automation over time

3. **Bidirectional mapping**
   - Map C# back to x86 addresses
   - Enable debugging in either direction
   - Synchronized breakpoints

4. **Multi-file support**
   - Handle multiple decompilation files
   - Resolve cross-file dependencies
   - Generate cohesive project

## Conclusion

The `Win32Emu.Tools.DecompToCS` tool provides a foundation for converting decompiled C++ code into executable C# code. While full automation is not yet complete, the current implementation provides:

- **Immediate value**: Structure for manual implementation
- **Future potential**: Path to full automation
- **Investigation tool**: Helps understand game initialization flows
- **Debugging aid**: Enables C# debugger for game code

For the ign_teas investigation, developers can now:
1. Generate C# skeletons from decompilation
2. Manually implement critical functions
3. Debug initialization flow in C#
4. Compare with emulator behavior
5. Identify and fix emulation issues

**Next step**: Manually implement key initialization functions from ign_teas decompilation to understand the initialization flow issues.

---

**Status**: 🚧 In Development (Phase 1 & 2 complete, Phase 3 & 4 planned)  
**Primary Use Case**: ign_teas initialization flow investigation  
**Current Version**: 0.1 (skeleton generation only)
