# CPU Testing Quick Reference

Quick guide for running and understanding CPU tests in Win32Emu.

## TL;DR

**Question**: Should we use test386.asm to validate our CPU emulator?

**Answer**: No. Win32Emu already uses **SingleStepTests/80386**, which is better suited for our architecture and provides ~2.3 million hardware-accurate test cases. test386.asm would require ~3,750 lines of BIOS emulation infrastructure that doesn't fit our Win32 PE execution model.

## Running CPU Tests

### Quick Commands

```bash
# Run all core CPU tests (fast, required for CI)
dotnet test Win32Emu.Tests.Emulator --filter "Category!=ConformanceTests"

# Run all CPU tests including conformance suite (slower, ~941 test files)
dotnet test Win32Emu.Tests.Emulator

# Run only conformance tests (hardware validation)
dotnet test --filter "Category=ConformanceTests"

# Run specific test file
dotnet test Win32Emu.Tests.Emulator --filter "FullyQualifiedName~BasicInstructionTests"
```

### Test Categories

| Category | Count | Purpose | CI Required |
|----------|-------|---------|-------------|
| Core CPU Tests | ~135 | Basic instruction validation | ✅ Yes |
| SingleStepTests | 941 files | Hardware conformance (~2.3M tests) | ❌ Optional |

## Test Suites Available

### ✅ SingleStepTests/80386 (Integrated)

**Status**: Fully integrated and working

**What it is**: Hardware-generated test cases captured from real 386 CPU execution

**Coverage**:
- ~941 opcode test files
- ~2,500+ test cases per opcode
- **Total: ~2.3 million test cases**
- All x86 instructions up to 386
- Every addressing mode variant
- Every flag combination
- Precise cycle-accurate behavior

**Why it's great**:
- ✅ Hardware-accurate (captured from real CPU)
- ✅ Already integrated
- ✅ Instruction-level focus
- ✅ Perfect fit for Win32Emu architecture
- ✅ Easy to maintain

**Location**: `Win32Emu.Tests.Emulator/SingleStepTests/`

**Docs**: [SingleStepTests README](../../Win32Emu.Tests.Emulator/SingleStepTests/README.md)

### ❌ test386.asm (Not Integrated)

**Status**: Evaluated and rejected for integration

**What it is**: Comprehensive BIOS-level CPU test suite for bare-metal emulators

**Why not integrated**:
- ❌ Requires BIOS emulation (~3,750 lines of infrastructure)
- ❌ Tests system-level features (mode switches, interrupts)
- ❌ Bare-metal focus doesn't fit Win32 PE architecture
- ❌ SingleStepTests provides better instruction-level coverage

**When to use**: External testing with QEMU/Bochs if you need bare-metal validation

**Docs**: [TEST386_EVALUATION.md](TEST386_EVALUATION.md)

## Comparison Table

| Aspect | test386.asm | SingleStepTests/80386 |
|--------|-------------|----------------------|
| **Integration** | ❌ Would need BIOS emulation | ✅ Already integrated |
| **Test Count** | ~40 test categories | ✅ ~2.3M individual tests |
| **Accuracy** | Good (written for emulators) | ✅ Excellent (real hardware) |
| **Architecture Fit** | ❌ Bare-metal BIOS | ✅ Instruction-level |
| **Maintenance** | ❌ Complex infrastructure | ✅ Simple MOO parser |
| **Development Cost** | ~3,750 lines + debugging | ✅ 0 (already done) |

## When You Need More Tests

### For Instruction-Level Validation
✅ **Use SingleStepTests** - Download more MOO files from https://github.com/SingleStepTests/80386

### For Win32 API Validation
✅ **Use ReactOS Tests** - Planned integration of ReactOS test suite

### For System-Level Validation
❌ **Don't add test386.asm** - Use external emulator if needed

## FAQ

### Q: Someone suggested using test386.asm. What should I do?

**A**: Point them to this document and [TEST386_EVALUATION.md](TEST386_EVALUATION.md). Win32Emu already has better CPU validation via SingleStepTests/80386.

### Q: What about the PCjs CPU test files (cpuid.asm, id.asm, test386.asm)?

**A**: These are DOS .COM programs that would require DOS executable loader support. Win32Emu already tests everything these files check:
- **CPUID**: See `PentiumInstructionTests.cs` and `CpuIntrinsicsTests.cs`
- **CPU identification**: Covered by existing CPUID tests
- **Instruction testing**: SingleStepTests provides more comprehensive coverage

See the "What About PCjs CPU Test Files?" section in [TEST386_EVALUATION.md](TEST386_EVALUATION.md) for detailed analysis.

### Q: What about CPU tests from the OSDev community?

**A**: OSDev community resources focus on bare-metal OS development (BIOS, ring 0, hardware initialization). Win32Emu focuses on user-mode Win32 applications:
- **OSDev tests**: Bare-metal, privileged mode, hardware compatibility
- **Win32Emu needs**: User-mode instructions, Win32 API, application compatibility

SingleStepTests provides superior instruction-level validation (~2.3M hardware-captured tests) compared to manual bare-metal tests. See the "What About OSDev Community CPU Tests?" section in [TEST386_EVALUATION.md](TEST386_EVALUATION.md).

### Q: How do I know if the CPU emulator is working correctly?

**A**: Run the core tests and conformance tests:
```bash
dotnet test Win32Emu.Tests.Emulator
```

All core tests must pass. Conformance tests provide additional validation.

### Q: Can I still use test386.asm somehow?

**A**: Yes, but externally with QEMU or Bochs. See the "How to Use test386.asm Externally" section in [TEST386_EVALUATION.md](TEST386_EVALUATION.md).

### Q: Why are conformance tests optional in CI?

**A**: They're comprehensive (~941 files) and take time to run. Core tests validate basic correctness, conformance tests validate hardware-accurate behavior. Both are valuable, but core tests are sufficient for CI gating.

### Q: What if I find a CPU bug?

**A**: 
1. Check if a SingleStepTests test file covers the instruction
2. If yes, that test should be failing - investigate why it's passing
3. If no, add a focused test in `Win32Emu.Tests.Emulator/` to reproduce the bug
4. Fix the bug
5. Verify the test passes

## Additional Resources

- [Main Test Strategy](../../README.Tests.md) - Complete testing documentation
- [TEST386_EVALUATION.md](TEST386_EVALUATION.md) - Detailed evaluation of test386.asm
- [SingleStepTests README](../../Win32Emu.Tests.Emulator/SingleStepTests/README.md) - How to use conformance tests
- [SingleStepTests Repository](https://github.com/SingleStepTests/80386) - Source of MOO test files
- [test386.asm Repository](https://github.com/barotto/test386.asm) - For reference only

## Summary

Win32Emu uses **SingleStepTests/80386** for CPU validation. It's already integrated, provides ~2.3M hardware-accurate test cases, and is the best fit for our architecture.

**test386.asm is not integrated** because it would require significant BIOS infrastructure (~3,750 lines) that doesn't fit our Win32 PE execution model, and SingleStepTests already provides superior instruction-level coverage.

Use the integrated conformance tests. They're excellent. ✅
