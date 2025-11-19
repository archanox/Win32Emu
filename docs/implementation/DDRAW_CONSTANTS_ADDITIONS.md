# DirectDraw Constants and Enums Additions

## Overview

This document describes the additions made to DirectDraw constants and enums based on analysis of the Olde-Skuul/directdraw repository and official Microsoft DirectX SDK documentation.

## Changes Made

### 1. Extended DDCaps2 Enum

**File**: `Win32Emu/Win32/NativeTypes.cs`

**Added Flags**:
- `DDCAPS2_FLIPINTERVAL` (0x00200000) - Supports DDFLIP_INTERVAL flags
- `DDCAPS2_FLIPNOVSYNC` (0x00400000) - Supports DDFLIP_NOVSYNC  
- `DDCAPS2_CANMANAGETEXTURE` (0x00800000) - Device can manage textures
- `DDCAPS2_TEXMANINNONLOCALVIDMEM` (0x01000000) - Texture manager uses non-local video memory
- `DDCAPS2_STEREO` (0x02000000) - Stereo driver
- `DDCAPS2_SYSTONONLOCAL_AS_SYSTOLOCAL` (0x04000000) - System to local blit uses same path as system to non-local

**Purpose**: These extended capability flags allow games to query more specific DirectDraw hardware capabilities, particularly for texture management and advanced display features.

### 2. New DDLock Enum

**File**: `Win32Emu/Win32/NativeTypes.cs`

**All Flags**:
- `DDLOCK_SURFACEMEMORYPTR` (0x00000000) - Default behavior
- `DDLOCK_EVENT` (0x00000002) - Not currently implemented
- `DDLOCK_READONLY` (0x00000010) - Surface can only be read
- `DDLOCK_WRITEONLY` (0x00000020) - Surface is write-enabled
- `DDLOCK_NOSYSLOCK` (0x00000800) - Do not take Win16Mutex
- `DDLOCK_WAIT` (0x00001000) - Retry until lock obtained
- `DDLOCK_NOOVERWRITE` (0x00001000) - DirectX 7+ vertex buffer flag (same value as WAIT)
- `DDLOCK_DISCARDCONTENTS` (0x00002000) - DirectX 7+ vertex buffer flag
- `DDLOCK_DONOTWAIT` (0x00004000) - Override default WAIT behavior
- `DDLOCK_OKTOSWAP` (0x00002000) - Obsolete, replaced by DISCARDCONTENTS

**Purpose**: These flags control surface locking behavior in `IDirectDrawSurface::Lock` method. Previously missing from our implementation.

**Note on Overlapping Values**: Some flags share the same value because they are context-dependent:
- `DDLOCK_WAIT` and `DDLOCK_NOOVERWRITE` (both 0x00001000) - NOOVERWRITE only applies to D3D vertex buffers
- `DDLOCK_DISCARDCONTENTS` and `DDLOCK_OKTOSWAP` (both 0x00002000) - OKTOSWAP is obsolete

## Source References

1. **Olde-Skuul DirectDraw Repository**: https://github.com/Olde-Skuul/directdraw
   - MIT Licensed header collection for DirectDraw 7.0
   - Provides authoritative constant values across compilers

2. **Microsoft DirectX SDK Documentation**:
   - IDirectDrawSurface7::Lock: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nf-ddraw-idirectdrawsurface7-lock
   - DDCAPS Structure: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddcaps_dx7

3. **ReactOS DDraw Headers**: https://doxygen.reactos.org/d7/de9/sdk_2include_2psdk_2ddraw_8h_source.html
   - Open source reference implementation

## Usage Examples

### Using DDLock Flags

```csharp
// Lock a surface for read-only access without blocking
var lockFlags = DDLock.DDLOCK_READONLY | DDLock.DDLOCK_DONOTWAIT;

// Lock a vertex buffer and discard previous contents
var vbLockFlags = DDLock.DDLOCK_DISCARDCONTENTS | DDLock.DDLOCK_WRITEONLY;

// Safe lock with wait for completion
var safeLockFlags = DDLock.DDLOCK_WAIT;
```

### Using Extended DDCaps2 Flags

```csharp
// Check if device supports texture management
if ((caps2 & DDCaps2.DDCAPS2_CANMANAGETEXTURE) != 0)
{
    _logger.LogInformation("Device supports texture management");
}

// Check for windowed rendering support
if ((caps2 & DDCaps2.DDCAPS2_CANRENDERWINDOWED) != 0)
{
    _logger.LogInformation("Device supports windowed rendering");
}
```

## Impact on Compatibility

### Games That Benefit

1. **DirectX 6-7 Games** (1999-2001):
   - Games using advanced surface locking modes
   - Games querying texture management capabilities
   - Games with windowed mode support

2. **Specific Use Cases**:
   - **3D Games**: Use DDLOCK_DISCARDCONTENTS for vertex buffers
   - **Software Renderers**: Use DDLOCK_READONLY for blit operations
   - **Texture-Heavy Games**: Query DDCAPS2_CANMANAGETEXTURE

### Backward Compatibility

- ✅ **Fully backward compatible** - all additions are new flags
- ✅ **No breaking changes** - existing code unaffected
- ✅ **Default behavior unchanged** - DDLOCK_SURFACEMEMORYPTR is 0x00

## Testing Recommendations

1. **Unit Tests**: Verify flag values match DirectX SDK
2. **Integration Tests**: Test lock operations with different flag combinations
3. **Regression Tests**: Ensure existing games still work

## Future Enhancements

Based on this analysis, future additions could include:

1. **DDSCAPS2 Enum**: Extended surface capability flags
2. **DDFLIP Enum**: Page flipping flags (referenced by DDCAPS2_FLIPINTERVAL)
3. **Additional Result Codes**: More specific error codes

## Conclusion

These additions improve Win32Emu's DirectDraw API completeness by:
- ✅ Adding missing surface lock flags (DDLock enum)
- ✅ Extending capability reporting (DDCaps2 flags)
- ✅ Maintaining 100% backward compatibility
- ✅ Following official SDK documentation
- ✅ Supporting DirectX 6-7 era games

**Total additions**: 16 new constants (6 DDCaps2 flags + 10 DDLock flags)
**Lines of code**: ~80 lines (including documentation)
**Breaking changes**: None
**Build status**: ✅ Successful (0 errors, 4841 warnings - unrelated)
