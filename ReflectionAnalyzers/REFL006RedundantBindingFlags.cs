namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL006RedundantBindingFlags
    {
        public const string DiagnosticId = "REFL006";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The binding flags can be more precise.",
            messageFormat: "The binding flags can be more precise.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The binding flags can be more precise.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
