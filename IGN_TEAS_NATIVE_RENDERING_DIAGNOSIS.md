# IGN_TEAS Native Headless Rendering Diagnosis

## Executive Summary

**Issue**: ign_teas shows only black screens when running headlessly with frame dumping on native version.

**Root Cause Found**: DirectDraw COM vtable methods (Lock, Unlock, Flip) are registered but **NOT being executed**. The game calls these methods via vtable, but the handler functions never run.

## Diagnostic Findings

### Test Environment
- Platform: Linux (Ubuntu 24.04.3 LTS)
- Backend: Software (CPU rendering)
- Frame Dumping: Enabled (`WIN32EMU_FRAME_DUMP_PATH` set)
- Mode: Headless (`SDL_VIDEODRIVER=dummy`)
- Binary: Native .NET 10 build

### Evidence

#### 1. Frame Dumps Analysis
- **Captured**: 50+ frames successfully dumped to disk
- **File Size**: All frames are exactly 2,263 bytes (640x480 PNG)
- **Content**: All frames are identical (same MD5 hash: `d6ed5f43b761a407b511604d6e8a158c`)
- **Finding**: Frames contain only black pixels (no rendered content)

```bash
$ md5sum test-screenshots/ign_teas_native_run/frame_0000{00,25,52}.png
d6ed5f43b761a407b511604d6e8a158c  frame_000000.png
d6ed5f43b761a407b511604d6e8a158c  frame_000025.png
d6ed5f43b761a407b511604d6e8a158c  frame_000052.png
```

#### 2. DirectDraw Initialization
DirectDraw initializes successfully:
- ✅ `DirectDrawCreate` called and returns success
- ✅ `SetCooperativeLevel` called (DDSCL_FULLSCREEN | DDSCL_EXCLUSIVE)
- ✅ `SetDisplayMode` called (640x480, 8bpp)
- ✅ Primary surface created with backbuffer (DDSCAPS_PRIMARYSURFACE | DDSCAPS_FLIP | DDSCAPS_COMPLEX)
- ✅ Backbuffer created and attached
- ✅ `SetPalette` called (palette assigned to surface)
- ✅ Software rendering backend initialized successfully

```
[BackendFactory] Using Software rendering backend as requested
[Software] Frame dumping enabled. Frames will be saved to: /home/runner/work/Win32Emu/Win32Emu/test-screenshots/ign_teas_native_run
[Software] Initializing SDL3 software rendering backend (640x480)...
[Software] Created software renderer
[Software] Software rendering backend initialized successfully
```

#### 3. COM Vtable Methods Registered
DirectDraw surface vtable methods are properly registered:
- ✅ `IDirectDrawSurface::Lock` → 0x0D003190
- ✅ `IDirectDrawSurface::Unlock` → 0x0D003200
- ✅ `IDirectDrawSurface::Flip` → 0x0D0020B0 (primary), 0x0D0030B0 (backbuffer)
- ✅ `IDirectDrawSurface::SetPalette` → 0x0D0021F0

#### 4. Critical Problem: Vtable Methods Not Executed

**Key Finding**: The game calls COM vtable methods, but the **handlers never execute**.

Evidence from logs:
```
[COM] Invoking vtable method (async fallback to sync): IDirectDrawSurface::Lock at address 0x0D003190
```

**But NO subsequent logs from the Lock handler:**
- ❌ NO `[DDraw COM] IDirectDrawSurface::Lock(...)` log
- ❌ NO `[DDraw COM] IDirectDrawSurface::Unlock(...)` log
- ❌ NO `[DDraw COM] IDirectDrawSurface::Flip(...)` log

**Expected**: The Lock handler (line 3695 in DDrawModule.cs) should log:
```csharp
_logger.LogInformation("[DDraw COM] IDirectDrawSurface::Lock(this=0x{ThisPtr:X8}, ...)");
```

