namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL036CheckNull
    {
        public const string DiagnosticId = "REFL036";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Check if null before using.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Check if null before using.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}