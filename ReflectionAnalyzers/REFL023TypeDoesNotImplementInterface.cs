namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL023TypeDoesNotImplementInterface
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL023",
            title: "The type does not implement the interface.",
            messageFormat: "The type does not implement the interface.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not implement the interface.");
    }
}
