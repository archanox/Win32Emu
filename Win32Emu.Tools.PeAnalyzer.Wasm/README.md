# Win32Emu.Tools.PeAnalyzer.Wasm

A Blazor WebAssembly application for analyzing Windows PE files (`.exe` and `.dll`) directly in the browser to check compatibility with Win32Emu.

## Features

- **Client-Side PE Parsing**: Uses PeNet library compiled to WebAssembly for in-browser PE file analysis
- **Compatibility Analysis**: Cross-references imported functions against Win32Emu's API implementation status
- **Visual Results**: Color-coded compatibility indicators showing implemented, stub, and missing functions
- **No Server Required**: Runs entirely in the browser - all analysis happens client-side
- **Detailed Reports**: Per-DLL and per-function compatibility breakdown

## Building

```bash
dotnet build
```

## Publishing for GitHub Pages

**Important**: The `wwwroot/index.html` file must have `<base href="/Win32Emu/pe-analyzer/" />` for GitHub Pages deployment. This ensures all resources load from the correct subpath.

```bash
# Build and publish
dotnet publish -c Release

# Copy to docs/pages/pe-analyzer/
rm -rf docs/pages/pe-analyzer/*
cp -r bin/Release/net10.0/publish/wwwroot/* docs/pages/pe-analyzer/
```

The output will be in `bin/Release/net10.0/publish/wwwroot/`.

## Integration with GitHub Pages

This Blazor WASM app is integrated into the Win32Emu GitHub Pages site at `https://archanox.github.io/Win32Emu/pe-analyzer/`.

To deploy updates:

1. Ensure `wwwroot/index.html` has the correct base href: `<base href="/Win32Emu/pe-analyzer/" />`
2. Build the project in Release mode: `dotnet publish -c Release`
3. Copy the `wwwroot` contents to `docs/pages/pe-analyzer/`: `cp -r bin/Release/net10.0/publish/wwwroot/* docs/pages/pe-analyzer/`
4. Commit and push to trigger GitHub Pages deployment

**Note**: The base href is critical for GitHub Pages deployment. Without the correct path, all resources will try to load from the root domain and fail.

## Technology

- **Blazor WebAssembly**: .NET 10 running in the browser via WebAssembly
- **PeNet 5.1.0**: PE file parsing library
- **Client-Side Only**: No backend server required

## Usage

1. Visit the page in a modern browser (Chrome, Edge, Firefox, Safari)
2. Click "Choose File" and select a Windows PE file (.exe or .dll)
3. The analysis runs entirely in your browser
4. Results show which Win32 APIs are implemented, stubbed, or missing

## Limitations

- **32-bit only**: Win32Emu only supports 32-bit PE files
- **File size**: Limited to 100MB due to browser memory constraints
- **Packed executables**: May not work with heavily packed or obfuscated executables
- **Browser compatibility**: Requires modern browser with WebAssembly support

## Security

- All processing happens in your browser
- Files are not uploaded to any server
- No data is stored or transmitted

## License

Same as the main Win32Emu project.
