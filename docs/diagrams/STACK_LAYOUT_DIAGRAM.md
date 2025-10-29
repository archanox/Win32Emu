# Stack Layout Comparison: Before and After Fix

## Before Fix (INCORRECT)

```
Higher Memory Addresses
┌─────────────────────┐
│     ...             │
├─────────────────────┤
│  0xDEADBEEF         │ ESP+16  <- Return address at wrong position
├─────────────────────┤
│  0x00000000         │ ESP+12  <- lParam
├─────────────────────┤
│  0x00000000         │ ESP+8   <- wParam
├─────────────────────┤
│  0x0000000F         │ ESP+4   <- message (WM_PAINT)
├─────────────────────┤
│  0x00010000         │ ESP+0   <- hwnd (ESP points here)
└─────────────────────┘
Lower Memory Addresses
```

When WndProc calls DefWindowProcA:
1. WndProc pushes parameters for DefWindowProcA
2. CALL instruction pushes return address
3. StackArgs reads from ESP+4, which gets 0x0000000F (wrong!)
4. Parameters are all shifted by 4 bytes

**Result**: DefWindowProcA receives:
- HWND = 0x0000000F (should be 0x00010000) ❌
- MSG = 0x00000000 (should be 0x0000000F) ❌
- wParam = 0x00000000 ✓
- lParam = 0xDEADBEEF (should be 0x00000000) ❌

## After Fix (CORRECT)

```
Higher Memory Addresses
┌─────────────────────┐
│     ...             │
├─────────────────────┤
│  0x00000000         │ ESP+16  <- lParam
├─────────────────────┤
│  0x00000000         │ ESP+12  <- wParam
├─────────────────────┤
│  0x0000000F         │ ESP+8   <- message (WM_PAINT)
├─────────────────────┤
│  0x00010000         │ ESP+4   <- hwnd
├─────────────────────┤
│  0xDEADBEEF         │ ESP+0   <- Return address (ESP points here)
└─────────────────────┘
Lower Memory Addresses
```

When WndProc calls DefWindowProcA:
1. WndProc pushes parameters for DefWindowProcA
2. CALL instruction pushes return address
3. StackArgs reads from ESP+4, which gets the correct hwnd value
4. All parameters are at the correct offsets

**Result**: DefWindowProcA receives:
- HWND = 0x00010000 ✓
- MSG = 0x0000000F ✓
- wParam = 0x00000000 ✓
- lParam = 0x00000000 ✓

## The x86 stdcall Convention

In stdcall, the stack must be set up as if these instructions were executed:

```asm
; Caller prepares parameters
push lParam        ; Last parameter first
push wParam
push message
push hwnd          ; First parameter last
call WndProc       ; CALL pushes return address onto stack

; After CALL, stack layout:
; [ESP+0]  = Return address
; [ESP+4]  = hwnd (first parameter)
; [ESP+8]  = message (second parameter)
; [ESP+12] = wParam (third parameter)
; [ESP+16] = lParam (fourth parameter)
```

The `CallWindowProcedure` method must simulate this exact stack layout manually since it's not using actual assembly PUSH/CALL instructions. The fix ensures the return address is placed on top of the stack (lowest address) after all parameters have been pushed.
