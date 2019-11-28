namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL021TypeDoesNotExist
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL021",
            title: "The type does not exist.",
            messageFormat: "The type does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not exist.");
    }
}
