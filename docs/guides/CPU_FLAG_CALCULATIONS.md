# CPU Flag Calculations - Quick Reference Guide

This guide explains how x86 CPU flags are calculated in Win32Emu's emulator, based on the Intel x86 architecture.

## Overview

The x86 EFLAGS register contains status and control flags that reflect the results of arithmetic and logical operations. Win32Emu correctly implements these calculations following Intel specifications.

## Status Flags

### Carry Flag (CF) - Bit 0

**Purpose:** Indicates unsigned overflow/underflow

**When Set:**
- **ADD**: Result wrapped around (unsigned overflow): `CF = (result < operand1)`
- **SUB**: Unsigned borrow occurred: `CF = (operand1 < operand2)`
- **SHL/SAL**: Last bit shifted out was 1
- **SHR**: Last bit shifted out was 1

**Example:**
```
0xFF + 0x02 = 0x01 (with CF=1, because 255 + 2 = 257, which wraps to 1)
0x01 - 0x02 = 0xFF (with CF=1, because we borrowed)
```

### Overflow Flag (OF) - Bit 11

**Purpose:** Indicates signed overflow

**When Set:**
- **ADD**: Two operands of same sign produce result of different sign
- **SUB**: Two operands of different signs produce result with unexpected sign

**Calculation (XOR-Based Detection):**
```csharp
// ADD: OF = (~(a ^ b) & (a ^ r)) & sign_bit
// Checks: operands same sign AND result different from operands
SetFlagVal(Of, (~(a ^ b) & (a ^ r) & signBitMask) != 0);

// SUB: OF = ((a ^ b) & (a ^ r)) & sign_bit  
// Checks: operands different signs AND result different from minuend
SetFlagVal(Of, ((a ^ b) & (a ^ r) & signBitMask) != 0);
```

**Why XOR works:**
- `a ^ b` is 0 when signs match (both bits same)
- `a ^ r` is 0 when signs match
- Combined with AND/NOT, detects sign mismatches

**Example:**
```
0x7F + 0x01 = 0x80 (with OF=1)
  127 +    1 = -128 in signed interpretation (overflow!)
```

### Zero Flag (ZF) - Bit 6

**Purpose:** Indicates result is zero

**When Set:** `result == 0`

**Example:**
```
0x05 - 0x05 = 0x00 (with ZF=1)
```

### Sign Flag (SF) - Bit 7

**Purpose:** Indicates result is negative (in signed interpretation)

**When Set:** Most significant bit (sign bit) of result is 1

**Calculation:**
```csharp
SetFlagVal(Sf, (result & signBitMask) != 0);
```

**Example:**
```
0x80 has SF=1 (represents -128 in signed 8-bit)
0x7F has SF=0 (represents +127 in signed 8-bit)
```

### Parity Flag (PF) - Bit 2

**Purpose:** Indicates even parity of low byte

**When Set:** Low byte of result has even number of 1-bits

**Calculation (Magic Constant Method):**
```csharp
var lo = (byte)result;
var bits = lo ^ (lo >> 4);  // XOR high and low nibbles
bits &= 0xF;                // Reduce to 4 bits
var even = (((0x6996 >> bits) & 1) == 0);  // Lookup in magic constant
SetFlagVal(Pf, even);
```

**Why 0x6996 works:**
```
Binary: 0110 1001 1001 0110
Bit 0 (0000): 0 -> even parity
Bit 1 (0001): 1 -> odd parity
Bit 2 (0010): 1 -> odd parity
...
Bit 15 (1111): 0 -> even parity
```

Each bit position in 0x6996 represents whether that 4-bit value has odd parity (1) or even parity (0).

**Example:**
```
0x03 = 0000 0011 (two 1-bits) -> PF=1 (even)
0x07 = 0000 0111 (three 1-bits) -> PF=0 (odd)
```

### Auxiliary Carry Flag (AF) - Bit 4

**Purpose:** Carry from bit 3 to bit 4 (used in BCD arithmetic)

**When Set:** Carry or borrow occurred from bit 3 to bit 4

**Calculation:**
```csharp
SetFlagVal(Af, ((a ^ b ^ result) & 0x10) != 0);
```

**Why XOR works:**
- Bit 4 of `a ^ b ^ result` is set if carry/borrow occurred
- XOR detects when the bit position changed due to carry

**Example (BCD):**
```
0x09 + 0x01 = 0x0A (with AF=1, because 9 + 1 in BCD needs adjustment)
```

