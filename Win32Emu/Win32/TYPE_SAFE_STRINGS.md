# Win32 Type-Safe String Pointers

This document describes the type-safe string pointer wrappers used in the Win32Emu project to better match Windows API signatures.

## Overview

The Win32 API uses various string pointer types like `LPCSTR`, `LPSTR`, `LPCWSTR`, and `LPWSTR`. Instead of using raw unsafe pointers (`char*`, `sbyte*`), we provide type-safe wrapper structs that:

1. Better match the Windows API documentation
2. Provide compile-time type safety
3. Support nullable parameters
4. Make the code more readable and maintainable
5. Simplify testing by avoiding unsafe code where possible

## Available Types

### LpcStr

Represents `LPCSTR` (Long Pointer to Const String) - a read-only ANSI string pointer.

```csharp
// Windows API definition:
// typedef const char* LPCSTR;
// HMODULE GetModuleHandleA([in, optional] LPCSTR lpModuleName);

// Win32Emu implementation:
private uint GetModuleHandleA(in LpcStr lpModuleName)
{
    var moduleName = lpModuleName.IsNull ? null : _env.ReadAnsiString(lpModuleName.Address);
    // ...
}
```

**Features:**
- `IsNull` property to check if pointer is null
- `Read(VirtualMemory mem, int max = int.MaxValue)` method to read the string from memory
- `Address` property to access the raw address
- Implicit conversions to/from `uint`

### LpStr

Represents `LPSTR` (Long Pointer to String) - a writable ANSI string pointer.

```csharp
// Windows API definition:
// typedef char* LPSTR;
// DWORD GetModuleFileNameA([in] HMODULE hModule, [out] LPSTR lpFilename, [in] DWORD nSize);

// Win32Emu usage:
var buffer = new LpStr(0x10000);
buffer.Write(_env.Memory, "C:\\Program Files\\MyApp\\app.exe");
```

**Features:**
- `Write(VirtualMemory mem, string s, bool nullTerminate = true)` method to write strings to memory
- `Read(VirtualMemory mem, int max = int.MaxValue)` method to read strings from memory
- `Address` property to access the raw address
- Implicit conversions to/from `uint`

### LpWStr

Represents `LPWSTR` (Long Pointer to Wide String) - a writable Unicode string pointer.

```csharp
// Windows API definition:
// typedef wchar_t* LPWSTR;
// LPWSTR GetEnvironmentStringsW();

// Win32Emu usage:
var wideString = new LpWStr(0x20000);
wideString.Write(_env.Memory, "Unicode String");
```

**Features:**
- Uses Unicode (UTF-16) encoding
- `Write(VirtualMemory mem, string s, bool nullTerminate = true)` method
- `Read(VirtualMemory mem, int maxChars = int.MaxValue)` method
- `Address` property
- Implicit conversions to/from `uint`

## Usage in StackArgs

The `StackArgs` helper class provides convenient methods to extract these types from the stack:

```csharp
public readonly ref struct StackArgs
{
    // For const string parameters
    public LpcStr LpcStr(int index) => new LpcStr(UInt32(index));
    
    // For writable string parameters
    public LpStr LpStr(int index) => new LpStr(UInt32(index));
    
    // For wide string parameters
    public LpWStr LpWStr(int index) => new LpWStr(UInt32(index));
}
```

## Example: Updating a Win32 API

### Before (unsafe pointers):

```csharp
[DllModuleExport(16)]
private unsafe uint GetModuleHandleA(char* lpModuleName)
{
    _logger.LogInformation($"Getting module handle for '{(lpModuleName != null ? new string(lpModuleName) : "NULL (current process)")}'");
    return _imageBase;
}

// In TryInvokeUnsafe:
case "GETMODULEHANDLEA":
    returnValue = GetModuleHandleA(a.Lpcstr(0));
    return true;
```

### After (type-safe wrappers):

```csharp
[DllModuleExport(16)]
private uint GetModuleHandleA(in LpcStr lpModuleName)
{
    var moduleName = lpModuleName.IsNull ? null : _env.ReadAnsiString(lpModuleName.Address);
    _logger.LogInformation($"Getting module handle for '{moduleName ?? "NULL (current process)"}'");
    return _imageBase;
}

// In TryInvokeUnsafe:
case "GETMODULEHANDLEA":
    returnValue = GetModuleHandleA(a.LpcStr(0));
    return true;
```

## Benefits

1. **Type Safety**: The compiler can catch type mismatches at compile time
2. **Nullability**: Clear indication when a parameter can be null via `IsNull` property
3. **Readability**: Method signatures match Windows API documentation more closely
4. **Testability**: Easier to write unit tests without unsafe code
5. **Documentation**: Type names (`LpcStr`, `LpStr`, etc.) are self-documenting

## Migration Guide

When updating existing Win32 API implementations:

1. Identify the string parameter types from the Windows API documentation
   - `LPCSTR` → `LpcStr` (read-only)
   - `LPSTR` → `LpStr` (writable)
   - `LPCWSTR` → `LpcWStr` (read-only wide - future)
   - `LPWSTR` → `LpWStr` (writable wide)

2. Update the method signature:
   - Remove `unsafe` modifier if no longer needed
   - Change pointer type to wrapper type
   - Use `in` modifier for const parameters (optional but recommended)

3. Update the implementation:
   - Use `.IsNull` instead of `== null` checks
   - Use `.Read()` or `_env.ReadAnsiString(param.Address)` to read strings
   - Use `.Write()` to write strings
   - Use `.Address` when you need the raw pointer value

4. Update the `TryInvokeUnsafe` dispatch:
   - Change `a.Lpcstr(0)` → `a.LpcStr(0)` (const string)
   - Change `a.Lpstr(0)` → `a.LpStr(0)` (writable string)

## Future Enhancements

Potential additions to this system:

- `LpcWStr` for read-only wide strings
- Extension methods for common string operations
- Validation helpers (e.g., `IsValidStringPointer()`)
- Performance optimizations for string reading/writing
- Support for other Win32 types (e.g., `LPVOID`, `HANDLE`, etc.)
