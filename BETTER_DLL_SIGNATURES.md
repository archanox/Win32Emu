# Better DLL Module Method Signatures - Implementation Summary

## Overview

This implementation enhances the type safety of Win32 API method signatures in the Win32Emu project by introducing wrapper types that better match the Microsoft Windows API documentation.

## Problem Statement

Previously, DLL module methods used raw unsafe pointers:

```csharp
case "GETMODULEHANDLEA":
    returnValue = GetModuleHandleA(a.Lpcstr(0));  // unsafe char*
    return true;

private unsafe uint GetModuleHandleA(char* name)
{
    return imageBase;
}
```

This approach had several issues:
- Required `unsafe` keyword throughout the codebase
- Difficult to determine if parameters could be null
- Didn't match Windows API documentation
- Made testing harder

## Solution

Introduced type-safe wrapper types that match Windows API conventions:

### LpcStr (LPCSTR)

Represents `const char*` - a read-only ANSI string pointer.

```csharp
public readonly struct LpcStr
{
    public readonly uint Address;
    public bool IsNull => Address == 0;
    public string? Read(VirtualMemory mem, int max = int.MaxValue);
}
```

**Windows API Example:**
```c
HMODULE GetModuleHandleA(
  [in, optional] LPCSTR lpModuleName
);
```

**Win32Emu Implementation:**
```csharp
private uint GetModuleHandleA(in LpcStr lpModuleName)
{
    var moduleName = lpModuleName.IsNull ? null : _env.ReadAnsiString(lpModuleName.Address);
    _logger.LogInformation($"Getting module handle for '{moduleName ?? "NULL (current process)"}'");
    return _imageBase;
}
```

### LpStr (LPSTR)

Represents `char*` - a writable ANSI string pointer (already existed, now integrated).

```csharp
public readonly struct LpStr(uint address)
{
    public readonly uint Address = address;
    public void Write(VirtualMemory mem, string s, bool nullTerminate = true);
    public string Read(VirtualMemory mem, int max = int.MaxValue);
}
```

### LpWStr (LPWSTR)

Represents `wchar_t*` - a writable Unicode string pointer (already existed, now integrated).

```csharp
public readonly struct LpWStr(uint address)
{
    public readonly uint Address = address;
    public void Write(VirtualMemory mem, string s, bool nullTerminate = true);
    public string Read(VirtualMemory mem, int maxChars = int.MaxValue);
}
```

## Implementation Details

### 1. New Type: LpcStr.cs

Created `/Win32Emu/Win32/LpcStr.cs` with:
- Null pointer detection via `IsNull` property
- String reading from virtual memory
- Implicit conversions to/from `uint`
- Full XML documentation

### 2. Updated StackArgs.cs

Added helper methods to extract new types from the stack:

```csharp
public LpcStr LpcStr(int index) => new LpcStr(UInt32(index));
public LpStr LpStr(int index) => new LpStr(UInt32(index));
public LpWStr LpWStr(int index) => new LpWStr(UInt32(index));
```

### 3. Updated Kernel32Module.cs

Migrated two methods as examples:

**GetModuleHandleA:**
- Before: `unsafe uint GetModuleHandleA(char* lpModuleName)`
- After: `uint GetModuleHandleA(in LpcStr lpModuleName)`

**LoadLibraryA:**
- Before: `unsafe uint LoadLibraryA(sbyte* lpLibFileName)`
- After: `uint LoadLibraryA(in LpcStr lpLibFileName)`

### 4. Comprehensive Testing

Created `LpcStrTests.cs` with 10 unit tests covering:
- Null pointer handling
- String reading with various inputs
- Max length limits
- Implicit conversions
- Special characters
- Null terminator handling

**Test Results:** ✅ All 10 tests passing

### 5. Documentation

Created `TYPE_SAFE_STRINGS.md` with:
- Overview of the type system
- Detailed API documentation
- Usage examples
- Migration guide
- Future enhancements

## Benefits

### Type Safety
```csharp
// Before: Compiler can't help
private unsafe uint GetModuleHandleA(char* lpModuleName)

// After: Clear intent and type checking
private uint GetModuleHandleA(in LpcStr lpModuleName)
```

