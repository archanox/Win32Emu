# JitCpu Instruction Analysis

## Overview

JitCpu now supports instruction-level analysis in interpreter mode, providing the same debugging capabilities that were previously exclusive to IcedCpu. This enables developers to inspect and analyze x86 instructions during emulation without needing to maintain a separate CPU implementation.

## Background

As part of the IcedCpu deprecation plan (see [ICEDCPU_DEPRECATION.md](ICEDCPU_DEPRECATION.md)), we identified the need to maintain instruction analysis capabilities for debugging purposes. The solution was to implement these features in JitCpu's interpreter mode, which already had access to the Iced instruction decoder.

## Features

### FormatCurrentInstruction()

Formats the instruction at the current EIP (instruction pointer) with its address for debugging output.

**Example Output:**
```
00401000 mov eax, ebx
```

### AnalyzeCurrentInstruction()

Provides detailed analysis of the instruction at the current EIP, including:
- **Mnemonic**: The instruction name (e.g., "Mov", "Add", "Push")
- **Length**: Instruction length in bytes
- **OpCode**: String representation of the opcode
- **Read Registers**: List of registers read by the instruction
- **Written Registers**: List of registers written by the instruction
- **Memory Accesses**: Detailed information about memory operands including:
  - Segment register
  - Base register
  - Index register
  - Scale factor
  - Displacement
  - Access type (Read, Write, ReadWrite)

## Usage

### Enabling Instruction Analysis

To use instruction analysis in JitCpu, you must:
1. Enable the instruction analyzer: `enableInstructionAnalyzer: true`
2. Force interpreter mode: `forceInterpreterMode: true`

**Example:**
```csharp
var memory = new VirtualMemory(0x10000);
var cpu = new JitCpu(
    memory, 
    logger: null, 
    enableInstructionAnalyzer: true, 
    forceInterpreterMode: true
);

// Write some x86 code to memory
memory.Write8(0x1000, 0x89); // mov eax, ebx (opcode)
memory.Write8(0x1001, 0xD8); // ModR/M byte

// Set instruction pointer
cpu.SetEip(0x1000);

// Format the instruction
string formatted = cpu.FormatCurrentInstruction();
// Output: "00001000 mov eax, ebx"

// Analyze the instruction
var analysis = cpu.AnalyzeCurrentInstruction();
Console.WriteLine($"Mnemonic: {analysis.Mnemonic}");
Console.WriteLine($"Length: {analysis.Length} bytes");
Console.WriteLine($"Reads from: {string.Join(", ", analysis.ReadRegisters)}");
Console.WriteLine($"Writes to: {string.Join(", ", analysis.WrittenRegisters)}");
```

### Why Interpreter Mode?

JIT compilation converts x86 code into .NET IL bytecode, making instruction-level analysis impossible at runtime. Interpreter mode executes instructions one at a time using the Iced decoder, which provides access to instruction metadata needed for analysis.

**Performance Note**: Interpreter mode is slower than JIT mode. Only enable it when debugging or when instruction analysis is required.

## Use Cases

### Debugging

When debugging emulated x86 code, instruction analysis helps understand:
- What registers are being modified
- What memory locations are being accessed
- The sequence of instructions being executed

### Development Tools

Instruction analysis enables development of:
- Step-through debuggers
- Instruction tracers
- Code coverage analyzers
- Profiling tools

### Testing

The InstructionAnalyzerTests demonstrate how to use these features:
```csharp
[Fact]
public void InstructionAnalyzer_AnalyzesInstruction()
{
    // Arrange
    var memory = new VirtualMemory(0x10000);
    var cpu = new JitCpu(
        memory, 
        logger: null, 
        enableInstructionAnalyzer: true, 
        forceInterpreterMode: true
    );
    
    // Write: mov eax, ebx (89 D8)
    memory.Write8(0x1000, 0x89);
    memory.Write8(0x1001, 0xD8);
    
    cpu.SetEip(0x1000);
    
    // Act
    var analysis = cpu.AnalyzeCurrentInstruction();
    
    // Assert
    Assert.NotNull(analysis);
    Assert.Equal("Mov", analysis.Mnemonic);
    Assert.Equal(2, analysis.Length);
    Assert.Contains("EBX", analysis.ReadRegisters);
    Assert.Contains("EAX", analysis.WrittenRegisters);
}
```

