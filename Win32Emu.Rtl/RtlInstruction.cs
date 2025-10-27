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
    public RtlExpression Condition { get; set; } = null!;
    public int TargetOffset { get; set; }
    public uint TargetAddress { get; set; }
    
    public override string ToReadableString() => $"if ({Condition}) goto {TargetOffset:X}";
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
    
    public override string ToReadableString() => ReturnValue != null ? $"return {ReturnValue}" : "return";
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
/// SIMD operation: vectorized operation on multiple values
/// </summary>
public class RtlSimdOp : RtlInstruction
{
    public string Operation { get; set; } = "";
    public int VectorSize { get; set; } // Number of elements in vector (4, 8, 16)
    public string Comment { get; set; } = "";
    
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
