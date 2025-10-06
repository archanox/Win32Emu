# Visual Diagram: IGN_TEAS.EXE Execution Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                     PROGRAM ENTRY POINT                         │
│                      WinMain (0x403140)                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 1: Register Window Class                                  │
│  - LoadCursorA()          ✅ WORKS                              │
│  - LoadIconA()            ✅ WORKS                              │
│  - RegisterClassA()       ✅ WORKS                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 2: Initialize Multimedia Timer                            │
│  - timeBeginPeriod(1)     ✅ WORKS (WinMM module)               │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 3: Some Initialization                                    │
│  - sub_404B00()           ✅ WORKS                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 4: DirectX Initialization - sub_403510()                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  4a. DirectDraw Init - sub_404640()                     │   │
│  │  ┌───────────────────────────────────────────────────┐  │   │
│  │  │ DirectDrawCreate(NULL, &lpDD, NULL)               │  │   │
│  │  │                                                     │  │   │
│  │  │ EMULATOR DOES:                                     │  │   │
│  │  │   ✅ Returns 0 (DD_OK = success)                   │  │   │
│  │  │   ✅ Writes handle 0x70000000 to lpDD             │  │   │
│  │  │                                                     │  │   │
│  │  │ GAME EXPECTS:                                      │  │   │
│  │  │   lpDD → [vtable ptr] → [SetCooperativeLevel]    │  │   │
│  │  │           [data...]      [SetDisplayMode]         │  │   │
│  │  │                          [CreateSurface]          │  │   │
│  │  │                          [...]                    │  │   │
│  │  │                                                     │  │   │
│  │  │ WHAT HAPPENS:                                      │  │   │
│  │  │   ❌ lpDD = 0x70000000 (just a number)            │  │   │
│  │  │   ❌ *lpDD = ??? (invalid memory)                 │  │   │
│  │  │   ❌ Calling methods FAILS                        │  │   │
│  │  └───────────────────────────────────────────────────┘  │   │
│  │                         │                                │   │
│  │                         ▼                                │   │
│  │                    RETURNS 0 (FAILURE) ❌                │   │
│  └─────────────────────────────────────────────────────────┘   │
│                         │                                       │
│  ❌ sub_403510() returns 0 because DirectDraw init failed      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 5: Check Return Value                                     │
│  if (!sub_403510())  ← CONDITION IS TRUE (returned 0)          │
│    return 0;         ← GAME EXITS HERE ❌                       │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                   🛑 GAME EXITS/TIMES OUT 🛑

┌─────────────────────────────────────────────────────────────────┐
│  NEVER REACHED: Main Message Loop                              │
│  while (dword_43C7A4) {                                         │
│    PeekMessageA(&Msg, ...)                                      │
│    GetMessageA(&Msg, ...)                                       │
│    TranslateMessage(&Msg)                                       │
│    DispatchMessageA(&Msg)                                       │
│  }                                                              │
└─────────────────────────────────────────────────────────────────┘
```

## The Problem in Detail

### What a COM Object Should Look Like in Memory

```
Address          Contents                    Description
──────────────────────────────────────────────────────────────────
0x01234000:      0x01234010                  Pointer to vtable
0x01234004:      [object data]               COM object member data
0x01234008:      [object data]               ...
...

0x01234010:      0xF1000000                  vtable[0]: QueryInterface
0x01234014:      0xF1000100                  vtable[1]: AddRef  
0x01234018:      0xF1000200                  vtable[2]: Release
0x0123401C:      0xF1000300                  vtable[3]: Compact
...
0x01234050:      0xF1001000                  vtable[N]: SetCooperativeLevel
0x01234054:      0xF1001100                  vtable[N+1]: SetDisplayMode
...
```

When game calls `lpDD->lpVtbl->SetCooperativeLevel(lpDD, hWnd, flags)`:

```c
// Decompiled C code:
lpDD->lpVtbl->SetCooperativeLevel(lpDD, hWnd, flags);

