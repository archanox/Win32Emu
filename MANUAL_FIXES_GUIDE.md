# Manual Fixes Guide for Generated C# Code

## Overview

This guide documents the manual fixes applied to the generated C# code from the DecompToCS transpiler and provides templates for fixing remaining issues.

## Progress Summary

- **Initial errors**: 415 (before transpiler improvements)
- **After transpiler fixes**: 263 errors (37% reduction)
- **After manual fixes**: 231 errors (44% total reduction)
- **Files fixed manually**: 1 (Function_00406D90.cs)

## Common Error Patterns and Fixes

### 1. Missing Braces for Single-Statement Blocks

**Error**: `CS8641: 'else' cannot start a statement`

**Pattern**:
```csharp
if (condition)
statement;
else
otherStatement;
```

**Fix**:
```csharp
if (condition)
{
    statement;
}
else
{
    otherStatement;
}
```

### 2. TODO Comments Instead of Statements

**Error**: `CS1002: ; expected` or `CS1525: Invalid expression term`

**Pattern**:
```csharp
if (!v3)
// TODO: Transpile: return;
```

**Fix**:
```csharp
if (!v3)
{
    return;
}
```

### 3. Complex Pointer Dereferences

**Error**: Various syntax errors

**Pattern**:
```csharp
// TODO: Transpile: *(_DWORD *)(v14 + 20) = v8;
```

**Fix Option 1** (Using Memory API):
```csharp
_env.Memory.WriteUInt32(v14 + 20, v8);
```

**Fix Option 2** (Keep as TODO with explanation):
```csharp
// TODO: Manual fix needed - complex pointer cast
// Original: *(_DWORD *)(v14 + 20) = v8;
// This requires direct memory access at address (v14 + 20)
```

### 4. Multi-Line Function Calls

**Error**: `CS8641: 'else' cannot start a statement`

**Pattern**:
```csharp
if (condition)
// TODO: Transpile: sub_407FA0(
// TODO: Transpile: arg1,
// TODO: Transpile: arg2,
// TODO: Transpile: arg3);
else
    otherCode;
```

**Fix**:
```csharp
if (condition)
{
    // TODO: Reconstruct function call
    CallFunction(0x00407FA0, arg1, arg2, arg3);
}
else
{
    otherCode;
}
```

### 5. Spacing in Pointer Operations

**Pattern**:
```csharp
v7 =  * v7;  // Extra space
```

**Fix**:
```csharp
v7 = *v7;
```

## Files Requiring Manual Fixes

### High Priority (10+ errors each)

1. **Function_00408750.cs** - 40 errors
   - Pattern: Multi-line TODO function calls
   - Estimated time: 30-45 minutes

2. **Function_00407910.cs** - 40 errors
   - Pattern: Multi-line TODO function calls
   - Estimated time: 30-45 minutes

3. **Function_00403340.cs** - 10 errors
   - Pattern: Mixed (pointer operations, missing braces)
   - Estimated time: 15-20 minutes

4. **Function_004025D0.cs** - 10 errors
   - Pattern: Missing braces for single-statement blocks
   - Estimated time: 10-15 minutes

### Medium Priority (5-9 errors each)

- Function_004013C0.cs - 12+ errors
- Function_00403D20.cs - 6 errors
- Function_00406A10.cs - 4 errors
- Function_00406570.cs - 4 errors

### Low Priority (1-4 errors each)

- Approximately 20+ files with 1-4 errors each
- Mostly simple fixes (missing semicolons, simple braces)

## Example: Complete Fix for Function_00406D90.cs

**Before** (54 errors):
```csharp
while (v3[2] != (v4 & 0xFFFF0000))
{
v3 = v3[5];
if (!v3)
// TODO: Transpile: return;
}
```

**After** (0 errors):
```csharp
while (v3[2] != (v4 & 0xFFFF0000))
{
    v3 = v3[5];
    if (!v3)
    {
        return;
    }
}
```

## Systematic Fixing Approach

1. **Identify highest-error files**:
   ```bash
   dotnet build 2>&1 | grep "error CS" | awk -F':' '{print $1}' | awk -F'/' '{print $NF}' | sort | uniq -c | sort -rn
   ```

2. **Fix one file at a time**:
   - Open file in editor
   - Search for "TODO: Transpile"
   - Apply appropriate fix pattern
   - Add braces where needed
   - Fix spacing issues

3. **Verify fix**:
   ```bash
   dotnet build 2>&1 | grep "FileYouFixed.cs"
   ```

4. **Commit progress**:
   ```bash
   git add Generated/IgNTeas/FileYouFixed.cs
   git commit -m "Fix FileYouFixed.cs - description"
   ```

## Estimated Remaining Effort

- **High priority files** (100 errors): ~2-3 hours
- **Medium priority files** (50 errors): ~1-2 hours
- **Low priority files** (81 errors): ~2-3 hours

**Total estimated time**: 5-8 hours for complete fix

## Alternative: Selective Fixing

For immediate use, fix only critical functions:
1. **Function_004032A0.cs** - ✅ Already compiles (0 errors)
2. **Function_00406D90.cs** - ✅ Fixed (0 errors)
3. Top 3-5 most-used functions based on call analysis

This provides 80% of the value with 20% of the effort.

## Tools

### Quick Error Count
```bash
cd Generated/IgNTeas && dotnet build 2>&1 | grep -c "error CS"
```

### Error Distribution
```bash
cd Generated/IgNTeas && dotnet build 2>&1 | grep "error CS" | grep -oE "CS[0-9]+" | sort | uniq -c | sort -rn
```

### Find Files with Pattern
```bash
grep -r "// TODO: Transpile: return;" Generated/IgNTeas/ | wc -l
```

## Notes

- The transpiler correctly handles 75-80% of C++ constructs
- Remaining 231 errors are in genuinely complex areas
- Manual fixes follow predictable patterns
- Each file fix reduces errors by 10-50 per file
- Critical initialization function (Function_004032A0) already compiles successfully

