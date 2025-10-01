# CPU Intrinsics Support

This document describes the CPU intrinsics support in Win32Emu for hardware-accelerated instruction emulation.

## Overview

Win32Emu now leverages .NET's `System.Runtime.Intrinsics` API to provide hardware-accelerated emulation of x86 SIMD and extended instructions on both x86 and ARM host systems. This enables:

- **Better Performance**: Native CPU instructions are used when available instead of software emulation
- **Cross-Platform**: The same emulated x86 code can run efficiently on both x86 and ARM hosts
- **Modern CPU Features**: Support for SSE, SSE2, SSE3, SSSE3, SSE4.1, SSE4.2, AVX, AVX2, and more

## Architecture Detection

The `CpuIntrinsics` class automatically detects the host CPU architecture and available features:

```csharp
// Architecture detection
if (CpuIntrinsics.IsX86)
{
    // Running on x86/x64 host
}
else if (CpuIntrinsics.IsArm)
{
    // Running on ARM/ARM64 host
}

// Feature detection
if (CpuIntrinsics.HasSse)
{
    // SSE instructions are available on x86 host
}
if (CpuIntrinsics.HasAdvSimd)
{
    // NEON instructions are available on ARM host
}
```

### Supported Features

#### x86/x64 Features
- **SSE**: Streaming SIMD Extensions
- **SSE2**: Streaming SIMD Extensions 2
- **SSE3**: Streaming SIMD Extensions 3
- **SSSE3**: Supplemental Streaming SIMD Extensions 3
- **SSE4.1**: Streaming SIMD Extensions 4.1
- **SSE4.2**: Streaming SIMD Extensions 4.2
- **AVX**: Advanced Vector Extensions
- **AVX2**: Advanced Vector Extensions 2
- **AES**: AES New Instructions
- **PCLMULQDQ**: Carry-Less Multiplication
- **POPCNT**: Population Count
- **LZCNT**: Leading Zero Count
- **BMI1**: Bit Manipulation Instructions 1
- **BMI2**: Bit Manipulation Instructions 2
- **FMA**: Fused Multiply-Add

#### ARM Features
- **ArmBase**: Base ARM64 instructions
- **AdvSimd**: ARM Advanced SIMD (NEON)
- **AES**: AES encryption instructions
- **CRC32**: CRC32 calculation instructions
- **SHA1/SHA256**: Cryptographic hash instructions

## CPUID Emulation

The CPUID instruction now returns accurate feature flags based on the host CPU capabilities:

### Function 0: Get Vendor String
- Returns "GenuineIntel" vendor string
- EAX contains max supported function (7)

### Function 1: Get Feature Flags
- **EAX**: Processor signature (Family 6, Model 0, Stepping 0)
- **EBX**: Brand/cache information
- **ECX**: Extended feature flags (based on host CPU)
  - Bit 0: SSE3
  - Bit 1: PCLMULQDQ
  - Bit 9: SSSE3
  - Bit 12: FMA
  - Bit 19: SSE4.1
  - Bit 20: SSE4.2
  - Bit 23: POPCNT
  - Bit 25: AES
  - Bit 28: AVX
- **EDX**: Standard feature flags (based on host CPU)
  - Bit 0: FPU (always set)
  - Bit 4: TSC/RDTSC (always set)
  - Bit 5: MSR (always set)
  - Bit 8: CMPXCHG8B (always set)
  - Bit 15: CMOV (always set)
  - Bit 25: SSE
  - Bit 26: SSE2

### Function 7, Sub-function 0: Extended Features
- **EAX**: Max sub-function (0)
- **EBX**: Extended feature flags (based on host CPU)
  - Bit 3: BMI1
  - Bit 5: AVX2
  - Bit 8: BMI2

## SIMD Intrinsics Helper

The `SimdIntrinsicsHelper` class provides hardware-accelerated implementations of common SIMD operations that can be used when implementing SSE/SSE2 instructions:

### Example Usage

