namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL043FirstArgumentType
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL043",
            title: "First argument must match type.",
            messageFormat: "First argument must be of type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "First argument must match type.");
    }
}
