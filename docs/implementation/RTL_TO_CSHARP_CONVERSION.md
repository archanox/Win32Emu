# RTL to C# Conversion Implementation

## Overview

This document describes the implementation of RTL (Register Transfer Language) to C# code conversion in the RekoDecompilerAdapter.

## Implementation Details

### Architecture

The conversion process follows these steps:

1. **Collect RTL Clusters** - Reko's rewriter produces `RtlInstructionCluster` objects
2. **Parse RTL Instructions** - Extract individual RTL instructions from each cluster
3. **Convert to C#** - Transform RTL operations into executable C# code
4. **Generate Method** - Create a complete C# method with CPU register handling

### Code Generation

#### Register Initialization

```csharp
// Initialize CPU register state
uint eax = cpu.EAX;
uint ebx = cpu.EBX;
uint ecx = cpu.ECX;
uint edx = cpu.EDX;
// ... more registers
```

#### RTL Instruction Conversion

The `ConvertRtlInstructionToCSharp` method handles various RTL patterns:

**Assignment Operations**:
```
RTL: eax = Mem0[esp:word32]
C#:  eax = mem.Read32(esp);

RTL: esp = esp + 0x00000004<32>
C#:  esp = esp + 0x00000004u;

RTL: Mem0[esp:word32] = eax
C#:  mem.Write32(esp, eax);
```

**Memory Access Patterns**:
- `Mem0[addr:word32]` → `mem.Read32(addr)`
- `Mem0[addr:word16]` → `mem.Read16(addr)`
- `Mem0[addr:byte]` → `mem.Read8(addr)`

**Constants**:
- `0x00000004<32>` → `0x00000004u`
- `0x0001<16>` → `0x0001u`

**Arithmetic Operations**:
- `a + b` → `a + b`
- `a - b` → `a - b`
- `a * b` → `a * b`
- `a / b` → `a / b`

#### Register Writeback

```csharp
// Write back CPU register state
cpu.EAX = eax;
cpu.EBX = ebx;
cpu.ECX = ecx;
// ... more registers
cpu.CF = CF;  // Carry flag
cpu.ZF = ZF;  // Zero flag
// ... more flags
```

### Supported RTL Patterns

| RTL Pattern | C# Translation | Notes |
|-------------|----------------|-------|
| `dst = src` | `dst = src;` | Simple assignment |
| `Mem0[addr:word32]` | `mem.Read32(addr)` | Memory read |
| `Mem0[addr:word32] = val` | `mem.Write32(addr, val)` | Memory write |
| `0xNNNN<32>` | `0xNNNNu` | Constant value |
| `branch target (cond)` | `// Control flow: ...` | Branch (comment) |
| `call target` | `// Function call: ...` | Call (comment) |
| `return` | `// return;` | Return (comment) |

### Limitations

**Current Implementation**:
- ✅ Basic assignments
- ✅ Memory read/write operations
- ✅ Register operations
- ✅ Arithmetic operations
- ✅ Constants
- ⚠️ Control flow (as comments)
- ⚠️ Function calls (as comments)
- ⚠️ Complex expressions (partial support)

**Not Yet Implemented**:
- Proper control flow (if/while/for)
- Function calls with proper stack handling
- Complex boolean expressions
- Type conversions
- Bitwise operations (partially supported)

### Example Output

**Input x86**:
```assembly
mov eax, 5
add eax, 3
mov [esp], eax
```

