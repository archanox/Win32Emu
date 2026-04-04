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
            {
                var dest = GetOperandExpression(insn, 0, block);
                var right = GetOperandExpression(insn, 1, block);
                var size = GetOperandSize(insn, 0);
                var tempLeft = block.NewTemporary();
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = tempLeft,
                    Source = dest
                });
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = dest,
                    Left = dest,
                    Operator = "+",
                    Right = right
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "ADD",
                    Result = dest,
                    Left = tempLeft,
                    Right = right,
                    OperandSize = size
                });
                break;
            }

            case Mnemonic.Sub:
            {
                var dest = GetOperandExpression(insn, 0, block);
                var right = GetOperandExpression(insn, 1, block);
                var size = GetOperandSize(insn, 0);
                var tempLeft = block.NewTemporary();
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = tempLeft,
                    Source = dest
                });
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = dest,
                    Left = dest,
                    Operator = "-",
                    Right = right
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "SUB",
                    Result = dest,
                    Left = tempLeft,
                    Right = right,
                    OperandSize = size
                });
                break;
            }

            case Mnemonic.And:
            {
                var dest = GetOperandExpression(insn, 0, block);
                var right = GetOperandExpression(insn, 1, block);
                var size = GetOperandSize(insn, 0);
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = dest,
                    Left = dest,
                    Operator = "&",
                    Right = right
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "AND",
                    Result = dest,
                    Left = dest,
                    Right = right,
                    OperandSize = size,
                    UpdateCF = false,
                    UpdateOF = false
                });
                break;
            }

            case Mnemonic.Or:
            {
                var dest = GetOperandExpression(insn, 0, block);
                var right = GetOperandExpression(insn, 1, block);
                var size = GetOperandSize(insn, 0);
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = dest,
                    Left = dest,
                    Operator = "|",
                    Right = right
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "OR",
                    Result = dest,
                    Left = dest,
                    Right = right,
                    OperandSize = size,
                    UpdateCF = false,
                    UpdateOF = false
                });
                break;
            }

            case Mnemonic.Xor:
            {
                var dest = GetOperandExpression(insn, 0, block);
                var right = GetOperandExpression(insn, 1, block);
                var size = GetOperandSize(insn, 0);
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = dest,
                    Left = dest,
                    Operator = "^",
                    Right = right
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "XOR",
                    Result = dest,
                    Left = dest,
                    Right = right,
                    OperandSize = size,
                    UpdateCF = false,
                    UpdateOF = false
                });
                break;
            }
                
            case Mnemonic.Shl:
                results.Add(ConvertBinaryOp(insn, block, "<<"));
                break;
                
            case Mnemonic.Shr:
                results.Add(ConvertBinaryOp(insn, block, ">>"));
                break;
                
            case Mnemonic.Cmp:
            {
                // CMP computes left - right for flag purposes only; result is discarded
                var left = GetOperandExpression(insn, 0, block);
                var right = GetOperandExpression(insn, 1, block);
                var size = GetOperandSize(insn, 0);
                var temp = block.NewTemporary();
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = temp,
                    Left = left,
                    Operator = "-",
                    Right = right
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "SUB",
                    Result = temp,
                    Left = left,
                    Right = right,
                    OperandSize = size
                });
                break;
            }

            case Mnemonic.Test:
            {
                // TEST computes left & right for flag purposes only; result is discarded
                var left = GetOperandExpression(insn, 0, block);
                var right = GetOperandExpression(insn, 1, block);
                var size = GetOperandSize(insn, 0);
                var temp = block.NewTemporary();
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = temp,
                    Left = left,
                    Operator = "&",
                    Right = right
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "AND",
                    Result = temp,
                    Left = left,
                    Right = right,
                    OperandSize = size,
                    UpdateCF = false,
                    UpdateOF = false
                });
                break;
            }
                
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
                    FlagCondition = GetFlagCondition(insn.Mnemonic),
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
            {
                var operand = GetOperandExpression(insn, 0, block);
                var size = GetOperandSize(insn, 0);
                var tempOrig = block.NewTemporary();
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = tempOrig,
                    Source = operand
                });
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = operand,
                    Left = operand,
                    Operator = "+",
                    Right = new RtlConstant { Value = 1 }
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "INC",
                    Result = operand,
                    Left = tempOrig,
                    OperandSize = size,
                    UpdateCF = false   // INC does not modify CF
                });
                break;
            }
                
            // DEC - Decrement by 1
            case Mnemonic.Dec:
            {
                var operand = GetOperandExpression(insn, 0, block);
                var size = GetOperandSize(insn, 0);
                var tempOrig = block.NewTemporary();
                results.Add(new RtlAssignment
                {
                    Offset = (int)insn.IP,
                    Destination = tempOrig,
                    Source = operand
                });
                results.Add(new RtlBinaryOp
                {
                    Offset = (int)insn.IP,
                    Destination = operand,
                    Left = operand,
                    Operator = "-",
                    Right = new RtlConstant { Value = 1 }
                });
                results.Add(new RtlFlagUpdate
                {
                    Offset = (int)insn.IP,
                    Operation = "DEC",
                    Result = operand,
                    Left = tempOrig,
                    OperandSize = size,
                    UpdateCF = false   // DEC does not modify CF
                });
                break;
            }
                
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
                    var size = GetOperandSize(insn, 0);
                    var tempOrig = block.NewTemporary();
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = tempOrig,
                        Source = operand
                    });
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = operand,
                        Left = new RtlConstant { Value = 0 },
                        Operator = "-",
                        Right = operand
                    });
                    results.Add(new RtlFlagUpdate
                    {
                        Offset = (int)insn.IP,
                        Operation = "NEG",
                        Result = operand,
                        Left = tempOrig,
                        OperandSize = size
                    });
                }
                break;
                
            // NOT - One's complement negation
            case Mnemonic.Not:
                {
                    var operand = GetOperandExpression(insn, 0, block);
                    var size = GetOperandSize(insn, 0);
                    // Use appropriate mask based on operand size
                    uint mask = size switch
                    {
                        1 => 0xFF,
                        2 => 0xFFFF,
                        _ => 0xFFFFFFFF
                    };
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = operand,
                        Left = operand,
                        Operator = "^",
                        Right = new RtlConstant { Value = mask }
                    });
                }
                break;
                
            // NOP - No operation
            case Mnemonic.Nop:
                results.Add(new RtlNop { Offset = (int)insn.IP });
                break;
                
            // SETO - Set byte on overflow
            case Mnemonic.Seto:
            {
                var flagRef = new RtlFlagReference { Condition = FlagCondition.Overflow };
                var destKind = insn.GetOpKind(0);

                if (destKind == OpKind.Memory)
                {
                    results.Add(new RtlStore
                    {
                        Offset = (int)insn.IP,
                        Address = GetMemoryAddressExpression(insn),
                        Value = flagRef,
                        Size = 1
                    });
                }
                else
                {
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = GetOperandExpression(insn, 0, block),
                        Source = flagRef
                    });
                }
                break;
            }
                
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
            // ROL formula: (val << count) | (val >> (bits - count))
            case Mnemonic.Rol:
                {
                    var dest = GetOperandExpression(insn, 0, block);
                    var count = GetOperandExpression(insn, 1, block);
                    var size = GetOperandSize(insn, 0);
                    var bits = (uint)(size * 8);

                    var tempLeft = block.NewTemporary();
                    var tempRight = block.NewTemporary();
                    var tempCount = block.NewTemporary();

                    // tempCount = bits - count (for the right shift)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempCount,
                        Left = new RtlConstant { Value = bits },
                        Operator = "-",
                        Right = count
                    });

                    // tempLeft = dest << count
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempLeft,
                        Left = dest,
                        Operator = "<<",
                        Right = count
                    });

                    // tempRight = dest >> (bits - count)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempRight,
                        Left = dest,
                        Operator = ">>",
                        Right = tempCount
                    });

                    // dest = tempLeft | tempRight
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = dest,
                        Left = tempLeft,
                        Operator = "|",
                        Right = tempRight
                    });
                }
                break;

            // ROR - Rotate Right
            // Rotates bits right by count, wrapping around
            // ROR formula: (val >> maskedCount) | (val << ((bits - maskedCount) & (bits - 1)))
            case Mnemonic.Ror:
                {
                    var dest = GetOperandExpression(insn, 0, block);
                    var count = GetOperandExpression(insn, 1, block);
                    var size = GetOperandSize(insn, 0);
                    var bits = (uint)(size * 8);
                    var rotateMask = bits - 1;
                    var operandMask = bits == 32 ? uint.MaxValue : ((1u << (int)bits) - 1u);

                    var tempMaskedDest = block.NewTemporary();
                    var tempMaskedCount = block.NewTemporary();
                    var tempRight = block.NewTemporary();
                    var tempLeft = block.NewTemporary();
                    var tempCount = block.NewTemporary();
                    var tempCount2 = block.NewTemporary();
                    var tempCombined = block.NewTemporary();

                    // tempMaskedDest = dest & operandMask
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempMaskedDest,
                        Left = dest,
                        Operator = "&",
                        Right = new RtlConstant { Value = operandMask }
                    });

                    // tempMaskedCount = count & (bits - 1)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempMaskedCount,
                        Left = count,
                        Operator = "&",
                        Right = new RtlConstant { Value = rotateMask }
                    });

                    // tempCount = bits - tempMaskedCount
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempCount,
                        Left = new RtlConstant { Value = bits },
                        Operator = "-",
                        Right = tempMaskedCount
                    });

                    // tempCount2 = tempCount & (bits - 1)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempCount2,
                        Left = tempCount,
                        Operator = "&",
                        Right = new RtlConstant { Value = rotateMask }
                    });

                    // tempRight = tempMaskedDest >> tempMaskedCount
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempRight,
                        Left = tempMaskedDest,
                        Operator = ">>",
                        Right = tempMaskedCount
                    });

                    // tempLeft = tempMaskedDest << tempCount2
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempLeft,
                        Left = tempMaskedDest,
                        Operator = "<<",
                        Right = tempCount2
                    });

                    // tempCombined = tempRight | tempLeft
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempCombined,
                        Left = tempRight,
                        Operator = "|",
                        Right = tempLeft
                    });

                    // dest = tempCombined & operandMask
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = dest,
                        Left = tempCombined,
                        Operator = "&",
                        Right = new RtlConstant { Value = operandMask }
                    });
                }
                break;

            // SAR - Shift Arithmetic Right (sign-extended shift)
            case Mnemonic.Sar:
                {
                    var dest = GetOperandExpression(insn, 0, block);
                    var count = GetOperandExpression(insn, 1, block);
                    var size = GetOperandSize(insn, 0);
                    var bits = (uint)(size * 8);
                    var allMask = size switch
                    {
                        1 => 0xFFu,
                        2 => 0xFFFFu,
                        4 => 0xFFFFFFFFu,
                        _ => 0xFFFFFFFFu
                    };

                    var tempMaskedDest = block.NewTemporary();
                    var tempLogicalShift = block.NewTemporary();
                    var tempSignBit = block.NewTemporary();
                    var tempFillMask = block.NewTemporary();
                    var tempTopClearMask = block.NewTemporary();
                    var tempTopFillBits = block.NewTemporary();
                    var tempSignFill = block.NewTemporary();
                    var tempResult = block.NewTemporary();

                    // tempMaskedDest = dest & allMask
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempMaskedDest,
                        Left = dest,
                        Operator = "&",
                        Right = new RtlConstant { Value = allMask }
                    });

                    // tempLogicalShift = tempMaskedDest >> count
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempLogicalShift,
                        Left = tempMaskedDest,
                        Operator = ">>",
                        Right = count
                    });

                    // tempSignBit = tempMaskedDest >> (bits - 1)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempSignBit,
                        Left = tempMaskedDest,
                        Operator = ">>",
                        Right = new RtlConstant { Value = bits - 1 }
                    });

                    // tempFillMask = 0 - tempSignBit
                    // Produces 0 when sign bit is clear, or all ones when sign bit is set.
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempFillMask,
                        Left = new RtlConstant { Value = 0u },
                        Operator = "-",
                        Right = tempSignBit
                    });

                    // tempTopClearMask = allMask >> count
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempTopClearMask,
                        Left = new RtlConstant { Value = allMask },
                        Operator = ">>",
                        Right = count
                    });

                    // tempTopFillBits = allMask ^ tempTopClearMask
                    // Sets the upper 'count' bits within the operand width.
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempTopFillBits,
                        Left = new RtlConstant { Value = allMask },
                        Operator = "^",
                        Right = tempTopClearMask
                    });

                    // tempSignFill = tempFillMask & tempTopFillBits
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempSignFill,
                        Left = tempFillMask,
                        Operator = "&",
                        Right = tempTopFillBits
                    });

                    // tempResult = tempLogicalShift | tempSignFill
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = tempResult,
                        Left = tempLogicalShift,
                        Operator = "|",
                        Right = tempSignFill
                    });

                    // dest = tempResult & allMask
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = dest,
                        Left = tempResult,
                        Operator = "&",
                        Right = new RtlConstant { Value = allMask }
                    });
                }
                break;

            // SAL - Shift Arithmetic Left (same as SHL)
            case Mnemonic.Sal:
                results.Add(ConvertBinaryOp(insn, block, "<<"));
                break;

            // XCHG - Exchange register/memory with register
            case Mnemonic.Xchg:
                {
                    var destKind = insn.GetOpKind(0);
                    var srcKind = insn.GetOpKind(1);
                    var size = GetOperandSize(insn, 0);

                    var temp1 = block.NewTemporary();
                    var temp2 = block.NewTemporary();

                    // Load dest into temp1
                    if (destKind == OpKind.Memory)
                    {
                        results.Add(new RtlLoad
                        {
                            Offset = (int)insn.IP,
                            Destination = temp1,
                            Address = GetMemoryAddressExpression(insn),
                            Size = size
                        });
                    }
                    else
                    {
                        results.Add(new RtlAssignment
                        {
                            Offset = (int)insn.IP,
                            Destination = temp1,
                            Source = GetOperandExpression(insn, 0, block)
                        });
                    }

                    // Load src into temp2
                    if (srcKind == OpKind.Memory)
                    {
                        results.Add(new RtlLoad
                        {
                            Offset = (int)insn.IP,
                            Destination = temp2,
                            Address = GetMemoryAddressExpression(insn),
                            Size = size
                        });
                    }
                    else
                    {
                        results.Add(new RtlAssignment
                        {
                            Offset = (int)insn.IP,
                            Destination = temp2,
                            Source = GetOperandExpression(insn, 1, block)
                        });
                    }

                    // Store temp2 to dest
                    if (destKind == OpKind.Memory)
                    {
                        results.Add(new RtlStore
                        {
                            Offset = (int)insn.IP,
                            Address = GetMemoryAddressExpression(insn),
                            Value = temp2,
                            Size = size
                        });
                    }
                    else
                    {
                        results.Add(new RtlAssignment
                        {
                            Offset = (int)insn.IP,
                            Destination = GetOperandExpression(insn, 0, block),
                            Source = temp2
                        });
                    }

                    // Store temp1 to src
                    if (srcKind == OpKind.Memory)
                    {
                        results.Add(new RtlStore
                        {
                            Offset = (int)insn.IP,
                            Address = GetMemoryAddressExpression(insn),
                            Value = temp1,
                            Size = size
                        });
                    }
                    else
                    {
                        results.Add(new RtlAssignment
                        {
                            Offset = (int)insn.IP,
                            Destination = GetOperandExpression(insn, 1, block),
                            Source = temp1
                        });
                    }
                }
                break;

            // MUL - Unsigned Multiply
            // Simplified: Only handles basic cases, doesn't set flags properly
            // Known limitations:
            // - For 32-bit: EDX is set to 0 instead of high 32 bits (RTL lacks 64-bit arithmetic)
            // - For 8-bit: Should use AL->AX (AH:AL), but uses EAX (incorrect implicit operands)
            // - For 16-bit: Should use AX->DX:AX, but uses EAX (incorrect implicit operands)
            // Full implementation would require RTL extensions for 64-bit types and proper register size handling
            case Mnemonic.Mul:
                {
                    var src = GetOperandExpression(insn, 0, block);
                    var size = GetOperandSize(insn, 0);

                    if (size == 4)
                    {
                        // 32-bit: EDX:EAX = EAX * src
                        var temp = block.NewTemporary();

                        // temp = EAX * src
                        results.Add(new RtlBinaryOp
                        {
                            Offset = (int)insn.IP,
                            Destination = temp,
                            Left = new RtlRegister { Name = "EAX" },
                            Operator = "*",
                            Right = src
                        });

                        // EAX = low 32 bits
                        results.Add(new RtlAssignment
                        {
                            Offset = (int)insn.IP,
                            Destination = new RtlRegister { Name = "EAX" },
                            Source = temp
                        });

                        // EDX = 0 (simplified - should be high 32 bits of 64-bit product)
                        results.Add(new RtlAssignment
                        {
                            Offset = (int)insn.IP,
                            Destination = new RtlRegister { Name = "EDX" },
                            Source = new RtlConstant { Value = 0 }
                        });
                    }
                    else
                    {
                        // For 8-bit and 16-bit, simplified version
                        // Note: Uses EAX incorrectly (should be AL->AX for 8-bit, AX->DX:AX for 16-bit)
                        results.Add(new RtlBinaryOp
                        {
                            Offset = (int)insn.IP,
                            Destination = new RtlRegister { Name = "EAX" },
                            Left = new RtlRegister { Name = "EAX" },
                            Operator = "*",
                            Right = src
                        });
                    }
                }
                break;

            // IMUL - Signed Multiply
            // Known limitations for single-operand form:
            // - Uses unsigned * operator (RTL lacks signed multiply)
            // - For 32-bit: Doesn't set EDX to high 32 bits (same issue as MUL)
            // - For 8-bit: Should use AL->AX, but uses EAX (incorrect implicit operands)
            // - For 16-bit: Should use AX->DX:AX, but uses EAX (incorrect implicit operands)
            // Two and three operand forms work correctly within RTL's unsigned arithmetic limitations
            case Mnemonic.Imul:
                {
                    if (insn.OpCount == 1)
                    {
                        // Single operand form: like MUL
                        // Note: Should set EDX:EAX (or DX:AX, or AH:AL) but simplified to only set EAX
                        var src = GetOperandExpression(insn, 0, block);
                        results.Add(new RtlBinaryOp
                        {
                            Offset = (int)insn.IP,
                            Destination = new RtlRegister { Name = "EAX" },
                            Left = new RtlRegister { Name = "EAX" },
                            Operator = "*",
                            Right = src
                        });
                    }
                    else if (insn.OpCount == 2)
                    {
                        // Two operand form: dest = dest * src
                        results.Add(ConvertBinaryOp(insn, block, "*"));
                    }
                    else if (insn.OpCount == 3)
                    {
                        // Three operand form: dest = src1 * src2
                        var dest = GetOperandExpression(insn, 0, block);
                        var src1 = GetOperandExpression(insn, 1, block);
                        var src2 = GetOperandExpression(insn, 2, block);

                        results.Add(new RtlBinaryOp
                        {
                            Offset = (int)insn.IP,
                            Destination = dest,
                            Left = src1,
                            Operator = "*",
                            Right = src2
                        });
                    }
                }
                break;

            // DIV - Unsigned Divide
            // Simplified: Only handles 32-bit division (EDX:EAX divided by src)
            // Note: Full implementation would need to construct 64-bit dividend from EDX:EAX
            case Mnemonic.Div:
                {
                    var src = GetOperandExpression(insn, 0, block);
                    var tempDividend = block.NewTemporary();

                    // Preserve original EAX value for remainder calculation
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = tempDividend,
                        Source = new RtlRegister { Name = "EAX" }
                    });

                    // EAX = EAX / src (quotient)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "EAX" },
                        Left = tempDividend,
                        Operator = "/",
                        Right = src
                    });

                    // EDX = tempDividend % src (remainder)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "EDX" },
                        Left = tempDividend,
                        Operator = "%",
                        Right = src
                    });
                }
                break;

            // IDIV - Signed Divide
            // Note: RTL uses uint registers, so signed semantics are approximated.
            // Full implementation would require signed cast and proper EDX:EAX dividend construction.
            case Mnemonic.Idiv:
                {
                    var src = GetOperandExpression(insn, 0, block);
                    var tempDividend = block.NewTemporary();

                    // Preserve original EAX value for remainder calculation
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = tempDividend,
                        Source = new RtlRegister { Name = "EAX" }
                    });

                    // EAX = EAX / src (quotient)
                    // Note: This is unsigned division on uint; proper IDIV needs signed handling
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "EAX" },
                        Left = tempDividend,
                        Operator = "/",
                        Right = src
                    });

                    // EDX = tempDividend % src (remainder)
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "EDX" },
                        Left = tempDividend,
                        Operator = "%",
                        Right = src
                    });
                }
                break;

            // CDQ - Convert Doubleword to Quadword
            // Sign-extends EAX into EDX:EAX
            case Mnemonic.Cdq:
                {
                    // If EAX < 0 (sign bit set), EDX = 0xFFFFFFFF, else EDX = 0
                    // Simplified version: always set EDX to 0
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "EDX" },
                        Source = new RtlConstant { Value = 0 }
                    });
                }
                break;

            // CWD - Convert Word to Doubleword
            // Sign-extends AX into DX:AX
            case Mnemonic.Cwd:
                {
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "DX" },
                        Source = new RtlConstant { Value = 0 }
                    });
                }
                break;

            // CBW - Convert Byte to Word
            // Sign-extends AL into AX
            case Mnemonic.Cbw:
                {
                    // Simplified: just copy AL to AX
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "AX" },
                        Source = new RtlRegister { Name = "AL" }
                    });
                }
                break;

            // CWDE - Convert Word to Doubleword Extended
            // Sign-extends AX into EAX
            case Mnemonic.Cwde:
                {
                    // Simplified: just copy AX to EAX
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "EAX" },
                        Source = new RtlRegister { Name = "AX" }
                    });
                }
                break;

            // BSWAP - Byte Swap (reverse byte order)
            case Mnemonic.Bswap:
                {
                    var dest = GetOperandExpression(insn, 0, block);
                    var t0 = block.NewTemporary();
                    var t1 = block.NewTemporary();
                    var t2 = block.NewTemporary();
                    var t3 = block.NewTemporary();

                    // Extract bytes: t0 = byte0, t1 = byte1, t2 = byte2, t3 = byte3
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = t0,
                        Left = dest,
                        Operator = "&",
                        Right = new RtlConstant { Value = 0xFF }
                    });

                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = t1,
                        Left = new RtlBinaryExpression
                        {
                            Left = dest,
                            Operator = ">>",
                            Right = new RtlConstant { Value = 8 }
                        },
                        Operator = "&",
                        Right = new RtlConstant { Value = 0xFF }
                    });

                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = t2,
                        Left = new RtlBinaryExpression
                        {
                            Left = dest,
                            Operator = ">>",
                            Right = new RtlConstant { Value = 16 }
                        },
                        Operator = "&",
                        Right = new RtlConstant { Value = 0xFF }
                    });

                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = t3,
                        Left = dest,
                        Operator = ">>",
                        Right = new RtlConstant { Value = 24 }
                    });

                    // Reassemble in reverse order: dest = (t0 << 24) | (t1 << 16) | (t2 << 8) | t3
                    var temp = block.NewTemporary();
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = temp,
                        Left = t0,
                        Operator = "<<",
                        Right = new RtlConstant { Value = 24 }
                    });

                    var temp2 = block.NewTemporary();
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = temp2,
                        Left = t1,
                        Operator = "<<",
                        Right = new RtlConstant { Value = 16 }
                    });

                    var temp3 = block.NewTemporary();
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = temp3,
                        Left = temp,
                        Operator = "|",
                        Right = temp2
                    });

                    var temp4 = block.NewTemporary();
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = temp4,
                        Left = t2,
                        Operator = "<<",
                        Right = new RtlConstant { Value = 8 }
                    });

                    var temp5 = block.NewTemporary();
                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = temp5,
                        Left = temp3,
                        Operator = "|",
                        Right = temp4
                    });

                    results.Add(new RtlBinaryOp
                    {
                        Offset = (int)insn.IP,
                        Destination = dest,
                        Left = temp5,
                        Operator = "|",
                        Right = t3
                    });
                }
                break;

            // LEAVE - High-level procedure exit
            // Equivalent to: MOV ESP, EBP; POP EBP
            case Mnemonic.Leave:
                {
                    // ESP = EBP
                    results.Add(new RtlAssignment
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "ESP" },
                        Source = new RtlRegister { Name = "EBP" }
                    });

                    // EBP = [ESP]; ESP += 4
                    results.Add(new RtlLoad
                    {
                        Offset = (int)insn.IP,
                        Destination = new RtlRegister { Name = "EBP" },
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
                }
                break;

            // SETCC - Set byte on condition
            // Sets destination byte to 1 if condition is true, 0 otherwise
            case Mnemonic.Sete:
            case Mnemonic.Setne:
            case Mnemonic.Seta:
            case Mnemonic.Setae:
            case Mnemonic.Setb:
            case Mnemonic.Setbe:
            case Mnemonic.Setg:
            case Mnemonic.Setge:
            case Mnemonic.Setl:
            case Mnemonic.Setle:
            case Mnemonic.Sets:
            case Mnemonic.Setns:
            case Mnemonic.Setp:
            case Mnemonic.Setnp:
                {
                    var flagRef = new RtlFlagReference { Condition = GetFlagConditionForSetcc(insn.Mnemonic) };
                    var destKind = insn.GetOpKind(0);

                    if (destKind == OpKind.Memory)
                    {
                        results.Add(new RtlStore
                        {
                            Offset = (int)insn.IP,
                            Address = GetMemoryAddressExpression(insn),
                            Value = flagRef,
                            Size = 1
                        });
                    }
                    else
                    {
                        results.Add(new RtlAssignment
                        {
                            Offset = (int)insn.IP,
                            Destination = GetOperandExpression(insn, 0, block),
                            Source = flagRef
                        });
                    }
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
    
    /// <summary>
    /// Maps a conditional jump mnemonic to the corresponding <see cref="FlagCondition"/>.
    /// </summary>
    private static FlagCondition GetFlagCondition(Mnemonic mnemonic)
    {
        return mnemonic switch
        {
            Mnemonic.Je  => FlagCondition.Equal,
            Mnemonic.Jne => FlagCondition.NotEqual,
            Mnemonic.Jl  => FlagCondition.Less,
            Mnemonic.Jle => FlagCondition.LessOrEqual,
            Mnemonic.Jg  => FlagCondition.Greater,
            Mnemonic.Jge => FlagCondition.GreaterOrEqual,
            Mnemonic.Ja  => FlagCondition.Above,
            Mnemonic.Jae => FlagCondition.AboveOrEqual,
            Mnemonic.Jb  => FlagCondition.Below,
            Mnemonic.Jbe => FlagCondition.BelowOrEqual,
            Mnemonic.Jo  => FlagCondition.Overflow,
            Mnemonic.Jno => FlagCondition.NotOverflow,
            Mnemonic.Js  => FlagCondition.Sign,
            Mnemonic.Jns => FlagCondition.NotSign,
            _            => FlagCondition.None
        };
    }

    /// <summary>
    /// Maps a SETCC mnemonic to the corresponding <see cref="FlagCondition"/>.
    /// </summary>
    private static FlagCondition GetFlagConditionForSetcc(Mnemonic mnemonic)
    {
        return mnemonic switch
        {
            Mnemonic.Sete  => FlagCondition.Equal,
            Mnemonic.Setne => FlagCondition.NotEqual,
            Mnemonic.Setl  => FlagCondition.Less,
            Mnemonic.Setle => FlagCondition.LessOrEqual,
            Mnemonic.Setg  => FlagCondition.Greater,
            Mnemonic.Setge => FlagCondition.GreaterOrEqual,
            Mnemonic.Seta  => FlagCondition.Above,
            Mnemonic.Setae => FlagCondition.AboveOrEqual,
            Mnemonic.Setb  => FlagCondition.Below,
            Mnemonic.Setbe => FlagCondition.BelowOrEqual,
            Mnemonic.Seto  => FlagCondition.Overflow,
            Mnemonic.Sets  => FlagCondition.Sign,
            Mnemonic.Setns => FlagCondition.NotSign,
            Mnemonic.Setp  => FlagCondition.Parity,
            Mnemonic.Setnp => FlagCondition.NotParity,
            _              => FlagCondition.None
        };
    }
}
