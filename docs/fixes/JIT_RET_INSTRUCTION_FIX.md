# JIT RET Instruction Bug Fix

## Problem
BasicDD.exe (simple DirectDraw example) was crashing at address 0x0040715A, which is in the data section. The crash occurred after `GetAttachedSurface` returned successfully, indicating execution had jumped to an invalid address.

## Root Cause
The JIT compiler's RTL (Register Transfer Language) pipeline was generating incorrect C# code for RET instructions. When compiling a block that ended with a RET instruction, the generated code would simply return from the method without implementing the actual RET semantics:

```csharp
// INCORRECT - what the JIT was generating before
return await Task.FromResult(new CpuStepResult { IsCall = false, CallTarget = 0 });
```

This meant:
1. The return address on the stack was never popped
2. EIP was never updated to the return address
3. Stack cleanup for stdcall (RET imm16) was not performed

As a result, when the JIT-compiled block returned, execution would continue from whatever EIP happened to be, not from the correct return address. This would eventually lead to jumping to invalid addresses like 0x0040715A.

## Solution
Implemented proper RET instruction semantics in the JIT-generated C# code:

```csharp
// CORRECT - what the JIT generates now
{ // RET instruction
    uint retAddr = mem.Read32(ESP);     // 1. Pop return address from stack
    ESP += 4;                            // 2. Increment stack pointer
    ESP += 0x8u;                         // 3. Stdcall cleanup (if RET has immediate)
    cpu.SetEip(retAddr);                 // 4. Update EIP to return address
    cpu.SetRegister("ESP", ESP);         // 5. Save updated ESP
    return await Task.FromResult(new CpuStepResult { IsCall = false, CallTarget = 0 });
}
```

## Changes Made

### 1. RtlInstruction.cs
Added `StackCleanup` field to `RtlReturn` class to store the immediate operand for RET imm16:
```csharp
public class RtlReturn : RtlInstruction
{
    public RtlExpression? ReturnValue { get; set; }
    public ushort StackCleanup { get; set; } // For stdcall RET imm16
    
    public override string ToReadableString() => StackCleanup > 0 ? $"return (pop stack + {StackCleanup})" : "return";
}
```

### 2. X86ToRtlConverter.cs
Updated RET instruction conversion to extract and store the immediate operand:
```csharp
case Mnemonic.Ret:
    var stackCleanup = (ushort)0;
    if (insn.OpCount > 0 && insn.Op0Kind == OpKind.Immediate16)
    {
        stackCleanup = insn.Immediate16;
    }
    results.Add(new RtlReturn
    {
        Offset = (int)insn.IP,
        ReturnValue = new RtlRegister { Name = "EAX" },
        StackCleanup = stackCleanup
    });
    break;
```

### 3. RtlToCSharpGenerator.cs
Implemented `GenerateReturn` method to generate correct C# code for RET:
```csharp
private string GenerateReturn(RtlReturn ret)
{
    var sb = new StringBuilder();
    sb.AppendLine("{ // RET instruction");
    sb.AppendLine("                uint retAddr = mem.Read32(ESP);");
    sb.AppendLine("                ESP += 4;");
    if (ret.StackCleanup > 0)
    {
        sb.AppendLine($"                ESP += 0x{ret.StackCleanup:X}u; // stdcall cleanup");
    }
    sb.AppendLine("                cpu.SetEip(retAddr);");
    sb.AppendLine("                cpu.SetRegister(\"ESP\", ESP);");
    sb.AppendLine("                return await Task.FromResult(new CpuStepResult { IsCall = false, CallTarget = 0 });");
    sb.Append("            }");
    return sb.ToString();
}
```

### 4. JitRetInstructionTests.cs
Added unit tests to verify RET and RET imm16 work correctly:
- `RetInstruction_ShouldPopAddressAndUpdateEIP` - Tests basic RET
- `RetWithImmediate_ShouldPopAddressAndCleanupStack` - Tests RET imm16

## Impact
This fix resolves:
- BasicDD.exe crash at 0x0040715A
- Any other crashes caused by incorrect RET handling in JIT-compiled code
- Execution jumping to data sections or other invalid addresses after function returns

Since RET is fundamental to function returns, this bug would have affected any JIT-compiled code that returned from functions, making it a critical fix for the JIT pipeline.

## Verification
- Unit tests for RET and RET imm16 pass
- No regression in existing emulator tests
- Code review completed with feedback addressed
- The fix follows x86 RET instruction specification exactly
