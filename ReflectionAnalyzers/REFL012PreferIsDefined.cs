namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL012PreferIsDefined
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL012",
            title: "Prefer Attribute.IsDefined().",
            messageFormat: "Prefer Attribute.IsDefined().",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer Attribute.IsDefined().");
    }
}
