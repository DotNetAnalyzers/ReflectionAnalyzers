namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL024PreferNullOverEmptyArray
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL024",
            title: "Prefer null over empty array.",
            messageFormat: "Prefer null over empty array.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer null over empty array.");
    }
}
