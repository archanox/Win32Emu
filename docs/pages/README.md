# Win32Emu GitHub Pages

This directory contains source files for the GitHub Pages site that displays Win32 API implementation status and CPU test results for Win32Emu.

## Live Site

Once deployed, the site will be available at: `https://archanox.github.io/Win32Emu/`

The site includes:
- **Main landing page** - Navigation to CPU tests and API status
- **CPU Test Results** - SingleStep x86 conformance test results (at `/cpu-tests/`)
- **API Status** - Win32 API implementation dashboard (at `/api-status.html`)

## Files

- **index.html** - API status web interface with interactive features (source for api-status.html)
- **api-status.json** - Generated data about all Win32 modules and functions (auto-updated)

## Features

### 📚 Browse Modules
- View all 31 Win32 DLL modules
- See implementation statistics (749 total functions, 88.5% implemented)
- Expand modules to see all exported functions
- Search/filter modules and functions
- Color-coded status badges (implemented vs stub)

### 🔍 Analyze PE File
- Upload Windows PE executables (.exe or .dll)
- Check compatibility with Win32Emu
- See which imported functions are implemented, stubbed, or missing
- Copy unimplemented functions to clipboard
- **Note**: The browser upload shows a mock demo for illustration. For actual PE analysis, use the command-line Win32Emu.Tools.PeAnalyzer tool which has full PeNet integration.

## Setup GitHub Pages

1. Go to repository **Settings** → **Pages**
2. Set **Source** to "GitHub Actions"

The consolidated GitHub Actions workflow (`.github/workflows/cpu-test-results.yml`) will automatically:
- Generate CPU test results from SingleStep tests
- Generate `api-status.json` from source code
- Create a unified landing page
- Deploy everything to GitHub Pages
- Update the live site

## Local Development

To test locally:

```bash
# First, build Win32Emu to generate the metadata via source generator
dotnet build Win32Emu/Win32Emu.csproj --configuration Release

# Generate the API status JSON
dotnet run --project Win32Emu.Tools.ApiStatusGenerator docs/pages/api-status.json

# Serve the page
cd docs/pages
python3 -m http.server 8080

# Open http://localhost:8080 in your browser
```

## Updating the Data

The GitHub Pages content is automatically regenerated when:
- Changes are pushed to `Win32Emu/Win32/Modules/**` (API status)
- Changes are pushed to `Win32Emu/Cpu/**` or `Win32Emu.Tests.Emulator/**` (CPU tests)
- The generator tools are updated
- Weekly on Mondays at 00:00 UTC (scheduled)
- Manually triggered via GitHub Actions workflow_dispatch

Or manually run:
```bash
# Build Win32Emu first to generate metadata
dotnet build Win32Emu/Win32Emu.csproj --configuration Release

# Export the metadata to JSON
dotnet run --project Win32Emu.Tools.ApiStatusGenerator docs/pages/api-status.json
```

## Future Enhancements

### PE File Analysis
Currently shows a mock analysis. To implement fully:

**Option 1: Server-side (Recommended)**
- Create a backend API using ASP.NET Core
- Use PeNet NuGet package to parse PE files
- Accept file uploads and return analysis results
- Host on Azure Functions or similar service

**Option 2: Client-side**
- Port PeNet to JavaScript or use existing PE parser library
- Parse PE import table in browser
- Pros: No server needed
- Cons: Large library, performance considerations

### Additional Features
- Export analysis results as JSON/CSV
- Historical tracking of API coverage over time
- Link to source code for each function
- Show function signatures and documentation
- Compatibility profiles for popular games
- Community ratings/feedback on stub quality

## Data Format

The `api-status.json` file has this structure:

```json
{
  "generatedAt": "2025-11-13T23:37:40.771Z",
  "modules": [
    {
      "name": "KERNEL32.DLL",
      "className": "Kernel32Module",
      "functions": [
        {
          "name": "GetVersion",
          "isStub": false,
          "ordinal": 42,
          "version": "5.1.2600.6532",
          "exportName": null,
          "forwardedTo": "KERNELBASE.GetVersion"
        }
      ]
    }
  ]
}
```

## Technical Details

### Generator Tool
- **Location**: `Win32Emu.Tools.ApiStatusGenerator/`
- **Language**: C# (.NET 9)
- **Method**: Compile-time source generator using Roslyn's semantic model
- **Targets**: Methods with `[DllModuleExport]` attributes analyzed during compilation

### Web Interface
- **Framework**: Vanilla HTML/CSS/JavaScript (no build step)
- **Design**: GitHub-style responsive UI
- **Browser Support**: Modern browsers with ES6+ support

### Automation
- **Workflow**: `.github/workflows/cpu-test-results.yml` (consolidated GitHub Pages workflow)
- **Trigger**: Push to main branch (module/CPU changes), weekly schedule, or manual dispatch
- **Deployment**: GitHub Actions using actions/deploy-pages@v4

## Contributing

When adding new Win32 API functions:

1. Add the function to the appropriate module in `Win32Emu/Win32/Modules/`
2. Use `[DllModuleExport]` attribute with `IsStub = true` for stubs
3. Push to main branch
4. GitHub Actions will automatically update the status page

Example:
```csharp
[DllModuleExport(42, IsStub = true)]
public uint MyNewFunction()
{
    _logger.LogWarning("[kernel32] MyNewFunction called (stub)");
    return 0;
}
```

## License

This documentation and web interface are part of Win32Emu and use the same license as the main project.

## Support

For issues or suggestions about the API status page:
- Open an issue on GitHub
- Tag with `documentation` label
- Describe the problem or enhancement request
