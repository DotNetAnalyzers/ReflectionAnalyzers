namespace ReflectionAnalyzers;

using Microsoft.CodeAnalysis;

internal static class INamedTypeSymbolExt
{
    private static readonly SymbolDisplayFormat Format = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable);

    internal static string QualifiedMetadataName(this INamedTypeSymbol type)
    {
        var text = type.ToDisplayString(Format);
        if (type.IsGenericType)
        {
            return $"{text}`{type.Arity}";
        }

        return text;
    }
}
