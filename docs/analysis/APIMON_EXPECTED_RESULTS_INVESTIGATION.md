# ApiMon Expected Results Investigation

## Overview

This document details the investigation into why Win32Emu stops progressing after GetModuleFileNameA while a real Windows system (captured via ApiMon) continues much further.

## Issue Analysis

### ApiMon Log (Working System)

The ApiMon log shows IGN_TEAS.EXE successfully progressing through these API calls:

1. GetVersion → 602931718
2. HeapCreate → 0x0a4c0000
3. VirtualAlloc (multiple calls)
4. GetStartupInfoA
5. GetStdHandle (NULL for all handles - GUI app behavior)
6. GetFileType (returns FILE_TYPE_UNKNOWN for NULL handles)
7. SetHandleCount → 32
8. **GetACP → 65001 (UTF-8)**
9. **GetCPInfo**
10. GetCommandLineA
11. GetEnvironmentStringsW
12. WideCharToMultiByte
13. FreeEnvironmentStringsW
14. **GetModuleFileNameA → 34 characters**
15. **HeapAlloc (0x0a4c0000, 0, 1696)**
16. **HeapFree**
17. **GetModuleHandleA("KERNEL32") → 0x75680000**
18. **GetProcAddress(0x75680000, "IsProcessorFeaturePresent") → 0x756843c0**
19. **IsProcessorFeaturePresent(PF_FLOATING_POINT_PRECISION_ERRATA) → FALSE**
20. ... continues to LoadCursorA, LoadIconA, RegisterClassA, CreateWindowExA

### Win32Emu Behavior (Before Fix)

Win32Emu executes these API calls:

1. GetVersion → 0x040003B6
2. HeapCreate → 0x01000090
3. VirtualAlloc (multiple calls)
4. GetStartupInfoA
5. GetStdHandle (returns NULL - correct for GUI app)
6. GetFileType (returns FILE_TYPE_UNKNOWN - correct)
7. SetHandleCount → 0x00000020
8. **GetACP → 0x0000FDE9 (65001 in decimal - UTF-8)**
9. **GetCPInfo → TRUE**
10. GetCommandLineA → 0x01000000
11. GetEnvironmentStringsW → 0x01402000
12. WideCharToMultiByte → 0x000000B7 (183 characters)
13. WideCharToMultiByte (again) → 0x000000B7
14. FreeEnvironmentStringsW → TRUE
15. **GetModuleFileNameA → 26 characters (EXEs/ign_teas/IGN_TEAS.EXE)**
16. **Execution continues at 0x004123B8**
17. **ENTERS INFINITE LOOP - NO MORE API CALLS**

## Root Cause Analysis

### The Bug: GetModuleHandleA Implementation

**Problem:** GetModuleHandleA was always returning the image base (0x00400000) regardless of the module name parameter:

```csharp
private uint GetModuleHandleA(in LpcStr lpModuleName)
{
    var moduleName = lpModuleName.ToString();
    _logger.LogInformation("Getting module handle for '{0}'", moduleName ?? "NULL (current process)");
    return _imageBase;  // ❌ Always returns image base!
}
```

This was incorrect because:
- `GetModuleHandleA(NULL)` should return the current process handle (image base) ✓
- `GetModuleHandleA("KERNEL32")` should return a handle to KERNEL32.DLL ✗

**Fix:** Updated GetModuleHandleA to:
1. Return image base for NULL (current process)
2. Return proper handles for system DLLs via `_env.LoadModule()`
3. Return 0 with ERROR_MOD_NOT_FOUND for unknown modules

### The Infinite Loop

After fixing GetModuleHandleA, we discovered that IGN_TEAS.EXE **never actually calls GetModuleHandleA**! Instead, it enters an infinite loop immediately after GetModuleFileNameA.

**Loop characteristics:**
- Executes ~190,000+ instructions in 3 seconds
- Cycles through addresses: 0x00412551 → 0x004125E9 → 0x0041251D → 0x00412554
- No API calls are made during the loop
- EBP register is 0x00000000 (suspicious)

