namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL010PreferGenericGetCustomAttribute
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL010",
            title: "Prefer the generic extension method GetCustomAttribute<T>.",
            messageFormat: "Prefer the generic extension method GetCustomAttribute<{0}>.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer the generic extension method GetCustomAttribute<T>.");
    }
}
