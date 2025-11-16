# SingleStep Test Debugging Notes

## Current Status (as of investigation)
- **179/942 test files passing (19.0%)**
- Each test file contains ~2500 individual test cases
- Total estimated: ~447,000 passing test cases, ~1,908,000 failing

## Test Failure Categories

### 1. EFLAGS Calculation Issues (Highest Impact)
**Examples:**
- `676669.MOO.gz` (IMUL): 1881 EFLAGS-only failures (75% of all failures in that file)
- IMUL instruction leaves SF, ZF, PF in undefined state per Intel docs
- Real hardware computes these deterministically but implementation-specific
- Would require matching exact 386 hardware behavior

**Priority:** HIGH - affects thousands of tests  
**Difficulty:** MEDIUM - need hardware behavior reference

### 2. LOCK Prefix on Register Operations (Complex Investigation)
**Key Finding:** The CPU emulator correctly handles LOCK prefix in isolation!

**Evidence:**
```csharp
// These tests PASS:
LockAddRegisterToRegister_ShouldExecute() - ✓
AddRegisterToRegister_WithoutLock_ShouldExecute() - ✓
```

**But:** Same instruction fails in full SingleStep test harness:
- Test 177 in `00.MOO.gz`: `lock add dh,bh` - FAILS
- Failure pattern: ESP changes by +6, EIP to random address, EDX unchanged
- Pattern suggests interrupt/exception taken (3 words pushed = 6 bytes)

**Hypothesis:** Issue is environmental, not in CPU implementation:
1. Test initial memory state may include Interrupt Vector Table (IVT)
2. Some condition triggers exception/interrupt during test execution  
3. Test harness interaction problem
4. Initial memory state setup issue

**Next Steps for Debugging:**
1. Add detailed logging to SingleStepTestRunner.ExecuteTest()
2. Compare memory state between isolated test and full harness test
3. Check if IVT entries exist in initial memory state
4. Verify instruction bytes aren't overwritten by initial memory state
5. Check for any exception-throwing code paths during test execution

**Priority:** MEDIUM - only affects 13 tests in 00.MOO.gz (but 100% of failures there)  
**Difficulty:** HIGH - requires deep environmental debugging

### 3. Memory Operation Failures
**Examples:**
- `6700.MOO.gz` (ADD to memory): 456 failures (18.2%)
  - 397 register errors
  - 50 memory errors  
  - 9 EFLAGS errors
- Issues with segment overrides and complex addressing modes

**Priority:** MEDIUM  
**Difficulty:** MEDIUM

## Test File Performance Analysis

| File | Instruction | Pass Rate | Main Issues |
|------|-------------|-----------|-------------|
| 00.MOO.gz | ADD reg,reg | 99.5% | 13 LOCK failures |
| 6700.MOO.gz | ADD to mem | 81.8% | Mixed issues |
| 676669.MOO.gz | IMUL | 5.4% | EFLAGS (1881), Register (480) |

## Implementation Notes

### MOO Test File Format
- Each test contains:
  - Initial CPU state (registers, flags)
  - Initial memory state
  - Instruction bytes to execute
  - Expected final CPU state
  - Expected final memory state

### Test Execution Flow
1. Apply initial register state
2. Write instruction bytes to CS:IP location
3. Write initial memory state
4. Execute until HLT (0xF4) instruction
5. Validate final state matches expected

### Known Working Features
- Basic arithmetic (ADD, SUB) without LOCK
- Register-to-register operations
- Instruction length calculation
- EIP advancement
- 16-bit real mode addressing (CS:IP to physical)

## Recommendations

### Short Term (High ROI)
1. Fix EFLAGS for common instructions (ADD, SUB, CMP)
2. Implement proper auxiliary flag (AF) calculations
3. Add more unit tests for flag edge cases

### Medium Term
1. Debug LOCK prefix environmental issue
2. Fix IMUL flag behavior to match 386 hardware
3. Improve segment override handling

### Long Term
1. Build comprehensive flag test suite
2. Create reference implementation comparison tool
3. Add performance benchmarking for test suite

## Useful Commands

```bash
# Run all SingleStep tests
dotnet test --filter "FullyQualifiedName~SingleStepConformanceTests"

# Run specific test file
dotnet test --filter "DisplayName~00.MOO"

# Run with detailed output
dotnet test --filter "DisplayName~00.MOO" --logger "console;verbosity=detailed"

# Get summary
dotnet test --filter "FullyQualifiedName~SingleStepConformanceTests" --logger "console;verbosity=quiet"
```

## References
- [SingleStepTests/80386 Repository](https://github.com/SingleStepTests/80386)
- Intel® 64 and IA-32 Architectures Software Developer's Manual, Volume 1
- Test documentation: `Win32Emu.Tests.Emulator/SingleStepTests/README.md`
