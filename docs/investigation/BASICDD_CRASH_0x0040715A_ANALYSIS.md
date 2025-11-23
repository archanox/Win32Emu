# BasicDD.exe Crash Analysis - Address 0x0040715A

## Problem Statement

BasicDD.exe was crashing at address 0x0040715A with the following characteristics:
- Crash occurred AFTER `GetAttachedSurface` returned successfully
- EBP register corrupted to code address 0x0040187C (not a valid frame pointer)
- Stack showed import stub address 0x0F000115 at ESP
- Crash address 0x0040715A is in the data section, not executable code

## Root Cause Analysis

### Memory Address Analysis
From Ghidra decompilation (Decomp/BasicDD/ghidra.cpp):
- `DAT_00407154` - Data variable at 0x00407154
- Crash address 0x0040715A is 6 bytes into this data structure
- Execution was attempting to execute data as code

### COM Vtable Ordering Issue

The crash was caused by incorrect COM vtable method ordering when using `Dictionary<string, ComMethodInfo>`:

**Problem**: C# Dictionary doesn't guarantee insertion order. When iterating over dictionary entries to populate the vtable, methods could be reordered, causing:
- Vtable entry for `Flip` (should be at offset 0x2C, index 11) could point to wrong address
- If the wrong address happened to be in the data section (e.g., 0x0040715A), attempting to call that method would execute data as code
- This explains why the crash happened "AFTER GetAttachedSurface returns successfully" - the next operation likely tried to call `Flip` on the returned surface

**Test Evidence**: `ComVtableOrderingTests.cs` line 183-185:
```csharp
// Verify Flip is not pointing to data section (0x00407154-0x0040715A range)
Assert.False(flipMethodPtr >= 0x00407150 && flipMethodPtr < 0x00407160,
    $"Flip method pointer 0x{flipMethodPtr:X8} is in data section! This would cause crash.");
```

### IDirectDrawSurface Vtable Structure

Correct vtable layout (per MSDN and COM specifications):
```
Index  Offset  Method
-----  ------  ------
0      0x00    QueryInterface (IUnknown)
1      0x04    AddRef (IUnknown)
2      0x08    Release (IUnknown)
3      0x0C    AddAttachedSurface
4      0x10    AddOverlayDirtyRect
5      0x14    Blt
6      0x18    BltBatch
7      0x1C    BltFast
8      0x20    DeleteAttachedSurface
9      0x24    EnumAttachedSurfaces
10     0x28    EnumOverlayZOrders
11     0x2C    Flip                    <-- CRITICAL: Must be at offset 0x2C
12     0x30    GetAttachedSurface     <-- Must be at offset 0x30
... (continues for all 36 methods)
```

## Fix Implementation

### Solution
Created `CreateComObjectOrdered()` method that accepts ordered method list:
```csharp
public uint CreateComObjectOrdered(string interfaceName, List<KeyValuePair<string, ComMethodInfo>> methods)
{
    return CreateComObjectInternal(
        interfaceName,
        methods,  // List preserves insertion order!
        info => info.Handler,
        info => null,
        info => info.ArgBytes,
        isAsync: false);
}
```

### DDrawModule.cs Changes
All DirectDraw surface creation now uses `List<KeyValuePair<string, ComMethodInfo>>`:
```csharp
var vtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
{
    new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>(...)),
    new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>(...)),
    new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>(...)),
    new("AddAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.AddAttachedSurface>(...)),
    // ... all methods in exact COM interface order
    new("Flip", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Flip>(...)),
    new("GetAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetAttachedSurface>(...)),
    // ... remaining methods
};

var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDrawSurface", vtableMethods);
```

## Test Coverage

### ComVtableOrderingTests.cs

1. **CreateComObjectOrdered_ShouldPreserveMethodOrder**
   - Verifies basic method ordering with 6 methods
   - Ensures each method is at correct vtable offset

2. **CreateComObjectOrdered_WithManyMethods_ShouldPreserveOrder**
   - Tests with 20 methods to verify scalability
   - Confirms sequential memory layout

