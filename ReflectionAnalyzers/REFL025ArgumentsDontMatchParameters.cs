namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL025ArgumentsDontMatchParameters
    {
        public const string DiagnosticId = "REFL025";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use correct arguments.",
            messageFormat: "Use correct arguments.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct arguments.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
