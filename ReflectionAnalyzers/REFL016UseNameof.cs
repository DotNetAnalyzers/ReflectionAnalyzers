namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL016UseNameof
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL016",
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use nameof.");
    }
}
