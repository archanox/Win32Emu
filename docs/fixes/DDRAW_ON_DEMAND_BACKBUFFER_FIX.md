# DirectDraw On-Demand Backbuffer Creation Fix

## Problem

After implementing the initial backbuffer creation fix, some DirectDraw applications were still failing with the error:
```
[DDraw] No attached surfaces found for surface 0x71000000
```

This resulted in the application showing "Backbuffer couldn't be obtained" error message and subsequently crashing with:
```
fail: Win32Emu.Emulator[0]
      Calculated memory address out of range: 0x40E20043 (EIP=0x001FFC65)
```

## Root Cause Analysis

The issue occurred in the following scenario:
1. Application creates a primary DirectDraw surface
2. For some reason, the backbuffer is not created during surface creation (e.g., surface was created before the fix was applied, or the surface creation flags didn't match the expected pattern)
3. Application calls `GetAttachedSurface` with `DDSCAPS_BACKBUFFER` (0x00000004) flag
4. The method finds no attached surfaces and returns `DDERR_NOTFOUND` (0x887601C2)
5. Application shows error message and attempts to continue with invalid state
6. Execution crashes due to corrupted registers and invalid memory access

## Solution

Enhanced the `Surface_GetAttachedSurface` method in `DDrawModule.cs` to create backbuffers on-demand when:
- The surface is a primary surface (`surface.IsPrimary == true`)
- The requested capabilities include `DDSCAPS_BACKBUFFER` (0x00000004)
- No attached surfaces currently exist (`surface.AttachedSurfaces.Count == 0`)

### Implementation Details

When these conditions are met, the code:
1. Retrieves the DirectDraw object to determine bits per pixel
2. Creates a new `DirectDrawSurface` with the same dimensions as the primary surface
3. Allocates memory for the backbuffer pixel data
4. Creates a complete COM vtable for the backbuffer with all IDirectDrawSurface methods
5. Creates the COM object and stores its address
6. Attaches the backbuffer to the primary surface's `AttachedSurfaces` list
7. Returns the backbuffer COM object pointer to the caller

### Code Changes

**File:** `Win32Emu/Win32/Modules/DDrawModule.cs`  
**Method:** `Surface_GetAttachedSurface`  
**Lines added:** ~95 lines

The fix is placed inside the existing check for `surface.AttachedSurfaces.Count == 0`, before returning `DDERR_NOTFOUND`. This ensures that:
- The fix only activates when truly needed (no backbuffer exists)
- It doesn't interfere with surfaces that already have attached surfaces
- It's a fallback mechanism that complements the initial backbuffer creation logic

## Benefits

1. **Robustness**: Applications work correctly even if backbuffers weren't created during surface initialization
2. **Compatibility**: Handles edge cases where surface creation patterns differ from expected
3. **Minimal Changes**: The fix is localized to the GetAttachedSurface method, reducing risk
4. **Fail-Safe**: Provides a safety net for the initial backbuffer creation logic

## Testing

- Added documentation test: `GetAttachedSurface_ShouldCreateBackbufferOnDemand()`
- All existing DirectDraw tests continue to pass
- CodeQL security analysis: No vulnerabilities found

## Logging

The fix adds informative log messages:
```
[DDraw] Primary surface needs backbuffer, creating on-demand
[DDraw] Created on-demand backbuffer at surface handle 0x{Handle:X8}, COM object at 0x{ComAddr:X8}
[DDraw] Returning on-demand backbuffer COM object at 0x{ComAddr:X8}
```

These help diagnose when on-demand creation is triggered and verify the backbuffer was created successfully.

## DirectDraw Constants Reference

- `DDSCAPS_BACKBUFFER` = 0x00000004 - Surface is a backbuffer
- `DDERR_NOTFOUND` = 0x887601C2 - The requested item was not found
- `DD_OK` = 0x00000000 - Success

## Related Fixes

This fix builds upon:
- `DDRAW_BACKBUFFER_FIX.md` - Initial backbuffer creation during surface creation
- `EBP_COM_POINTER_FIX.md` - EBP register corruption detection and recovery

Together, these fixes provide comprehensive support for DirectDraw backbuffer operations.