## Special Cases

### INC and DEC

**Important:** INC and DEC do NOT affect the Carry Flag (CF)!

```csharp
// INC updates OF, AF, ZF, SF, PF but NOT CF
SetFlagsIncDecAdd(a, result);

// DEC updates OF, AF, ZF, SF, PF but NOT CF  
SetFlagsIncDecSub(a, result);
```

**Rationale:** This allows using INC/DEC in loops without affecting conditional jumps based on CF.

### Logical Operations (AND, OR, XOR)

Logical operations clear CF and OF:
```csharp
SetFlagVal(Cf, false);  // Always clear
SetFlagVal(Of, false);  // Always clear
UpdateLogicResultFlags(result);  // Set ZF, SF, PF based on result
```

## Flag Register Layout

```
31                                                            0
┌─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┐
│ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │
└─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┘
                        │ │ │ │ │         │ │   │   │     │
                        │ │ │ │ │         │ │   │   │     └─ CF (0)  Carry
                        │ │ │ │ │         │ │   │   └─────── PF (2)  Parity
                        │ │ │ │ │         │ │   └─────────── AF (4)  Auxiliary Carry
                        │ │ │ │ │         │ └─────────────── ZF (6)  Zero
                        │ │ │ │ │         └───────────────── SF (7)  Sign
                        │ │ │ │ └─────────────────────────── TF (8)  Trap
                        │ │ │ └───────────────────────────── IF (9)  Interrupt Enable
                        │ │ └───────────────────────────────DF (10) Direction
                        │ └─────────────────────────────────OF (11) Overflow
                        └───────────────────────────────────IOPL (12-13) I/O Privilege Level
```

## Testing Flag Calculations

### Test Pattern for Correctness

1. **Boundary Conditions**
   - Test minimum and maximum values (0x00, 0xFF, 0x7F, 0x80)
   - Test overflow conditions (signed and unsigned)

2. **Sign Changes**
   - Positive + Positive = Negative (signed overflow)
   - Negative + Negative = Positive (signed overflow)

3. **Parity Edge Cases**
   - All zeros (even parity)
   - All ones (even parity for 8 bits)
   - Single bit set (odd parity)

4. **Carry Propagation**
   - Test with and without existing CF in ADC/SBB
   - Verify INC/DEC don't modify CF

## References

- [Intel® 64 and IA-32 Architectures Software Developer's Manual, Volume 1](https://www.intel.com/content/www/us/en/developer/articles/technical/intel-sdm.html) - Section 3.4.3 (EFLAGS Register)
- [AMD64 Architecture Programmer's Manual](https://www.amd.com/en/support/tech-docs)
- Win32Emu Source: `/Win32Emu/Cpu/Iced/IcedCpu.cs` - Flag calculation methods

## Implementation Files

- **Flag Calculations**: `Win32Emu/Cpu/Iced/IcedCpu.cs` (lines 4673-4729)
  - `SetFlagsAdd()` - ADD operation flags
  - `SetFlagsSub()` - SUB operation flags
  - `SetFlagsIncDecAdd()` - INC operation flags
  - `SetFlagsIncDecSub()` - DEC operation flags
  - `UpdateLogicResultFlags()` - ZF, SF, PF updates

## Quick Formulas

```csharp
// Carry Flag
CF_ADD = (result < operand1)
CF_SUB = (operand1 < operand2)

// Overflow Flag  
OF_ADD = (~(a ^ b) & (a ^ result)) & sign_bit != 0
OF_SUB = ((a ^ b) & (a ^ result)) & sign_bit != 0

// Zero Flag
ZF = (result == 0)

// Sign Flag
SF = (result & sign_bit) != 0

// Parity Flag (even parity of low byte)
PF = (popcount(result & 0xFF) % 2 == 0)

// Auxiliary Carry
AF = ((a ^ b ^ result) & 0x10) != 0
```

## Common Pitfalls

1. **Forgetting sign bit varies by operand size**
   - 8-bit: 0x80
   - 16-bit: 0x8000  
   - 32-bit: 0x80000000

2. **Confusing signed vs unsigned overflow**
   - CF = unsigned overflow
   - OF = signed overflow
   - Both can occur simultaneously!

3. **INC/DEC special behavior**
   - Don't update CF (common mistake in emulators)

4. **Parity is only for low byte**
   - Even for 32-bit operations, PF only considers bits 0-7

5. **Logical operations clear CF and OF**
   - AND, OR, XOR always set CF=0, OF=0
