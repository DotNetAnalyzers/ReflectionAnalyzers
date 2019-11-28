namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL043FirstArgumentType
    {
        public const string DiagnosticId = "REFL043";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "First argument must match type.",
            messageFormat: "First argument must be of type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "First argument must match type.");
    }
}
