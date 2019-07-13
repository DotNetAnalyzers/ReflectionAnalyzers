namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL036CheckNull
    {
        public const string DiagnosticId = "REFL036";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Pass 'throwOnError: true' or check if null.",
            messageFormat: "Pass 'throwOnError: true' or check if null.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Pass 'throwOnError: true' or check if null.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
