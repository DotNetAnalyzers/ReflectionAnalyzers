namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL019NoMemberMatchesTheTypes
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL019",
            title: "No member matches the types.",
            messageFormat: "No member matches the types.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "No member matches the types.");
    }
}
