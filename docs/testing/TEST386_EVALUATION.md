# test386.asm CPU Test Suite Evaluation

## Overview

This document evaluates the [test386.asm](https://github.com/barotto/test386.asm) CPU test suite and explains why it cannot be directly integrated into Win32Emu's current architecture, and what alternatives are available.

## What is test386.asm?

test386.asm is a comprehensive 80386+ CPU test suite designed to validate CPU emulators. It was originally developed for PCjs and later adapted for IBMulator.

### Key Features

- **Comprehensive Testing**: Tests arithmetic, logic, string operations, control flow, segment handling, and more
- **Real & Protected Mode**: Tests both real mode (16-bit) and protected mode (32-bit) execution
- **40+ Test Categories**: Each identified by a POST code (0x00-0xFF)
- **Hardware-level Testing**: Designed to run as a BIOS replacement in bare-metal environment
- **Diagnostic Output**: Reports status via POST port (typically 0x80) and serial/parallel ports

### Test Coverage

| POST Code | Description |
|-----------|-------------|
| 0x00 | Real mode initialization |
| 0x01 | Conditional jumps and loops |
| 0x02 | Unsigned 32-bit multiplication and division |
| 0x03 | Move segment registers in real mode |
| 0x04 | Store, move, scan, and compare string data |
| 0x05 | Calls in real mode |
| 0x06 | Load full pointer in real mode |
| 0x07 | Shifts and rotates |
| 0x08 | ENTER instruction |
| 0x09 | BCD arithmetic |
| 0x0A | Bit operations |
| 0x0B | LEA instruction |
| ... | ... (continues through 0xFF) |

Full test list: See [test386.asm README](https://github.com/barotto/test386.asm#readme)

## Architecture Requirements

### What test386.asm Needs

1. **BIOS Execution Model**
   - Binary must be loaded at physical address `0xf0000` (960KB)
   - Binary must be aliased at physical address `0xffff0000` (4GB - 64KB)
   - CPU reset vector at `0xfffffff0` must jump to `f000:0045`
   - 64KB binary image (exactly 65,536 bytes)

2. **Bare-Metal Environment**
   - No operating system or PE loader
   - Direct hardware access
   - Real mode CPU startup
   - Protected mode transitions via GDT/IDT setup

3. **I/O Port Infrastructure**
   - POST port (diagnostic codes): typically port 0x80
   - Serial port (COM1/COM2): ASCII output of test results
   - Parallel port (LPT1): ASCII output of test results
   - Custom output ports: configurable in source

4. **Memory Layout**
   ```
   00000-003FF: Real mode IDT
   00400-004FF: Protected mode IDT
   00500-0077F: Protected mode GDT
   00800-00FFF: Protected mode LDT
   01000-01FFF: Page directory
   02000-02FFF: Page table 0
   03000-03FFF: Page table 1
   04000-04FFF: TSS
   10000-1FFFF: Stack
   20000-9FFFF: Test code/data
   F0000-FFFFF: ROM (test386.bin)
   ```

## Why It Can't Be Integrated Into Win32Emu

### Architectural Mismatch

1. **Win32Emu is PE-Focused**
   - Designed to emulate Windows 32-bit PE executables
   - Not a bare-metal emulator or virtual machine
   - Assumes Windows API environment (Kernel32, User32, etc.)
   - No BIOS or firmware emulation

2. **No BIOS Infrastructure**
   - No support for loading at physical address 0xf0000
   - No CPU reset vector handling
   - No POST port emulation
   - No serial/parallel port emulation for BIOS-level I/O

3. **Different Execution Model**
   - Win32Emu starts from PE entry point with Windows environment
   - test386.asm expects CPU reset → BIOS boot → test execution
   - Protected mode setup is handled by Windows loader in Win32Emu
   - test386.asm wants to test the protected mode transition itself

4. **Memory Management Differences**
   - Win32Emu uses virtual memory with PE sections
   - test386.asm expects direct physical memory access
   - Win32Emu manages memory via Windows APIs (VirtualAlloc, etc.)
   - test386.asm requires specific physical address layout

### Development Effort Required

To support test386.asm would require:

- [ ] Complete BIOS emulation layer (~2000-3000 lines)
- [ ] POST port I/O implementation (~100 lines)
- [ ] Serial/parallel port emulation (~500 lines)
- [ ] Physical memory layout management (~300 lines)
- [ ] CPU reset vector handling (~200 lines)
- [ ] Raw binary loader (not PE) (~150 lines)
- [ ] Test harness to capture POST codes (~200 lines)
- [ ] Test harness to parse serial/parallel output (~300 lines)

**Total estimated effort**: ~3,750 lines + testing/debugging time

This represents a significant architectural change that would add complexity for limited benefit, given better alternatives exist.

## Better Alternative: SingleStepTests/80386

Win32Emu **already integrates** the [SingleStepTests/80386](https://github.com/SingleStepTests/80386) test suite, which provides superior instruction-level validation.

### Why SingleStepTests is Better for Win32Emu

| Feature | test386.asm | SingleStepTests/80386 |
|---------|-------------|----------------------|
| **Integration Complexity** | High (BIOS emulation) | Low (parse MOO files) |
| **Test Granularity** | Test suites (~40 categories) | Individual instructions (~941 files) |
| **Test Count** | ~40 test categories | ~2,500 tests per opcode × 941 opcodes = **~2.3M tests** |
| **Hardware Accuracy** | Good (written for emulators) | **Excellent** (captured from real 386 hardware) |
| **Execution Model** | Bare-metal BIOS | Instruction-level snapshots |
| **Debugging** | POST codes (0x00-0xFF) | Precise register/memory diffs |
| **CPU Coverage** | 386 instructions + system features | **All x86 instructions up to 386** |
| **Maintenance** | Requires BIOS infrastructure | Self-contained MOO file parser |

### SingleStepTests Integration Status

✅ **FULLY INTEGRATED** - Available now in Win32Emu.Tests.Emulator

```bash
# Run all conformance tests (may take a while - 941 test files!)
dotnet test --filter "Category=ConformanceTests"

# Run specific opcode tests
dotnet test --filter "FullyQualifiedName~SingleStepConformanceTests"
```

See: `Win32Emu.Tests.Emulator/SingleStepTests/README.md`

### Test Coverage Comparison

**test386.asm covers:**
- Arithmetic and logic operations ✓
- String operations ✓
- Control flow (jumps, calls, loops) ✓
- Segment register manipulation ✓
- Protected mode transitions ✓
- BCD operations ✓
- Bit operations ✓

**SingleStepTests covers:**
- **All of the above** ✓
- **Every addressing mode variant** ✓
- **Every flag combination** ✓
- **Edge cases** (overflow, underflow, wrapping) ✓
- **Precise cycle-accurate behavior** ✓
- **2.3 million individual test cases** ✓

## Recommendations

### For CPU Validation

1. **Use SingleStepTests/80386** (already integrated)
   - Provides more comprehensive instruction-level testing
   - Hardware-accurate test data from real 386 CPU
   - Easy to integrate and maintain
   - Already passing in Win32Emu

2. **Manual test386.asm Testing** (if needed for system-level validation)
   - Use a dedicated x86 emulator (QEMU, Bochs, DOSBox-X)
   - Run test386.bin as BIOS replacement
   - Useful for testing system-level features like:
     - Interrupt handling across mode switches
     - Segment descriptor validation
     - Page table management
     - Task switching
   - Not necessary for instruction-level correctness

### For Win32Emu Development

1. **Focus on PE executable compatibility**
   - Win32Emu's goal is running Windows games/apps, not bare-metal code
   - SingleStepTests validates instruction correctness
   - Integration tests with real Win32 executables validate API correctness

2. **Extend existing test infrastructure**
   - Add more SingleStepTests test files as needed
   - Use ReactOS test suite for Win32 API validation (planned)
   - Create integration tests with real game executables

3. **Document this decision**
   - This document serves as reference for future contributors
   - Explains why test386.asm is not integrated
   - Points to better alternatives

## How to Use test386.asm Externally (Optional)

If you want to run test386.asm to validate system-level behavior:

### 1. Build test386.bin

```bash
# Install NASM
sudo apt-get install nasm  # Debian/Ubuntu
brew install nasm          # macOS

# Clone and build
git clone https://github.com/barotto/test386.asm.git
cd test386.asm
make
# Creates test386.bin (65,536 bytes)
```

### 2. Run in QEMU

```bash
# Use test386.bin as BIOS
qemu-system-i386 -bios test386.bin \
  -serial stdio \
  -parallel stdio \
  -d int,cpu_reset,guest_errors \
  -D qemu.log

# Watch for POST codes in qemu.log
# Test output appears on serial console
```

### 3. Run in Bochs

```bochsrc
# .bochsrc configuration
romimage: file=test386.bin
megs: 4
log: bochs.log
debug: action=report
info: action=report
error: action=report
panic: action=report
```

```bash
bochs -f .bochsrc -q
```

### 4. Interpret Results

- **POST code 0xFF**: All tests passed ✓
- **Other POST codes**: Test failed at that stage (see table above)
- **HLT instruction**: Test stopped due to error
- **Infinite loop**: Stack or CALL/RET issue

Check serial output for detailed error messages (if configured in `src/configuration.asm`).

## Conclusion

While test386.asm is an excellent CPU test suite, it's **not suitable for integration** into Win32Emu due to architectural differences:

- ❌ Win32Emu is a **Win32 PE emulator**, not a bare-metal virtual machine
- ❌ Requires significant BIOS infrastructure (~3,750 lines)
- ❌ Provides **system-level** testing, not instruction-level validation

Instead, Win32Emu uses:

- ✅ **SingleStepTests/80386**: Hardware-accurate instruction-level tests (2.3M test cases)
- ✅ **ReactOS test suite** (planned): Win32 API validation
- ✅ **Integration tests**: Real Win32 game/app executables

This approach provides **better coverage** with **less complexity** and aligns with Win32Emu's architecture and goals.

## What About PCjs CPU Test Files?

The user mentioned three test files from PCjs:
1. https://www.pcjs.org/software/pcx86/test/cpu/cpuid.asm
2. https://www.pcjs.org/software/pcx86/test/cpu/id.asm
3. https://www.pcjs.org/software/pcx86/test/cpu/80386/test386.asm

### Analysis of PCjs Test Files

These are different from the barotto/test386.asm discussed above:

#### cpuid.asm
- **What it is**: DOS .COM program (org 0x100) for CPU identification
- **Tests**: Identifies CPU type (8086/8088, NEC V20/V30, 80186, 80286, 80386+)
- **Output**: Text output via DOS INT 21h
- **Size**: Small (~2KB compiled)

#### id.asm  
- **What it is**: DOS .COM program for CPU identification with extended info
- **Tests**: CPU type, MSW register, IDTR, CR0 register
- **Output**: Text output via DOS INT 21h showing register values
- **Size**: Small (~1-2KB compiled)

#### test386.asm (PCjs version)
- **What it is**: Dual-purpose - can run as ROM OR DOS .COM program
- **Tests**: Comprehensive 386 instruction testing (similar to barotto version)
- **Output**: Works as ROM (BIOS-style) or DOS program
- **Size**: Larger program with extensive tests

### Can These Be Integrated?

#### Short Answer: Partially, but not worth it

**cpuid.asm and id.asm:**
- ✅ Could potentially run if Win32Emu adds DOS .COM support
- ⚠️ Win32Emu has limited DOS INT 21h support (mainly for Win16 NE executables)
- ❌ Win32Emu is focused on Win32 PE and Win16 NE, not DOS .COM programs
- ✅ **But Win32Emu already tests CPUID**: See `PentiumInstructionTests.cs` and `CpuIntrinsicsTests.cs`

**test386.asm (PCjs):**
- ❌ As ROM: Same issues as barotto version (requires BIOS infrastructure)
- ⚠️ As DOS .COM: Would need DOS .COM loader + extensive DOS API support
- ✅ **But covered by SingleStepTests**: More comprehensive instruction-level testing

### What Win32Emu Already Tests

Win32Emu already has comprehensive CPU feature tests:

**CPUID Testing** (what cpuid.asm tests):
```csharp
// Win32Emu.Tests.Emulator/PentiumInstructionTests.cs
[Fact]
public void CPUID_Function0_ShouldReturnVendorString()

[Fact]
public void CPUID_Function1_ShouldReturnFeatureFlags()

// Win32Emu.Tests.Emulator/CpuIntrinsicsTests.cs
[Fact]
public void CPUID_Function0_ShouldReturnMaxFunction()

[Fact]
public void CPUID_Function1_ShouldReturnHostBasedFeatures()

[Fact]
public void CPUID_Function7_SubFunction0_ShouldReturnExtendedFeatures()

[Fact]
public void CPUID_UnsupportedFunction_ShouldReturnZeros()

[Fact]
public void CPUID_Function80000000_ShouldReturnMaxExtendedFunction()
```

**CPU Identification** (what id.asm tests):
- Win32Emu reports accurate CPU features via CPUID
- MSW/CR0 testing exists for protected mode operations
- IDTR testing covered by descriptor table tests

**Instruction Testing** (what test386.asm tests):
- SingleStepTests/80386: ~2.3M hardware-accurate instruction tests
- Win32Emu.Tests.Emulator: 135+ focused instruction tests
- Covers all instructions that these test files would check

### Recommendation

**Do not integrate these PCjs test files** because:

1. ✅ **CPUID functionality is already tested** in Win32Emu
2. ✅ **Instruction testing is more comprehensive** via SingleStepTests
3. ❌ **Would require DOS .COM loader** (not Win32Emu's focus)
4. ❌ **Would require extensive DOS INT 21h API** implementation
5. ❌ **Limited additional value** over existing tests

### If You Still Want to Use Them

You can run these tests **externally** with DOSBox or QEMU:

**Using DOSBox:**
```bash
# Assemble with NASM (16-bit DOS output)
nasm -f bin cpuid.asm -o cpuid.com
nasm -f bin id.asm -o id.com

# Run in DOSBox
dosbox cpuid.com
dosbox id.com
```

**Using QEMU with DOS:**
```bash
# Create a DOS disk image and add the .COM files
# Run with FreeDOS or MS-DOS
qemu-system-i386 -fda freedos.img
```

These tests are useful for validating CPU identification on real DOS systems, but Win32Emu's existing test infrastructure provides equivalent or better coverage for its Win32/Win16 emulation needs.

## References

- [test386.asm Repository (barotto)](https://github.com/barotto/test386.asm)
- [PCjs CPU Tests](https://www.pcjs.org/software/pcx86/test/cpu/)
- [SingleStepTests/80386 Repository](https://github.com/SingleStepTests/80386)
- [Win32Emu Test Strategy](../../README.Tests.md)
- [SingleStepTests Integration](../../Win32Emu.Tests.Emulator/SingleStepTests/README.md)
- [ReactOS Test Integration Research](../research/REACTOS_TEST_INTEGRATION.md)

## See Also

- [CPU Conformance Tests](../../Win32Emu.Tests.Emulator/SingleStepTests/README.md)
- [Test Strategy Overview](../../README.Tests.md)
- [Win32Emu Architecture](../../README.md)
- [PentiumInstructionTests.cs](../../Win32Emu.Tests.Emulator/PentiumInstructionTests.cs) - CPUID tests
- [CpuIntrinsicsTests.cs](../../Win32Emu.Tests.Emulator/CpuIntrinsicsTests.cs) - CPU feature detection tests
