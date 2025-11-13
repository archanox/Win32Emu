# SingleStepTests/80386 CPU Conformance Tests

This directory contains infrastructure for running hardware-generated CPU conformance tests from the [SingleStepTests/80386](https://github.com/SingleStepTests/80386) repository.

## Overview

The SingleStepTests/80386 project provides hardware-generated CPU test cases created by running instructions on real 80386 hardware and capturing the exact CPU state before and after execution. These tests provide the highest level of accuracy for validating CPU emulation.

## Components

### MooFileParser.cs
Parses MOO (Moolah) binary test files from the SingleStepTests repository. The parser handles:
- Gzipped and uncompressed MOO files
- Test case extraction
- Initial and final CPU state parsing
- Memory state parsing
- Instruction bytes extraction

### SingleStepTestRunner.cs
Executes test cases against Win32Emu's CPU implementation:
- Applies initial CPU and memory state
- Executes the instruction
- Validates final CPU and memory state
- Reports mismatches in registers and memory

### SingleStepConformanceTests.cs
xUnit test class that integrates the test runner:
- Discovers test files in the TestData directory
- Runs conformance tests
- Reports results with detailed diagnostics

## Usage

### Downloading Test Files

1. Clone or download test files from https://github.com/SingleStepTests/80386
2. Extract the gzipped test files from the `v1_ex_real_mode` directory
3. Place them in `Win32Emu.Tests.Emulator/TestData/SingleStepTests/`

For example:
```bash
cd Win32Emu.Tests.Emulator
mkdir -p TestData/SingleStepTests
cd TestData/SingleStepTests
curl -L -O https://github.com/SingleStepTests/80386/raw/refs/heads/main/v1_ex_real_mode/00.MOO.gz
curl -L -O https://github.com/SingleStepTests/80386/raw/refs/heads/main/v1_ex_real_mode/01.MOO.gz
# Download more test files as needed...
```

### Running Tests

Once test files are in place, run the tests using xUnit:

```bash
# Run all SingleStep conformance tests
dotnet test --filter "FullyQualifiedName~SingleStepConformanceTests"

# Run a specific test file
dotnet test --filter "FullyQualifiedName~SingleStepConformanceTests.CPU_ShouldPassHardwareTests"
```

### Test File Format

Each MOO file contains approximately 2,500 test cases for a specific opcode. The files are named by opcode (e.g., `00.MOO.gz` for ADD instruction with opcode 0x00).

Test files include:
- **Initial State**: Register values, memory contents, instruction pointer
- **Instruction Bytes**: The exact bytes of the instruction to execute
- **Final State**: Expected register values and memory contents after execution
- **Cycle Information**: Bus cycles executed (optional for validation)

## Current Status

The test infrastructure is complete and ready to use. However, there is a pre-existing build issue in the main Win32Emu project (related to the DSBCAPS source generator) that prevents the tests from being compiled and run currently.

Once the main project builds successfully, these tests can be run by:
1. Downloading test files as described above
2. Running `dotnet test` on the test project

## Test Coverage

The SingleStepTests/80386 repository provides tests for:
- All x86 instructions up to 386
- Real mode execution
- Various addressing modes
- Edge cases and flag interactions
- Memory access patterns

Each test file covers one opcode with multiple variations, providing comprehensive coverage of instruction behavior.

## Contributing

To add more test coverage:
1. Download additional MOO files from the SingleStepTests repository
2. Place them in the TestData/SingleStepTests directory
3. Add new test cases in SingleStepConformanceTests.cs using the `[Theory]` attribute

Example:
```csharp
[Theory]
[InlineData("02.MOO.gz", 10)] // ADD r8, r/m8
[InlineData("03.MOO.gz", 10)] // ADD r16/32, r/m16/32
public void CPU_ShouldPassHardwareTests(string fileName, int maxTests)
{
    // Test implementation...
}
```

## References

- [SingleStepTests/80386 Repository](https://github.com/SingleStepTests/80386)
- [SingleStepTests/80386 README](https://github.com/SingleStepTests/80386/blob/main/README.md)
- [Test Methodology](https://github.com/SingleStepTests/80386/blob/main/README.md#test-methodology)
