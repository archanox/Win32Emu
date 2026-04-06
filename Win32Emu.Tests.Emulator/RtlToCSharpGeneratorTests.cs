using System.Reflection;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Win32Emu.Cpu;
using Win32Emu.Rtl;

namespace Win32Emu.Tests.Emulator;

public class RtlToCSharpGeneratorTests
{
	[Fact]
	public void GenerateCSharpCode_Compiles_WhenBlockUsesPartialAndSegmentRegisters()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x401000,
			EndAddress = 0x401003,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x401000,
					Instructions =
					{
						new RtlLoad
						{
							Offset = 0x401000,
							Destination = new RtlRegister { Name = "AL" },
							Address = new RtlRegister { Name = "ES" },
							Size = 1
						},
						new RtlBinaryOp
						{
							Offset = 0x401001,
							Destination = new RtlRegister { Name = "AL" },
							Left = new RtlRegister { Name = "AL" },
							Operator = "+",
							Right = new RtlRegister { Name = "CL" }
						},
						new RtlAssignment
						{
							Offset = 0x401002,
							Destination = new RtlRegister { Name = "AX" },
							Source = new RtlBinaryExpression
							{
								Left = new RtlRegister { Name = "AX" },
								Operator = "|",
								Right = new RtlConstant { Value = 1 }
							}
						}
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();

		var code = generator.GenerateCSharpCode(block, "TestClass", "Execute");
		var diagnostics = CompileGeneratedCode(code);
		var errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

		Assert.Contains("uint ES = cpu.GetRegister(\"ES\");", code);
		Assert.Contains("EAX = (EAX & 0xFFFFFF00u)", code);
		Assert.Contains("(ECX & 0xFFu)", code);
		Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
	}

	[Fact]
	public void GenerateCSharpCode_ForReturn_SavesRegistersBeforeReturning()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x402000,
			EndAddress = 0x402001,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x402000,
					Instructions =
					{
						new RtlAssignment
						{
							Offset = 0x402000,
							Destination = new RtlRegister { Name = "ES" },
							Source = new RtlConstant { Value = 0x1234 }
						},
						new RtlReturn
						{
							Offset = 0x402001
						}
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();

		var code = generator.GenerateCSharpCode(block, "ReturnTestClass", "Execute");
		var returnSaveIndex = code.IndexOf("cpu.SetRegister(\"ES\", ES);", StringComparison.Ordinal);
		var setEipIndex = code.IndexOf("cpu.SetEip(retAddr);", StringComparison.Ordinal);

		Assert.True(returnSaveIndex >= 0);
		Assert.True(setEipIndex > returnSaveIndex);
	}

	[Fact]
	public void GenerateCSharpCode_Compiles_WhenBlockUsesShiftOperators()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x403000,
			EndAddress = 0x403004,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x403000,
					Instructions =
					{
						// EAX = EAX << 0x2u  (SHL EAX, 2)
						new RtlBinaryOp
						{
							Offset = 0x403000,
							Destination = new RtlRegister { Name = "EAX" },
							Left = new RtlRegister { Name = "EAX" },
							Operator = "<<",
							Right = new RtlConstant { Value = 2 }
						},
						// EBX = EBX >> 0x1u  (SHR EBX, 1)
						new RtlBinaryOp
						{
							Offset = 0x403001,
							Destination = new RtlRegister { Name = "EBX" },
							Left = new RtlRegister { Name = "EBX" },
							Operator = ">>",
							Right = new RtlConstant { Value = 1 }
						},
						// ECX = (EAX << ECX) via RtlBinaryExpression
						new RtlAssignment
						{
							Offset = 0x403002,
							Destination = new RtlRegister { Name = "ECX" },
							Source = new RtlBinaryExpression
							{
								Left = new RtlRegister { Name = "EAX" },
								Operator = "<<",
								Right = new RtlRegister { Name = "ECX" }
							}
						}
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();

		var code = generator.GenerateCSharpCode(block, "ShiftTestClass", "Execute");
		var diagnostics = CompileGeneratedCode(code);
		var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

		Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
	}

	[Fact]
	public void GenerateCSharpCode_ContainsFlagVariablesLoadedFromEflags()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x404000,
			EndAddress = 0x404002,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x404000,
					Instructions =
					{
						new RtlNop { Offset = 0x404000 }
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();
		var code = generator.GenerateCSharpCode(block, "FlagTestClass", "Execute");

		// Flag booleans must be declared and loaded from EFLAGS
		Assert.Contains("uint EFLAGS = cpu.GetRegister(\"EFLAGS\");", code);
		Assert.Contains("bool ZF = (EFLAGS & 0x40u) != 0;", code);
		Assert.Contains("bool CF = (EFLAGS & 0x1u) != 0;", code);
		Assert.Contains("bool SF = (EFLAGS & 0x80u) != 0;", code);
		Assert.Contains("bool OF = (EFLAGS & 0x800u) != 0;", code);
		Assert.Contains("bool PF = (EFLAGS & 0x4u) != 0;", code);

		// EFLAGS must be saved back at the end of the block
		Assert.Contains("cpu.SetRegister(\"EFLAGS\", EFLAGS);", code);
	}

