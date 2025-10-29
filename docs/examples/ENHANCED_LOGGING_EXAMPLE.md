# Enhanced Logging Example

This document shows example output with the enhanced INT/INT3 hooking logging.

## Example: Running CHKCPU32.exe

When you run an executable with the emulator, you'll now see detailed logging about function hooking:

### Console Output with Enhanced Logging

```
[IcedCpu] INT3 (0xCC) hooking import stub at address 0x0F000010
[Import] Hooked function: KERNEL32.DLL!GetModuleFileNameA at address 0x0F000010
[Dispatcher] Dispatching KERNEL32.DLL!GetModuleFileNameA at EIP=0x0F000010 ESP=0x0012FF40 stack=...
[Kernel32] GetModuleFileNameA called: h=0x00400000 lp=0x0012FE00 n=260
[Kernel32] GetModuleFileNameA: Returning path: C:\test\program.exe
[Dispatcher] KERNEL32.DLL!GetModuleFileNameA returned 0x00000014, argBytes=12
[Emulator] Before stack adjustment: ESP=0x0012FF40 EBP=0x0012FF88 RetAddr=0x00401234 ArgBytes=12
[Emulator] After stack adjustment: ESP=0x0012FF50 EBP=0x0012FF88 NewEIP=0x00401234
[Emulator] GetModuleFileNameA complete - execution continuing at 0x00401234
```

### What This Shows

1. **INT3 Detected**: `[IcedCpu] INT3 (0xCC) hooking import stub at address 0x0F000010`
   - The CPU encountered an INT3 breakpoint at synthetic address `0x0F000010`
   
2. **Function Identified**: `[Import] Hooked function: KERNEL32.DLL!GetModuleFileNameA at address 0x0F000010`
   - The emulator looked up the synthetic address in the import map
   - Identified it as `KERNEL32.DLL!GetModuleFileNameA`
   
3. **Dispatcher Called**: The dispatcher invokes the emulated function
   - Shows the current register state (EIP, ESP, stack contents)
   
4. **Function Execution**: The actual function implementation logs its parameters and return value
   
5. **Stack Cleanup**: Shows how the stack is adjusted after the function returns

## Example: COM Vtable Method Call

For COM objects (like DirectDraw), you'll see:

```
[IcedCpu] INT3 (0xCC) hooking COM vtable stub at address 0x0D001020
[COM] Vtable method call at address 0x0D001020
[COM] Invoking vtable method: IDirectDraw::SetCooperativeLevel at address 0x0D001020
[DirectDraw] SetCooperativeLevel called with hwnd=0x00000000, flags=0x00000008
[COM] Method returned 0x00000000
```

### What This Shows

1. **INT3 at COM Address**: `[IcedCpu] INT3 (0xCC) hooking COM vtable stub at address 0x0D001020`
   - Detected INT3 in the COM vtable range (`0x0D000000-0x0DFFFFFF`)
   
2. **Vtable Lookup**: `[COM] Vtable method call at address 0x0D001020`
   - Identified as a COM vtable method call
   
3. **Method Name**: `[COM] Invoking vtable method: IDirectDraw::SetCooperativeLevel`
   - Shows the interface and method name
   - Shows the exact vtable address
   
4. **Method Execution**: The COM method implementation logs its work

## Call Patterns Handled

Both of these call patterns result in the same INT3 interception:

### Pattern 1: Indirect Call Through IAT
```assembly
; Ghidra decompilation might show:
CALL       dword ptr [->KERNEL32.DLL::GetModuleFileNameA]

; What actually happens:
MOV  EAX, [0x00403000]    ; Read IAT entry, gets 0x0F000010
CALL EAX                   ; Call synthetic address
; CPU executes INT3 at 0x0F000010
```

Logs:
```
[IcedCpu] INT3 (0xCC) hooking import stub at address 0x0F000010
[Import] Hooked function: KERNEL32.DLL!GetModuleFileNameA at address 0x0F000010
```

### Pattern 2: Register Call
```assembly
; Ghidra decompilation might show:
CALL       EBP=>KERNEL32.DLL::GetModuleFileNameA

; What actually happens:
MOV  EBP, [0x00403000]    ; Load function pointer, gets 0x0F000010
CALL EBP                   ; Call through register
; CPU executes INT3 at 0x0F000010
```

Logs:
```
[IcedCpu] INT3 (0xCC) hooking import stub at address 0x0F000010
[Import] Hooked function: KERNEL32.DLL!GetModuleFileNameA at address 0x0F000010
```

**The logging is identical because the CPU ends up at the same synthetic address!**

## Benefits of Enhanced Logging

1. **Debugging**: Clearly see which functions are being hooked and when
2. **Understanding**: See the vtable addresses for COM methods
3. **Verification**: Confirm that function hooking is working correctly
4. **Call Pattern Transparency**: Both direct and indirect calls show the same logging

## Enabling the Logging

The logging uses standard .NET logging infrastructure. To see all the logs:

- **Information level**: Shows all import/COM calls
- **Debug level**: Shows additional diagnostic information
- **Warning level**: Shows only problems or unexpected situations

Example:
```bash
dotnet run --project Win32Emu -- --exe path/to/program.exe --log-level Information
```

## Summary

With these enhancements, you can now:
- ✅ See exactly which function is being hooked when INT3 is encountered
- ✅ See the vtable address for COM method calls
- ✅ Understand that both call patterns (`CALL [mem]` and `CALL reg`) work the same way
- ✅ Debug function hooking issues more easily
