using System.Text.Json;
using System.Text.RegularExpressions;

namespace Win32Emu.Tools.ApiStatusGenerator;

/// <summary>
/// Generates JSON data about Win32 module API implementation status
/// for use in GitHub Pages documentation.
/// </summary>
class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Win32Emu API Status Generator");
		Console.WriteLine("==============================");
		Console.WriteLine();

		if (args.Length < 2)
		{
			Console.WriteLine("Usage: Win32Emu.Tools.ApiStatusGenerator <win32emu-modules-dir> <output-json-path>");
			Console.WriteLine();
			Console.WriteLine("Example:");
			Console.WriteLine("  Win32Emu.Tools.ApiStatusGenerator Win32Emu/Win32/Modules docs/pages/api-status.json");
			return;
		}

		var modulesDir = args[0];
		var outputPath = args[1];

		if (!Directory.Exists(modulesDir))
		{
			Console.WriteLine($"Error: Modules directory not found: {modulesDir}");
			return;
		}

		var outputDir = Path.GetDirectoryName(outputPath);
		if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
		{
			Directory.CreateDirectory(outputDir);
		}

		var generator = new ApiStatusGenerator(modulesDir);
		var apiData = generator.Generate();
		
		var json = JsonSerializer.Serialize(apiData, new JsonSerializerOptions 
		{ 
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});
		
		File.WriteAllText(outputPath, json);
		
		Console.WriteLine();
		Console.WriteLine($"Generated API status data: {outputPath}");
		Console.WriteLine($"Total modules: {apiData.Modules.Count}");
		Console.WriteLine($"Total functions: {apiData.Modules.Sum(m => m.Functions.Count)}");
		Console.WriteLine($"Stub functions: {apiData.Modules.Sum(m => m.Functions.Count(f => f.IsStub))}");
	}
}

/// <summary>
/// Model for API status data
/// </summary>
public class ApiStatusData
{
	public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("O");
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

/// <summary>
/// Generates API status data from Win32 module source files
/// </summary>
class ApiStatusGenerator
{
	private readonly string _modulesDir;

	public ApiStatusGenerator(string modulesDir)
	{
		_modulesDir = modulesDir;
	}

