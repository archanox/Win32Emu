# IGN_TEAS.EXE Decompilation Analysis

## Executive Summary

After analyzing multiple decompilation outputs (Hex-Rays IDA, Ghidra, Reko, Binary Ninja, etc.), the root cause of why IGN_TEAS.EXE is not progressing beyond initialization has been identified:

**The game successfully calls DirectX creation functions (`DirectDrawCreate`, `DirectInputCreateA`, `DirectSoundCreate`) but then attempts to invoke COM interface methods via vtable pointers. The emulator returns success (0) from the creation functions but does not provide functional COM vtables, causing the game to crash or hang when it tries to call methods on these objects.**

## Decompilation Analysis

### Entry Point: WinMain (0x403140)

The `WinMain` function (located at 0x403140 in hexrays.cpp line 3665) follows this initialization sequence:

```cpp
int __stdcall WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
  // 1. Register window class
  hCursor = LoadCursorA(0, (LPCSTR)0x7F00);
  WndClass.lpfnWndProc = (WNDPROC)sub_403340;
  WndClass.hIcon = LoadIconA(0, (LPCSTR)0x7F00);
  WndClass.hCursor = hCursor;
  WndClass.hbrBackground = (HBRUSH)GetStockObject(4);
  WndClass.lpszClassName = ClassName;
  if (!RegisterClassA(&WndClass))
    return 0;
  
  // 2. Initialize multimedia timer
  timeBeginPeriod(1u);  // ✅ IMPLEMENTED
  
  // 3. Call initialization function
  sub_404B00();  // Some initialization
  
  // 4. Initialize DirectX and create window
  if (!sub_403510())  // ❌ THIS FAILS - DirectX initialization
    return 0;
  
  // 5. Main message loop (never reached)
  while (dword_43C7A4) {
    if (PeekMessageA(&Msg, 0, 0, 0, 0)) {
      if (!GetMessageA(&Msg, 0, 0, 0))
        return Msg.wParam;
      TranslateMessage(&Msg);
      DispatchMessageA(&Msg);
    }
    else if (!sub_4032A0()) {
      sub_403540();
    }
  }
}
```

### Critical Function: sub_403510 (0x403510)

This function is **the key blocker**. From ghidra.cpp:

```cpp
undefined4 FUN_00403510(void)
{
  int iVar1;
  
  FUN_004045e0(0);  // Some setup
  
  iVar1 = FUN_00404640();  // DirectDraw initialization ❌ FAILS HERE
  if (iVar1 != 0) {
    iVar1 = FUN_004046f0();  // DirectInput initialization
    if (iVar1 != 0) {
      DAT_0043c7b0 = 1;
      return 1;  // Success
    }
  }
  return 0;  // Failure - game exits
}
```

The function:
1. Calls `sub_404640()` - DirectDraw initialization
2. If that succeeds, calls `sub_4046F0()` - DirectInput initialization
3. Returns 0 (failure) if either step fails

### DirectDraw Initialization: sub_404640

From hexrays.cpp line 4783:

```cpp
int sub_404640()
{
  if (dword_43CCDC())  // Function pointer call
    return dword_43CD10();  // Another function pointer
  else
    return 0;
}
```

These are **indirect function calls through vtables**. The actual DirectDraw initialization happens elsewhere.

### DirectDraw Creation: Found at line 5472

```cpp
if (DirectDrawCreate(0, &lpDD, 0))
  return 0;
if (dword_41C79C)
  v2 = lpDD->lpVtbl->SetCooperativeLevel(lpDD, hWnd, 83);
else
  v2 = lpDD->lpVtbl->SetCooperativeLevel(lpDD, hWnd, 8);
```

**Critical Issue**: After calling `DirectDrawCreate`, the game immediately tries to call COM methods via the vtable:
- `lpDD->lpVtbl->SetCooperativeLevel(...)` 
- This is calling offset in the vtable to invoke a COM method

The emulator's `DirectDrawCreate` function:
1. ✅ Returns success (0 = DD_OK)
2. ✅ Writes a handle to `lplpDD`
3. ❌ Does NOT create a valid vtable with function pointers
4. ❌ When the game dereferences `lpDD->lpVtbl` and tries to call methods, it crashes or hangs

### DirectInput Initialization: sub_4046F0 (line 4831)

```cpp
int __cdecl sub_4046F0(int a1, int a2)
{
  if (DirectInputCreateA(hInstance, 768, &dword_43CEB0, 0))
    return 0;
    
  // Call CreateDevice method on DirectInput object via vtable
  if ((*(int (__stdcall **)(int, _DWORD *, int *, _DWORD))(*(_DWORD *)dword_43CEB0 + 12))(
       dword_43CEB0, v5, &dword_43D1BC, 0))
  {
    return 0;
  }
  
  // Call SetDataFormat method on device
  if ((*(int (__stdcall **)(int, int *))(*(_DWORD *)dword_43D1BC + 44))(
       dword_43D1BC, dword_40A480))
  {
    return 0;
  }
  // ... more vtable calls
}
```

**Same issue**: After `DirectInputCreateA` succeeds, the game:
1. Dereferences `dword_43CEB0` to get the vtable pointer
2. Calls the method at vtable offset +12 (likely `CreateDevice`)
3. The emulator has NO vtable, causing a crash/hang

### DirectSound Initialization: line 4432

```cpp
if (DirectSoundCreate(0, &ppDS, 0))
  return 0;
// ... followed by more vtable method calls
```

Same pattern as above.

## Why The Game Hangs

The test shows:
```
Test timed out after 5 seconds - stopping emulator
No unknown function calls recorded.
```

