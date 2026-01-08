using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Win32Emu.Tools.DecompToCS;

/// <summary>
/// Transpiler that converts decompiled C++ code from various decompilers
/// (Hex-Rays, Ghidra, Binary Ninja, etc.) into executable C# code.
/// </summary>
class Program
{
	static async Task<int> Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine("Win32Emu Decompilation to C# Transpiler");
			Console.WriteLine();
			Console.WriteLine("Converts decompiled C++ code into executable C# for debugging and analysis.");
			Console.WriteLine();
			Console.WriteLine("Usage: Win32Emu.Tools.DecompToCS <decomp-file.cpp> [options]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("  --output <dir>        Output directory for C# files (default: ./CSharpOutput)");
			Console.WriteLine("  --format <format>     Decompiler format: hexrays, ghidra, binaryninja, retdec");
			Console.WriteLine("                        (auto-detected if not specified)");
			Console.WriteLine("  --exe <file>          Original executable for address mapping");
			Console.WriteLine("  --jit-cache           Generate JIT cache integration");
			Console.WriteLine("  --cache-dir <dir>     JIT cache directory (default: ./JitCache)");
			Console.WriteLine("  --namespace <ns>      C# namespace (default: Win32Emu.Generated)");
			Console.WriteLine("  --partial             Allow partial compilation (skip unresolved symbols)");
			Console.WriteLine("  --verbose             Enable verbose logging");
			Console.WriteLine();
			Console.WriteLine("Examples:");
			Console.WriteLine("  # Basic transpilation");
			Console.WriteLine("  Win32Emu.Tools.DecompToCS hexrays.cpp");
			Console.WriteLine();
			Console.WriteLine("  # With JIT cache integration");
			Console.WriteLine("  Win32Emu.Tools.DecompToCS hexrays.cpp --jit-cache --exe game.exe");
			Console.WriteLine();
			Console.WriteLine("  # For ign_teas investigation");
			Console.WriteLine("  Win32Emu.Tools.DecompToCS Decomp/ign_teas/hexrays.cpp \\");
			Console.WriteLine("      --exe EXEs/ign_teas.exe --jit-cache --output ./IgNTeasCS");
			Console.WriteLine();
			return 1;
		}

		var decompFile = args[0];
		var outputDir = GetArgument(args, "--output", "./CSharpOutput");
		var format = GetArgument(args, "--format", null);
		var exePath = GetArgument(args, "--exe", null);
		var jitCache = args.Contains("--jit-cache");
		var cacheDir = GetArgument(args, "--cache-dir", "./JitCache");
		var namespaceName = GetArgument(args, "--namespace", "Win32Emu.Generated");
		var partial = args.Contains("--partial");
		var verbose = args.Contains("--verbose");

		if (!File.Exists(decompFile))
		{
			Console.Error.WriteLine($"Error: Decompilation file not found: {decompFile}");
			return 1;
		}