```csharp
// Add four single-precision floats (ADDPS)
var result = SimdIntrinsicsHelper.AddPackedSingle(operand1, operand2);

// Multiply four single-precision floats (MULPS)
var result = SimdIntrinsicsHelper.MultiplyPackedSingle(operand1, operand2);

// Add two double-precision floats (ADDPD)
var result = SimdIntrinsicsHelper.AddPackedDouble(operand1, operand2);

// Add 16 bytes (PADDB)
var result = SimdIntrinsicsHelper.AddPackedBytes(operand1, operand2);

// Population count (POPCNT)
uint setBits = SimdIntrinsicsHelper.PopCount(value);

// Leading zero count (LZCNT)
uint leadingZeros = SimdIntrinsicsHelper.LeadingZeroCount(value);

// CRC32 calculation (CRC32 from SSE4.2)
uint crc = SimdIntrinsicsHelper.Crc32C(previousCrc, dataByte);
```

### Hardware Acceleration

The helper automatically selects the best implementation:

1. **x86 hosts**: Uses x86 intrinsics (SSE, SSE2, etc.)
2. **ARM hosts**: Uses ARM NEON intrinsics (AdvSimd)
3. **Fallback**: Software implementation when intrinsics aren't available

All implementations produce identical results, ensuring emulation accuracy regardless of host platform.

## Benefits

### Performance

- **Native Speed**: SIMD operations run at native hardware speed instead of being emulated in software
- **Zero Overhead**: Direct mapping from emulated SSE instructions to host SIMD instructions
- **Scalability**: Newer host CPUs automatically provide better performance

### Compatibility

- **Cross-Platform**: Same emulated x86 code runs on both x86 and ARM hosts
- **Future-Proof**: New CPU features can be easily added as they become available in .NET
- **Graceful Degradation**: Software fallbacks ensure compatibility on older CPUs

### Accuracy

- **Correct Results**: All implementations produce bit-identical results
- **Proper CPUID**: Emulated software can query actual host capabilities
- **Testing**: Comprehensive test suite validates all implementations

## Implementation Details

### Adding New Instructions

To add support for a new SIMD instruction:

1. Add feature detection in `CpuIntrinsics.cs` if needed
2. Add the instruction case in `IcedCpu.cs` SingleStep method
3. Implement the instruction execution method in `IcedCpu.cs`
4. Use `SimdIntrinsicsHelper` methods or add new ones as needed
5. Add tests in `Win32Emu.Tests.Emulator`

Example:
```csharp
case Mnemonic.Addps:
    ExecAddps(insn);
    break;

private void ExecAddps(Instruction insn)
{
    // Read 16 bytes from source operand
    var src = ReadOperand16Bytes(insn, 1);
    var dst = ReadOperand16Bytes(insn, 0);
    
    // Use hardware acceleration
    var result = SimdIntrinsicsHelper.AddPackedSingle(dst, src);
    
    // Write back result
    WriteOperand16Bytes(insn, 0, result);
}
```

## Testing

All intrinsics support is thoroughly tested:

- **CpuIntrinsicsTests**: Validates architecture detection and CPUID reporting
- **SimdIntrinsicsHelperTests**: Tests all SIMD helper functions
- **Cross-Platform**: Tests run on both x86 and ARM in CI/CD

Run tests with:
```bash
dotnet test Win32Emu.sln
```

## References

- [.NET x86 Intrinsics API](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.x86)
- [.NET ARM Intrinsics API](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.arm)
- [Intel SSE/AVX Instructions Reference](https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html)
- [ARM NEON Intrinsics Reference](https://developer.arm.com/architectures/instruction-sets/intrinsics/)

## Future Enhancements

Potential improvements for the intrinsics support:

1. **More Instructions**: Add support for additional SSE/AVX instructions as needed
2. **XMM Register Storage**: Store XMM registers for proper state management
3. **MXCSR Support**: Implement SSE control/status register
4. **AVX 256-bit**: Support wider vector operations
5. **Instruction Tracing**: Optional logging of SIMD instruction execution
