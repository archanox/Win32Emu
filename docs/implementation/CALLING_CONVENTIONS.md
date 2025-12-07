# x86 Calling Conventions Implementation

This document describes the x86 calling conventions implemented in Win32Emu, based on the specification from [Wikipedia - x86 calling conventions](https://en.wikipedia.org/wiki/X86_calling_conventions).

## Overview

Win32Emu implements five x86 calling conventions:

1. **stdcall** - Standard Win32 API convention
2. **cdecl** - C declaration convention
3. **fastcall** - Performance-optimized convention
4. **thiscall** - C++ member function convention
5. **Pascal** - Win16/Pascal convention

## Calling Convention Details

### Stdcall (Win32 Standard)

**Specification:**
- Arguments pushed **right-to-left** on stack
- **Callee cleans** the stack using `RET N` instruction (0xC2 opcode)
- Return value in **EAX/AX/AL** register
- Used by most Win32 APIs

**Stack cleanup:** `RET N` where N = total argument bytes

**Example:**
```c
uint32_t MessageBoxA(HWND hwnd, LPCSTR text, LPCSTR caption, UINT type);
```
Call with 4 parameters (16 bytes total):
```asm
push type       ; 4 bytes
push caption    ; 4 bytes  
push text       ; 4 bytes
push hwnd       ; 4 bytes
call MessageBoxA
; Function returns with RET 16, cleaning 16 bytes from stack
```

### Cdecl (C Declaration)

**Specification:**
- Arguments pushed **right-to-left** on stack
- **Caller cleans** the stack by adjusting ESP after `RET`
- Return value in **EAX/AX/AL** register
- Used by variadic functions (printf, sprintf, etc.)

**Stack cleanup:** Caller adds ESP after function returns

**Example:**
```c
int printf(const char *format, ...);
```
Call with variable arguments:
```asm
push arg2       ; Variable argument
push arg1       ; Variable argument
push format     ; Format string
call printf
add esp, 12     ; Caller cleans up stack
```

### Fastcall

**Specification:**
- **First two arguments** in **ECX** and **EDX** registers
- Remaining arguments pushed **right-to-left** on stack
- **Callee cleans** the stack using `RET N` instruction
- Return value in **EAX/AX/AL** register
- Used for performance-critical APIs

**Stack cleanup:** `RET N` where N = stack argument bytes only (excludes register arguments)

**Example:**
```c
__fastcall uint32_t FastFunction(uint32_t a, uint32_t b, uint32_t c);
```
Call with 3 parameters:
```asm
mov ecx, a      ; First param in ECX
mov edx, b      ; Second param in EDX
push c          ; Third param on stack (4 bytes)
call FastFunction
; Function returns with RET 4, cleaning only stack arguments
```

### Thiscall (C++ Member Functions)

**Specification:**
- **'this' pointer** in **ECX** register
- Remaining arguments pushed **right-to-left** on stack
- **Callee cleans** the stack using `RET N` instruction
- Return value in **EAX/AX/AL** register
- Used by C++ non-static member functions

**Stack cleanup:** `RET N` where N = stack argument bytes only (excludes 'this' in ECX)

**Example:**
```cpp
class MyClass {
    uint32_t MyMethod(uint32_t param1, uint32_t param2);
};
```
Call:
```asm
mov ecx, pThis  ; 'this' pointer in ECX
push param2     ; 4 bytes
push param1     ; 4 bytes
call MyMethod
; Function returns with RET 8, cleaning only stack arguments
```

### Pascal (Win16/Pascal Convention)

**Specification:**
- Arguments pushed **left-to-right** on stack (OPPOSITE of stdcall!)
- **Callee cleans** the stack using `RET N` instruction
- Return value in **AL/AX/EAX** register
- Used by Win16 applications and Pascal compilers

**Stack cleanup:** `RET N` where N = total argument bytes

**Key Difference from Stdcall:** The argument push order is **reversed**!

**Example:**
```pascal
function MessageBox(hwnd: HWND; text, caption: PChar; uType: UINT): Integer; pascal;
```
Call with 4 parameters (16 bytes total):
```asm
push hwnd       ; FIRST argument pushed first (left-to-right)
push text       ; Second argument
push caption    ; Third argument
push type       ; LAST argument pushed last
call MessageBox
; Function returns with RET 16, cleaning 16 bytes from stack
```

## Implementation Notes

### Stack Cleanup

The emulator correctly implements stack cleanup for all conventions:

1. **Stdcall, Fastcall, Thiscall, Pascal:** Callee uses `RET N` (opcode 0xC2) to pop N bytes from stack
2. **Cdecl:** Caller adjusts ESP after `RET` (opcode 0xC3)

The difference in argument order between stdcall and Pascal does NOT affect stack cleanup - both use `RET N` with the same byte count. The order only matters when marshalling arguments from the stack.

### Export Name Decoration

Win32 DLLs use name decoration to encode calling conventions:

- **Stdcall:** `FunctionName@N` (e.g., `MessageBoxA@16`)
- **Fastcall:** `@FunctionName@N` (e.g., `@FastFunc@8`)
- **Thiscall:** `?MethodName@@...` (C++ name mangling)
- **Cdecl:** `_FunctionName` or undecorated
- **Pascal:** Usually undecorated in Win16 exports

### Parameter Marshalling

When reading parameters from the stack:

- **Stdcall/Cdecl/Fastcall/Thiscall:** Read parameters in declaration order (index 0, 1, 2...)
- **Pascal:** Read parameters in REVERSE order due to left-to-right push

#### StackArgs (Stdcall/Cdecl)

For Win32 functions using stdcall or cdecl:

```csharp
var args = new StackArgs(cpu, memory);
var param1 = args.UInt32(0);  // First parameter at ESP+4
var param2 = args.UInt32(1);  // Second parameter at ESP+8
```

#### PascalStackArgs (Win16)

For Win16 functions using Pascal calling convention:

```csharp
// Must provide parameter count for correct offset calculation
var args = new PascalStackArgs(cpu, memory, paramCount: 3);
var param1 = args.UInt32(0);  // First parameter at ESP+(3*4)=ESP+12
var param2 = args.UInt32(1);  // Second parameter at ESP+(2*4)=ESP+8
var param3 = args.UInt32(2);  // Third parameter at ESP+(1*4)=ESP+4
```

The `PascalStackArgs` wrapper automatically reverses the index mapping, so you can still use logical parameter indices (0, 1, 2...) and it will read from the correct stack offsets.

**Win16 Thunking Example:**

```csharp
// In Win16 module forwarding to Win32 implementation
public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
{
    switch (export.ToUpperInvariant())
    {
        case "GLOBALALLOC":
            // Win16 GlobalAlloc has 2 parameters
            // Use PascalStackArgs to handle reversed parameter order
            var args = new PascalStackArgs(cpu, memory, paramCount: 2);
            
            // Now can forward to Win32 which will read correct parameter order
            return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);
    }
}
```

## Testing

Comprehensive tests validate calling convention implementations:

### CallingConventionTests.cs

- Enum value existence checks
- Documentation validation
- Export name decoration parsing
- Convention difference verification

### PascalStackArgsTests.cs

- Parameter order reversal verification
- Comparison with stdcall StackArgs behavior
- Support for different parameter counts
- All helper method functionality

All tests pass and validate conformance to the x86 calling convention specification.

## References

- [Wikipedia - x86 calling conventions](https://en.wikipedia.org/wiki/X86_calling_conventions)
- [Microsoft - Argument Passing and Naming Conventions](https://docs.microsoft.com/en-us/cpp/cpp/argument-passing-and-naming-conventions)
- `Win32Emu/Loader/ExportMetadata.cs` - Convention enum and metadata
- `Win32Emu.CallingConvention/Win32CallingConvention.cs` - Convention enum for code generation
- `Win32Emu.CallingConvention/MarshallingCodeGenerator.cs` - Parameter marshalling logic
- `Win32Emu/Win32/StackArgs.cs` - Stdcall parameter reading
- `Win32Emu/Win32/PascalStackArgs.cs` - Pascal parameter reading (Win16)
