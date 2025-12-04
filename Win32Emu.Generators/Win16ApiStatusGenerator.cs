using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Win32Emu.Generators;

/// <summary>
/// Source generator that extracts Win16 API functions from switch statements in Win16 module classes.
/// Generates win16-api-status.json file at compile time.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class Win16ApiStatusGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Find Win16 module classes that inherit from Win16ThunkingLayer
		var win16Modules = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (node, _) => IsWin16ModuleClass(node),
				static (ctx, _) => GetWin16ModuleInfo(ctx))
			.Where(static m => m is not null);

		// Collect all modules and generate JSON
		var moduleData = win16Modules.Collect()
			.Select(static (modules, _) => 
			{
				var result = ImmutableArray.CreateBuilder<Win16ModuleInfo>();
				foreach (var module in modules)
				{
					if (module != null)
						result.Add(module);
				}
				return result.ToImmutable();
			});

		// Generate the Win16 API status JSON file
		context.RegisterSourceOutput(moduleData, static (spc, modules) => GenerateWin16ApiStatus(spc, modules));
	}

	private static bool IsWin16ModuleClass(SyntaxNode node)
	{
		// Look for class declarations that might be Win16 modules
		if (node is not ClassDeclarationSyntax classDecl)
			return false;

		// Check if it has a Name property (all Win16 modules have this)
		var hasNameProperty = classDecl.Members
			.OfType<PropertyDeclarationSyntax>()
			.Any(p => p.Identifier.Text == "Name");

		return hasNameProperty;
	}

	private static Win16ModuleInfo? GetWin16ModuleInfo(GeneratorSyntaxContext context)
	{
		var classDecl = (ClassDeclarationSyntax)context.Node;
		var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

		if (classSymbol is null)
			return null;

		// Check if this class inherits from Win16ThunkingLayer
		if (!InheritsFromWin16ThunkingLayer(classSymbol))
			return null;

		// Get the module name from the Name property
		var moduleName = GetModuleName(classDecl);
		if (string.IsNullOrEmpty(moduleName))
			return null;

		// Extract function names from switch statements in TryInvokeWin16 method
		var functions = ExtractFunctionsFromSwitchStatements(classDecl);
		if (functions.Count == 0)
			return null;

		return new Win16ModuleInfo(moduleName + ".DLL", functions);
	}

	private static bool InheritsFromWin16ThunkingLayer(INamedTypeSymbol classSymbol)
	{
		var baseType = classSymbol.BaseType;
		while (baseType != null)
		{
			if (baseType.Name == "Win16ThunkingLayer")
				return true;
			baseType = baseType.BaseType;
		}
		return false;
	}

	private static string? GetModuleName(ClassDeclarationSyntax classDecl)
	{
		// Find the Name property and extract its return value
		var nameProperty = classDecl.Members
			.OfType<PropertyDeclarationSyntax>()
			.FirstOrDefault(p => p.Identifier.Text == "Name");

		if (nameProperty?.ExpressionBody?.Expression is LiteralExpressionSyntax literal)
		{
			return literal.Token.ValueText;
		}

		return null;
	}

	private static ImmutableHashSet<string> ExtractFunctionsFromSwitchStatements(ClassDeclarationSyntax classDecl)
	{
		var functions = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);

		// Find TryInvokeWin16 method
		var method = classDecl.Members
			.OfType<MethodDeclarationSyntax>()
			.FirstOrDefault(m => m.Identifier.Text == "TryInvokeWin16");

		if (method == null)
			return functions.ToImmutable();

		// Find all switch statements in the method
		var switchStatements = method.DescendantNodes().OfType<SwitchStatementSyntax>();

		foreach (var switchStatement in switchStatements)
		{
			// Extract case labels from the switch statement
			var caseSections = switchStatement.Sections;
			foreach (var section in caseSections)
			{
				foreach (var label in section.Labels.OfType<CaseSwitchLabelSyntax>())
				{
					if (label.Value is LiteralExpressionSyntax literal)
					{
						var functionName = literal.Token.ValueText;
						if (!string.IsNullOrEmpty(functionName))
						{
							functions.Add(functionName);
						}
					}
				}
			}
		}

		return functions.ToImmutable();
	}

	private static void GenerateWin16ApiStatus(SourceProductionContext context, ImmutableArray<Win16ModuleInfo> modules)
	{
		if (modules.IsEmpty)
			return;

		// Sort modules by name
		var sortedModules = modules
			.OrderBy(m => m.Name)
			.Select(m => new
			{
				name = m.Name,
				functions = m.Functions.OrderBy(f => f).ToArray()
			})
			.ToArray();

		// Generate JSON
		var jsonData = new
		{
			modules = sortedModules
		};

		var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});

		// Generate a C# class that contains the Win16 API status data
		var sb = new StringBuilder();
		sb.AppendLine("// <auto-generated />");
		sb.AppendLine("#nullable enable");
		sb.AppendLine();
		sb.AppendLine("namespace Win32Emu.Win32;");
		sb.AppendLine();
		sb.AppendLine("/// <summary>");
		sb.AppendLine("/// Auto-generated Win16 API status metadata.");
		sb.AppendLine("/// This data is generated at compile-time from Win16 module switch statements.");
		sb.AppendLine("/// </summary>");
		sb.AppendLine("public static class Win16ApiStatusMetadata");
		sb.AppendLine("{");
		sb.AppendLine($"\tpublic const string Json = @\"{json.Replace("\"", "\"\"")}\";");
		sb.AppendLine();
		sb.AppendLine($"\tpublic static int TotalModules => {sortedModules.Length};");

		var totalFunctions = sortedModules.Sum(m => m.functions.Length);
		sb.AppendLine($"\tpublic static int TotalFunctions => {totalFunctions};");

		sb.AppendLine("}");

		context.AddSource("Win16ApiStatusMetadata.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
	}

	private class Win16ModuleInfo
	{
		public string Name { get; }
		public ImmutableHashSet<string> Functions { get; }

		public Win16ModuleInfo(string name, ImmutableHashSet<string> functions)
		{
			Name = name;
			Functions = functions;
		}
	}
}
