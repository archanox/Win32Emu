# JitCpu ADD Carry Flag Fix

## Issue
EflagsTests had 2 failing tests (`ADD_8Bit_ShouldCalculateFlagsCorrectly`) indicating incorrect carry flag calculation for 8-bit and 16-bit ADD operations in JitCpu.

## Root Cause
The `SetFlagsAdd` method in `Win32Emu/Cpu/Jit/JitCpu.cs` (line 4545) calculated the carry flag as `r < a`, which works correctly for 32-bit operations due to uint overflow behavior, but fails for 8-bit and 16-bit operations.

### Example: 8-bit ADD
```
Instruction: ADD CH, DL
CH = 0x4F, DL = 0xC1
Expected result: CH = 0x10, CF = 1

What happened:
- a = 0x4F (CH value)
- b = 0xC1 (DL value)  
- r = a + b = 0x110 (32-bit uint, no overflow)
- Carry check: r < a → 0x110 < 0x4F → false ❌

What should happen:
- Full result: 0x4F + 0xC1 = 0x110
- 8-bit carry check: 0x110 > 0xFF → true ✅
```

## Solution
Modified `SetFlagsAdd` to check for carry based on the operation size:

```csharp
bool carry = signBitMask switch
{
    0x80 => (a + b) > 0xFF,        // 8-bit: carry if sum > 255
    0x8000 => (a + b) > 0xFFFF,    // 16-bit: carry if sum > 65535
    _ => r < a                      // 32-bit: use original check
};
```

The `signBitMask` parameter is already calculated by `ExecAdd` based on the operand size:
- 8-bit operations: `signBitMask = 0x80`
- 16-bit operations: `signBitMask = 0x8000`
- 32-bit operations: `signBitMask = 0x80000000`

## Changes Made
**File**: `Win32Emu/Cpu/Jit/JitCpu.cs`
**Method**: `SetFlagsAdd` (lines 4543-4559)
**Lines Changed**: 11 lines modified (added switch expression for carry calculation)

## Test Results

### Before Fix
- **EflagsTests**: 5/7 passing (2 failures in `ADD_8Bit_ShouldCalculateFlagsCorrectly`)
- **Conformance (00.MOO.gz)**: 38/2500 passing (1.5%)
  - EFLAGS-only errors: 39

### After Fix
- **EflagsTests**: ✅ 7/7 passing (ALL TESTS PASS)
- **Conformance (00.MOO.gz)**: ✅ 55/2500 passing (2.2%) - **+45% improvement**
  - EFLAGS-only errors: 22 - **-44% reduction**

### Other Tests Verified
- ArithmeticOperationTests: 6/6 passing ✅
- ConditionalJumpTests: 4/4 passing ✅
- HighByteRegisterTests: 11/11 passing ✅
- RegisterPreservationTests: 8/8 passing ✅

## Impact
- **Correctness**: Fixed incorrect carry flag calculation for sub-32-bit ADD operations
- **Performance**: No performance impact (added one switch expression)
- **Compatibility**: Improved x86 instruction emulation accuracy
- **Test Coverage**: 17 additional conformance tests now pass

## Related Files
- `Win32Emu/Cpu/Jit/JitCpu.cs` - Main fix
- `Win32Emu.Tests.Emulator/EflagsTests.cs` - Test coverage
- `docs/implementation/ICEDCPU_MIGRATION_SUMMARY.md` - Documentation update

## Future Work
Other arithmetic operations (ADC, SBB, INC, DEC, NEG) may have similar issues with overflow flag calculation for 8-bit and 16-bit operations. However, these were not addressed in this fix to minimize scope and risk. They can be addressed in future updates if test failures are discovered.

## References
- Intel 64 and IA-32 Architectures Software Developer's Manual, Volume 1, Section 3.4.3.1 (Status Flags)
- Original issue: "EflagsTests has 2 failing tests" in `docs/implementation/ICEDCPU_MIGRATION_SUMMARY.md`
