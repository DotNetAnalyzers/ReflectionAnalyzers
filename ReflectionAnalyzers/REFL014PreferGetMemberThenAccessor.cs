namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL014PreferGetMemberThenAccessor
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL014",
            title: "Prefer GetMember().AccessorMethod.",
            messageFormat: "Prefer {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer GetMember().AccessorMethod.");
    }
}
