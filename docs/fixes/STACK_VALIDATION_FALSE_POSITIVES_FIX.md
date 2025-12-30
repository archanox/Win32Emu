# Stack Validation False Positives Fix

## Issue

The emulator was generating excessive warning messages during syscall execution:

```
[WRN] [Emulator] [Syscall] SUSPICIOUS: Stack location 0x001FEEAC contains suspiciously low value 0x000010A4 which could cause corruption if used as return address or function pointer
```

These warnings appeared repeatedly in logs, suggesting potential corruption issues, but execution continued normally without any actual problems.

## Root Cause

The stack validation code added in `Emulator.cs` (lines 2456-2461) was checking ALL values in future stack positions after syscall completion. It warned if any value was:
- Greater than 0
- Less than `MemoryRegions.MinValidUserAddress` (0x00010000 or 64KB)

The logic assumed that low values "might be used as return addresses or function pointers" and therefore could cause corruption. However, this assumption was **too broad** and created false positives.

## Why These Were False Positives

The stack contains many legitimate data values that fall below 64KB:

1. **Size parameters**: 0x00001000 (4096 bytes - page size), 0x00010000 (64KB)
2. **Boolean/enum values**: 0x00000001 (true/1), 0x00000002 (2)
3. **File handles**: 0x000010A4, 0x0000109C (valid Win32 file handles)
4. **Counts and flags**: Small integers used as parameters
5. **NULL-like values**: 0x00000001, 0x00000002 (sentinel values)

These values are **just data** on the stack - parameters, local variables, or return values. They are NOT addresses that will be dereferenced or jumped to.

## The Fix

Removed the warning code (7 lines) that was checking stack values:

```csharp
// REMOVED:
// Warn about suspicious values (very low addresses that might be used as return addresses or function pointers)
if (offset >= 0 && val > 0 && val < MemoryRegions.MinValidUserAddress)
{
    _logger.LogWarning("[Syscall] SUSPICIOUS: Stack location 0x{Addr:X8} contains suspiciously low value 0x{Val:X8} which could cause corruption if used as return address or function pointer", 
        addr, val);
}
```

**What remains**:
- The stack dump logging is still present when debug logging is enabled
- Stack validation for ESP itself (checking it's not suspiciously low)
- Return address validation (checking it's in the import hook range)

## Impact

- **Positive**: Eliminates noisy false-positive warnings that polluted logs
- **Positive**: Makes actual issues easier to spot (less noise)
- **Neutral**: Stack dump debugging info is still available when needed
- **No negative impact**: These warnings never indicated real problems

## Testing

- ✅ Build succeeds
- ✅ 803/813 core emulator tests pass (2 pre-existing unrelated failures)
- ✅ 208/208 instruction tests pass
- ✅ 21/21 IcedCpu tests pass

## Example Log Output

### Before (noisy):
```
[10:52:13] [WRN] [Emulator] [Syscall] SUSPICIOUS: Stack location 0x001FEEAC contains suspiciously low value 0x000010A4 which could cause corruption if used as return address or function pointer
[10:52:13] [DBG] [Emulator] 
[Syscall] Stack validation after KERNEL32.DLL!CloseHandle:
  [ESP+-8] = 0x001FEEA4: 0x0F000315
  [ESP+-4] = 0x001FEEA8: 0x004137BB
  [ESP++0] = 0x001FEEAC: 0x000010A4 <-- Future ESP
  [ESP++4] = 0x001FEEB0: 0x00000000
```

### After (clean):
```
[10:52:13] [DBG] [Emulator] 
[Syscall] Stack validation after KERNEL32.DLL!CloseHandle:
  [ESP+-8] = 0x001FEEA4: 0x0F000315
  [ESP+-4] = 0x001FEEA8: 0x004137BB
  [ESP++0] = 0x001FEEAC: 0x000010A4 <-- Future ESP
  [ESP++4] = 0x001FEEB0: 0x00000000
```

The stack dump info is still there for debugging, but without the false-positive warnings.

## References

- Original analysis: `docs/investigation/SYSCALL_RET_CLEANUP_ANALYSIS.md`
- Fix commit: c98de22
- File modified: `Win32Emu/Emulator.cs` (lines 2456-2461 removed)
