# Unicorn Engine CPU Conformance Tests

## Overview

The `UnicornConformanceTests` class provides comprehensive validation of the Win32Emu CPU emulator by comparing its behavior against the Unicorn Engine, a well-established CPU emulator used as a reference implementation.

## Purpose

These tests ensure that:
1. Win32Emu correctly executes x86 32-bit instructions
2. Register values are calculated correctly
3. CPU flags (EFLAGS) are set appropriately
4. Memory operations work as expected

## Test Coverage

The test suite covers the following instruction categories:

### Arithmetic Instructions
- ADD, SUB (with overflow/underflow)
- INC, DEC
- IMUL, DIV (signed/unsigned multiply and divide)
- ADC, SBB (add/subtract with carry/borrow)
- NEG (two's complement negation)

### Logical Instructions
- AND, OR, XOR
- NOT (bitwise NOT)
- TEST (non-destructive AND)

### Bit Manipulation
- SHL, SHR (logical shifts)
- SAR (arithmetic shift right)
- ROL, ROR (rotates)

### Comparison
- CMP (comparison)
- TEST (non-destructive AND for flags)

### Data Movement
- MOV (register-to-register and immediate)
- PUSH, POP (stack operations)

### Sign Extension
- CDQ (sign-extend EAX into EDX:EAX)

## Known Differences

### Parity Flag (PF)
The Parity Flag indicates whether the least significant byte of the result has an even number of set bits. There are known subtle differences in PF calculation between Win32Emu and Unicorn for some instructions. This is acceptable as:
- PF is rarely used in modern x86 code
- Both implementations follow x86 specification for the primary use cases
- The differences are in edge cases and complex instruction combinations

### Overflow Flag (OF) in Rotate Instructions
ROR/ROL instructions may set the Overflow Flag differently in edge cases. This is documented behavior in x86 processors where OF is "undefined" for multi-bit rotates.

## Usage

### Running Tests

```bash
# Run all conformance tests
dotnet test --filter "FullyQualifiedName~UnicornConformanceTests"

# Run specific instruction category
dotnet test --filter "FullyQualifiedName~UnicornConformanceTests.ADD"
```

### Adding New Tests

To add new conformance tests:

1. Use the `UnicornTestHelper` class to set up both emulators
2. Write machine code bytes using `WriteCode()`
3. Set initial register state with `SetReg()`
4. Execute with `ExecuteInstruction()`
5. Assert results match using `AssertRegistersMatch()` and `AssertFlagsMatch()`

Example:
```csharp
[Fact]
public void MyNewInstruction_ShouldMatchUnicorn()
{
    // Arrange
    _helper.SetReg("EAX", 0x00000005);
    _helper.WriteCode(0x40); // INC EAX

    // Act
    _helper.ExecuteInstruction();

    // Assert
    _helper.AssertRegistersMatch("EAX");
    _helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of);
}
```

## Dependencies

- **UnicornEngine.Unicorn** (v2.1.4-a40db6c): Reference CPU emulator
- **xUnit**: Test framework

## References

- [Unicorn Engine](https://www.unicorn-engine.org/)
- [x86 Instruction Set Reference](https://www.felixcloutier.com/x86/)
- [Intel x86 Architecture Manual](https://www.intel.com/content/www/us/en/developer/articles/technical/intel-sdm.html)
