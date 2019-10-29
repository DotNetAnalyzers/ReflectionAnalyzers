namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Assembly
    {
        internal static bool HasVisibleTypes(this IAssemblySymbol assembly)
        {
            if (assembly.Locations.Any(x => x.IsInSource))
            {
                return true;
            }

            return false;
        }

        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out IAssemblySymbol? assembly)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax candidate when candidate.Name is IdentifierNameSyntax identifierName &&
                                                                 identifierName.Identifier.ValueText == "GetType":
                    return TryGet(candidate.Expression, context, out assembly);
                case MemberAccessExpressionSyntax candidate when candidate.Name is IdentifierNameSyntax identifierName &&
                                                                 identifierName.Identifier.ValueText == "Assembly" &&
                                                                 Type.TryGet(candidate.Expression, context, out var typeInAssembly, out _):
                    assembly = typeInAssembly.ContainingAssembly;
                    return assembly != null;
            }

            assembly = null;
            return false;
        }

        internal static INamedTypeSymbol? GetTypeByMetadataName(this IAssemblySymbol assembly, TypeNameArgument typeName, bool ignoreCase)
        {
            if (typeName.TryGetGeneric(out var generic))
            {
                return GetTypeByMetadataName(assembly, generic, ignoreCase);
            }

            return GetTypeByMetadataName(assembly, typeName.Value, ignoreCase);
        }

        internal static INamedTypeSymbol? GetTypeByMetadataName(this IAssemblySymbol assembly, string fullyQualifiedMetadataName, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return assembly.GetTypeByMetadataName(fullyQualifiedMetadataName);
            }

            return assembly.GetTypeByMetadataName(fullyQualifiedMetadataName) ??
                   GetTypeByMetadataNameIgnoreCase(assembly.GlobalNamespace);

            INamedTypeSymbol? GetTypeByMetadataNameIgnoreCase(INamespaceSymbol ns)
            {
                if (fullyQualifiedMetadataName.StartsWith(ns.MetadataName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var candidate in ns.GetTypeMembers())
                    {
                        if (fullyQualifiedMetadataName.EndsWith(candidate.MetadataName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(candidate.QualifiedMetadataName(), fullyQualifiedMetadataName, StringComparison.OrdinalIgnoreCase))
                        {
                            return candidate;
                        }
                    }

                    foreach (var nested in ns.GetNamespaceMembers())
                    {
                        if (GetTypeByMetadataNameIgnoreCase(nested) is INamedTypeSymbol match)
                        {
                            return match;
                        }
                    }
                }

                return null;
            }
        }

        private static INamedTypeSymbol? GetTypeByMetadataName(this IAssemblySymbol assembly, GenericTypeName genericTypeName, bool ignoreCase)
        {
            if (TryGetArgsTypes(out var args))
            {
                return assembly.GetTypeByMetadataName(genericTypeName.MetadataName, ignoreCase)?.Construct(args);
            }

            return null;

            bool TryGetArgsTypes(out ITypeSymbol[] result)
            {
                result = new ITypeSymbol[genericTypeName.TypeArguments.Length];
                for (var i = 0; i < genericTypeName.TypeArguments.Length; i++)
                {
                    var argument = genericTypeName.TypeArguments[i];
                    if (argument.TypeArguments == null)
                    {
                        var type = GetTypeByMetadataName(assembly, argument.MetadataName, ignoreCase);
                        if (type == null)
                        {
                            return false;
                        }

                        result[i] = type;
                    }
                    else
                    {
                        var type = GetTypeByMetadataName(assembly, new GenericTypeName(argument.MetadataName, argument.TypeArguments.ToImmutableArray()), ignoreCase);
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
