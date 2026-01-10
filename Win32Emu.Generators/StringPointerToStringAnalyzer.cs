using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Win32Emu.Generators;

/// <summary>
/// Roslyn analyzer that detects incorrect usage of ToString() on string pointer types.
/// String pointer types (LpcStr, LpcWStr, LpWStr) should use the Read() method instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringPointerToStringAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "WIN32EMU002";
	private const string Category = "Usage";

	private static readonly LocalizableString Title = "ToString() should not be used on string pointer types";
	private static readonly LocalizableString MessageFormat = "Use '{0}.Read(memory)' instead of '{0}.ToString()' to read the string from emulated memory";
	private static readonly LocalizableString Description = "String pointer types (LpcStr, LpcWStr, LpWStr) wrap memory addresses. Calling ToString() returns the type name instead of the string content. Use the Read() method to properly read strings from emulated memory.";

	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
		DiagnosticId,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		// Register for syntax node analysis on invocation expressions
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;

		// Check if this is a member access expression (e.g., something.ToString())
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return;

		// Check if the method being called is "ToString"
		if (memberAccess.Name.Identifier.Text != "ToString")
			return;

		// Get the symbol info for the expression being accessed
		var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Expression);
		if (symbolInfo.Symbol is not ILocalSymbol and not IParameterSymbol and not IFieldSymbol and not IPropertySymbol)
			return;

		// Get the type of the expression
		var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
		if (typeInfo.Type is not INamedTypeSymbol namedType)
			return;

		// Check if the type is one of our string pointer types
		var typeName = namedType.Name;
		if (typeName != "LpcStr" && typeName != "LpcWStr" && typeName != "LpWStr")
			return;

		// Verify it's in the Win32Emu.Win32 namespace
		if (namedType.ContainingNamespace?.ToDisplayString() != "Win32Emu.Win32")
			return;

		// Report the diagnostic
		var diagnostic = Diagnostic.Create(
			Rule,
			invocation.GetLocation(),
			typeName);

		context.ReportDiagnostic(diagnostic);
	}
}
