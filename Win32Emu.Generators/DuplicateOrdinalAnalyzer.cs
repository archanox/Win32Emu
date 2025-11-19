using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Win32Emu.Generators;

/// <summary>
/// Roslyn analyzer that detects duplicate ordinals in DllModuleExport attributes
/// within the same Win32 module for the same version.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DuplicateOrdinalAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "WIN32EMU001";
	private const string Category = "Usage";

	private static readonly LocalizableString Title = "Duplicate DLL export ordinal";
	private static readonly LocalizableString MessageFormat = "Ordinal {0} is used multiple times in module '{1}' for version '{2}'";
	private static readonly LocalizableString Description = "Each ordinal must be unique per DLL version. Multiple methods cannot have the same ordinal for the same version.";

	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
		DiagnosticId,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		// Register for symbol analysis on named types (classes)
		context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
	}

	private static void AnalyzeNamedType(SymbolAnalysisContext context)
	{
		var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

		// Only analyze classes that look like Win32 modules (end with "Module")
		if (!namedTypeSymbol.Name.EndsWith("Module", StringComparison.Ordinal))
			return;

		// Dictionary to track ordinals: (ordinal, version) -> list of methods
		var ordinalMap = new Dictionary<(uint ordinal, string version), List<(IMethodSymbol method, AttributeData attribute)>>();

		// Collect all methods with DllModuleExport attributes
		foreach (var methodSymbol in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
		{
			// Get all DllModuleExport attributes on this method
			var exportAttributes = methodSymbol.GetAttributes()
				.Where(attr => attr.AttributeClass?.Name is "DllModuleExportAttribute" or "DllModuleExport")
				.ToList();

			foreach (var attribute in exportAttributes.Where(attr => attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is uint))
			{
				// Extract ordinal from constructor arguments
				var ordinal = (uint)attribute.ConstructorArguments[0].Value!;

				// Extract version from named arguments (default to empty string if not specified)
				string version = attribute.NamedArguments
					.Where(na => na.Key == "Version" && na.Value.Value is string)
					.Select(na => (string)na.Value.Value!)
					.FirstOrDefault() ?? string.Empty;

				// Track this ordinal-version combination
				var key = (ordinal, version);
				if (!ordinalMap.TryGetValue(key, out var list))
				{
					list = new List<(IMethodSymbol, AttributeData)>();
					ordinalMap[key] = list;
				}
				list.Add((methodSymbol, attribute));
			}
		}

		// Check for duplicates
		foreach (var kvp in ordinalMap.Where(kvp => kvp.Value.Count > 1))
		{
			var (ordinal, version) = kvp.Key;
			var methods = kvp.Value;

			// We have duplicate ordinals for the same version
			var versionDisplay = string.IsNullOrEmpty(version) ? "(no version specified)" : version;

			// Report diagnostic for each duplicate
			foreach (var (method, attribute) in methods)
			{
				// Get the location of the attribute
				var location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() 
					?? method.Locations.FirstOrDefault() 
					?? Location.None;

				var diagnostic = Diagnostic.Create(
					Rule,
					location,
					ordinal,
					namedTypeSymbol.Name,
					versionDisplay);

				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
