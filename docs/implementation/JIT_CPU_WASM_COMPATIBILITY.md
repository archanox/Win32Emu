# JIT CPU WASM Compatibility

## Overview

This document explains how the JIT CPU emulator has been modified to work in WebAssembly (WASM) environments while maintaining full functionality on native platforms.

## Problem Statement

The original JIT CPU implementation used the RTL (Register Transfer Language) pipeline with Roslyn compilation:

```
x86 Instructions → RTL → Optimized RTL → C# Code → Compiled Assembly → Execution
```

This approach had several WASM incompatibilities:

1. **Roslyn Compilation**: `Microsoft.CodeAnalysis` (Roslyn) is not available in WASM
2. **Dynamic Assembly Loading**: `Assembly.Load()` from byte arrays is not fully supported in WASM
3. **File System Access**: JIT cache persistence to disk is not available in browser environments
4. **Lokad.ILPack**: The assembly serialization library is not WASM-compatible

## Solution

The JIT CPU has been modified to detect the runtime environment and adapt its behavior:

### WASM Runtime Detection

A new `RuntimeEnvironment` utility class provides platform detection:

```csharp
public static class RuntimeEnvironment
{
    public static bool IsWasm => 
        RuntimeInformation.OSArchitecture == Architecture.Wasm ||
        RuntimeInformation.ProcessArchitecture == Architecture.Wasm;
    
    public static bool IsNative => !IsWasm;
}
```

### JIT CPU Behavior

The `JitCpu` class now checks the runtime environment at initialization:

**Native Platform (Windows/Linux/macOS)**:
- Full RTL JIT compilation is enabled
- JIT cache is persisted to disk for performance
- `SupportsJit` returns `true`
- Blocks are compiled to optimized C# code and cached

**WASM Environment**:
- RTL JIT compilation is disabled (Roslyn not available)
- Falls back to direct interpretation using `InterpretSingleInstruction()`
- `SupportsJit` returns `false`
- No disk-based caching (browser has no file system)
- All cache operations (Load/Save/Precompile) are no-ops

### Code Flow

```csharp
public async Task<CpuStepResult> ExecuteBlockAsync(VirtualMemory mem)
{
    // In WASM environment, JIT compilation is not available
    // Fall back to single instruction interpretation
    if (_isWasmEnvironment)
    {
        return await Task.FromResult(InterpretSingleInstruction(mem));
    }
    
    // Native: Use JIT compilation
    var blockStart = _eip;
    if (!_compiledBlocks.TryGetValue(blockStart, out var compiledBlock))
    {
        compiledBlock = CompileBlock(blockStart, mem);
        _compiledBlocks[blockStart] = compiledBlock;
    }
    
    var result = await ExecuteRtlBlock(compiledBlock, this, mem);
    return result;
}
```

## API Changes

### EmulatorService (WASM)

Added optional `useJitCpu` parameter to `LoadExecutableAsync`:

```csharp
public async Task<bool> LoadExecutableAsync(
    byte[] executableBytes, 
    string fileName,
    Dictionary<string, byte[]>? additionalFiles = null,
    bool force32BitStackOps = true,
    bool useJitCpu = false)  // New parameter
```

**Note**: Even when `useJitCpu=true` in WASM, the CPU runs in interpreter mode. The parameter exists for API consistency.

### Emulator

Added `useJitCpu` parameter to `LoadExecutableFromBytes`:

```csharp
public void LoadExecutableFromBytes(
    byte[] executableBytes, 
    string executableName, 
    string[]? programArgs, 
    bool debugMode, 
    int reservedMemoryMb, 
    VirtualFileSystem.IVirtualFileSystem? virtualFileSystem, 
    bool force32BitStackOps = true, 
    bool useJitCpu = false)  // New parameter
```

## Performance Implications

### Native Platform
- **With JIT**: Excellent performance due to compiled code
- **Without JIT**: Good performance using `IcedCpu` interpreter

### WASM Platform
- **JitCpu**: Same performance as `IcedCpu` (both use interpretation)
- **Memory**: JitCpu uses slightly less memory in WASM (no JIT cache)
- **Startup**: Faster startup (no JIT compilation overhead)

## Usage Examples

### Native Platform (CLI)

```bash
# Use JIT CPU (recommended for performance)
./Win32Emu.exe game.exe --use-jit-cpu

# Use IcedCpu (compatibility mode)
./Win32Emu.exe game.exe
```

### WASM Platform

```csharp
// Both CPUs work identically in WASM
await EmulatorService.LoadExecutableAsync(
    executableBytes, 
    "game.exe",
    useJitCpu: false);  // IcedCpu (default)

await EmulatorService.LoadExecutableAsync(
    executableBytes, 
    "game.exe",
    useJitCpu: true);   // JitCpu (interpreter mode)
```

## Testing

### Build Verification

```bash
# Native build
dotnet build Win32Emu/Win32Emu.csproj --configuration Release

# WASM build
dotnet build Win32Emu.Wasm/Win32Emu.Wasm.csproj --configuration Release
```

### Runtime Testing

```bash
# Test in WASM (browser)
cd Win32Emu.Wasm
dotnet run

# Test in native CLI
cd Win32Emu
dotnet run -- --exe test.exe --use-jit-cpu
```

## Migration Notes

### For Existing Code

**No breaking changes** for existing code:
- Default behavior remains unchanged (`useJitCpu: false`)
- `IcedCpu` continues to work as before
- `JitCpu` remains fully functional on native platforms

### For New Features

When adding new features that interact with the CPU:
1. Use `RuntimeEnvironment.IsWasm` to detect the platform
2. Avoid assumptions about JIT compilation availability
3. Test on both native and WASM platforms

## Future Improvements

### Possible Enhancements

1. **AOT Compilation**: Pre-compile popular game code to WASM modules
2. **Interpreter Optimization**: Add JIT-style optimizations to the interpreter
3. **Tiered Compilation**: Start with interpreter, upgrade to JIT after warm-up
4. **WebAssembly SIMD**: Use WASM SIMD instructions for x86 emulation

### Known Limitations

1. **No JIT Performance**: WASM always uses interpretation
2. **No Cache Persistence**: Browser has no file system access
3. **Memory Overhead**: No code deduplication across sessions

## References

- **JIT CPU Implementation**: `Win32Emu/Cpu/Jit/JitCpu.cs`
- **Runtime Detection**: `Win32Emu/RuntimeEnvironment.cs`
- **WASM Service**: `Win32Emu.Wasm/Services/EmulatorService.cs`
- **RTL JIT Cache**: `Win32Emu.Rtl/RtlJitCache.cs`
- **WASM Compatibility Analysis**: `docs/investigation/IGN_TEAS_WASM_COMPATIBILITY_ANALYSIS.md`

## Conclusion

The JIT CPU now works seamlessly in both native and WASM environments:

- **Native**: Full JIT compilation with excellent performance
- **WASM**: Graceful fallback to interpretation with stable performance
- **API**: Consistent interface across platforms
- **Code**: Zero runtime overhead for platform detection (cached at initialization)

This implementation enables the WASM UI to use either CPU backend without modification, providing a foundation for future optimizations.
