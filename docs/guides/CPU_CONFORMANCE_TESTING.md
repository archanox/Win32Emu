# CPU Conformance Testing with SingleStepTests/80386

## Quick Start

This implementation adds support for running hardware-generated CPU conformance tests from the [SingleStepTests/80386](https://github.com/SingleStepTests/80386) repository.

### Prerequisites

1. The main Win32Emu project must build successfully (currently blocked by DSBCAPS generator issue)
2. Test files from SingleStepTests/80386 repository

### Setup

```bash
# 1. Download test files
cd Win32Emu.Tests.Emulator
mkdir -p TestData/SingleStepTests
cd TestData/SingleStepTests

# 2. Download MOO test files (examples)
curl -L -O https://github.com/SingleStepTests/80386/raw/main/v1_ex_real_mode/00.MOO.gz  # ADD
curl -L -O https://github.com/SingleStepTests/80386/raw/main/v1_ex_real_mode/01.MOO.gz  # ADD
curl -L -O https://github.com/SingleStepTests/80386/raw/main/v1_ex_real_mode/08.MOO.gz  # OR
curl -L -O https://github.com/SingleStepTests/80386/raw/main/v1_ex_real_mode/09.MOO.gz  # OR

# 3. Return to solution root
cd ../../../

# 4. Run tests
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~SingleStepConformanceTests"
```

### Architecture

```
┌─────────────────────────────────┐
│ SingleStepConformanceTests      │  (xUnit tests)
│  - Test file discovery          │
│  - Test execution orchestration │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│ SingleStepTestRunner            │  (Test execution)
│  - Apply initial state          │
│  - Execute instruction          │
│  - Validate final state         │
│  - Report mismatches            │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│ MooFileParser                   │  (File parsing)
│  - Parse binary MOO format      │
│  - Extract test cases           │
│  - Decompress gzip files        │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│ IcedCpu + VirtualMemory         │  (Win32Emu CPU)
│  - x86 instruction execution    │
│  - Register & memory management │
└─────────────────────────────────┘
```

### What Gets Tested

Each test case validates:
- ✅ All general-purpose registers (EAX, EBX, ECX, EDX, ESI, EDI, EBP, ESP)
- ✅ Instruction pointer (EIP)
- ✅ CPU flags (EFLAGS)
- ✅ Memory state (byte-by-byte comparison)
- ✅ Instruction execution correctness

### Example Output

```
Test 1: add [ss:bp+60h],bl
  PASS: All registers and memory match

Test 2: add [cs:bp+di+4Eh],cl  
  FAIL: Register mismatch - EAX(expected=0x00001234, actual=0x00001235)
  FAIL: Memory mismatch - @0x00400010(expected=0x42, actual=0x43)
```

### Test Coverage

- **2,500 test cases per opcode file**
- **Covers**: Real mode x86 instructions (8086-386)
- **Includes**: Edge cases, flag interactions, addressing modes
- **Quality**: Hardware-generated from real 80386 CPU

### Adding More Tests

Edit `SingleStepConformanceTests.cs`:

```csharp
[Theory]
[InlineData("00.MOO.gz", 100)]  // ADD r/m8, r8
[InlineData("08.MOO.gz", 100)]  // OR r/m8, r8
[InlineData("20.MOO.gz", 50)]   // AND r/m8, r8
public void CPU_ShouldPassHardwareTests(string fileName, int maxTests)
{
    // Test implementation...
}
```

### Troubleshooting

**Tests are skipped**: Test files not found in TestData/SingleStepTests directory

**Build fails**: Pre-existing DSBCAPS generator issue (unrelated to this feature)

**All tests fail**: CPU implementation may have bugs - analyze mismatch details

### References

- [SingleStepTests/80386 Repository](https://github.com/SingleStepTests/80386)
- [Test Methodology](https://github.com/SingleStepTests/80386/blob/main/README.md#test-methodology)
- [MOO File Format](Win32Emu.Tests.Emulator/SingleStepTests/README.md)
