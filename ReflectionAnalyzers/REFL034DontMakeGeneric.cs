namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL034DontMakeGeneric
    {
        public const string DiagnosticId = "REFL034";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
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
