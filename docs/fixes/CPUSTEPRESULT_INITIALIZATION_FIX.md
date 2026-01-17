# CpuStepResult Initialization Fix

## Issue

**Error:** `System.MissingMethodException: Method not found: 'Void Win32Emu.Cpu.CpuStepResult.set_IsCall(Boolean)'`

This error occurred when executing JIT-compiled code blocks due to an incompatibility between the generated code and the `CpuStepResult` struct definition.

## Root Cause

The `CpuStepResult` is defined as a readonly record struct:
```csharp
public readonly record struct CpuStepResult(bool IsCall, uint CallTarget, bool IsSyscall = false, bool IsDosInterrupt = false);
```

However, the code generator (`RtlToCSharpGenerator.cs`) and several error handling paths in `JitCpu.cs` were using object initializer syntax:
```csharp
new CpuStepResult { IsCall = false, CallTarget = 0 }
```

Object initializer syntax requires settable properties, but readonly record structs with positional parameters don't expose property setters. When JIT-compiled code using object initializers was invoked via reflection, it resulted in a `MissingMethodException` at runtime.

## Solution

Changed all `CpuStepResult` instantiations to use constructor syntax instead of object initializer syntax:
```csharp
new CpuStepResult(IsCall: false, CallTarget: 0)
```

### Files Modified

1. **Win32Emu.Rtl/RtlToCSharpGenerator.cs**
   - Updated generated code to use constructor syntax (lines 88, 157)
   - Removed duplicate `CpuStepResult` struct definition

2. **Win32Emu/Cpu/Jit/JitCpu.cs**
   - Updated error handling paths to use constructor syntax (lines 1651, 1660, 1668, 1695, 1700)

## User Action Required

If you encounter this error, clear your JIT cache to force recompilation with the new syntax:

**Windows:**
```powershell
Remove-Item -Recurse $env:LOCALAPPDATA\Win32Emu\JitCache
```

**Linux:**
```bash
rm -rf ~/.local/share/Win32Emu/JitCache
```

**macOS:**
```bash
rm -rf ~/Library/Application\ Support/Win32Emu/JitCache
```

After clearing the cache, JIT blocks will be automatically recompiled with the correct constructor syntax on the next run.

## Testing

- ✅ All 52 JitCpuInstructionTests pass
- ✅ 130 out of 132 JIT-related tests pass
- ✅ Build succeeds with 0 errors

## Related Files

- `Win32Emu/Cpu/CpuStepResult.cs` - Struct definition
- `Win32Emu.Rtl/RtlToCSharpGenerator.cs` - Code generator
- `Win32Emu/Cpu/Jit/JitCpu.cs` - JIT CPU implementation
- `docs/implementation/JIT_CACHE_IMPLEMENTATION.md` - JIT cache documentation
