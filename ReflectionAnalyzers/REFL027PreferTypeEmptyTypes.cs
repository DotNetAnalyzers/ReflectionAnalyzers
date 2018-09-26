namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL027PreferTypeEmptyTypes
    {
        public const string DiagnosticId = "REFL027";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer Type.EmptyTypes.",
            messageFormat: "Prefer Type.EmptyTypes.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer Type.EmptyTypes.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}