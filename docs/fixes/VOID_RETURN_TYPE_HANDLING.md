# Void Return Type Handling in argBytes Calculation

## Problem Statement

**Question**: With our win32/com function argument byte calculations, are we factoring functions that don't return uint32, but rather are void?

**Answer**: Yes, we are correctly handling void-returning functions. The return type does NOT affect argBytes calculation.

## Background: stdcall Calling Convention

In the x86 stdcall calling convention:

1. **Parameters** are pushed onto the stack by the caller (right-to-left)
2. **Return values** are passed via the EAX register (or EAX:EDX for 64-bit values)
3. **Stack cleanup** is performed by the callee using `RET N` instruction
4. The `N` in `RET N` represents the number of **parameter bytes** to pop from the stack

### Key Point: Return Type is Irrelevant for Stack Cleanup

Because return values are passed via registers (EAX), not the stack:
- A function returning `void` has the same stack cleanup as one returning `uint32`
- A function returning `HRESULT` (int) has the same stack cleanup as one returning `void`
- Only the **parameters** determine the `argBytes` value

## Implementation

### StdCallArgBytesGenerator.cs

The `GetParamSize` method only examines **parameters**, not return types:

```csharp
/// <summary>
/// Calculate the size in bytes of a parameter on the x86 stack.
/// 
/// NOTE: This method ONLY calculates parameter sizes, NOT return values.
/// In stdcall calling convention:
/// - Return values are passed via EAX register (not on stack)
/// - argBytes is used for stack cleanup (RET N instruction)
/// - RET N pops N bytes of PARAMETERS from the stack
/// - Return type (void, uint32, etc.) does NOT affect argBytes
/// 
/// This applies to both Win32 API functions and COM interface methods.
/// Whether a function returns void or uint32, only parameters matter for stack cleanup.
/// </summary>
private static int GetParamSize(ITypeSymbol t)
{
    // Implementation only considers parameter types
}
```

The generator calculates argBytes using:
```csharp
var argBytes = sym.Parameters.Sum(p => GetParamSize(p.Type));
```

Note: `sym.Parameters` includes only parameters, not the return type.

### ComDelegateHelper.cs

The `GetArgBytes` method for COM interfaces follows the same principle:

```csharp
/// <summary>
/// Calculate the number of bytes of arguments for a delegate type.
/// 
/// IMPORTANT: This method ONLY calculates parameter sizes, NOT return values.
/// In stdcall calling convention:
/// - Return values are passed via EAX register (or EAX:EDX for 64-bit)
/// - argBytes is used for stack cleanup (RET N instruction)
/// - RET N pops N bytes of PARAMETERS from the stack
/// - Return type (void, int, HRESULT, etc.) does NOT affect argBytes
/// </summary>
public static int GetArgBytes(Type delegateType)
{
    // Implementation iterates over parameters only
    foreach (var param in invokeMethod.GetParameters())
    {
        // Calculate parameter sizes
    }
}
```

## Test Coverage

### New Tests Added

Three new tests validate void return type handling:

1. **GetArgBytes_VoidReturnType_ShouldOnlyCountParameters**
   - Tests void functions with 0, 1, 2, and 5 parameters
   - Validates that argBytes = parameter count × 4 bytes

2. **GetArgBytes_VoidVsIntReturnType_ShouldBeIdentical**
   - Compares void and int delegates with same parameters
   - Proves that return type does NOT affect argBytes

3. **GetArgBytes_VoidWithTwoParams_MatchesIntWithTwoParams**
   - Additional validation with 2-parameter functions

### Test Delegates

```csharp
// Void return type delegates
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidNoParamsDelegate();  // argBytes = 0

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidOneParamDelegate(IntPtr pThis);  // argBytes = 4

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidTwoParamsDelegate(IntPtr pThis, uint dwFlags);  // argBytes = 8

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidFiveParamsDelegate(IntPtr pThis, uint param1, IntPtr param2, uint param3, IntPtr param4);  // argBytes = 20
```

### Test Results

All tests pass, confirming:
- ✅ Void functions calculate argBytes correctly
- ✅ Void and int functions with same parameters have identical argBytes
- ✅ Return type has zero impact on stack cleanup calculations

## Examples

### Example 1: No Parameters

```csharp
void DoSomething();         // argBytes = 0
int GetSomething();         // argBytes = 0
```

Both have `argBytes = 0` because neither has parameters.

### Example 2: Three Parameters

```csharp
void SetData(IntPtr handle, uint flags, IntPtr data);      // argBytes = 12
HRESULT Initialize(IntPtr handle, uint flags, IntPtr data); // argBytes = 12
```

Both have `argBytes = 12` (3 parameters × 4 bytes = 12 bytes).

### Example 3: COM Interface Method

```csharp
// IDirectInput::Release - returns ULONG but has only 'this' pointer
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate uint Release(IntPtr pThis);  // argBytes = 4

// Hypothetical void version would have same argBytes
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void ReleaseVoid(IntPtr pThis);  // argBytes = 4
```

## Assembly Perspective

When the emulator generates a stdcall function call:

```assembly
; Before call
push dword [param3]    ; Push parameter 3
push dword [param2]    ; Push parameter 2
push dword [param1]    ; Push parameter 1
call 0x12345678        ; Call function

; Inside function (at end)
mov eax, return_value  ; Set return value (or skip this for void)
ret 12                 ; Pop 12 bytes (3 params × 4 bytes)
                       ; ^ This 12 is the argBytes value

; After return
; Stack is now clean (caller's responsibility in cdecl, callee's in stdcall)
; EAX contains return value (if not void)
```

The `ret 12` instruction pops the parameters off the stack. Whether the function returns void or a value, the `12` stays the same because it represents parameter bytes.

## Conclusion

The implementation correctly handles void-returning functions by:

1. **Only considering parameters** when calculating argBytes
2. **Ignoring return types** entirely in the calculation
3. **Following stdcall convention** where return values use registers

This is the correct behavior for x86 stdcall calling convention and is now explicitly documented and tested.

## Related Files

- `Win32Emu.Generators/StdCallArgBytesGenerator.cs` - Generates argBytes metadata for Win32 API functions
- `Win32Emu/Win32/COM/ComInterfaceAttribute.cs` - Contains ComDelegateHelper for COM interface argBytes
- `Win32Emu.Tests.Emulator/ComDelegateHelperTests.cs` - Test coverage for void return types
- `Win32Emu/Win32/COM/ComVtableDispatcher.cs` - Uses argBytes for COM method dispatch
- `Win32Emu/Win32/Win32Dispatcher.cs` - Uses argBytes for Win32 API dispatch

## References

- [x86 Calling Conventions (MSDN)](https://docs.microsoft.com/en-us/cpp/cpp/argument-passing-and-naming-conventions)
- [stdcall Calling Convention](https://en.wikipedia.org/wiki/X86_calling_conventions#stdcall)
- [RET instruction (Intel Manual)](https://www.felixcloutier.com/x86/ret)
