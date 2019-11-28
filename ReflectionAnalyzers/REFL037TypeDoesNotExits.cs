namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL037TypeDoesNotExits
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL037",
            title: "The type does not exist.",
            messageFormat: "The type does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not exist.");
    }
}
