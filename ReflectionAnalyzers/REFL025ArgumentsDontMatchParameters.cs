namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL025ArgumentsDontMatchParameters
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL025",
            title: "Use correct arguments.",
            messageFormat: "Use correct arguments.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct arguments.");
    }
}
