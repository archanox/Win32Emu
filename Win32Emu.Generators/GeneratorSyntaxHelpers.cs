using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Win32Emu.Generators;

internal static class GeneratorSyntaxHelpers
{
	public static bool HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
	{
		foreach (var attributeList in attributeLists)
		{
			foreach (var attribute in attributeList.Attributes)
			{
				if (HasName(attribute, attributeName))
					return true;
			}
		}

		return false;
	}

	private static bool HasName(AttributeSyntax attribute, string attributeName)
	{
		var simpleName = GetSimpleName(attribute.Name);
		return simpleName == attributeName || simpleName == attributeName + "Attribute";
	}

	private static string GetSimpleName(NameSyntax nameSyntax)
	{
		return nameSyntax switch
		{
			IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
			QualifiedNameSyntax qualified => GetSimpleName(qualified.Right),
			AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.ValueText,
			SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
			_ => nameSyntax.ToString()
		};
	}
}
