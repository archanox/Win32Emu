# Win32Emu.Tools.ApiStatusGenerator

A tool that generates JSON data about Win32 module API implementation status for use in GitHub Pages documentation.

## Purpose

This tool scans the Win32Emu source code and extracts:
- List of all Win32 DLL modules
- All exported functions in each module
- Implementation status (implemented vs stub)
- Metadata (ordinal, version, export name, etc.)

The generated JSON is consumed by the GitHub Pages site to display API coverage status.

## Usage

```bash
dotnet run --project Win32Emu.Tools.ApiStatusGenerator <modules-dir> <output-json>
```

Example:
```bash
dotnet run --project Win32Emu.Tools.ApiStatusGenerator Win32Emu/Win32/Modules docs/pages/api-status.json
```

## Output Format

The tool generates a JSON file with this structure:

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
          "version": null,
          "exportName": null,
          "forwardedTo": null
        }
      ]
    }
  ]
}
```

## How It Works

1. **Scans module files**: Looks for `*Module.cs` files in the modules directory
2. **Extracts DLL names**: From `public string Name =>` properties
3. **Parses functions**: Using two methods:
   - For modules with `[DllModuleExport]` attributes: Regex parsing of attributes
   - For legacy modules with switch statements: Regex parsing of case labels
4. **Determines stub status**: From `IsStub = true` in attribute
5. **Generates JSON**: Structured data for consumption by GitHub Pages

## Integration

This tool is designed to be run:
- Manually by developers when updating documentation
- Automatically via GitHub Actions on push to main branch
- Before deploying GitHub Pages site

## See Also

- [GitHub Pages Site](../../docs/pages/index.html) - The web interface that consumes this data
- [Win32Emu Modules](../../Win32Emu/Win32/Modules/) - The source modules being analyzed
