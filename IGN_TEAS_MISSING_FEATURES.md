# IGN_TEAS: Missing and Incomplete Features Analysis

Based on comprehensive review of the decompilation files in `/Decomp/ign_teas/` and comparison with the current emulator implementation.

## Summary of Findings

The emulator has **excellent structural support** for DirectX with proper COM vtable implementation. However, many methods are **stubs that return success without performing actual work**.

### Current Implementation Status

| Component | Structure | Methods | Functionality |
|-----------|----------|---------|---------------|
| DirectDraw | ✅ Complete | ⚠️ Mostly Stubs | ⚠️ Partial |
| DirectInput | ✅ Complete | ❌ All Stubs | ❌ None |
| DirectSound | ✅ Complete | ❌ All Stubs | ❌ None |

## Missing/Incomplete DirectInput Implementation

**Critical Issue**: The game will not respond to any keyboard or mouse input.

### IDirectInputDevice Interface

All methods except `QueryInterface`, `AddRef`, and `Release` are non-functional stubs:

#### 1. SetDataFormat (Vtable offset +44)
**Current**: Stub that logs and returns 0
```csharp
private uint DInputDevice_SetDataFormat(ICpu cpu, VirtualMemory memory)
{
    var args = new StackArgs(cpu, memory);
    var thisPtr = args.UInt32(0);
    var lpdf = args.UInt32(1);
    _logger.LogInformation("[DInput COM] IDirectInputDevice::SetDataFormat(this=0x{ThisPtr:X8}, lpdf=0x{Lpdf:X8}) - stub", thisPtr, lpdf);
    return 0; // DI_OK
}
```

**Needed**:
- Parse `DIDATAFORMAT` structure at address `lpdf`
- Read `dwSize`, `dwObjSize`, `dwFlags`, `dwDataSize`, `dwNumObjs`
- Read array of `DIOBJECTDATAFORMAT` structures
- Store format for later use in `GetDeviceState` and `GetDeviceData`

**Evidence from Decompilation** (hexrays.cpp:4847):
```cpp
// SetDataFormat called with pointer to predefined format structure
if ( (*(int (__stdcall **)(int, int *))(*(_DWORD *)dword_43D1BC + 44))(dword_43D1BC, dword_40A480) )
    return 0;
```

The game uses a predefined format at `dword_40A480` which defines which keyboard keys/mouse buttons to track.

#### 2. SetCooperativeLevel (Vtable offset +52)
**Current**: Stub that logs and returns 0
```csharp
private uint DInputDevice_SetCooperativeLevel(ICpu cpu, VirtualMemory memory)
{
    var args = new StackArgs(cpu, memory);
    var thisPtr = args.UInt32(0);
    var hwnd = args.UInt32(1);
    var dwFlags = args.UInt32(2);
    _logger.LogInformation("[DInput COM] IDirectInputDevice::SetCooperativeLevel(this=0x{ThisPtr:X8}, hwnd=0x{Hwnd:X8}, flags=0x{DwFlags:X8}) - stub", thisPtr, hwnd, dwFlags);
    return 0; // DI_OK
}
```

**Needed**:
- Store `hwnd` and `dwFlags` for the device
- Recognize flags:
  - `DISCL_NONEXCLUSIVE` (0x02) - Share device with other apps
  - `DISCL_FOREGROUND` (0x04) - Only get input when window has focus
  - `DISCL_EXCLUSIVE` (0x01) - Exclusive access
  - `DISCL_BACKGROUND` (0x08) - Get input even when not focused

**Evidence** (hexrays.cpp:4849):
```cpp
// Called with flags = 6 (DISCL_NONEXCLUSIVE | DISCL_FOREGROUND)
if ( (*(int (__stdcall **)(int, HWND, int))(*(_DWORD *)dword_43D1BC + 52))(dword_43D1BC, hWnd, 6) )
    return 0;
```

#### 3. SetProperty (Vtable offset +24)
**Current**: Stub that logs and returns 0

**Needed**:
- Parse property GUID and DIPROPHEADER
- Support at minimum:
  - `DIPROP_BUFFERSIZE` - Set buffered input queue size
  - `DIPROP_AXISMODE` - Set axis mode (absolute vs relative)
  - `DIPROP_RANGE` - Set axis value range

**Evidence** (hexrays.cpp:4851):
```cpp
// SetProperty called with property ID 1 and property data in v4
if ( (*(int (__stdcall **)(int, int, _DWORD *))(*(_DWORD *)dword_43D1BC + 24))(dword_43D1BC, 1, v4) )
    return 0;
```

#### 4. Acquire (Vtable offset +28)
**Current**: Stub that logs and returns 0

**Needed**:
- Mark device as "acquired"
- Begin capturing input events from the rendering backend
- Subscribe to keyboard/mouse events
- Fill input buffer

