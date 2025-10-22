# GetStringTypeW Blank Flag Bug Fix

## Issue Summary

The `GetStringTypeW` function in `Kernel32Module.cs` had a critical bug that prevented space and tab characters from receiving the `CT_CTYPE1_BLANK` flag, which is required by the Windows API specification.

## Root Cause

The bug was in the character classification logic at lines 999-1010 of `Kernel32Module.cs`:

```csharp
// BEFORE (BUGGY CODE):
else if (wchar is ' ' or '\t' or '\n' or '\r')
{
    charType = ctCtype1Space;
}
else if (wchar is ' ' or '\t')  // ❌ UNREACHABLE CODE
{
    charType |= ctCtype1Blank;
}
```

The second `else if` block was **unreachable** because space and tab characters were already handled by the first `else if` block. This meant that space and tab characters only received the `CT_CTYPE1_SPACE` flag but not the `CT_CTYPE1_BLANK` flag.

## The Fix

Changed the code to nest the blank flag check inside the space character block:

```csharp
// AFTER (FIXED CODE):
else if (wchar is ' ' or '\t' or '\n' or '\r')
{
    charType = ctCtype1Space;
    // Space and tab are also blank characters
    if (wchar is ' ' or '\t')
    {
        charType |= ctCtype1Blank;  // ✅ NOW SETS BOTH FLAGS
    }
}
```

## Windows API Specification

According to the Windows API documentation for `GetStringTypeW` with `CT_CTYPE1`:

| Character | Space Flag (0x0008) | Blank Flag (0x0040) | Notes |
|-----------|---------------------|---------------------|--------|
| Space (0x20) | ✅ Yes | ✅ Yes | Horizontal separator |
| Tab (0x09) | ✅ Yes | ✅ Yes | Horizontal separator |
| Newline (0x0A) | ✅ Yes | ❌ No | Vertical separator |
| Carriage Return (0x0D) | ✅ Yes | ❌ No | Vertical separator |

**Blank characters** are defined as horizontal white space (space and tab).
**Space characters** include all whitespace (space, tab, newline, carriage return, etc.).

## Impact on IGN_TEAS.EXE

This bug was identified as a potential root cause of the infinite loop in IGN_TEAS.EXE during C runtime initialization:

1. The C runtime uses `GetStringTypeW` for command-line parsing
2. Command-line parsing needs to identify blank characters (space/tab) to separate arguments
3. Without the blank flag, the parser may not correctly identify token boundaries
4. This could cause the parser to enter an infinite loop or fail to parse arguments correctly

## Test Coverage

Added 5 comprehensive tests in `GetStringTypeWBlankFlagTests.cs`:

1. ✅ `GetStringTypeW_WithSpace_ShouldHaveBothSpaceAndBlankFlags` - Verifies space has both flags
2. ✅ `GetStringTypeW_WithTab_ShouldHaveBothSpaceAndBlankFlags` - Verifies tab has both flags  
3. ✅ `GetStringTypeW_WithNewline_ShouldHaveSpaceButNotBlankFlag` - Verifies newline has only space flag
4. ✅ `GetStringTypeW_WithCarriageReturn_ShouldHaveSpaceButNotBlankFlag` - Verifies CR has only space flag
5. ✅ `GetStringTypeW_WithMixedWhitespace_ShouldClassifyCorrectly` - Verifies all whitespace types together

All tests pass ✅

## Comparison with GetStringTypeA

`GetStringTypeA` (ANSI version) does NOT have this bug. It correctly sets both flags:

```csharp
// GetStringTypeA (CORRECT):
else if (ch == ' ' || ch == '\t')
{
    charType |= ctCtype1Space | ctCtype1Blank;  // ✅ BOTH FLAGS SET
}
```

## Related Documentation

- `IGN_TEAS_INFINITE_LOOP_INVESTIGATION.md` - Documents the investigation into the infinite loop
- `IGN_TEAS_MISSING_FEATURES.md` - Lists missing features but doesn't mention this specific bug
- `ARGBYTES_INVESTIGATION.md` - Documents argBytes issues (ruled out as cause)

## Next Steps

1. Test if this fix resolves the IGN_TEAS infinite loop
2. If the loop persists, investigate other potential causes:
   - CPU FLAGS handling (compare, test, conditional jumps)
   - Stack corruption
   - Other character classification edge cases

## Files Changed

1. `Win32Emu/Win32/Modules/Kernel32Module.cs` - Fixed blank flag logic
2. `Win32Emu.Tests.Kernel32/GetStringTypeWBlankFlagTests.cs` - Added test coverage (NEW FILE)

## Verification

```bash
# Run the new tests
dotnet test Win32Emu.Tests.Kernel32/Win32Emu.Tests.Kernel32.csproj \
    --filter "FullyQualifiedName~GetStringTypeWBlankFlag"

# All 5 tests pass ✅
```
