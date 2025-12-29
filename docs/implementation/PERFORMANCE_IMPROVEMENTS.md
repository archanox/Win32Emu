# Performance Improvements

This document describes performance improvements made to the Win32Emu codebase.

## VirtualMemory Read/Write Optimizations

### Problem
The `VirtualMemory` class's `Read16`, `Read32`, `Read64` and their corresponding `Write*` methods were calling nested methods repeatedly, each with redundant bounds checks.

For example, `Read32` called `Read16` twice, which each called `Read8` twice, resulting in:
- 4 calls to `Read8`
- 4 calls to `EnsureRange`
- 4 calls to `ReadByteInternal`

### Solution
Changed all multi-byte read/write operations to call `EnsureRange` once and then directly call `ReadByteInternal`/`WriteByteInternal` for each byte. This:
- Eliminates redundant bounds checking
- Reduces function call overhead
- Allows JIT to better inline the operations

### Affected Files
- `Win32Emu/Memory/VirtualMemory.cs`

### Performance Impact
- `Read32`: ~4x fewer method calls
- `Read64`: ~8x fewer method calls
- Similar improvements for write operations

## CPU Register Access Optimizations

### Problem
The `GetRegister` and `SetRegister` methods in `IcedCpu` and `JitCpu` called `ToUpperInvariant()` on every call. This allocated a new string on every register access, which is problematic because:
- Register access occurs on every CPU instruction
- String allocation triggers GC pressure
- The comparison against a switch expression already uses string matching

### Solution
Replaced `name.ToUpperInvariant()` with direct `string.Equals(name, "XXX", StringComparison.OrdinalIgnoreCase)` comparisons. This:
- Eliminates string allocation entirely
- Uses ordinal comparison which is fast
- Maintains case-insensitivity

### Affected Files
- `Win32Emu/Cpu/Iced/IcedCpu.cs`
- `Win32Emu/Cpu/Jit/JitCpu.cs`

### Performance Impact
- Zero string allocations per register access
- Faster comparison (ordinal vs culture-aware)

## LINQ Optimization

### Problem
The `PeResourceReader` used `Where().FirstOrDefault()` patterns instead of `FirstOrDefault()` with a predicate.

### Solution
Changed to use `FirstOrDefault(predicate)` directly, which avoids creating an intermediate iterator.

### Affected Files
- `Win32Emu/Loader/PeResourceReader.cs`

### Performance Impact
- Fewer allocations for enumerator
- Slightly faster execution

## Future Improvements

Additional performance improvements that could be considered:

1. **LoggerMessage Delegates**: The codebase has many direct `_logger.Log*()` calls that could be replaced with `LoggerMessage.Define<T>` delegates for improved performance when logging is disabled.

2. **StringComparer.OrdinalIgnoreCase**: Many places call `ToUpperInvariant()` for dictionary lookups. Using dictionaries with `StringComparer.OrdinalIgnoreCase` would eliminate these allocations.

3. **Span<byte> for memory operations**: The `WriteBytes` and `GetSpan` methods already use spans, but more operations could benefit from span-based APIs.

## Already Optimized

- **CpuStepResult**: Already defined as `readonly record struct` (value type), which means it's allocated on the stack instead of the heap. No GC pressure from this type - object pooling is not needed.
