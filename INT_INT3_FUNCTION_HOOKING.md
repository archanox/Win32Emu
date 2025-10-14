# INT/INT3 Function Hooking Mechanism

## Overview

This document explains how Win32Emu hooks function calls using INT and INT3 instructions, and how to interpret the logging output.

## How Function Hooking Works

Win32Emu uses a technique called "synthetic stubs" to intercept function calls to Windows DLL functions. When a PE executable is loaded, the Import Address Table (IAT) is modified to point to special synthetic addresses instead of the actual DLL functions.

### Address Ranges

The emulator uses specific address ranges to identify different types of hooked calls:

- **`0x0F000000 - 0x0FFFFFFF`**: Import stubs (Windows DLL functions)
- **`0x0D000000 - 0x0DFFFFFF`**: COM vtable method stubs

### Import Stub Creation (PeImageLoader.cs)

When a PE executable is loaded:

1. The PE loader parses the Import Address Table (IAT) to find all imported functions
2. For each import, a synthetic address is generated in the `0x0F000000` range
3. The IAT entry is rewritten to point to this synthetic address
4. An INT3 (0xCC) stub is created at the synthetic address:
   ```
   0xCC        ; INT3 - breakpoint instruction
   0x90 ...    ; NOP padding
   ```
5. The import map stores the mapping: `synthetic_address -> (dll_name, function_name)`

Example from `PeImageLoader.cs`:
```csharp
var synthetic = 0x0F000000u + (uint)(synth++ * 0x10u);
vm.Write32(va, synthetic);  // Write synthetic address to IAT
var stub = new byte[] { 0xCC, 0x90, 0x90, ... };
vm.WriteBytes(synthetic, stub);
map[synthetic] = (dll.ToUpperInvariant(), name);
```

### COM Vtable Stub Creation (ComVtableDispatcher.cs)

For COM objects:

1. A vtable is allocated in guest memory
2. Each vtable slot points to a synthetic address in the `0x0D000000` range
3. An INT3 stub is created at each synthetic address
4. The method name is stored for debugging purposes

Example:
```csharp
var methodStubAddr = stubAddr + (methodIndex * 0x10);
_env.MemWrite32(vtableAddr + (methodIndex * 4), methodStubAddr);
var stub = new byte[] { 0xCC, 0x90, 0x90, ... };
_env.MemWriteBytes(methodStubAddr, stub);
_vtableHandlers[methodStubAddr] = handler;
_vtableMethodNames[methodStubAddr] = $"{interfaceName}::{methodName}";
```

## Call Instruction Handling

The CPU emulator (IcedCpu.cs) handles both direct and indirect CALL instructions:

### 1. Register Call: `CALL EBP` or `CALL EAX`
```csharp
if (insn.GetOpKind(0) == OpKind.Register)
{
    _eip = GetReg32(insn.GetOpRegister(0));
    callTarget = _eip;
    isCall = true;
}
```

### 2. Indirect Memory Call: `CALL dword ptr [address]`
```csharp
else if (insn.GetOpKind(0) == OpKind.Memory)
{
    _eip = Read32(CalcMemAddress(insn));
    callTarget = _eip;
    isCall = true;
}
```

### 3. Direct Call: `CALL immediate`
```csharp
else
{
    _eip = (uint)insn.NearBranchTarget;
    callTarget = _eip;
    isCall = true;
}
```

**YES, both call types mentioned in the problem statement are handled!**

## INT/INT3 Interception

When the CPU executes an instruction at a synthetic address, it encounters the INT3 breakpoint:

### INT3 (0xCC) Instruction
```csharp
case Mnemonic.Int3:
    if (oldEip is >= 0x0F000000 and < 0x10000000)
    {
        // Import stub
        isCall = true;
        callTarget = oldEip;
        _logger.LogInformation("[IcedCpu] INT3 (0xCC) hooking import stub at address 0x{OldEip:X8}", oldEip);
    }
    else if (oldEip is >= 0x0D000000 and < 0x0E000000)
    {
        // COM vtable stub
        isCall = true;
        callTarget = oldEip;
        _logger.LogInformation("[IcedCpu] INT3 (0xCC) hooking COM vtable stub at address 0x{OldEip:X8}", oldEip);
    }
```

### INT 3 (two-byte form)
```csharp
case Mnemonic.Int:
    if (insn.Immediate8 == 3)
    {
        // Same handling as INT3...
        _logger.LogInformation("[IcedCpu] INT 3 hooking import stub at address 0x{OldEip:X8}", oldEip);
    }
```