## Implementation Details

### Architecture

The implementation leverages:
1. **Iced.Intel**: x86/x64 instruction decoder library
2. **InstructionAnalyzer**: Shared analyzer class used by both IcedCpu and JitCpu
3. **Decoder**: Per-CPU decoder instance configured for the target bitness

### Methods Added to JitCpu

```csharp
// Format instruction at current EIP with address
public string FormatCurrentInstruction()

// Analyze instruction at current EIP  
public InstructionAnalysis? AnalyzeCurrentInstruction()

// Helper to decode instruction at current EIP
private Instruction DecodeCurrentInstruction()
```

### InstructionAnalysis Data Structure

```csharp
public class InstructionAnalysis
{
    public string FormattedInstruction { get; set; }
    public ulong Address { get; set; }
    public int Length { get; set; }
    public string Mnemonic { get; set; }
    public string OpCodeString { get; set; }
    public List<string> ReadRegisters { get; set; }
    public List<string> WrittenRegisters { get; set; }
    public List<MemoryAccess> MemoryAccesses { get; set; }
}
```

## Migration from IcedCpu

If you're currently using IcedCpu for instruction analysis:

**Before:**
```csharp
var cpu = new IcedCpu(memory, enableInstructionAnalyzer: true);
var analysis = cpu.AnalyzeCurrentInstruction();
var formatted = cpu.FormatCurrentInstruction();
```

**After:**
```csharp
var cpu = new JitCpu(
    memory, 
    logger: null, 
    enableInstructionAnalyzer: true, 
    forceInterpreterMode: true
);
var analysis = cpu.AnalyzeCurrentInstruction();
var formatted = cpu.FormatCurrentInstruction();
```

### Breaking Changes

**FormatCurrentInstruction() behavior change:**
- **IcedCpu**: Returns string "Instruction analyzer not enabled" when analyzer is disabled
- **JitCpu**: Throws `InvalidOperationException` when analyzer is disabled

**Rationale**: This change makes JitCpu's API consistent with other analyzer-related methods (`FormatInstruction`, `AnalyzeInstruction`) which throw exceptions when the analyzer is not enabled. This provides better error detection at development time.

**Migration code:**
```csharp
// If you were relying on the string return value:
// Old IcedCpu code:
string result = cpu.FormatCurrentInstruction();
if (result == "Instruction analyzer not enabled") { /* handle */ }

// New JitCpu code:
try 
{
    string result = cpu.FormatCurrentInstruction();
    // use result
}
catch (InvalidOperationException)
{
    // Analyzer not enabled
}

// Or better - check analyzer availability first:
if (cpu.GetInstructionAnalyzer() != null)
{
    string result = cpu.FormatCurrentInstruction();
}
```

**AnalyzeCurrentInstruction() - No breaking change:**
- Both IcedCpu and JitCpu return `null` when analyzer is disabled
- This method maintains backward compatibility

## Limitations

1. **JIT Mode Not Supported**: Instruction analysis only works in interpreter mode
2. **Performance Impact**: Interpreter mode is slower than JIT compilation
3. **No Retroactive Analysis**: Can only analyze the current instruction, not previously executed ones
4. **Single Instruction**: Analysis is per-instruction, not per-block

## Future Enhancements

Potential improvements to consider:
- Instruction history/trace buffer
- Conditional breakpoint support based on register/memory values
- Integration with GDB server for remote debugging
- Export analysis data for external tools

## References

- Source: `Win32Emu/Cpu/Jit/JitCpu.cs`
- Tests: `Win32Emu.Tests.Emulator/InstructionAnalyzerTests.cs`
- Analyzer: `Win32Emu/Cpu/Iced/InstructionAnalyzer.cs`
- IcedCpu Deprecation: [ICEDCPU_DEPRECATION.md](ICEDCPU_DEPRECATION.md)

---

**Last Updated**: 2026-01-02
**Status**: Implemented and Tested
