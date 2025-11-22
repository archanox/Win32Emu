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

Initial hypothesis (vtable ordering) was **incorrect**. The crash persists despite correct vtable layout, indicating:

**Possible actual causes:**
1. **Return address corruption** - EIP reaches 0x0040715A through corrupted return address on stack
2. **Bad indirect call** - Code pointer loaded from memory points to data section
3. **Missing Win32 API** - Unimplemented function returns invalid address
4. **Stack corruption** - Buffer overflow or incorrect stack cleanup elsewhere

**Evidence from debug run:**
- GetAttachedSurface returns successfully at 0x0040140C
- All registers preserved correctly (EBP=0x001FEFFC before crash)
- Crash occurs later at 0x0040715A with EBP=0x0040187C (different!)
- Stack at ESP shows import stub 0x0F000115

This suggests the crash occurs during subsequent execution in BasicDD.exe code, not during COM vtable calls.

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

## Next Steps for Further Investigation

1. **Disassemble BasicDD.exe around 0x0040140C** - Understand what code executes after GetAttachedSurface returns
2. **Trace execution path** - Use GDB server (`--gdb-server`) to step through and find where EIP diverges
3. **Analyze EBP corruption** - Determine why/how EBP changes from 0x001FEFFC to 0x0040187C
4. **Examine import stub 0x0F000115** - Identify which API it represents and why it's on stack at crash
5. **Review unimplemented APIs** - Check if BasicDD.exe calls functions that aren't properly implemented

The vtable ordering fix was necessary and correct, but insufficient to resolve the actual crash.
