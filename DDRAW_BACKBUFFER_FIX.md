# DirectDraw Backbuffer Creation Fix

## Problem

Applications using DirectDraw were failing with the error message:
```
Backbuffer couldn't be obtained
```

The error occurred when calling `IDirectDrawSurface::GetAttachedSurface` with `DDSCAPS_BACKBUFFER` (0x00000004) capability flag, which returned `DDERR_NOTFOUND` (0x887601C2) instead of the expected backbuffer surface.

## Root Cause

The backbuffer creation logic in `DDraw_CreateSurface` was only creating backbuffers when the `DDSD_BACKBUFFERCOUNT` (0x00000020) flag was explicitly set in the surface description flags. However, many DirectDraw applications follow a common pattern where they:

1. Set `DDSCAPS_PRIMARYSURFACE` (0x00000200) to indicate a primary surface
2. Set `DDSCAPS_FLIP` (0x00000010) to indicate the surface is part of a flipping chain
3. Set `DDSCAPS_COMPLEX` (0x00000008) to indicate the surface has attached surfaces

Without explicitly setting `DDSD_BACKBUFFERCOUNT` in the flags or providing a backbuffer count value, expecting the implementation to default to creating at least 1 backbuffer (which is the minimum needed for a flipping chain).

## Solution

The fix detects when a primary surface has both `DDSCAPS_FLIP` and `DDSCAPS_COMPLEX` flags set without an explicit backbuffer count, and automatically defaults to creating 1 backbuffer. This matches the expected behavior of legacy DirectDraw implementations.

### Code Changes

In `Win32Emu/Win32/Modules/DDrawModule.cs`:

1. **Added logging** to track surface creation parameters:
   ```csharp
   _logger.LogInformation("[DDraw] Surface creation: flags=0x{Flags:X8}, caps=0x{Caps:X8}, width={Width}, height={Height}, backbufferCount={Count}",
       dwFlags, dwSurfaceCaps, dwWidth, dwHeight, dwBackBufferCount);
   ```

2. **Added detection logic** for flipping chains:
   ```csharp
   // Check if this is a flipping complex surface that needs backbuffers
   // DDSCAPS_FLIP = 0x00000010, DDSCAPS_COMPLEX = 0x00000008
   var isFlippingChain = (dwSurfaceCaps & 0x00000010) != 0 && (dwSurfaceCaps & 0x00000008) != 0;
   
   // If this is a primary surface with flipping capabilities but no explicit backbuffer count,
   // default to creating 1 backbuffer (common DirectDraw pattern)
   if (surface.IsPrimary && isFlippingChain && dwBackBufferCount == 0)
   {
       dwBackBufferCount = 1;
       _logger.LogInformation("[DDraw] Primary surface has FLIP+COMPLEX caps but no explicit backbuffer count, defaulting to 1 backbuffer");
   }
   ```

## Impact

- Applications that create flipping chains without explicitly setting the backbuffer count will now work correctly
- The fix is backward compatible - applications that do set the backbuffer count explicitly continue to work as before
- The logging helps diagnose surface creation issues in the future

## Testing

- Existing DirectDraw unit tests continue to pass
- CodeQL security analysis shows no vulnerabilities introduced by the changes
- The fix is minimal and focused, changing only the backbuffer creation logic

## Related Files

- `Win32Emu/Win32/Modules/DDrawModule.cs` - Main implementation file
- Lines changed: ~15 lines added

## DirectDraw Constants Reference

- `DDSCAPS_PRIMARYSURFACE` = 0x00000200 - Surface is the primary surface
- `DDSCAPS_FLIP` = 0x00000010 - Surface is part of a flipping surface chain
- `DDSCAPS_COMPLEX` = 0x00000008 - Surface is a complex surface (has attached surfaces)
- `DDSCAPS_BACKBUFFER` = 0x00000004 - Surface is a backbuffer
- `DDSD_BACKBUFFERCOUNT` = 0x00000020 - dwBackBufferCount field is valid
- `DDERR_NOTFOUND` = 0x887601C2 - The requested item was not found
