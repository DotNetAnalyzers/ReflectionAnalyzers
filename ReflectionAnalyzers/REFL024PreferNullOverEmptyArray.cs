namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL024PreferNullOverEmptyArray
    {
        public const string DiagnosticId = "REFL024";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer null over empty array.",
            messageFormat: "Prefer null over empty array.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer null over empty array.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}