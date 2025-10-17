# DirectDraw 8-bit Palettized Mode Black Screen Fix

## Summary
Fixed critical bugs in DirectDraw implementation that caused black screens when games used 8-bit (256 color) palettized display modes.

## Issues Fixed

### 1. SDL Window Not Displaying Initial Frame
**Problem**: When `SetDisplayMode` was called, the SDL window was created but showed a blank/undefined screen because no initial frame was rendered.

**Solution**: Added initial `RenderClear` and `RenderPresent` calls to the `SilkSdlRenderingBackend.Initialize()` method to ensure the window displays a proper black frame immediately upon creation.

### 2. 8-bit Palettized Pixel Data Misinterpreted as RGBA
**Problem**: When a surface was created in 8-bit palettized mode (BPP=8), the `Surface_Unlock` method had flawed conversion logic:
- It checked for an attached palette first
- If no palette was attached (common during initialization), it would fall through to the else branch
- The else branch treated the 8-bit indexed pixel data as if it were RGBA, causing severe visual corruption or black screens

**Solution**: Rewrote the bit depth conversion logic to check the actual bits-per-pixel value FIRST:
- For 8-bit mode: Use attached palette if available, otherwise use a grayscale fallback palette
- For 16-bit mode: Convert RGB565 to RGBA
- For 24/32-bit mode: Use data as-is (assuming RGBA format)

## Code Changes

### SilkSdlRenderingBackend.cs
```csharp
// Initialize method - Added after texture creation:
_initialized = true;

// Clear the window with black to show it's properly initialized
_sdl.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
_sdl.RenderClear(_renderer);
_sdl.RenderPresent(_renderer);
```

### DDrawModule.cs - Surface_Unlock
Changed from palette-first checking to bit-depth-first checking:

**Before (Broken)**:
```csharp
if (surface.PaletteHandle != 0 && palette exists)
    ConvertPalettizedToRGBA(...)
else if (ddrawObj.BitsPerPixel == 16)
    Convert16BitToRGBA(...)
else
    displayData = surface.Bits; // BUG: Treats 8-bit indexed as RGBA!
```

**After (Fixed)**:
```csharp
if (ddrawObj.BitsPerPixel == 8)
    if (surface.PaletteHandle != 0 && palette exists)
        ConvertPalettizedToRGBA(surface, palette, ...)
    else
        // Fallback: Use grayscale palette
        ConvertPalettizedToRGBA(surface, grayscalePalette, ...)
else if (ddrawObj.BitsPerPixel == 16)
    Convert16BitToRGBA(...)
else if (ddrawObj.BitsPerPixel == 24 || ddrawObj.BitsPerPixel == 32)
    displayData = surface.Bits; // Correct for RGBA
```

## Technical Details

### Palettized Mode (8-bit)
In 8-bit palettized mode:
- Each pixel is a single byte containing an index (0-255)
- The index refers to an entry in a 256-color palette (PALETTEENTRY array)
- Each palette entry contains RGB values (3 bytes + 1 byte flags)
- To display on modern hardware, we must convert: `indexed_pixels[palette]` → RGBA

### Why the Bug Caused Black Screens
When 8-bit indexed data was treated as RGBA:
- Bytes 0-3: Treated as R, G, B, A (but they're actually palette indices)
- Example: Index `0x42` would become color `(0x42, 0x??, 0x??, 0x??)`
- Most palette indices are low values (0-255), resulting in dark/black colors
- Without proper palette lookup, images appear completely wrong or black

### Grayscale Fallback
When no palette is attached to an 8-bit surface, we use a grayscale palette:
```csharp
var grayscalePalette = new uint[256];
for (int i = 0; i < 256; i++)
{
    grayscalePalette[i] = (uint)((i << 16) | (i << 8) | i); // RGB all same value
}
```
This ensures that:
- Index 0 = black (0,0,0)
- Index 255 = white (255,255,255)
- Intermediate values = shades of gray
- Images are at least visible (though not colored correctly)

## Verification

### Expected Behavior After Fix
1. Game calls `DirectDrawCreate` → Success
2. Game calls `SetCooperativeLevel` → Success
3. Game calls `SetDisplayMode(640, 480, 8)` → SDL window opens showing black screen
4. Game calls `CreateSurface` → Primary surface created in 8-bit mode
5. Game calls `CreatePalette` and `SetPalette` → Palette attached to surface (or uses grayscale)
6. Game calls `Lock` → Returns pointer to 8-bit indexed pixel buffer
7. Game draws to buffer (writing palette indices)
8. Game calls `Unlock` → Converts 8-bit indexed to RGBA using palette, presents to screen
9. Game calls `Flip` → Processes SDL events, keeps window responsive

### Log Messages to Look For
```
[SilkSDL] Initialized 640x480 display
[DDraw COM] IDirectDraw::SetDisplayMode(this=..., width=640, height=480, bpp=8)
[DDraw COM] IDirectDraw::CreateSurface(...)
[DDraw] Created IDirectDrawSurface COM object at ...
[DDraw COM] IDirectDrawSurface::Lock(...)
[DDraw] Locked surface ..., memory at ...
[DDraw COM] IDirectDrawSurface::Unlock(...)
[DDraw] Converting 8-bit palettized surface to RGBA (or using grayscale)
[DDraw] Unlocked surface ...
[DDraw COM] IDirectDrawSurface::Flip(...)
[DDraw] Flipped primary surface
```

## Games Using 8-bit Palettized Mode
Many older games (mid-1990s to early 2000s) used 8-bit palettized mode:
- Reduces memory usage (1 byte per pixel vs 2-4 bytes)
- Common in games like:
  - Ignition (IGN_TEAS.EXE) - the test case for this fix
  - Command & Conquer
  - Warcraft II
  - StarCraft (original)
  - Diablo
  - Age of Empires

## References
- [DirectDraw Surface Formats](https://learn.microsoft.com/en-us/windows/win32/directx9/d3dformat)
- [PALETTEENTRY Structure](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-paletteentry)
- [RGB565 Format](https://en.wikipedia.org/wiki/List_of_monochrome_and_RGB_color_formats#16-bit_RGB_(also_known_as_RGB565))
