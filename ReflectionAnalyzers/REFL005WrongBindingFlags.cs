namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL005WrongBindingFlags
    {
        public const string DiagnosticId = "REFL005";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "There is no method matching the name and binding flags.",
            messageFormat: "There is no method matching the name and binding flags.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is no method matching the name and binding flags.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
