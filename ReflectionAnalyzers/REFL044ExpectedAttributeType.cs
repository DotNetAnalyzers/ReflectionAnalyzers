namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL044ExpectedAttributeType
    {
        public const string DiagnosticId = "REFL044";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Expected attribute type.",
            messageFormat: "Expected attribute type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Expected attribute type.");
    }
}