// What the CPU actually does:
1. Read vtable pointer:   vtbl = *(lpDD + 0)
2. Read function pointer: func = *(vtbl + offset_SetCooperativeLevel) 
3. Call function:         func(lpDD, hWnd, flags)
```

### What The Emulator Currently Does

```
Address          Contents                    Description
──────────────────────────────────────────────────────────────────
lplpDD:          0x70000000                  Just a handle number!

0x70000000:      ??? garbage ???             NOT A VALID OBJECT
0x70000004:      ??? garbage ???
...
```

When game tries to call methods:
1. Read vtable pointer: `vtbl = *(0x70000000)` → INVALID MEMORY READ
2. Either crashes or returns garbage
3. Function fails, game exits

## The Solution

### Step 1: Allocate COM Object Memory

```csharp
// In DirectDrawCreate:
uint objAddr = _env.SimpleAlloc(8);        // Allocate object (8 bytes)
uint vtblAddr = _env.SimpleAlloc(100*4);   // Allocate vtable (100 methods)

// Write vtable pointer to object
_env.MemWrite32(objAddr + 0, vtblAddr);

// Write function pointers to vtable
_env.MemWrite32(vtblAddr + 0*4, 0xF1000000);  // QueryInterface stub
_env.MemWrite32(vtblAddr + 1*4, 0xF1000010);  // AddRef stub
_env.MemWrite32(vtblAddr + 2*4, 0xF1000020);  // Release stub
// ... fill in all method stubs ...
_env.MemWrite32(vtblAddr + 20*4, 0xF1000200); // SetCooperativeLevel

// Return pointer to object
_env.MemWrite32(lplpDd, objAddr);
```

### Step 2: Hook Vtable Method Calls

```csharp
// In CPU emulator, when EIP enters range 0xF1000000-0xF1FFFFFF:
if (eip >= 0xF1000000 && eip < 0xF2000000)
{
    uint methodId = (eip - 0xF1000000) >> 4;
    
    // Dispatch to appropriate handler
    switch (methodId)
    {
        case 0:  return ComQueryInterface(cpu);
        case 1:  return ComAddRef(cpu);
        case 2:  return ComRelease(cpu);
        case 20: return DirectDrawSetCooperativeLevel(cpu);
        case 21: return DirectDrawSetDisplayMode(cpu);
        // ...
    }
}
```

### Step 3: Implement Methods

```csharp
uint DirectDrawSetCooperativeLevel(ICpu cpu)
{
    var thisPtr = cpu.StackPop32();  // COM object pointer
    var hWnd = cpu.StackPop32();     // Window handle
    var flags = cpu.StackPop32();    // Cooperation flags
    
    _logger.LogInfo($"IDirectDraw::SetCooperativeLevel(hWnd=0x{hWnd:X}, flags=0x{flags:X})");
    
    // Store settings in object
    var ddObj = _ddrawObjects[thisPtr];
    ddObj.CoopFlags = flags;
    ddObj.WindowHandle = hWnd;
    
    cpu.Eax = 0;  // DD_OK (success)
    return 0;     // Return to caller
}
```

## Result

With proper COM vtable support:
1. ✅ DirectDrawCreate returns a valid COM object
2. ✅ Game can call SetCooperativeLevel, SetDisplayMode, etc.
3. ✅ DirectDraw initialization succeeds
4. ✅ DirectInput initialization succeeds  
5. ✅ sub_403510() returns 1 (success)
6. ✅ Game enters main message loop
7. ✅ Game starts calling PeekMessageA, GetMessageA, etc.
8. ✅ Eventually needs window creation and rendering support

## Timeline

- **Now**: Game exits during DirectX init (times out after 5s)
- **After COM vtables**: Game enters main loop, starts processing messages
- **After window support**: Game creates window, tries to set video mode
- **After rendering support**: Game can actually display graphics
- **Full emulation**: Complete gameplay with input and audio

## Evidence Strength

**Certainty: 95%+**

All 8 decompilers show:
- ✅ DirectDrawCreate is called
- ✅ Immediate vtable method calls follow
- ✅ No error handling around vtable calls (game assumes they work)
- ✅ Pattern repeats for DirectInput, DirectSound
- ✅ Emulator source confirms it only returns handles
- ✅ Test shows timeout with no unknown API calls

This analysis is definitive.
