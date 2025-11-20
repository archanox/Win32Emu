using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Win32Emu.Tools.TestGenerator;

/// <summary>
/// Generates A-B test scaffolding from native DLL analysis reports.
/// This tool helps implement use case #3 from NATIVE_DLL_ANALYSIS.md:
/// Test-driven development using native DLL exports as specification.
/// </summary>
class Program
{
	static async Task<int> Main(string[] args)
	{
		Console.WriteLine("Win32Emu Test Generator");
		Console.WriteLine("=======================");
		Console.WriteLine();

		if (args.Length < 2)
		{
			Console.WriteLine("Usage: Win32Emu.Tools.TestGenerator <missing-functions-json> <output-directory> [dll-filter]");
			Console.WriteLine();
			Console.WriteLine("Arguments:");
			Console.WriteLine("  missing-functions-json - Missing functions JSON from NativeDllAnalyzer");
			Console.WriteLine("  output-directory       - Directory where test files will be generated");
			Console.WriteLine("  dll-filter            - Optional: specific DLL to generate tests for (e.g., KERNEL32.DLL)");
			Console.WriteLine();
			Console.WriteLine("Examples:");
			Console.WriteLine("  # Generate tests for all DLLs");
			Console.WriteLine("  Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated");
			Console.WriteLine();
			Console.WriteLine("  # Generate tests for specific DLL");
			Console.WriteLine("  Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated KERNEL32.DLL");
			Console.WriteLine();
			return 1;
		}

		var jsonFile = args[0];
		var outputDir = args[1];
		var dllFilter = args.Length > 2 ? args[2].ToUpperInvariant() : null;

		if (!File.Exists(jsonFile))
		{
			Console.WriteLine($"Error: JSON file not found: {jsonFile}");
			return 1;
		}

		try
		{
			var json = await File.ReadAllTextAsync(jsonFile);
			var report = JsonSerializer.Deserialize<NativeDllAnalysisReport>(json, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			if (report == null)
			{
				Console.WriteLine("Error: Failed to deserialize report");
				return 1;
			}

			var generator = new TestGenerator(report, outputDir, dllFilter);
			await generator.GenerateTestsAsync();

			Console.WriteLine();
			Console.WriteLine("Test generation complete!");
			return 0;
		}
		catch (Exception ex) when (
			ex is not OutOfMemoryException &&
			ex is not StackOverflowException)
		{
			Console.WriteLine($"Error: {ex.Message}");
			return 1;
		}
	}
}

/// <summary>
/// Generates test files from the native DLL analysis report.
/// </summary>
public class TestGenerator
{
	private readonly NativeDllAnalysisReport _report;
	private readonly string _outputDir;
	private readonly string? _dllFilter;

	public TestGenerator(NativeDllAnalysisReport report, string outputDir, string? dllFilter)
	{
		_report = report;
		_outputDir = outputDir;
		_dllFilter = dllFilter;
	}

	public async Task GenerateTestsAsync()
	{
		// Create output directory if it doesn't exist
		if (!Directory.Exists(_outputDir))
		{
			Directory.CreateDirectory(_outputDir);
			Console.WriteLine($"Created output directory: {_outputDir}");
		}

		// Generate README
		await GenerateReadmeAsync();

		// Generate base test infrastructure
		await GenerateTestInfrastructureAsync();

		// Generate tests for each DLL
		var dllsToProcess = _dllFilter != null
			? _report.Dlls.Where(d => d.DllName.Equals(_dllFilter, StringComparison.OrdinalIgnoreCase))
			: _report.Dlls;

		foreach (var dll in dllsToProcess)
		{
			// Only generate tests if there are missing or stub functions
			if (dll.MissingFunctions.Count > 0 || dll.StubFunctions.Count > 0)
			{
				await GenerateDllTestsAsync(dll);
			}
		}

		Console.WriteLine();
		Console.WriteLine("Generated test files:");
		Console.WriteLine($"  {_outputDir}/README.md");
		Console.WriteLine($"  {_outputDir}/ABTestBase.cs");
		Console.WriteLine($"  {_outputDir}/NativeDllLoader.cs");

		foreach (var dll in dllsToProcess)
		{
			if (dll.MissingFunctions.Count > 0 || dll.StubFunctions.Count > 0)
			{
				Console.WriteLine($"  {_outputDir}/{GetTestFileName(dll.DllName)}");
			}
		}
	}

