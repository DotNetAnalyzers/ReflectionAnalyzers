namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL044ExpectedAttributeType
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL044",
            title: "Expected attribute type.",
            messageFormat: "Expected attribute type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Expected attribute type.");
    }
}
