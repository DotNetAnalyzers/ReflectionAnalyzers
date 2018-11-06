namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL045InsufficientFlags
    {
        internal const string DiagnosticId = "REFL045";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "These flags are insufficient to match any members.",
            messageFormat: "These flags are insufficient to match any members.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "These flags are insufficient to match any members.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
