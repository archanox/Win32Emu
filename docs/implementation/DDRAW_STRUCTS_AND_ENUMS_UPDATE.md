# DirectDraw Structures and Enums Update

## Summary

Updated DirectDraw enums and added missing structures based on Olde-Skuul DirectDraw repository and Microsoft DirectX SDK documentation.

## Changes Made

### 1. Extended DDSCaps Enum (Complete)

**Before**: 6 flags
**After**: 28 flags

Added missing surface capability flags:
- `DDSCAPS_3DDEVICE` - Surface can be used for 3D rendering
- `DDSCAPS_ALLOCONLOAD` - Memory allocated on texture load
- `DDSCAPS_ALPHA` - Surface contains alpha only
- `DDSCAPS_FRONTBUFFER` - Front buffer of flipping structure
- `DDSCAPS_HWCODEC` - Hardware codec support
- `DDSCAPS_LIVEVIDEO` - Can receive live video
- `DDSCAPS_LOCALVIDMEM` - True local display memory
- `DDSCAPS_MIPMAP` - Mipmap level
- `DDSCAPS_MODEX` - Mode X surface
- `DDSCAPS_NONLOCALVIDMEM` - Non-local display memory
- `DDSCAPS_OVERLAY` - Overlay surface
- `DDSCAPS_OPTIMIZED` - Optimized surface
- `DDSCAPS_OWNDC` - Long-term DC association
- `DDSCAPS_PALETTE` - Supports unique palettes
- `DDSCAPS_PRIMARYSURFACELEFT` - Left eye primary (stereo)
- `DDSCAPS_STANDARDVGAMODE` - Standard VGA mode
- `DDSCAPS_SYSTEMMEMORY` - System memory surface
- `DDSCAPS_TEXTURE` - Can be used as 3D texture
- `DDSCAPS_VIDEOPORT` - Can receive video port data
- `DDSCAPS_VISIBLE` - Changes immediately visible
- `DDSCAPS_WRITEONLY` - Write-only access
- `DDSCAPS_ZBUFFER` - Z-buffer surface

### 2. New DDSCaps2Flags Enum

Added complete enum for extended surface capabilities (dwCaps2):
- Cube mapping flags (6 faces)
- Texture management flags
- Mipmap flags
- Stereo flags
- Volume texture flags
- User lockable flags
- And 28 more flags

### 3. New DDSCaps3Flags Enum

Added enum for additional surface capabilities (dwCaps3):
- Multisample type and quality masks
- Video surface flags
- Mipmap generation flags
- Displacement map flags

### 4. New DDSCAPS Structure (4 bytes)

```csharp
public struct DDSCAPS
{
    public DDSCaps dwCaps;  // Surface capability flags
}
```

### 5. New DDSCAPS2 Structure (16 bytes)

```csharp
public struct DDSCAPS2
{
    public DDSCaps dwCaps;           // Base surface capability flags
    public DDSCaps2Flags dwCaps2;    // Extended surface capability flags
    public DDSCaps3Flags dwCaps3;    // Additional surface capability flags
    public uint dwCaps4;             // Volume depth or additional flags
}
```

## Usage Examples

### Using DDSCAPS Structure

```csharp
var caps = new DDSCAPS
{
    dwCaps = DDSCaps.DDSCAPS_TEXTURE | DDSCaps.DDSCAPS_VIDEOMEMORY
};
```

### Using DDSCAPS2 Structure

```csharp
var caps2 = new DDSCAPS2
{
    dwCaps = DDSCaps.DDSCAPS_TEXTURE | DDSCaps.DDSCAPS_COMPLEX | DDSCaps.DDSCAPS_MIPMAP,
    dwCaps2 = DDSCaps2Flags.DDSCAPS2_CUBEMAP | DDSCaps2Flags.DDSCAPS2_CUBEMAP_ALLFACES,
    dwCaps3 = DDSCaps3Flags.DDSCAPS3_AUTOGENMIPMAP,
    dwCaps4 = 0
};
```

### Creating a Cube Map

```csharp
var cubemapCaps = new DDSCAPS2
{
    dwCaps = DDSCaps.DDSCAPS_TEXTURE | DDSCaps.DDSCAPS_COMPLEX,
    dwCaps2 = DDSCaps2Flags.DDSCAPS2_CUBEMAP | 
              DDSCaps2Flags.DDSCAPS2_CUBEMAP_POSITIVEX |
              DDSCaps2Flags.DDSCAPS2_CUBEMAP_NEGATIVEX |
              DDSCaps2Flags.DDSCAPS2_CUBEMAP_POSITIVEY |
              DDSCaps2Flags.DDSCAPS2_CUBEMAP_NEGATIVEY |
              DDSCaps2Flags.DDSCAPS2_CUBEMAP_POSITIVEZ |
              DDSCaps2Flags.DDSCAPS2_CUBEMAP_NEGATIVEZ
};
```

## Verification

- ✅ All enum values verified against Microsoft DirectX SDK documentation
- ✅ All structure layouts match SDK specifications
- ✅ All structures are C# structs (value types) as requested
- ✅ All enums properly marked with [Flags] attribute
- ✅ Build successful (0 errors)

## References

1. **Microsoft DirectX SDK**:
   - [DDSCAPS Structure](https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddscaps)
   - [DDSCAPS2 Structure](https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddscaps2)

2. **Olde-Skuul DirectDraw Repository**:
   - https://github.com/Olde-Skuul/directdraw
   - MIT License, header-only SDK

3. **ReactOS DDraw Headers**:
   - https://doxygen.reactos.org/d7/de9/sdk_2include_2psdk_2ddraw_8h_source.html

## Impact

### Compatibility Improvements
- ✅ Enables proper surface capability reporting
- ✅ Supports cube mapping for 3D textures
- ✅ Supports mipmap generation
- ✅ Supports advanced DirectX 7+ features
- ✅ Enables stereo rendering support

### Code Quality
- ✅ Type-safe enums replace magic numbers
- ✅ Complete documentation for all flags
- ✅ Consistent with Microsoft SDK naming

### Games That Benefit
- DirectX 6-7 era games (1999-2001)
- Games using cube mapping
- Games using mipmapped textures
- Games with advanced surface features

## Statistics

- **DDSCaps enum**: 6 → 28 flags (+22 flags, +367% increase)
- **New enums**: 2 (DDSCaps2Flags with 29 flags, DDSCaps3Flags with 7 flags)
- **New structs**: 2 (DDSCAPS, DDSCAPS2)
- **Total new constants**: 58 flags across 3 enums
- **Build status**: ✅ Successful (0 errors, 4943 warnings - unrelated)

## Future Enhancements

1. **DDSURFACEDESC2** - Complete surface description structure (124 bytes)
2. **DDBLTFX** - Complete blit effects structure
3. **DDOVERLAYFX** - Complete overlay effects structure
4. **DDCAPS_DX7** - Complete device capabilities structure

These can be added in future PRs as needed.
