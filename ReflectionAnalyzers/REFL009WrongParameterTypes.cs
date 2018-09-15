namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL009WrongParameterTypes
    {
        public const string DiagnosticId = "REFL009";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "There is no method matching the types.",
            messageFormat: "There is no method matching the types.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is no method matching the types.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
