namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL009MemberCantBeFound
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL009",
            title: "The referenced member is not known to exist.",
            messageFormat: "The referenced member {0} is not known to exist in {1}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The referenced member is not known to exist.");
    }
}
