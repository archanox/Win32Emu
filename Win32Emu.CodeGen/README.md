# Win32Emu.CodeGen - API Metadata Parser and Stub Generator

This tool provides automated analysis and code generation capabilities for Win32 API implementation in the Win32Emu emulator.

## Features

### 1. PE DLL Export Parser
- Parses export tables from PE DLL files using AsmResolver
- Supports both WinME and WinXP DLL analysis
- Extracts function names, ordinals, entry points, and forwarded exports
- Handles exports by name and by ordinal
- **NEW:** Groups exports by DLL name across multiple versions
- **NEW:** Extracts entry point addresses (RVAs) for each export
- **NEW:** Case-insensitive DLL file search

### 2. API Monitor XML Parser
- Parses API Monitor XML definition files from the `ApiMon XMLs` directory
- Extracts function signatures, parameters, and types
- Calculates expected argument bytes for stdcall convention
- Compatible with definitions from https://github.com/jozefizso/apimonitor
- **NEW:** Handles `BothCharset` attribute to generate both A and W function variants
- **NEW:** Integrated directly into stub generation workflow

### 3. Metadata Database
- Stores and queries API metadata from multiple sources
- Combines PE DLL exports with implemented APIs
- Generates coverage reports showing implementation status

### 4. Implemented API Extractor
- Reads generated `StdCallMeta` from compiled Win32Emu assembly
- Extracts which APIs are already implemented
- Provides argument byte information

### 5. Auto-Stub Generator
- Generates C# method stubs for APIs across multiple DLL versions
- Creates properly attributed methods with `[DllModuleExport]`
- **NEW:** Generates multiple `[DllModuleExport]` attributes for functions across versions
- **NEW:** Populates `entryPoint` parameter with actual RVA addresses
- **NEW:** Adds `ExportName` field for non-C#-compatible function names
- **NEW:** Uses `_logger.LogWarning` instead of `Diagnostics.Diagnostics.LogWarn`
- **NEW:** Generates parameter-aware logging with proper formatting
- **NEW:** Generates ALL exports (not just missing ones)
- Adds TODO comments for future implementation
- Can generate complete module classes with ILogger support

## Usage

### Analyze DLL Exports

```bash
dotnet run --project Win32Emu.CodeGen -- analyze-dlls --dll-dir DLLs/WinME --output exports_report.txt
```

### Generate Coverage Report

```bash
dotnet run --project Win32Emu.CodeGen -- coverage-report \
  --winme DLLs/WinME \
  --winxp DLLs/WinXP \
  --assembly Win32Emu/bin/Debug/net9.0/Win32Emu.dll \
  --output coverage_report.txt
```

Example output:
```
API Coverage Report
===================

Overall Coverage: 59/3003 (2.0%)

KERNEL32.DLL
  Exports: 1181
  Implemented: 49 (4.1%)
  Implemented APIs: CloseHandle, CreateFileA, ExitProcess, ...
  Sample Missing APIs: _DebugOut, _DebugPrintf, _hread, ...
```

### Generate Stubs for APIs

**NEW:** The tool now automatically:
- Searches for the DLL in both WinME and WinXP directories (case-insensitive)
- Loads API definitions from `ApiMon XMLs/Windows/` directory
- Groups exports by function name across versions
- Generates complete parameter lists from XML definitions

Generate method stubs only:
```bash
dotnet run --project Win32Emu.CodeGen -- generate-stubs \
  --dll KERNEL32.DLL \
  --output KernelStubs.cs
```

Generate complete module class:
```bash
dotnet run --project Win32Emu.CodeGen -- generate-stubs \
  --dll USER32.DLL \
  --output User32Module.cs \
  --module-class
```

Example generated stub with parameters and multi-version support:
```csharp
[DllModuleExport(1, entryPoint: 0x00001371, IsStub = true)]
[DllModuleExport(1, entryPoint: 0x00018673, IsStub = true)]
public uint SetWindowPos(uint hWnd, uint hWndInsertAfter, uint X, uint Y, uint cx, uint cy, uint uFlags)
{
    _logger.LogWarning("[USER32] SetWindowPos: hWnd=0x{hWnd:X8}, hWndInsertAfter=0x{hWndInsertAfter:X8}, X={X}, Y={Y}, cx={cx}, cy={cy}, uFlags=0x{uFlags:X8}", hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
    // TODO: Implement SetWindowPos
    return 0; // DWORD default
}
```