### Nullability
```csharp
// Before: Unclear if null is valid
if (lpModuleName == null) { ... }

// After: Explicit null checking
if (lpModuleName.IsNull) { ... }
```

### Documentation Alignment
```csharp
// Windows API:
// HMODULE GetModuleHandleA([in, optional] LPCSTR lpModuleName);

// Our implementation uses matching types:
private uint GetModuleHandleA(in LpcStr lpModuleName)
//                              ^^ matches [in]
//                                 ^^^^^^ matches LPCSTR
```

### Reduced Unsafe Code
```csharp
// Before: unsafe keyword required
private unsafe uint LoadLibraryA(sbyte* lpLibFileName)
{
    if (lpLibFileName == null) return 0;
    var libraryName = _env.ReadAnsiString((uint)lpLibFileName);
    // ...
}

// After: no unsafe needed
private uint LoadLibraryA(in LpcStr lpLibFileName)
{
    if (lpLibFileName.IsNull) return 0;
    var libraryName = _env.ReadAnsiString(lpLibFileName.Address);
    // ...
}
```

## Test Coverage

### Module Tests
- `GetModuleHandleA_WithNullModuleName_ShouldReturnImageBase` ✅
- `GetModuleHandleA_WithKernel32_ShouldReturnKernel32Handle` ✅
- `GetModuleHandleA_WithInvalidModuleName_ShouldReturnZero` ✅
- `LoadLibraryA_WithNullLibraryName_ShouldReturnZero` ✅
- `LoadLibraryA_WithEmptyLibraryName_ShouldReturnZero` ✅
- `LoadLibraryA_WithSystemDLL_ShouldReturnNonZeroHandle` ✅
- `LoadLibraryA_LocalDLL_ShouldLoadForEmulation` ✅

### Type Tests
- All 10 LpcStr unit tests ✅

## Migration Pattern

To update other Win32 API methods:

### Step 1: Identify the type
Check Microsoft documentation for parameter types:
- `LPCSTR` → `LpcStr`
- `LPSTR` → `LpStr`
- `LPCWSTR` → `LpcWStr` (future)
- `LPWSTR` → `LpWStr`

### Step 2: Update method signature
```csharp
// Before
private unsafe uint MyFunction(char* lpParam)

// After
private uint MyFunction(in LpcStr lpParam)
```

### Step 3: Update null checks
```csharp
// Before
if (lpParam == null)

// After
if (lpParam.IsNull)
```

### Step 4: Update string access
```csharp
// Before
var str = _env.ReadAnsiString((uint)lpParam);

// After
var str = _env.ReadAnsiString(lpParam.Address);
```

### Step 5: Update dispatch call
```csharp
// Before
case "MYFUNCTION":
    returnValue = MyFunction(a.Lpcstr(0));

// After
case "MYFUNCTION":
    returnValue = MyFunction(a.LpcStr(0));
```

## Future Enhancements

### Additional Types
- `LpcWStr` for read-only wide strings (LPCWSTR)
- Strongly-typed return values (e.g., methods returning HMODULE should return NativeTypes.HModule)
- Support for other Win32 types (HANDLE, LPVOID, etc.)

### Performance Optimizations
- Caching of frequently-read strings
- Span-based reading for better performance
- Optional validation helpers

### Developer Experience
- Extension methods for common operations
- Better error messages
- IntelliSense documentation

## Files Modified

1. `/Win32Emu/Win32/LpcStr.cs` - New file
2. `/Win32Emu/Win32/StackArgs.cs` - Added LpcStr and LpStr methods
3. `/Win32Emu/Win32/Modules/Kernel32Module.cs` - Updated GetModuleHandleA and LoadLibraryA
4. `/Win32Emu.Tests.Kernel32/LpcStrTests.cs` - New test file
5. `/Win32Emu/Win32/TYPE_SAFE_STRINGS.md` - New documentation

## Conclusion

This implementation provides a solid foundation for improving method signatures throughout the Win32Emu project. The pattern is:
- **Simple**: Easy to understand and apply
- **Type-safe**: Compiler-enforced correctness
- **Documented**: Clear examples and migration guide
- **Tested**: Comprehensive test coverage
- **Incremental**: Can be applied gradually without breaking existing code

The changes maintain backward compatibility (old unsafe pointer methods in StackArgs still exist) while providing a clearer, safer path forward.
