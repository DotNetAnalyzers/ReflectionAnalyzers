namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL009WrongTypes
    {
        public const string DiagnosticId = "REFL009";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "There is no member matching the types.",
            messageFormat: "There is no member matching the types.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is no member matching the types.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
