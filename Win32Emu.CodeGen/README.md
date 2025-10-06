# Win32Emu.CodeGen - API Metadata Parser and Stub Generator

This tool provides automated analysis and code generation capabilities for Win32 API implementation in the Win32Emu emulator.

## Features

### 1. PE DLL Export Parser
- Parses export tables from PE DLL files using AsmResolver
- Supports both WinME and WinXP DLL analysis
- Extracts function names, ordinals, and forwarded exports
- Handles exports by name and by ordinal

### 2. API Monitor XML Parser
- Ready to parse API Monitor XML definition files
- Extracts function signatures, parameters, and types
- Calculates expected argument bytes for stdcall convention
- Compatible with definitions from https://github.com/jozefizso/apimonitor

### 3. Metadata Database
- Stores and queries API metadata from multiple sources
- Combines PE DLL exports with implemented APIs
- Generates coverage reports showing implementation status

### 4. Implemented API Extractor
- Reads generated `StdCallMeta` from compiled Win32Emu assembly
- Extracts which APIs are already implemented
- Provides argument byte information

### 5. Auto-Stub Generator
- Generates C# method stubs for missing APIs
- Creates properly attributed methods with `[DllModuleExport]`
- Adds logging and TODO comments
- Can generate complete module classes

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

### Generate Stubs for Missing APIs

Generate method stubs only:
```bash
dotnet run --project Win32Emu.CodeGen -- generate-stubs \
  --dll DPLAYX.DLL \
  --output DPlayXStubs.cs \
  --assembly Win32Emu/bin/Debug/net9.0/Win32Emu.dll
```

Generate complete module class:
```bash
dotnet run --project Win32Emu.CodeGen -- generate-stubs \
  --dll dinput8.dll \
  --output DInput8Module.cs \
  --module-class \
  --assembly Win32Emu/bin/Debug/net9.0/Win32Emu.dll
```

Example generated stub:
```csharp
[DllModuleExport]
public uint DirectInput8Create()
{
    Diagnostics.Diagnostics.LogWarn("Stub called: DirectInput8Create()");
    // TODO: Implement DirectInput8Create
    return 0; // DWORD default
}
```

### Parse API Monitor XML Files

```bash
dotnet run --project Win32Emu.CodeGen -- parse-xml --xml-dir path/to/apimonitor/xml
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
Generate C# stub methods for missing APIs.

**Options:**
- `--dll` - DLL name to generate stubs for (required, e.g., `ADVAPI32.DLL`)
- `--output` - Output file (default: `GeneratedStubs.cs`)
- `--module-class` - Generate complete module class instead of just methods
- `--winme` - Path to WinME DLLs directory (default: `DLLs/WinME`)
- `--assembly` - Path to Win32Emu.dll to determine which APIs are already implemented

### `parse-xml`
Parse API Monitor XML definition files.

**Options:**
- `--xml-dir` - Directory containing XML files (required)

## Architecture

### Class Structure

```
Win32Emu.CodeGen/
├── ApiMetadata/
│   ├── PeExportParser.cs         # Parse PE DLL exports
│   ├── XmlParser.cs               # Parse API Monitor XML
│   ├── MetadataDatabase.cs        # Store/query metadata
│   ├── ImplementedApiExtractor.cs # Extract from compiled assembly
│   └── StubGenerator.cs           # Generate C# stubs
└── Program.cs                     # CLI commands
```

### Data Flow

1. **PE DLL Parsing** → Extracts all available exports
2. **Assembly Analysis** → Identifies implemented APIs
3. **Metadata Database** → Combines data from multiple sources
4. **Coverage Analysis** → Calculates implementation percentages
5. **Stub Generation** → Creates C# code for missing APIs

## Current Coverage Statistics

As of the latest analysis:

- **Total Exports:** 3,003 across all DLLs
- **Implemented:** 59 (2.0%)
- **Top Coverage:**
  - DPLAYX.DLL: 2/11 (18.2%)
  - KERNEL32.DLL: 49/1,181 (4.1%)
  - USER32.DLL: 8/756 (1.1%)

## Testing

The project includes comprehensive tests:

```bash
dotnet test Win32Emu.Tests.CodeGen
```

**Test Coverage:**
- PeExportParser (4 tests)
- MetadataDatabase (4 tests)
- StubGenerator (5 tests)
- **Total: 13 tests, 100% passing**

## Future Enhancements

1. **API Monitor XML Integration**
   - Download XML files from apimonitor repository
   - Parse parameter types and generate better stub signatures
   - Validate argument bytes against XML definitions

2. **Intelligent Stub Generation**
   - Infer parameter types from function names
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

## Dependencies

- **AsmResolver.PE** (6.0.0-beta.4) - PE file parsing
- **System.CommandLine** (2.0.0-beta4) - CLI framework
- **.NET 9.0** - Target framework

## License

Part of the Win32Emu project. See LICENSE in the repository root.
