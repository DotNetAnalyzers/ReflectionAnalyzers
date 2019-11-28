namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL040PreferIsInstanceOfType
    {
        public const string DiagnosticId = "REFL040";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Prefer type.IsInstanceOfType(...).",
            messageFormat: "Prefer {0}.IsInstanceOfType({1}).",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer type.IsInstanceOfType(...).");
    }
}
