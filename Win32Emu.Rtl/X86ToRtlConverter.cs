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
        
        // Track the end address (address after the last instruction)
        uint endAddress = startAddress;
        
        foreach (var insn in instructions)
        {
            var rtlInstructions = ConvertInstruction(insn, codeBlock);
            basicBlock.Instructions.AddRange(rtlInstructions);
            
            // Update end address to be after this instruction
            endAddress = (uint)insn.NextIP;
            
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
        
        // Set the end address (address of instruction following this block)
        codeBlock.EndAddress = endAddress;
        
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
            case Mnemonic.Ja:   // Jump if above (unsigned)
            case Mnemonic.Jae:  // Jump if above or equal (unsigned)
            case Mnemonic.Jb:   // Jump if below (unsigned)
            case Mnemonic.Jbe:  // Jump if below or equal (unsigned)
            case Mnemonic.Jo:   // Jump if overflow
            case Mnemonic.Jno:  // Jump if not overflow
            case Mnemonic.Js:   // Jump if sign
            case Mnemonic.Jns:  // Jump if not sign
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
                    ReturnValue = new RtlRegister { Name = "EAX" }, // Stdcall return
                    ReturnAddress = (uint)insn.NextIP // Address of instruction after the CALL
                });
                break;
                
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
            
            // INC - Increment by 1
            case Mnemonic.Inc:
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = GetOperandExpression(insn, 0, block),
                    Left = GetOperandExpression(insn, 0, block),
                    Operator = "+",
                    Right = new RtlConstant { Value = 1 }
                });
                break;
                
            // DEC - Decrement by 1
            case Mnemonic.Dec:
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = GetOperandExpression(insn, 0, block),
                    Left = GetOperandExpression(insn, 0, block),
                    Operator = "-",
                    Right = new RtlConstant { Value = 1 }
                });
                break;
                
            // ADC - Add with Carry (simplified: ignores carry flag)
            case Mnemonic.Adc:
                results.Add(ConvertBinaryOp(insn, block, "+"));
                break;
                
            // SBB - Subtract with Borrow (simplified: ignores borrow flag)
            case Mnemonic.Sbb:
                results.Add(ConvertBinaryOp(insn, block, "-"));
                break;
                
            // LEA - Load Effective Address
            case Mnemonic.Lea:
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = GetOperandExpression(insn, 0, block),
                    Source = GetMemoryAddressExpression(insn)
                });
                break;
                
            // MOVZX - Move with Zero-Extend
            case Mnemonic.Movzx:
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = GetOperandExpression(insn, 0, block),
                    Source = GetOperandExpression(insn, 1, block)
                });
                break;
                
            // MOVSX - Move with Sign-Extend
            case Mnemonic.Movsx:
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = GetOperandExpression(insn, 0, block),
                    Source = GetOperandExpression(insn, 1, block)
                });
                break;
                
            // NEG - Two's complement negation
            case Mnemonic.Neg:
                {
                    var operand = GetOperandExpression(insn, 0, block);
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = operand,
                        Left = new RtlConstant { Value = 0 },
                        Operator = "-",
                        Right = operand
                    });
                }
                break;
                
            // NOT - One's complement negation
            case Mnemonic.Not:
                {
                    var operand = GetOperandExpression(insn, 0, block);
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = operand,
                        Left = operand,
                        Operator = "^",
                        Right = new RtlConstant { Value = 0xFFFFFFFF }
                    });
                }
                break;
                
            // NOP - No operation
            case Mnemonic.Nop:
                results.Add(new RtlNop { Offset = (int)insn.IP });
                break;
                
            // SETO - Set byte on overflow
            // Sets destination byte to 1 if overflow flag is set, 0 otherwise
            case Mnemonic.Seto:
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = GetOperandExpression(insn, 0, block),
                    Source = new RtlBinaryExpression
                    {
                        // Check if overflow flag is set (simplified: always 0 for now)
                        // Full overflow flag tracking would require proper flag modeling
                        Left = new RtlConstant { Value = 0 },
                        Operator = "!=",
                        Right = new RtlConstant { Value = 0 }
                    }
                });
                break;
                
            // XADD - Exchange and Add
            // TEMP = DEST; DEST = DEST + SRC; SRC = TEMP
            case Mnemonic.Xadd:
                {
                    var dest = GetOperandExpression(insn, 0, block);
                    var src = GetOperandExpression(insn, 1, block);
                    var temp = block.NewTemporary();
                    
                    // temp = dest (save original destination)
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = temp,
                        Source = dest
                    });
                    
                    // dest = dest + src
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = dest,
                        Left = dest,
                        Operator = "+",
                        Right = src
                    });
                    
                    // src = temp (original dest value)
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = src,
                        Source = temp
                    });
                }
                break;
                
            // ROL - Rotate Left
            // Rotates bits left by count, wrapping around
            case Mnemonic.Rol:
                {
                    var dest = GetOperandExpression(insn, 0, block);
                    var count = GetOperandExpression(insn, 1, block);
                    var size = GetOperandSize(insn, 0);
                    var bits = (uint)(size * 8);
                    
                    // ROL simplified: (val << count) | (val >> (bits - count))
                    // For JIT purposes, we approximate this with a shift operation
                    // Full implementation would need proper bit rotation
                    var temp = block.NewTemporary();
                    
                    // temp = dest << count
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = temp,
                        Left = dest,
                        Operator = "<<",
                        Right = count
                    });
                    
                    // dest = temp (simplified - full ROL would include OR with right-shifted bits)
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = dest,
                        Source = temp
                    });
                }
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
    
    private RtlInstruction ConvertMov(Instruction insn, RtlCodeBlock block)
    {
        var destKind = insn.GetOpKind(0);
        var srcKind = insn.GetOpKind(1);
        
        // MOV to memory: use RtlStore
        if (destKind == OpKind.Memory)
        {
            return new RtlStore
            {
                Offset = (int)insn.IP,
                Address = GetMemoryAddressExpression(insn),
                Value = GetOperandExpression(insn, 1, block),
                Size = GetOperandSize(insn, 0)
            };
        }
        
        // MOV from memory: use RtlLoad
        if (srcKind == OpKind.Memory)
        {
            return new RtlLoad
            {
                Offset = (int)insn.IP,
                Destination = GetOperandExpression(insn, 0, block),
                Address = GetMemoryAddressExpression(insn),
                Size = GetOperandSize(insn, 1)
            };
        }
        
        // Register to register or immediate to register: use RtlAssignment
        return new RtlAssignment
        {
            Offset = (int)insn.IP,
            Destination = GetOperandExpression(insn, 0, block),
            Source = GetOperandExpression(insn, 1, block)
        };
    }
    
    /// <summary>
    /// Gets the memory address expression for the first memory operand in an instruction.
    /// Supports x86 memory addressing modes:
    /// - Direct: [disp] - displacement only (e.g., MOV EAX, [0x12345678])
    /// - Base: [base] - base register only (e.g., MOV EAX, [EBX])
    /// - Base+Disp: [base+disp] - base plus displacement (e.g., MOV EAX, [EBX+4])
    /// - Base+Index: [base+index] - base plus index (e.g., MOV EAX, [EBX+ECX])
    /// - Base+Index*Scale: [base+index*scale] - SIB addressing (e.g., MOV EAX, [EBX+ECX*4])
    /// - Base+Index*Scale+Disp: [base+index*scale+disp] - full SIB (e.g., MOV EAX, [EBX+ECX*4+8])
    /// </summary>
    private RtlExpression GetMemoryAddressExpression(Instruction insn)
    {
        var baseReg = insn.MemoryBase;
        var indexReg = insn.MemoryIndex;
        var scale = insn.MemoryIndexScale;
        var disp = (uint)insn.MemoryDisplacement64;
        
        RtlExpression addr;
        
        // Start with base register or displacement
        if (baseReg != Register.None)
        {
            addr = new RtlRegister { Name = baseReg.ToString().ToUpper() };
            
            // Add index register if present (with scale)
            if (indexReg != Register.None)
            {
                RtlExpression indexExpr = new RtlRegister { Name = indexReg.ToString().ToUpper() };
                if (scale > 1)
                {
                    indexExpr = new RtlBinaryExpression
                    {
                        Left = indexExpr,
                        Operator = "*",
                        Right = new RtlConstant { Value = (uint)scale }
                    };
                }
                addr = new RtlBinaryExpression
                {
                    Left = addr,
                    Operator = "+",
                    Right = indexExpr
                };
            }
            
            // Add displacement if present
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
        else if (indexReg != Register.None)
        {
            // Index-only addressing (rare)
            addr = new RtlRegister { Name = indexReg.ToString().ToUpper() };
            if (scale > 1)
            {
                addr = new RtlBinaryExpression
                {
                    Left = addr,
                    Operator = "*",
                    Right = new RtlConstant { Value = (uint)scale }
                };
            }
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
            // Direct memory addressing (displacement only)
            addr = new RtlConstant { Value = disp };
        }
        
        return addr;
    }
    
    /// <summary>
    /// Gets the size of an operand in bytes.
    /// </summary>
    private int GetOperandSize(Instruction insn, int opIndex)
    {
        var kind = insn.GetOpKind(opIndex);
        
        switch (kind)
        {
            case OpKind.Register:
                var reg = insn.GetOpRegister(opIndex);
                // Check register size based on name
                if (reg >= Register.EAX && reg <= Register.EDI) return 4;
                if (reg >= Register.AX && reg <= Register.DI) return 2;
                if (reg >= Register.AL && reg <= Register.BH) return 1;
                return 4; // Default to 32-bit
                
            case OpKind.Memory:
                // Use instruction's memory size hint
                return insn.MemorySize switch
                {
                    MemorySize.UInt8 or MemorySize.Int8 => 1,
                    MemorySize.UInt16 or MemorySize.Int16 => 2,
                    MemorySize.UInt32 or MemorySize.Int32 or MemorySize.Float32 => 4,
                    MemorySize.UInt64 or MemorySize.Int64 or MemorySize.Float64 => 8,
                    _ => 4 // Default to 32-bit
                };
                
            case OpKind.Immediate8:
                return 1;
            case OpKind.Immediate16:
                return 2;
            case OpKind.Immediate32:
            default:
                return 4;
        }
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
            Mnemonic.Ja or Mnemonic.Jae or Mnemonic.Jb or Mnemonic.Jbe or
            Mnemonic.Jo or Mnemonic.Jno or Mnemonic.Js or Mnemonic.Jns or
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
            Mnemonic.Ja => ">",   // unsigned greater than (simplified)
            Mnemonic.Jae => ">=", // unsigned greater than or equal
            Mnemonic.Jb => "<",   // unsigned less than
            Mnemonic.Jbe => "<=", // unsigned less than or equal
            Mnemonic.Jo => "!=",  // overflow (simplified to non-zero check)
            Mnemonic.Jno => "==", // not overflow
            Mnemonic.Js => "<",   // sign flag (negative)
            Mnemonic.Jns => ">=", // not sign (positive or zero)
            _ => "=="
        };
    }
}
