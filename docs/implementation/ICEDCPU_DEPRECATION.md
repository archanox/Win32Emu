# IcedCpu Deprecation Plan

## Summary

This document outlines the plan to deprecate and remove IcedCpu in favor of JitCpu as the solitary CPU emulator for Win32Emu.

## Background

Win32Emu currently has two CPU emulator implementations:
- **IcedCpu**: Interpreter-based emulator using the Iced library for instruction decoding
- **JitCpu**: JIT-compiled emulator with fallback to interpreter mode

JitCpu is the preferred emulator going forward as it:
- Provides better performance through JIT compilation on supported platforms
- Falls back gracefully to interpreter mode when JIT is unavailable (e.g., WASM)
- Has the same functionality as IcedCpu for core CPU emulation
- Is actively maintained and improved

## Current Status

### Completed
✅ **All tests migrated from IcedCpu to JitCpu** (as of this PR)
- Test infrastructure helpers (CpuTestHelper, ThreeWayTestHelper) now use JitCpu
- 31 test files in Win32Emu.Tests.Emulator migrated
- 7 test files in Win32Emu.Tests.Kernel32 migrated
- SingleStepTests subdirectory migrated

### Production Code Still Using IcedCpu

#### 1. MsvcrtModule.cs
**Location**: `Win32Emu/Win32/Modules/MsvcrtModule.cs`

**Current State**: Type checks for both IcedCpu and JitCpu
```csharp
if (_cpu is Cpu.Iced.IcedCpu icedCpu)
{
    // IcedCpu-specific FPU operations
}
else if (_cpu is Cpu.Jit.JitCpu jitCpu)
{
    // JitCpu FPU operations
}
```

**Action Required**: Remove IcedCpu-specific code paths after verifying JitCpu FPU operations work correctly

**Affected Methods**:
- Line 1438-1441: `FpuGetSt(0)` and `FpuPop()`
- Line 1510-1514: `FpuReset()`
- Line 2186: FPU operations
- Line 2217: FPU operations

#### 2. Emulator.cs
**Location**: `Win32Emu/Emulator.cs`

**Current State**: Only creates JitCpu instances (already using JitCpu as default)
```csharp
// Line 540: Create unified CPU backend (JitCpu)
var jitCpu = new Cpu.Jit.JitCpu(_vm, _logger, decoderOptions, ...);
```

**Action Required**: None - already using JitCpu exclusively

#### 3. InstructionAnalyzerTests.cs
**Location**: `Win32Emu.Tests.Emulator/InstructionAnalyzerTests.cs`

**Current State**: ✅ Migrated to JitCpu with interpreter mode

**Implementation**: 
- JitCpu now supports instruction analysis in interpreter mode
- Tests updated to use `JitCpu` with `forceInterpreterMode: true` and `enableInstructionAnalyzer: true`
- All 7 tests passing with JitCpu implementation

**Available Features**:
- `FormatCurrentInstruction()` - Formats instruction at current EIP with address
- `AnalyzeCurrentInstruction()` - Provides detailed analysis including:
  - Read and written registers
  - Memory accesses with segment, base, index, scale, displacement
  - Instruction mnemonic and length
  - OpCode information

**Action Completed**: 
- ✅ Implemented Option 3: Added instruction analysis to JitCpu for debugging purposes
- ✅ InstructionAnalyzerTests migrated from IcedCpu to JitCpu
- No dependency on IcedCpu remaining for these tests

## Dependencies to Remove

### NuGet Packages
- **Iced**: x86/x64 instruction decoder library used by IcedCpu
  - Referenced in: Win32Emu.csproj, Win32Emu.Rtl.csproj, Win32Emu.Tools.AotCompiler.csproj, Win32Emu.Tools.WasmCacheGenerator.csproj
  - Note: JitCpu also uses Iced for instruction decoding, so this cannot be removed

### Source Files to Remove
- `Win32Emu/Cpu/Iced/IcedCpu.cs` (198 KB) - Main IcedCpu implementation
- `Win32Emu/Cpu/Iced/InstructionAnalysis.cs` - Instruction analysis data structure
- `Win32Emu/Cpu/Iced/InstructionAnalyzer.cs` - Instruction analyzer
- `Win32Emu/Cpu/Iced/MemoryAccess.cs` - Memory access tracking

**Note**: Cannot remove Iced directory entirely as JitCpu uses Iced.Intel for instruction decoding

## Breaking Changes

### API Changes
1. **InstructionAnalyzer Methods** - ✅ No longer breaking!
   - `AnalyzeCurrentInstruction()` - Now available in JitCpu interpreter mode
   - `FormatCurrentInstruction()` - Now available in JitCpu interpreter mode
   - **Migration**: Use JitCpu with `enableInstructionAnalyzer: true` and `forceInterpreterMode: true`
   - **Impact**: Minimal - debugging tools can continue to use instruction analysis via JitCpu

2. **Type Checks**
   - Code that checks `if (cpu is IcedCpu)` will no longer work
   - **Impact**: Any external code or plugins checking CPU type
   - **Migration**: Use feature checks instead of type checks, or check for JitCpu

