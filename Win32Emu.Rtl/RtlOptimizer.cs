namespace Win32Emu.Rtl;

/// <summary>
/// Optimizes RTL code through various passes.
/// Clean-room implementation for Win32Emu.
/// </summary>
public class RtlOptimizer
{
    /// <summary>
    /// Apply all optimization passes to an RTL code block
    /// </summary>
    public RtlCodeBlock Optimize(RtlCodeBlock block)
    {
        // Pass 1: Remove NOPs
        RemoveNops(block);
        
        // Pass 2: Constant folding
        ConstantFolding(block);
        
        // Pass 3: Dead code elimination
        DeadCodeElimination(block);
        
        // Pass 4: Copy propagation
        CopyPropagation(block);
        
        return block;
    }
    
    /// <summary>
    /// Remove NOP instructions
    /// </summary>
    private void RemoveNops(RtlCodeBlock block)
    {
        foreach (var bb in block.BasicBlocks)
        {
            bb.Instructions.RemoveAll(i => i is RtlNop);
        }
    }
    
    /// <summary>
    /// Fold constant expressions
    /// Example: t1 = 5 + 3 => t1 = 8
    /// </summary>
    private void ConstantFolding(RtlCodeBlock block)
    {
        foreach (var bb in block.BasicBlocks)
        {
            for (int i = 0; i < bb.Instructions.Count; i++)
            {
                if (bb.Instructions[i] is RtlBinaryOp binOp)
                {
                    if (binOp.Left is RtlConstant lc && binOp.Right is RtlConstant rc)
                    {
                        var result = EvaluateConstant(lc.Value, binOp.Operator, rc.Value);
                        bb.Instructions[i] = new RtlAssignment
                        {
                            Offset = binOp.Offset,
                            Destination = binOp.Destination,
                            Source = new RtlConstant { Value = result }
                        };
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Remove assignments to temporaries that are never read
    /// </summary>
    private void DeadCodeElimination(RtlCodeBlock block)
    {
        // Simple approach: track temporary usage
        var usedTemps = new HashSet<int>();
        
        // First pass: find all used temporaries
        foreach (var bb in block.BasicBlocks)
        {
            foreach (var insn in bb.Instructions)
            {
                CollectUsedTemporaries(insn, usedTemps);
            }
        }
        
        // Second pass: remove assignments to unused temporaries
        foreach (var bb in block.BasicBlocks)
        {
            bb.Instructions.RemoveAll(insn =>
            {
                if (insn is RtlAssignment assign && assign.Destination is RtlTemporary temp)
                {
                    return !usedTemps.Contains(temp.Id);
                }
                return false;
            });
        }
    }
    
    /// <summary>
    /// Propagate copies: if t1 = t2 and t2 is constant, replace uses of t1 with constant
    /// </summary>
    private void CopyPropagation(RtlCodeBlock block)
    {
        var copies = new Dictionary<string, RtlExpression>();
        
        foreach (var bb in block.BasicBlocks)
        {
            copies.Clear();
            
            for (int i = 0; i < bb.Instructions.Count; i++)
            {
                var insn = bb.Instructions[i];
                
                // Track copies
                if (insn is RtlAssignment assign)
                {
                    var destKey = GetExpressionKey(assign.Destination);
                    if (destKey != null && assign.Source is RtlConstant)
                    {
                        copies[destKey] = assign.Source;
                    }
                }
                
                // Replace uses of copied values
                bb.Instructions[i] = PropagateInInstruction(insn, copies);
            }
        }
    }
    
    private RtlInstruction PropagateInInstruction(RtlInstruction insn, Dictionary<string, RtlExpression> copies)
    {
        return insn switch
        {
            RtlAssignment assign => new RtlAssignment
            {
                Offset = assign.Offset,
                Destination = assign.Destination,
                Source = PropagateInExpression(assign.Source, copies)
            },
            RtlBinaryOp binOp => new RtlBinaryOp
            {
                Offset = binOp.Offset,
                Destination = binOp.Destination,
                Left = PropagateInExpression(binOp.Left, copies),
                Operator = binOp.Operator,
                Right = PropagateInExpression(binOp.Right, copies)
            },
            _ => insn
        };
    }
    
    private RtlExpression PropagateInExpression(RtlExpression expr, Dictionary<string, RtlExpression> copies)
    {
        var key = GetExpressionKey(expr);
        if (key != null && copies.TryGetValue(key, out var replacement))
        {
            return replacement;
        }
        
        if (expr is RtlBinaryExpression binExpr)
        {
            return new RtlBinaryExpression
            {
                Left = PropagateInExpression(binExpr.Left, copies),
                Operator = binExpr.Operator,
                Right = PropagateInExpression(binExpr.Right, copies)
            };
        }
        
        return expr;
    }
    
    private void CollectUsedTemporaries(RtlInstruction insn, HashSet<int> usedTemps)
    {
        switch (insn)
        {
            case RtlAssignment assign:
                CollectFromExpression(assign.Source, usedTemps);
                break;
            case RtlBinaryOp binOp:
                CollectFromExpression(binOp.Left, usedTemps);
                CollectFromExpression(binOp.Right, usedTemps);
                break;
            case RtlBranch branch:
                CollectFromExpression(branch.Condition, usedTemps);
                break;
            case RtlStore store:
                CollectFromExpression(store.Address, usedTemps);
                CollectFromExpression(store.Value, usedTemps);
                break;
            case RtlLoad load:
                CollectFromExpression(load.Address, usedTemps);
                break;
        }
    }
    
    private void CollectFromExpression(RtlExpression expr, HashSet<int> usedTemps)
    {
        if (expr is RtlTemporary temp)
        {
            usedTemps.Add(temp.Id);
        }
        else if (expr is RtlBinaryExpression binExpr)
        {
            CollectFromExpression(binExpr.Left, usedTemps);
            CollectFromExpression(binExpr.Right, usedTemps);
        }
        else if (expr is RtlUnaryExpression unExpr)
        {
            CollectFromExpression(unExpr.Operand, usedTemps);
        }
    }
    
    private string? GetExpressionKey(RtlExpression expr)
    {
        return expr switch
        {
            RtlRegister reg => reg.Name,
            RtlTemporary temp => $"t{temp.Id}",
            _ => null
        };
    }
    
    private uint EvaluateConstant(uint left, string op, uint right)
    {
        return op switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" => right != 0 ? left / right : 0,
            "&" => left & right,
            "|" => left | right,
            "^" => left ^ right,
            "<<" => left << (int)right,
            ">>" => left >> (int)right,
            _ => left
        };
    }
}
