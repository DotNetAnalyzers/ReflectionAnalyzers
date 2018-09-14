namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL004GetMethodAmbiguousMatch
    {
        public const string DiagnosticId = "REFL004";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "There is more than one method matching the criteria.",
            messageFormat: "There is more than one method matching the criteria.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is more than one method matching the criteria.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
