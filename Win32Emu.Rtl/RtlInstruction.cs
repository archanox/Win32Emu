namespace Win32Emu.Rtl;

/// <summary>
/// Register Transfer Language intermediate representation.
/// Clean-room implementation for Win32Emu - no Reko code used.
/// Represents low-level operations in a platform-independent format suitable for optimization.
/// </summary>
public abstract class RtlInstruction
{
    public int Offset { get; set; }
    public abstract string ToReadableString();
}

/// <summary>
/// Assignment: dst = src
/// </summary>
public class RtlAssignment : RtlInstruction
{
    public RtlExpression Destination { get; set; } = null!;
    public RtlExpression Source { get; set; } = null!;
    
    public override string ToReadableString() => $"{Destination} = {Source}";
}

/// <summary>
/// Binary operation: dst = left op right
/// </summary>
public class RtlBinaryOp : RtlInstruction
{
    public RtlExpression Destination { get; set; } = null!;
    public RtlExpression Left { get; set; } = null!;
    public string Operator { get; set; } = ""; // +, -, *, /, &, |, ^, <<, >>
    public RtlExpression Right { get; set; } = null!;
    
    public override string ToReadableString() => $"{Destination} = {Left} {Operator} {Right}";
}

/// <summary>
/// Conditional branch: if (condition) goto target
/// </summary>
public class RtlBranch : RtlInstruction
{
    /// <summary>Legacy direct-expression condition. Null when FlagCondition is used.</summary>
    public RtlExpression? Condition { get; set; }
    /// <summary>Flag-based condition. Takes precedence over Condition when not None.</summary>
    public FlagCondition FlagCondition { get; set; } = FlagCondition.None;
    public int TargetOffset { get; set; }
    public uint TargetAddress { get; set; }

    public override string ToReadableString() => FlagCondition != FlagCondition.None
        ? $"if ({FlagCondition}) goto {TargetOffset:X}"
        : $"if ({Condition}) goto {TargetOffset:X}";
}

/// <summary>
/// Unconditional jump: goto target
/// </summary>
public class RtlGoto : RtlInstruction
{
    public int TargetOffset { get; set; }
    
    public override string ToReadableString() => $"goto {TargetOffset:X}";
}

/// <summary>
/// Function call: result = call(target, args...)
/// </summary>
public class RtlCall : RtlInstruction
{
    public RtlExpression? ReturnValue { get; set; }
    public RtlExpression Target { get; set; } = null!;
    public List<RtlExpression> Arguments { get; set; } = new();
    /// <summary>
    /// The address to return to after the call completes (instruction following the CALL)
    /// </summary>
    public uint ReturnAddress { get; set; }
    
    public override string ToReadableString() 
    {
        var args = string.Join(", ", Arguments.Select(a => a.ToString()));
        var ret = ReturnValue != null ? $"{ReturnValue} = " : "";
        return $"{ret}call {Target}({args})";
    }
}

/// <summary>
/// Return from function
/// </summary>
public class RtlReturn : RtlInstruction
{
    public RtlExpression? ReturnValue { get; set; }
    public ushort StackCleanup { get; set; } // For stdcall RET imm16
    
    public override string ToReadableString() => StackCleanup > 0 ? $"return (pop stack + {StackCleanup})" : "return";
}

/// <summary>
/// Memory load: dst = mem[address]
/// </summary>
public class RtlLoad : RtlInstruction
{
    public RtlExpression Destination { get; set; } = null!;
    public RtlExpression Address { get; set; } = null!;
    public int Size { get; set; } // 1, 2, 4 bytes
    
    public override string ToReadableString() => $"{Destination} = mem{Size * 8}[{Address}]";
}

/// <summary>
/// Memory store: mem[address] = value
/// </summary>
public class RtlStore : RtlInstruction
{
    public RtlExpression Address { get; set; } = null!;
    public RtlExpression Value { get; set; } = null!;
    public int Size { get; set; } // 1, 2, 4 bytes
    
    public override string ToReadableString() => $"mem{Size * 8}[{Address}] = {Value}";
}

/// <summary>
/// No operation (placeholder, can be optimized away)
/// </summary>
public class RtlNop : RtlInstruction
{
    public override string ToReadableString() => "nop";
}

/// <summary>
/// Represents the CPU flag condition for a conditional branch or SETCC instruction.
/// </summary>
public enum FlagCondition
{
    None,
    Equal,              // ZF=1  (JE/JZ/SETE)
    NotEqual,           // ZF=0  (JNE/JNZ/SETNE)
    Below,              // CF=1  (JB/JC/SETB)
    AboveOrEqual,       // CF=0  (JAE/JNC/SETAE)
    BelowOrEqual,       // CF=1 or ZF=1  (JBE/SETBE)
    Above,              // CF=0 and ZF=0  (JA/SETA)
    Sign,               // SF=1  (JS/SETS)
    NotSign,            // SF=0  (JNS/SETNS)
    Overflow,           // OF=1  (JO/SETO)
    NotOverflow,        // OF=0  (JNO/SETNO)
    Less,               // SF≠OF  (JL/SETL)
    LessOrEqual,        // ZF=1 or SF≠OF  (JLE/SETLE)
    Greater,            // ZF=0 and SF=OF  (JG/SETG)
    GreaterOrEqual,     // SF=OF  (JGE/SETGE)
    Parity,             // PF=1  (JP/SETP)
    NotParity,          // PF=0  (JNP/SETNP)
}

