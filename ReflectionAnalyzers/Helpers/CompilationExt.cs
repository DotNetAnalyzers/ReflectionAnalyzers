namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static class CompilationExt
    {
        internal static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, TypeNameArgument typeName, bool ignoreCase)
        {
            if (typeName.TryGetGeneric(out var generic))
            {
                return GetTypeByMetadataName(compilation, generic, ignoreCase);
            }

            return GetTypeByMetadataName(compilation, typeName.Value, ignoreCase);
        }

        internal static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, string fullyQualifiedMetadataName, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
            }

            return compilation.GetTypeByMetadataName(fullyQualifiedMetadataName) ??
                   compilation.Assembly.GetTypeByMetadataName(fullyQualifiedMetadataName, ignoreCase: true) ??
                   compilation.Assembly.Modules.SelectMany(x => x.ReferencedAssemblySymbols)
                              .Select(x => x.GetTypeByMetadataName(fullyQualifiedMetadataName, ignoreCase: true))
                              .FirstOrDefault();
        }

        private static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, GenericTypeName genericTypeName, bool ignoreCase)
        {
            if (TryGetArgsTypes(out var args))
            {
                return compilation.GetTypeByMetadataName(genericTypeName.MetadataName, ignoreCase).Construct(args);
            }

            return null;

            bool TryGetArgsTypes(out ITypeSymbol[] result)
            {
                result = new ITypeSymbol[genericTypeName.TypeArguments.Length];
                for (var i = 0; i < genericTypeName.TypeArguments.Length; i++)
                {
                    var argument = genericTypeName.TypeArguments[i];
                    if (argument.TypeArguments is IReadOnlyList<GenericTypeArgument> typeArguments)
                    {
                        var type = GetTypeByMetadataName(compilation, new GenericTypeName(argument.MetadataName, typeArguments.ToImmutableArray()), ignoreCase);
                        if (type == null)
                        {
                            return false;
                        }

                        result[i] = type;
                    }
                    else
                    {
                        var type = GetTypeByMetadataName(compilation, argument.MetadataName, ignoreCase);
                        if (type == null)
                        {
                            return false;
                        }

                        result[i] = type;
                    }
                }

                return true;
            }
        }
    }
}