**Observed**: This log NEVER appears in 11,024 lines of debug output.

#### 5. Rendering Pipeline Flow (Expected)

For rendering to work in DirectDraw, this flow should occur:

1. Game calls `Lock()` on backbuffer surface → allocates memory, returns pointer
2. Game writes pixel data to the returned memory pointer
3. Game calls `Unlock()` on backbuffer surface → marks surface dirty
4. Game calls `Flip()` on primary surface → copies backbuffer to primary, calls `UpdateRenderingBackend()`
5. `UpdateRenderingBackend()` converts surface to RGBA and calls backend's `UpdateFrameBuffer()`
6. `UpdateFrameBuffer()` saves frame to disk (when frame dumping enabled)

**Current Status**:
- ❌ Step 1: Lock never executes (handler not called)
- ❌ Step 3: Unlock never executes (handler not called)
- ❌ Step 4: Flip never executes (handler not called)
- ❌ Step 5: UpdateRenderingBackend never called (no logs)
- ❌ Step 6: UpdateFrameBuffer never called (no logs)

Result: Black frames are dumped because the surface data is never populated.

## Technical Analysis

### COM Vtable Invocation Issue

The log shows:
```
[COM] Invoking vtable method (async fallback to sync): IDirectDrawSurface::Lock at address 0x0D003190
```

The phrase "async fallback to sync" suggests the COM infrastructure is attempting to invoke the method, but something prevents the actual handler from executing.

### Possible Causes

1. **Async COM Handler Registration Issue**: The vtable methods may be registered as async but the fallback to sync execution is failing
2. **CPU State Corruption**: Register state might be incorrect when entering the handler
3. **Stack Corruption**: Stack pointer or return address might be corrupted
4. **Handler Lookup Failure**: The vtable address might not correctly map to the handler function

### Code References

- Flip implementation: `Win32Emu/Win32/Modules/DDrawModule.cs:1991` (Surface_Flip)
- Lock implementation: `Win32Emu/Win32/Modules/DDrawModule.cs:3686` (Surface_Lock)
- UpdateRenderingBackend: `Win32Emu/Win32/Modules/DDrawModule.cs:3911`
- Frame dumping: `Win32Emu.Gui/Backends/SoftwareRenderingBackend.cs:307` (SaveFrameToDisk)

## Impact

- Game cannot render anything (black screen only)
- Frame dumping works mechanically but captures empty frames
- All other DirectX initialization works correctly
- This affects ALL games that use DirectDraw Lock/Unlock rendering pattern

## Next Steps

1. **Debug COM vtable dispatch mechanism** - Find why "async fallback to sync" fails to execute handlers
2. **Add instrumentation** to vtable method invocation code to trace execution path
3. **Check CPU state** at vtable call site - verify registers, stack, and instruction pointer
4. **Test simpler DirectDraw sample** - Verify if Lock/Unlock work with test executables like simple_ddraw.exe
5. **Review async COM infrastructure** - Check if sync fallback is properly implemented

## Logs

- Full debug log: `/tmp/ign_teas_full_debug.log` (11,024 lines)
- Stdout log: `/tmp/ign_teas_run.log`
- Frame dumps: `/home/runner/work/Win32Emu/Win32Emu/test-screenshots/ign_teas_native_run/`

## Test Command

```bash
cd /home/runner/work/Win32Emu/Win32Emu/EXEs/ign_teas
export SDL_VIDEODRIVER=dummy
export WIN32EMU_FRAME_DUMP_PATH=/home/runner/work/Win32Emu/Win32Emu/test-screenshots/ign_teas_native_run
timeout 30 dotnet /home/runner/work/Win32Emu/Win32Emu/Win32Emu.Gui/bin/Release/net10.0/Win32Emu.Gui.dll \
  --nogui --backend Software --debug IGN_TEAS.EXE
```

---

**Date**: 2026-02-06
**Diagnosed by**: Claude Code Agent
