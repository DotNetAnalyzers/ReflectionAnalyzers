namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    internal static class SymbolExt
    {
        internal static bool IsGenericDefinition(this ISymbol symbol)
        {
            return symbol switch
            {
                INamedTypeSymbol { ContainingType: { } containingType } type => IsGenericDefinition(containingType) ||
                                                                                IsGenericDefinition(type.TypeArguments),
                INamedTypeSymbol type => IsGenericDefinition(type.TypeArguments),
                IMethodSymbol method => IsGenericDefinition(method.TypeArguments),
                _ => false,
            };
        }

        internal static bool IsGenericDefinition(this INamedTypeSymbol symbol)
        {
            if (symbol is null)
            {
                return false;
            }

            if (symbol.ContainingType is { } containingType)
            {
                return IsGenericDefinition(containingType) ||
                       IsGenericDefinition(symbol.TypeArguments);
            }

            return IsGenericDefinition(symbol.TypeArguments);
        }

        internal static bool IsGenericDefinition(this IMethodSymbol symbol)
        {
            return symbol != null && IsGenericDefinition(symbol.TypeArguments);
        }

        private static bool IsGenericDefinition(ImmutableArray<ITypeSymbol> arguments)
        {
            if (arguments.IsEmpty)
            {
                return false;
            }

            foreach (var argument in arguments)
            {
                switch (argument.TypeKind)
                {
                    case TypeKind.Error:
                    case TypeKind.TypeParameter:
                        continue;
                    default:
                        return false;
                }
            }

            return true;
        }
    }
}
