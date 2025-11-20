# Native DLL Analysis Feature

This document describes the native DLL analysis feature added to Win32Emu for discovering missing function implementations.

## Overview

The native DLL analysis feature helps Win32Emu developers:
- **Discover missing functions** by comparing native Windows ME DLL exports with Win32Emu implementations
- **Track progress** of Win32 API coverage over time
- **Prioritize work** by identifying which functions are most important to implement
- **Test-driven development** by using native DLL exports as a specification

## Components

### 1. Win32Emu.Tools.NativeDllAnalyzer

A command-line tool that analyzes native Windows DLLs and generates reports.

**Location:** `Win32Emu.Tools.NativeDllAnalyzer/`

**Usage:**
```bash
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer <dll-directory> <api-status-json> [output-json]
```

**Example:**
```bash
# Generate API status first
dotnet run --project Win32Emu.Tools.ApiStatusGenerator docs/pages/api-status.json

# Analyze native DLLs
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json docs/pages/missing-functions.json
```

**Features:**
- Parses PE DLL exports using PeNet library
- Compares with Win32Emu's API status (from source generators)
- Generates JSON report with missing, stub, and implemented functions
- Console output shows summary statistics

**See:** [Win32Emu.Tools.NativeDllAnalyzer/README.md](../Win32Emu.Tools.NativeDllAnalyzer/README.md)

### 2. Win32Emu.Tools.TestGenerator

A command-line tool that generates A-B test scaffolding from native DLL analysis reports.

**Location:** `Win32Emu.Tools.TestGenerator/`

**Usage:**
```bash
dotnet run --project Win32Emu.Tools.TestGenerator <missing-functions-json> <output-directory> [dll-filter]
```

**Example:**
```bash
# Generate tests for all DLLs
dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated

# Generate tests for specific DLL
dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated KERNEL32.DLL
```

**Features:**
- Generates xUnit test scaffolding for missing and stub functions
- Creates A-B testing infrastructure (base classes, native DLL loader)
- Platform-aware: Works on Windows (with native DLLs) and Linux/macOS (Win32Emu only)
- Generates README and documentation for tests
- Limits generation to first 10 functions per DLL to avoid overwhelming

**See:** [Win32Emu.Tools.TestGenerator/README.md](../Win32Emu.Tools.TestGenerator/README.md)

### 3. Missing Functions Web Page

An interactive HTML page that displays the analysis results.

**Location:** `docs/pages/missing-functions.html`

**Features:**
- Summary statistics (total exports, implemented, stubs, missing)
- Interactive DLL list with expandable details
- Search functionality to find specific DLLs or functions
- Filter by missing/complete implementations
- Sort by name, missing count, or coverage
- Client-side JavaScript (no server required)

**Live Demo:** Once deployed, available at: `https://archanox.github.io/Win32Emu/missing-functions.html`

### 4. GitHub Actions Integration

The GitHub Pages workflow automatically generates the missing functions report.

**Location:** `.github/workflows/cpu-test-results.yml`

**Workflow Steps:**
1. Build Win32Emu (generates API metadata via source generators)
2. Build Win32Emu.Tools.ApiStatusGenerator
3. Build Win32Emu.Tools.NativeDllAnalyzer
4. Generate API status JSON
5. Generate missing functions JSON
6. Copy HTML pages to GitHub Pages
7. Deploy to GitHub Pages

**Triggers:**
- Weekly schedule (every Monday)
- Manual workflow dispatch
- Push to main affecting:
  - Win32 modules (`Win32Emu/Win32/Modules/**`)
  - Native DLLs (`DLLs/WinME/**`)
  - Analysis tools (`Win32Emu.Tools.NativeDllAnalyzer/**`)
  - HTML pages (`docs/pages/missing-functions.html`)

## Data Flow