Example with decorated export name:
```csharp
[DllModuleExport(12, entryPoint: 0x00001230, ExportName = "_grBufferClear@12", IsStub = true)]
public uint grBufferClear()
{
    _logger.LogWarning("[GLIDE2X] grBufferClear called (stub)");
    // TODO: Implement _grBufferClear@12
    return 0; // DWORD default
}
```

### Parse API Monitor XML Files

```bash
dotnet run --project Win32Emu.CodeGen -- parse-xml --xml-dir "ApiMon XMLs/Windows"
```

## Command Reference

### `analyze-dlls`
Analyze PE DLL exports and generate a detailed report.

**Options:**
- `--dll-dir` - Directory containing DLLs (default: `DLLs/WinME`)
- `--output` - Output file (optional, defaults to console)

### `coverage-report`
Generate API coverage report comparing implemented vs available APIs.

**Options:**
- `--winme` - Path to WinME DLLs (default: `DLLs/WinME`)
- `--winxp` - Path to WinXP DLLs (default: `DLLs/WinXP`)
- `--assembly` - Path to Win32Emu.dll to extract implemented APIs (optional)
- `--output` - Output file (optional, defaults to console)

### `generate-stubs`
Generate C# stub methods for APIs across multiple DLL versions.

**Options:**
- `--dll` - DLL name to generate stubs for (required, e.g., `KERNEL32.DLL`)
- `--output` - Output file (default: `GeneratedStubs.cs`)
- `--module-class` - Generate complete module class instead of just methods

**Behavior Changes:**
- **Removed:** `--winme` and `--assembly` options (tool now searches all DLL directories automatically)
- **NEW:** Automatically searches `DLLs/WinME` and `DLLs/WinXP` directories
- **NEW:** Automatically loads API definitions from `ApiMon XMLs/Windows/`
- **NEW:** Generates stubs for ALL exports (not just missing ones)

### `parse-xml`
Parse API Monitor XML definition files.

**Options:**
- `--xml-dir` - Directory containing XML files (required)

## Architecture

### Class Structure

```
Win32Emu.CodeGen/
├── ApiMetadata/
│   ├── PeExportParser.cs         # Parse PE DLL exports (enhanced)
│   ├── XmlParser.cs               # Parse API Monitor XML (enhanced)
│   ├── MetadataDatabase.cs        # Store/query metadata
│   ├── ImplementedApiExtractor.cs # Extract from compiled assembly
│   └── StubGenerator.cs           # Generate C# stubs (enhanced)
└── Program.cs                     # CLI commands (updated)
```

### Key Enhancements

#### ExportedFunction Record
```csharp
public record ExportedFunction(
    string Name,
    uint Ordinal,
    string? ForwardedTo,
    uint? EntryPoint = null,    // NEW: RVA entry point
    string? Version = null      // NEW: DLL version (TODO)
);
```

#### DllModuleExportAttribute
```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class DllModuleExportAttribute : Attribute
{
    public uint Ordinal { get; }
    public uint? EntryPoint { get; }
    public string? Version { get; init; }
    public string? ForwardedTo { get; init; }
    public string? ExportName { get; init; }  // NEW: Original export name
    public bool IsStub { get; init; }
}
```

### Data Flow

1. **PE DLL Parsing** → Extracts all exports with entry points from multiple versions
2. **XML Parsing** → Loads API definitions from ApiMon XMLs
3. **Export Grouping** → Groups exports by function name across versions
4. **Stub Generation** → Creates C# code with complete signatures and multi-version attributes

## Testing

The project includes comprehensive tests:

```bash
dotnet test Win32Emu.Tests.CodeGen
```

**Test Coverage:**
- PeExportParser (4 tests)
- MetadataDatabase (4 tests)
- StubGenerator (7 tests) - **Enhanced with multi-version and decorated name tests**
- **Total: 15 tests, 100% passing**

## Dependencies

- **AsmResolver.PE** (6.0.0-beta.4) - PE file parsing
- **System.CommandLine** (2.0.0-beta4) - CLI framework
- **.NET 9.0** - Target framework

## Future Enhancements

1. **Version Resource Extraction**
   - Extract actual version strings from PE file resources
   - Populate the `Version` field in `[DllModuleExport]` attributes

2. **Intelligent Stub Generation**
   - Infer more accurate parameter types
   - Generate more realistic default return values
   - Add parameter validation

3. **Coverage Tracking**
   - Generate HTML coverage reports
   - Track coverage trends over time
   - Identify high-priority APIs to implement

4. **Validation**
   - Compare StdCallMeta against PE DLL exports
   - Identify argument byte mismatches
   - Detect missing or extra exports

## License

Part of the Win32Emu project. See LICENSE in the repository root.
