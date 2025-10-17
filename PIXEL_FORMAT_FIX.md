# Pixel Format Conversion Fix - SDL ABGR8888 Format

## Summary
Fixed critical bug in DirectDraw rendering where color conversion functions were writing pixel data in RGBA byte order, but SDL textures expect ABGR8888 format. This caused red and blue channels to be swapped, resulting in incorrect colors or black screens in games using 8-bit, 16-bit, and 24-bit color modes.

## Issue
After implementing the DDRAW_8BIT_FIX.md, Ignition game was still showing a black screen. The previous fix correctly handled the palette lookup and conversion logic, but the converted pixels were being written to the SDL texture in the wrong byte order.

## Root Cause
The SDL texture is created with `Sdl.PixelformatAbgr8888`:

```csharp
_texture = _sdl.CreateTexture(_renderer, Sdl.PixelformatAbgr8888, (int)TextureAccess.Streaming, width, height);
```

`ABGR8888` format expects bytes in this order:
- Byte 0: **A**lpha
- Byte 1: **B**lue
- Byte 2: **G**reen
- Byte 3: **R**ed

However, all three color conversion functions were writing in RGBA order:
- Byte 0: Red
- Byte 1: Green
- Byte 2: Blue
- Byte 3: Alpha

This mismatch caused:
- Red channel data written to alpha position (mostly transparent/black)
- Green channel data written to blue position
- Blue channel data written to green position
- Alpha channel data (0xFF) written to red position (white/red tint)

## Solution

### ConvertPalettizedToRGBA (8-bit indexed color)
**Before:**
```csharp
rgbaData[dstOffset + 0] = (byte)(color & 0xFF);         // R
rgbaData[dstOffset + 1] = (byte)((color >> 8) & 0xFF);  // G
rgbaData[dstOffset + 2] = (byte)((color >> 16) & 0xFF); // B
rgbaData[dstOffset + 3] = 0xFF;                          // A
```

**After:**
```csharp
// SDL texture format is ABGR8888, so we need to write in ABGR byte order
rgbaData[dstOffset + 0] = 0xFF;                          // A
rgbaData[dstOffset + 1] = (byte)((color >> 16) & 0xFF); // B
rgbaData[dstOffset + 2] = (byte)((color >> 8) & 0xFF);  // G
rgbaData[dstOffset + 3] = (byte)(color & 0xFF);         // R
```

### Convert16BitToRGBA (RGB565 format)
**Before:**
```csharp
rgbaData[dstOffset + 0] = r;
rgbaData[dstOffset + 1] = g;
rgbaData[dstOffset + 2] = b;
rgbaData[dstOffset + 3] = 0xFF;
```

**After:**
```csharp
// SDL texture format is ABGR8888, so we need to write in ABGR byte order
rgbaData[dstOffset + 0] = 0xFF;  // A
rgbaData[dstOffset + 1] = b;     // B
rgbaData[dstOffset + 2] = g;     // G
rgbaData[dstOffset + 3] = r;     // R
```

### Convert24BitToRGBA (BGR24 format)
**Before:**
```csharp
// 24-bit is typically BGR format in Windows
rgbaData[dstOffset + 0] = rgb24Data[srcOffset + 2]; // R
rgbaData[dstOffset + 1] = rgb24Data[srcOffset + 1]; // G
rgbaData[dstOffset + 2] = rgb24Data[srcOffset + 0]; // B
rgbaData[dstOffset + 3] = 0xFF;                      // A
```

**After:**
```csharp
// 24-bit is typically BGR format in Windows
// SDL texture format is ABGR8888, so we need to write in ABGR byte order
rgbaData[dstOffset + 0] = 0xFF;                      // A
rgbaData[dstOffset + 1] = rgb24Data[srcOffset + 0]; // B (already in correct position)
rgbaData[dstOffset + 2] = rgb24Data[srcOffset + 1]; // G (already in correct position)
rgbaData[dstOffset + 3] = rgb24Data[srcOffset + 2]; // R (already in correct position)
```

Note: The 24-bit case is simpler because Windows BGR format happens to match SDL's GBR order (when alpha is excluded).

## Technical Details

### PALETTEENTRY Structure
The Windows PALETTEENTRY structure is defined as:
```c
typedef struct tagPALETTEENTRY {
  BYTE peRed;
  BYTE peGreen;
  BYTE peBlue;
  BYTE peFlags;
} PALETTEENTRY;
```

When read as a uint32 with little-endian byte order:
```
Memory:  [R] [G] [B] [F]
uint32:  F << 24 | B << 16 | G << 8 | R
```

So:
- `color & 0xFF` = R
- `(color >> 8) & 0xFF` = G
- `(color >> 16) & 0xFF` = B
- `(color >> 24) & 0xFF` = F

### SDL Pixel Formats
SDL supports various pixel formats. The format used in Win32Emu is:
- `SDL_PIXELFORMAT_ABGR8888` - 32-bit ABGR format (8 bits per channel)

This is a packed format where a 32-bit value contains all four channels:
```
Memory:  [A] [B] [G] [R]
uint32:  R << 24 | G << 16 | B << 8 | A
```

When writing individual bytes to memory:
```csharp
data[offset + 0] = A;  // Alpha at lowest address
data[offset + 1] = B;  // Blue
data[offset + 2] = G;  // Green
data[offset + 3] = R;  // Red at highest address
```

## Impact
This fix affects all games using DirectDraw with any of these color depths:
- 8-bit palettized mode (most common in mid-1990s games)
- 16-bit RGB565 mode
- 24-bit BGR mode

### Games Known to Be Affected
- **Ignition (1997)** - Uses 8-bit palettized mode
- Any game using DirectDraw with non-32-bit color modes

### Verification
To verify the fix:
1. Run a game that uses 8-bit palettized DirectDraw mode (like Ignition)
2. The colors should now display correctly
3. Red objects should appear red, blue objects should appear blue
4. No black screen or color swap artifacts

## Files Modified
- `Win32Emu/Rendering/SilkSdlRenderingBackend.cs`
  - `ConvertPalettizedToRGBA()` - Fixed ABGR byte order
  - `Convert16BitToRGBA()` - Fixed ABGR byte order
  - `Convert24BitToRGBA()` - Fixed ABGR byte order (simplified)

**Note:** Other rendering backends (Vulkan and GLFW) were not modified as they use RGBA pixel format and already have the correct byte order.

## Related Fixes
- `DDRAW_8BIT_FIX.md` - Fixed palette lookup logic and bit depth handling
- This fix complements the 8-bit fix by ensuring converted pixels are in the correct format

## Future Considerations
The 32-bit case (in `DDrawModule.cs`) currently passes data through without conversion:
```csharp
else if (ddrawObj.BitsPerPixel == 32)
{
    // 32-bit RGBA - pass through
    displayData = surface.Bits;
}
```

This assumes DirectDraw 32-bit surfaces are already in ABGR format, which may not be true. DirectDraw 32-bit surfaces are typically ARGB or XRGB. This may need to be addressed in a future fix if games using 32-bit mode show color issues.

## References
- [SDL_PixelFormatEnum Documentation](https://wiki.libsdl.org/SDL2/SDL_PixelFormatEnum)
- [PALETTEENTRY Structure](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-paletteentry)
- [DirectDraw Pixel Formats](https://learn.microsoft.com/en-us/windows/win32/directx9/d3dformat)
