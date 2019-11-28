namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL039PreferTypeof
    {
        public const string DiagnosticId = "REFL039";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Prefer typeof(...) over instance.GetType when the type is sealed.",
            messageFormat: "Prefer typeof({0}) over instance.GetType when the type is sealed.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer typeof(...) over instance.GetType when the type is sealed.");
    }
}
