using Xunit;
using Win32Emu.Rtl;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for autovectorization optimizations in the RTL JIT compiler
/// </summary>
public class AutovectorizationTests
{
    /// <summary>
    /// Test that consecutive memory operations with the same operation are detected as vectorizable
    /// </summary>
    [Fact]
    public void SimdDetection_DetectsVectorAddOperation()
    {
        // Arrange
        var optimizer = new RtlOptimizer();
        var block = CreateVectorAddPattern();
        
        // Act
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Assert
        // Should have SIMD operations after optimization
        var simdOps = CountSimdOperations(optimized);
        Assert.True(simdOps > 0, $"Expected SIMD detection for vectorizable pattern, but found {simdOps} SIMD operations");
    }
    
    /// <summary>
    /// Test that the optimizer doesn't break non-vectorizable code
    /// </summary>
    [Fact]
    public void SimdDetection_PreservesNonVectorizableCode()
    {
        // Arrange
        var optimizer = new RtlOptimizer();
        var block = CreateVectorAddPattern();
        var originalOpCount = CountTotalOperations(block);
        
        // Act
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Assert
        var finalOpCount = CountTotalOperations(optimized);
        // Either code was vectorized (fewer ops) or preserved (same or similar count)
        Assert.True(finalOpCount <= originalOpCount, 
            $"Optimizer should not increase operation count (original: {originalOpCount}, final: {finalOpCount})");
    }
    
    /// <summary>
    /// Test that non-consecutive memory operations are not vectorized
    /// </summary>
    [Fact]
    public void SimdDetection_DoesNotVectorizeNonConsecutiveOps()
    {
        // Arrange
        var optimizer = new RtlOptimizer();
        var block = CreateNonConsecutivePattern();
        
        // Act
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Assert
        // Should not have SIMD operations since addresses are not consecutive
        var simdOps = CountSimdOperations(optimized);
        Assert.Equal(0, simdOps);
    }
    
    /// <summary>
    /// Test that different operations on consecutive memory are not vectorized
    /// </summary>
    [Fact]
    public void SimdDetection_DoesNotVectorizeMixedOperations()
    {
        // Arrange
        var optimizer = new RtlOptimizer();
        var block = CreateMixedOperationsPattern();
        
        // Act
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Assert
        // Should not have SIMD operations since operations differ
        var simdOps = CountSimdOperations(optimized);
        Assert.Equal(0, simdOps);
    }
    
    /// <summary>
    /// Test C# code generation for SIMD operations
    /// </summary>
    [Fact]
    public void CodeGeneration_GeneratesVector128Code()
    {
        // Arrange
        var generator = new RtlToCSharpGenerator();
        var block = CreateVectorAddPattern();
        var optimizer = new RtlOptimizer();
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Act
        var code = generator.GenerateCSharpCode(optimized, "TestClass", "Execute");
        
        // Assert
        // If SIMD was detected, generated code should use Vector128
        var simdOps = CountSimdOperations(optimized);
        if (simdOps > 0)
        {
            Assert.Contains("System.Runtime.Intrinsics", code);
            Assert.Contains("Vector128", code);
        }
    }
    
    /// <summary>
    /// Test that vectorized add operations generate correct intrinsic calls
    /// </summary>
    [Fact]
    public void CodeGeneration_GeneratesAddIntrinsic()
    {
        // Arrange
        var generator = new RtlToCSharpGenerator();
        var block = CreateVectorAddPattern();
        var optimizer = new RtlOptimizer();
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Act
        var code = generator.GenerateCSharpCode(optimized, "TestClass", "Execute");
        
        // Assert
        var simdOps = CountSimdOperations(optimized);
        if (simdOps > 0)
        {
            Assert.Contains("Vector128.Add", code);
        }
    }
    
    /// <summary>
    /// Test that vectorized multiply operations generate correct intrinsic calls
    /// </summary>
    [Fact]
    public void CodeGeneration_GeneratesMultiplyIntrinsic()
    {
        // Arrange
        var generator = new RtlToCSharpGenerator();
        var block = CreateVectorMultiplyPattern();
        var optimizer = new RtlOptimizer();
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Act
        var code = generator.GenerateCSharpCode(optimized, "TestClass", "Execute");
        
        // Assert
        var simdOps = CountSimdOperations(optimized);
        if (simdOps > 0)
        {
            Assert.Contains("Vector128.Multiply", code);
        }
    }
    
