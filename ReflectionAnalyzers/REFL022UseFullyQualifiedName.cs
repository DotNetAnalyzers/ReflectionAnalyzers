namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL022UseFullyQualifiedName
    {
        public const string DiagnosticId = "REFL022";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use fully qualified name.",
            messageFormat: "Use fully qualified name.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use fully qualified name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
