namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL039PreferTypeof
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL039",
            title: "Prefer typeof(...) over instance.GetType when the type is sealed.",
            messageFormat: "Prefer typeof({0}) over instance.GetType when the type is sealed.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer typeof(...) over instance.GetType when the type is sealed.");
    }
}
