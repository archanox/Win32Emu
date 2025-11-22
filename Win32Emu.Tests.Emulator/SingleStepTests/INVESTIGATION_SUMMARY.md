# SingleStep Test Investigation Summary

## Overview
This document summarizes the investigation into improving SingleStep test pass rates for the Win32Emu CPU emulator.

## Test Suite Statistics (Baseline)
- **Total test files**: 941
- **Passing all tests**: 294 files (31%)
- **Failing some/all tests**: 647 files (69%)
- **Pass rate range**: 0% to 100%

## Failure Categories (By Frequency)

### 1. EFLAGS-Only Failures (~60-70%)
The most common issue across all failing tests. Flag calculations are incorrect for various instructions.

**Affected Instructions:**
- AAD (Decimal Adjust): D5.MOO.gz - 15.3% pass rate
- SHRD (Double Precision Shift): 660FAC.MOO.gz - 30.7% pass rate
- Comparison instructions: CMP, TEST
- Shift/Rotate: SHL, SHR, ROL, ROR, RCL, RCR
- Arithmetic: IMUL with wrong OF/CF flags

**Flags Most Commonly Wrong:**
- CF (Carry Flag)
- OF (Overflow Flag)  
- PF (Parity Flag)
- AF (Auxiliary Flag)

### 2. Register/Exception Failures (~20%)
Pattern: ESP decreases by 6 bytes, EIP jumps to different location, memory changes on stack.

**Root Cause:** Real 80386 hardware triggers General Protection Fault (#GP, vector 13) when memory access crosses 64KB segment boundary in real mode.

**Example:**
- Instruction: `add ax,[ds:bx]` when BX=0xFFFF
- Reading 2-byte word from offset 0xFFFF extends to 0x10000 (beyond segment limit)
- Hardware pushes FLAGS, CS, IP to stack (6 bytes) and jumps to exception handler

**Note:** Attempted implementing segment limit checking but caused regressions. Needs more research on exact conditions when 80386 enforces limits in real mode.

### 3. Memory Access Errors (~10%)
Incorrect memory reads/writes, often coupled with register mismatches.

### 4. Execution Errors (~10%)
Infinite loops or crashes from unimplemented or incorrectly implemented instructions.

## High-Pass-Rate Test Files (Good Baselines)
Use these for regression testing:

- **03.MOO.gz**: 99.6% pass (9/2500 failures)
- **FE.0.MOO.gz, FE.1.MOO.gz**: 99.96% pass (1/2500 failures each)
- **F6.0-3.MOO.gz**: 99.96% pass rates
- **F7.0-3.MOO.gz**: 99.5% pass rates
- **81.x series**: 99.5-99.6% pass rates

## Problematic Test Files (Avoid Initially)
These have fundamental issues that need deep investigation:

- **D5.MOO.gz** (AAD): 15.3% pass - pure EFLAGS issues
- **660FAC.MOO.gz** (SHRD): 30.7% pass - EFLAGS issues
- **String operations**:
  - A6.MOO.gz (CMPSB): 13.4% pass
  - A7.MOO.gz (CMPSW): 0% pass
  - AA.MOO.gz (STOSB): 0% pass

## Recommendations

### Phase 1: EFLAGS Fixes (High Priority)
Fix flag calculations for specific instruction classes, one at a time:

1. **AAD instruction** (D5.MOO.gz)
   - Currently 15.3% pass, 84.7% fail (all EFLAGS)
   - Focus on CF, PF, SF, ZF, OF calculations
   
2. **SHRD instruction** (660FAC.MOO.gz)
   - Currently 30.7% pass, 69.3% fail (mostly EFLAGS)
   - Double-precision shift right needs careful CF/OF handling

3. **Comparison instructions** (CMP, TEST)
   - Multiple test files affected
   - Arithmetic flag calculations

4. **Shift/Rotate instructions**
   - SHL, SHR, ROL, ROR, RCL, RCR
   - CF and OF flag handling critical

### Phase 2: Segment Limits (Medium Priority)
Research and implement proper segment limit checking:

1. Study Intel 80386 Programmer's Reference Manual
2. Understand when limits are enforced (CR0 flags, protection mode vs real mode)
3. Implement conservative check that doesn't break passing tests
4. Test with 03.MOO.gz failures as validation cases

### Phase 3: Complex Instructions (Low Priority)
After core issues fixed:

1. String operations (CMPSB, SCASB, LODSB, STOSB, etc.)
2. Complex arithmetic (MUL/IMUL variations)
3. Obscure/rarely used instructions

## Implementation Approach

1. **One instruction class at a time**
2. **Create focused unit tests** for specific instruction
3. **Use FailureAnalyzer.cs** to debug specific failures
4. **Run regression tests** after each fix (use high-pass-rate files)
5. **Measure improvement** with full test suite

## Test Infrastructure

**Key Files:**
- `SingleStepConformanceTests.cs` - Main test runner
- `SingleStepTestRunner.cs` - Test execution engine
- `MooFileParser.cs` - Parses MOO test files
- `FailureAnalyzer.cs` - Debugging tool for failures
- `DebugSpecificTest.cs` - Debug individual failing tests

**Test Data Location:**
`Win32Emu.Tests.Emulator/TestData/SingleStepTests/*.MOO.gz`

## CPU Implementation Location

**Main CPU file:**
`Win32Emu/Cpu/Iced/IcedCpu.cs`

**Key methods:**
- `SingleStep()` - Main instruction execution
- `CalcMemAddress()` - Memory address calculation
- Flag calculation methods for each instruction type

## Notes on 80386 Segment Behavior

From investigation:
- Real mode segments have 64KB limit (offsets 0x0000-0xFFFF)
- Accessing beyond segment boundary may trigger #GP exception
- Exception pushes FLAGS (2 bytes), CS (2 bytes), IP (2 bytes) = 6 bytes total
- Clears IF and TF flags
- Jumps to IVT entry (vector * 4) for handler address

**Uncertainty:**
- Exact conditions when 80386 enforces limits in real mode
- Whether it depends on CR0 or other control registers
- Why some boundary accesses don't trigger exceptions in test data

## Investigation Tools Created

**DebugSpecificTest.cs:**
- Analyzes individual failing tests
- Shows initial/final CPU state
- Displays IVT entries and memory changes
- Useful for understanding specific failure patterns

**Usage:**
```csharp
dotnet test --filter "FullyQualifiedName~DebugSpecificTest.Debug_03MOO_Test39"
```

## Additional Resources

- SingleStepTests/80386 Repository: https://github.com/SingleStepTests/80386
- Intel 80386 Programmer's Reference Manual
- Win32Emu documentation: `/docs/`

## Future Investigation Topics

1. Why do some memory accesses at segment boundaries NOT trigger exceptions?
2. Is there a CPU mode or flag that controls segment limit enforcement?
3. Are there specific instruction types that bypass limit checks?
4. How does A20 gate affect addressing in tests?

## Last Updated
2025-11-22 by GitHub Copilot Agent
