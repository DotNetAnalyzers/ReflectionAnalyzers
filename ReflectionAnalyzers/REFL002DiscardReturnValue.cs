namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL002DiscardReturnValue
    {
        public const string DiagnosticId = "REFL002";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Discard the return value.",
            messageFormat: "Discard the return value.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method returns void. Discard the return value.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
