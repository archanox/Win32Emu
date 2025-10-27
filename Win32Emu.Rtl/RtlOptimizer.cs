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
    public RtlCodeBlock Optimize(RtlCodeBlock block, bool enableAdvancedOptimizations = true)
    {
        // Pass 1: Remove NOPs
        RemoveNops(block);
        
        // Pass 2: Constant folding
        ConstantFolding(block);
        
        // Pass 3: Dead code elimination
        DeadCodeElimination(block);
        
        // Pass 4: Copy propagation
        CopyPropagation(block);
        
        if (enableAdvancedOptimizations)
        {
            // Pass 5: Loop unrolling
            LoopUnrolling(block);
            
            // Pass 6: Function inlining (for small blocks)
            FunctionInlining(block);
            
            // Pass 7: SIMD detection
            SimdDetection(block);
            
            // Pass 8: Strength reduction
            StrengthReduction(block);
        }
        
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
    
    /// <summary>
    /// Loop unrolling: detect small loops and unroll them
    /// Example: for (i=0; i<4; i++) { body } => body; body; body; body;
    /// </summary>
    private void LoopUnrolling(RtlCodeBlock block)
    {
        // Detect simple counted loops
        for (int bbIndex = 0; bbIndex < block.BasicBlocks.Count; bbIndex++)
        {
            var bb = block.BasicBlocks[bbIndex];
            
            // Look for loop pattern: counter increment + conditional branch back
            if (bb.Instructions.Count < 2) continue;
            
            var lastInsn = bb.Instructions[^1];
            if (lastInsn is not RtlBranch branch) continue;
            
            // Check if it's a back-edge (loop)
            var targetBb = FindBasicBlockByAddress(block, branch.TargetAddress);
            if (targetBb == null || block.BasicBlocks.IndexOf(targetBb) >= bbIndex) continue;
            
            // Simple heuristic: if loop body is small (< 20 instructions) and count is <= 4, unroll
            var loopSize = EstimateLoopSize(block, targetBb, bb);
            if (loopSize > 0 && loopSize <= 20)
            {
                // Try to determine iteration count from condition
                var iterationCount = EstimateIterationCount(branch.Condition);
                if (iterationCount > 0 && iterationCount <= 4)
                {
                    UnrollLoop(block, targetBb, bb, iterationCount);
                }
            }
        }
    }
    
    /// <summary>
    /// Function inlining: inline small function calls
    /// </summary>
    private void FunctionInlining(RtlCodeBlock block)
    {
        // Track small blocks that could be inlined (< 10 instructions)
        var inlineCandidates = new Dictionary<uint, RtlBasicBlock>();
        
        foreach (var bb in block.BasicBlocks)
        {
            if (bb.Instructions.Count < 10 && bb.Instructions.Count > 0)
            {
                var lastInsn = bb.Instructions[^1];
                if (lastInsn is RtlReturn)
                {
                    inlineCandidates[bb.StartAddress] = bb;
                }
            }
        }
        
        // Replace calls to inline candidates with their body
        foreach (var bb in block.BasicBlocks)
        {
            for (int i = 0; i < bb.Instructions.Count; i++)
            {
                if (bb.Instructions[i] is RtlCall call)
                {
                    if (call.Target is RtlConstant constTarget &&
                        inlineCandidates.TryGetValue(constTarget.Value, out var targetBb))
                    {
                        // Replace call with inlined instructions
                        bb.Instructions.RemoveAt(i);
                        bb.Instructions.InsertRange(i, targetBb.Instructions.Take(targetBb.Instructions.Count - 1));
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// SIMD detection: identify patterns that can use SIMD instructions
    /// Example: 4 consecutive add operations on adjacent memory => Vector128.Add
    /// </summary>
    private void SimdDetection(RtlCodeBlock block)
    {
        foreach (var bb in block.BasicBlocks)
        {
            // Look for patterns of consecutive operations on adjacent memory locations
            for (int i = 0; i < bb.Instructions.Count - 3; i++)
            {
                // Pattern: 4 consecutive loads from adjacent addresses
                if (IsConsecutiveMemoryOps(bb.Instructions, i, 4))
                {
                    // Check if followed by same operation (e.g., all ADD)
                    if (AreSimilarOperations(bb.Instructions, i, 4, out var operation))
                    {
                        // Mark for SIMD optimization
                        bb.Instructions[i] = new RtlSimdOp
                        {
                            Offset = bb.Instructions[i].Offset,
                            Operation = operation,
                            VectorSize = 4,
                            Comment = $"SIMD: Vectorized {operation} operation (4 elements)"
                        };
                        
                        // Remove the replaced instructions
                        bb.Instructions.RemoveRange(i + 1, 3);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Strength reduction: replace expensive operations with cheaper equivalents
    /// Example: x * 2 => x << 1, x / 4 => x >> 2
    /// </summary>
    private void StrengthReduction(RtlCodeBlock block)
    {
        foreach (var bb in block.BasicBlocks)
        {
            for (int i = 0; i < bb.Instructions.Count; i++)
            {
                if (bb.Instructions[i] is RtlBinaryOp binOp)
                {
                    // Multiplication by power of 2 => left shift
                    if (binOp.Operator == "*" && binOp.Right is RtlConstant multConst)
                    {
                        var shiftAmount = Log2IfPowerOf2(multConst.Value);
                        if (shiftAmount >= 0)
                        {
                            bb.Instructions[i] = new RtlBinaryOp
                            {
                                Offset = binOp.Offset,
                                Destination = binOp.Destination,
                                Left = binOp.Left,
                                Operator = "<<",
                                Right = new RtlConstant { Value = (uint)shiftAmount }
                            };
                        }
                    }
                    // Division by power of 2 => right shift
                    else if (binOp.Operator == "/" && binOp.Right is RtlConstant divConst)
                    {
                        var shiftAmount = Log2IfPowerOf2(divConst.Value);
                        if (shiftAmount >= 0)
                        {
                            bb.Instructions[i] = new RtlBinaryOp
                            {
                                Offset = binOp.Offset,
                                Destination = binOp.Destination,
                                Left = binOp.Left,
                                Operator = ">>",
                                Right = new RtlConstant { Value = (uint)shiftAmount }
                            };
                        }
                    }
                    // Addition of 0 => remove
                    else if (binOp.Operator == "+" && binOp.Right is RtlConstant addConst && addConst.Value == 0)
                    {
                        bb.Instructions[i] = new RtlAssignment
                        {
                            Offset = binOp.Offset,
                            Destination = binOp.Destination,
                            Source = binOp.Left
                        };
                    }
                }
            }
        }
    }
    
    // Helper methods for advanced optimizations
    
    private RtlBasicBlock? FindBasicBlockByAddress(RtlCodeBlock block, uint address)
    {
        return block.BasicBlocks.FirstOrDefault(bb => bb.StartAddress == address);
    }
    
    private int EstimateLoopSize(RtlCodeBlock block, RtlBasicBlock loopStart, RtlBasicBlock loopEnd)
    {
        var startIndex = block.BasicBlocks.IndexOf(loopStart);
        var endIndex = block.BasicBlocks.IndexOf(loopEnd);
        
        if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
            return -1;
        
        int totalSize = 0;
        for (int i = startIndex; i <= endIndex; i++)
        {
            totalSize += block.BasicBlocks[i].Instructions.Count;
        }
        
        return totalSize;
    }
    
    private int EstimateIterationCount(RtlExpression? condition)
    {
        // Simple heuristic: look for comparisons with small constants
        if (condition is RtlBinaryExpression binExpr)
        {
            if (binExpr.Right is RtlConstant constant && constant.Value <= 4)
            {
                return (int)constant.Value;
            }
        }
        return -1;
    }
    
    private void UnrollLoop(RtlCodeBlock block, RtlBasicBlock loopStart, RtlBasicBlock loopEnd, int count)
    {
        // Simplified loop unrolling: duplicate loop body 'count' times
        var startIndex = block.BasicBlocks.IndexOf(loopStart);
        var endIndex = block.BasicBlocks.IndexOf(loopEnd);
        
        if (startIndex < 0 || endIndex < 0) return;
        
        // Collect loop body instructions
        var loopBody = new List<RtlInstruction>();
        for (int i = startIndex; i <= endIndex; i++)
        {
            loopBody.AddRange(block.BasicBlocks[i].Instructions);
        }
        
        // Remove branch instruction
        if (loopBody.Count > 0 && loopBody[^1] is RtlBranch)
        {
            loopBody.RemoveAt(loopBody.Count - 1);
        }
        
        // Insert duplicated body
        var insertPoint = block.BasicBlocks[endIndex].Instructions.Count;
        for (int iteration = 1; iteration < count; iteration++)
        {
            block.BasicBlocks[endIndex].Instructions.InsertRange(insertPoint, loopBody);
        }
    }
    
    private bool IsConsecutiveMemoryOps(List<RtlInstruction> instructions, int start, int count)
    {
        if (start + count > instructions.Count) return false;
        
        uint? baseAddress = null;
        for (int i = 0; i < count; i++)
        {
            if (instructions[start + i] is not RtlLoad load) return false;
            if (load.Address is not RtlConstant addrConst) return false;
            
            if (baseAddress == null)
            {
                baseAddress = addrConst.Value;
            }
            else if (addrConst.Value != baseAddress.Value + (uint)(i * 4))
            {
                return false;
            }
        }
        
        return true;
    }
    
    private bool AreSimilarOperations(List<RtlInstruction> instructions, int start, int count, out string operation)
    {
        operation = "";
        
        if (start + count * 2 > instructions.Count) return false;
        
        string? firstOp = null;
        for (int i = 0; i < count; i++)
        {
            var opIndex = start + count + i;
            if (opIndex >= instructions.Count) return false;
            
            if (instructions[opIndex] is RtlBinaryOp binOp)
            {
                if (firstOp == null)
                {
                    firstOp = binOp.Operator;
                }
                else if (binOp.Operator != firstOp)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        
        operation = firstOp ?? "";
        return firstOp != null;
    }
    
    private int Log2IfPowerOf2(uint value)
    {
        if (value == 0 || (value & (value - 1)) != 0)
            return -1;
        
        int log = 0;
        while (value > 1)
        {
            value >>= 1;
            log++;
        }
        return log;
    }
}