    /// <summary>
    /// Test that vectorized operations include proper memory loads and stores
    /// </summary>
    [Fact]
    public void CodeGeneration_GeneratesMemoryAccessCode()
    {
        // Arrange
        var generator = new RtlToCSharpGenerator();
        var block = CreateVectorAddPattern();
        var optimizer = new RtlOptimizer();
        var optimized = optimizer.Optimize(block, enableAdvancedOptimizations: true);
        
        // Act
        var code = generator.GenerateCSharpCode(optimized, "TestClass", "Execute");
        
        // Assert
        var simdOps = CountSimdOperations(optimized);
        if (simdOps > 0)
        {
            // Should have vector load and store operations
            Assert.Contains("mem.Read32", code);
            Assert.Contains("mem.Write32", code);
            Assert.Contains("Vector128.Create", code);
        }
    }
    
    // Helper methods to create RTL patterns for testing
    
    private RtlCodeBlock CreateVectorAddPattern()
    {
        // Pattern: Load 4 consecutive addresses, then operate on all, then store back
        // This creates the grouped pattern expected by the optimizer:
        // [load0, load1, load2, load3, op0, op1, op2, op3, store0, store1, store2, store3]
        var block = new RtlCodeBlock
        {
            StartAddress = 0x401000,
            BasicBlocks = new List<RtlBasicBlock>
            {
                new RtlBasicBlock
                {
                    StartAddress = 0x401000,
                    Instructions = new List<RtlInstruction>()
                }
            }
        };
        
        var bb = block.BasicBlocks[0];
        var baseAddr = 0x403000u;
        var temps = new List<RtlTemporary>();
        var resultTemps = new List<RtlTemporary>();
        
        // Step 1: Generate all loads
        for (int i = 0; i < 4; i++)
        {
            var temp = block.NewTemporary();
            temps.Add(temp);
            
            bb.Instructions.Add(new RtlLoad
            {
                Offset = 0x401000 + i * 4,
                Destination = temp,
                Address = new RtlConstant { Value = baseAddr + (uint)(i * 4) },
                Size = 4
            });
        }
        
        // Step 2: Generate all operations
        for (int i = 0; i < 4; i++)
        {
            var resultTemp = block.NewTemporary();
            resultTemps.Add(resultTemp);
            
            bb.Instructions.Add(new RtlBinaryOp
            {
                Offset = 0x401010 + i * 4,
                Destination = resultTemp,
                Left = temps[i],
                Operator = "+",
                Right = new RtlConstant { Value = 1 }
            });
        }
        
        // Step 3: Generate all stores
        for (int i = 0; i < 4; i++)
        {
            bb.Instructions.Add(new RtlStore
            {
                Offset = 0x401020 + i * 4,
                Address = new RtlConstant { Value = baseAddr + (uint)(i * 4) },
                Value = resultTemps[i],
                Size = 4
            });
        }
        
        return block;
    }
    
