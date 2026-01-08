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
				
				// Generate JIT integration code
				await transpiler.GenerateJitIntegrationAsync(functions, outputDir, namespaceName, logger);
				logger.LogInformation("Generated JIT integration code");
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

		// Parse function implementations with bodies
		// Pattern: return_type [__cdecl|__stdcall|etc] function_name(params) { body }
		var functionPattern = @"//-+\s*\(([0-9A-Fa-f]+)\)\s*-+\s*\n(.*?)\n\{(.*?)^\}";
		var matches = Regex.Matches(decompCode, functionPattern, RegexOptions.Multiline | RegexOptions.Singleline);

		_logger.LogDebug("Found {Count} function implementations with bodies", matches.Count);

		foreach (Match match in matches)
		{
			var addressStr = match.Groups[1].Value;
			var signature = match.Groups[2].Value.Trim();
			var body = match.Groups[3].Value;

			// Parse the signature
			var sigMatch = Regex.Match(signature, @"^(\w+)\s+(?:__(?:cdecl|stdcall|fastcall|thiscall))?\s*(\w+)\s*\((.*?)\)");
			if (!sigMatch.Success)
				continue;

			var returnType = sigMatch.Groups[1].Value;
			var functionName = sigMatch.Groups[2].Value;
			var parameters = sigMatch.Groups[3].Value;

			// Skip if it's a standard library function or external API
			if (IsExternalFunction(functionName))
			{
				continue;
			}

			// Parse address
			var address = Convert.ToUInt32(addressStr, 16);

			functions.Add(new FunctionInfo
			{
				Name = functionName,
				ReturnType = returnType,
				Parameters = parameters,
				Address = address,
				Comment = "",
				Body = body
			});
		}

		_logger.LogInformation("Parsed {Count} functions with bodies (excluding external APIs)", functions.Count);
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
	
	public async Task GenerateJitIntegrationAsync(List<FunctionInfo> functions, string outputDir, string namespaceName, ILogger logger)
	{
		// Generate a loader class that can integrate with the JIT system
		var sb = new StringBuilder();
		
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Collections.Generic;");
		sb.AppendLine("using System.Reflection;");
		sb.AppendLine("using Microsoft.Extensions.Logging;");
		sb.AppendLine("using Win32Emu;");
		sb.AppendLine();
		sb.AppendLine($"namespace {namespaceName}");
		sb.AppendLine("{");
		sb.AppendLine("\t/// <summary>");
		sb.AppendLine("\t/// JIT integration loader for transpiled functions");
		sb.AppendLine("\t/// </summary>");
		sb.AppendLine("\tpublic class TranspiledFunctionLoader");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tprivate readonly Dictionary<uint, Func<EmulatorEnvironment, object[], object>> _functions = new();");
		sb.AppendLine("\t\tprivate readonly ILogger? _logger;");
		sb.AppendLine();
		sb.AppendLine("\t\tpublic TranspiledFunctionLoader(ILogger? logger = null)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t_logger = logger;");
		sb.AppendLine("\t\t\tLoadFunctions();");
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.AppendLine("\t\tprivate void LoadFunctions()");
		sb.AppendLine("\t\t{");
		
		// Generate registration for each function
		foreach (var func in functions)
		{
			sb.AppendLine($"\t\t\t// Register function at 0x{func.Address:X8} ({func.Name})");
			sb.AppendLine($"\t\t\t_functions[0x{func.Address:X8}u] = (env, args) =>");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine($"\t\t\t\tvar instance = new Function_{func.Address:X8}(env);");
			sb.AppendLine("\t\t\t\treturn instance.Execute();");
			sb.AppendLine("\t\t\t};");
			sb.AppendLine();
		}
		
		sb.AppendLine("\t\t\t_logger?.LogInformation(\"Loaded {Count} transpiled functions\", _functions.Count);");
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <summary>");
		sb.AppendLine("\t\t/// Try to execute a transpiled function at the given address");
		sb.AppendLine("\t\t/// </summary>");
		sb.AppendLine("\t\tpublic bool TryExecuteFunction(uint address, EmulatorEnvironment env, object[] args, out object? result)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tif (_functions.TryGetValue(address, out var func))");
		sb.AppendLine("\t\t\t{");
		sb.AppendLine("\t\t\t\ttry");
		sb.AppendLine("\t\t\t\t{");
		sb.AppendLine("\t\t\t\t\tresult = func(env, args);");
		sb.AppendLine("\t\t\t\t\treturn true;");
		sb.AppendLine("\t\t\t\t}");
		sb.AppendLine("\t\t\t\tcatch (Exception ex)");
		sb.AppendLine("\t\t\t\t{");
		sb.AppendLine("\t\t\t\t\t_logger?.LogError(ex, \"Error executing transpiled function at 0x{Address:X8}\", address);");
		sb.AppendLine("\t\t\t\t\tresult = null;");
		sb.AppendLine("\t\t\t\t\treturn false;");
		sb.AppendLine("\t\t\t\t}");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t\tresult = null;");
		sb.AppendLine("\t\t\treturn false;");
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <summary>");
		sb.AppendLine("\t\t/// Check if a transpiled function exists at the given address");
		sb.AppendLine("\t\t/// </summary>");
		sb.AppendLine("\t\tpublic bool HasFunction(uint address)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\treturn _functions.ContainsKey(address);");
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <summary>");
		sb.AppendLine("\t\t/// Get all registered function addresses");
		sb.AppendLine("\t\t/// </summary>");
		sb.AppendLine("\t\tpublic IEnumerable<uint> GetFunctionAddresses()");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\treturn _functions.Keys;");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		
		var filePath = Path.Combine(outputDir, "TranspiledFunctionLoader.cs");
		await File.WriteAllTextAsync(filePath, sb.ToString());
		logger.LogDebug("Generated JIT integration loader: {Path}", filePath);
		
		// Generate usage documentation
		await GenerateJitUsageDocAsync(outputDir, namespaceName, functions.Count, logger);
	}
	
	private async Task GenerateJitUsageDocAsync(string outputDir, string namespaceName, int functionCount, ILogger logger)
	{
		var doc = new StringBuilder();
		
		doc.AppendLine("# JIT Integration Usage");
		doc.AppendLine();
		doc.AppendLine("## Overview");
		doc.AppendLine();
		doc.AppendLine($"This project contains {functionCount} transpiled C# functions that can be integrated with Win32Emu's JIT system.");
		doc.AppendLine();
		doc.AppendLine("## Loading Transpiled Functions");
		doc.AppendLine();
		doc.AppendLine("```csharp");
		doc.AppendLine("using Win32Emu;");
		doc.AppendLine($"using {namespaceName};");
		doc.AppendLine();
		doc.AppendLine("// Create the function loader");
		doc.AppendLine("var loader = new TranspiledFunctionLoader(logger);");
		doc.AppendLine();
		doc.AppendLine("// Check if a function is available");
		doc.AppendLine("if (loader.HasFunction(0x004032A0))");
		doc.AppendLine("{");
		doc.AppendLine("    Console.WriteLine(\"Initialization function available\");");
		doc.AppendLine("}");
		doc.AppendLine();
		doc.AppendLine("// Execute a transpiled function");
		doc.AppendLine("if (loader.TryExecuteFunction(0x004032A0, env, Array.Empty<object>(), out var result))");
		doc.AppendLine("{");
		doc.AppendLine("    Console.WriteLine($\"Function returned: {result}\");");
		doc.AppendLine("}");
		doc.AppendLine("```");
		doc.AppendLine();
		doc.AppendLine("## Integration with JitCpu");
		doc.AppendLine();
		doc.AppendLine("To integrate with the JIT CPU, you can modify the emulator to check for transpiled functions before JIT compiling:");
		doc.AppendLine();
		doc.AppendLine("```csharp");
		doc.AppendLine("// In your emulator initialization");
		doc.AppendLine($"var transpiledLoader = new {namespaceName}.TranspiledFunctionLoader(logger);");
		doc.AppendLine();
		doc.AppendLine("// Before executing a block at an address:");
		doc.AppendLine("if (transpiledLoader.HasFunction(eip))");
		doc.AppendLine("{");
		doc.AppendLine("    // Execute the transpiled C# version instead of JIT compiling");
		doc.AppendLine("    if (transpiledLoader.TryExecuteFunction(eip, env, Array.Empty<object>(), out var result))");
		doc.AppendLine("    {");
		doc.AppendLine("        // Update CPU state based on result");
		doc.AppendLine("        // Set EIP to return address, etc.");
		doc.AppendLine("        return;");
		doc.AppendLine("    }");
		doc.AppendLine("}");
		doc.AppendLine();
		doc.AppendLine("// Otherwise, proceed with normal JIT compilation");
		doc.AppendLine("await cpu.ExecuteBlockAsync(memory);");
		doc.AppendLine("```");
		doc.AppendLine();
		doc.AppendLine("## Compiled Assembly");
		doc.AppendLine();
		doc.AppendLine("You can also compile this project into a DLL and load it dynamically:");
		doc.AppendLine();
		doc.AppendLine("```bash");
		doc.AppendLine("# Build the transpiled functions");
		doc.AppendLine("dotnet build -c Release");
		doc.AppendLine();
		doc.AppendLine("# Reference the DLL in your emulator project");
		doc.AppendLine("# Or load it dynamically at runtime");
		doc.AppendLine("```");
		doc.AppendLine();
		doc.AppendLine("## Benefits");
		doc.AppendLine();
		doc.AppendLine("- **Debugging**: Step through C# code in Visual Studio/dnSpy");
		doc.AppendLine("- **Performance**: Pre-compiled C# executes faster than JIT compilation");
		doc.AppendLine("- **Inspection**: Understand game logic without reverse engineering");
		doc.AppendLine("- **Modification**: Easily modify behavior for testing/patching");
		doc.AppendLine();
		doc.AppendLine("## Limitations");
		doc.AppendLine();
		doc.AppendLine("- Global variables (dword_XXXXXX) need to be mapped to emulator memory");
		doc.AppendLine("- Function calls to other transpiled functions need proper integration");
		doc.AppendLine("- Complex pointer operations may need manual refinement");
		doc.AppendLine("- Win32 API calls are routed through EmulatorEnvironment");
		
		var docPath = Path.Combine(outputDir, "JIT_INTEGRATION.md");
		await File.WriteAllTextAsync(docPath, doc.ToString());
		logger.LogDebug("Generated JIT usage documentation: {Path}", docPath);
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
		
		// Transpile function body if available
		if (!string.IsNullOrEmpty(func.Body))
		{
			var transpiledBody = TranspileFunctionBody(func.Body, func);
			sb.Append(transpiledBody);
		}
		else
		{
			sb.AppendLine("\t\t\t// TODO: Implementation needs to be extracted from decompilation");
			sb.AppendLine("\t\t\t// This is a placeholder for manual implementation");
			sb.AppendLine("\t\t\tthrow new NotImplementedException(\"Function implementation not yet transpiled\");");
		}
		
		sb.AppendLine("\t\t}");
		
		// Add helper method for calling other decompiled functions
		sb.AppendLine();
		sb.AppendLine("\t\t/// <summary>");
		sb.AppendLine("\t\t/// Call another function at the specified address");
		sb.AppendLine("\t\t/// </summary>");
		sb.AppendLine("\t\tprivate uint CallFunction(uint address, params object[] args)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t// TODO: Implement function calling mechanism");
		sb.AppendLine("\t\t\t// This would need to interact with the emulator or other generated functions");
		sb.AppendLine("\t\t\t_env.Logger?.LogWarning(\"CallFunction not yet implemented for address 0x{Address:X8}\", address);");
		sb.AppendLine("\t\t\treturn 0;");
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

	private string TranspileFunctionBody(string cppBody, FunctionInfo func)
	{
		var sb = new StringBuilder();
		var lines = cppBody.Split('\n');
		
		// Track variables declared in the function
		var variables = new HashSet<string>();
		
		foreach (var line in lines)
		{
			var trimmed = line.Trim();
			
			// Skip empty lines and comments
			if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//"))
			{
				if (!string.IsNullOrWhiteSpace(trimmed))
					sb.AppendLine($"\t\t\t{trimmed}");
				continue;
			}
			
			// Transpile the line
			var csLine = TranspileLine(trimmed, variables, func);
			if (!string.IsNullOrEmpty(csLine))
			{
				sb.AppendLine($"\t\t\t{csLine}");
			}
		}
		
		return sb.ToString();
	}
	
	private string TranspileLine(string cppLine, HashSet<string> variables, FunctionInfo func)
	{
		// Remove trailing semicolon for processing
		var line = cppLine.TrimEnd(';', ' ');
		
		// Handle return statements
		if (line.StartsWith("return "))
		{
			var returnValue = line.Substring(7).Trim();
			var csValue = TranspileExpression(returnValue, variables, func);
			return $"return {csValue};";
		}
		
		// Handle variable declarations
		// Pattern: type varname = value; or type varname;
		var declMatch = Regex.Match(line, @"^([\w\s\*]+)\s+(\w+)(\s*=\s*(.+))?$");
		if (declMatch.Success)
		{
			var cppType = declMatch.Groups[1].Value.Trim();
			var varName = declMatch.Groups[2].Value;
			var hasInit = declMatch.Groups[3].Success;
			var initValue = hasInit ? declMatch.Groups[4].Value.Trim() : "";
			
			// Map type
			var csType = MapCppTypeToCSharp(cppType);
			variables.Add(varName);
			
			if (hasInit)
			{
				var csValue = TranspileExpression(initValue, variables, func);
				return $"{csType} {varName} = {csValue};";
			}
			else
			{
				return $"{csType} {varName};";
			}
		}
		
		// Handle assignment statements
		// Pattern: varname = value;
		var assignMatch = Regex.Match(line, @"^(\w+)\s*=\s*(.+)$");
		if (assignMatch.Success)
		{
			var varName = assignMatch.Groups[1].Value;
			var value = assignMatch.Groups[2].Value.Trim();
			var csValue = TranspileExpression(value, variables, func);
			return $"{varName} = {csValue};";
		}
		
		// Handle if statements
		if (line.StartsWith("if "))
		{
			var condMatch = Regex.Match(line, @"^if\s*\(\s*(.+?)\s*\)$");
			if (condMatch.Success)
			{
				var condition = condMatch.Groups[1].Value;
				var csCondition = TranspileExpression(condition, variables, func);
				return $"if ({csCondition})";
			}
		}
		
		// Handle function calls
		// Pattern: functionName(args);
		var callMatch = Regex.Match(line, @"^(\w+)\s*\((.*)?\)$");
		if (callMatch.Success)
		{
			var funcName = callMatch.Groups[1].Value;
			var args = callMatch.Groups[2].Value;
			
			// Check if it's a Win32 API call
			if (IsWin32ApiCall(funcName))
			{
				var csArgs = TranspileArguments(args, variables, func);
				return $"_env.CallWin32Api(\"{funcName}\"{(string.IsNullOrEmpty(csArgs) ? "" : ", " + csArgs)});";
			}
			// Check if it's a call to another decompiled function
			else if (funcName.StartsWith("sub_"))
			{
				var address = ExtractAddressFromName(funcName);
				var csArgs = TranspileArguments(args, variables, func);
				return $"CallFunction(0x{address:X8}{(string.IsNullOrEmpty(csArgs) ? "" : ", " + csArgs)});";
			}
		}
		
		// Handle block markers
		if (line == "{")
			return "{";
		if (line == "}")
			return "}";
		
		// Default: return line as comment
		return $"// TODO: Transpile: {cppLine}";
	}
	
	private string TranspileExpression(string cppExpr, HashSet<string> variables, FunctionInfo func)
	{
		var expr = cppExpr.Trim();
		
		// Handle numeric literals
		if (Regex.IsMatch(expr, @"^-?\d+$"))
			return expr;
		
		// Handle hex literals
		if (Regex.IsMatch(expr, @"^0x[0-9A-Fa-f]+$"))
			return expr;
		
		// Handle NULL
		if (expr == "NULL" || expr == "0")
			return "0";
		
		// Handle boolean expressions
		if (expr == "TRUE" || expr == "true")
			return "true";
		if (expr == "FALSE" || expr == "false")
			return "false";
		
		// Handle function calls in expressions
		var callMatch = Regex.Match(expr, @"^(\w+)\s*\((.*?)\)$");
		if (callMatch.Success)
		{
			var funcName = callMatch.Groups[1].Value;
			var args = callMatch.Groups[2].Value;
			
			if (IsWin32ApiCall(funcName))
			{
				var csArgs = TranspileArguments(args, variables, func);
				return $"_env.CallWin32Api<uint>(\"{funcName}\"{(string.IsNullOrEmpty(csArgs) ? "" : ", " + csArgs)})";
			}
			else if (funcName.StartsWith("sub_"))
			{
				var address = ExtractAddressFromName(funcName);
				var csArgs = TranspileArguments(args, variables, func);
				return $"CallFunction(0x{address:X8}{(string.IsNullOrEmpty(csArgs) ? "" : ", " + csArgs)})";
			}
		}
		
		// Handle dereferencing and pointer operations
		expr = expr.Replace("->", ".");
		
		// Handle comparison operators (convert to C# style)
		expr = expr.Replace("!=", "!=");
		expr = expr.Replace("==", "==");
		
		// Handle negation
		if (expr.StartsWith("!"))
		{
			var inner = TranspileExpression(expr.Substring(1), variables, func);
			return $"!{inner}";
		}
		
		// Handle binary operators
		foreach (var op in new[] { "&&", "||", "+", "-", "*", "/", "%", "<", ">", "<=", ">=", "==", "!=" })
		{
			var parts = expr.Split(new[] { op }, 2, StringSplitOptions.None);
			if (parts.Length == 2)
			{
				var left = TranspileExpression(parts[0].Trim(), variables, func);
				var right = TranspileExpression(parts[1].Trim(), variables, func);
				return $"{left} {op} {right}";
			}
		}
		
		// Default: return as-is
		return expr;
	}
	
	private string TranspileArguments(string cppArgs, HashSet<string> variables, FunctionInfo func)
	{
		if (string.IsNullOrWhiteSpace(cppArgs))
			return "";
		
		var args = SplitArguments(cppArgs);
		var csArgs = new List<string>();
		
		foreach (var arg in args)
		{
			csArgs.Add(TranspileExpression(arg.Trim(), variables, func));
		}
		
		return string.Join(", ", csArgs);
	}
	
	private List<string> SplitArguments(string args)
	{
		var result = new List<string>();
		var current = new StringBuilder();
		int depth = 0;
		
		foreach (var ch in args)
		{
			if (ch == '(' || ch == '{' || ch == '[')
			{
				depth++;
				current.Append(ch);
			}
			else if (ch == ')' || ch == '}' || ch == ']')
			{
				depth--;
				current.Append(ch);
			}
			else if (ch == ',' && depth == 0)
			{
				result.Add(current.ToString());
				current.Clear();
			}
			else
			{
				current.Append(ch);
			}
		}
		
		if (current.Length > 0)
			result.Add(current.ToString());
		
		return result;
	}
	
	private bool IsWin32ApiCall(string funcName)
	{
		var win32Apis = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"CreateWindowExA", "CreateWindowExW", "ShowWindow", "UpdateWindow",
			"GetMessageA", "GetMessageW", "TranslateMessage", "DispatchMessageA", "DispatchMessageW",
			"DefWindowProcA", "DefWindowProcW", "RegisterClassA", "RegisterClassW",
			"LoadIconA", "LoadIconW", "LoadCursorA", "LoadCursorW",
			"GetStockObject", "GetSystemMetrics",
			"DirectDrawCreate", "DirectSoundCreate", "DirectInputCreateA",
			"PostMessageA", "PostMessageW", "SendMessageA", "SendMessageW",
			"GetDC", "ReleaseDC", "BeginPaint", "EndPaint",
			"CreateFileA", "CreateFileW", "ReadFile", "WriteFile", "CloseHandle",
			"GetLastError", "SetLastError",
			"MessageBoxA", "MessageBoxW"
		};
		
		return win32Apis.Contains(funcName);
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
	public string Body { get; set; } = "";
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
