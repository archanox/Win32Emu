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