	private async Task GenerateReadmeAsync()
	{
		var readme = new StringBuilder();
		readme.AppendLine("# Generated A-B Tests for Native DLL Functions");
		readme.AppendLine();
		readme.AppendLine("This directory contains auto-generated test scaffolding for Win32 API functions.");
		readme.AppendLine("These tests enable **A-B testing** where Win32Emu's implementations are compared");
		readme.AppendLine("against native Windows DLL behavior.");
		readme.AppendLine();
		readme.AppendLine("## Purpose");
		readme.AppendLine();
		readme.AppendLine("These tests implement **Use Case #3** from `NATIVE_DLL_ANALYSIS.md`:");
		readme.AppendLine("Test-driven development using native DLL exports as specification.");
		readme.AppendLine();
		readme.AppendLine("## Test Structure");
		readme.AppendLine();
		readme.AppendLine("Each test follows this pattern:");
		readme.AppendLine();
		readme.AppendLine("1. **Setup**: Initialize test environment and parameters");
		readme.AppendLine("2. **Execute Both**: Call both Win32Emu and native DLL implementations");
		readme.AppendLine("3. **Compare**: Verify results match (return value, error codes, side effects)");
		readme.AppendLine();
		readme.AppendLine("## Running Tests");
		readme.AppendLine();
		readme.AppendLine("```bash");
		readme.AppendLine("# Run all generated tests");
		readme.AppendLine("dotnet test");
		readme.AppendLine();
		readme.AppendLine("# Run tests for specific DLL");
		readme.AppendLine("dotnet test --filter \"FullyQualifiedName~Kernel32ABTests\"");
		readme.AppendLine("```");
		readme.AppendLine();
		readme.AppendLine("## Platform Support");
		readme.AppendLine();
		readme.AppendLine("- **Windows**: Tests can load native DLLs directly via P/Invoke");
		readme.AppendLine("- **Linux/macOS**: Native DLL loading is disabled; tests only verify Win32Emu behavior");
		readme.AppendLine();
		readme.AppendLine("## Implementation Status");
		readme.AppendLine();
		readme.AppendLine("Tests are generated for:");
		readme.AppendLine("- **Missing functions**: Not yet implemented in Win32Emu");
		readme.AppendLine("- **Stub functions**: Placeholder implementations that need completion");
		readme.AppendLine();
		readme.AppendLine("## Test Categories");
		readme.AppendLine();
		readme.AppendLine("Tests are marked with traits:");
		readme.AppendLine("- `Category=ABTest`: All A-B comparison tests");
		readme.AppendLine("- `Category=NeedsImplementation`: Function not yet implemented");
		readme.AppendLine("- `Category=Stub`: Function has stub implementation");
		readme.AppendLine();
		readme.AppendLine("## Adding Implementation");
		readme.AppendLine();
		readme.AppendLine("1. Implement the function in the appropriate Win32 module");
		readme.AppendLine("2. Run the generated test to verify behavior matches native DLL");
		readme.AppendLine("3. Update test with proper test data and assertions");
		readme.AppendLine("4. Remove `Skip` attribute once test is fully implemented");
		readme.AppendLine();
		readme.AppendLine($"## Generated");
		readme.AppendLine();
		readme.AppendLine($"Generated on: {_report.AnalyzedAt}");
		readme.AppendLine($"Source: {_report.DllDirectory}");

		await File.WriteAllTextAsync(Path.Combine(_outputDir, "README.md"), readme.ToString());
	}

	private async Task GenerateTestInfrastructureAsync()
	{
		// Generate ABTestBase.cs
		var baseClass = GenerateABTestBase();
		await File.WriteAllTextAsync(Path.Combine(_outputDir, "ABTestBase.cs"), baseClass);

		// Generate NativeDllLoader.cs
		var loader = GenerateNativeDllLoader();
		await File.WriteAllTextAsync(Path.Combine(_outputDir, "NativeDllLoader.cs"), loader);
	}

