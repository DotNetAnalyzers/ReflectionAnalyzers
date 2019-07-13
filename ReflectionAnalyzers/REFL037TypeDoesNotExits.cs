namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL037TypeDoesNotExits
    {
        public const string DiagnosticId = "REFL037";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The type does not exist.",
            messageFormat: "The type does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not exist.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