	public ApiStatusData Generate()
	{
		Console.WriteLine("Scanning modules directory...");
		
		var data = new ApiStatusData();
		var moduleFiles = Directory.GetFiles(_modulesDir, "*Module.cs")
			.OrderBy(f => Path.GetFileName(f))
			.ToList();

		Console.WriteLine($"Found {moduleFiles.Count} module files");
		Console.WriteLine();

		foreach (var moduleFile in moduleFiles)
		{
			try
			{
				var moduleInfo = ParseModule(moduleFile);
				if (moduleInfo != null && moduleInfo.Functions.Count > 0)
				{
					data.Modules.Add(moduleInfo);
					Console.WriteLine($"  {moduleInfo.Name}: {moduleInfo.Functions.Count} functions ({moduleInfo.Functions.Count(f => f.IsStub)} stubs)");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Warning: Failed to parse {Path.GetFileName(moduleFile)}: {ex.Message}");
			}
		}

		return data;
	}

	private ModuleInfo? ParseModule(string moduleFile)
	{
		var content = File.ReadAllText(moduleFile);
		var fileName = Path.GetFileNameWithoutExtension(moduleFile);
		
		// Extract class name (e.g., "Kernel32Module")
		var className = fileName;
		
		// Extract DLL name from class property or infer from filename
		string? dllName = ExtractDllName(content, className);
		
		if (string.IsNullOrEmpty(dllName))
		{
			// Infer from filename: "Kernel32Module" -> "KERNEL32.DLL"
			if (fileName.EndsWith("Module", StringComparison.OrdinalIgnoreCase))
			{
				var baseName = fileName.Substring(0, fileName.Length - 6);
				dllName = baseName.ToUpperInvariant() + ".DLL";
			}
			else
			{
				return null;
			}
		}

		var moduleInfo = new ModuleInfo
		{
			Name = dllName,
			ClassName = className
		};

		// Parse functions with [DllModuleExport] attributes
		var functions = ParseFunctions(content);
		moduleInfo.Functions.AddRange(functions);

		return moduleInfo;
	}

	private string? ExtractDllName(string content, string className)
	{
		// Look for: public string Name => "KERNEL32.DLL";
		var namePattern = new Regex(@"public\s+string\s+Name\s*=>\s*""([^""]+)""");
		var match = namePattern.Match(content);
		return match.Success ? match.Groups[1].Value : null;
	}

	private List<FunctionInfo> ParseFunctions(string content)
	{
		var functions = new List<FunctionInfo>();
		
		// Pattern to match [DllModuleExport] attributes followed by public methods
		// This handles multi-line attributes and various attribute formats
		var pattern = new Regex(
			@"\[DllModuleExport\((?<ordinal>\d+)(?:,\s*entryPoint:\s*0x[0-9A-Fa-f]+)?(?:,\s*Version\s*=\s*""(?<version>[^""]+)"")?(?:,\s*IsStub\s*=\s*(?<isStub>true|false))?(?:,\s*ExportName\s*=\s*""(?<exportName>[^""]+)"")?(?:,\s*ForwardedTo\s*=\s*""(?<forwardedTo>[^""]+)"")?\)\]\s*(?:\[DllModuleExport[^\]]*\]\s*)*public\s+\w+\s+(?<methodName>\w+)\s*\(",
			RegexOptions.Multiline
		);

		var matches = pattern.Matches(content);
		
		// Track functions by name to merge multiple attribute declarations
		var functionMap = new Dictionary<string, FunctionInfo>();

		foreach (Match match in matches)
		{
			var methodName = match.Groups["methodName"].Value;
			var ordinal = uint.Parse(match.Groups["ordinal"].Value);
			var isStub = match.Groups["isStub"].Success && match.Groups["isStub"].Value == "true";
			var version = match.Groups["version"].Success ? match.Groups["version"].Value : null;
			var exportName = match.Groups["exportName"].Success ? match.Groups["exportName"].Value : null;
			var forwardedTo = match.Groups["forwardedTo"].Success ? match.Groups["forwardedTo"].Value : null;

			if (!functionMap.TryGetValue(methodName, out var functionInfo))
			{
				functionInfo = new FunctionInfo
				{
					Name = methodName,
					IsStub = isStub,
					Ordinal = ordinal,
					Version = version,
					ExportName = exportName,
					ForwardedTo = forwardedTo
				};
				functionMap[methodName] = functionInfo;
			}
			else
			{
				// Update with additional info if this attribute has more details
				if (isStub)
					functionInfo.IsStub = true;
				if (!string.IsNullOrEmpty(exportName))
					functionInfo.ExportName = exportName;
				if (!string.IsNullOrEmpty(forwardedTo))
					functionInfo.ForwardedTo = forwardedTo;
			}
		}

		// For modules that don't use [DllModuleExport], fall back to switch case parsing
		if (functionMap.Count == 0)
		{
			functions.AddRange(ParseSwitchCaseFunctions(content));
		}
		else
		{
			functions.AddRange(functionMap.Values);
		}

		return functions.OrderBy(f => f.Name).ToList();
	}

	private List<FunctionInfo> ParseSwitchCaseFunctions(string content)
	{
		// Pattern to match case statements in TryInvokeUnsafe methods
		// Example: case "GETVERSION":
		var pattern = new Regex(@"case\s+""([A-Z0-9_]+)"":", RegexOptions.Multiline);
		var matches = pattern.Matches(content);

		var functions = new List<FunctionInfo>();
		var seen = new HashSet<string>();

		foreach (Match match in matches)
		{
			var name = match.Groups[1].Value;
			// Convert from uppercase to PascalCase approximation
			var displayName = ConvertToPascalCase(name);
			
			if (seen.Add(displayName))
			{
				functions.Add(new FunctionInfo
				{
					Name = displayName,
					IsStub = false // Assume not stub if implemented in switch
				});
			}
		}

		return functions;
	}

	private string ConvertToPascalCase(string upperCase)
	{
		// Simple conversion: "GETVERSION" -> "GetVersion"
		// "CREATEWINDOWEXA" -> "CreateWindowExA"
		
		var parts = upperCase.Split('_');
		var result = string.Join("", parts.Select(part =>
		{
			if (part.Length == 0) return "";
			if (part.Length == 1) return part;
			
			// Keep trailing A/W as uppercase
			if (part.EndsWith("A") || part.EndsWith("W"))
			{
				var main = part.Substring(0, part.Length - 1);
				return char.ToUpper(main[0]) + main.Substring(1).ToLower() + part[^1];
			}
			
			return char.ToUpper(part[0]) + part.Substring(1).ToLower();
		}));

		return result;
	}
}
