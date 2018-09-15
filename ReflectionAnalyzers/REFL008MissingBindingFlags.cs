namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL008MissingBindingFlags
    {
        public const string DiagnosticId = "REFL008";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Specify binding flags for better performance and less fragile code.",
            messageFormat: "Specify binding flags for better performance and less fragile code.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Specify binding flags for better performance and less fragile code.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
