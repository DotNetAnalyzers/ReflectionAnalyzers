namespace ReflectionAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;

    internal static class AssemblyExt
    {
        internal static INamedTypeSymbol GetTypeByMetadataName(this IAssemblySymbol assembly, string fullyQualifiedMetadataName, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return assembly.GetTypeByMetadataName(fullyQualifiedMetadataName);
            }

            return assembly.GetTypeByMetadataName(fullyQualifiedMetadataName) ??
                   GetTypeByMetadataNameIgnoreCase(assembly.GlobalNamespace);

            INamedTypeSymbol GetTypeByMetadataNameIgnoreCase(INamespaceSymbol ns)
            {
                foreach (var member in ns.GetTypeMembers())
                {
                    if (fullyQualifiedMetadataName.EndsWith(member.MetadataName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(member.QualifiedMetadataName(), fullyQualifiedMetadataName, StringComparison.OrdinalIgnoreCase))
                    {
                        return member;
                    }
                }

                foreach (var nested in ns.GetNamespaceMembers())
                {
                    if (GetTypeByMetadataNameIgnoreCase(nested) is INamedTypeSymbol match)
                    {
                        return match;
                    }
                }

                return null;
            }
        }
    }
}
