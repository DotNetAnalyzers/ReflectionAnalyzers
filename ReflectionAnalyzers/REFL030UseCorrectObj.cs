namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL030UseCorrectObj
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL030",
            title: "Use correct obj parameter.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct obj parameter.");
    }
}