	private string GenerateABTestBase()
	{
		var sb = new StringBuilder();
		sb.AppendLine("using Xunit;");
		sb.AppendLine();
		sb.AppendLine("namespace Win32Emu.Tests.Generated;");
		sb.AppendLine();
		sb.AppendLine("/// <summary>");
		sb.AppendLine("/// Base class for A-B tests that compare Win32Emu behavior against native Windows DLLs.");
		sb.AppendLine("/// </summary>");
		sb.AppendLine("public abstract class ABTestBase : IDisposable");
		sb.AppendLine("{");
		sb.AppendLine("\tprotected readonly bool _nativeAvailable;");
		sb.AppendLine("\tprotected readonly NativeDllLoader? _nativeLoader;");
		sb.AppendLine();
		sb.AppendLine("\tprotected ABTestBase(string dllName)");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t// Only load native DLLs on Windows");
		sb.AppendLine("\t\tif (OperatingSystem.IsWindows())");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\ttry");
		sb.AppendLine("\t\t\t{");
		sb.AppendLine("\t\t\t\t_nativeLoader = new NativeDllLoader(dllName);");
		sb.AppendLine("\t\t\t\t_nativeAvailable = true;");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t\tcatch");
		sb.AppendLine("\t\t\t{");
		sb.AppendLine("\t\t\t\t_nativeAvailable = false;");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.AppendLine("\tpublic void Dispose()");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t_nativeLoader?.Dispose();");
		sb.AppendLine("\t\tGC.SuppressFinalize(this);");
		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.AppendLine("\tprotected void AssertABMatch<T>(string functionName, T win32EmuResult, T nativeResult)");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tif (_nativeAvailable)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tAssert.Equal(nativeResult, win32EmuResult);");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\telse");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t// Native DLL not available, just document Win32Emu behavior");
		sb.AppendLine("\t\t\t// This happens on Linux/macOS CI");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");

		return sb.ToString();
	}

	private string GenerateNativeDllLoader()
	{
		var sb = new StringBuilder();
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Runtime.InteropServices;");
		sb.AppendLine("using EasyHook;");
		sb.AppendLine();
		sb.AppendLine("namespace Win32Emu.Tests.Generated;");
		sb.AppendLine();
		sb.AppendLine("/// <summary>");
		sb.AppendLine("/// Loads native Windows DLLs for A-B testing using EasyHook.");
		sb.AppendLine("/// Only works on Windows; gracefully degrades on other platforms.");
		sb.AppendLine("/// </summary>");
		sb.AppendLine("public class NativeDllLoader : IDisposable");
		sb.AppendLine("{");
		sb.AppendLine("\tprivate readonly IntPtr _handle;");
		sb.AppendLine("\tprivate readonly string _dllName;");
		sb.AppendLine();
		sb.AppendLine("\tpublic NativeDllLoader(string dllName)");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t_dllName = dllName;");
		sb.AppendLine();
		sb.AppendLine("\t\tif (!OperatingSystem.IsWindows())");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tthrow new PlatformNotSupportedException(\"Native DLL loading only supported on Windows\");");
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.AppendLine("\t\t// Load the native DLL using EasyHook's NativeAPI");
		sb.AppendLine("\t\t_handle = NativeAPI.LoadLibrary(dllName);");
		sb.AppendLine("\t\tif (_handle == IntPtr.Zero)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tthrow new DllNotFoundException($\"Could not load native DLL: {dllName}\");");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.AppendLine("\tpublic IntPtr GetProcAddress(string functionName)");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t// Use P/Invoke GetProcAddress since EasyHook's LocalHook.GetProcAddress");
		sb.AppendLine("\t\t// takes a module name string, not a handle");
		sb.AppendLine("\t\tvar address = GetProcAddressInternal(_handle, functionName);");
		sb.AppendLine("\t\tif (address == IntPtr.Zero)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tthrow new EntryPointNotFoundException($\"Function {functionName} not found in {_dllName}\");");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\treturn address;");
		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.AppendLine("\tpublic void Dispose()");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tif (_handle != IntPtr.Zero)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t// EasyHook doesn't provide FreeLibrary, use P/Invoke");
		sb.AppendLine("\t\t\tFreeLibrary(_handle);");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\tGC.SuppressFinalize(this);");
		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.AppendLine("\t[DllImport(\"kernel32.dll\", SetLastError = true)]");
		sb.AppendLine("\tprivate static extern bool FreeLibrary(IntPtr hModule);");
		sb.AppendLine();
		sb.AppendLine("\t[DllImport(\"kernel32.dll\", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = \"GetProcAddress\")]");
		sb.AppendLine("\tprivate static extern IntPtr GetProcAddressInternal(IntPtr hModule, string lpProcName);");
		sb.AppendLine("}");

		return sb.ToString();
	}