/// <summary>
/// Flag update: recomputes CPU flags (ZF, SF, CF, OF, PF) after an arithmetic or logical operation.
/// </summary>
public class RtlFlagUpdate : RtlInstruction
{
    /// <summary>Operation type: "ADD", "SUB", "AND", "OR", "XOR", "INC", "DEC", "NEG"</summary>
    public string Operation { get; set; } = "";
    /// <summary>Result of the operation (used for ZF, SF, PF).</summary>
    public RtlExpression Result { get; set; } = null!;
    /// <summary>Left (or only) operand before the operation (used for CF and OF computation).</summary>
    public RtlExpression Left { get; set; } = null!;
    /// <summary>Right operand. Null for unary operations such as INC, DEC, and NEG.</summary>
    public RtlExpression? Right { get; set; }
    /// <summary>Operand size in bytes (1, 2, or 4).</summary>
    public int OperandSize { get; set; } = 4;
    /// <summary>Whether to update the carry flag. False for INC/DEC which do not modify CF.</summary>
    public bool UpdateCF { get; set; } = true;
    /// <summary>Whether to update the overflow flag.</summary>
    public bool UpdateOF { get; set; } = true;

    public override string ToReadableString() => $"update_flags({Operation}, {Result})";
}

/// <summary>
/// A reference to a CPU flag condition, evaluating to 1u when true and 0u when false.
/// Used as the source expression for SETCC instructions.
/// </summary>
public class RtlFlagReference : RtlExpression
{
    public FlagCondition Condition { get; set; }

    public override string ToString() => $"flag({Condition})";
}

/// <summary>
/// SIMD operation: vectorized operation on multiple values
/// </summary>
public class RtlSimdOp : RtlInstruction
{
    public string Operation { get; set; } = "";
    public int VectorSize { get; set; } // Number of elements in vector (4, 8, 16)
    public string Comment { get; set; } = "";
    public RtlExpression? BaseAddress { get; set; } // Base memory address for vector load/store
    public RtlExpression? Destination { get; set; } // Destination for vector operation result
    public RtlExpression? Operand1 { get; set; } // First operand
    public RtlExpression? Operand2 { get; set; } // Second operand (for binary ops)
    public bool IsMemoryOperation { get; set; } // True for vector load/store
    public bool IsStore { get; set; } // True for vector store, false for load
    
    public override string ToReadableString() => $"SIMD[{VectorSize}]: {Operation} // {Comment}";
}

// === RTL Expressions ===

public abstract class RtlExpression
{
    public abstract override string ToString();
}

/// <summary>
/// Register reference (EAX, EBX, etc.)
/// </summary>
public class RtlRegister : RtlExpression
{
    public string Name { get; set; } = "";
    
    public override string ToString() => Name;
}

/// <summary>
/// Constant value
/// </summary>
public class RtlConstant : RtlExpression
{
    public uint Value { get; set; }
    
    public override string ToString() => $"0x{Value:X}";
}

/// <summary>
/// Temporary variable (introduced during conversion)
/// </summary>
public class RtlTemporary : RtlExpression
{
    public int Id { get; set; }
    
    public override string ToString() => $"t{Id}";
}

/// <summary>
/// Binary expression (used in nested expressions)
/// </summary>
public class RtlBinaryExpression : RtlExpression
{
    public RtlExpression Left { get; set; } = null!;
    public string Operator { get; set; } = "";
    public RtlExpression Right { get; set; } = null!;
    
    public override string ToString() => $"({Left} {Operator} {Right})";
}

/// <summary>
/// Unary expression (NOT, NEG, etc.)
/// </summary>
public class RtlUnaryExpression : RtlExpression
{
    public string Operator { get; set; } = ""; // !, -, ~
    public RtlExpression Operand { get; set; } = null!;
    
    public override string ToString() => $"{Operator}{Operand}";
}

/// <summary>
/// A basic block of RTL instructions
/// </summary>
public class RtlBasicBlock
{
    public int StartOffset { get; set; }
    public uint StartAddress { get; set; }
    public List<RtlInstruction> Instructions { get; set; } = new();
    public List<int> Successors { get; set; } = new(); // Block offsets that can follow this one
    
    public override string ToString()
    {
        return $"Block @{StartOffset:X}:\n" + 
               string.Join("\n", Instructions.Select(i => $"  {i.ToReadableString()}"));
    }
}

/// <summary>
/// Complete RTL representation of a code block
/// </summary>
public class RtlCodeBlock
{
    public uint StartAddress { get; set; }
    /// <summary>
    /// The address of the instruction following the last instruction in this block.
    /// Used to set EIP after block execution completes.
    /// </summary>
    public uint EndAddress { get; set; }
    public List<RtlBasicBlock> BasicBlocks { get; set; } = new();
    public Dictionary<string, RtlRegister> LiveRegisters { get; set; } = new();
    public int NextTemporaryId { get; set; } = 0;
    
    public RtlTemporary NewTemporary()
    {
        return new RtlTemporary { Id = NextTemporaryId++ };
    }
    
    public string ToReadableString()
    {
        return $"RTL Block @0x{StartAddress:X}:\n" +
               string.Join("\n\n", BasicBlocks.Select(b => b.ToString()));
    }
}
