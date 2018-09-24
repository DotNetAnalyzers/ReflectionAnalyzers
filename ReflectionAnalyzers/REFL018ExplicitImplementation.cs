namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL018ExplicitImplementation
    {
        public const string DiagnosticId = "REFL018";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The member is explicitly implemented.",
            messageFormat: "{0} is explicitly implemented.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The member is explicitly implemented.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}