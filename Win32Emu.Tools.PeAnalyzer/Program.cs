using System.Text.Json;
using PeNet;

namespace Win32Emu.Tools.PeAnalyzer;

/// <summary>
/// Analyzes PE executables and checks compatibility with Win32Emu implementation status.
/// Uses PeNet library to parse PE imports and cross-references with api-status.json.
/// </summary>
class Program
{
	static async Task<int> Main(string[] args)
	{
		Console.WriteLine("Win32Emu PE Compatibility Analyzer");
		Console.WriteLine("===================================");
		Console.WriteLine();

		if (args.Length < 2)
		{
			Console.WriteLine("Usage: Win32Emu.Tools.PeAnalyzer <pe-file> <api-status-json>");
			Console.WriteLine();
			Console.WriteLine("Example:");
			Console.WriteLine("  Win32Emu.Tools.PeAnalyzer game.exe docs/pages/api-status.json");
			Console.WriteLine();
			Console.WriteLine("Output: JSON report of PE compatibility with Win32Emu");
			return 1;
		}

		var peFile = args[0];
		var apiStatusFile = args[1];

		if (!File.Exists(peFile))
		{
			Console.WriteLine($"Error: PE file not found: {peFile}");
			return 1;
		}

		if (!File.Exists(apiStatusFile))
		{
			Console.WriteLine($"Error: API status file not found: {apiStatusFile}");
			return 1;
		}

		try
		{
			var analyzer = new PeCompatibilityAnalyzer(peFile, apiStatusFile);
			var report = await analyzer.AnalyzeAsync();
			
			// Output JSON report
			var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
			{ 
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
			
			Console.WriteLine(json);
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
}

public class CompatibilityReport
{
	public string FileName { get; set; } = "";
	public long FileSize { get; set; }
	public bool Is32Bit { get; set; }
	public bool Is64Bit { get; set; }
	public string AnalyzedAt { get; set; } = DateTime.UtcNow.ToString("O");
	public OverallStatus Status { get; set; } = new();
	public List<DllDependency> Dependencies { get; set; } = new();
}

public class OverallStatus
{
	public int TotalDlls { get; set; }
	public int TotalFunctions { get; set; }
	public int ImplementedFunctions { get; set; }
	public int StubFunctions { get; set; }
	public int MissingFunctions { get; set; }
	public double ImplementationPercentage { get; set; }
	public string Verdict { get; set; } = "";
}

public class DllDependency
{
	public string DllName { get; set; } = "";
	public bool IsSupported { get; set; }
	public List<FunctionStatus> Functions { get; set; } = new();
	public int ImplementedCount { get; set; }
	public int StubCount { get; set; }
	public int MissingCount { get; set; }
	public double ImplementationPercentage { get; set; }
}

public class FunctionStatus
{
	public string Name { get; set; } = "";
	public string Status { get; set; } = ""; // "implemented", "stub", "missing"
	public uint? Ordinal { get; set; }
}

class PeCompatibilityAnalyzer
{
	private readonly string _peFile;
	private readonly ApiStatusData _apiStatus;

	public PeCompatibilityAnalyzer(string peFile, string apiStatusFile)
	{
		_peFile = peFile;
		
		var json = File.ReadAllText(apiStatusFile);
		_apiStatus = JsonSerializer.Deserialize<ApiStatusData>(json, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		}) ?? throw new Exception("Failed to deserialize API status data");
	}

	public async Task<CompatibilityReport> AnalyzeAsync()
	{
		await Task.CompletedTask; // Make async for future enhancements
		
		var peFile = new PeFile(_peFile);
		var fileInfo = new FileInfo(_peFile);

		var report = new CompatibilityReport
		{
			FileName = fileInfo.Name,
			FileSize = fileInfo.Length,
			Is32Bit = !peFile.Is64Bit,
			Is64Bit = peFile.Is64Bit
		};

		// Parse import table
		// Only support 32-bit for Win32Emu
		if (peFile.Is64Bit)
		{
			report.Status.Verdict = "INCOMPATIBLE - 64-bit PE files are not supported by Win32Emu";
			return report;
		}

		var imports = peFile.ImportedFunctions;
		if (imports == null || !imports.Any())
		{
			report.Status.Verdict = "No imports found - may be statically linked or packed";
			return report;
		}

		// Group imports by DLL
		var importsByDll = imports
			.Where(i => !string.IsNullOrEmpty(i.DLL) && !string.IsNullOrEmpty(i.Name))
			.GroupBy(i => i.DLL.ToUpperInvariant())
			.OrderBy(g => g.Key)
			.ToList();

		foreach (var dllGroup in importsByDll)
		{
			var dllName = dllGroup.Key;
			var functions = dllGroup.ToList();

			// Find matching module in API status
			var module = _apiStatus.Modules
				.FirstOrDefault(m => m.Name.Equals(dllName, StringComparison.OrdinalIgnoreCase));

			var dependency = new DllDependency
			{
				DllName = dllName,
				IsSupported = module != null
			};

			foreach (var import in functions)
			{
				var functionName = import.Name ?? "";
				var ordinal = import.Hint; // Hint is the ordinal in PeNet

				FunctionStatus status;
				
				if (module == null)
				{
					// DLL not supported at all
					status = new FunctionStatus
					{
						Name = functionName,
						Status = "missing",
						Ordinal = ordinal
					};
					dependency.MissingCount++;
				}
				else
				{
					// Check if function is implemented
					var function = module.Functions.FirstOrDefault(f => 
						f.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase));

					if (function == null)
					{
						status = new FunctionStatus
						{
							Name = functionName,
							Status = "missing",
							Ordinal = ordinal
						};
						dependency.MissingCount++;
					}
					else if (function.IsStub)
					{
						status = new FunctionStatus
						{
							Name = functionName,
							Status = "stub",
							Ordinal = ordinal
						};
						dependency.StubCount++;
					}
					else
					{
						status = new FunctionStatus
						{
							Name = functionName,
							Status = "implemented",
							Ordinal = ordinal
						};
						dependency.ImplementedCount++;
					}
				}

				dependency.Functions.Add(status);
			}

			var totalFunctions = dependency.Functions.Count;
			dependency.ImplementationPercentage = totalFunctions > 0
				? (dependency.ImplementedCount * 100.0 / totalFunctions)
				: 0;

			report.Dependencies.Add(dependency);
		}

		// Calculate overall statistics
		report.Status.TotalDlls = report.Dependencies.Count;
		report.Status.TotalFunctions = report.Dependencies.Sum(d => d.Functions.Count);
		report.Status.ImplementedFunctions = report.Dependencies.Sum(d => d.ImplementedCount);
		report.Status.StubFunctions = report.Dependencies.Sum(d => d.StubCount);
		report.Status.MissingFunctions = report.Dependencies.Sum(d => d.MissingCount);
		
		if (report.Status.TotalFunctions > 0)
		{
			report.Status.ImplementationPercentage = 
				(report.Status.ImplementedFunctions * 100.0 / report.Status.TotalFunctions);
		}

		// Determine verdict
		if (report.Status.MissingFunctions == 0 && report.Status.StubFunctions == 0)
		{
			report.Status.Verdict = "FULLY COMPATIBLE - All required APIs are implemented";
		}
		else if (report.Status.MissingFunctions == 0 && report.Status.StubFunctions > 0)
		{
			report.Status.Verdict = $"MOSTLY COMPATIBLE - {report.Status.StubFunctions} stub(s) may affect functionality";
		}
		else if (report.Status.ImplementationPercentage >= 80)
		{
			report.Status.Verdict = $"PARTIALLY COMPATIBLE - {report.Status.MissingFunctions} missing function(s)";
		}
		else
		{
			report.Status.Verdict = $"LIMITED COMPATIBILITY - {report.Status.MissingFunctions} missing function(s)";
		}

		return report;
	}
}

// Reuse the same data models from ApiStatusGenerator
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
