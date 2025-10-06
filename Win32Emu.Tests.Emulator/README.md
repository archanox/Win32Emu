# CPU Emulator Instruction Coverage

This document tracks the x86 instruction coverage in the Win32Emu CPU emulator tests.

## Test Summary

**Total Tests**: 37  
**Status**: All passing ✅

## Instruction Categories

### 8086/286/386 Basic Instructions (18 tests)
These foundational instructions are tested for correctness:

| Instruction | Tests | Description |
|------------|-------|-------------|
| ADD | 2 | Addition with and without carry |
| SUB | 2 | Subtraction with and without borrow |
| XOR | 1 | Exclusive OR (commonly used to clear registers) |
| AND | 1 | Bitwise AND |
| OR | 1 | Bitwise OR |
| TEST | 2 | Logical comparison (AND without storing result) |
| CMP | 2 | Compare (SUB without storing result) |
| INC | 1 | Increment |
| DEC | 1 | Decrement |
| SHL | 1 | Shift left |
| SHR | 1 | Shift right |
| CDQ | 3 | Convert Doubleword to Quadword (sign-extend EAX into EDX:EAX) |

### Intel 486 Instructions (11 tests)
New instructions introduced with the 486 processor:

| Instruction | Tests | Description |
|------------|-------|-------------|
| BSWAP | 4 | Byte swap (endianness conversion) |
| CMPXCHG | 2 | Compare and exchange (atomic operation) |
| XADD | 2 | Exchange and add (atomic operation) |
| INVD | 1 | Invalidate cache (privileged, stubbed) |
| WBINVD | 1 | Write-back and invalidate cache (privileged, stubbed) |
| INVLPG | 1 | Invalidate TLB entry (privileged, stubbed) |

### Intel Pentium Instructions (8 tests)
New instructions introduced with the Pentium processor:

| Instruction | Tests | Description |
|------------|-------|-------------|
| RDTSC | 1 | Read Time-Stamp Counter |
| CPUID | 2 | CPU Identification (functions 0 and 1) |
| CMPXCHG8B | 2 | Compare and exchange 8 bytes (64-bit atomic operation) |
| RDMSR | 1 | Read Model Specific Register (privileged, stubbed) |
| WRMSR | 1 | Write Model Specific Register (privileged, stubbed) |
| RSM | 1 | Resume from System Management Mode (privileged, stubbed) |

## Implementation Notes

### Privileged Instructions
Several privileged instructions (INVD, WBINVD, INVLPG, RDMSR, WRMSR, RSM) are implemented as stubs for user-mode emulation. These instructions:
- Don't perform their full hardware functionality
- Don't raise exceptions (for compatibility)
- Are implemented as no-ops or return dummy values
- Allow emulated code using these instructions to continue execution

### Atomic Operations
Instructions like CMPXCHG, XADD, and CMPXCHG8B are critical for multithreaded code. While Win32Emu is single-threaded, these instructions are correctly implemented for their sequential semantics.

### CPU Identification
CPUID returns basic CPU information:
- Function 0: Returns "GenuineIntel" vendor string and max function (1)
- Function 1: Returns basic feature flags indicating Pentium-class CPU

### Time-Stamp Counter
RDTSC returns a timestamp based on `Environment.TickCount64`, providing a monotonically increasing value suitable for basic timing operations.

### Sign Extension (CDQ)
CDQ (Convert Doubleword to Quadword) is a critical instruction for signed division operations. It sign-extends EAX into EDX:EAX:
- If EAX is positive (bit 31 = 0), EDX becomes 0x00000000
- If EAX is negative (bit 31 = 1), EDX becomes 0xFFFFFFFF

This is commonly used before IDIV to prepare the dividend for signed division.

## Already Implemented (Not New)

The following instructions were already implemented in IcedCpu before this PR:
- MOV, LEA, MOVZX, MOVSX
- PUSH, POP, PUSHAD, POPAD
- ADC, SBB
- SAL, SAR, ROL, ROR, RCL, RCR
- NOT, NEG
- XCHG
- SETcc family (SETO, SETNO, SETB, SETAE, SETE, SETNE, SETBE, SETA, SETS, SETNS, SETP, SETNP, SETL, SETGE, SETLE, SETG)
- CMOVcc family (CMOVE, CMOVNE, CMOVB, CMOVBE, CMOVA, CMOVGE, CMOVG, CMOVL)
- String operations (MOVSB, MOVSD, STOSB, STOSD, LODSB, LODSD, CMPSB, SCASB)
- Control flow (JMP, CALL, RET, Jcc family)
- Flag operations (CLD, STD, CLC, STC, CMC, PUSHFD, POPFD, LAHF, SAHF)
- Interrupts (INT, INT3)
- Multiplication/Division (MUL, IMUL, DIV, IDIV)

## Future Work

Additional instruction coverage could include:
- More 386 instructions (BSF, BSR, BT, BTC, BTR, BTS, SHLD, SHRD)
- Floating-point instructions (FPU/x87)
- MMX instructions (Pentium MMX)
- SSE instructions (Pentium III+)
- String instruction variants (CMPSW, CMPSD, SCASW, SCASD, etc.)
- More addressing modes and edge cases
