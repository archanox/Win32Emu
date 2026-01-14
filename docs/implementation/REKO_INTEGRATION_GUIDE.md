# Reko Decompiler Integration for Win32Emu

## Overview

This document describes the integration of Reko.Decompiler.Runtime into Win32Emu's JIT compilation pipeline as an alternative to parsing C/C++ decompiler text output.

## Problem Statement

**Original Request**: Instead of parsing the c/C++ decomp text output, could we use https://www.nuget.org/packages/Reko.Decompiler.Runtime to generate the c# decompiled code in the jit?

## Solution Architecture

### Pluggable Decompiler Adapter Pattern

We implemented a **pluggable adapter pattern** that allows different decompiler backends to be used interchangeably in the JIT pipeline:

```
┌─────────────────────────────────────────┐
│          JitCpu / RtlJitCache           │
└──────────────┬──────────────────────────┘
               │
               ▼
     ┌─────────────────────┐
     │ IDecompilerAdapter  │ ◄── Plugin Interface
     └─────────────────────┘
               │
      ┌────────┴──────────┐
      │                   │
      ▼                   ▼
┌──────────────┐  ┌───────────────┐
│ CustomRTL    │  │ Reko Adapter  │
│ (MIT)        │  │ (GPLv2)       │
│ ✓ Default    │  │ ⚠ Optional    │
└──────────────┘  └───────────────┘
```

### Components

#### 1. `IDecompilerAdapter` Interface
```csharp
public interface IDecompilerAdapter
{
    string Name { get; }
    bool IsAvailable { get; }
    string LicenseInfo { get; }
    Task<string> DecompileToCSharpAsync(uint startAddress, 
        List<Instruction> instructions, string className);
}
```

#### 2. `CustomRtlDecompilerAdapter` (Default)
- **License**: MIT (compatible with Win32Emu)
- **Status**: Always available
- **Implementation**: Uses existing Win32Emu RTL pipeline
  - `X86ToRtlConverter` - x86 → RTL
  - `RtlOptimizer` - Optimization passes
  - `RtlToCSharpGenerator` - RTL → C#

