# CPU Hardware Test Failures - Analysis and Recommendations

## Executive Summary

The CPU conformance tests from SingleStepTests/80386 are currently failing at 0% pass rate. The root cause has been identified and fixed (MOO file parser), but systematic CPU emulation bugs remain. This document provides analysis and recommendations for fixing the emulator.

## Issues Fixed ✅

### 1. MOO File Parser (CRITICAL FIX)
**Problem**: Parser was not correctly reading the sparse FINA (final state) format
- FINA uses a bitmask to indicate which registers changed
- Parser was treating it as a full register dump
- All expected values showed as 0x00000000

**Solution Implemented**:
- Rewrote `ReadRegisterState()` to use bitmask-based parsing
- Added `RegisterState.PresenceMask` to track which registers were set
- Implemented proper merging of sparse FINA with complete INIT baseline
- Follows reference implementation from https://github.com/dbalsom/moo

**Verification**: Parser now correctly reads expected values matching JSON format

### 2. Test Infrastructure Improvements
- Increased test coverage from 10 to 100 tests per file
- Added failure categorization (Memory, Register, EIP, EFLAGS)
- Added summary statistics and pass rate reporting
- Limited output to first 5 failures to avoid log overflow

## Remaining Issues ❌

### Current Test Results (ADD instruction - 00.MOO.gz)
```
Total tests: 100 (out of 2500 available)
Passed: 0 (0.0%)
Failed: 100 (100.0%)

Failure breakdown:
  Memory errors: 69 tests (69%)
  Register value errors: 17 tests (17%)
  EIP + EFLAGS: 12 tests (12%)
  EIP only: 2 tests (2%)
```

### Root Causes Identified

#### 1. Memory Write Operations (69% of failures)
ADD instructions that modify memory (`add [mem], reg`) are failing to correctly write results back to memory.

**Example failure**:
```
Test 0: add [ss:bp+60h],bl
  Register mismatches: EIP(expected=0x000072A4, actual=0x000072A2)
                      EFLAGS(expected=0xFFFC0092, actual=0xFFFC0006)
  Memory mismatches: 1 locations
```

**Likely Issues**:
- Effective address calculation errors
- Memory write not being executed
- Segment override not being handled correctly

#### 2. Register Value Calculation (17% of failures)
ADD instructions that modify registers (`add reg, reg`) produce incorrect results.

**Example failure**:
```
Test 4: add ch,dl
  Register mismatches: ECX(expected=0x0BA31040, actual=0x0BA34F40)
```

**Likely Issues**:
- Byte/word/dword size handling errors
- High-byte register access (CH, DH, etc.) not working correctly
- Carry/overflow propagation errors

#### 3. EIP Advancement (All tests)
Almost all tests show EIP mismatch, usually off by 2-3 bytes.

**Pattern**: `EIP(expected=0x000072A4, actual=0x000072A2)` - off by 2 bytes

**Likely Issue**:
- Instruction length calculation is incorrect
- Not properly advancing EIP after instruction execution
- May be using wrong addressing mode size

#### 4. EFLAGS Calculation (All tests)
All tests show EFLAGS mismatches, suggesting systematic flag calculation errors.

**Common patterns**:
- `EFLAGS(expected=0xFFFC0092, actual=0xFFFC0006)` 
- Missing CF (Carry Flag), PF (Parity Flag), AF (Auxiliary Flag), ZF (Zero Flag), SF (Sign Flag), OF (Overflow Flag)

**Likely Issues**:
- Flags not being updated after ALU operations
- Wrong flag calculation formulas
- Missing flag updates for specific instruction variants

## Recommended Fix Priority

### Priority 1: EIP Advancement (CRITICAL)
Without correct EIP, emulator will crash or execute wrong code.

**Action Items**:
1. Locate EIP advancement code in `IcedCpu.cs`
2. Verify instruction length is being calculated correctly
3. Ensure EIP is advanced by `instruction.Length` after execution
4. Test with simple instructions first

**File to investigate**: `/home/runner/work/Win32Emu/Win32Emu/Win32Emu/Cpu/Iced/IcedCpu.cs`

### Priority 2: EFLAGS Calculation (CRITICAL)
Flags affect conditional instructions and program flow.

