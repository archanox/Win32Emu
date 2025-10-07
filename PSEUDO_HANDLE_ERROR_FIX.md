# Fix for winapi.exe Memory Access Error (0xFFFFFFF5)

## Summary

This fix enhances the emulator's diagnostic error messages when memory access violations occur, specifically addressing the error seen when running `winapi.exe`:

```
Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00401002)
```

## What Changed

### Enhanced Error Messages

The error messages now include:

1. **All general-purpose registers**: EBX, ESI, and EDI are now shown in addition to ESP, EBP, EAX, ECX, and EDX
2. **Pseudo-handle detection**: When the invalid address is a Windows pseudo-handle (STD_INPUT_HANDLE, STD_OUTPUT_HANDLE, or STD_ERROR_HANDLE), the error message includes:
   - The handle name (e.g., "STD_OUTPUT_HANDLE")
   - An explanation of what went wrong
   - Guidance on how to fix it (use GetStdHandle() to translate pseudo-handles)

### Example

**Before:**
```
fail: Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00401002) 
      size=0x10000000; ESP=0x00200000 EBP=0x00200000 
      EAX=0x00000002 ECX=0x00000000 EDX=0x00000000
```

**After:**
```
fail: Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00001003) 
      size=0x100000; ESP=0x00200000 EBP=0x00200000 
      EAX=0x00000002 EBX=0x00000000 ECX=0x00000000 
      EDX=0x00000000 ESI=0x12345678 EDI=0x87654321
      NOTE: Address 0xFFFFFFF5 is STD_OUTPUT_HANDLE (pseudo-handle value -11).
      This error typically occurs when code tries to dereference a pseudo-handle as a memory address.
      Pseudo-handles must be translated to real handles via GetStdHandle() before use.
```

## Why This Helps

### Better Diagnostics
With all registers visible, you can now see exactly which register contains the small value that causes address wraparound. In the example above, EBX=0x00000000, and the instruction likely accesses `[EBX-11]`, which calculates to 0xFFFFFFF5.

### Understanding Pseudo-Handles
Windows pseudo-handles are special constant values:
- `STD_INPUT_HANDLE` = 0xFFFFFFF6 (-10)
- `STD_OUTPUT_HANDLE` = 0xFFFFFFF5 (-11)
- `STD_ERROR_HANDLE` = 0xFFFFFFF4 (-12)

These are **not** valid memory addresses. They're sentinel values that must be translated to real handles using `GetStdHandle()` before use.

### Common Cause
The error typically occurs when:
1. A register (often EBX, ESI, or EDI) contains a very small value (0-11)
2. Code tries to access memory with a negative displacement: `[register - N]`
3. The arithmetic wraps around: `small_value + (-N) = 0xFFFFFFFx`

## Files Modified

1. **Win32Emu/Diagnostics/Diagnostics.cs**
   - Enhanced `LogCalcMemAddressFailure()` to include all registers
   - Added pseudo-handle detection and helpful error messages

2. **Win32Emu/Cpu/Iced/IcedCpu.cs**
   - Updated `CalcMemAddress()` to pass all registers to diagnostics

3. **Win32Emu.Tests.Kernel32/CpuMemoryAccessTests.cs**
   - Added tests for pseudo-handle detection
   - Added test for all-registers logging

## Testing

All tests pass:
- ✅ 15/15 CpuMemoryAccessTests
- ✅ 159/160 Win32Emu.Tests.Kernel32 (1 pre-existing failure)

## Next Steps

When you encounter this error with `winapi.exe`:
1. Look at the enhanced error message to see all register values
2. Check which register has a small value (close to 0)
3. Look at the instruction bytes to understand what the program is trying to do
4. The program likely has a bug where it's dereferencing a pseudo-handle instead of calling GetStdHandle() first

If the program is correct and this is an emulation issue, the enhanced diagnostics will make it much easier to identify the root cause.
