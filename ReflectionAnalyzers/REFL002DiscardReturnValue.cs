namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL002DiscardReturnValue
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL002",
            title: "Discard the return value.",
            messageFormat: "Discard the return value.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method returns void. Discard the return value.");
    }
}
