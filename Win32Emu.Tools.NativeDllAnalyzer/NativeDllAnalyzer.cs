using System.Text.Json;
using PeNet;

namespace Win32Emu.Tools.NativeDllAnalyzer;

/// <summary>
/// Analyzes native DLLs and compares their exports with Win32Emu's API implementation status.
/// </summary>
public class NativeDllAnalyzer
{
	private readonly string _dllDirectory;
	private readonly ApiStatusData _apiStatus;

	public NativeDllAnalyzer(string dllDirectory, string apiStatusFile)
	{
		_dllDirectory = dllDirectory;

		var json = File.ReadAllText(apiStatusFile);
		_apiStatus = JsonSerializer.Deserialize<ApiStatusData>(json, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		}) ?? throw new Exception("Failed to deserialize API status data");
	}

	public async Task<NativeDllAnalysisReport> AnalyzeAsync()
	{
		await Task.CompletedTask; // Make async for future enhancements

		var report = new NativeDllAnalysisReport
		{
			AnalyzedAt = DateTime.UtcNow.ToString("O"),
			DllDirectory = _dllDirectory
		};

		// Find all DLL files
		var dllFiles = Directory.GetFiles(_dllDirectory, "*.DLL", SearchOption.TopDirectoryOnly)
			.Concat(Directory.GetFiles(_dllDirectory, "*.dll", SearchOption.TopDirectoryOnly))
			.Distinct()
			.OrderBy(f => f)
			.ToList();

		Console.WriteLine($"Found {dllFiles.Count} DLL files to analyze...");
		Console.WriteLine();

		foreach (var dllPath in dllFiles)
		{
			var dllName = Path.GetFileName(dllPath).ToUpperInvariant();
			Console.WriteLine($"Analyzing {dllName}...");

			try
			{
				var dllAnalysis = AnalyzeSingleDll(dllPath);
				if (dllAnalysis != null)
				{
					report.Dlls.Add(dllAnalysis);
				}
			}
			catch (Exception ex) when (
				ex is not OutOfMemoryException &&
				ex is not StackOverflowException &&
				ex is not System.Threading.ThreadAbortException)
			{
				Console.WriteLine($"  [ERROR] Failed to analyze: {ex.Message}");
			}
		}

		// Calculate summary statistics
		report.Summary.TotalDllsAnalyzed = report.Dlls.Count;
		report.Summary.TotalNativeExports = report.Dlls.Sum(d => d.NativeExports.Count);
		report.Summary.TotalImplemented = report.Dlls.Sum(d => d.ImplementedFunctions.Count);
		report.Summary.TotalStubs = report.Dlls.Sum(d => d.StubFunctions.Count);
		report.Summary.TotalMissing = report.Dlls.Sum(d => d.MissingFunctions.Count);

		if (report.Summary.TotalNativeExports > 0)
		{
			report.Summary.ImplementationPercentage =
				(report.Summary.TotalImplemented * 100.0 / report.Summary.TotalNativeExports);
		}

		return report;
	}

	private DllAnalysis? AnalyzeSingleDll(string dllPath)
	{
		var peFile = new PeFile(dllPath);
		var fileName = Path.GetFileName(dllPath).ToUpperInvariant();

		// Skip 64-bit DLLs
		if (peFile.Is64Bit)
		{
			Console.WriteLine($"  [SKIP] 64-bit DLL");
			return null;
		}

		// Get exported functions
		var exports = peFile.ExportedFunctions;
		if (exports == null || !exports.Any())
		{
			Console.WriteLine($"  [INFO] No exports found");
			return null;
		}

		var analysis = new DllAnalysis
		{
			DllName = fileName
		};

		// Extract export names (filter out ordinal-only exports)
		var exportList = exports
			.Where(e => !string.IsNullOrEmpty(e.Name))
			.Select(e => new ExportInfo
			{
				Name = e.Name ?? "",
				Ordinal = e.Ordinal
			})
			.OrderBy(e => e.Name)
			.ToList();

		analysis.NativeExports = exportList;

		// Find corresponding Win32Emu module
		var module = _apiStatus.Modules
			.FirstOrDefault(m => m.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

		if (module == null)
		{
			Console.WriteLine($"  [INFO] Module not implemented in Win32Emu");
			// All exports are missing
			analysis.MissingFunctions = exportList.Select(e => e.Name).ToList();
		}
		else
		{
			// Compare exports with Win32Emu implementation
			foreach (var export in exportList)
			{
				var function = module.Functions.FirstOrDefault(f =>
					f.Name.Equals(export.Name, StringComparison.OrdinalIgnoreCase));

				if (function == null)
				{
					analysis.MissingFunctions.Add(export.Name);
				}
				else if (function.IsStub)
				{
					analysis.StubFunctions.Add(export.Name);
				}
				else
				{
					analysis.ImplementedFunctions.Add(export.Name);
				}
			}

			// Also find Win32Emu functions that don't exist in native DLL (extra implementations)
			var nativeExportNames = new HashSet<string>(
				exportList.Select(e => e.Name),
				StringComparer.OrdinalIgnoreCase);

			analysis.ExtraImplementations = module.Functions
				.Where(f => !nativeExportNames.Contains(f.Name))
				.Select(f => f.Name)
				.ToList();
		}

		// Calculate coverage percentage
		var totalExports = analysis.NativeExports.Count;
		if (totalExports > 0)
		{
			analysis.CoveragePercentage =
				(analysis.ImplementedFunctions.Count * 100.0 / totalExports);
		}

		Console.WriteLine($"  Native exports: {analysis.NativeExports.Count}");
		Console.WriteLine($"  Implemented: {analysis.ImplementedFunctions.Count}");
		Console.WriteLine($"  Stubs: {analysis.StubFunctions.Count}");
		Console.WriteLine($"  Missing: {analysis.MissingFunctions.Count}");
		Console.WriteLine($"  Coverage: {analysis.CoveragePercentage:F1}%");

		return analysis;
	}
}

// Reuse the same data models from ApiStatusGenerator and PeAnalyzer
public class ApiStatusData
{
	public string GeneratedAt { get; set; } = "";
	public List<ModuleInfo> Modules { get; set; } = new();
}

public class ModuleInfo
{
	public string Name { get; set; } = "";
	public string ClassName { get; set; } = "";
	public List<FunctionInfo> Functions { get; set; } = new();
}

public class FunctionInfo
{
	public string Name { get; set; } = "";
	public bool IsStub { get; set; }
	public uint? Ordinal { get; set; }
	public string? Version { get; set; }
	public string? ExportName { get; set; }
	public string? ForwardedTo { get; set; }
}
