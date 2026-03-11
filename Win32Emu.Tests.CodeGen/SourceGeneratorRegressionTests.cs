using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Win32Emu.Generators;

namespace Win32Emu.Tests.CodeGen;

public class SourceGeneratorRegressionTests
{
	[Fact]
	public void ApiStatusGenerator_IsDeterministic_AndIgnoresNonExportAttributes()
	{
		var compilation = CreateCompilation(
			"""
			using System;

			namespace Win32Emu.Win32
			{
				[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
				public sealed class DllModuleExportAttribute : Attribute
				{
					public bool IsStub { get; init; }
					public string? Version { get; init; }
					public string? ExportName { get; init; }
					public string? ForwardedTo { get; init; }

					public DllModuleExportAttribute(uint ordinal)
					{
					}
				}
			}

			namespace Win32Emu.Win32.Modules
			{
				using Win32Emu.Win32;

				public sealed class SampleModule
				{
					public string Name => "SAMPLE.DLL";

					[Obsolete]
					private static void Helper()
					{
					}

					[DllModuleExport(1)]
					private static uint Exported()
					{
						return 0;
					}
				}
			}
			""");

		var firstRun = GetGeneratedSource(compilation, new ApiStatusGenerator(), "ApiStatusMetadata.g.cs");
		var secondRun = GetGeneratedSource(compilation, new ApiStatusGenerator(), "ApiStatusMetadata.g.cs");

		Assert.Equal(firstRun, secondRun);
		Assert.Contains("SAMPLE.DLL", firstRun, StringComparison.Ordinal);
		Assert.Contains("Exported", firstRun, StringComparison.Ordinal);
		Assert.DoesNotContain("Helper", firstRun, StringComparison.Ordinal);
		Assert.DoesNotContain("generatedAt", firstRun, StringComparison.Ordinal);
	}

	[Fact]
	public void StdCallArgBytesGenerator_IgnoresNonExportAttributes()
	{
		var compilation = CreateCompilation(
			"""
			using System;

			namespace Win32Emu.Win32
			{
				[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
				public sealed class DllModuleExportAttribute : Attribute
				{
					public string? ExportName { get; init; }
					public int CallingConvention { get; init; }

					public DllModuleExportAttribute(uint ordinal)
					{
					}
				}
			}

			namespace Win32Emu.Win32.Modules
			{
				using Win32Emu.Win32;

				public sealed class SampleModule
				{
					public string Name => "SAMPLE.DLL";

					[Obsolete]
					private static void Helper()
					{
					}

					[DllModuleExport(1)]
					private static uint Exported(uint value)
					{
						return value;
					}
				}
			}
			""");

		var stdCallMeta = GetGeneratedSource(compilation, new StdCallArgBytesGenerator(), "StdCallMeta.g.cs");
		var exportInfo = GetGeneratedSource(compilation, new StdCallArgBytesGenerator(), "DllModuleExportInfo.g.cs");

		Assert.Contains("(\"SAMPLE.DLL\", \"EXPORTED\"): return 4;", stdCallMeta, StringComparison.Ordinal);
		Assert.Contains("exports[\"Exported\"] = 1;", exportInfo, StringComparison.Ordinal);
		Assert.DoesNotContain("\"HELPER\"", stdCallMeta, StringComparison.Ordinal);
		Assert.DoesNotContain("exports[\"Helper\"]", exportInfo, StringComparison.Ordinal);
	}

	private static CSharpCompilation CreateCompilation(string source)
	{
		return CSharpCompilation.Create(
			assemblyName: "GeneratorTests",
			syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
			references: GetMetadataReferences(),
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}

	private static IReadOnlyList<MetadataReference> GetMetadataReferences()
	{
		return ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
			.Select(path => MetadataReference.CreateFromFile(path))
			.Concat([
				MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)
			])
			.ToArray();
	}

	private static string GetGeneratedSource(CSharpCompilation compilation, IIncrementalGenerator generator, string hintName)
	{
		var driver = CSharpGeneratorDriver.Create(generator)
			.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
		var result = driver.GetRunResult();

		return result.Results
			.SelectMany(static generatorResult => generatorResult.GeneratedSources)
			.First(source => source.HintName == hintName)
			.SourceText
			.ToString();
	}
}
