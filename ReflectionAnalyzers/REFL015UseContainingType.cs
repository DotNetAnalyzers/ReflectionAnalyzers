namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL015UseContainingType
    {
        public const string DiagnosticId = "REFL015";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use the containing type.",
            messageFormat: "Use the containing type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use the containing type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