**Evidence** (hexrays.cpp:4853 and 4962):
```cpp
// Called after setup to begin capturing input
dword_43D1C0 = (*(int (__stdcall **)(int))(*(_DWORD *)dword_43D1BC + 28))(dword_43D1BC) >= 0;

// Called again later (possibly to re-acquire after losing focus)
result = (*(int (__stdcall **)(int))(*(_DWORD *)dword_43D1BC + 28))(dword_43D1BC);
```

#### 5. GetDeviceState (Vtable offset +36)
**Current**: Zeros out buffer and returns 0
```csharp
private uint DInputDevice_GetDeviceState(ICpu cpu, VirtualMemory memory)
{
    var args = new StackArgs(cpu, memory);
    var thisPtr = args.UInt32(0);
    var cbData = args.UInt32(1);
    var lpvData = args.UInt32(2);
    _logger.LogInformation("[DInput COM] IDirectInputDevice::GetDeviceState(this=0x{ThisPtr:X8}, cbData={CbData}, lpvData=0x{LpvData:X8}) - stub", thisPtr, cbData, lpvData);
    
    // Zero out the device state buffer
    if (lpvData != 0 && cbData > 0)
    {
        _env.MemZero(lpvData, cbData);
    }
    return 0; // DI_OK
}
```

**Needed**:
- Query current keyboard/mouse state from input backend
- Fill buffer according to `DIDATAFORMAT` from `SetDataFormat`
- For keyboard: Array of 256 bytes (one per key, 0x80 = pressed, 0x00 = released)
- For mouse: Structure with X, Y, Z deltas and button states

