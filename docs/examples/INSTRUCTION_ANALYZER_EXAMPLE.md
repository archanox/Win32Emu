# Instruction Analyzer Example

This example demonstrates how to use the new `InstructionAnalyzer` class to get detailed information about x86 instructions.

## Features

The `InstructionAnalyzer` provides:

1. **Instruction Formatting** - Convert instructions to human-readable assembly format
2. **Register Analysis** - Identify which registers are read and written
3. **Memory Access Analysis** - Detect memory reads and writes with addressing details
4. **Virtual Address Calculation** - Calculate effective addresses for memory operands

## Basic Usage

### Enable Instruction Analyzer

```csharp
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;

// Create CPU with instruction analyzer enabled
var memory = new VirtualMemory(0x10000000);
var cpu = new IcedCpu(memory, logger: null, 
    decoderOptions: DecoderOptions.None, 
    enableInstructionAnalyzer: true);
```

### Format Current Instruction

```csharp
// Get formatted instruction at current EIP
string formatted = cpu.FormatCurrentInstruction();
Console.WriteLine(formatted);
// Output: 00401000 mov eax, [ebp+8]
```

### Analyze Instruction

```csharp
// Get detailed analysis
var analysis = cpu.AnalyzeCurrentInstruction();
if (analysis != null)
{
    Console.WriteLine($"Instruction: {analysis.FormattedInstruction}");
    Console.WriteLine($"Mnemonic: {analysis.Mnemonic}");
    Console.WriteLine($"Length: {analysis.Length} bytes");
    
    Console.WriteLine($"Reads: {string.Join(", ", analysis.ReadRegisters)}");
    Console.WriteLine($"Writes: {string.Join(", ", analysis.WrittenRegisters)}");
    
    foreach (var mem in analysis.MemoryAccesses)
    {
        Console.WriteLine($"Memory {mem.Access}: [{mem.Base}+{mem.Displacement:X}]");
    }
}
```

### Using the Instruction Analyzer Directly

```csharp
var analyzer = cpu.GetInstructionAnalyzer();
if (analyzer != null)
{
    // Decode an instruction
    var decoder = Decoder.Create(32, new byte[] { 0x8B, 0x45, 0x08 }); // mov eax, [ebp+8]
    decoder.IP = 0x00401000;
    var instruction = decoder.Decode();
    
    // Format it
    string formatted = analyzer.FormatInstructionWithAddress(instruction);
    
    // Analyze it
    var analysis = analyzer.AnalyzeInstruction(instruction);
}
```

## Decoder Options

The `IcedCpu` constructor now accepts `DecoderOptions` to enable decoding of old or deprecated instructions:

```csharp
// Enable decoding of old Cyrix, Centaur, and MPX instructions
var decoderOptions = DecoderOptions.MPX | 
                     DecoderOptions.MovTr | 
                     DecoderOptions.Cyrix | 
                     DecoderOptions.Cyrix_DMI | 
                     DecoderOptions.ALTINST;

var cpu = new IcedCpu(memory, logger: null, decoderOptions);
```

This is useful for emulating legacy software that uses deprecated CPU instructions.

## Example Output

```
Instruction: mov       [ebp+8],eax
Mnemonic: Mov
Length: 3 bytes
Reads: EAX, EBP
Writes: 
Memory Write: [EBP+8]
```

## Integration with Debugging

The instruction analyzer integrates well with the existing debugging infrastructure:

```csharp
var debugger = new EnhancedCpuDebugger(cpu, memory);

// Before stepping
var analysis = cpu.AnalyzeCurrentInstruction();
if (analysis != null)
{
    Console.WriteLine($"About to execute: {analysis.FormattedInstruction}");
    Console.WriteLine($"Will modify: {string.Join(", ", analysis.WrittenRegisters)}");
}

// Execute
cpu.SingleStep(memory);
```
