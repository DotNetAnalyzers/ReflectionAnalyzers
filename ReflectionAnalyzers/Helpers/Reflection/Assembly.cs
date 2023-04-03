namespace ReflectionAnalyzers;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    internal static IAssemblySymbol? Find(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return expression switch
        {
            MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier: { ValueText: "GetType" } } } candidate
                => Find(candidate.Expression, semanticModel, cancellationToken),
            MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier: { ValueText: "Assembly" } } } candidate
                when Type.TryGet(candidate.Expression, semanticModel, cancellationToken, out var typeInAssembly, out _)
                => typeInAssembly.ContainingAssembly,
            _ => null,
        };
    }

    internal static INamedTypeSymbol? GetTypeByMetadataName(this IAssemblySymbol assembly, TypeNameArgument typeName, bool ignoreCase)
    {
        if (typeName.TryGetGeneric() is { } generic)
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
                    if (GetTypeByMetadataNameIgnoreCase(nested) is { } match)
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
                if (argument.TypeArguments is null)
                {
                    var type = GetTypeByMetadataName(assembly, argument.MetadataName, ignoreCase);
                    if (type is null)
                    {
                        return false;
                    }

                    result[i] = type;
                }
                else
                {
                    var type = GetTypeByMetadataName(assembly, new GenericTypeName(argument.MetadataName, argument.TypeArguments.ToImmutableArray()), ignoreCase);
                    if (type is null)
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