	[Fact]
	public void GenerateCSharpCode_ForFlagUpdate_GeneratesCorrectFlagCode()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x405000,
			EndAddress = 0x405003,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x405000,
					Instructions =
					{
						new RtlFlagUpdate
						{
							Offset = 0x405000,
							Operation = FlagUpdateOperation.Sub,
							Result = new RtlRegister { Name = "EAX" },
							Left = new RtlTemporary { Id = 0 },
							Right = new RtlRegister { Name = "EBX" },
							OperandSize = 4
						}
					}
				}
			}
		};
		block.NextTemporaryId = 1;

		var generator = new RtlToCSharpGenerator();
		var code = generator.GenerateCSharpCode(block, "FlagUpdateClass", "Execute");

		Assert.Contains("ZF = _r == 0u;", code);
		Assert.Contains("SF =", code);
		Assert.Contains("CF = _l < _ri;", code);  // borrow for SUB
		Assert.Contains("OF =", code);

		var diagnostics = CompileGeneratedCode(code);
		var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
	}

	[Fact]
	public void GenerateCSharpCode_ForFlagReference_GeneratesConditionalExpression()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x406000,
			EndAddress = 0x406002,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x406000,
					Instructions =
					{
						new RtlAssignment
						{
							Offset = 0x406000,
							Destination = new RtlRegister { Name = "AL" },
							Source = new RtlFlagReference { Condition = FlagCondition.Equal }
						}
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();
		var code = generator.GenerateCSharpCode(block, "FlagRefClass", "Execute");

		// SETE AL should emit (ZF ? 1u : 0u)
		Assert.Contains("ZF ? 1u : 0u", code);

		var diagnostics = CompileGeneratedCode(code);
		var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
	}

	[Fact]
	public void GenerateCSharpCode_ForFlagConditionBranch_GeneratesCorrectCondition()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x407000,
			EndAddress = 0x407010,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x407000,
					Instructions =
					{
						new RtlBranch
						{
							Offset = 0x407000,
							FlagCondition = FlagCondition.Less,
							TargetOffset = 0x407020  // outside the block → triggers register save + return
						}
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();
		var code = generator.GenerateCSharpCode(block, "BranchFlagClass", "Execute");

		// JL should check (SF != OF)
		Assert.Contains("(SF != OF)", code);

		var diagnostics = CompileGeneratedCode(code);
		var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
	}

	[Fact]
	public void GenerateCSharpCode_ForStore_CastsAddressAndValue()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x408000,
			EndAddress = 0x408004,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x408000,
					Instructions =
					{
						// MOV BYTE PTR [EAX], CL  (size=1 store)
						new RtlStore
						{
							Offset = 0x408000,
							Address = new RtlRegister { Name = "EAX" },
							Value = new RtlRegister { Name = "CL" },
							Size = 1
						},
						// MOV WORD PTR [EBX], DX  (size=2 store)
						new RtlStore
						{
							Offset = 0x408002,
							Address = new RtlRegister { Name = "EBX" },
							Value = new RtlRegister { Name = "DX" },
							Size = 2
						},
						// MOV DWORD PTR [ECX], EDX  (size=4 store)
						new RtlStore
						{
							Offset = 0x408003,
							Address = new RtlRegister { Name = "ECX" },
							Value = new RtlRegister { Name = "EDX" },
							Size = 4
						}
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();
		var code = generator.GenerateCSharpCode(block, "StoreTestClass", "Execute");

		// Address must be cast to ulong
		Assert.Contains("mem.Write8((ulong)(", code);
		Assert.Contains("mem.Write16((ulong)(", code);
		Assert.Contains("mem.Write32((ulong)(", code);

		// Value must be narrowed to the correct integer type
		Assert.Contains("), (byte)(", code);
		Assert.Contains("), (ushort)(", code);
		Assert.Contains("), (uint)(", code);
	}

	[Fact]
	public void GenerateCSharpCode_ForLoad_CastsAddress()
	{
		var block = new RtlCodeBlock
		{
			StartAddress = 0x409000,
			EndAddress = 0x409004,
			BasicBlocks =
			{
				new RtlBasicBlock
				{
					StartAddress = 0x409000,
					Instructions =
					{
						// MOV AL, BYTE PTR [EAX]
						new RtlLoad
						{
							Offset = 0x409000,
							Destination = new RtlRegister { Name = "AL" },
							Address = new RtlRegister { Name = "EAX" },
							Size = 1
						},
						// MOV EBX, DWORD PTR [ECX]
						new RtlLoad
						{
							Offset = 0x409002,
							Destination = new RtlRegister { Name = "EBX" },
							Address = new RtlRegister { Name = "ECX" },
							Size = 4
						}
					}
				}
			}
		};

		var generator = new RtlToCSharpGenerator();
		var code = generator.GenerateCSharpCode(block, "LoadTestClass", "Execute");

		// Address must be cast to ulong for both Read8 and Read32
		Assert.Contains("mem.Read8((ulong)(", code);
		Assert.Contains("mem.Read32((ulong)(", code);
	}

	private static ImmutableArray<Diagnostic> CompileGeneratedCode(string code)
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var compilation = CSharpCompilation.Create(
			"GeneratedJitBlockTests",
			new[] { syntaxTree },
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
		);

		return compilation.GetDiagnostics();
	}

	private static IEnumerable<MetadataReference> GetMetadataReferences()
	{
		yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(Task).Assembly.Location);
		yield return MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location);
		yield return MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(CpuStepResult).Assembly.Location);
	}
}
