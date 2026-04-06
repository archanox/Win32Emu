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
    private const uint ByteMask = 0xFFu;
    private const uint WordMask = 0xFFFFu;
    private const uint FullRegisterMask = uint.MaxValue;
    private const int LowByteShift = 0;
    private const int HighByteShift = 8;

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
        sb.AppendLine("            uint CS = cpu.GetRegister(\"CS\");");
        sb.AppendLine("            uint DS = cpu.GetRegister(\"DS\");");
        sb.AppendLine("            uint ES = cpu.GetRegister(\"ES\");");
        sb.AppendLine("            uint FS = cpu.GetRegister(\"FS\");");
        sb.AppendLine("            uint GS = cpu.GetRegister(\"GS\");");
        sb.AppendLine("            uint SS = cpu.GetRegister(\"SS\");");
        sb.AppendLine("            uint EFLAGS = cpu.GetRegister(\"EFLAGS\");");
        sb.AppendLine("            bool ZF = (EFLAGS & 0x40u) != 0;");
        sb.AppendLine("            bool CF = (EFLAGS & 0x1u) != 0;");
        sb.AppendLine("            bool SF = (EFLAGS & 0x80u) != 0;");
        sb.AppendLine("            bool OF = (EFLAGS & 0x800u) != 0;");
        sb.AppendLine("            bool PF = (EFLAGS & 0x4u) != 0;");
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
        sb.AppendLine("            cpu.SetRegister(\"CS\", CS);");
        sb.AppendLine("            cpu.SetRegister(\"DS\", DS);");
        sb.AppendLine("            cpu.SetRegister(\"ES\", ES);");
        sb.AppendLine("            cpu.SetRegister(\"FS\", FS);");
        sb.AppendLine("            cpu.SetRegister(\"GS\", GS);");
        sb.AppendLine("            cpu.SetRegister(\"SS\", SS);");
        sb.AppendLine("            EFLAGS = (EFLAGS & 0xFFFFF73Au) | (CF ? 0x1u : 0u) | (PF ? 0x4u : 0u) | (ZF ? 0x40u : 0u) | (SF ? 0x80u : 0u) | (OF ? 0x800u : 0u);");
        sb.AppendLine("            cpu.SetRegister(\"EFLAGS\", EFLAGS);");
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
            RtlAssignment assign => GenerateAssignment(assign),
            RtlBinaryOp binOp => GenerateBinaryOp(binOp),
            RtlBranch branch => GenerateBranch(branch, rtlBlock),
            RtlGoto goto_ => GenerateGoto(goto_, rtlBlock),
            RtlCall call => GenerateCall(call),
            RtlReturn ret => GenerateReturn(ret),
            RtlLoad load => GenerateLoad(load),
            RtlStore store => store.Size switch
            {
                1 => $"mem.Write8((ulong)({ExpressionToString(store.Address)}), (byte)({ExpressionToString(store.Value)}));",
                2 => $"mem.Write16((ulong)({ExpressionToString(store.Address)}), (ushort)({ExpressionToString(store.Value)}));",
                4 => $"mem.Write32((ulong)({ExpressionToString(store.Address)}), (uint)({ExpressionToString(store.Value)}));",
                8 => $"mem.Write64((ulong)({ExpressionToString(store.Address)}), (ulong)({ExpressionToString(store.Value)}));",
                _ => $"/* ERROR: unsupported store size {store.Size} */"
            },
            RtlSimdOp simd => GenerateSimdOperation(simd),
            RtlFlagUpdate flagUpdate => GenerateFlagUpdate(flagUpdate),
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
                cpu.SetRegister(""ESP"", ESP);
                cpu.SetRegister(""CS"", CS);
                cpu.SetRegister(""DS"", DS);
                cpu.SetRegister(""ES"", ES);
                cpu.SetRegister(""FS"", FS);
                cpu.SetRegister(""GS"", GS);
                cpu.SetRegister(""SS"", SS);
                EFLAGS = (EFLAGS & 0xFFFFF73Au) | (CF ? 0x1u : 0u) | (PF ? 0x4u : 0u) | (ZF ? 0x40u : 0u) | (SF ? 0x80u : 0u) | (OF ? 0x800u : 0u);
                cpu.SetRegister(""EFLAGS"", EFLAGS);";
    }

    private string GenerateAssignment(RtlAssignment assign)
    {
        return GenerateDestinationAssignment(assign.Destination, ExpressionToString(assign.Source));
    }

    private string GenerateBinaryOp(RtlBinaryOp binOp)
    {
        var right = IsShiftOperator(binOp.Operator)
            ? $"(int)({ExpressionToString(binOp.Right)})"
            : ExpressionToString(binOp.Right);
        return GenerateDestinationAssignment(
            binOp.Destination,
            $"{ExpressionToString(binOp.Left)} {binOp.Operator} {right}"
        );
    }

    private static bool IsShiftOperator(string op) => op is "<<" or ">>";

    private string GenerateLoad(RtlLoad load)
    {
        return GenerateDestinationAssignment(
            load.Destination,
            $"mem.Read{load.Size * 8}((ulong)({ExpressionToString(load.Address)}))"
        );
    }

    private string GenerateDestinationAssignment(RtlExpression destination, string sourceExpression)
    {
        if (destination is RtlRegister register)
        {
            return GenerateRegisterWrite(register.Name, sourceExpression);
        }

        return $"{ExpressionToString(destination)} = {sourceExpression};";
    }
    
    /// <summary>
    /// Generate code for a conditional branch instruction.
    /// If the target is outside the block, sets EIP and returns.
    /// </summary>
    private string GenerateBranch(RtlBranch branch, RtlCodeBlock rtlBlock)
    {
        var targetAddr = (uint)branch.TargetOffset;
        var condition = branch.FlagCondition != FlagCondition.None
            ? GenerateFlagConditionExpression(branch.FlagCondition)
            : ExpressionToString(branch.Condition!);
        return IsAddressInBlock(targetAddr, rtlBlock)
            ? $"if ({condition}) goto Label_{branch.TargetOffset:X};"
            : $@"if ({condition}) {{ // @0x{branch.Offset:X}
                {GenerateRegisterSave()}
                cpu.SetEip(0x{branch.TargetOffset:X8}u);
                return await Task.FromResult(new CpuStepResult(IsCall: false, CallTarget: 0));
            }}";
    }
    
    /// <summary>
    /// Generate code for an unconditional goto instruction.
    /// If the target is outside the block, sets EIP and returns.
    /// </summary>
    private string GenerateGoto(RtlGoto goto_, RtlCodeBlock rtlBlock)
    {
        var targetAddr = (uint)goto_.TargetOffset;
        return IsAddressInBlock(targetAddr, rtlBlock)
            ? $"goto Label_{goto_.TargetOffset:X};"
            : $@"{{ // @0x{goto_.Offset:X}
                {GenerateRegisterSave()}
                cpu.SetEip(0x{goto_.TargetOffset:X8}u);
                return await Task.FromResult(new CpuStepResult(IsCall: false, CallTarget: 0));
            }}";
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
        sb.AppendLine("                mem.Write32((ulong)ESP, returnAddr);");
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
		sb.AppendLine("                uint retAddr = mem.Read32((ulong)ESP);");
		sb.AppendLine("                ESP += 4;");
		if (ret.StackCleanup > 0)
		{
			sb.AppendLine($"                ESP += 0x{ret.StackCleanup:X}u; // stdcall cleanup");
		}
		sb.AppendLine("                // Save register state before return; EIP is updated below");
		sb.AppendLine($"                {GenerateRegisterSave()}");
		sb.AppendLine("                cpu.SetEip(retAddr);");
		sb.AppendLine("                return await Task.FromResult(new CpuStepResult(IsCall: false, CallTarget: 0));");
		sb.Append("            }");
		return sb.ToString();
	}
    
    private string ExpressionToString(RtlExpression expr)
    {
        return expr switch
        {
            RtlRegister reg => GetRegisterReadExpression(reg.Name),
            RtlConstant const_ => $"0x{const_.Value:X}u",
            RtlTemporary temp => $"t{temp.Id}",
            RtlBinaryExpression binExpr => IsShiftOperator(binExpr.Operator)
                ? $"({ExpressionToString(binExpr.Left)} {binExpr.Operator} (int)({ExpressionToString(binExpr.Right)}))"
                : $"({ExpressionToString(binExpr.Left)} {binExpr.Operator} {ExpressionToString(binExpr.Right)})",
            RtlUnaryExpression unExpr => $"{unExpr.Operator}({ExpressionToString(unExpr.Operand)})",
            RtlFlagReference flagRef => $"({GenerateFlagConditionExpression(flagRef.Condition)} ? 1u : 0u)",
            _ => "0"
        };
    }

    private string GetRegisterReadExpression(string registerName)
    {
        var normalizedName = registerName.ToUpperInvariant();

        return normalizedName switch
        {
            "AL" => $"(EAX & 0x{ByteMask:X}u)",
            "AH" => $"((EAX >> {HighByteShift}) & 0x{ByteMask:X}u)",
            "AX" => $"(EAX & 0x{WordMask:X}u)",
            "BL" => $"(EBX & 0x{ByteMask:X}u)",
            "BH" => $"((EBX >> {HighByteShift}) & 0x{ByteMask:X}u)",
            "BX" => $"(EBX & 0x{WordMask:X}u)",
            "CL" => $"(ECX & 0x{ByteMask:X}u)",
            "CH" => $"((ECX >> {HighByteShift}) & 0x{ByteMask:X}u)",
            "CX" => $"(ECX & 0x{WordMask:X}u)",
            "DL" => $"(EDX & 0x{ByteMask:X}u)",
            "DH" => $"((EDX >> {HighByteShift}) & 0x{ByteMask:X}u)",
            "DX" => $"(EDX & 0x{WordMask:X}u)",
            "SI" => $"(ESI & 0x{WordMask:X}u)",
            "DI" => $"(EDI & 0x{WordMask:X}u)",
            "BP" => $"(EBP & 0x{WordMask:X}u)",
            "SP" => $"(ESP & 0x{WordMask:X}u)",
            _ => normalizedName
        };
    }

    private string GenerateRegisterWrite(string registerName, string valueExpression)
    {
        var normalizedName = registerName.ToUpperInvariant();
        var value = $"unchecked((uint)({valueExpression}))";

        if (TryGetPartialRegisterInfo(normalizedName, out var parentRegister, out var shift, out var mask))
        {
            return GeneratePartialRegisterWrite(parentRegister, shift, mask, value);
        }

        if (IsSegmentRegister(normalizedName))
        {
            return $"{normalizedName} = {value} & 0x{WordMask:X}u;";
        }

        return $"{normalizedName} = {value};";
    }

    private static string GeneratePartialRegisterWrite(string parentRegister, int shift, uint mask, string valueExpression)
    {
        var shiftedMask = mask << shift;
        var preservedMask = FullRegisterMask ^ shiftedMask;
        var shiftedValue = shift == LowByteShift
            ? $"({valueExpression} & 0x{mask:X}u)"
            : $"(({valueExpression} & 0x{mask:X}u) << {shift})";

        return $"{parentRegister} = ({parentRegister} & 0x{preservedMask:X}u) | {shiftedValue};";
    }

    private static bool TryGetPartialRegisterInfo(string registerName, out string parentRegister, out int shift, out uint mask)
    {
        switch (registerName)
        {
            case "AL":
                parentRegister = "EAX";
                shift = LowByteShift;
                mask = ByteMask;
                return true;
            case "AH":
                parentRegister = "EAX";
                shift = HighByteShift;
                mask = ByteMask;
                return true;
            case "AX":
                parentRegister = "EAX";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            case "BL":
                parentRegister = "EBX";
                shift = LowByteShift;
                mask = ByteMask;
                return true;
            case "BH":
                parentRegister = "EBX";
                shift = HighByteShift;
                mask = ByteMask;
                return true;
            case "BX":
                parentRegister = "EBX";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            case "CL":
                parentRegister = "ECX";
                shift = LowByteShift;
                mask = ByteMask;
                return true;
            case "CH":
                parentRegister = "ECX";
                shift = HighByteShift;
                mask = ByteMask;
                return true;
            case "CX":
                parentRegister = "ECX";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            case "DL":
                parentRegister = "EDX";
                shift = LowByteShift;
                mask = ByteMask;
                return true;
            case "DH":
                parentRegister = "EDX";
                shift = HighByteShift;
                mask = ByteMask;
                return true;
            case "DX":
                parentRegister = "EDX";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            case "SI":
                parentRegister = "ESI";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            case "DI":
                parentRegister = "EDI";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            case "BP":
                parentRegister = "EBP";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            case "SP":
                parentRegister = "ESP";
                shift = LowByteShift;
                mask = WordMask;
                return true;
            default:
                parentRegister = string.Empty;
                shift = LowByteShift;
                mask = 0;
                return false;
        }
    }

    private static bool IsSegmentRegister(string registerName)
    {
        return registerName is "CS" or "DS" or "ES" or "FS" or "GS" or "SS";
    }

    /// <summary>
    /// Returns the C# boolean expression that evaluates the given <see cref="FlagCondition"/>.
    /// </summary>
    private static string GenerateFlagConditionExpression(FlagCondition condition)
    {
        return condition switch
        {
            FlagCondition.Equal           => "ZF",
            FlagCondition.NotEqual        => "!ZF",
            FlagCondition.Below           => "CF",
            FlagCondition.AboveOrEqual    => "!CF",
            FlagCondition.BelowOrEqual    => "(CF || ZF)",
            FlagCondition.Above           => "(!CF && !ZF)",
            FlagCondition.Sign            => "SF",
            FlagCondition.NotSign         => "!SF",
            FlagCondition.Overflow        => "OF",
            FlagCondition.NotOverflow     => "!OF",
            FlagCondition.Less            => "(SF != OF)",
            FlagCondition.LessOrEqual     => "(ZF || SF != OF)",
            FlagCondition.Greater         => "(!ZF && SF == OF)",
            FlagCondition.GreaterOrEqual  => "(SF == OF)",
            FlagCondition.Parity          => "PF",
            FlagCondition.NotParity       => "!PF",
            _                             => "false"
        };
    }

    /// <summary>
    /// Generates C# code that updates the CPU flag variables (ZF, SF, CF, OF, PF)
    /// after an arithmetic or logical operation.
    /// </summary>
    private string GenerateFlagUpdate(RtlFlagUpdate flagUpdate)
    {
        var r = ExpressionToString(flagUpdate.Result);
        var l = ExpressionToString(flagUpdate.Left);
        var ri = flagUpdate.Right != null ? ExpressionToString(flagUpdate.Right) : "0u";

        var size = flagUpdate.OperandSize;
        var (signBit, allMask) = size switch
        {
            4 => ("0x80000000u", "0xFFFFFFFFu"),
            2 => ("0x8000u",     "0xFFFFu"),
            1 => ("0x80u",       "0xFFu"),
            _ => throw new ArgumentOutOfRangeException(nameof(flagUpdate), size,
                     "Only operand sizes 1, 2, and 4 are supported for flag generation.")
        };

        var sb = new StringBuilder();
        sb.AppendLine($"{{ // Flag update: {flagUpdate.Operation} @0x{flagUpdate.Offset:X}");

        // Mask operands to the correct size
        if (size < 4)
        {
            sb.AppendLine($"                uint _r = {r} & {allMask};");
            sb.AppendLine($"                uint _l = {l} & {allMask};");
            if (flagUpdate.Right != null)
                sb.AppendLine($"                uint _ri = {ri} & {allMask};");
        }
        else
        {
            sb.AppendLine($"                uint _r = {r};");
            sb.AppendLine($"                uint _l = {l};");
            if (flagUpdate.Right != null)
                sb.AppendLine($"                uint _ri = {ri};");
        }

        // ZF: result is zero
        sb.AppendLine("                ZF = _r == 0u;");

        // SF: sign bit of result is set
        sb.AppendLine($"                SF = (_r & {signBit}) != 0u;");

        // CF
        if (flagUpdate.UpdateCF)
        {
            var cfLine = flagUpdate.Operation switch
            {
                FlagUpdateOperation.Add => "                CF = _r < _l; // unsigned carry",
                FlagUpdateOperation.Sub => "                CF = _l < _ri; // unsigned borrow",
                FlagUpdateOperation.Neg => "                CF = _l != 0u; // non-zero source produces carry",
                _                       => "                CF = false; // AND/OR/XOR clear CF"
            };
            sb.AppendLine(cfLine);
        }

        // OF
        if (flagUpdate.UpdateOF)
        {
            var maxSigned = size == 4 ? "0x7FFFFFFFu" : (size == 2 ? "0x7FFFu" : "0x7Fu");
            var ofLine = flagUpdate.Operation switch
            {
                FlagUpdateOperation.Add => $"                OF = ((~(_l ^ _ri) & (_l ^ _r)) & {signBit}) != 0u;",
                FlagUpdateOperation.Sub => $"                OF = (((_l ^ _ri) & (_l ^ _r)) & {signBit}) != 0u;",
                FlagUpdateOperation.Inc => $"                OF = _l == {maxSigned};",
                FlagUpdateOperation.Dec => $"                OF = _l == {signBit};",
                FlagUpdateOperation.Neg => $"                OF = _l == {signBit}; // NEG of minimum signed value overflows",
                _                       => "                OF = false; // AND/OR/XOR clear OF"
            };
            sb.AppendLine(ofLine);
        }

        // PF: even parity of low byte
        sb.AppendLine("                { var _pv = _r & 0xFFu; _pv ^= _pv >> 4; _pv ^= _pv >> 2; _pv ^= _pv >> 1; PF = (_pv & 1u) == 0u; }");

        sb.Append("            }");
        return sb.ToString();
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
        sb.AppendLine($"                var v1_0 = mem.Read32((ulong)vecAddr);");
        sb.AppendLine($"                var v1_1 = mem.Read32((ulong)vecAddr + 4);");
        sb.AppendLine($"                var v1_2 = mem.Read32((ulong)vecAddr + 8);");
        sb.AppendLine($"                var v1_3 = mem.Read32((ulong)vecAddr + 12);");
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
            sb.AppendLine($"                mem.Write32((ulong)vecAddr, result.GetElement(0));");
            sb.AppendLine($"                mem.Write32((ulong)vecAddr + 4, result.GetElement(1));");
            sb.AppendLine($"                mem.Write32((ulong)vecAddr + 8, result.GetElement(2));");
            sb.AppendLine($"                mem.Write32((ulong)vecAddr + 12, result.GetElement(3));");
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
