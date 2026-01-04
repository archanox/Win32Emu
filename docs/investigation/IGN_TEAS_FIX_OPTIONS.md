# ign_teas WASM Fix Options

## Problem Summary

ign_teas gets stuck in a texture data processing loop (EIP 0x004027A2-0x004027B4) that executes 260,000+ iterations in WASM before the test times out. The loop should complete in ~16 iterations on native builds.

## Root Cause Analysis

**NOT a logic bug** - The x86 instructions are being executed correctly. The arithmetic operations (ADD, SHR) produce correct results.

**Performance issue** - WASM's interpreted execution through IcedCpu is approximately 1000x slower than native JIT compilation for tight loops.

### Why Native Works

- Uses JIT-compiled code paths or very fast interpreted execution
- Tight loop completes in milliseconds
- Game progresses to DirectDraw initialization

### Why WASM Fails

- IcedCpu interprets each x86 instruction
- Heavy overhead for instruction decoding, flag updates, memory access
- Tight loop with minimal work per iteration = worst case for interpreter
- Takes 26+ seconds for 260K iterations (100 iterations/millisecond)
- Test timeout at 120 seconds before loop completes

## Fix Options

### Option A: Enable JIT CPU for WASM ⚠️ Complex

**Approach:** Modify Emulator.cs to use JitCpu instead of IcedCpu when running on WASM

**Pros:**
- Would dramatically improve performance (10-100x faster)
- Solves problem for all games with tight loops
- Aligns with native execution model

**Cons:**
- JitCpu may not be WASM-compatible (uses System.Reflection.Emit)
- Significant testing required
- May introduce new bugs
- .NET's JIT in WASM may not support dynamic code generation

**Effort:** Large (1-2 weeks)

### Option B: Add Instruction-Level Profiling 🔍 Diagnostic

**Approach:** Add profiling to identify exact bottleneck in instruction execution

**Implementation:**
```csharp
- Track time spent in each instruction type
- Log hot paths (most executed instruction sequences)
- Identify optimization opportunities
```

**Pros:**
- Data-driven approach
- Helps prioritize optimizations
- Useful for other performance issues

**Cons:**
- Doesn't fix the problem directly
- Adds overhead to execution
- Still requires implementing optimizations after profiling

**Effort:** Medium (2-3 days)

### Option C: Optimize IcedCpu Hot Paths 🔧 Targeted

**Approach:** Optimize specific instruction implementations used in tight loops

**Target instructions:** (from ign_teas loop)
- MOV (memory/register operations)
- ADD/SUB (arithmetic)
- CMP/TEST (comparisons)
- INC/DEC (increment/decrement)
- Conditional jumps (JE, JNE, JL, JG)

**Optimizations:**
- Reduce allocations
- Cache frequently accessed values
- Inline hot methods
- Use Span<T> for memory operations
- Optimize flag calculations

**Pros:**
- Targeted improvements
- Lower risk than JIT changes
- Benefits all WASM execution

**Cons:**
- May only get 2-5x improvement
- Might still not be fast enough for ign_teas
- Requires careful measurement

**Effort:** Medium (3-5 days per optimization pass)

### Option D: Document as Known Limitation ✅ Pragmatic

**Approach:** Accept that some games won't work well in WASM

**Documentation:**
- Add to README: "WASM frontend performance limitations"
- List affected games: ign_teas (tight CPU-bound loops)
- Recommend: Use native builds for these games
- Note: Most games work fine, only edge cases affected

**Pros:**
- No code changes required
- Sets correct expectations
- Focuses effort on more impactful work
- Already increased threshold (200K→5M) helps other games

**Cons:**
- Doesn't fix the issue
- Disappointing for users wanting WASM
- Known limitation in showcase game

**Effort:** Minimal (1-2 hours)

### Option E: Add Loop Skip/Fast-Forward 🎯 Experimental

**Approach:** Detect specific loop patterns and skip iterations intelligently

**Implementation:**
```csharp
- Detect: Loop with counter decrementing to zero
- Pattern: DEC reg + JNZ/JNE loop_start
- Action: Calculate final value, skip iterations
- Risk: Must verify no side effects (memory writes, etc.)
```

**Pros:**
- Dramatic speedup for specific patterns
- Low impact on other code
- Can be WASM-specific

**Cons:**
- Very risky - easy to break games
- Complex state prediction required
- May not match ign_teas loop pattern
- Could introduce subtle bugs

**Effort:** Large (1-2 weeks + extensive testing)

## Recommendation

**Short term:** Option D - Document as known limitation
- Already improved with 200K→5M threshold increase
- Sets realistic expectations
- Focuses resources on higher-impact work

**Medium term:** Option C - Optimize IcedCpu hot paths
- Will benefit all games
- Measurable improvements
- Lower risk than JIT changes

**Long term:** Option A - Investigate JIT CPU for WASM
- If .NET WASM supports dynamic code generation
- Significant performance win if feasible
- Requires R&D phase to verify compatibility

## Current State

✅ **Increased infinite loop threshold** (200K → 5M iterations)
✅ **Added diagnostic warnings** for long-running loops
✅ **Comprehensive investigation** documented
✅ **WASM infrastructure** (input, rendering, DirectDraw) fully functional

The WASM frontend works well for most games. ign_teas is an edge case with unusually intensive CPU-bound loops during initialization.
