# Testing Instructions for DirectDraw Auto-Initialization Fix

## Purpose
Test the fix for ign_teas and similar applications that don't call `SetDisplayMode` before creating DirectDraw surfaces.

## What Was Fixed
Added auto-initialization of the rendering backend when primary DirectDraw surfaces are created, ensuring applications that skip `SetDisplayMode` can still render to the canvas in the WASM frontend.

## Prerequisites

### Build the WASM Project
```bash
cd /home/runner/work/Win32Emu/Win32Emu
dotnet build Win32Emu.Wasm/Win32Emu.Wasm.csproj --configuration Release
```

### Deploy to Local Server
```bash
cd Win32Emu.Wasm/bin/Release/net9.0/wwwroot
python -m http.server 8080
```

## Test Case 1: ign_teas Executable

### Steps
1. Open browser to `http://localhost:8080`
2. Load the ign_teas executable
3. Click "Start" to begin emulation

### Expected Results

#### Before Fix (Baseline Behavior)
- ❌ Canvas remains black (no content)
- ❌ Diagnostic panel shows "Backend Initialized: No"
- ❌ Canvas Update Count: 0
- ❌ Last Update: Never

#### After Fix (Expected Behavior)
- ✅ Canvas displays game graphics
- ✅ Diagnostic panel shows "Backend Initialized: Yes"
- ✅ Canvas Update Count: > 0 (increments with each frame)
- ✅ Last Update: Shows recent timestamp (e.g., "0.5s ago")

### Log Messages to Verify
Open browser developer console and look for these log messages:

```
[DDraw] Created IDirectDrawSurface COM object at 0x... for surface 0x...
[DDraw] Auto-initializing rendering backend for primary surface (640x480)
[DDraw] Set display mode dimensions from primary surface: 640x480
[DDraw] Initialized frame buffering for WASM mode (auto-init)
[WASM] Initializing rendering backend (640x480)
[WASM] Rendering backend initialized successfully
[DDraw] Rendering backend auto-initialized successfully with 640x480 (WASM mode)
[DDraw] Subscribed to UI events from rendering backend (auto-init)
```

Later when rendering starts:
```
[DDraw COM] IDirectDrawSurface::Unlock(this=0x..., lpRect=0x00000000)
[DDraw] Unlocked surface 0x...
[WASM] UpdateFrameBuffer called: width=640, height=480, pitch=2560, dataLength=1228800
[WASM] Calling updateCanvas: canvasId=emulatorCanvas, width=640, height=480, base64Length=...
Canvas updated successfully: 640x480 (1228800 bytes, update #1)
```

## Test Case 2: Verify No Regressions

### Applications That Call SetDisplayMode
Test with applications that properly call `SetDisplayMode` to ensure they still work:

1. Load any DirectDraw application that calls `SetDisplayMode`
2. Verify it renders correctly
3. Check diagnostic panel shows "Initialized: Yes"
4. Verify no errors in console

### Expected Behavior
- ✅ Application works exactly as before
- ✅ Backend initializes in `SetDisplayMode` (not in CreateSurface)
- ✅ Canvas displays correctly
- ✅ No new errors or warnings

## Diagnostic Panel Reference

### Location
Scroll down on the home page to find "DirectDraw Diagnostics" panel

### Key Fields to Monitor

| Field | Before Fix | After Fix |
|-------|-----------|-----------|
| Backend Initialized | No | **Yes** |
| Canvas Update Count | 0 | **> 0** |
| Last Update | Never | **Recent timestamp** |
| Frame Buffer Size | Not allocated | **~1.2 MB for 640x480** |
| Error Occurred | No | No |

## Success Criteria

### Must Have (Critical)
- [ ] Canvas displays game graphics (not black)
- [ ] Diagnostic panel shows "Backend Initialized: Yes"
- [ ] Canvas Update Count increases over time
- [ ] No JavaScript errors in console
- [ ] Game is playable/interactive

### Should Have (Important)
- [ ] Auto-initialization log messages appear
- [ ] First frame renders within 1 second of surface creation
- [ ] Frame rate is acceptable (>15 FPS)
- [ ] No memory leaks over extended play

### Nice to Have (Optional)
- [ ] Performance metrics look good
- [ ] No visual artifacts
- [ ] Smooth rendering

## Troubleshooting

### If Canvas Remains Black
1. Check browser console for JavaScript errors
2. Verify log shows "Auto-initializing rendering backend"
3. Check if `UpdateFrameBuffer` is being called
4. Verify `updateCanvas` JavaScript function exists
5. Check canvas element visibility in browser DevTools

### If Initialization Fails
1. Look for error messages in logs:
   - `[DDraw] Rendering backend auto-initialization failed`
   - `[WASM] Failed to initialize rendering backend`
2. Check if canvas element exists with correct ID
3. Verify JavaScript functions are loaded
4. Check browser WebAssembly support

### If Performance Is Poor
1. Check Canvas Update Count - should be ~60/second
2. Monitor browser CPU usage
3. Check for repeated initialization (should only happen once)
4. Verify frame buffering queue isn't growing indefinitely

## Known Limitations

### Current Implementation
- Uses `GetAwaiter().GetResult()` for initialization (synchronous blocking)
- Frame buffering limited to WASM mode only
- No automatic dimension detection from display mode
- Hardcoded window title "Win32Emu DirectDraw"

### Future Improvements
- Make CreateSurface fully async
- Better dimension fallback strategies
- Performance optimizations
- Enhanced error recovery

## Reporting Issues

### Information to Collect
1. Browser console logs (full output)
2. Diagnostic panel screenshot
3. Canvas screenshot (or note that it's black)
4. Steps to reproduce
5. Browser version and platform
6. WASM project build timestamp

### Where to Report
- GitHub issue on archanox/Win32Emu repository
- Include "DirectDraw Auto-Init" in the title
- Tag with "WASM" and "rendering" labels

## Additional Test Cases

### Edge Cases to Test
1. **Multiple Surface Creation**: Create multiple primary surfaces sequentially
2. **Surface Destruction**: Destroy and recreate surfaces
3. **Dimension Changes**: Test with different surface dimensions
4. **Rapid Lock/Unlock**: Stress test with many lock/unlock cycles
5. **Early Rendering**: Draw immediately after surface creation

## Success Confirmation

Once testing is complete, confirm:
- [ ] Code compiles successfully
- [ ] Auto-initialization logic verified in code
- [ ] Documentation complete
- [ ] Manual test with ign_teas PASSED
- [ ] Regression test PASSED
- [ ] No new issues discovered

## Next Steps After Successful Testing
1. Merge PR to main branch
2. Update release notes
3. Consider backporting to stable branch
4. Monitor for any reported issues
5. Plan performance optimizations if needed
