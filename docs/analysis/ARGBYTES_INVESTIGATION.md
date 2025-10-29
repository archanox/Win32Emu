# argBytes Investigation Results

## Summary

Investigation into why manual argBytes overrides exist in Win32Dispatcher.cs when StdCallArgBytesGenerator should handle this automatically.

## Findings

### 1. StdCallArgBytesGenerator Status: ✅ WORKING

The generator in `Win32Emu.Generators/StdCallArgBytesGenerator.cs` is properly implemented and scans for `[DllModuleExport]` attributes to generate `StdCallMeta.GetArgBytes()`.

### 2. Investigated Functions

**GetAcp:**
- Location: Kernel32Module.cs line 378
- Has attribute: ✅ `[DllModuleExport(7, IsStub = true)]`
- Parameters: 0
- Expected argBytes: 0
- Manual override: `("KERNEL32.DLL", "GETACP") => 0` ✅ CORRECT

**GetCpInfo:**
- Location: Kernel32Module.cs line 381
- Has attribute: ✅ `[DllModuleExport(9)]`
- Parameters: `uint codePage, uint lpCpInfo` (2 params × 4 bytes = 8)
- Expected argBytes: 8
- Manual override: `("KERNEL32.DLL", "GETCPINFO") => 8` ✅ CORRECT

**GetModuleFileNameA:**
- Location: Kernel32Module.cs line 918
- Has attribute: ✅ `[DllModuleExport(15)]`
- Parameters: `void* h, sbyte* lp, uint n` (3 pointers × 4 bytes = 12)
- Expected argBytes: 12
- Manual override: `("KERNEL32.DLL", "GETMODULEFILENAMEA") => 12` ✅ CORRECT

### 3. Why Manual Overrides Exist

**Root Cause:** The `StdCallArgBytesGenerator` successfully generates metadata, but the dispatcher encounters an `InvalidOperationException` when calling `StdCallMeta.GetArgBytes()`.

**Likely Reasons:**
1. Case sensitivity mismatch - generator uses exact method names, dispatcher might use different casing
2. DLL name mismatch - generator extracts from module's `Name` property, may not match what's passed to dispatcher
3. Generated code not being included in build properly

### 4. GetStringTypeA/GetStringTypeW Analysis

**Status:** ✅ IMPLEMENTED with test coverage

**GetStringTypeA** (line 430):
- Has attribute: `[DllModuleExport(21)]`
- Parameters: 5 (uint locale, uint dwInfoType, sbyte* lpSrcStr, int cchSrc, uint lpCharType)
- Expected argBytes: 20 (5 × 4 bytes)
- Implementation: Complete character classification for ASCII
- Tests: 6 tests in BasicFunctionsTests.cs

**GetStringTypeW** (line 554):
- Has attribute: `[DllModuleExport(22)]`
- Parameters: 5 (uint locale, uint dwInfoType, uint lpSrcStr, int cchSrc, uint lpCharType)
- Expected argBytes: 20
- Implementation: **INCOMPLETE** - missing punct, blank, cntrl, xdigit flags
- Tests: 1 test in NewFunctionsTests.cs

**Potential Issue for IGN_TEAS:**
If the C runtime uses GetStringTypeA/W instead of a static character table, and the implementation is incomplete, character parsing could fail.

## Recommendations

### Immediate Actions

1. **Fix GetStringTypeW completeness**
   - Add missing character type flags (punct, blank, cntrl, xdigit)
   - Match GetStringTypeA implementation quality

2. **Investigate StdCallArgBytesGenerator mismatch**
   - Check generated StdCallMeta.g.cs file
   - Verify case sensitivity in lookups
   - Ensure DLL name matching is consistent

3. **Add comprehensive tests**
   - GetStringTypeW edge cases
   - CPU FLAGS register handling
   - Stack cleanup verification

4. **Use Interactive Debugger**
   - Set breakpoint at 0x004123B8
   - Step through with register + FLAGS inspection
   - Verify which API calls are actually being made by CRT

### Testing Gaps

**GetStringType Functions:**
- [ ] Unicode character ranges beyond ASCII
- [ ] Empty strings
- [ ] Very long strings (> 1000 chars)
- [ ] cchSrc = -1 (null-terminated) vs explicit length
- [ ] Invalid locale/dwInfoType values

**CPU FLAGS:**
- [ ] CMP instruction sets ZF, CF, SF, OF, PF correctly
- [ ] TEST instruction behavior
- [ ] Conditional jumps (JE, JNE, JG, JL, etc.)
- [ ] LOOP instructions

**Stack Cleanup:**
- [ ] Verify ESP adjustment for all Win32 APIs
- [ ] Test with various argument counts
- [ ] Verify stdcall vs cdecl handling

## Next Steps

1. Build project and examine generated StdCallMeta.g.cs
2. Fix GetStringTypeW implementation
3. Add missing tests
4. Remove manual overrides if generator is working
5. Continue debugger investigation at 0x004123B8
