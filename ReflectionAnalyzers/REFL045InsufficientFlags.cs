namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL045InsufficientFlags
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL045",
            title: "These flags are insufficient to match any members.",
            messageFormat: "These flags are insufficient to match any members.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "These flags are insufficient to match any members.");
    }
}
