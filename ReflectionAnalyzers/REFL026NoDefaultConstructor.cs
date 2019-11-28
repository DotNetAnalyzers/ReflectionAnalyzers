namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL026NoDefaultConstructor
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL026",
            title: "No parameterless constructor defined for this object.",
            messageFormat: "No parameterless constructor defined for {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "No parameterless constructor defined for this object.");
    }
}
