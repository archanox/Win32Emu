using System.Text.Json;
using PeNet;

namespace Win32Emu.Tools.NativeDllAnalyzer;

/// <summary>
/// Analyzes native Windows DLLs to extract exported functions and compare with Win32Emu implementations.
/// This tool helps identify missing function implementations by comparing native DLL exports
/// with Win32Emu's current API status.
/// </summary>
class Program
{
	static async Task<int> Main(string[] args)
	{
		Console.WriteLine("Win32Emu Native DLL Analyzer");
		Console.WriteLine("=============================");
		Console.WriteLine();

		if (args.Length < 2)
		{
			Console.WriteLine("Usage: Win32Emu.Tools.NativeDllAnalyzer <dll-directory> <api-status-json> [output-json]");
			Console.WriteLine();
			Console.WriteLine("Arguments:");
			Console.WriteLine("  dll-directory    - Directory containing native DLLs (e.g., DLLs/WinME)");
			Console.WriteLine("  api-status-json  - API status JSON file from ApiStatusGenerator");
			Console.WriteLine("  output-json      - Optional output file for missing functions report");
			Console.WriteLine();
			Console.WriteLine("Examples:");
			Console.WriteLine("  Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json");
			Console.WriteLine("  Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json docs/pages/missing-functions.json");
			Console.WriteLine();
			Console.WriteLine("This tool extracts exported functions from native DLLs and");
			Console.WriteLine("compares them with Win32Emu's implementations to identify gaps.");
			return 1;
		}

		var dllDirectory = args[0];
		var apiStatusFile = args[1];
		var outputFile = args.Length > 2 ? args[2] : null;

		if (!Directory.Exists(dllDirectory))
		{
			Console.WriteLine($"Error: DLL directory not found: {dllDirectory}");
			return 1;
		}

		if (!File.Exists(apiStatusFile))
		{
			Console.WriteLine($"Error: API status file not found: {apiStatusFile}");
			return 1;
		}

		try
		{
			var analyzer = new NativeDllAnalyzer(dllDirectory, apiStatusFile);
			var report = await analyzer.AnalyzeAsync();

			// Print summary to console
			PrintReport(report);

			// Write JSON output if requested
			if (outputFile != null)
			{
				var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
				{
					WriteIndented = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				var outputDir = Path.GetDirectoryName(outputFile);
				if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
				{
					Directory.CreateDirectory(outputDir);
				}

				await File.WriteAllTextAsync(outputFile, json);
				Console.WriteLine();
				Console.WriteLine($"Report written to: {outputFile}");
			}

			return 0;
		}
		catch (Exception ex) when (
			ex is not OutOfMemoryException &&
			ex is not StackOverflowException &&
			ex is not System.Threading.ThreadAbortException)
		{
			Console.WriteLine($"Error: {ex.Message}");
			return 1;
		}
	}

	static void PrintReport(NativeDllAnalysisReport report)
	{
		Console.WriteLine();
		Console.WriteLine("=".PadRight(70, '='));
		Console.WriteLine("ANALYSIS SUMMARY");
		Console.WriteLine("=".PadRight(70, '='));
		Console.WriteLine();

		Console.WriteLine($"Total DLLs analyzed: {report.Summary.TotalDllsAnalyzed}");
		Console.WriteLine($"Total native exports: {report.Summary.TotalNativeExports}");
		Console.WriteLine($"Total implemented: {report.Summary.TotalImplemented}");
		Console.WriteLine($"Total stubs: {report.Summary.TotalStubs}");
		Console.WriteLine($"Total missing: {report.Summary.TotalMissing}");
		Console.WriteLine($"Implementation rate: {report.Summary.ImplementationPercentage:F1}%");
		Console.WriteLine();

		// Show top DLLs with missing functions
		var dllsWithMissing = report.Dlls
			.Where(d => d.MissingFunctions.Any())
			.OrderByDescending(d => d.MissingFunctions.Count)
			.Take(5)
			.ToList();

		if (dllsWithMissing.Any())
		{
			Console.WriteLine("Top DLLs with missing functions:");
			Console.WriteLine();
			foreach (var dll in dllsWithMissing)
			{
				Console.WriteLine($"  {dll.DllName}:");
				Console.WriteLine($"    Native exports: {dll.NativeExports.Count}");
				Console.WriteLine($"    Implemented: {dll.ImplementedFunctions.Count}");
				Console.WriteLine($"    Stubs: {dll.StubFunctions.Count}");
				Console.WriteLine($"    Missing: {dll.MissingFunctions.Count}");
				Console.WriteLine($"    Coverage: {dll.CoveragePercentage:F1}%");
				Console.WriteLine();
			}
		}

		Console.WriteLine("Run with output file path to save detailed report as JSON.");
	}
}
