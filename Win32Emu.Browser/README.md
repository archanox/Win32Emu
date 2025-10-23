# Win32Emu.Browser

Web deployment of Win32Emu for testing on mobile devices and browsers.

## Features

- **WebGPU Rendering Backend**: Hardware-accelerated graphics using WebGPU API
- **WebAssembly Execution**: Blazor WebAssembly for high-performance web deployment
- **Mobile Optimized**: Responsive design optimized for phone and tablet testing
- **Playwright MCP Compatible**: Designed for automated testing with Playwright MCP
- **Cross-Browser Support**: Works on modern browsers with WebGPU support

## Live Demo

Visit the live demo at: [https://archanox.github.io/Win32Emu/](https://archanox.github.io/Win32Emu/)

## Browser Requirements

- **WebGPU Support**: Required for rendering
  - Chrome/Edge 113+ (WebGPU enabled by default)
  - Firefox 119+ (WebGPU can be enabled in about:config)
  - Safari 17.4+ (WebGPU experimental feature)

- **WebAssembly**: All modern browsers

## Testing on Mobile

The web interface is optimized for mobile device testing:

1. Open the deployed URL on your phone/tablet
2. The interface automatically detects mobile platform
3. Touch-friendly controls and responsive layout
4. WebGPU rendering adapts to device capabilities

## Development

### Building Locally

```bash
dotnet build Win32Emu.Browser/Win32Emu.Browser.csproj
```

### Running Locally

```bash
dotnet run --project Win32Emu.Browser/Win32Emu.Browser.csproj
```

Then navigate to `https://localhost:5001` (or the URL shown in the console).

### Publishing

```bash
dotnet publish Win32Emu.Browser/Win32Emu.Browser.csproj -c Release -o publish/web
```

The published files will be in `publish/web/wwwroot/`.

## Playwright MCP Integration

This web interface is designed to work seamlessly with Playwright MCP for automated testing:

### Features for Playwright

- **Accessible UI Elements**: All controls have proper ARIA labels
- **Console Output**: Real-time console for test validation
- **Status Indicators**: Clear success/error states
- **Test Buttons**: Dedicated test functions for automated scenarios

### Example Playwright Test

```javascript
// Check WebGPU support
await page.goto('https://archanox.github.io/Win32Emu/');
const webgpuBadge = await page.locator('text=WebGPU').locator('..').locator('.badge');
const isSupported = await webgpuBadge.textContent();
expect(isSupported).toContain('Supported');

// Initialize emulator
await page.click('button:has-text("Initialize")');
await page.waitForSelector('text=Initialized');

// Test rendering
await page.click('button:has-text("Test Rendering")');
await page.waitForSelector('text=Rendering Test Complete');
```

## Architecture

- **Framework**: Blazor WebAssembly (.NET 9)
- **Rendering**: WebGPU API via JavaScript interop
- **UI**: Bootstrap 5 with responsive design
- **Build**: GitHub Actions automatic deployment

## Deployment

Automatic deployment to GitHub Pages occurs on every push to the `main` branch via GitHub Actions.

The workflow:
1. Builds the Blazor WebAssembly project
2. Publishes to `wwwroot`
3. Deploys to GitHub Pages
4. Available at the repository's GitHub Pages URL

## Related

- [Win32Emu](../Win32Emu/README.md) - Desktop CLI emulator
- [Win32Emu.Gui](../Win32Emu.Gui/README.md) - Desktop GUI application
- [Main README](../README.md) - Project overview

## License

See [LICENSE](../LICENSE) in the repository root.
