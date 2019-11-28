namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL024PreferNullOverEmptyArray
    {
        public const string DiagnosticId = "REFL024";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Prefer null over empty array.",
            messageFormat: "Prefer null over empty array.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer null over empty array.");
    }
}
