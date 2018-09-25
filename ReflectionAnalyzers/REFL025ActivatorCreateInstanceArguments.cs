namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL025ActivatorCreateInstanceArguments
    {
        public const string DiagnosticId = "REFL025";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use correct arguments.",
            messageFormat: "Use correct arguments, expected: {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct arguments.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
