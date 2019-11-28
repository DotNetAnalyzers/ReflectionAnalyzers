namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL016UseNameof
    {
        public const string DiagnosticId = "REFL016";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use nameof.");
    }
}
