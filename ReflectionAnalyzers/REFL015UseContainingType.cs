namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL015UseContainingType
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL015",
            title: "Use the containing type.",
            messageFormat: "Use the containing type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use the containing type.");
    }
}
