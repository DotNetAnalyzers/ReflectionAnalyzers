namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL034DontMakeGeneric
    {
        public const string DiagnosticId = "REFL034";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't call MakeGeneric when not a generic definition.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't call MakeGeneric when not a generic definition.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
