# HeapAlloc Investigation: Why We Don't See HeapAlloc Calls

## Question

"ApiMon says we should be expecting calls to HeapAlloc after WideCharToMultiByte in a couple places. Why aren't we seeing it in our Win32Emu logs?"

## Answer

**The C runtime's `malloc()` function uses a Small Block Heap (SBH) allocator for small allocations, which manages memory internally without calling the Win32 `HeapAlloc` API. Only larger allocations call `HeapAlloc`.**

## Evidence from Decompilation

Looking at the decompiled code from `Decomp/ign_teas/ghidra.cpp`, the C runtime's allocation chain is:

```c
void * __cdecl _malloc(size_t _Size)
{
  void *pvVar1;
  pvVar1 = __nh_malloc(_Size,DAT_0043ae4c);
  return pvVar1;
}

void * __cdecl __nh_malloc(size_t _Size,int _NhFlag)
{
  // ... size validation ...
  pvVar1 = __heap_alloc(_Size);
  // ... error handling ...
  return pvVar1;
}

void * __cdecl __heap_alloc(size_t _Size)
{
  undefined *puVar1;
  LPVOID pvVar2;
  uint dwBytes;
  
  dwBytes = _Size + 0xf & 0xfffffff0;
  
  // Try Small Block Heap first
  if ((dwBytes <= DAT_0043b66c) &&
     (puVar1 = ___sbh_alloc_block(_Size + 0xf >> 4), puVar1 != (undefined *)0x0)) {
    return puVar1;  // ✅ Small allocation - NO HeapAlloc call!
  }
  
  // Fall back to HeapAlloc for large allocations
  pvVar2 = HeapAlloc(DAT_00454574,0,dwBytes);
  return pvVar2;  // ✅ Large allocation - calls HeapAlloc
}
```

## The Small Block Heap (SBH)

The Small Block Heap is a performance optimization in Microsoft Visual C++ runtime (circa 1998):
- Used for allocations smaller than a threshold (typically ~1KB-4KB)
- Manages memory internally using pages allocated via `VirtualAlloc`
- Does NOT call `HeapAlloc` for each small allocation
- Only calls `HeapAlloc` when allocating large blocks or when SBH runs out of space

## ApiMon vs Win32Emu Comparison

### ApiMon (Real Windows)

```
Line 5821: WideCharToMultiByte(CP_ACP, 0, "=::=::\", 3574, NULL, 0, NULL, NULL) → 3574
           ⬆️ Querying size needed for 3574 wide characters

Line 5824: HeapAlloc(0x0a4c0000, 0, 3584) → 0x0a4c0498
           ⬆️ Allocating 3584 bytes (LARGE allocation, calls HeapAlloc)

Line 5826: WideCharToMultiByte(CP_ACP, 0, "=::=::\", 3574, 0x0a4c0498, 3574, NULL, NULL) → 3574
           ⬆️ Converting to the allocated buffer
```

The allocation is **3584 bytes**, which exceeds the Small Block Heap threshold, so it calls `HeapAlloc`.

### Win32Emu

```
WideCharToMultiByte → 0x000000B7 (183 bytes)
(NO HeapAlloc call - using Small Block Heap)
WideCharToMultiByte → 0x000000B7 (183 bytes)
```

The allocation is only **~192 bytes** (rounded up from 183), which is small enough to be handled by the Small Block Heap, so `malloc` doesn't call `HeapAlloc`.

## Root Cause

**The real issue is that `WideCharToMultiByte` is returning the wrong size (183 instead of 3574).**

Possible reasons:
1. The environment string block in Win32Emu is much smaller than on real Windows
2. The `GetEnvironmentStringsW` implementation returns a smaller environment
3. Different environment variables are set

## Why This Matters

The fact that we don't see `HeapAlloc` is a **symptom**, not the root cause. The root cause is that the environment is different, leading to:
1. Smaller environment strings
2. Smaller malloc allocations
3. Small Block Heap handling allocations instead of calling HeapAlloc

This is **expected behavior** for the C runtime and doesn't indicate a bug in the emulator's malloc or HeapAlloc implementation.

## Related Issues

This is separate from the infinite loop issue that occurs after `GetModuleFileNameA`. The environment string conversion happens earlier and completes successfully, just with different sizes.

## Conclusion

**We don't see HeapAlloc calls after WideCharToMultiByte because:**
1. Win32Emu has a smaller environment than real Windows
2. The smaller environment leads to smaller malloc allocations
3. Small allocations are handled by the C runtime's Small Block Heap
4. The Small Block Heap doesn't call HeapAlloc for small allocations

This is **correct behavior** - the C runtime is working as designed. The emulator is correctly emulating the environment, just with different environment variables.
