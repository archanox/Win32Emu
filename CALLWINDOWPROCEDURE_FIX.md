# Fix for CallWindowProcedure Stack Parameter Order

## Problem

The issue titled "Kaboom. Where did the DirectX calls go?" was caused by incorrect parameter passing when calling window procedures from within `CallWindowProcedure`.

### Symptoms

When a window procedure (WndProc) called `DefWindowProcA` (or other import functions), the parameters were offset by 4 bytes:
- HWND parameter received the MESSAGE value (0x0000000F instead of 0x00010000)
- MESSAGE parameter received 0 instead of the actual message
- This caused crashes and undefined behavior

### Root Cause

The `CallWindowProcedure` method in `User32Module.cs` was setting up the stack incorrectly. It was pushing the return address BEFORE the parameters:

```csharp
// INCORRECT ORDER (before fix):
esp -= 4; memory.Write32(esp, RETURN_ADDRESS);  // Return address first
esp -= 4; memory.Write32(esp, lParam);
esp -= 4; memory.Write32(esp, wParam);
esp -= 4; memory.Write32(esp, message);
esp -= 4; memory.Write32(esp, hwnd);
cpu.SetRegister("ESP", esp);  // ESP points to hwnd
```

This resulted in the stack layout:
```
ESP+0:  hwnd
ESP+4:  message
ESP+8:  wParam
ESP+12: lParam
ESP+16: RETURN_ADDRESS
```

### The x86 stdcall Calling Convention

In the x86 stdcall calling convention, when you call a function:

1. Parameters are pushed onto the stack right-to-left (last parameter first)
2. The CALL instruction pushes the return address
3. ESP then points to the return address
4. The function accesses parameters at [ESP+4], [ESP+8], etc.

Example assembly:
```asm
push lParam    ; Last parameter
push wParam
push message
push hwnd      ; First parameter
call WndProc   ; CALL pushes return address
; Now ESP points to return address
; Parameters are at [ESP+4], [ESP+8], [ESP+12], [ESP+16]
```

## Solution

The fix reorders the stack setup to push parameters first, then the return address last:

```csharp
// CORRECT ORDER (after fix):
esp -= 4; memory.Write32(esp, lParam);
esp -= 4; memory.Write32(esp, wParam);
esp -= 4; memory.Write32(esp, message);
esp -= 4; memory.Write32(esp, hwnd);
esp -= 4; memory.Write32(esp, RETURN_ADDRESS);  // Return address last
cpu.SetRegister("ESP", esp);  // ESP points to return address
```

This results in the correct stack layout:
```
ESP+0:  RETURN_ADDRESS  <- ESP points here
ESP+4:  hwnd
ESP+8:  message
ESP+12: wParam
ESP+16: lParam
```

Now when the WndProc code calls an import function like `DefWindowProcA`:
1. It pushes the parameters for DefWindowProcA onto the stack
2. The CALL instruction pushes a new return address
3. ESP points to the new return address
4. The `StackArgs` class correctly reads parameters from ESP+4, ESP+8, etc.

## Changes Made

- **File**: `Win32Emu/Win32/Modules/User32Module.cs`
- **Method**: `CallWindowProcedure`
- **Lines**: 771-794
- **Change**: Reordered stack setup to push return address after parameters

## Testing

- CodeQL security scan: ✓ No issues found
- Build: ✓ Successful
- Unit tests: ✓ Passing (with one pre-existing unrelated failure)

## Impact

This fix resolves crashes when window procedures call Win32 API functions (like DefWindowProcA), enabling proper DirectX and other API call handling within window message processing.
