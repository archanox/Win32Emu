# Fix for ddraw.exe Crash Issue

## Problem Description

When running `ddraw.exe`, the emulator crashed with the following symptoms:

1. **Warning**: "No arg bytes metadata for DDRAW.DLL!DirectDrawCreate, using 0"
2. **Garbled Output**: WriteFile output showed "veLevel\nawCreate" instead of proper text
3. **Crash**: "Calculated memory address out of range: 0xFFFFFFFB (EIP=0x001FFB4A)"

### Error Log Analysis

```
warn: Win32Emu.Emulator[0]
      No arg bytes metadata for DDRAW.DLL!DirectDrawCreate, using 0
info: Win32Emu.Emulator[0]
      [ProcessEnv] StdOutput: veLevel
      awCreate
fail: Win32Emu.Emulator[0]
      Calculated memory address out of range: 0xFFFFFFFB (EIP=0x001FFB4A)
```

## Root Cause

The issue was caused by missing `[DllModuleExport]` attributes on the `DirectDrawCreate` and `DirectDrawCreateEx` methods in `DDrawModule.cs`.

### Technical Details

1. **Missing Metadata**: Without the `[DllModuleExport]` attribute, the source generator didn't create arg bytes metadata for these functions
2. **Stack Corruption**: The Win32 stdcall calling convention requires the callee to clean up the stack by popping argument bytes
3. **Wrong Stack Cleanup**: Without metadata, the emulator used 0 bytes instead of the correct values:
   - `DirectDrawCreate(uint, uint, uint)` = 12 bytes (3 × 4-byte uint parameters)
   - `DirectDrawCreateEx(uint, uint, uint, uint)` = 16 bytes (4 × 4-byte uint parameters)

### Impact Chain

```
DirectDrawCreate called with 3 args (12 bytes on stack)
  ↓
Function returns, emulator pops 4 bytes (return address) + 0 bytes (args) = 4 bytes
  ↓
Stack still has 12 bytes of junk (the original arguments)
  ↓
Next function call reads parameters from wrong locations
  ↓
Buffer pointer is wrong → garbled output
  ↓
Function returns, pops garbage as return address
  ↓
Execution jumps to 0x001FFB4A (on the stack) → CRASH
```

## Solution

Added `[DllModuleExport]` attributes to both DirectDraw creation functions:

```csharp
[DllModuleExport(9)]
private unsafe uint DirectDrawCreate(in uint lpGuid, uint lplpDd, in uint pUnkOuter)

[DllModuleExport(11)]
private unsafe uint DirectDrawCreateEx(uint lpGuid, uint lplpDd, uint iid, uint pUnkOuter)
```

The ordinal numbers (9 and 11) match the DirectDraw DLL export table defined in `GetExportOrdinals()`.

### How It Works

1. **Source Generation**: The `StdCallArgBytesGenerator` scans all methods with `[DllModuleExport]` attributes
2. **Metadata Creation**: It calculates arg bytes based on parameter types (each uint = 4 bytes)
3. **Runtime Lookup**: When invoking DLL functions, the emulator calls `StdCallMeta.GetArgBytes(dll, export)`
4. **Stack Cleanup**: After the function returns, the emulator pops the return address + arg bytes

## Verification

Created unit tests in `DDrawStdCallMetaTests.cs`:

```csharp
[Fact]
public void DirectDrawCreate_ShouldHaveCorrectArgBytes()
{
    var argBytes = StdCallMeta.GetArgBytes("DDRAW.DLL", "DirectDrawCreate");
    Assert.Equal(12, argBytes);
}

[Fact]
public void DirectDrawCreateEx_ShouldHaveCorrectArgBytes()
{
    var argBytes = StdCallMeta.GetArgBytes("DDRAW.DLL", "DirectDrawCreateEx");
    Assert.Equal(16, argBytes);
}
```

Both tests pass, confirming the metadata is correctly generated.

## Expected Outcome

With this fix:

1. ✅ DirectDrawCreate will clean up 12 bytes from the stack after returning
2. ✅ DirectDrawCreateEx will clean up 16 bytes from the stack after returning
3. ✅ Stack alignment will be maintained across function calls
4. ✅ No more garbled output from WriteFile
5. ✅ No more crashes due to jumping to invalid addresses

## Related Files

- `Win32Emu/Win32/Modules/DDrawModule.cs` - Added `[DllModuleExport]` attributes
- `Win32Emu.Tests.Emulator/DDrawStdCallMetaTests.cs` - Unit tests for verification
- `Win32Emu.Generators/StdCallArgBytesGenerator.cs` - Source generator that processes the attributes

## Testing Recommendations

To fully verify this fix:

1. ✅ Unit tests pass (confirmed)
2. ⏳ Run `ddraw.exe` to verify it no longer crashes
3. ⏳ Check that WriteFile output is correct
4. ⏳ Verify DirectDraw COM methods can be called successfully

Note: Full integration testing requires the `ddraw.exe` test binary from the retrowin32 submodule.
