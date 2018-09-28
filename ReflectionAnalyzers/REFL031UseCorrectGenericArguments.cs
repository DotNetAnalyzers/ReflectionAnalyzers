namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL031UseCorrectGenericArguments
    {
        public const string DiagnosticId = "REFL031";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use correct generic parameters.",
            messageFormat: "Use correct generic parameters.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct generic parameters.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
