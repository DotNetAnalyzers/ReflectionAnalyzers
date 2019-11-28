namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL034DontMakeGeneric
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL034",
            title: "Don't call MakeGeneric when not a generic definition.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't call MakeGeneric when not a generic definition.");
    }
}