		// Setup logging
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
		});

		var logger = loggerFactory.CreateLogger<Program>();

		logger.LogInformation("Win32Emu Decompilation to C# Transpiler");
		logger.LogInformation("Input: {File}", decompFile);
		logger.LogInformation("Output: {Dir}", outputDir);
		logger.LogInformation("Namespace: {Namespace}", namespaceName);

		try
		{
			// Detect decompiler format if not specified
			if (format == null)
			{
				format = DetectDecompilerFormat(decompFile, logger);
				logger.LogInformation("Detected decompiler format: {Format}", format);
			}

			// Read decompiled code
			var decompCode = await File.ReadAllTextAsync(decompFile);
			logger.LogInformation("Read {Size} bytes from decompilation file", decompCode.Length);

			// Parse and transpile
			var transpiler = new CppToCsTranspiler(
				format, 
				namespaceName, 
				partial, 
				logger);

			var functions = transpiler.ParseFunctions(decompCode);
			logger.LogInformation("Parsed {Count} functions", functions.Count);

			// Generate C# code
			Directory.CreateDirectory(outputDir);
			var generatedFiles = await transpiler.GenerateCSharpAsync(functions, outputDir);
			logger.LogInformation("Generated {Count} C# files", generatedFiles.Count);

			// Generate JIT cache integration if requested
			if (jitCache)
			{
				if (exePath == null)
				{
					logger.LogWarning("JIT cache requested but no executable specified. Cache will not include address mapping.");
				}

				Directory.CreateDirectory(cacheDir);
				await transpiler.GenerateJitCacheAsync(functions, cacheDir, exePath);
				logger.LogInformation("Generated JIT cache in: {Dir}", cacheDir);
			}

			// Generate project file
			await GenerateProjectFileAsync(outputDir, namespaceName, logger);

			logger.LogInformation("");
			logger.LogInformation("Transpilation complete!");
			logger.LogInformation("Output directory: {Dir}", Path.GetFullPath(outputDir));
			logger.LogInformation("");
			logger.LogInformation("Next steps:");
			logger.LogInformation("  1. Build: cd {Dir} && dotnet build", Path.GetFullPath(outputDir));
			if (jitCache)
			{
				logger.LogInformation("  2. Run with cache: Win32Emu.Gui --nogui {ExeName} --cache-dir {CacheDir}", 
					exePath != null ? Path.GetFileName(exePath) : "game.exe",
					Path.GetFullPath(cacheDir));
			}
			logger.LogInformation("  3. Debug: Open in Visual Studio and set breakpoints in Function_*.cs files");

			return 0;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Transpilation failed");
			return 1;
		}
	}

	static string? GetArgument(string[] args, string name, string? defaultValue)
	{
		for (var i = 0; i < args.Length - 1; i++)
		{
			if (args[i] == name)
			{
				return args[i + 1];
			}
		}
		return defaultValue;
	}

	static string DetectDecompilerFormat(string filePath, ILogger logger)
	{
		var content = File.ReadAllText(filePath);
		
		// Hex-Rays: "This file was generated by the Hex-Rays decompiler"
		if (content.Contains("Hex-Rays decompiler"))
		{
			return "hexrays";
		}

		// Ghidra: specific comment patterns and undefined type usage
		if (content.Contains("/* WARNING:") || content.Contains("undefined"))
		{
			return "ghidra";
		}

		// Binary Ninja: specific function signature patterns
		if (content.Contains("data_") || content.Contains("sub_"))
		{
			return "binaryninja";
		}

		// RetDec: verbose output with many type casts
		if (content.Contains("int32_t") && content.Contains("// 0x"))
		{
			return "retdec";
		}

		logger.LogWarning("Could not detect decompiler format, defaulting to hexrays");
		return "hexrays";
	}

	static async Task GenerateProjectFileAsync(string outputDir, string namespaceName, ILogger logger)
	{
		var projectContent = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>{namespaceName}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference to Win32Emu for EmulatorEnvironment -->
    <!-- Update path as needed -->
    <!-- <ProjectReference Include=""../../Win32Emu/Win32Emu.csproj"" /> -->
  </ItemGroup>

</Project>";

		var projectPath = Path.Combine(outputDir, $"{namespaceName}.csproj");
		await File.WriteAllTextAsync(projectPath, projectContent);
		logger.LogDebug("Generated project file: {Path}", projectPath);
	}
}

/// <summary>
/// Transpiler that converts C++ decompilation output to C# code.
/// </summary>
class CppToCsTranspiler
{
	private readonly string _format;
	private readonly string _namespace;
	private readonly bool _allowPartial;
	private readonly ILogger _logger;

	public CppToCsTranspiler(string format, string namespaceName, bool allowPartial, ILogger logger)
	{
		_format = format;
		_namespace = namespaceName;
		_allowPartial = allowPartial;
		_logger = logger;
	}