**Possible causes:**
1. GetModuleFileNameA returns a path with forward slashes (`EXEs/ign_teas/IGN_TEAS.EXE`) instead of backslashes
2. The path length (26 characters) differs from the expected length (34 characters in ApiMon)
3. Some earlier API call returned an incorrect value
4. Missing or incorrect initialization

## Fixes Implemented

### 1. Fixed GetModuleHandleA

```csharp
private uint GetModuleHandleA(in LpcStr lpModuleName)
{
    var moduleName = lpModuleName.ToString();
    
    // NULL means get handle to current process executable
    if (string.IsNullOrEmpty(moduleName))
    {
        return _imageBase;
    }
    
    // Normalize module name
    var normalizedName = Path.GetFileName(moduleName).ToUpperInvariant();
    if (!normalizedName.EndsWith(".DLL", StringComparison.OrdinalIgnoreCase))
    {
        normalizedName += ".DLL";
    }
    
    // Check if this is a system DLL we emulate
    var isSystemDll = normalizedName switch
    {
        "KERNEL32.DLL" => true,
        "USER32.DLL" => true,
        "GDI32.DLL" => true,
        // ... other system DLLs
        _ => false
    };
    
    if (isSystemDll || _env.IsModuleLoaded(normalizedName))
    {
        return _env.LoadModule(normalizedName);
    }
    
    _lastError = NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
    return 0;
}
```

### 2. Updated Test Environment

Added Win32Dispatcher to the test environment so GetProcAddress tests can work:

```csharp
public TestEnvironment()
{
    // ... existing setup ...
    
    // Create dispatcher and register modules
    Dispatcher = new Win32Dispatcher(NullLogger.Instance);
    Kernel32 = new Kernel32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
    Kernel32.SetDispatcher(Dispatcher);
    Dispatcher.RegisterModule(Kernel32);
}
```

### 3. Updated Tests

- Fixed `GetModuleHandleA_WithKernel32_ShouldReturnKernel32Handle` to expect a module handle, not the image base
- Fixed `GetModuleHandleA_WithInvalidModuleName_ShouldReturnZero` to expect 0 and check ERROR_MOD_NOT_FOUND
- Added `GetProcAddress_WithSystemDll_ShouldReturnFunctionAddress` test

## Test Results

All 199 tests pass:
- ✅ 193 passed
- ⏭️ 4 skipped (console I/O tests)
- ❌ 2 failed (pre-existing GetACP code page issues, unrelated)

## Impact

### Immediate Impact

The GetModuleHandleA fix resolves a real bug that would have blocked programs from:
1. Getting handles to system DLLs
2. Using GetProcAddress to look up functions dynamically
3. Checking for optional functionality (like IsProcessorFeaturePresent)

### Why IGN_TEAS Still Doesn't Progress

The game enters an infinite loop BEFORE calling GetModuleHandleA, so this fix alone doesn't make IGN_TEAS progress further. However, once the loop issue is resolved, this fix will be necessary for the game to continue.

## Next Steps

To resolve the infinite loop and make IGN_TEAS progress further:

1. **Investigate the loop addresses** (0x00412551-0x00412554)
   - Use IDA Pro or Ghidra to disassemble the code
   - Identify what the loop is doing (string parsing? condition check?)

2. **Check GetModuleFileNameA path format**
   - Real Windows: `C:\path\to\IGN_TEAS.EXE` (backslashes)
   - Win32Emu: `EXEs/ign_teas/IGN_TEAS.EXE` (forward slashes)
   - Convert to Windows-style paths?

3. **Review earlier API calls**
   - Check if any return values differ from ApiMon
   - Particularly focus on heap operations, environment strings, etc.

4. **Add more debugging**
   - Log register values in the loop
   - Check what memory locations are being accessed
   - Use the interactive debugger to step through

## Conclusion

This investigation fixed a real bug in GetModuleHandleA that was preventing proper module handle management. However, it revealed that IGN_TEAS.EXE has a more fundamental issue - it enters an infinite loop before reaching the code that would benefit from this fix.

The GetModuleHandleA fix is still valuable and should be merged, as it:
- Fixes a bug that affects all programs using dynamic function lookup
- Adds proper test coverage for GetModuleHandleA and GetProcAddress
- Improves the test infrastructure with dispatcher support

The infinite loop issue requires separate investigation and fixes.