```
┌─────────────────┐
│ Native DLLs     │
│ (DLLs/WinME/)   │
└────────┬────────┘
         │
         │ PeNet
         ▼
┌─────────────────┐     ┌──────────────────┐
│ DLL Exports     │     │ Win32Emu Source  │
│ (Functions)     │     │ (Modules/*.cs)   │
└────────┬────────┘     └────────┬─────────┘
         │                       │
         │                       │ Source Generators
         │                       ▼
         │              ┌──────────────────┐
         │              │ API Status JSON  │
         │              │ (api-status.json)│
         │              └────────┬─────────┘
         │                       │
         └───────┬───────────────┘
                 │
                 │ NativeDllAnalyzer
                 ▼
         ┌──────────────────┐
         │ Missing Functions│
         │ Report (JSON)    │
         └────────┬─────────┘
                  │
                  │
                  ▼
         ┌──────────────────┐
         │ GitHub Pages     │
         │ (HTML + JS)      │
         └──────────────────┘
```

## Report Format

### JSON Structure

```json
{
  "analyzedAt": "2025-11-20T18:49:49Z",
  "dllDirectory": "DLLs/WinME",
  "summary": {
    "totalDllsAnalyzed": 27,
    "totalNativeExports": 4962,
    "totalImplemented": 764,
    "totalStubs": 129,
    "totalMissing": 4069,
    "implementationPercentage": 15.4
  },
  "dlls": [
    {
      "dllName": "KERNEL32.DLL",
      "nativeExports": [
        { "name": "CreateFileA", "ordinal": 45 }
      ],
      "implementedFunctions": ["CreateFileA"],
      "stubFunctions": [],
      "missingFunctions": ["CreateFileMappingA"],
      "extraImplementations": [],
      "coveragePercentage": 32.6
    }
  ]
}
```

### Console Output

```
Win32Emu Native DLL Analyzer
=============================

Found 27 DLL files to analyze...

Analyzing KERNEL32.DLL...
  Native exports: 760
  Implemented: 248
  Stubs: 2
  Missing: 510
  Coverage: 32.6%

======================================================================
ANALYSIS SUMMARY
======================================================================

Total DLLs analyzed: 27
Total native exports: 4962
Total implemented: 764
Total stubs: 129
Total missing: 4069
Implementation rate: 15.4%

Top DLLs with missing functions:

  MSVCRT.DLL:
    Native exports: 779
    Implemented: 50
    Missing: 729
    Coverage: 6.4%
```

## Use Cases

### 1. Finding Missing Functions

**Problem:** "Which functions do I need to implement for DirectDraw support?"

**Solution:**
1. Run the analyzer
2. Open missing-functions.html
3. Search for "DDRAW.DLL"
4. Expand the card to see missing functions
5. Prioritize based on game requirements

### 2. Tracking Progress

**Problem:** "How much has our API coverage improved?"

**Solution:**
1. Run analyzer monthly
2. Compare `implementationPercentage` over time
3. Track specific DLLs (e.g., KERNEL32, USER32)
4. Create charts showing progress

### 3. Test-Driven Development with A-B Testing

**Problem:** "I want to ensure my implementation matches Windows behavior"

**Solution:**
1. Use native DLL exports as specification
2. **Generate test scaffolding** using Win32Emu.Tools.TestGenerator
3. Implement functions to pass tests
4. **A-B test** against native DLL behavior (on Windows)
5. Re-run analyzer to verify coverage

**Implementation:**

```bash
# Step 1: Generate missing functions report
dotnet run --project Win32Emu.Tools.ApiStatusGenerator docs/pages/api-status.json
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json docs/pages/missing-functions.json

# Step 2: Generate test scaffolding for a specific DLL
dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated KERNEL32.DLL

# Step 3: Implement function in Win32Emu (e.g., Win32Emu.Kernel32)

# Step 4: Update generated test with proper test data and assertions

# Step 5: Run A-B test
dotnet test --filter "Function=YourFunction"

# Step 6: Verify coverage improvement
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json
```

**A-B Testing:**
- **A**: Win32Emu's implementation
- **B**: Native Windows DLL behavior
- Tests run both implementations with identical inputs
- Compare results to ensure Win32Emu matches native behavior
- Works on Windows (with native DLLs); gracefully degrades on Linux/macOS

See: [Win32Emu.Tools.TestGenerator README](../Win32Emu.Tools.TestGenerator/README.md)

### 4. Prioritization

**Problem:** "Which functions are most critical to implement?"

**Solution:**
1. Run PeAnalyzer on target games/apps
2. See which functions they import
3. Cross-reference with missing functions report
4. Implement functions used by target applications

## Statistics (as of November 2025)

Based on Windows ME DLLs:

| Metric | Value |
|--------|-------|
| Total DLLs Analyzed | 27 |
| Total Native Exports | 4,962 |
| Implemented Functions | 764 (15.4%) |
| Stub Functions | 129 (2.6%) |
| Missing Functions | 4,069 (82.0%) |

**Top DLLs by Missing Functions:**

1. **MSVCRT.DLL**: 729 missing (93.6% missing)
2. **KERNEL32.DLL**: 510 missing (67.1% missing)
3. **USER32.DLL**: 419 missing (64.4% missing)
4. **OLEAUT32.DLL**: 358 missing (100% missing)
5. **SHLWAPI.DLL**: 292 missing (98.0% missing)

## Future Enhancements

### Planned Features

- [ ] **Signature validation**: Check parameter types and counts match
- [ ] **Ordinal-only exports**: Handle exports without names
- [ ] **Function usage tracking**: Identify which functions are actually used by games
- [ ] **Auto-stub generation**: Generate skeleton implementations for missing functions
- [ ] **Historical tracking**: Track coverage changes over time with charts
- [ ] **Integration with issues**: Auto-create GitHub issues for missing functions
- [ ] **Priority scoring**: Calculate importance based on usage frequency

### Possible Improvements

- **Multiple DLL versions**: Compare different Windows versions (95, 98, ME, XP)
- **Function categories**: Group functions by type (file I/O, graphics, etc.)
- **Dependency analysis**: Show which missing functions depend on other missing functions
- **Compatibility matrix**: Show which games work with current coverage
- **Test coverage**: Link to tests for implemented functions

## Maintenance

### Updating Native DLLs

If you add or update DLLs in `DLLs/WinME/`:

1. Ensure they are 32-bit PE DLLs
2. Commit to repository
3. GitHub Actions will automatically regenerate the report

### Modifying the Tool

To modify the analyzer:

1. Edit files in `Win32Emu.Tools.NativeDllAnalyzer/`
2. Build and test locally
3. Commit changes
4. GitHub Actions will use the new version

### Updating the Web Page

To modify the HTML interface:

1. Edit `docs/pages/missing-functions.html`
2. Test locally by opening in browser with mock JSON
3. Commit changes
4. GitHub Actions will deploy to Pages

## Technical Details

### Dependencies

- **PeNet 4.0.3**: PE file parsing for reading DLL exports
- **System.Text.Json**: JSON serialization
- **.NET 9.0**: Runtime

### PeNet Usage

```csharp
var peFile = new PeFile(dllPath);
var exports = peFile.ExportedFunctions;
foreach (var export in exports)
{
    Console.WriteLine($"{export.Name} (ordinal: {export.Ordinal})");
}
```

### Performance

- Analyzing 27 DLLs takes ~5 seconds
- JSON report is ~1-2 MB
- HTML page loads instantly (client-side rendering)

## Troubleshooting

### "Module not implemented in Win32Emu"

**Cause:** DLL exists in WinME but not implemented in Win32Emu

**Solution:** This is expected. The report identifies which DLLs need work.

### "No exports found"

**Cause:** DLL has no named exports or is not a valid PE file

**Solution:** Check if DLL is valid with `dumpbin /exports` (Windows) or `objdump -p` (Linux)

### JSON file too large

**Cause:** Report includes all function names for all DLLs

**Solution:** The current size (~1-2 MB) is acceptable for GitHub Pages. If it grows too large, consider:
- Paginating results
- Compressing with gzip
- Storing in a database

## Related Documentation

- [Win32Emu.Tools.NativeDllAnalyzer README](../Win32Emu.Tools.NativeDllAnalyzer/README.md)
- [Win32Emu.Tools.ApiStatusGenerator README](../Win32Emu.Tools.ApiStatusGenerator/README.md)
- [Win32Emu.Tools.PeAnalyzer README](../Win32Emu.Tools.PeAnalyzer/README.md)
- [GitHub Pages Workflow](.github/workflows/cpu-test-results.yml)

## Contributing

To contribute to this feature:

1. **Add missing functions**: Implement functions in Win32 modules
2. **Improve analyzer**: Enhance NativeDllAnalyzer tool
3. **Better UI**: Improve missing-functions.html interface
4. **Documentation**: Update READMEs and guides

See [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines.