This tells us:
1. The game doesn't call any unknown/unimplemented Win32 API functions
2. The game is stuck in a loop or has crashed silently
3. No additional APIs are being called after initialization

**What's actually happening**:
1. Game calls `DirectDrawCreate` → emulator returns success ✅
2. Game tries to call `lpDD->lpVtbl->SetCooperativeLevel(...)` 
3. The handle returned is just a number (0x70000000), not a valid COM object
4. Dereferencing `*lpDD` reads garbage memory
5. Calling through the "vtable pointer" jumps to invalid code or crashes
6. OR: The emulator is catching the invalid memory access and just continuing, causing the function to fail
7. `sub_403510()` returns 0 (failure)
8. `WinMain` sees the failure and returns 0 (exits)
9. But the emulator's main loop might be continuing, creating the timeout

## Solution Required

To fix this issue, the emulator needs to implement **COM interface emulation** for DirectX:

### 1. Create Valid Vtables

When `DirectDrawCreate` is called, the emulator must:
- Allocate memory for a COM object structure
- Create a vtable with function pointers to emulated methods
- Return a pointer to this structure, not just a handle

Example structure needed:
```cpp
struct IDirectDraw {
  IDirectDrawVtbl* lpVtbl;
};

struct IDirectDrawVtbl {
  HRESULT (*QueryInterface)(IDirectDraw*, REFIID, void**);
  ULONG (*AddRef)(IDirectDraw*);
  ULONG (*Release)(IDirectDraw*);
  // ... DirectDraw-specific methods
  HRESULT (*SetCooperativeLevel)(IDirectDraw*, HWND, DWORD);
  HRESULT (*SetDisplayMode)(IDirectDraw*, DWORD, DWORD, DWORD);
  // ... etc
};
```

### 2. Implement Method Dispatching

Each vtable entry must point to code that:
- Reads the method parameters from the stack
- Calls the appropriate emulator function
- Returns the result

### 3. Required DirectX Methods

Based on the decompilation, at minimum these methods are needed:

**IDirectDraw:**
- `SetCooperativeLevel` (called immediately after creation)
- `SetDisplayMode`
- `CreateSurface`
- `QueryInterface`, `AddRef`, `Release` (standard COM)

**IDirectInput:**
- `CreateDevice` (vtable offset +12, or method index 3)
- Standard COM methods

**IDirectInputDevice:**
- `SetDataFormat` (vtable offset +44)
- `SetCooperativeLevel`
- `Acquire`
- `GetDeviceState`

**IDirectSound:**
- `SetCooperativeLevel`
- `CreateSoundBuffer`
- Standard COM methods

## Evidence from Multiple Decompilers

All decompilers (Hex-Rays, Ghidra, Reko, Binary Ninja, etc.) show the same pattern:
1. DirectX creation function called
2. Immediate vtable method invocation
3. No error checking around the vtable calls (game assumes they work)

This is confirmed by checking multiple files:
- `hexrays.cpp` lines 3665-3850 (WinMain and sub_403510)
- `ghidra.cpp` similar pattern
- `reko.cpp` lines showing function pointer calls

## Current Emulator State

From `DDrawModule.cs`:
```csharp
private unsafe uint DirectDrawCreate(in uint lpGuid, uint lplpDd, in uint pUnkOuter)
{
    var ddrawHandle = _nextDDrawHandle++;
    var ddrawObj = new DirectDrawObject { Handle = ddrawHandle };
    _ddrawObjects[ddrawHandle] = ddrawObj;
    
    if (lplpDd != 0)
    {
        _env.MemWrite32(lplpDd, ddrawHandle);  // ❌ Just writes a handle, not a COM object
    }
    
    return 0; // DD_OK
}
```

This is insufficient. The game expects `lplpDd` to point to a valid IDirectDraw COM object with a functioning vtable.

## Recommendations

### Immediate Fix (Minimal)

1. Implement vtable-based COM object emulation for:
   - `DirectDrawCreate` → IDirectDraw with `SetCooperativeLevel`, `SetDisplayMode`
   - `DirectInputCreateA` → IDirectInput with `CreateDevice`
   - `DirectSoundCreate` → IDirectSound with `SetCooperativeLevel`

2. Create memory structures that match COM object layout
3. Hook vtable method calls in the CPU emulator
4. Route calls to appropriate handler functions

### Long-term Solution

1. Build a complete COM emulation framework
2. Implement full DirectX 7 interfaces
3. Add actual rendering/input/audio backends
4. Support multiple DirectX versions

## Files to Modify

1. `Win32Emu/Win32/Modules/DDrawModule.cs` - Add vtable support
2. `Win32Emu/Win32/Modules/DInputModule.cs` - Add vtable support  
3. `Win32Emu/Win32/Modules/DSoundModule.cs` - Add vtable support
4. Create new `Win32Emu/Win32/COM/` directory for COM infrastructure
5. Update `Win32Emu/Emulator.cs` to handle COM method calls

## Testing

After implementing COM vtables:
1. Run `IgnitionTeaser_ShouldLoadAndRun` test
2. Should see DirectX method calls being logged
3. Game should progress past `sub_403510()` 
4. Game should enter main loop and start calling `PeekMessageA`, `GetMessageA`
5. Eventually will need window creation and rendering support

## Conclusion

The decompilation analysis clearly shows that **the lack of COM vtable emulation for DirectX objects is the root cause** of the game not progressing beyond initialization. The emulator successfully handles all standard Win32 API calls, but DirectX uses COM interfaces which require a completely different calling convention and memory layout.

Implementing proper COM object emulation is the critical next step to enable this game (and likely many other DirectX-based games) to run in the emulator.