### Feature Changes
1. **Instruction Analysis** - ✅ Available in JitCpu
   - **Available**: Instruction-level analysis in JitCpu's interpreter mode
   - **How to use**: Create JitCpu with `enableInstructionAnalyzer: true` and `forceInterpreterMode: true`
   - **Features**: Same analysis capabilities as IcedCpu (register tracking, memory access detection, formatting)

2. **Debugging Capabilities** - ✅ Maintained in JitCpu
   - **Available**: Instruction-by-instruction analysis via JitCpu interpreter mode
   - **Alternative options**: JIT cache inspection, disassembly tools, enhanced logging

## Migration Steps

### Phase 1: Cleanup Production Code (Current Phase)
1. ✅ Update all test infrastructure to use JitCpu
2. ✅ Migrate all tests to JitCpu (except InstructionAnalyzerTests)
3. ⏳ Verify FPU operations work correctly in JitCpu
4. ⏳ Remove IcedCpu type checks from MsvcrtModule.cs
5. ⏳ Add deprecation warnings to IcedCpu class

### Phase 2: Documentation & Communication
1. ⏳ Update README.md to reflect JitCpu as the only CPU emulator
2. ⏳ Update architecture documentation
3. ⏳ Add migration guide for anyone using IcedCpu directly
4. ⏳ Communicate changes in release notes

### Phase 3: Removal (Future Release)
1. ⏳ Mark IcedCpu as obsolete with ObsoleteAttribute
2. ⏳ Wait one release cycle for users to migrate
3. ⏳ Remove IcedCpu source files
4. ⏳ Remove InstructionAnalyzerTests (or rewrite for JitCpu if possible)
5. ⏳ Clean up any remaining references

## Testing Strategy

### Before Removal
1. ✅ Verify all migrated tests pass with JitCpu
2. ⏳ Run full test suite with JitCpu only
3. ⏳ Test FPU operations specifically (especially MSVCRT functions)
4. ⏳ Test on all supported platforms (Windows, Linux, macOS, WASM)
5. ⏳ Verify game compatibility hasn't regressed

### Specific Test Cases
- ✅ Arithmetic operations
- ✅ Conditional jumps and loops
- ⏳ FPU operations (sin, cos, sqrt, etc.)
- ⏳ CPU state suspend/resume
- ⏳ Async JIT compilation
- ⏳ Interpreter fallback mode

## Compatibility Notes

### What Still Works
- All CPU emulation features (arithmetic, jumps, calls, FPU, etc.)
- Interpreter mode on platforms without JIT support (WASM)
- CPU state management (save/restore)
- Debugging with GDB server
- Interactive debugger
- ✅ Instruction-level analysis (via JitCpu interpreter mode)

### What Changes
- ✅ Instruction-level analysis now requires `forceInterpreterMode: true` in JitCpu
- Cannot switch between IcedCpu and JitCpu at runtime
- Slightly different performance characteristics

## Recommendations

### For End Users
- No action required - JitCpu is already the default emulator
- Games and applications should work identically

### For Developers
1. Update any code that directly creates IcedCpu instances to use JitCpu
2. Remove any type checks for IcedCpu
3. ✅ If using instruction analysis, use JitCpu with `enableInstructionAnalyzer: true` and `forceInterpreterMode: true`

### For Contributors
1. New CPU-related tests should use JitCpu
2. Do not add new features to IcedCpu
3. Focus development efforts on JitCpu improvements

## Timeline

- **Current**: All tests migrated to JitCpu ✅
- **Next Release (v1.x)**: Add deprecation warnings, verify FPU operations
- **Release v1.x+1**: Mark IcedCpu as obsolete
- **Release v1.x+2**: Remove IcedCpu completely

## Questions & Decisions Needed

1. **InstructionAnalyzerTests**: Keep with IcedCpu or remove entirely?
   - ✅ **RESOLVED**: Implemented instruction analysis in JitCpu's interpreter mode
   - JitCpu now has `FormatCurrentInstruction()` and `AnalyzeCurrentInstruction()` methods
   - InstructionAnalyzerTests migrated to use JitCpu with `forceInterpreterMode: true`
   - All 7 tests passing with JitCpu implementation

2. **FPU Operations**: Are JitCpu FPU operations fully tested?
   - Need: Comprehensive FPU testing with real-world applications

3. **Debugging Tools**: What replaces instruction-level analysis?
   - ✅ **RESOLVED**: Instruction analysis available in JitCpu's interpreter mode
   - When `enableInstructionAnalyzer: true` and `forceInterpreterMode: true`, JitCpu provides:
     - Instruction formatting with `FormatCurrentInstruction()`
     - Detailed analysis with `AnalyzeCurrentInstruction()`
     - Register read/write tracking
     - Memory access detection
   - Also consider: JIT cache inspection, disassembly output, enhanced logging

## References

- JitCpu Implementation: `Win32Emu/Cpu/Jit/JitCpu.cs`
- IcedCpu Implementation: `Win32Emu/Cpu/Iced/IcedCpu.cs`
- Test Migration PR: This PR
- JIT Cache Implementation: `docs/implementation/JIT_CACHE_IMPLEMENTATION.md`

---

**Last Updated**: 2026-01-02
**Status**: In Progress - Test Migration Complete, Instruction Analysis Implemented in JitCpu
