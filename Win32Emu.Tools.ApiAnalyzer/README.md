# Win32Emu.Tools.ApiAnalyzer

A proof-of-concept tool that demonstrates the value of integrating Reko's API definitions with Win32Emu.

## Purpose

This tool analyzes Win32 API coverage by comparing:
- **Reko's XML API definitions** - Comprehensive, machine-readable specifications from the Reko decompiler project
- **Win32Emu's module implementations** - Current API implementations in Win32Emu

## What It Does

1. **Parses Reko XML files** - Extracts API procedure definitions from XML files
2. **Analyzes Win32Emu modules** - Scans C# module files for `[DllModuleExport]` methods
3. **Generates coverage report** - Shows which APIs are implemented, missing, or have extra implementations

## Usage

### Prerequisites

1. Clone the Reko repository:
   ```bash
   git clone https://github.com/uxmal/reko.git /tmp/reko
   ```

### Run the Analyzer

```bash
cd Win32Emu.Tools.ApiAnalyzer
dotnet run -- /tmp/reko/src/Environments/Windows ../Win32Emu/Win32/Modules
```

### Sample Output

```
Win32Emu API Coverage Analyzer
================================

Step 1: Parsing Reko XML API definitions...
  Loaded 64 APIs from kernel32.xml
  Loaded 60 APIs from user32.xml
  Loaded 45 APIs from gdi32.xml
  ...

Step 2: Analyzing Win32Emu module implementations...
  Found 52 APIs in Kernel32Module.cs
  Found 48 APIs in User32Module.cs
  ...

Step 3: Generating Coverage Report...

kernel32.dll:
  Total APIs in Reko: 64
  Implemented in Win32Emu: 52 (81.3%)
  Missing: 12
  Sample missing APIs: CreateFileMappingA, MapViewOfFile, UnmapViewOfFile...

user32.dll:
  Total APIs in Reko: 60
  Implemented in Win32Emu: 48 (80.0%)
  Missing: 12
  ...

Overall Summary
===============
Total APIs in Reko definitions: 324
Total implemented in Win32Emu: 245
Total missing: 79
Overall coverage: 75.6%
```

## Value Demonstration

This tool demonstrates how Reko's API definitions could be used to:

1. **Track API coverage** - See which Windows APIs are implemented vs. missing
2. **Prioritize implementation** - Focus on commonly-used missing APIs
3. **Validate signatures** - Ensure parameter types match Windows specifications
4. **Generate stubs** - Auto-create skeleton implementations for missing APIs
5. **Maintain quality** - Catch regressions in API coverage over time

## Integration Opportunities

See [REKO_INTEGRATION_ANALYSIS.md](../REKO_INTEGRATION_ANALYSIS.md) for a comprehensive analysis of how Reko could enhance Win32Emu.

### Potential Enhancements

1. **Signature validation** - Compare parameter types and counts
2. **Stub generation** - Auto-generate skeleton code for missing APIs
3. **Documentation generation** - Create API docs from XML
4. **Test generation** - Create unit test templates
5. **CI/CD integration** - Track coverage over time in build pipeline

## Technical Details

### Reko XML Format

Reko's API definitions use XML with this structure:

```xml
<library xmlns="http://schemata.jklnet.org/Decompiler">
  <Types>
    <typedef name="DWORD">
      <prim domain="UnsignedInt" size="4" />
    </typedef>
    <!-- more types... -->
  </Types>
  
  <procedure name="CreateWindowExA">
    <signature>
      <return>
        <type>HWND</type>
        <reg>eax</reg>
      </return>
      <arg name="dwExStyle">
        <type>DWORD</type>
        <stack size="4" />
      </arg>
      <!-- more args... -->
    </signature>
  </procedure>
</library>
```

### Win32Emu Module Format

Win32Emu implements APIs with this pattern:

```csharp
[DllModuleExport]
public uint CreateWindowExA(
    uint dwExStyle,
    uint lpClassName,
    // ... more parameters
)
{
    // Implementation...
}
```

## Limitations

This is a **proof-of-concept** with limitations:

- Basic regex parsing of C# files (doesn't use Roslyn)
- Manual DLL name mapping
- No signature comparison (only API name matching)
- Doesn't handle conditional compilation or variants

A production version would:
- Use Roslyn for accurate C# parsing
- Parse type definitions from XML
- Validate signatures and parameter types
- Generate actionable reports (JSON/HTML/Markdown)
- Integrate with CI/CD for automated tracking

## License Considerations

**Important**: Reko is licensed under GPLv2+. This tool:
- Uses Reko's XML files as **data/specification** (not code)
- Implements parsing independently (no Reko code copied)
- Generates analysis reports only (no code generation yet)

The XML API definitions are factual information about Windows APIs and may not be subject to GPL (consult legal counsel if uncertain).

## See Also

- [Reko Project](https://github.com/uxmal/reko)
- [Reko Integration Analysis](../REKO_INTEGRATION_ANALYSIS.md)
- [Win32Emu Documentation](../README.md)
