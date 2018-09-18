namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL005WrongBindingFlags
    {
        public const string DiagnosticId = "REFL005";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "There is no member matching the filter.",
            messageFormat: "There is no member matching the filter.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is no member matching the filter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
