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
