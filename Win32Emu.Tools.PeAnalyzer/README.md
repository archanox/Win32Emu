# Win32Emu.Tools.PeAnalyzer

A tool that analyzes PE (Portable Executable) files and checks their compatibility with Win32Emu's current API implementation status.

## Purpose

This tool uses [PeNet](https://github.com/secana/PeNet) library to:
- Parse PE executable import tables
- Extract all imported DLLs and functions
- Cross-reference with Win32Emu's API status data
- Generate detailed compatibility reports

## Features

- ✅ Analyzes 32-bit PE executables (.exe and .dll)
- ✅ Identifies all imported Win32 API functions
- ✅ Checks implementation status (implemented, stub, or missing)
- ✅ Calculates compatibility percentages
- ✅ Provides actionable verdict
- ✅ Outputs structured JSON reports

## Usage

```bash
dotnet run --project Win32Emu.Tools.PeAnalyzer <pe-file> <api-status-json>
```

### Example

```bash
# Analyze a game executable
dotnet run --project Win32Emu.Tools.PeAnalyzer game.exe docs/pages/api-status.json

# Analyze multiple files and save reports
for file in *.exe; do
    dotnet run --project Win32Emu.Tools.PeAnalyzer "$file" docs/pages/api-status.json > "${file}.report.json"
done
```

## Sample Output

```json
{
  "fileName": "gdi.exe",
  "fileSize": 3584,
  "is32Bit": true,
  "is64Bit": false,
  "analyzedAt": "2025-11-13T23:44:13Z",
  "status": {
    "totalDlls": 3,
    "totalFunctions": 15,
    "implementedFunctions": 13,
    "stubFunctions": 0,
    "missingFunctions": 2,
    "implementationPercentage": 86.67,
    "verdict": "PARTIALLY COMPATIBLE - 2 missing function(s)"
  },
  "dependencies": [
    {
      "dllName": "USER32.DLL",
      "isSupported": true,
      "functions": [
        {
          "name": "CreateWindowExA",
          "status": "implemented",
          "ordinal": 120
        }
      ],
      "implementedCount": 12,
      "stubCount": 0,
      "missingCount": 0,
      "implementationPercentage": 100
    }
  ]
}
```

## Verdict Categories

The tool provides one of these verdicts based on analysis:

- **FULLY COMPATIBLE** - All required APIs are implemented (no stubs, no missing)
- **MOSTLY COMPATIBLE** - All APIs present but some are stubs
- **PARTIALLY COMPATIBLE** - 80%+ implemented, some functions missing
- **LIMITED COMPATIBILITY** - <80% implemented, many functions missing
- **INCOMPATIBLE** - 64-bit PE file (Win32Emu only supports 32-bit)

## Integration with GitHub Pages

This tool can be integrated with the GitHub Pages site to provide real-time compatibility analysis:

### Option 1: Server-side API (Recommended)

Deploy as an Azure Function or AWS Lambda:

```csharp
[Function("AnalyzePe")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
{
    // Read uploaded PE file
    var file = await req.ReadFromMultipartAsync();
    
    // Analyze with PeNet
    var analyzer = new PeCompatibilityAnalyzer(file, apiStatusData);
    var report = await analyzer.AnalyzeAsync();
    
    // Return JSON response
    return await req.WriteJsonAsync(report);
}
```

### Option 2: Command-line Tool

Users can download and run locally:

```bash
# Install as global tool
dotnet tool install -g Win32Emu.Tools.PeAnalyzer

# Analyze any PE file
pe-analyzer game.exe
```

## Dependencies

- **.NET 9.0** - Runtime
- **PeNet 4.0.3** - PE file parsing
- **System.Text.Json 9.0.0** - JSON serialization

## Technical Details

### PE Parsing

The tool uses PeNet to:
1. Read PE headers and validate 32-bit architecture
2. Parse the import address table (IAT)
3. Extract DLL names and function names/ordinals
4. Handle both name-based and ordinal-based imports

### Status Determination

For each imported function:
1. Find the DLL module in API status data
2. Locate the function by name (case-insensitive)
3. Check the `IsStub` flag
4. Classify as: `implemented`, `stub`, or `missing`

### Compatibility Scoring

```
implementationPercentage = (implementedFunctions / totalFunctions) * 100
```

Where:
- `implementedFunctions` = functions with `status == "implemented"`
- `totalFunctions` = all imported functions from all DLLs

## Limitations

- Only supports 32-bit PE files (64-bit shows as INCOMPATIBLE)
- Does not analyze delay-loaded imports
- Does not check API signature compatibility
- Does not detect runtime DLL loading (LoadLibrary)
- Cannot analyze packed/obfuscated executables

## Future Enhancements

- [ ] Support for delay-loaded DLLs
- [ ] Analyze exported functions (for DLL files)
- [ ] Detect common packers (UPX, ASPack, etc.)
- [ ] Check API signature compatibility
- [ ] Estimate runtime compatibility score
- [ ] Suggest missing API implementations to prioritize
- [ ] Integration with GitHub Issues (auto-create for missing APIs)

## See Also

- [PeNet Library](https://github.com/secana/PeNet) - PE parser used by this tool
- [API Status Generator](../Win32Emu.Tools.ApiStatusGenerator/) - Generates API status data
- [GitHub Pages Site](../../docs/pages/) - Web interface for API status
- [Win32Emu Documentation](../../docs/) - Main project documentation
