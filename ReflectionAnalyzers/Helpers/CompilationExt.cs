namespace ReflectionAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;

    internal static class CompilationExt
    {
        internal static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, TypeNameArgument typeName, bool ignoreCase)
        {
            if (typeName.TryGetGeneric(out var metadataName, out var arity, out var typeArguments))
            {
                throw new NotImplementedException();
                //if (GetTypeByMetadataName(compilation, metadataName, ignoreCase) is INamedTypeSymbol typeDef)
                //{
                //    return typeDef.Construct(typeArguments);
                //}

                //return null;
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