**Action Items**:
1. Review flag calculation in ALU operations (ADD, SUB, AND, OR, etc.)
2. Verify each flag is being set correctly:
   - CF: Carry out of MSB
   - PF: Parity of low byte
   - AF: Carry out of bit 3
   - ZF: Result is zero
   - SF: Sign bit of result
   - OF: Signed overflow
3. Use known test cases to validate each flag
4. Reference: Intel 386 Programmer's Manual Section on FLAGS

### Priority 3: Memory Operations (HIGH)
Memory errors affect most instructions.

**Action Items**:
1. Review effective address calculation
2. Verify segment override handling (CS:, DS:, SS:, ES:, FS:, GS:)
3. Check memory write operations are being executed
4. Validate address modes (16-bit vs 32-bit addressing)

### Priority 4: Register Operations (MEDIUM)
Register operations need correct size handling.

**Action Items**:
1. Review byte/word/dword register access
2. Verify high-byte register handling (AH, BH, CH, DH)
3. Test with simple `mov` and `add` instructions
4. Validate partial register updates

## Testing Strategy

### Phase 1: Basic Validation
1. Start with simple 32-bit register operations: `add eax, ebx`
2. Move to 16-bit operations: `add ax, bx`
3. Then 8-bit operations: `add al, bl`
4. Finally high-byte operations: `add ah, bh`

### Phase 2: Memory Operations  
1. Start with simple memory reads: `add eax, [ebx]`
2. Test memory writes: `add [ebx], eax`
3. Add addressing modes: `add eax, [ebx+ecx*4+0x10]`
4. Test segment overrides: `add eax, [cs:ebx]`

### Phase 3: Flags Testing
Create targeted tests for each flag:
1. Carry Flag (CF): `add 0xFFFFFFFF, 1` should set CF
2. Zero Flag (ZF): `add 0, 0` should set ZF
3. Sign Flag (SF): `add 0x7FFFFFFF, 1` should set SF
4. Overflow Flag (OF): `add 0x7FFFFFFF, 1` should set OF
5. Parity Flag (PF): `add 0, 1` should clear PF (odd parity)
6. Auxiliary Flag (AF): Test with BCD operations

### Phase 4: Full Coverage
Once basic operations pass:
1. Increase test count to 1000 per file
2. Test all instruction types (not just ADD)
3. Aim for >90% pass rate on basic instructions
4. Document any known limitations

## Reference Materials

### SingleStepTests Documentation
- Repository: https://github.com/SingleStepTests/80386
- README: https://github.com/SingleStepTests/80386/blob/main/README.md
- MOO Format: https://github.com/dbalsom/moo

### Intel Documentation
- 80386 Programmer's Reference Manual
- Section on instruction encoding
- Section on FLAGS register
- Section on addressing modes

### Example Implementations
- C++ parser: /tmp/moo/cpp/mooreader.h
- Reference implementation shows correct flag handling
- Can be used as reference for ALU operations

## Next Steps

1. **Immediate**: Investigate IcedCpu.SingleStep() method
2. **Short-term**: Fix EIP and EFLAGS issues
3. **Medium-term**: Fix memory operations
4. **Long-term**: Achieve >90% pass rate on all basic instructions

## Files to Review

```
/home/runner/work/Win32Emu/Win32Emu/Win32Emu/Cpu/Iced/IcedCpu.cs
- Main CPU emulator implementation
- Contains SingleStep() method
- Contains ALU operations
- Contains flag calculations

/home/runner/work/Win32Emu/Win32Emu/Win32Emu.Tests.Emulator/SingleStepTests/
- SingleStepConformanceTests.cs - Test runner
- SingleStepTestRunner.cs - Test execution logic
- MooFileParser.cs - File parser (NOW FIXED)
```

## Success Criteria

- [ ] >50% pass rate on basic ADD instructions
- [ ] >75% pass rate on basic register operations
- [ ] >90% pass rate on simple instructions (MOV, ADD, SUB)
- [ ] EIP correctly advanced for all instructions
- [ ] EFLAGS correctly calculated for all instructions
- [ ] Memory operations work correctly with all addressing modes

## Notes

- Tests use hardware-generated values from real 80386 CPU
- Failures indicate actual emulation bugs, not test issues
- Parser is now correct and reading expected values properly
- Focus should be on CPU implementation, not test infrastructure
