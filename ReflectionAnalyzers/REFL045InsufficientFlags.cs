namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL045InsufficientFlags
    {
        public const string DiagnosticId = "REFL045";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "These flags are insufficient to match any members.",
            messageFormat: "These flags are insufficient to match any members.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "These flags are insufficient to match any members.");
    }
}
