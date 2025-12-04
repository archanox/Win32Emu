# Win16 API Status Source Generator

## Overview
Win16 API compatibility data is automatically generated at compile time using a Roslyn source generator (`Win16ApiStatusGenerator`), similar to how Win32 API status is generated.

## Implementation

### Source Generator
**File**: `Win32Emu.Generators/Win16ApiStatusGenerator.cs`

The generator:
1. Scans for classes that inherit from `Win16ThunkingLayer`
2. Extracts the module name from the `Name` property
3. Parses `TryInvokeWin16` methods to find switch statements
4. Extracts all case labels (function names) from the switch statements
5. Generates `Win16ApiStatusMetadata.g.cs` with embedded JSON at compile time

### Generated Metadata
**File**: `Win32Emu/Win32/Win16ApiStatusMetadata.g.cs` (auto-generated)

Contains:
```csharp
public static class Win16ApiStatusMetadata
{
    public const string Json = @"{...}";  // Embedded JSON
    public static int TotalModules => 6;
    public static int TotalFunctions => 325;
}
```

### Export Tool
**File**: `Win32Emu.Tools.ApiStatusGenerator/Program.cs`

The tool exports both Win32 and Win16 API status:
- Reads `ApiStatusMetadata.Json` (Win32 modules with `[DllModuleExport]` attributes)
- Reads `Win16ApiStatusMetadata.Json` (Win16 modules from switch statements)
- Writes `api-status.json` and `win16-api-status.json` to specified directory

## Usage

### Regenerating API Status Files

When Win16 functions are added or modified in `Win32Emu/Win32/Win16/` modules:

1. **Build the project** (triggers source generator):
   ```bash
   dotnet build Win32Emu/Win32Emu.csproj --configuration Release
   ```

2. **Export to JSON files**:
   ```bash
   dotnet run --project Win32Emu.Tools.ApiStatusGenerator --configuration Release -- Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot
   ```

This generates:
- `Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/api-status.json` (Win32)
- `Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/win16-api-status.json` (Win16)

### How It Works

#### 1. Compile-Time Generation
When you build the `Win32Emu` project:
```
Win32Emu.csproj
  ↓ (references)
Win32Emu.Generators
  ↓ (contains)
Win16ApiStatusGenerator
  ↓ (scans at compile time)
Win16 Module Classes
  ↓ (generates)
Win16ApiStatusMetadata.g.cs (in obj/ directory)
```

#### 2. Export to JSON
When you run the export tool:
```
Win32Emu.Tools.ApiStatusGenerator
  ↓ (references)
Win32Emu.dll (contains generated metadata)
  ↓ (reads)
Win16ApiStatusMetadata.Json constant
  ↓ (writes)
win16-api-status.json file
```

## How the Generator Finds Functions

The generator looks for this pattern in Win16 module source files:

```csharp
internal class Win16KernelModule : Win16ThunkingLayer
{
    public string Name => "KERNEL";  // ← Module name extracted

    public override bool TryInvokeWin16(...)
    {
        switch (exportUpper)  // ← Switch statement found
        {
            case "GLOBALALLOC":  // ← Function name extracted
            case "GLOBALFREE":   // ← Function name extracted
            // ...
        }
    }
}
```

Result:
```json
{
  "name": "KERNEL.DLL",
  "functions": ["GLOBALALLOC", "GLOBALFREE", ...]
}
```

## Benefits Over Python Script

### Previous Approach (Python Script)
- ❌ Separate Python script to run manually
- ❌ Regex parsing of C# source code
- ❌ Could get out of sync if not run
- ❌ Extra dependency (Python)
- ❌ Separate build step

### Current Approach (Source Generator)
- ✅ Integrated into build process
- ✅ Roslyn-based parsing (proper C# understanding)
- ✅ Always in sync with source code
- ✅ No external dependencies
- ✅ Same approach as Win32 modules
- ✅ Automatic at compile time

## Technical Details

### Generator Pipeline
1. **Syntax Provider**: Filters for class declarations with `Name` property
2. **Semantic Analysis**: Checks if class inherits from `Win16ThunkingLayer`
3. **Extraction**: Parses `TryInvokeWin16` method's switch statements
4. **Collection**: Groups functions by module
5. **Generation**: Creates C# class with embedded JSON

### Output Format
```json
{
  "modules": [
    {
      "name": "KERNEL.DLL",
      "functions": ["FUNCTION1", "FUNCTION2", ...]
    },
    ...
  ]
}
```

### Handled Cases
- ✅ Multiple classes per file (e.g., `Win16AuxiliaryModules.cs`)
- ✅ Multiple switch statements per method
- ✅ Case labels with string literals
- ✅ Case-insensitive function names

## Maintenance

When adding a new Win16 module:
1. Create class inheriting from `Win16ThunkingLayer`
2. Add `public string Name => "MODULENAME"` property
3. Implement `TryInvokeWin16` with switch statement
4. Build project (generator runs automatically)
5. Run export tool to update JSON files

When adding Win16 functions:
1. Add case labels to existing switch statement
2. Build project (generator runs automatically)
3. Run export tool to update JSON files

## Integration with PE Analyzer

The PE Analyzer Blazor WASM app:
1. Loads `win16-api-status.json` at startup
2. Uses it to check Win16 executable compatibility
3. Shows which imported functions are implemented
4. Calculates compatibility percentages

## Comparison with Win32 Approach

| Aspect | Win32 Modules | Win16 Modules |
|--------|---------------|---------------|
| Marking | `[DllModuleExport]` attribute | Switch case labels |
| Generator | `ApiStatusGenerator` | `Win16ApiStatusGenerator` |
| Detection | Method attributes | Switch statements |
| Module name | From class `Name` property | From class `Name` property |
| Functions | Attributed methods | Case labels |
| Export tool | `ApiStatusGenerator` | `ApiStatusGenerator` |
| Output | `api-status.json` | `win16-api-status.json` |

Both approaches:
- Use Roslyn source generators
- Generate metadata at compile time
- Export via same tool
- Integrate with PE Analyzer

## Files

- `Win32Emu.Generators/Win16ApiStatusGenerator.cs` - Source generator
- `Win32Emu.Tools.ApiStatusGenerator/Program.cs` - Export tool (handles both Win32 and Win16)
- `Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/win16-api-status.json` - Generated JSON file
- `Win32Emu/Win32/Win16ApiStatusMetadata.g.cs` - Generated C# class (in obj/ directory)
