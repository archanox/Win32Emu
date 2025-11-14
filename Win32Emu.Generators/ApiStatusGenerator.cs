using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Win32Emu.Generators;

/// <summary>
/// Source generator that extracts API status metadata from Win32 modules
/// and generates a JSON file for documentation purposes.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ApiStatusGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Find methods with [DllModuleExport] attribute
		var exportedMethods = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (node, _) => node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0,
				static (ctx, _) => GetExportedMethod(ctx))
			.Where(static m => m is not null);

		// Collect all methods and group by module
		var moduleData = exportedMethods.Collect()
			.Select(static (methods, _) => GroupByModule(methods!));

		// Generate the API status data file
		context.RegisterSourceOutput(moduleData, static (spc, modules) => GenerateApiStatus(spc, modules));
	}

	private static ExportedMethodInfo? GetExportedMethod(GeneratorSyntaxContext context)
	{
		var methodDecl = (MethodDeclarationSyntax)context.Node;
		var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

		if (methodSymbol is null)
			return null;

		// Check for DllModuleExport attribute
		var dllExportAttr = methodSymbol.GetAttributes()
			.FirstOrDefault(attr => attr.AttributeClass?.Name is "DllModuleExportAttribute" or "DllModuleExport");

		if (dllExportAttr is null)
			return null;

		// Extract attribute parameters
		uint? ordinal = null;
		bool isStub = false;
		string? version = null;
		string? exportName = null;
		string? forwardedTo = null;

		// Parse constructor arguments
		if (dllExportAttr.ConstructorArguments.Length > 0)
		{
			var firstArg = dllExportAttr.ConstructorArguments[0];
			if (firstArg.Value is uint ordinalValue)
				ordinal = ordinalValue;
		}

		// Parse named arguments
		foreach (var namedArg in dllExportAttr.NamedArguments)
		{
			switch (namedArg.Key)
			{
				case "IsStub" when namedArg.Value.Value is bool stubValue:
					isStub = stubValue;
					break;
				case "Version" when namedArg.Value.Value is string versionValue:
					version = versionValue;
					break;
				case "ExportName" when namedArg.Value.Value is string exportNameValue:
					exportName = exportNameValue;
					break;
				case "ForwardedTo" when namedArg.Value.Value is string forwardedToValue:
					forwardedTo = forwardedToValue;
					break;
			}
		}

		// Get module info
		var containingType = methodSymbol.ContainingType;
		var moduleName = GetModuleDllName(containingType);

		return new ExportedMethodInfo(
			moduleName: moduleName,
			className: containingType.Name,
			methodName: methodSymbol.Name,
			ordinal: ordinal,
			isStub: isStub,
			version: version,
			exportName: exportName,
			forwardedTo: forwardedTo);
	}

	private static string GetModuleDllName(INamedTypeSymbol typeSymbol)
	{
		// Try to get the Name property value
		var nameProperty = typeSymbol.GetMembers("Name")
			.OfType<IPropertySymbol>()
			.FirstOrDefault();

		if (nameProperty?.GetMethod != null)
		{
			var syntaxRef = nameProperty.GetMethod.DeclaringSyntaxReferences.FirstOrDefault();
			if (syntaxRef != null)
			{
				var syntax = syntaxRef.GetSyntax();
				var syntaxText = syntax.ToString();
				
				// Look for => "DLL_NAME"
				var arrowIndex = syntaxText.IndexOf("=>", StringComparison.Ordinal);
				if (arrowIndex >= 0)
				{
					var firstQuote = syntaxText.IndexOf('"', arrowIndex);
					if (firstQuote >= 0)
					{
						var secondQuote = syntaxText.IndexOf('"', firstQuote + 1);
						if (secondQuote > firstQuote)
						{
							return syntaxText.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
						}
					}
				}
			}
		}

		// Fallback: derive from class name (e.g., Kernel32Module -> KERNEL32.DLL)
		var className = typeSymbol.Name;
		if (className.EndsWith("Module", StringComparison.OrdinalIgnoreCase))
		{
			var baseName = className.Substring(0, className.Length - 6);
			return baseName.ToUpperInvariant() + ".DLL";
		}

		return className.ToUpperInvariant() + ".DLL";
	}

	private static ImmutableArray<ModuleInfo> GroupByModule(ImmutableArray<ExportedMethodInfo> methods)
	{
		var moduleDict = new Dictionary<string, List<FunctionInfo>>();

		foreach (var method in methods)
		{
			if (!moduleDict.TryGetValue(method.ModuleName, out var functions))
			{
				functions = new List<FunctionInfo>();
				moduleDict[method.ModuleName] = functions;
			}

			functions.Add(new FunctionInfo(
				name: method.MethodName,
				isStub: method.IsStub,
				ordinal: method.Ordinal,
				version: method.Version,
				exportName: method.ExportName,
				forwardedTo: method.ForwardedTo));
		}

		// Sort functions within each module and create module info
		return moduleDict
			.OrderBy(kvp => kvp.Key)
			.Select(kvp => new ModuleInfo(
				name: kvp.Key,
				className: ExtractClassNameFromFirstFunction(methods, kvp.Key),
				functions: kvp.Value.OrderBy(f => f.Name).ToImmutableArray()))
			.ToImmutableArray();
	}

	private static string ExtractClassNameFromFirstFunction(ImmutableArray<ExportedMethodInfo> methods, string moduleName)
	{
		return methods.FirstOrDefault(m => m.ModuleName == moduleName)?.ClassName ?? "";
	}

	private static void GenerateApiStatus(SourceProductionContext context, ImmutableArray<ModuleInfo> modules)
	{
		// Generate a C# class that contains the API status data
		var sb = new StringBuilder();
		sb.AppendLine("// <auto-generated />");
		sb.AppendLine("#nullable enable");
		sb.AppendLine();
		sb.AppendLine("namespace Win32Emu.Win32;");
		sb.AppendLine();
		sb.AppendLine("/// <summary>");
		sb.AppendLine("/// Auto-generated API status metadata for Win32 modules.");
		sb.AppendLine("/// This data is generated at compile-time from [DllModuleExport] attributes.");
		sb.AppendLine("/// </summary>");
		sb.AppendLine("public static class ApiStatusMetadata");
		sb.AppendLine("{");
		
		// Generate JSON as a string constant
		var jsonData = new
		{
			generatedAt = DateTime.UtcNow.ToString("O"),
			modules = modules.Select(m => new
			{
				name = m.Name,
				className = m.ClassName,
				functions = m.Functions.Select(f => new
				{
					name = f.Name,
					isStub = f.IsStub,
					ordinal = f.Ordinal,
					version = f.Version,
					exportName = f.ExportName,
					forwardedTo = f.ForwardedTo
				}).ToArray()
			}).ToArray()
		};

		var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions 
		{ 
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});

		// Escape the JSON string for C#
		var escapedJson = json.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
		
		sb.AppendLine($"\tpublic const string Json = @\"{json.Replace("\"", "\"\"")}\";");
		sb.AppendLine();
		sb.AppendLine($"\tpublic static int TotalModules => {modules.Length};");
		
		var totalFunctions = modules.Sum(m => m.Functions.Length);
		var stubFunctions = modules.Sum(m => m.Functions.Count(f => f.IsStub));
		
		sb.AppendLine($"\tpublic static int TotalFunctions => {totalFunctions};");
		sb.AppendLine($"\tpublic static int StubFunctions => {stubFunctions};");
		sb.AppendLine($"\tpublic static double ImplementationRate => {((totalFunctions - stubFunctions) * 100.0 / totalFunctions):F1};");
		
		sb.AppendLine("}");

		context.AddSource("ApiStatusMetadata.g.cs", sb.ToString());
	}

	private class ExportedMethodInfo
	{
		public string ModuleName { get; set; } = "";
		public string ClassName { get; set; } = "";
		public string MethodName { get; set; } = "";
		public uint? Ordinal { get; set; }
		public bool IsStub { get; set; }
		public string? Version { get; set; }
		public string? ExportName { get; set; }
		public string? ForwardedTo { get; set; }

		public ExportedMethodInfo(
			string moduleName,
			string className,
			string methodName,
			uint? ordinal,
			bool isStub,
			string? version,
			string? exportName,
			string? forwardedTo)
		{
			ModuleName = moduleName;
			ClassName = className;
			MethodName = methodName;
			Ordinal = ordinal;
			IsStub = isStub;
			Version = version;
			ExportName = exportName;
			ForwardedTo = forwardedTo;
		}
	}

	private class ModuleInfo
	{
		public string Name { get; set; } = "";
		public string ClassName { get; set; } = "";
		public ImmutableArray<FunctionInfo> Functions { get; set; }

		public ModuleInfo(string name, string className, ImmutableArray<FunctionInfo> functions)
		{
			Name = name;
			ClassName = className;
			Functions = functions;
		}
	}

	private class FunctionInfo
	{
		public string Name { get; set; } = "";
		public bool IsStub { get; set; }
		public uint? Ordinal { get; set; }
		public string? Version { get; set; }
		public string? ExportName { get; set; }
		public string? ForwardedTo { get; set; }

		public FunctionInfo(
			string name,
			bool isStub,
			uint? ordinal,
			string? version,
			string? exportName,
			string? forwardedTo)
		{
			Name = name;
			IsStub = isStub;
			Ordinal = ordinal;
			Version = version;
			ExportName = exportName;
			ForwardedTo = forwardedTo;
		}
	}
}