#### 3. `RekoDecompilerAdapter` (Optional)
- **License**: GPLv2 (incompatible with Win32Emu's MIT license)
- **Status**: Opt-in only, disabled by default
- **Activation**: Requires:
  1. NuGet package: `Reko.Decompiler.Runtime`
  2. Environment variable: `WIN32EMU_USE_REKO=true`
  3. User acknowledgment of GPLv2 licensing

## Licensing Considerations

### Why Opt-In?

Reko is licensed under **GPLv2**, which is a viral "copyleft" license:
- ✅ **Allowed**: Use for internal/personal projects
- ✅ **Allowed**: Use when distributing GPLv2-licensed software
- ❌ **Not Allowed**: Use in proprietary software without releasing source
- ❌ **Not Allowed**: Use in MIT-licensed projects without contamination

Win32Emu is **MIT licensed**, which is permissive and incompatible with GPLv2.

### Solution: Optional Plugin

By making Reko an **optional, opt-in plugin**:
1. Win32Emu remains MIT-licensed (doesn't depend on Reko)
2. Users who want Reko can enable it knowingly
3. Users are informed about GPL requirements
4. Default behavior (CustomRTL) is MIT-compliant

### Legal Safety

```csharp
// RekoDecompilerAdapter checks availability via reflection
// No hard dependency on Reko assemblies
private bool CheckRekoAvailability()
{
    var rekoCore = Type.GetType("Reko.Core.Address, Reko.Core");
    var rekoArch = Type.GetType("Reko.Arch.X86.X86ArchitectureFlat32, Reko.Arch.X86");
    return rekoCore != null && rekoArch != null;
}
```

## Usage

### Default Behavior (CustomRTL)

```bash
# No configuration needed - uses MIT-licensed CustomRTL by default
dotnet run --project Win32Emu.Gui -- --nogui game.exe
```

```csharp
// In code - default adapter selected automatically
var cache = new RtlJitCache(cacheDir, logger);
// Uses CustomRtlDecompilerAdapter automatically
```

### Enabling Reko (Opt-In)

```bash
# Step 1: Add Reko NuGet package
dotnet add package Reko.Decompiler.Runtime --version 0.11.6

# Step 2: Set environment variable
export WIN32EMU_USE_REKO=true

# Step 3: Run emulator
dotnet run --project Win32Emu.Gui -- --nogui game.exe

# Output will show:
# [RtlJitCache] Using decompiler: Reko (GPLv2 - Reko Decompiler)
# [RekoAdapter] Reko decompiler is enabled. Note: Reko is GPLv2 licensed.
```

```csharp
// In code - explicitly use Reko adapter
var rekoAdapter = new RekoDecompilerAdapter(logger);
if (rekoAdapter.IsAvailable)
{
    var cache = new RtlJitCache(cacheDir, logger, rekoAdapter);
}
```

### Programmatic Selection

```csharp
// Factory pattern for adapter selection
IDecompilerAdapter adapter = Environment.GetEnvironmentVariable("WIN32EMU_USE_REKO") == "true"
    ? new RekoDecompilerAdapter(logger)
    : new CustomRtlDecompilerAdapter(logger);

var cache = new RtlJitCache(cacheDir, logger, adapter);
```

## Current Implementation Status

### ✅ Completed

1. **IDecompilerAdapter Interface**
   - Defines contract for pluggable decompilers
   - Includes license information exposure
   - Async-ready API

2. **CustomRtlDecompilerAdapter**
   - Wraps existing Win32Emu RTL pipeline
   - MIT-licensed, always available
   - Fully functional

3. **RekoDecompilerAdapter (Stub)**
   - Opt-in detection logic
   - License warning/logging
   - Availability checking via reflection
   - Stub implementation (not yet fully integrated with Reko API)

4. **RtlJitCache Integration**
   - Modified to accept `IDecompilerAdapter`
   - Auto-selects appropriate adapter
   - Backward compatible

### 🚧 Not Yet Implemented (Reko Integration Details)

The `RekoDecompilerAdapter` currently has placeholder logic. Full integration requires:

1. **Reko x86 Architecture Setup**
   ```csharp
   // Pseudo-code for future implementation
   var sc = new ServiceContainer();
   var arch = new X86ArchitectureFlat32(sc, "x86-protected-32");
   var mem = new ByteMemoryArea(Address.Ptr32(startAddress), bytes);
   ```

2. **Instruction Lifting to Reko RTL**
   ```csharp
   var rewriter = arch.CreateRewriter(mem.CreateLeReader(0));
   // Convert x86 → Reko's RTL representation
   ```

3. **Decompilation to High-Level Code**
   ```csharp
   // Use Reko's decompiler to generate C or high-level pseudocode
   // Convert output to C# format
   ```

4. **C# Code Generation**
   - Reko outputs C code by default
   - Need converter: Reko C output → C# method bodies
   - Preserve control flow, types, variable names

## Why This Design?

### Benefits

1. **Legal Compliance**
   - Win32Emu stays MIT-licensed
   - Users make informed choice about GPL
   - No viral license contamination

2. **Flexibility**
   - Easy to add more decompiler backends (Ghidra, IDA, etc.)
   - Users can implement custom adapters
   - A/B testing of decompiler quality

3. **Backward Compatibility**
   - Existing code continues to work
   - Default behavior unchanged
   - No breaking changes

4. **Experimentation**
   - Can compare CustomRTL vs. Reko output quality
   - Performance benchmarking
   - Switch adapters at runtime

### Trade-offs

1. **Reko Integration Incomplete**
   - Full Reko API integration is complex
   - Reko outputs C, not C# natively
   - Requires manual C → C# conversion

2. **Maintenance Burden**
   - Two decompiler implementations to maintain
   - Interface versioning if adapters evolve
   - Testing across multiple adapters

3. **User Confusion**
   - Need clear documentation about licensing
   - Environment variable configuration
   - Debugging which adapter is being used

## Future Enhancements

### Short Term

1. **Complete Reko Integration**
   - Implement actual Reko API calls
   - C → C# converter
   - Test with real x86 binaries

2. **Adapter Factory**
   ```csharp
   public static class DecompilerAdapterFactory
   {
       public static IDecompilerAdapter Create(string name);
       public static IEnumerable<IDecompilerAdapter> GetAvailable();
   }
   ```

3. **Configuration File Support**
   ```json
   {
       "decompiler": {
           "adapter": "Reko",
           "options": { ... }
       }
   }
   ```

### Long Term

1. **More Adapters**
   - Ghidra adapter (via pyhidra or bridge)
   - IDA Pro adapter (via IDA SDK)
   - RetDec adapter
   - Binary Ninja adapter

2. **Quality Metrics**
   - Compare decompiler outputs
   - Measure readability scores
   - Performance benchmarks
   - Correctness testing

3. **Hybrid Approach**
   - Use Reko for complex functions
   - Use CustomRTL for simple blocks
   - Dynamic selection based on heuristics

4. **Decompiler Ensemble**
   - Run multiple decompilers
   - Vote on best output
   - Merge complementary results

## Testing

### Unit Tests

```csharp
[Fact]
public void CustomRtlAdapter_IsAlwaysAvailable()
{
    var adapter = new CustomRtlDecompilerAdapter();
    Assert.True(adapter.IsAvailable);
}

[Fact]
public void RekoAdapter_RequiresOptIn()
{
    Environment.SetEnvironmentVariable("WIN32EMU_USE_REKO", null);
    var adapter = new RekoDecompilerAdapter();
    Assert.False(adapter.IsAvailable);
}

[Fact]
public async Task Adapter_ProducesValidCSharp()
{
    var adapter = new CustomRtlDecompilerAdapter();
    var instructions = /* ... */;
    var csharp = await adapter.DecompileToCSharpAsync(0x401000, instructions, "TestClass");
    
    // Should compile without errors
    var compilation = CSharpCompilation.Create("test")
        .AddSyntaxTrees(CSharpSyntaxTree.ParseText(csharp));
    Assert.Empty(compilation.GetDiagnostics());
}
```

### Integration Tests

```bash
# Test with CustomRTL (default)
dotnet test Win32Emu.Tests.Emulator

# Test with Reko (if available)
WIN32EMU_USE_REKO=true dotnet test Win32Emu.Tests.Emulator
```

## Examples

### Example 1: Simple Function

**Input**: x86 instructions
```assembly
mov eax, 5
mov ebx, 3
add eax, ebx
ret
```

**CustomRTL Output**:
```csharp
public class JitBlock_00401000
{
    public async Task<dynamic> Execute(dynamic cpu, dynamic mem)
    {
        uint EAX = 5u;
        uint EBX = 3u;
        EAX = EAX + EBX;
        return new CpuStepResult { IsCall = false };
    }
}
```

**Reko Output** (future):
```csharp
public class JitBlock_00401000
{
    public async Task<dynamic> Execute(dynamic cpu, dynamic mem)
    {
        // Decompiled using Reko (GPLv2)
        return 8; // Optimized constant folding
    }
}
```

### Example 2: Adapter Comparison

```csharp
var customAdapter = new CustomRtlDecompilerAdapter(logger);
var rekoAdapter = new RekoDecompilerAdapter(logger);

var instructions = DisassembleFunction(0x401000);

var customCSharp = await customAdapter.DecompileToCSharpAsync(0x401000, instructions, "Test_Custom");
var rekoCSharp = rekoAdapter.IsAvailable
    ? await rekoAdapter.DecompileToCSharpAsync(0x401000, instructions, "Test_Reko")
    : null;

logger.LogInformation("CustomRTL code length: {Length}", customCSharp.Length);
logger.LogInformation("Reko code length: {Length}", rekoCSharp?.Length ?? 0);
```

## Documentation Updates

### README.md

Add section:
```markdown
## Decompiler Backends

Win32Emu supports pluggable decompiler backends for JIT compilation:

- **CustomRTL** (default, MIT): Built-in decompiler using Win32Emu's RTL pipeline
- **Reko** (opt-in, GPLv2): Industry-standard decompiler for higher quality output

To use Reko:
1. Install: `dotnet add package Reko.Decompiler.Runtime`
2. Enable: `export WIN32EMU_USE_REKO=true`
3. Note: Reko is GPLv2-licensed

See `docs/implementation/REKO_INTEGRATION_GUIDE.md` for details.
```

### CLI Help

```
--decompiler <name>   Select decompiler backend (CustomRTL, Reko)
                      Default: CustomRTL
                      Note: Reko requires GPLv2 compliance
```

## Conclusion

This implementation provides a **clean, legally-safe way** to integrate Reko.Decompiler.Runtime into Win32Emu's JIT pipeline while:

1. ✅ Maintaining MIT licensing for Win32Emu
2. ✅ Allowing users to opt-in to Reko
3. ✅ Providing a pluggable architecture for future decompilers
4. ✅ Keeping existing functionality intact
5. ⚠️ Requiring additional work to complete Reko API integration

The foundation is in place - next step is to implement the actual Reko decompilation logic in `RekoDecompilerAdapter.GenerateRekoIntegrationStubAsync()`.

## References

- [Reko Decompiler](https://github.com/uxmal/reko)
- [Reko.Decompiler.Runtime NuGet](https://www.nuget.org/packages/Reko.Decompiler.Runtime)
- [Reko API Guide](https://github.com/uxmal/reko/blob/master/doc/guide/api.md)
- [GPLv2 License](https://www.gnu.org/licenses/old-licenses/gpl-2.0.en.html)
- [MIT License](https://opensource.org/licenses/MIT)

---

**Status**: ✅ Foundation Complete, 🚧 Reko Integration Pending  
**Date**: January 14, 2026  
**Author**: GitHub Copilot Agent
