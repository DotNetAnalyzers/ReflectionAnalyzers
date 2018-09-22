namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL017DontUseNameof
    {
        public const string DiagnosticId = "REFL017";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't use nameof.",
            messageFormat: "Don't use nameof.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use nameof.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