## Function Resolution and Invocation

After the CPU signals an import call, the emulator (Emulator.cs) resolves and invokes the function:

```csharp
if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
{
    var dll = imp.dll.ToUpperInvariant();
    var name = imp.name;
    _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", 
        dll, name, step.CallTarget);
    
    if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm, out var ret, out var argBytes))
    {
        // Handle return...
    }
}
```

For COM vtable methods:
```csharp
if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
{
    _logger.LogInformation("[COM] Vtable method call at address 0x{CallTarget:X8}", step.CallTarget);
    
    if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm, out var ret))
    {
        // This logs: "[COM] Invoking vtable method: {MethodName} at address 0x{Address:X8}"
    }
}
```

## Understanding the Log Output

When you run an emulated program, you'll see logging like this:

### Import Function Call
```
[IcedCpu] INT3 (0xCC) hooking import stub at address 0x0F000010
[Import] Hooked function: KERNEL32.DLL!GetModuleFileNameA at address 0x0F000010
[Dispatcher] KERNEL32.DLL!GetModuleFileNameA at EIP=0x0F000010 ESP=0x...
[Kernel32] GetModuleFileNameA called: h=0x00400000 lp=0x... n=260
```

This tells you:
1. INT3 was encountered at the synthetic address `0x0F000010`
2. This address maps to `KERNEL32.DLL!GetModuleFileNameA`
3. The dispatcher is invoking the function
4. The actual function implementation logs its parameters

### COM Vtable Method Call
```
[IcedCpu] INT3 (0xCC) hooking COM vtable stub at address 0x0D001020
[COM] Vtable method call at address 0x0D001020
[COM] Invoking vtable method: IDirectDraw::SetCooperativeLevel at address 0x0D001020
```

This tells you:
1. INT3 was encountered at the synthetic address `0x0D001020`
2. This is a COM vtable method call
3. The specific method is `IDirectDraw::SetCooperativeLevel`

## Call Patterns Handled

The emulator handles both call patterns mentioned in the problem statement:

### Pattern 1: `CALL dword ptr [->KERNEL32.DLL::GetModuleFileNameA]`
This is an indirect call through the IAT:
1. The IAT contains `0x0F000010` (synthetic address)
2. `CALL [IAT_entry]` reads the value and jumps to `0x0F000010`
3. CPU executes INT3 at `0x0F000010`
4. Emulator resolves to `KERNEL32.DLL!GetModuleFileNameA`

### Pattern 2: `CALL EBP=>KERNEL32.DLL::GetModuleFileNameA`
This is a register call where EBP contains the function pointer:
1. EBP contains `0x0F000010` (loaded from IAT earlier)
2. `CALL EBP` jumps to `0x0F000010`
3. CPU executes INT3 at `0x0F000010`
4. Emulator resolves to `KERNEL32.DLL!GetModuleFileNameA`

**Both patterns ultimately result in the same INT3 interception and function resolution!**

## Debugging Tips

1. **Enable Information logging** to see all function hooks
2. **Look for the address range** to determine if it's an import (0x0F...) or COM (0x0D...)
3. **Check the import map** if a function name isn't shown
4. **Verify vtable setup** for COM objects if methods aren't being intercepted

## Architecture Diagram

```
Guest Code                     Emulator
    |                              |
    v                              |
CALL [IAT]  ----reads------> 0x0F000010 (synthetic addr)
    |                              |
    v                              |
JMP 0x0F000010                     |
    |                              |
    v                              |
INT3 at 0x0F000010 -----signals--> IcedCpu detects INT3
                                   |
                                   v
                              Import map lookup
                                   |
                                   v
                              "KERNEL32.DLL!GetModuleFileNameA"
                                   |
                                   v
                              Dispatcher.TryInvoke()
                                   |
                                   v
                              Kernel32Module.GetModuleFileNameA()
```

## Summary

- ✅ Both `CALL [mem]` and `CALL reg` patterns are handled
- ✅ Logging shows which function is being hooked at the INT/INT3 level
- ✅ Logging shows the vtable address for COM method calls
- ✅ Logging shows the resolved function name (DLL!Export)
- ✅ Synthetic addresses in the `0x0F000000` range are import stubs
- ✅ Synthetic addresses in the `0x0D000000` range are COM vtable stubs
