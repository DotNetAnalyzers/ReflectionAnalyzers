namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL020AmbiguousMatchInterface
    {
        public const string DiagnosticId = "REFL020";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "More than one interface is matching the name.",
            messageFormat: "More than one interface is matching the name.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "More than one interface is matching the name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
