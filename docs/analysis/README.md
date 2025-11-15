# CPU Emulation Documentation

This directory contains documentation related to Win32Emu's x86 CPU emulation implementation.

## Documents

### Analysis Documents

#### [REKO_X86EMULATOR_ANALYSIS.md](REKO_X86EMULATOR_ANALYSIS.md)
Comprehensive analysis comparing Win32Emu's IcedCpu implementation with Reko's X86Emulator. Includes:
- Side-by-side comparison of implementations
- Flag calculation algorithms
- REP prefix handling patterns
- Register and memory access patterns
- Detailed recommendations with priority levels

**Key Takeaway:** Win32Emu's emulator is already well-designed and more sophisticated than Reko's implementation.

#### [REKO_REVIEW_SUMMARY.md](REKO_REVIEW_SUMMARY.md)
Executive summary of the Reko X86Emulator review. Provides:
- TL;DR verdict and outcome
- Comparative analysis table
- Key findings and insights
- Changes made and validation results
- Future recommendations

**Key Takeaway:** No architectural changes needed. Documentation improvements were the main outcome.

### Reference Guides

#### [CPU Flag Calculations Guide](../guides/CPU_FLAG_CALCULATIONS.md)
Quick reference for x86 CPU flag calculations. Includes:
- Visual flag register layout
- Detailed explanation of each flag (CF, OF, ZF, SF, PF, AF)
- XOR-based overflow detection explained
- Parity lookup table (0x6996 magic constant) explained
- Common pitfalls and test patterns
- Quick reference formulas

**Use Case:** Reference when implementing or debugging CPU instructions

## Background

This documentation was created as part of a review of the [Reko X86Emulator](https://github.com/uxmal/reko/blob/master/src/Arch/X86/Emulator/X86Emulator.cs) to identify potential improvements for Win32Emu. The review validated that Win32Emu's implementation is correct and follows industry best practices.

## Related Files

### Source Code
- `Win32Emu/Cpu/Iced/IcedCpu.cs` - Main CPU emulator implementation
- `Win32Emu/Cpu/ICpu.cs` - CPU interface
- `Win32Emu/Cpu/CpuState.cs` - CPU state structure

### Tests
- `Win32Emu.Tests.Emulator/` - CPU emulator tests
- `Win32Emu.Tests.Emulator/SingleStepTests/` - Single-step conformance tests

## Key Concepts

### Flag Calculations
CPU flags reflect the results of arithmetic and logical operations:
- **CF** (Carry) - Unsigned overflow/underflow
- **OF** (Overflow) - Signed overflow
- **ZF** (Zero) - Result is zero
- **SF** (Sign) - Result is negative
- **PF** (Parity) - Even parity of low byte
- **AF** (Auxiliary) - Carry from bit 3 to 4 (BCD)

### XOR-Based Overflow Detection
Both ADD and SUB use XOR operations to detect signed overflow:
```csharp
// ADD: Operands same sign, result different
OF = (~(a ^ b) & (a ^ r)) & sign_bit

// SUB: Operands different signs, result unexpected
OF = ((a ^ b) & (a ^ r)) & sign_bit
```

### Parity Calculation
Uses lookup table encoded in magic constant 0x6996:
```csharp
bits = low_byte ^ (low_byte >> 4) & 0xF
PF = ((0x6996 >> bits) & 1) == 0
```

## Architecture Validation

The review confirmed that Win32Emu's architecture is sound:
- ✅ Flag calculations are correct
- ✅ Register access patterns are optimal for performance
- ✅ Memory abstraction supports advanced features
- ✅ Instruction coverage is comprehensive
- ✅ Performance optimizations are appropriate

## Future Enhancements

Potential improvements identified (low priority):
1. Add unit tests specifically for flag calculations
2. Consider instruction tracing hooks for debugging
3. Profile critical paths for optimization opportunities
4. Extract REP logic if adding many more string instructions

## References

- [Intel® 64 and IA-32 Architectures Software Developer's Manual](https://www.intel.com/content/www/us/en/developer/articles/technical/intel-sdm.html)
- [Reko Decompiler](https://github.com/uxmal/reko)
- [Win32Emu Repository](https://github.com/archanox/Win32Emu)

## Contributing

When modifying CPU emulation code:
1. Refer to the CPU Flag Calculations Guide
2. Maintain flag calculation correctness
3. Add XML documentation comments for complex algorithms
4. Include references to Intel SDM when applicable
5. Test with conformance tests in `Win32Emu.Tests.Emulator`

## Questions?

For questions about:
- **Flag calculations:** See [CPU_FLAG_CALCULATIONS.md](../guides/CPU_FLAG_CALCULATIONS.md)
- **Architecture decisions:** See [REKO_X86EMULATOR_ANALYSIS.md](REKO_X86EMULATOR_ANALYSIS.md)
- **Review findings:** See [REKO_REVIEW_SUMMARY.md](REKO_REVIEW_SUMMARY.md)
