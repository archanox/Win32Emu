using Iced.Intel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Rtl;

/// <summary>
/// Converts x86 instructions to RTL intermediate representation.
/// Clean-room implementation - no Reko code used.
/// Enables optimization and analysis before code generation.
/// </summary>
public class X86ToRtlConverter
{
    private readonly ILogger _logger;
    private int _nextTempId = 0;
    
    public X86ToRtlConverter(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }
    
    /// <summary>
    /// Convert a sequence of x86 instructions to RTL
    /// </summary>
    public RtlCodeBlock Convert(uint startAddress, List<Instruction> instructions)
    {
        var codeBlock = new RtlCodeBlock
        {
            StartAddress = startAddress
        };
        
        var basicBlock = new RtlBasicBlock
        {
            StartOffset = (int)startAddress
        };
        
        foreach (var insn in instructions)
        {
            var rtlInstructions = ConvertInstruction(insn, codeBlock);
            basicBlock.Instructions.AddRange(rtlInstructions);
            
            // Split into new basic block on control flow changes
            if (IsControlFlow(insn))
            {
                codeBlock.BasicBlocks.Add(basicBlock);
                basicBlock = new RtlBasicBlock
                {
                    StartOffset = (int)(insn.NextIP)
                };
            }
        }
        
        // Add final block if it has instructions
        if (basicBlock.Instructions.Count > 0)
        {
            codeBlock.BasicBlocks.Add(basicBlock);
        }
        
        return codeBlock;
    }
    
    private List<RtlInstruction> ConvertInstruction(Instruction insn, RtlCodeBlock block)
    {
        var results = new List<RtlInstruction>();
        
        switch (insn.Mnemonic)
        {
            case Mnemonic.Mov:
                results.Add(ConvertMov(insn, block));
                break;
                
            case Mnemonic.Add:
                results.Add(ConvertBinaryOp(insn, block, "+"));
                break;
                
            case Mnemonic.Sub:
                results.Add(ConvertBinaryOp(insn, block, "-"));
                break;
                
            case Mnemonic.And:
                results.Add(ConvertBinaryOp(insn, block, "&"));
                break;
                
            case Mnemonic.Or:
                results.Add(ConvertBinaryOp(insn, block, "|"));
                break;
                
            case Mnemonic.Xor:
                results.Add(ConvertBinaryOp(insn, block, "^"));
                break;
                
            case Mnemonic.Shl:
                results.Add(ConvertBinaryOp(insn, block, "<<"));
                break;
                
            case Mnemonic.Shr:
                results.Add(ConvertBinaryOp(insn, block, ">>"));
                break;
                
            case Mnemonic.Cmp:
                // CMP sets flags - model as temporary for condition
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = new RtlRegister { Name = "FLAGS" },
                    Source = new RtlBinaryExpression
                    {
                        Left = GetOperandExpression(insn, 0, block),
                        Operator = "-",
                        Right = GetOperandExpression(insn, 1, block)
                    }
                });
                break;
                