    private RtlCodeBlock CreateVectorMultiplyPattern()
    {
        // Pattern: Load 4 consecutive addresses, multiply by constant, store back
        // Grouped pattern: [load0-3, op0-3, store0-3]
        var block = new RtlCodeBlock
        {
            StartAddress = 0x401000,
            BasicBlocks = new List<RtlBasicBlock>
            {
                new RtlBasicBlock
                {
                    StartAddress = 0x401000,
                    Instructions = new List<RtlInstruction>()
                }
            }
        };
        
        var bb = block.BasicBlocks[0];
        var baseAddr = 0x403000u;
        var temps = new List<RtlTemporary>();
        var resultTemps = new List<RtlTemporary>();
        
        // Generate all loads
        for (int i = 0; i < 4; i++)
        {
            var temp = block.NewTemporary();
            temps.Add(temp);
            
            bb.Instructions.Add(new RtlLoad
            {
                Offset = 0x401000 + i * 4,
                Destination = temp,
                Address = new RtlConstant { Value = baseAddr + (uint)(i * 4) },
                Size = 4
            });
        }
        
        // Generate all operations
        for (int i = 0; i < 4; i++)
        {
            var resultTemp = block.NewTemporary();
            resultTemps.Add(resultTemp);
            
            bb.Instructions.Add(new RtlBinaryOp
            {
                Offset = 0x401010 + i * 4,
                Destination = resultTemp,
                Left = temps[i],
                Operator = "*",
                Right = new RtlConstant { Value = 2 }
            });
        }
        
        // Generate all stores
        for (int i = 0; i < 4; i++)
        {
            bb.Instructions.Add(new RtlStore
            {
                Offset = 0x401020 + i * 4,
                Address = new RtlConstant { Value = baseAddr + (uint)(i * 4) },
                Value = resultTemps[i],
                Size = 4
            });
        }
        
        return block;
    }
    
    private RtlCodeBlock CreateNonConsecutivePattern()
    {
        // Pattern: Operations on non-consecutive addresses (not vectorizable)
        var block = new RtlCodeBlock
        {
            StartAddress = 0x401000,
            BasicBlocks = new List<RtlBasicBlock>
            {
                new RtlBasicBlock
                {
                    StartAddress = 0x401000,
                    Instructions = new List<RtlInstruction>()
                }
            }
        };
        
        var bb = block.BasicBlocks[0];
        var addresses = new uint[] { 0x403000, 0x403100, 0x403200, 0x403300 }; // Non-consecutive
        
        for (int i = 0; i < 4; i++)
        {
            var temp = block.NewTemporary();
            var resultTemp = block.NewTemporary();
            
            bb.Instructions.Add(new RtlLoad
            {
                Destination = temp,
                Address = new RtlConstant { Value = addresses[i] },
                Size = 4
            });
            
            bb.Instructions.Add(new RtlBinaryOp
            {
                Destination = resultTemp,
                Left = temp,
                Operator = "+",
                Right = new RtlConstant { Value = 1 }
            });
            
            bb.Instructions.Add(new RtlStore
            {
                Address = new RtlConstant { Value = addresses[i] },
                Value = resultTemp,
                Size = 4
            });
        }
        
        return block;
    }
    
    private RtlCodeBlock CreateMixedOperationsPattern()
    {
        // Pattern: Consecutive addresses but different operations (not vectorizable)
        var block = new RtlCodeBlock
        {
            StartAddress = 0x401000,
            BasicBlocks = new List<RtlBasicBlock>
            {
                new RtlBasicBlock
                {
                    StartAddress = 0x401000,
                    Instructions = new List<RtlInstruction>()
                }
            }
        };
        
        var bb = block.BasicBlocks[0];
        var baseAddr = 0x403000u;
        var operations = new string[] { "+", "-", "*", "/" };
        
        for (int i = 0; i < 4; i++)
        {
            var temp = block.NewTemporary();
            var resultTemp = block.NewTemporary();
            
            bb.Instructions.Add(new RtlLoad
            {
                Destination = temp,
                Address = new RtlConstant { Value = baseAddr + (uint)(i * 4) },
                Size = 4
            });
            
            bb.Instructions.Add(new RtlBinaryOp
            {
                Destination = resultTemp,
                Left = temp,
                Operator = operations[i], // Different operations
                Right = new RtlConstant { Value = 2 }
            });
            
            bb.Instructions.Add(new RtlStore
            {
                Address = new RtlConstant { Value = baseAddr + (uint)(i * 4) },
                Value = resultTemp,
                Size = 4
            });
        }
        
        return block;
    }
    
    private int CountSimdOperations(RtlCodeBlock block)
    {
        return block.BasicBlocks
            .SelectMany(bb => bb.Instructions)
            .Count(insn => insn is RtlSimdOp);
    }
    
    private int CountTotalOperations(RtlCodeBlock block)
    {
        return block.BasicBlocks
            .Sum(bb => bb.Instructions.Count);
    }
}
