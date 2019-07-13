namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL012PreferIsDefined
    {
        public const string DiagnosticId = "REFL012";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer Attribute.IsDefined().",
            messageFormat: "Prefer Attribute.IsDefined().",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer Attribute.IsDefined().",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
