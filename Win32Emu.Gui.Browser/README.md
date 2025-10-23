# Win32Emu.Gui.Browser

WebAssembly version of the Win32Emu GUI, built with Avalonia UI and optimized for browser deployment.

## Overview

This project demonstrates the Win32Emu GUI interface running entirely in the browser using WebAssembly. It's designed for:

- **Mobile Testing**: Test the GUI on phones and tablets
- **Automated Testing**: Optimized for Playwright MCP interaction
- **Quick Demos**: Share the GUI interface without requiring desktop installation
- **Development Preview**: Preview GUI changes in a web browser

## Features

- ✅ Runs entirely in the browser (no server required after initial load)
- ✅ WebGPU renderer backend for hardware acceleration
- ✅ Optimized for Playwright automation and testing
- ✅ Mobile-friendly interface
- ✅ Automatic deployment to GitHub Pages

## Limitations

This is a **demonstration version** of the GUI only. The full Win32 emulator functionality requires:
- Native CPU instruction execution
- Direct hardware access
- File system integration
- Native graphics APIs (DirectDraw, DirectX, etc.)

These features are not available in WebAssembly and require the desktop version.

## Live Demo

Visit https://archanox.github.io/Win32Emu/ to try the live demo.

## Building

### Prerequisites

- .NET 9.0 SDK or later
- `wasm-tools` workload installed:
  ```bash
  dotnet workload install wasm-tools
  ```

### Build

```bash
dotnet build --configuration Release
```

### Publish

```bash
dotnet publish --configuration Release --output ../publish-browser
```

The published files will be in `../publish-browser/wwwroot/`.

### Local Testing

You can test the published application locally using a simple HTTP server:

```bash
cd ../publish-browser/wwwroot
python3 -m http.server 8080
```

Then open http://localhost:8080 in your browser.

## Deployment

The project is automatically deployed to GitHub Pages when changes are pushed to the `main` branch. The deployment is handled by the `.github/workflows/deploy-pages.yml` workflow.

## Architecture

### Project Structure

```
Win32Emu.Gui.Browser/
├── Program.cs              # Entry point, configures Avalonia for browser
├── App.axaml               # Application definition
├── App.axaml.cs            # Application initialization
├── MainView.axaml          # Main view UI definition
├── MainView.axaml.cs       # Main view code-behind
├── wwwroot/
│   └── index.html          # HTML shell for the application
└── Win32Emu.Gui.Browser.csproj
```

### Key Technologies

- **Avalonia UI 11.3**: Cross-platform UI framework
- **Avalonia.Browser**: WebAssembly support for Avalonia
- **.NET 9**: Modern .NET with WebAssembly support
- **WebGPU**: Hardware-accelerated rendering in the browser

## Playwright Integration

The browser version is optimized for Playwright MCP (Model Context Protocol) interaction, enabling:

- Automated GUI testing
- Visual regression testing
- User interaction simulation
- Screenshot capture for documentation

Example Playwright test (conceptual):

```typescript
test('Win32Emu GUI loads', async ({ page }) => {
  await page.goto('https://archanox.github.io/Win32Emu/');
  await expect(page.locator('text=Win32Emu GUI')).toBeVisible();
});
```

## Development

To modify the browser version:

1. Edit the XAML files for UI changes
2. Edit the code-behind files for logic changes
3. Test locally using `dotnet run` (will open in browser)
4. Publish and deploy

The browser project uses file-scoped namespaces and modern C# patterns for clean code.

## Troubleshooting

### Build Errors

If you encounter build errors:

1. Ensure `wasm-tools` workload is installed:
   ```bash
   dotnet workload install wasm-tools
   ```

2. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

### Runtime Errors

If the application doesn't load in the browser:

1. Check browser console for errors
2. Ensure you're serving the files over HTTP/HTTPS (not file://)
3. Try a different browser (Chrome, Firefox, Edge all support WebAssembly)

## Contributing

When contributing to the browser version:

- Keep dependencies minimal
- Test on multiple browsers
- Ensure mobile responsiveness
- Update documentation

## License

Same license as the main Win32Emu project.
