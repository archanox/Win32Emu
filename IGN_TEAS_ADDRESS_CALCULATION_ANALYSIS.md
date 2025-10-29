# IGN_TEAS.EXE Address Calculation Issue - Detailed Analysis

## Problem Summary

IGN_TEAS.EXE crashes with "Calculated memory address out of range: 0xEF1C4D28" when the emulated memory is only 256 MB (0x10000000).

## Root Cause Analysis

### Instruction Sequence at Failure Point

At address 0x00413609, the following sequence executes:

1. `SUB EDI, EAX` - EDI = 0x00000000 - 0x01203F28 = 0xFEDFC0D8
2. `POP EBP` - (unrelated, but logged during investigation)
3. `SHL EDI, 4` - EDI = 0xFEDFC0D80
4. `LEA EAX, [EDI+EAX+80h]` - EAX = 0xEDFC0D80 + 0x01203F28 + 0x80 = 0xEF1C4D28
5. Later attempt to access memory at 0xEF1C4D28 fails

### Mathematical Verification

The decompilation (retdec.cpp) shows this code computes:
```cpp
return a1 + 128 + 16 * (v4 - a1);
```

Where:
- `a1` = 0x01203F28 (value from VirtualAlloc)
- `v4` = 0 (EDI register value)

Result: `0x01203F28 + 0x80 + 16 * (0 - 0x01203F28) = 0xEF1C4D28`

This is mathematically correct two's complement arithmetic.

### The Core Issue

The calculated address 0xEF1C4D28 ≈ 3.7 GB is valid in a 32-bit address space but exceeds:
- Current emulated memory: 256 MB (0x10000000)
- Maximum C# byte array size: ~2 GB (Int32.MaxValue)
- Typical 32-bit Windows user mode: 2 GB (0x80000000)

## Why Does This Work on Real Windows?

Possible explanations:
1. **Windows 95/98 Memory Model**: Less strict memory protection; user code could access higher addresses
2. **Memory Mapping**: The address might map to a valid region through the Windows memory manager
3. **Wrong Assumption**: EDI should NOT be 0; there's a bug elsewhere that causes EDI=0

## Investigation Findings

### EDI Value Pattern

EDI cycles between different values in loops:
- EDI = 0x00000000 → produces invalid address
- EDI = 0x00000003 → might produce valid address  

This suggests EDI is a loop counter or index, and when EDI=0, the calculation goes out of bounds.

### Decompilation Context

From retdec.cpp, this function appears to be computing an offset within a data structure. The formula `a1 + 128 + 16 * (v4 - a1)` suggests:
- `a1` is a base address
- `v4` is an index or offset
- The result is an element address in an array-like structure

When `v4=0`, this produces a large negative offset, which wraps around in 32-bit arithmetic to a high address.

## Potential Solutions

### Option 1: Sparse Memory Model (Recommended but Complex)

Implement a sparse or segmented memory model that can handle 4 GB address space without allocating 4 GB of RAM.

**Pros:**
- Proper solution for 32-bit emulation
- Would fix this and similar issues
- More accurate Windows behavior

**Cons:**
- Significant refactoring required
- Performance implications
- Complex implementation

### Option 2: Investigate EDI=0 Case

Determine why EDI is 0 and whether it's supposed to be:
- Check if there's a missing initialization
- Check if there's a bounds check that should prevent this case
- Trace execution backwards to find where EDI should be set

**Pros:**
- Might be the actual bug
- Minimal code changes if found

**Cons:**
- Requires deep debugging of game logic
- May not be the real issue

### Option 3: Address Wrapping/Mapping

Implement special handling for out-of-bounds addresses:
- Wrap addresses modulo memory size
- Map high addresses to low addresses
- Return zero/default for invalid accesses

**Pros:**
- Quick workaround
- Minimal changes

**Cons:**
- Not accurate to real Windows behavior
- Might hide real bugs
- Could cause subtle issues

### Option 4: Increase Memory Size (Not Feasible)

Increase emulated memory to 4 GB.

**Cons:**
- C# byte arrays limited to ~2 GB  
- Would require multiple arrays or different storage
- Memory usage concerns
- Still a workaround, not a fix

## Debugging Additions

Added comprehensive debug logging in IcedCpu.cs:
- Instruction decode logging for address range 0x00413600-0x00413620
- CalcMemAddress logging with displacement, base, index details
- CalcLeaAddress logging for suspicious addresses
- SetReg32 logging to track register assignments
- WriteOp logging to track memory writes

These logs revealed the exact instruction sequence and register values leading to the failure.

## Recommendations

1. **Short-term**: Document this as a known limitation for games that use high memory addresses
2. **Medium-term**: Implement sparse memory model to support full 32-bit address space
3. **Long-term**: Consider using existing emulation libraries (e.g., Unicorn) that handle this properly

## Related Files

- `/home/runner/work/Win32Emu/Win32Emu/Win32Emu/Cpu/Iced/IcedCpu.cs` - Contains debug logging additions
- `/home/runner/work/Win32Emu/Win32Emu/Win32Emu/Memory/VirtualMemory.cs` - Memory model implementation
- `/home/runner/work/Win32Emu/Win32Emu/Win32Emu/Emulator.cs` - Memory size configuration
- `/home/runner/work/Win32Emu/Win32Emu/Decomp/ign_teas/retdec.cpp` - Decompiled game code

## Next Steps

To proceed with fixing this issue, we need to decide on the approach:
1. Implement sparse memory model (significant effort)
2. Investigate game logic to understand if EDI=0 is expected
3. Implement a workaround (address wrapping/mapping)

The investigation has identified the exact failure point and mechanism, but the proper fix requires architectural changes to the memory model.
