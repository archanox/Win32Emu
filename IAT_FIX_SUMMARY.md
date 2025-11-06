# IAT Corruption Fix - Technical Summary

## Problem Description

The Win32Emu emulator was failing when the Ignition game tried to call `LoadIconA` from USER32.DLL. The error message was:

```
[IcedCpu] Invalid indirect CALL at 0x0040319A: Target address 0x001FEF10 (from register EBP) points to stack instead of code.
```

## Root Cause Analysis

### What Happened
1. Game executed: `mov ebp,[0x004552F8]` - Load function pointer from IAT
2. EBP received: `0x001FEF10` (a stack address)  
3. Game executed: `call ebp` - Call through function pointer
4. Emulator detected invalid call target and threw error

### What Should Have Happened
1. Game executed: `mov ebp,[0x004552F8]` - Load function pointer from IAT
2. EBP should receive: `0x0F000060` (synthetic import stub for LoadIconA)
3. Game executed: `call ebp` - Call through function pointer
4. Execution jumps to synthetic stub at 0x0F000060
5. Synthetic stub triggers syscall to LoadIconA implementation

### Why It Failed
The Import Address Table (IAT) entry at virtual address 0x004552F8 contained the wrong value. This entry should point to the synthetic import stub for LoadIconA, but instead contained a stack address.

Possible causes:
- Memory corruption during or after PE load
- Race condition in multi-threaded initialization
- Buffer overflow from earlier code execution
- Incorrect IAT structure in PE file (non-standard layout)

## Solution Implemented

### 1. IAT Verification Pass

Added a comprehensive verification step after all IAT entries are initialized:

```csharp
// After BuildImportMap completes, verify every IAT entry
foreach (var import in all_imports)
{
    expected = calculate_synthetic_address(import);
    actual = read_iat_entry(import.address);
    
    if (actual != expected)
    {
        log_error("IAT entry corrupted!");
        write_iat_entry(import.address, expected); // FIX IT
    }
}
```

### 2. Auto-Fix Mechanism

When a corrupted IAT entry is detected:
- Logs detailed error with DLL name, function name, address, expected vs actual value
- Immediately rewrites the correct synthetic address
- Continues verification for remaining entries
- Reports total number of fixes applied

### 3. Extensive Debug Logging

Added logging at three critical points:

**A. Memory Reads from IAT**
```
[IcedCpu] Read32 from IAT area: addr=0x004552F8 -> value=0x0F000060 at EIP=0x00403180
```

**B. Memory Writes to IAT**
```
[IcedCpu] Write32 to IAT area: addr=0x004552F8 <- value=0x0F000060 at EIP=0x00123456
```

**C. Address Calculations for IAT**
```
[IcedCpu] CalcMemAddress for IAT area: disp=0x004552F8 base=None index=None -> addr=0x004552F8 at EIP=0x00403180
```

## Expected Outcomes

### During Load Time
```
info: [Loader] Import mapping complete: 83 imports mapped to addresses 0x0F000000 - 0x0F000520
info: [Loader] IAT VERIFICATION FAILED: USER32.DLL!LoadIconA at VA 0x004552F8 contains 0x001FEF10, expected 0x0F000060
warn: [Loader] Fixing IAT entry at 0x004552F8: writing 0x0F000060
warn: [Loader] Fixed 1 corrupted IAT entries
```

OR (if no corruption):
```
info: [Loader] Import mapping complete: 83 imports mapped to addresses 0x0F000000 - 0x0F000520
info: [Loader] IAT verification passed: all 83 entries are correct
```

### During Execution
```
info: [IcedCpu] Executing at 0x00403180: mov ebp,ds:[4552F8h]
info: [IcedCpu] CalcMemAddress for IAT area: disp=0x004552F8 base=None index=None -> addr=0x004552F8 at EIP=0x00403180
info: [IcedCpu] Read32 from IAT area: addr=0x004552F8 -> value=0x0F000060 at EIP=0x00403180
info: [IcedCpu] Executing at 0x0040319A: call ebp
info: [Syscall] USER32.DLL!LoadIconA from stub at 0x0F000060
```

## Testing Instructions

1. Build the emulator with Release configuration:
   ```bash
   dotnet build --configuration Release
   ```

2. Run the Ignition game:
   ```bash
   Win32Emu.Gui.exe "path\to\IGN_TEAS.EXE"
   ```

3. Check the log file for:
   - IAT verification results (passed or fixed count)
   - LoadIconA being called successfully
   - Game progressing past the previous failure point
   - Any new errors that occur further along

## Success Criteria

✓ IAT verification completes (with or without fixes)
✓ LoadIconA is called from synthetic stub 0x0F000060
✓ No "Invalid indirect CALL" error at 0x0040319A
✓ Game progresses further toward DirectDraw initialization

## Potential Next Issues

After this fix, the game may encounter new issues as it progresses:
- Missing DirectDraw COM interface implementations
- Missing DirectSound COM interface implementations  
- Missing User32 window message handlers
- Missing GDI operations

Each of these will be logged clearly and can be addressed individually.

## Code Locations

- **IAT Verification:** `Win32Emu/Loader/PeImageLoader.cs` lines 383-425
- **Debug Logging:** `Win32Emu/Cpu/Iced/IcedCpu.cs` lines 3975-4015, 4096-4111

## Related Documentation

- Import Address Table: https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#import-address-table
- PE Format: https://docs.microsoft.com/en-us/windows/win32/debug/pe-format
- Win32 Import Resolution: https://docs.microsoft.com/en-us/windows/win32/dlls/dynamic-link-library-search-order