**Generated C#**:
```csharp
using System;
using System.Threading.Tasks;

namespace Win32Emu.Generated
{
    // Decompiled using Reko (GPLv2)
    public class JitBlock_abc12345_00401000
    {
        // Block at 0x00401000
        // Reko RTL clusters: 3
        
        public async Task<dynamic> Execute(dynamic cpu, dynamic mem)
        {
            // Initialize CPU register state
            uint eax = cpu.EAX;
            uint ebx = cpu.EBX;
            uint ecx = cpu.ECX;
            uint edx = cpu.EDX;
            uint esi = cpu.ESI;
            uint edi = cpu.EDI;
            uint esp = cpu.ESP;
            uint ebp = cpu.EBP;
            uint eip = cpu.EIP;
            bool CF = cpu.CF;
            bool ZF = cpu.ZF;
            bool SF = cpu.SF;
            bool OF = cpu.OF;
            bool PF = cpu.PF;
            
            // RTL Cluster 0
            eax = 0x00000005u;
            
            // RTL Cluster 1
            eax = eax + 0x00000003u;
            
            // RTL Cluster 2
            mem.Write32(esp, eax);
            
            // Write back CPU register state
            cpu.EAX = eax;
            cpu.EBX = ebx;
            cpu.ECX = ecx;
            cpu.EDX = edx;
            cpu.ESI = esi;
            cpu.EDI = edi;
            cpu.ESP = esp;
            cpu.EBP = ebp;
            cpu.EIP = eip;
            cpu.CF = CF;
            cpu.ZF = ZF;
            cpu.SF = SF;
            cpu.OF = OF;
            cpu.PF = PF;
            
            return await Task.FromResult<dynamic>(new { IsCall = false });
        }
    }
}
```

## Usage

The RTL to C# conversion is automatic when using the Reko adapter:

```bash
export WIN32EMU_USE_REKO=true
dotnet run --project Win32Emu.Gui -- --nogui game.exe
```

Generated C# files are saved to the JIT cache for inspection and debugging.

## Future Enhancements

### Phase 1: Control Flow (High Priority)
- Convert `branch` instructions to `if` statements
- Handle loops with `while`/`for`
- Implement `goto` for complex control flow

### Phase 2: Advanced Operations (Medium Priority)
- Bitwise operations (AND, OR, XOR, shifts)
- Type conversions (sign extension, zero extension)
- Complex expressions (nested operations)
- Flag calculations (CF, ZF, SF, OF, PF)

### Phase 3: Function Calls (Medium Priority)
- Stack frame management
- Parameter passing
- Return value handling
- Calling conventions (cdecl, stdcall, fastcall)

### Phase 4: Optimization (Low Priority)
- Dead code elimination
- Constant folding
- Common subexpression elimination
- Register allocation optimization

## Technical Details

### Reflection Usage

The implementation uses minimal reflection to access RTL cluster properties:

```csharp
var instructionsProperty = clusterType.GetProperty("Instructions");
var instructions = instructionsProperty.GetValue(cluster) as IEnumerable;
```

This is necessary because Reko's RTL types are not directly exposed in the public API.

### Pattern Matching

The converter uses simple string pattern matching and regex for parsing:

```csharp
// Assignment pattern
if (instrString.Contains(" = "))
{
    var parts = instrString.Split(new[] { " = " }, ...);
    // Process assignment
}

// Memory access pattern
var match = Regex.Match(operand, @"Mem\d+\[([^:]+):(\w+)\]");
```

### Error Handling

Unrecognized RTL patterns are converted to comments:

```csharp
// Fallback: add as comment for manual review
return $"// {instrString}";
```

This ensures the generated code always compiles, even if some operations aren't fully supported yet.

## Debugging

To debug RTL conversion:

1. Set `WIN32EMU_USE_REKO=true`
2. Run the emulator
3. Check generated C# files in JIT cache directory
4. Review comments for unsupported operations
5. Examine RTL patterns in log output

## References

- [Reko RTL Documentation](https://github-wiki-see.page/m/uxmal/reko/wiki/RTL)
- [Reko Rewriter Guide](https://github-wiki-see.page/m/uxmal/reko/wiki/Rewriter)
- [Win32Emu JIT Architecture](JIT_CACHE_IMPLEMENTATION.md)

---

**Status**: ✅ Basic implementation complete  
**Date**: January 14, 2026  
**Next Steps**: Control flow reconstruction, advanced operations