	public List<FunctionInfo> ParseFunctions(string decompCode)
	{
		var functions = new List<FunctionInfo>();

		// Parse function declarations
		// Pattern: return_type [__cdecl|__stdcall|etc] function_name(params)
		var functionPattern = @"(\w+)\s+(?:__(?:cdecl|stdcall|fastcall|thiscall))?\s*(\w+)\s*\((.*?)\)(?:\s*;\s*)?(?:\s*//\s*(.+))?";
		var matches = Regex.Matches(decompCode, functionPattern, RegexOptions.Multiline);

		_logger.LogDebug("Found {Count} potential function declarations", matches.Count);

		foreach (Match match in matches)
		{
			var returnType = match.Groups[1].Value;
			var functionName = match.Groups[2].Value;
			var parameters = match.Groups[3].Value;
			var comment = match.Groups.Count > 4 ? match.Groups[4].Value : "";

			// Skip if it's a standard library function or external API
			if (IsExternalFunction(functionName))
			{
				continue;
			}

			// Extract address from function name (e.g., sub_401000 -> 0x00401000)
			var address = ExtractAddressFromName(functionName);

			functions.Add(new FunctionInfo
			{
				Name = functionName,
				ReturnType = returnType,
				Parameters = parameters,
				Address = address,
				Comment = comment
			});
		}

		_logger.LogInformation("Parsed {Count} functions (excluding external APIs)", functions.Count);
		return functions;
	}

	public async Task<List<string>> GenerateCSharpAsync(List<FunctionInfo> functions, string outputDir)
	{
		var generatedFiles = new List<string>();

		foreach (var func in functions)
		{
			var csCode = GenerateCSharpFunction(func);
			var fileName = $"Function_{func.Address:X8}.cs";
			var filePath = Path.Combine(outputDir, fileName);
			
			await File.WriteAllTextAsync(filePath, csCode);
			generatedFiles.Add(filePath);
			
			_logger.LogDebug("Generated C# for function {Name} at 0x{Address:X8}", func.Name, func.Address);
		}

		return generatedFiles;
	}

	public async Task GenerateJitCacheAsync(List<FunctionInfo> functions, string cacheDir, string? exePath)
	{
		// Generate JIT cache metadata file
		var metadata = new StringBuilder();
		metadata.AppendLine("# JIT Cache Metadata");
		metadata.AppendLine($"# Generated from decompilation");
		metadata.AppendLine($"# Executable: {exePath ?? "unknown"}");
		metadata.AppendLine($"# Functions: {functions.Count}");
		metadata.AppendLine();

		foreach (var func in functions)
		{
			metadata.AppendLine($"Function: {func.Name}");
			metadata.AppendLine($"  Address: 0x{func.Address:X8}");
			metadata.AppendLine($"  ReturnType: {func.ReturnType}");
			metadata.AppendLine($"  Parameters: {func.Parameters}");
			metadata.AppendLine();
		}

		var metadataPath = Path.Combine(cacheDir, "decompilation_metadata.txt");
		await File.WriteAllTextAsync(metadataPath, metadata.ToString());
		
		_logger.LogInformation("Generated JIT cache metadata: {Path}", metadataPath);
	}

