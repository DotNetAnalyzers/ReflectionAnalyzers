namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class CompilationExt
    {
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
