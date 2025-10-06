# IGN_TEAS.EXE Decompilation Files

## Overview

This directory contains decompilation outputs from various decompilers analyzing the IGN_TEAS.EXE file. Each decompiler has different strengths and may recover different aspects of the code more accurately.

## Files

### Decompiler Outputs

- **`hexrays.cpp`** - Hex-Rays IDA Pro decompilation
  - Generally considered the gold standard for decompilation quality
  - Best for understanding high-level program flow
  - Good function name recovery and type inference
  - Size: ~343 KB

- **`ghidra.cpp`** - NSA Ghidra decompilation  
  - Open-source decompiler with good accuracy
  - Excellent for cross-referencing with IDA output
  - Strong data flow analysis
  - Size: ~397 KB

- **`binaryninja.cpp`** - Binary Ninja decompilation
  - Modern commercial decompiler
  - Clean, readable output
  - Good intermediate representation
  - Size: ~674 KB

- **`reko.cpp`** - Reko decompiler output
  - Open-source decompiler
  - Focuses on portability and multiple architectures
  - Size: ~274 KB

- **`retdec.cpp`** - RetDec decompilation
  - Machine-learning enhanced decompilation
  - Can recover some patterns other tools miss
  - Size: ~1.06 MB

- **`snowman.cpp`** - Snowman decompiler output
  - Part of the radare2 ecosystem
  - Good for comparing against other tools
  - Size: ~1.27 MB

- **`recstudio.cpp`** - Rec Studio decompilation
  - Commercial decompiler
  - Alternative perspective on the code
  - Size: ~616 KB

- **`boomerang.cpp`** - Boomerang decompiler output
  - Research-oriented decompiler
  - Experimental but can provide insights
  - Size: ~877 KB

### Analysis Documents

- **`ANALYSIS.md`** - Comprehensive analysis of the decompilation results
  - Identifies the root cause of emulation issues
  - Documents the game's initialization sequence
  - Explains why DirectX COM interfaces are needed
  - Provides recommendations for fixing the emulator

## Key Findings

After analyzing these decompilation files, we identified that IGN_TEAS.EXE:

1. **Successfully calls** standard Win32 APIs during initialization
2. **Calls DirectX creation functions** which the emulator implements
3. **Immediately tries to use COM vtable methods** on DirectX objects
4. **Fails/hangs** because the emulator doesn't provide functional COM vtables

The critical code path is:
```
WinMain() 
  → timeBeginPeriod(1) ✅
  → sub_404B00() ✅
  → sub_403510() ❌ FAILS HERE
      → sub_404640() - DirectDraw init via function pointers
      → sub_4046F0() - DirectInput init with vtable calls
```

## How to Use These Files

### For Understanding Program Flow

1. Start with `hexrays.cpp` - it has the cleanest high-level view
2. Look for the `WinMain` function (around line 3665)
3. Follow the initialization sequence

### For Finding Specific APIs

Use grep to search across all files:
```bash
grep -n "DirectDrawCreate" *.cpp
grep -n "DirectInputCreateA" *.cpp
grep -n "timeGetTime" *.cpp
```

### For Understanding COM Method Calls

Look for patterns like:
```cpp
(*(int (__stdcall **)(int, _DWORD *, int *, _DWORD))(*(_DWORD *)ptr + offset))(args...)
```

This indicates:
- Dereferencing a pointer to get a vtable
- Calling a method at a specific vtable offset
- These are COM interface method calls

### Cross-Referencing

When in doubt about a function's behavior:
1. Check the same function in multiple decompilers
2. Compare their interpretations
3. The commonalities are likely correct

## Notable Functions

### WinMain (0x403140)
- Entry point
- Registers window class
- Initializes DirectX
- Main message loop (never reached)

### sub_403510 (0x403510)
- Critical initialization function
- Returns 0 on failure (game exits)
- Calls DirectDraw and DirectInput init

### sub_404640 (0x404640)
- DirectDraw initialization wrapper
- Uses function pointers/vtables

### sub_4046F0 (0x4046F0)
- DirectInput initialization
- Creates device via COM vtable call at offset +12
- Sets data format via vtable call at offset +44

### sub_4034D0 (0x4034D0)
- Timer function using `timeGetTime()`
- Called frequently for game timing

### sub_403340 (0x403340)
- Window procedure (WndProc)
- Handles messages like WM_DESTROY, WM_ACTIVATEAPP, etc.

## Common Patterns

### DirectX Creation Pattern
```cpp
// 1. Call creation function
if (DirectDrawCreate(0, &lpDD, 0))
    return 0;

// 2. Immediately call COM methods via vtable
lpDD->lpVtbl->SetCooperativeLevel(lpDD, hWnd, flags);
lpDD->lpVtbl->SetDisplayMode(lpDD, width, height, bpp);
```

### Function Pointer Calls
```cpp
// These appear as:
dword_43CCDC()  // Call function at address stored in dword_43CCDC
dword_43CD10()  // Another function pointer

// These are likely dynamically resolved DirectX functions
// or vtable method pointers
```

## Limitations of Decompilation

These files are **approximations** of the original source code:
- Variable names are guessed (v1, v2, iVar1, etc.)
- Some types may be incorrect
- Optimizations may obscure intent
- Inlined functions are not always obvious
- Some constructs may be artifacts of compilation

Always cross-reference with:
- Multiple decompilers (provided here)
- Win32 API documentation
- DirectX SDK documentation
- Actual emulator behavior when running the code

## Related Files

- `/EXEs/ign_teas/IGN_TEAS.EXE` - The original executable
- `/Win32Emu.Tests.Emulator/IgnitionTeaserTests.cs` - Integration test
- `/Win32Emu.Tests.Emulator/README_IGN_TEAS_TEST.md` - Test documentation
- `/Win32Emu/Win32/Modules/DDrawModule.cs` - DirectDraw emulation
- `/Win32Emu/Win32/Modules/DInputModule.cs` - DirectInput emulation

## Next Steps

See `ANALYSIS.md` for detailed recommendations on implementing COM vtable support to enable the game to progress beyond the initialization phase.

## Contributing

When analyzing the decompilation:
1. Document your findings in `ANALYSIS.md`
2. Update function comments with better names if you identify them
3. Note any discrepancies between decompilers
4. Reference Win32/DirectX documentation for API calls
