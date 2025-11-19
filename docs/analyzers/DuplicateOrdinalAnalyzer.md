# DuplicateOrdinalAnalyzer

## Overview

`DuplicateOrdinalAnalyzer` is a Roslyn diagnostic analyzer that detects duplicate DLL export ordinals within Win32 module classes in the Win32Emu project.

## Problem

In Win32 DLL modules, each exported function is identified by an ordinal number. The `DllModuleExportAttribute` is used to mark methods as DLL exports with their associated ordinal. Different versions of the same DLL can have different ordinal mappings, which is why the attribute supports a `Version` property.

**Valid scenario:**
```csharp
public class DDrawModule
{
    // Same method with different ordinals for different DLL versions
    [DllModuleExport(31, Version = "4.90.0.3000")]
    [DllModuleExport(9, Version = "5.1.2600.6532")]
    private uint DirectDrawCreate(...) => ...;
}
```

**Invalid scenario:**
```csharp
public class Kernel32Module
{
    [DllModuleExport(37)]  // Error: duplicate ordinal 37
    private uint RtlUnwind(...) => ...;

    [DllModuleExport(37)]  // Error: duplicate ordinal 37
    private uint GetCurrentThreadId() => ...;
}
```

## Diagnostic ID

**WIN32EMU001**: Duplicate DLL export ordinal

**Severity**: Warning (currently set as a warning until all existing duplicates are resolved)

## When is a Diagnostic Reported?

The analyzer reports a warning when:
1. Two or more methods in the same module class have `DllModuleExportAttribute` with the same ordinal
2. The attributes have the same version (or both have no version specified)

The analyzer does NOT report an error when:
- Same ordinal is used with different versions (this is valid)
- The class name doesn't end with "Module" (not analyzed)
- Different ordinals are used

## Message Format

```
Ordinal {ordinal} is used multiple times in module '{moduleName}' for version '{version}'
```

Where:
- `{ordinal}` is the duplicate ordinal number
- `{moduleName}` is the name of the module class
- `{version}` is the DLL version, or "(no version specified)" if no version is set

## Examples

### Example 1: Duplicate ordinals without version

```csharp
public class Advapi32Module : IWin32ModuleUnsafe
{
    [DllModuleExport(4)]  // ❌ Error
    private uint RegCloseKey(uint hKey) => ...;

    [DllModuleExport(4)]  // ❌ Error
    private uint RegFlushKey(uint hKey) => ...;
}
```

**Diagnostic**: `Ordinal 4 is used multiple times in module 'Advapi32Module' for version '(no version specified)'`

### Example 2: Duplicate ordinals with same version

```csharp
public class DDrawModule : IWin32ModuleUnsafe
{
    [DllModuleExport(10, Version = "5.1.2600.6532")]  // ❌ Error
    private uint Function1() => ...;

    [DllModuleExport(10, Version = "5.1.2600.6532")]  // ❌ Error
    private uint Function2() => ...;
}
```

**Diagnostic**: `Ordinal 10 is used multiple times in module 'DDrawModule' for version '5.1.2600.6532'`

### Example 3: Same ordinal with different versions (VALID)

```csharp
public class DDrawModule : IWin32ModuleUnsafe
{
    [DllModuleExport(31, Version = "4.90.0.3000")]
    [DllModuleExport(9, Version = "5.1.2600.6532")]
    private uint DirectDrawCreate(...) => ...;  // ✅ OK - different versions
}
```

No diagnostic is reported because the ordinals are for different DLL versions.

## How to Fix

To fix duplicate ordinal errors:

1. **Option 1**: Assign unique ordinals to each function
   ```csharp
   [DllModuleExport(4)]
   private uint RegCloseKey(uint hKey) => ...;

   [DllModuleExport(5)]  // Changed from 4 to 5
   private uint RegFlushKey(uint hKey) => ...;
   ```

2. **Option 2**: If the duplicates are version-specific, add version information
   ```csharp
   [DllModuleExport(4, Version = "5.0.0.0")]
   private uint RegCloseKey(uint hKey) => ...;

   [DllModuleExport(4, Version = "6.0.0.0")]  // Different version
   private uint RegFlushKey(uint hKey) => ...;
   ```

3. **Option 3**: Remove one of the duplicate methods if they serve the same purpose

## Testing

The analyzer includes comprehensive unit tests covering:
- No duplicates (should pass)
- Same ordinal with different versions (should pass)
- Duplicate ordinal with same version (should fail)
- Duplicate ordinal with no version specified (should fail)
- Multiple duplicate ordinals (should fail for all)
- Non-module classes (should not be analyzed)

Run tests with:
```bash
dotnet test Win32Emu.Tests.CodeGen --filter "DuplicateOrdinalAnalyzer"
```

## Implementation Details

The analyzer:
- Targets named types (classes) whose names end with "Module"
- Scans all methods for `DllModuleExportAttribute` or `DllModuleExport` attributes
- Groups attributes by `(ordinal, version)` tuple
- Reports diagnostics for any group with more than one method
- Reports the diagnostic at the attribute location for better error messaging

## Known Issues

As of the initial implementation, the analyzer has detected **1,462 duplicate ordinal errors** across the Win32Emu codebase. These should be reviewed and fixed by:
1. Consulting the actual Windows DLL export tables for the correct ordinals
2. Ensuring ordinals match the target Windows version being emulated
3. Verifying that different methods don't accidentally share the same ordinal

## References

- [DllModuleExportAttribute.cs](/Win32Emu/Win32/DllModuleExportAttribute.cs)
- [DuplicateOrdinalAnalyzer.cs](/Win32Emu.Generators/DuplicateOrdinalAnalyzer.cs)
- [DuplicateOrdinalAnalyzerTests.cs](/Win32Emu.Tests.CodeGen/DuplicateOrdinalAnalyzerTests.cs)