3. **IDirectDrawSurface_Flip_ShouldBeAtOffset0x2C** ⭐ **KEY TEST**
   - Specifically addresses BasicDD.exe crash
   - Verifies Flip at offset 0x2C (index 11)
   - Asserts Flip does NOT point to data section (0x00407150-0x00407160)
   - Confirms Flip points to COM stub region (0x0D000000-0x0E000000)

4. **CreateComObject_WithDictionary_StillWorks**
   - Maintains backward compatibility with old Dictionary-based API
   - For code that doesn't require strict ordering

### Test Results
```
✅ All tests pass
✅ Flip method correctly at offset 0x2C
✅ Flip method pointer: 0x0D0010B0 (COM stub region)
✅ NOT in data section (0x00407150-0x00407160)
```

## Memory Regions

The emulator uses distinct memory ranges for different purposes:

```
0x00400000 - 0x00FFFFFF  PE executable (code + data sections)
  0x00407154 - 0x0040715A  ⚠️ Data section (DAT_00407154)
0x0D000000 - 0x0DFFFFFF  COM vtable stubs (16 MB)
0x0E000000 - 0x0EFFFFFF  Syscall dispatcher (16 MB)
0x0F000000 - 0x0FFFFFFF  Import hooks and stubs (16 MB)
```

## Resolution

### Status: ⚠️ PARTIALLY ADDRESSED

The vtable ordering issue has been fixed:
- `CreateComObjectOrdered()` ensures correct method ordering ✅
- All DirectDraw surface creation uses ordered lists ✅
- Comprehensive tests verify vtable layout ✅
- Flip method correctly at offset 0x2C (0x0D0010B0) ✅

**However, BasicDD.exe still crashes at 0x0040715A** ❌

### Verification Steps

1. **Vtable layout test passes:**
   ```bash
   $ dotnet test Win32Emu.Tests.Emulator --filter "IDirectDrawSurface_Flip_ShouldBeAtOffset0x2C"
   ✅ PASSED - Flip at 0x0D0010B0 (COM region), NOT 0x0040715A (data section)
   ```

2. **But runtime crash still occurs:**
   ```bash
   $ dotnet run --project Win32Emu.Gui/Win32Emu.Gui.csproj --no-build -- --nogui EXEs/BasicDD.exe
   ❌ CRASH - [IcedCpu] Unhandled mnemonic INVALID at 0x0040715A, ESP=0x001FEF70, EBP=0x0040187C
   ```

### Root Cause Re-analysis

Initial hypothesis (vtable ordering) was **incorrect**. Verified against retrowin32 source code - vtable layout is correct.

### Verification Against retrowin32

From `retrowin32/win32/dll/ddraw/src/ddraw1.rs`, the IDirectDrawSurface vtable uses identical ordering:
- Flip at index 11 (offset 0x2C) ✓
- GetAttachedSurface at index 12 (offset 0x30) ✓
- Restore at index 27 (offset 0x6C) ✓

### Actual Root Cause: Severe Stack Corruption

**Crash characteristics:**
```
EIP = 0x0040715A    (data section - contains: FF FF FF FF - invalid opcodes)
EBP = 0x0040187C    (inside FUN_00401873 - invalid frame pointer)
ESP = 0x001FEF70 -> points to 0x0F000115 (GetModuleHandleA import stub + 5 bytes!)
```

**Analysis:**
1. GetAttachedSurface returns successfully to 0x0040140C ✓
2. Registers preserved correctly (EBP=0x001FEFFC) ✓
3. Execution continues in FUN_00401310, returns to FUN_00401040 (WinMain)
4. Main loop calls FUN_00401130 which calls Flip (offset 0x2C)
5. **Stack becomes corrupted** - ESP points to middle of import stub
6. Execution jumps/falls through to data section at 0x0040715A

**Key evidence:**
- ESP=0x001FEF70 contains 0x0F000115 (5 bytes into GetModuleHandleA stub at 0x0F000110)
- Import stubs are 16 bytes: 6-byte CALL + 2-3 byte RET
- 5 bytes in = middle of CALL instruction encoding
- This is impossible as a valid return address
- Indicates stack was corrupted **before** this value was popped

