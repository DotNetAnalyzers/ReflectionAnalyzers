namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    internal static class SymbolExt
    {
        internal static bool IsGenericDefinition(this ISymbol symbol)
        {
            switch (symbol)
            {
                case INamedTypeSymbol type:
                    if (type.ContainingType is INamedTypeSymbol containingType)
                    {
                        return IsGenericDefinition(containingType) ||
                               IsGenericDefinition(type.TypeArguments);
                    }

                    return IsGenericDefinition(type.TypeArguments);
                case IMethodSymbol method:
                    return IsGenericDefinition(method.TypeArguments);
                default:
                    return false;
            }
        }

        internal static bool IsGenericDefinition(this INamedTypeSymbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            if (symbol.ContainingType is INamedTypeSymbol containingType)
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
            if (arguments.Length == 0)
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
