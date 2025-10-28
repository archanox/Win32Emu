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
        
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("namespace Win32Emu.Jit.Generated");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Auto-generated JIT code for block at 0x{rtlBlock.StartAddress:X8}");
        sb.AppendLine($"    /// Generated from RTL intermediate representation");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");
        sb.AppendLine($"        public async Task<CpuStepResult> {methodName}(dynamic cpu, dynamic mem)");
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
                GenerateInstruction(sb, insn);
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
        sb.AppendLine("            return await Task.FromResult(new CpuStepResult { IsCall = false, CallTarget = 0 });");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    private void GenerateInstruction(StringBuilder sb, RtlInstruction insn)
    {
        var code = insn switch
        {
            RtlAssignment assign => $"{ExpressionToString(assign.Destination)} = {ExpressionToString(assign.Source)};",
            RtlBinaryOp binOp => $"{ExpressionToString(binOp.Destination)} = {ExpressionToString(binOp.Left)} {binOp.Operator} {ExpressionToString(binOp.Right)};",
            RtlBranch branch => $"if ({ExpressionToString(branch.Condition)}) goto Label_{branch.TargetOffset:X};",
            RtlGoto goto_ => $"goto Label_{goto_.TargetOffset:X};",
            RtlCall call => GenerateCall(call),
            RtlReturn ret => ret.ReturnValue != null ? 
                $"return await Task.FromResult(new CpuStepResult {{ IsCall = false, CallTarget = 0 }});" :
                "return await Task.FromResult(new CpuStepResult { IsCall = false, CallTarget = 0 });",
            RtlLoad load => $"{ExpressionToString(load.Destination)} = mem.Read{load.Size * 8}({ExpressionToString(load.Address)});",
            RtlStore store => $"mem.Write{store.Size * 8}({ExpressionToString(store.Address)}, {ExpressionToString(store.Value)});",
            RtlSimdOp simd => $"// {simd.Comment}",
            RtlNop => "// nop",
            _ => "// unknown instruction"
        };
        
        sb.AppendLine($"            {code} // @0x{insn.Offset:X}");
    }
    
    private string GenerateCall(RtlCall call)
    {
        var target = ExpressionToString(call.Target);
        var args = string.Join(", ", call.Arguments.Select(ExpressionToString));
        
        if (call.ReturnValue != null)
        {
            return $"{ExpressionToString(call.ReturnValue)} = await CallFunction({target}, new object[] {{ {args} }});";
        }
        return $"await CallFunction({target}, new object[] {{ {args} }});";
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
    /// Generate a complete compilation unit (for Roslyn compilation)
    /// </summary>
    public CompilationUnitSyntax GenerateCompilationUnit(RtlCodeBlock rtlBlock, string className, string methodName)
    {
        var code = GenerateCSharpCode(rtlBlock, className, methodName);
        return (CompilationUnitSyntax)SyntaxFactory.ParseCompilationUnit(code);
    }
}

/// <summary>
/// Helper struct for CPU step results (matches Win32Emu.Cpu.CpuStepResult)
/// </summary>
public struct CpuStepResult
{
    public bool IsCall { get; set; }
    public uint CallTarget { get; set; }
}
