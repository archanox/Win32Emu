# LOOP Instruction Family Implementation

## Problem

The issue reported that `metrics.exe` was failing with:
```
Win32Emu.Emulator[0]
      [IcedCpu] Unhandled mnemonic Loop at 0x00000045
fail: Win32Emu.Emulator[0]
      Calculated memory address out of range: 0xFFFFFFB0 (EIP=0x00000008) size=0x10000000; ESP=0x002000C4 EBP=0x00000002 EAX=0xFFFFFFB0 EBX=0x00000000 ECX=0x00000000 EDX=0x00000000 ESI=0x00000003 EDI=0x00000000
fail: Win32Emu.Emulator[0]
      Instruction bytes at EIP: 00 00 00 00 00 00 00 00
```

The LOOP instruction family was not implemented in the IcedCpu emulator.

## Solution

Implemented support for the complete LOOP instruction family in `Win32Emu/Cpu/Iced/IcedCpu.cs`:

### LOOP Instructions Implemented:

1. **LOOP** (opcode E2)
   - Decrements ECX
   - Jumps to target if ECX != 0
   
2. **LOOPE/LOOPZ** (opcode E1)
   - Decrements ECX
   - Jumps to target if ECX != 0 AND ZF = 1
   
3. **LOOPNE/LOOPNZ** (opcode E0)
   - Decrements ECX
   - Jumps to target if ECX != 0 AND ZF = 0

### Implementation Details:

The LOOP instructions were added to the main instruction switch statement in `IcedCpu.SingleStep()`:

```csharp
case Mnemonic.Loop:
    // LOOP - Decrement ECX and jump if ECX != 0
    _ecx--;
    if (_ecx != 0)
    {
        _eip = (uint)insn.NearBranchTarget;
    }
    break;

case Mnemonic.Loope:
    // LOOPE/LOOPZ - Decrement ECX and jump if ECX != 0 and ZF = 1
    _ecx--;
    if (_ecx != 0 && GetFlag(Zf))
    {
        _eip = (uint)insn.NearBranchTarget;
    }
    break;

case Mnemonic.Loopne:
    // LOOPNE/LOOPNZ - Decrement ECX and jump if ECX != 0 and ZF = 0
    _ecx--;
    if (_ecx != 0 && !GetFlag(Zf))
    {
        _eip = (uint)insn.NearBranchTarget;
    }
    break;
```

## Testing

Created comprehensive test suite in `Win32Emu.Tests.Emulator/LoopInstructionTests.cs` with 9 tests:

### Test Coverage:

1. **LOOP_WithNonZeroECX_ShouldDecrementAndJump** - Basic LOOP functionality
2. **LOOP_WithECXEqualsOne_ShouldDecrementToZeroAndNotJump** - Boundary condition
3. **LOOP_WithECXEqualsZero_ShouldDecrementToMaxAndNotJump** - Wraparound behavior
4. **LOOPE_WithNonZeroECXAndZFSet_ShouldDecrementAndJump** - LOOPE with condition met
5. **LOOPE_WithNonZeroECXAndZFClear_ShouldDecrementAndNotJump** - LOOPE with condition not met
6. **LOOPE_WithECXEqualsOne_ShouldDecrementToZeroAndNotJump** - LOOPE boundary
7. **LOOPNE_WithNonZeroECXAndZFClear_ShouldDecrementAndJump** - LOOPNE with condition met
8. **LOOPNE_WithNonZeroECXAndZFSet_ShouldDecrementAndNotJump** - LOOPNE with condition not met
9. **LOOPNE_WithECXEqualsOne_ShouldDecrementToZeroAndNotJump** - LOOPNE boundary

### Test Results:

- ✅ All 9 LOOP instruction tests pass
- ✅ metrics.exe test passes 
- ✅ All 12 retrowin32 integration tests pass
- ✅ All 119 existing emulator tests pass
- ✅ No regressions detected

## Impact

**Minimal Changes:**
- Modified 1 file: `Win32Emu/Cpu/Iced/IcedCpu.cs` (+27 lines)
- Added 1 test file: `Win32Emu.Tests.Emulator/LoopInstructionTests.cs` (+168 lines)

**No Breaking Changes:**
- All existing tests continue to pass
- The implementation follows the same pattern as other conditional jumps
- Standard x86 LOOP instruction semantics are correctly implemented

## Background: x86 LOOP Instructions

The LOOP family of instructions provides a compact way to implement counted loops in x86 assembly:

- **LOOP** is equivalent to: `DEC ECX; JNZ target`
- **LOOPE** is equivalent to: `DEC ECX; JNZ target if ZF=1`
- **LOOPNE** is equivalent to: `DEC ECX; JNZ target if ZF=0`

These instructions are commonly used in assembly code for:
- String/array processing loops
- Memory scanning operations
- Iteration over fixed-size data structures

The instructions automatically decrement ECX and test for zero, eliminating the need for separate DEC and conditional jump instructions.
