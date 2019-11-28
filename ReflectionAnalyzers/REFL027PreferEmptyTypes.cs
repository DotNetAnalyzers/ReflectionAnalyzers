namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL027PreferEmptyTypes
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL027",
            title: "Prefer Type.EmptyTypes.",
            messageFormat: "Prefer Type.EmptyTypes.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer Type.EmptyTypes.");
    }
}