	private string GenerateCSharpFunction(FunctionInfo func)
	{
		var sb = new StringBuilder();

		// File header
		sb.AppendLine("using System;");
		sb.AppendLine("using Win32Emu;");
		sb.AppendLine();
		sb.AppendLine($"namespace {_namespace}");
		sb.AppendLine("{");

		// Class documentation
		sb.AppendLine("\t/// <summary>");
		sb.AppendLine($"\t/// Function at 0x{func.Address:X8}");
		sb.AppendLine($"\t/// Original name: {func.Name}");
		if (!string.IsNullOrEmpty(func.Comment))
		{
			sb.AppendLine($"\t/// Note: {func.Comment}");
		}
		sb.AppendLine("\t/// Decompiled from C++ and transpiled to C#");
		sb.AppendLine("\t/// </summary>");
		
		// Class declaration
		sb.AppendLine($"\tpublic class Function_{func.Address:X8}");
		sb.AppendLine("\t{");
		
		// Environment field
		sb.AppendLine("\t\tprivate readonly EmulatorEnvironment _env;");
		sb.AppendLine();
		
		// Constructor
		sb.AppendLine($"\t\tpublic Function_{func.Address:X8}(EmulatorEnvironment env)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t_env = env;");
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		
		// Execute method
		sb.AppendLine("\t\t/// <summary>");
		sb.AppendLine($"\t\t/// Execute function at 0x{func.Address:X8}");
		sb.AppendLine("\t\t/// </summary>");
		sb.AppendLine($"\t\t[OriginalAddress(0x{func.Address:X8})]");
		
		var csReturnType = MapCppTypeToCSharp(func.ReturnType);
		var csParams = MapParametersToCSharp(func.Parameters);
		
		sb.AppendLine($"\t\tpublic {csReturnType} Execute({csParams})");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t// TODO: Implementation needs to be extracted from decompilation");
		sb.AppendLine("\t\t\t// This is a placeholder for manual implementation");
		sb.AppendLine("\t\t\tthrow new NotImplementedException(\"Function implementation not yet transpiled\");");
		sb.AppendLine("\t\t}");
		
		// Close class and namespace
		sb.AppendLine("\t}");
		sb.AppendLine("}");

		return sb.ToString();
	}

	private uint ExtractAddressFromName(string functionName)
	{
		// Extract address from names like "sub_401000" or "FUN_00401000"
		var match = Regex.Match(functionName, @"[_]([0-9A-Fa-f]{6,8})$");
		if (match.Success)
		{
			return Convert.ToUInt32(match.Groups[1].Value, 16);
		}
		return 0;
	}

	private bool IsExternalFunction(string name)
	{
		// List of common external functions to skip
		var externalApis = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"CreateWindowExA", "ShowWindow", "GetMessageA", "TranslateMessage", 
			"DispatchMessageA", "DefWindowProcA", "RegisterClassA",
			"malloc", "free", "printf", "fopen", "fclose", "fread", "fwrite",
			"DirectDrawCreate", "DirectSoundCreate", "DirectInputCreateA",
			"exit", "memcpy", "memset", "strlen", "strcpy"
		};

		return externalApis.Contains(name);
	}

	private string MapCppTypeToCSharp(string cppType)
	{
		// Map C++ types to C# equivalents
		return cppType switch
		{
			"void" => "void",
			"int" => "int",
			"unsigned int" => "uint",
			"DWORD" => "uint",
			"BOOL" => "int",
			"HWND" => "uint",
			"HINSTANCE" => "uint",
			"LPSTR" => "string",
			"LPCSTR" => "string",
			"__int16" => "short",
			"__int64" => "long",
			_ => "uint" // Default to uint for pointers/handles
		};
	}

	private string MapParametersToCSharp(string cppParams)
	{
		if (string.IsNullOrWhiteSpace(cppParams) || cppParams == "void")
		{
			return "";
		}

		// Parse parameters and convert types
		var parameters = cppParams.Split(',');
		var csParams = new List<string>();

		for (var i = 0; i < parameters.Length; i++)
		{
			var param = parameters[i].Trim();
			// Simple type extraction (this is a basic implementation)
			var parts = param.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length >= 2)
			{
				var type = string.Join(" ", parts.Take(parts.Length - 1));
				var name = parts[^1].TrimStart('*');
				var csType = MapCppTypeToCSharp(type);
				csParams.Add($"{csType} {name}");
			}
			else
			{
				// If we can't parse properly, use generic parameter
				csParams.Add($"uint param{i}");
			}
		}

		return string.Join(", ", csParams);
	}
}

/// <summary>
/// Information about a parsed function from decompilation.
/// </summary>
class FunctionInfo
{
	public required string Name { get; set; }
	public required string ReturnType { get; set; }
	public required string Parameters { get; set; }
	public uint Address { get; set; }
	public string Comment { get; set; } = "";
}

/// <summary>
/// Attribute to mark the original x86 address of a function.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
class OriginalAddressAttribute : Attribute
{
	public uint Address { get; }
	
	public OriginalAddressAttribute(uint address)
	{
		Address = address;
	}
}