**Evidence**: Not directly called in visible decompilation but would be called in game loop (possibly in functions we haven't fully analyzed).

#### 6. GetDeviceData (Vtable offset +40)
**Current**: Stub that logs and returns 0

**Needed**:
- Return buffered input events since last call
- Fill array of `DIDEVICEOBJECTDATA` structures
- Each structure contains:
  - `dwOfs` - Offset in data format
  - `dwData` - Value (key pressed/released, mouse moved, etc.)
  - `dwTimeStamp` - Time of event
  - `dwSequence` - Sequence number

**Evidence** (hexrays.cpp:4953-4962):
```cpp
// GetDeviceData called in input polling loop
v5 = (*(int (__stdcall **)(int, int, _BYTE *, int *, _DWORD))(*(_DWORD *)dword_43D1BC + 40))(
       dword_43D1BC,
       256,           // Size of each element
       v1,            // Output buffer
       v2,            // In/out: number of elements
       0);            // Flags
```

The game uses buffered input events rather than just polling state.

#### 7. Unacquire (Vtable offset +32)
**Current**: Stub that logs and returns 0

**Needed**:
- Mark device as "unacquired"
- Stop capturing input events
- Clear input buffer

**Evidence** (hexrays.cpp:4893):
```cpp
// Called during cleanup
if ( dword_43D1C0 )
{
    (*(void (__stdcall **)(int))(*(_DWORD *)dword_43D1BC + 32))(dword_43D1BC);
    dword_43D1C0 = 0;
}
```

### Implementation Priority for DirectInput

1. **Critical** (Game Breaking):
   - `SetDataFormat` - Required to understand input layout
   - `Acquire` - Required to begin capturing input
   - `GetDeviceState` or `GetDeviceData` - At least one must work

2. **High** (Major Functionality):
   - `SetCooperativeLevel` - Affects input behavior
   - The second of `GetDeviceState`/`GetDeviceData` not implemented above

3. **Medium** (Polish):
   - `SetProperty` - For fine-tuning input behavior
   - `Unacquire` - For cleanup (less critical during testing)

## Missing/Incomplete DirectSound Implementation

**Critical Issue**: The game will have no audio.

### IDirectSound Interface

#### SetCooperativeLevel (Vtable offset +12)
**Current**: Stub
**Needed**: Store cooperation level (DSSCL_NORMAL, DSSCL_PRIORITY, etc.)

**Evidence** (hexrays.cpp:4434-4436):
```cpp
// Try DSSCL_WRITEPRIMARY (4) first, fall back to DSSCL_NORMAL (3)
if ( ppDS->lpVtbl->SetCooperativeLevel(ppDS, (HWND)dword_41C7AC, 4) )
{
    if ( !ppDS->lpVtbl->SetCooperativeLevel(ppDS, (HWND)dword_41C7AC, 3) )
    {
        // Continue with sound initialization
    }
}
```

#### GetCaps (Vtable offset +20)
**Current**: Stub
**Needed**: Return `DSCAPS` structure with device capabilities

**Evidence** (hexrays.cpp:4501):
```cpp
// Query capabilities to check for hardware mixing support
if ( !ppDS->lpVtbl->GetCaps(ppDS, (LPDSCAPS)v35) && (v35[1] & 0x20) == 0 )
{
    // Check if DSCAPS_SECONDARYMONO flag is set
}
```

### IDirectSoundBuffer Interface

All buffer methods are stubs. These are needed for audio playback:

#### Lock (Vtable offset +52)
**Needed**: 
- Allocate or return pointer to audio buffer memory
- Support two-region locking for circular buffers
- Write lock info to output parameters

#### Unlock (Vtable offset +76)
**Needed**:
- Mark buffer regions as unlocked
- Schedule audio data for playback

#### Play (Vtable offset +48)
**Needed**:
- Start playing audio buffer
- Support looping flag
- Integrate with audio backend

#### Stop (Vtable offset +68)
**Needed**:
- Stop playing audio buffer
- Preserve current position (unless requested to reset)

#### SetFormat (Vtable offset +60)
**Needed**:
- Parse `WAVEFORMATEX` structure
- Store audio format (sample rate, bits per sample, channels)
- Reconfigure audio backend if necessary

**Evidence**: While the decompilation shows sound buffer creation, the actual playback code may be in areas we haven't fully analyzed. Audio is less critical than input for initial testing.

### Implementation Priority for DirectSound

1. **High** (Major Feature):
   - `Lock` / `Unlock` - Required for writing audio data
   - `SetFormat` - Required for configuring audio
   - `Play` - Required for playback

2. **Medium** (Enhancement):
   - `Stop` - For controlling playback
   - `GetCaps` - For capability detection
   - `SetCooperativeLevel` - For sharing audio device

3. **Low** (Optional):
   - `SetVolume`, `GetVolume` - Volume control
   - `SetCurrentPosition`, `GetCurrentPosition` - Position control

## Missing/Incomplete DirectDraw Features

Most DirectDraw methods are implemented, but some may need enhancement:

### CreateClipper (Vtable offset +28)
**Current**: Stub
**Needed**: Create `IDirectDrawClipper` COM object for windowed mode clipping

**Evidence** (hexrays.cpp:5598):
```cpp
if ( lpDD->lpVtbl->CreateClipper(lpDD, 0, (LPDIRECTDRAWCLIPPER *)&dword_41C9E8, 0) )
    return 0;
```

The game calls this but may not strictly require it to run. It's used for clipping when in windowed mode.

### EnumDisplayModes (Vtable offset +36)
**Current**: Stub
**Needed**: Enumerate available display modes via callback

Not seen in analyzed decompilation, but may be called.

## Additional Win32 API Considerations

All 137 required Win32 APIs are marked as implemented in `IGNITION_API_STATUS.md`. However, some may need behavioral enhancements:

### Window Messages

From `IGN_TEAS_IMPLEMENTATION_ANALYSIS.md`, the game depends on:

- **WM_ACTIVATEAPP** (0x001C) - Critical for game loop activation
- **WM_CREATE** - Sent when window is created
- **WM_SIZE** - Sent when window is resized
- **WM_MOVE** - Sent when window is moved

These messages should be automatically sent by `CreateWindowExA` and `ShowWindow`.

### Message Queue Behavior

Ensure proper message delivery:
- `PostMessageA` / `SendMessageA` - Should queue/dispatch messages
- `PeekMessageA` / `GetMessageA` - Should retrieve messages
- `DispatchMessageA` - Should call window procedure

## Testing Approach

### Phase 1: Verification (Current)
- ✅ Confirm all DirectX objects are created with COM vtables
- ✅ Confirm game reaches main loop
- [ ] Identify which methods are being called
- [ ] Determine which stubs are blocking progress

### Phase 2: Input Implementation
- [ ] Implement `SetDataFormat` to parse input format
- [ ] Implement `Acquire` to begin input capture
- [ ] Implement `GetDeviceState` or `GetDeviceData` to return input
- [ ] Test that game responds to keyboard/mouse

### Phase 3: Audio Implementation (Optional)
- [ ] Implement `Lock`/`Unlock` for audio buffer access
- [ ] Implement `SetFormat` to configure audio
- [ ] Implement `Play` to start audio playback
- [ ] Test that game has audio

### Phase 4: Polish
- [ ] Implement remaining stub methods as needed
- [ ] Optimize performance
- [ ] Add proper error handling

## Code Structure Recommendations

### DirectInput Integration

```csharp
// In DInputModule.cs, enhance device implementation:

private sealed class DirectInputDevice
{
    public uint Handle { get; set; }
    public uint BackendDeviceId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // NEW: Store input state
    public bool IsAcquired { get; set; }
    public DIDATAFORMAT? DataFormat { get; set; }
    public uint CooperativeLevel { get; set; }
    public uint WindowHandle { get; set; }
    
    // NEW: Input buffers
    public byte[] KeyboardState { get; set; } = new byte[256];
    public Queue<DIDEVICEOBJECTDATA> EventQueue { get; set; } = new();
}

// NEW: Parse DIDATAFORMAT structure
private DIDATAFORMAT ParseDataFormat(uint lpdf)
{
    var dwSize = _env.MemRead32(lpdf);
    var dwObjSize = _env.MemRead32(lpdf + 4);
    var dwFlags = _env.MemRead32(lpdf + 8);
    var dwDataSize = _env.MemRead32(lpdf + 12);
    var dwNumObjs = _env.MemRead32(lpdf + 16);
    var rgodf = _env.MemRead32(lpdf + 20);
    
    // Parse object format array...
    return new DIDATAFORMAT { /* ... */ };
}

// NEW: Wire to input backend
private uint DInputDevice_GetDeviceState(ICpu cpu, VirtualMemory memory)
{
    // Get device from handle
    var device = GetDeviceFromThisPtr(thisPtr);
    
    // Query input backend for current state
    var keyState = _env.InputBackend?.GetKeyboardState() ?? new byte[256];
    
    // Copy to output buffer according to DataFormat
    CopyStateToBuffer(keyState, lpvData, device.DataFormat);
    
    return 0; // DI_OK
}
```

### DirectSound Integration

```csharp
// In DSoundModule.cs, enhance buffer implementation:

private sealed class DirectSoundBuffer
{
    public uint Handle { get; set; }
    public uint AudioStreamId { get; set; }
    public int Size { get; set; }
    public byte[]? Data { get; set; }
    public bool IsPrimary { get; set; }
    
    // NEW: Audio state
    public WAVEFORMATEX? Format { get; set; }
    public bool IsPlaying { get; set; }
    public uint PlayPosition { get; set; }
    public uint WritePosition { get; set; }
}

// NEW: Implement Lock
private uint DSoundBuffer_Lock(ICpu cpu, VirtualMemory memory)
{
    // Allocate buffer if needed
    if (buffer.Data == null && buffer.Size > 0)
    {
        buffer.Data = new byte[buffer.Size];
    }
    
    // Return pointer(s) to buffer region(s)
    var ptr1 = _env.SimpleAlloc((uint)buffer.Size);
    _env.MemWrite32(ppvAudioPtr1, ptr1);
    _env.MemWrite32(pdwAudioBytes1, (uint)buffer.Size);
    
    return 0; // DS_OK
}

// NEW: Wire to audio backend
private uint DSoundBuffer_Play(ICpu cpu, VirtualMemory memory)
{
    buffer.IsPlaying = true;
    
    // Send audio data to backend
    if (_env.AudioBackend != null && buffer.Data != null && buffer.Format != null)
    {
        buffer.AudioStreamId = _env.AudioBackend.CreateStream(buffer.Format);
        _env.AudioBackend.WriteData(buffer.AudioStreamId, buffer.Data);
        _env.AudioBackend.Play(buffer.AudioStreamId, isLooping);
    }
    
    return 0; // DS_OK
}
```

## Success Metrics

The implementation will be considered complete when:

1. ✅ Game initializes all DirectX components
2. ✅ Game creates window and enters main loop
3. ⚠️ Game responds to keyboard input (requires DirectInput implementation)
4. ⚠️ Game responds to mouse input (requires DirectInput implementation)
5. ⚠️ Game plays audio (requires DirectSound implementation)
6. ❓ Game renders graphics (may require additional work on rendering backend)

Currently at step 2. Steps 3-4 are blocked by DirectInput stubs, step 5 by DirectSound stubs.

## Conclusion

The emulator has excellent foundation with:
- ✅ Complete COM vtable infrastructure
- ✅ Proper DirectX module structure
- ✅ All required Win32 APIs implemented

The main gaps are:
- ❌ **DirectInput device methods** - All critical methods are stubs
- ❌ **DirectSound buffer methods** - All critical methods are stubs
- ⚠️ **Some DirectDraw methods** - Clipper creation is stub

**Priority**: Implement DirectInput device methods first, as input is essential for any interactivity. Audio can wait until after the game is responding to input.

**Estimated Effort**:
- DirectInput implementation: 4-8 hours (parsing formats, state management, backend integration)
- DirectSound implementation: 4-8 hours (buffer management, format parsing, backend integration)
- Testing and debugging: 2-4 hours

**Next Steps**:
1. Review decompilation more thoroughly to understand exact input format used
2. Implement `SetDataFormat` with full DIDATAFORMAT parsing
3. Implement `GetDeviceState` or `GetDeviceData` with backend integration
4. Test with ign_teas to verify input works
5. Move on to audio if desired