            case Mnemonic.Test:
                // TEST sets flags - model as temporary for condition
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = new RtlRegister { Name = "FLAGS" },
                    Source = new RtlBinaryExpression
                    {
                        Left = GetOperandExpression(insn, 0, block),
                        Operator = "&",
                        Right = GetOperandExpression(insn, 1, block)
                    }
                });
                break;
                
            case Mnemonic.Jmp:
                results.Add(new RtlGoto
                {
                    Offset = (int)insn.IP,
                    TargetOffset = (int)GetBranchTarget(insn)
                });
                break;
                
            case Mnemonic.Je:
            case Mnemonic.Jne:
            case Mnemonic.Jl:
            case Mnemonic.Jle:
            case Mnemonic.Jg:
            case Mnemonic.Jge:
                results.Add(new RtlBranch
                {
                    Offset = (int)insn.IP,
                    Condition = new RtlBinaryExpression
                    {
                        Left = new RtlRegister { Name = "FLAGS" },
                        Operator = GetConditionOperator(insn.Mnemonic),
                        Right = new RtlConstant { Value = 0 }
                    },
                    TargetOffset = (int)GetBranchTarget(insn)
                });
                break;
                
            case Mnemonic.Call:
                results.Add(new RtlCall
                {
                    Offset = (int)insn.IP,
                    Target = GetOperandExpression(insn, 0, block),
                    ReturnValue = new RtlRegister { Name = "EAX" } // Stdcall return
                });
                break;
                
            case Mnemonic.Ret:
                results.Add(new RtlReturn
                {
                    Offset = (int)insn.IP,
                    ReturnValue = new RtlRegister { Name = "EAX" }
                });
                break;
                
            case Mnemonic.Push:
                // PUSH = ESP -= 4; mem[ESP] = value
                var pushTemp = block.NewTemporary();
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = new RtlRegister { Name = "ESP" },
                    Left = new RtlRegister { Name = "ESP" },
                    Operator = "-",
                    Right = new RtlConstant { Value = 4 }
                });
                results.Add(new RtlStore
                {
                    Offset = (int)insn.IP,
                    Address = new RtlRegister { Name = "ESP" },
                    Value = GetOperandExpression(insn, 0, block),
                    Size = 4
                });
                break;
                
            case Mnemonic.Pop:
                // POP = value = mem[ESP]; ESP += 4
                results.Add(new RtlLoad
                {
                    Offset = (int)insn.IP,
                    Destination = GetOperandExpression(insn, 0, block),
                    Address = new RtlRegister { Name = "ESP" },
                    Size = 4
                });
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = new RtlRegister { Name = "ESP" },
                    Left = new RtlRegister { Name = "ESP" },
                    Operator = "+",
                    Right = new RtlConstant { Value = 4 }
                });
                break;
                
            default:
                // Unsupported instruction - emit NOP with comment
                _logger.LogWarning("[X86ToRtl] Unsupported instruction: {Mnemonic} at 0x{IP:X}", 
                    insn.Mnemonic, insn.IP);
                results.Add(new RtlNop { Offset = (int)insn.IP });
                break;
        }
        
        return results;
    }
    
    private RtlAssignment ConvertMov(Instruction insn, RtlCodeBlock block)
    {
        return new RtlAssignment
        {
            Offset = (int)insn.IP,
            Destination = GetOperandExpression(insn, 0, block),
            Source = GetOperandExpression(insn, 1, block)
        };
    }
    
    private RtlBinaryOp ConvertBinaryOp(Instruction insn, RtlCodeBlock block, string op)
    {
        var dest = GetOperandExpression(insn, 0, block);
        return new RtlBinaryOp
        {
            Offset = (int)insn.IP,
            Destination = dest,
            Left = dest, // x86 binary ops modify first operand
            Operator = op,
            Right = GetOperandExpression(insn, 1, block)
        };
    }
    
    private RtlExpression GetOperandExpression(Instruction insn, int opIndex, RtlCodeBlock block)
    {
        if (opIndex >= insn.OpCount)
            return new RtlConstant { Value = 0 };
            
        switch (insn.GetOpKind(opIndex))
        {
            case OpKind.Register:
                var reg = insn.GetOpRegister(opIndex);
                return new RtlRegister { Name = reg.ToString().ToUpper() };
                
            case OpKind.Immediate8:
            case OpKind.Immediate16:
            case OpKind.Immediate32:
                return new RtlConstant { Value = (uint)insn.GetImmediate(opIndex) };
                
            case OpKind.Memory:
                // Simplified memory operand handling
                var baseReg = insn.MemoryBase;
                var indexReg = insn.MemoryIndex;
                var disp = (uint)insn.MemoryDisplacement64;
                
                RtlExpression addr;
                if (baseReg != Register.None)
                {
                    addr = new RtlRegister { Name = baseReg.ToString().ToUpper() };
                    if (disp != 0)
                    {
                        addr = new RtlBinaryExpression
                        {
                            Left = addr,
                            Operator = "+",
                            Right = new RtlConstant { Value = disp }
                        };
                    }
                }
                else
                {
                    addr = new RtlConstant { Value = disp };
                }
                
                return addr;
                
            case OpKind.NearBranch16:
            case OpKind.NearBranch32:
                return new RtlConstant { Value = (uint)insn.NearBranchTarget };
                
            default:
                _logger.LogWarning("[X86ToRtl] Unsupported operand kind: {Kind}", insn.GetOpKind(opIndex));
                return new RtlConstant { Value = 0 };
        }
    }
    
    private ulong GetBranchTarget(Instruction insn)
    {
        if (insn.Op0Kind == OpKind.NearBranch32)
            return insn.NearBranch32;
        if (insn.Op0Kind == OpKind.NearBranch16)
            return insn.NearBranch16;
        return 0;
    }
    
    private bool IsControlFlow(Instruction insn)
    {
        return insn.Mnemonic switch
        {
            Mnemonic.Jmp or Mnemonic.Je or Mnemonic.Jne or
            Mnemonic.Jl or Mnemonic.Jle or Mnemonic.Jg or Mnemonic.Jge or
            Mnemonic.Call or Mnemonic.Ret => true,
            _ => false
        };
    }
    
    private string GetConditionOperator(Mnemonic mnemonic)
    {
        return mnemonic switch
        {
            Mnemonic.Je => "==",
            Mnemonic.Jne => "!=",
            Mnemonic.Jl => "<",
            Mnemonic.Jle => "<=",
            Mnemonic.Jg => ">",
            Mnemonic.Jge => ">=",
            _ => "=="
        };
    }
}
