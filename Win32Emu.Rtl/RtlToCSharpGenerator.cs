using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Win32Emu.Rtl;

/// <summary>
/// Generates readable C# code from RTL intermediate representation.
/// Produces compilable C# that can be persisted and inspected.
/// </summary>
public class RtlToCSharpGenerator
{
    /// <summary>
    /// Generate C# code for an RTL code block
    /// </summary>
    public string GenerateCSharpCode(RtlCodeBlock rtlBlock, string className, string methodName)
    {
        var sb = new StringBuilder();
        
        // Check if we need SIMD namespaces
        var usesSIMD = HasSimdInstructions(rtlBlock);
        
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        if (usesSIMD)
        {
            sb.AppendLine("using System.Runtime.Intrinsics;");
        }
        sb.AppendLine("using Win32Emu.Cpu; // For CpuStepResult");
        sb.AppendLine();
        sb.AppendLine("namespace Win32Emu.Jit.Generated");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Auto-generated JIT code for block at 0x{rtlBlock.StartAddress:X8}");
        sb.AppendLine($"    /// Generated from RTL intermediate representation");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");
        sb.AppendLine($"        public static async Task<CpuStepResult> {methodName}(dynamic cpu, dynamic mem)");
        sb.AppendLine("        {");
        sb.AppendLine("            // CPU state");
        sb.AppendLine("            uint EAX = cpu.GetRegister(\"EAX\");");
        sb.AppendLine("            uint EBX = cpu.GetRegister(\"EBX\");");
        sb.AppendLine("            uint ECX = cpu.GetRegister(\"ECX\");");
        sb.AppendLine("            uint EDX = cpu.GetRegister(\"EDX\");");
        sb.AppendLine("            uint ESI = cpu.GetRegister(\"ESI\");");
        sb.AppendLine("            uint EDI = cpu.GetRegister(\"EDI\");");
        sb.AppendLine("            uint EBP = cpu.GetRegister(\"EBP\");");
        sb.AppendLine("            uint ESP = cpu.GetRegister(\"ESP\");");
        sb.AppendLine("            uint FLAGS = 0;");
        sb.AppendLine();
        
        // Generate temporaries
        if (rtlBlock.NextTemporaryId > 0)
        {
            sb.AppendLine("            // Temporaries");
            for (int i = 0; i < rtlBlock.NextTemporaryId; i++)
            {
                sb.AppendLine($"            uint t{i} = 0;");
            }
            sb.AppendLine();
        }
        
        // Generate code for each basic block
        foreach (var bb in rtlBlock.BasicBlocks)
        {
            sb.AppendLine($"            // Block at offset 0x{bb.StartOffset:X}");
            foreach (var insn in bb.Instructions)
            {
                GenerateInstruction(sb, insn, rtlBlock);
            }
            sb.AppendLine();
        }
        
        // Save state and return
        sb.AppendLine("            // Save CPU state");
        sb.AppendLine("            cpu.SetRegister(\"EAX\", EAX);");
        sb.AppendLine("            cpu.SetRegister(\"EBX\", EBX);");
        sb.AppendLine("            cpu.SetRegister(\"ECX\", ECX);");
        sb.AppendLine("            cpu.SetRegister(\"EDX\", EDX);");
        sb.AppendLine("            cpu.SetRegister(\"ESI\", ESI);");
        sb.AppendLine("            cpu.SetRegister(\"EDI\", EDI);");
        sb.AppendLine("            cpu.SetRegister(\"EBP\", EBP);");
        sb.AppendLine("            cpu.SetRegister(\"ESP\", ESP);");
        sb.AppendLine();
        // Set EIP to the address following this block (critical for execution to continue)
        sb.AppendLine($"            // Advance EIP to next instruction after this block");
        sb.AppendLine($"            cpu.SetEip(0x{rtlBlock.EndAddress:X8}u);");
        sb.AppendLine();
        sb.AppendLine("            return await Task.FromResult(new CpuStepResult(IsCall: false, CallTarget: 0));");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    private void GenerateInstruction(StringBuilder sb, RtlInstruction insn, RtlCodeBlock rtlBlock)
    {
        var code = insn switch
        {
            RtlAssignment assign => $"{ExpressionToString(assign.Destination)} = {ExpressionToString(assign.Source)};",
            RtlBinaryOp binOp => $"{ExpressionToString(binOp.Destination)} = {ExpressionToString(binOp.Left)} {binOp.Operator} {ExpressionToString(binOp.Right)};",
            RtlBranch branch => GenerateBranch(branch, rtlBlock),
            RtlGoto goto_ => GenerateGoto(goto_, rtlBlock),
            RtlCall call => GenerateCall(call),
            RtlReturn ret => GenerateReturn(ret),
            RtlLoad load => $"{ExpressionToString(load.Destination)} = mem.Read{load.Size * 8}({ExpressionToString(load.Address)});",
            RtlStore store => $"mem.Write{store.Size * 8}({ExpressionToString(store.Address)}, {ExpressionToString(store.Value)});",
            RtlSimdOp simd => GenerateSimdOperation(simd),
            RtlNop => "// nop",
            _ => "// unknown instruction"
        };
        
        // For multi-line instructions (like RET), the offset is already included in the code string
        // Check if offset comment already exists to avoid duplication
        if (code.Contains($"@0x{insn.Offset:X}"))
        {
            sb.AppendLine($"            {code}");
        }
        else
        {
            sb.AppendLine($"            {code} // @0x{insn.Offset:X}");
        }
    }
    
    /// <summary>
    /// Checks if an address is within the current RTL block
    /// </summary>
    private bool IsAddressInBlock(uint address, RtlCodeBlock rtlBlock)
    {
        return address >= rtlBlock.StartAddress && address < rtlBlock.EndAddress;
    }
    
    /// <summary>
    /// Generates code to save all CPU registers.
    /// Used when exiting the JIT block early (branches, gotos, calls).
    /// </summary>
    private static string GenerateRegisterSave()
    {
        return @"cpu.SetRegister(""EAX"", EAX);
                cpu.SetRegister(""EBX"", EBX);
                cpu.SetRegister(""ECX"", ECX);
                cpu.SetRegister(""EDX"", EDX);
                cpu.SetRegister(""ESI"", ESI);
                cpu.SetRegister(""EDI"", EDI);
                cpu.SetRegister(""EBP"", EBP);
                cpu.SetRegister(""ESP"", ESP);";
    }
    
    /// <summary>
    /// Generate code for a conditional branch instruction.
    /// If the target is outside the block, sets EIP and returns.
    /// </summary>
    private string GenerateBranch(RtlBranch branch, RtlCodeBlock rtlBlock)
    {
        var targetAddr = (uint)branch.TargetOffset;
        if (IsAddressInBlock(targetAddr, rtlBlock))
        {
            // Target is within block - use goto
            return $"if ({ExpressionToString(branch.Condition)}) goto Label_{branch.TargetOffset:X};";
        }
        else
        {
            // Target is outside block - set EIP and return
            return $@"if ({ExpressionToString(branch.Condition)}) {{ // @0x{branch.Offset:X}
                {GenerateRegisterSave()}
                cpu.SetEip(0x{branch.TargetOffset:X8}u);
                return await Task.FromResult(new CpuStepResult(IsCall: false, CallTarget: 0));
            }}";
        }
    }
    
    /// <summary>
    /// Generate code for an unconditional goto instruction.
    /// If the target is outside the block, sets EIP and returns.
    /// </summary>
    private string GenerateGoto(RtlGoto goto_, RtlCodeBlock rtlBlock)
    {
        var targetAddr = (uint)goto_.TargetOffset;
        if (IsAddressInBlock(targetAddr, rtlBlock))
        {
            // Target is within block - use goto
            return $"goto Label_{goto_.TargetOffset:X};";
        }
        else
        {
            // Target is outside block - set EIP and return
            return $@"{{ // @0x{goto_.Offset:X}
                {GenerateRegisterSave()}
                cpu.SetEip(0x{goto_.TargetOffset:X8}u);
                return await Task.FromResult(new CpuStepResult(IsCall: false, CallTarget: 0));
            }}";
        }
    }
    
    private string GenerateCall(RtlCall call)
    {
        var target = ExpressionToString(call.Target);
        
        // CALL instruction semantics:
        // 1. Push return address onto stack: ESP -= 4; [ESP] = nextEIP
        // 2. Set EIP to call target
        // 3. Return from compiled block to let main emulator handle the call
        // 
        // The JIT block cannot execute the call inline because we need to
        // return control to the emulator loop which handles syscalls, COM calls, etc.
        
        var sb = new StringBuilder();
        sb.AppendLine($"{{ // CALL instruction @0x{call.Offset:X}");
        sb.AppendLine("                // Save all registers before call");
        sb.AppendLine($"                {GenerateRegisterSave()}");
        sb.AppendLine($"                uint callTarget = {target};");
        sb.AppendLine($"                uint returnAddr = 0x{call.ReturnAddress:X8}u;");
        sb.AppendLine("                // Push return address");
        sb.AppendLine("                ESP -= 4;");
        sb.AppendLine("                mem.Write32(ESP, returnAddr);");
        sb.AppendLine("                cpu.SetRegister(\"ESP\", ESP);");
        sb.AppendLine("                // Set EIP to call target");
        sb.AppendLine("                cpu.SetEip(callTarget);");
        sb.AppendLine("                return await Task.FromResult(new CpuStepResult(IsCall: true, CallTarget: callTarget));");
        sb.Append("            }");
        return sb.ToString();
    }
    
    private string GenerateReturn(RtlReturn ret)
    {
        // RET instruction semantics:
        // 1. Pop return address from stack: retAddr = [ESP]; ESP += 4
        // 2. If immediate operand, add it to ESP (stdcall cleanup): ESP += imm16
        // 3. Update EIP to return address
        // 4. Return from compiled block
        // Note: ESP variable is defined in the generated method (see GenerateCSharpCode line 43)
        
        var sb = new StringBuilder();
        // For multi-line blocks, include offset on the opening line instead of closing brace
        sb.AppendLine($"{{ // RET instruction @0x{ret.Offset:X}");
        sb.AppendLine("                uint retAddr = mem.Read32(ESP);");
        sb.AppendLine("                ESP += 4;");
        if (ret.StackCleanup > 0)
        {
            sb.AppendLine($"                ESP += 0x{ret.StackCleanup:X}u; // stdcall cleanup");
        }
        sb.AppendLine("                cpu.SetEip(retAddr);");
        sb.AppendLine("                cpu.SetRegister(\"ESP\", ESP);");
        sb.AppendLine("                return await Task.FromResult(new CpuStepResult(IsCall: false, CallTarget: 0));");
        sb.Append("            }");
        return sb.ToString();
    }
    
    private string ExpressionToString(RtlExpression expr)
    {
        return expr switch
        {
            RtlRegister reg => reg.Name,
            RtlConstant const_ => $"0x{const_.Value:X}u",
            RtlTemporary temp => $"t{temp.Id}",
            RtlBinaryExpression binExpr => $"({ExpressionToString(binExpr.Left)} {binExpr.Operator} {ExpressionToString(binExpr.Right)})",
            RtlUnaryExpression unExpr => $"{unExpr.Operator}({ExpressionToString(unExpr.Operand)})",
            _ => "0"
        };
    }
    
    /// <summary>
    /// Check if the RTL block contains SIMD instructions
    /// </summary>
    private bool HasSimdInstructions(RtlCodeBlock rtlBlock)
    {
        return rtlBlock.BasicBlocks
            .SelectMany(bb => bb.Instructions)
            .Any(insn => insn is RtlSimdOp);
    }
    
    /// <summary>
    /// Generate C# code for SIMD operations using System.Runtime.Intrinsics
    /// </summary>
    private string GenerateSimdOperation(RtlSimdOp simd)
    {
        if (!simd.IsMemoryOperation)
        {
            // Legacy comment-only SIMD marker
            return $"// {simd.Comment}";
        }
        
        if (simd.BaseAddress == null)
        {
            return "// ERROR: SIMD memory operation requires a non-null BaseAddress";
        }
        
        var sb = new StringBuilder();
        sb.AppendLine($"{{ // {simd.Comment}");
        
        var baseAddr = ExpressionToString(simd.BaseAddress);
        
        // Generate vector load
        sb.AppendLine($"                // Load vector from memory");
        sb.AppendLine($"                var vecAddr = {baseAddr};");
        
        // For 4x uint32, we need to load 16 bytes
        sb.AppendLine($"                var v1_0 = mem.Read32(vecAddr);");
        sb.AppendLine($"                var v1_1 = mem.Read32(vecAddr + 4);");
        sb.AppendLine($"                var v1_2 = mem.Read32(vecAddr + 8);");
        sb.AppendLine($"                var v1_3 = mem.Read32(vecAddr + 12);");
        sb.AppendLine($"                var vector1 = Vector128.Create(v1_0, v1_1, v1_2, v1_3);");
        
        // Generate operand2 (either vector or scalar to broadcast)
        if (simd.Operand2 != null)
        {
            var operand2Str = ExpressionToString(simd.Operand2);
            if (simd.Operand2 is RtlConstant)
            {
                // Broadcast scalar to vector
                sb.AppendLine($"                var vector2 = Vector128.Create({operand2Str});");
            }
            else
            {
                // Assume it's another vector (would need address)
                sb.AppendLine($"                var vector2 = Vector128.Create({operand2Str});");
            }
        }
        
        // Generate vector operation using platform-agnostic Vector128 operations
        var intrinsicOp = simd.Operation switch
        {
            "Add" => "Vector128.Add",
            "Subtract" => "Vector128.Subtract",
            "Multiply" => "Vector128.Multiply",
            "BitwiseAnd" => "Vector128.BitwiseAnd",
            "BitwiseOr" => "Vector128.BitwiseOr",
            "Xor" => "Vector128.Xor",
            _ => $"/* Unsupported: {simd.Operation} */"
        };
        
        if (simd.Operand2 != null)
        {
            sb.AppendLine($"                var result = {intrinsicOp}(vector1, vector2);");
        }
        else
        {
            sb.AppendLine($"                var result = {intrinsicOp}(vector1);");
        }
        
        // Extract and store results
        if (simd.IsStore)
        {
            sb.AppendLine($"                // Store vector result to memory");
            sb.AppendLine($"                mem.Write32(vecAddr, result.GetElement(0));");
            sb.AppendLine($"                mem.Write32(vecAddr + 4, result.GetElement(1));");
            sb.AppendLine($"                mem.Write32(vecAddr + 8, result.GetElement(2));");
            sb.AppendLine($"                mem.Write32(vecAddr + 12, result.GetElement(3));");
        }
        
        sb.Append("            }");
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate a complete compilation unit (for Roslyn compilation)
    /// </summary>
    public CompilationUnitSyntax GenerateCompilationUnit(RtlCodeBlock rtlBlock, string className, string methodName)
    {
        var code = GenerateCSharpCode(rtlBlock, className, methodName);
        return (CompilationUnitSyntax)SyntaxFactory.ParseCompilationUnit(code);
    }
}


