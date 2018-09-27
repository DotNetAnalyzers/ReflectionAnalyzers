namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL030UseCorrectObj
    {
        public const string DiagnosticId = "REFL030";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use correct obj parameter.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct obj parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}