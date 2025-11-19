# DirectDraw Structures and Enums Implementation Plan

## Based on Olde-Skuul DirectDraw Repository and Microsoft SDK

### Current Status

**What We Have:**
- DDSURFACEDESC (minimal - 6 fields)
- DDCOLORKEY (complete - 2 fields)
- DDPIXELFORMAT (complete - 8 fields)
- DDSCaps enum (minimal - 6 flags)
- Various DD enums (partially complete)

**What We Need to Add:**

### 1. DDSCAPS Structure (4 bytes)
```csharp
public struct DDSCAPS
{
    public DDSCaps dwCaps;  // Using existing enum
}
```

### 2. DDSCAPS2 Structure (16 bytes)
```csharp
public struct DDSCAPS2
{
    public DDSCaps dwCaps;        // Base capabilities (reuse existing enum)
    public DDSCaps2Flags dwCaps2; // Extended capabilities (new enum)
    public DDSCaps3Flags dwCaps3; // Additional capabilities (new enum)
    public uint dwCaps4;          // Union with dwVolumeDepth
}
```

### 3. Complete DDSURFACEDESC2 Structure (124 bytes)
This is the full surface description structure needed for DirectDraw 7.

### 4. Extended Enum Values

**DDSCaps enum** - Add missing flags:
- DDSCAPS_3DDEVICE
- DDSCAPS_ALPHA
- DDSCAPS_FRONTBUFFER
- DDSCAPS_MIPMAP
- DDSCAPS_MODEX
- DDSCAPS_OVERLAY
- DDSCAPS_PALETTE
- DDSCAPS_SYSTEMMEMORY
- DDSCAPS_TEXTURE
- DDSCAPS_VISIBLE
- DDSCAPS_WRITEONLY
- DDSCAPS_ZBUFFER
- And many more...

**New DDSCaps2Flags enum** - For dwCaps2 field
**New DDSCaps3Flags enum** - For dwCaps3 field

### Implementation Approach

1. Add complete DDSCAPS enum with all flags
2. Add new DDSCaps2Flags enum
3. Add new DDSCaps3Flags enum  
4. Add DDSCAPS struct
5. Add DDSCAPS2 struct
6. Add complete DDSURFACEDESC2 struct
7. Update existing DDSURFACEDESC to be complete

### Priority Order

1. **High Priority** - DDSCAPS2 struct and enums (most requested)
2. **Medium Priority** - Complete DDSURFACEDESC2  
3. **Low Priority** - Additional helper structs