	private async Task GenerateDllTestsAsync(DllAnalysis dll)
	{
		var sb = new StringBuilder();
		var dllNameWithoutExtension = Path.GetFileNameWithoutExtension(dll.DllName);
		var className = $"{dllNameWithoutExtension}ABTests";

		sb.AppendLine("using Xunit;");
		sb.AppendLine();
		sb.AppendLine("namespace Win32Emu.Tests.Generated;");
		sb.AppendLine();
		sb.AppendLine("/// <summary>");
		sb.AppendLine($"/// A-B tests for {dll.DllName} functions.");
		sb.AppendLine("/// These tests compare Win32Emu implementations against native Windows DLL behavior.");
		sb.AppendLine("/// </summary>");
		sb.AppendLine($"public class {className} : ABTestBase");
		sb.AppendLine("{");
		sb.AppendLine($"\tpublic {className}() : base(\"{dll.DllName}\")");
		sb.AppendLine("\t{");
		sb.AppendLine("\t}");

		// Generate tests for missing functions
		foreach (var function in dll.MissingFunctions.Take(10)) // Limit to first 10 for now
		{
			sb.AppendLine();
			GenerateFunctionTest(sb, function, "NeedsImplementation");
		}

		// Generate tests for stub functions
		foreach (var function in dll.StubFunctions.Take(10)) // Limit to first 10 for now
		{
			sb.AppendLine();
			GenerateFunctionTest(sb, function, "Stub");
		}

		sb.AppendLine("}");

		var fileName = GetTestFileName(dll.DllName);
		await File.WriteAllTextAsync(Path.Combine(_outputDir, fileName), sb.ToString());
	}

	private void GenerateFunctionTest(StringBuilder sb, string functionName, string category)
	{
		sb.AppendLine("\t[Fact]");
		sb.AppendLine($"\t[Trait(\"Category\", \"ABTest\")]");
		sb.AppendLine($"\t[Trait(\"Category\", \"{category}\")]");
		sb.AppendLine($"\t[Trait(\"Function\", \"{functionName}\")]");
		sb.AppendLine($"\tpublic void {SafeTestName(functionName)}_ShouldMatchNativeBehavior()");
		sb.AppendLine("\t{");
		sb.AppendLine($"\t\t// TODO: Implement test for {functionName}");
		sb.AppendLine("\t\t// 1. Setup test parameters");
		sb.AppendLine("\t\t// 2. Call Win32Emu implementation");
		sb.AppendLine("\t\t// 3. Call native DLL implementation (if available)");
		sb.AppendLine("\t\t// 4. Compare results using AssertABMatch");
		sb.AppendLine();
		sb.AppendLine($"\t\tSkip.If(true, \"Test not yet implemented for {functionName}\");");
		sb.AppendLine("\t}");
	}

	private string SafeTestName(string functionName)
	{
		// Remove characters that aren't valid in C# identifiers
		var safe = new StringBuilder();
		foreach (var c in functionName)
		{
			if (char.IsLetterOrDigit(c) || c == '_')
			{
				safe.Append(c);
			}
			else
			{
				safe.Append('_');
			}
		}
		return safe.ToString();
	}

	private string GetTestFileName(string dllName)
	{
		var nameWithoutExtension = Path.GetFileNameWithoutExtension(dllName);
		return $"{nameWithoutExtension}ABTests.cs";
	}
}
