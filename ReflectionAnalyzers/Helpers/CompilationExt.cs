namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class CompilationExt
    {
        internal static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, TypeNameArgument typeName, bool ignoreCase)
        {
            if (typeName.TryGetGeneric(out var metadataName, out var arity, out var typeArguments))
            {

            }

            return GetTypeByMetadataName(compilation, typeName.Value, ignoreCase);
        }

        internal static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, string fullyQualifiedMetadataName, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
            }

            return compilation.Assembly.GetTypeByMetadataName(fullyQualifiedMetadataName, ignoreCase: true);
        }
    }
}
