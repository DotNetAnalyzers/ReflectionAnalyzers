namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL033UseMoreSpecificTypes
    {
        public const string DiagnosticId = "REFL033";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use more specific types in the filter.",
            messageFormat: "Use more specific types in the filter.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use more specific types in the filter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