**Possible causes:**
1. **CPU emulation bug** in instruction execution corrupting stack
2. **Incorrect stdcall cleanup** for some Win32 API (wrong argBytes)
3. **Missing Win32 API implementation** that BasicDD calls
4. **Unhandled calling convention** (e.g., fastcall vs stdcall confusion)
5. **Buffer overflow** in application code due to incorrect API behavior

## Related Documentation

- `Win32Emu/Win32/COM/ComVtableDispatcher.cs` - COM vtable creation
- `Win32Emu/Win32/Modules/DDrawModule.cs` - DirectDraw implementation
- `Win32Emu.Tests.Emulator/ComVtableOrderingTests.cs` - Test suite
- `Decomp/BasicDD/ghidra.cpp` - Decompiled BasicDD.exe

## Lessons Learned

1. **COM interfaces require exact method ordering** - vtable layout is defined by the COM interface specification
2. **Dictionary is not suitable for vtable creation** - insertion order is not guaranteed in C#
3. **Use List<KeyValuePair> for ordered collections** - preserves insertion order
4. **Test vtable layout explicitly** - verify methods are at correct offsets
5. **Memory region validation is critical** - detect when pointers cross into wrong regions

## Conclusion

The investigation initially hypothesized that the BasicDD.exe crash at 0x0040715A was caused by incorrect COM vtable method ordering. While `CreateComObjectOrdered()` was implemented and tests confirm correct vtable layout, **the crash still occurs at runtime**.

This indicates the root cause is **not** vtable ordering but likely:
- Stack corruption or return address manipulation in BasicDD.exe code
- Missing or incorrect Win32 API implementation
- Indirect call through corrupted function pointer

## Debugging Strategy

### Tools Assessment

The **interactive debugger** (`--interactive-debug`) is sufficient for diagnosis. Provides:
- Breakpoint capabilities at specific addresses
- Single-step execution through instructions  
- Register and memory inspection
- Call stack examination

### Immediate Steps

1. **Set strategic breakpoints:**
   ```
   break 0x0040140C  // After GetAttachedSurface returns
   break 0x00401640  // FUN_00401640 (calls SetColorKey)
   break 0x004014d0  // FUN_004014d0 (calls LoadImageA, Restore)
   break 0x00401130  // FUN_00401130 (main loop, calls Flip)
   continue
   ```

2. **At each breakpoint:**
   - Examine ESP and EBP values
   - Single-step through function
   - Monitor stack changes after each CALL/RET
   - Verify stdcall cleanup (ESP += 4 + argBytes)

3. **Trace function calls:**
   - Log ESP at function entry/exit
   - Identify which call causes ESP/EBP corruption
   - Check argBytes calculation for that function

### Investigation Areas

1. **FUN_004014d0 analysis (HIGH PRIORITY)**
   - Calls LoadImageA(param_1, 0x65, 0, 0x5dc, 0x118, 0)
   - Line 272: Calls Restore at offset 0x6C (suspicious decompilation)
   - Calls GetSurfaceDesc, GetDC, ReleaseDC
   - Check if argBytes correct for all COM methods
   - **Note:** EAX=0x65 at crash (same as LoadImageA resource ID param)

2. **FUN_00401130 execution flow**
   - Main loop calls Flip (offset 0x2C) repeatedly
   - Checks for DDERR_SURFACELOST (-0x7789fe3e)
   - May call Restore (offset 0x6C) if surface lost
   - Monitor ESP through loop iterations

3. **Execution timeline:**
   - GetAttachedSurface returns: ESP=0x001FEEC0, EBP=0x001FEFFC ✓
   - Crash at 0x0040715A: ESP=0x001FEF70, EBP=0x0040187C ❌
   - Delta: ESP moved 176 bytes (0xB0) - multiple calls occurred
   - Need to trace execution between these points

4. **Import stub 0x0F000115**
   - GetModuleHandleA stub starts at 0x0F000110
   - 0x0F000115 is 5 bytes in (middle of CALL instruction)
   - Impossible as valid return address
   - Indicates stack was corrupted before crash

### Long-term Solutions

1. Add stack integrity validation
2. Implement stack canaries for debugging
3. Enhanced logging for all stack operations
4. Automated detection of invalid return addresses

The vtable ordering fix was necessary and correct, but the actual crash stems from stack corruption elsewhere in the execution flow.
