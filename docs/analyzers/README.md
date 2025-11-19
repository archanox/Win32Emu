# Win32Emu Roslyn Analyzers

This directory contains documentation for custom Roslyn analyzers used in the Win32Emu project.

## Available Analyzers

### WIN32EMU001: DuplicateOrdinalAnalyzer

Detects duplicate DLL export ordinals within Win32 module classes.

[View Documentation](./DuplicateOrdinalAnalyzer.md)

**Purpose**: Ensures that each DLL export ordinal is unique within a given DLL version.

**Severity**: Warning

**Example**:
```csharp
public class Kernel32Module
{
    [DllModuleExport(37)]  // ⚠️ Warning: duplicate ordinal
    private uint RtlUnwind(...) => ...;

    [DllModuleExport(37)]  // ⚠️ Warning: duplicate ordinal
    private uint GetCurrentThreadId() => ...;
}
```

## Running Analyzers

Analyzers run automatically during compilation. To see analyzer diagnostics:

```bash
# Build any project that references Win32Emu.Generators
dotnet build Win32Emu/Win32Emu.csproj

# Or build the entire solution
dotnet build
```

## Testing Analyzers

All analyzers have corresponding unit tests in the `Win32Emu.Tests.CodeGen` project.

```bash
# Run all analyzer tests
dotnet test Win32Emu.Tests.CodeGen

# Run specific analyzer tests
dotnet test Win32Emu.Tests.CodeGen --filter "DuplicateOrdinalAnalyzer"
```

## Creating New Analyzers

To create a new analyzer:

1. Add the analyzer class to `Win32Emu.Generators` project
2. Implement `DiagnosticAnalyzer` interface
3. Define a unique diagnostic ID (WIN32EMU0XX)
4. Create unit tests in `Win32Emu.Tests.CodeGen`
5. Document the analyzer in this directory

## Analyzer Configuration

Analyzers can be configured in `.editorconfig` files. For example:

```ini
# Disable a specific analyzer
dotnet_diagnostic.WIN32EMU001.severity = none

# Change severity to warning
dotnet_diagnostic.WIN32EMU001.severity = warning
```

## References

- [Roslyn Analyzer Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix)
- [Win32Emu.Generators Project](../../Win32Emu.Generators/)
- [Win32Emu.Tests.CodeGen Project](../../Win32Emu.Tests.CodeGen/)
