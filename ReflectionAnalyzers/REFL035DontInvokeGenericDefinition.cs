namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL035DontInvokeGenericDefinition
    {
        public const string DiagnosticId = "REFL035";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't call Invoke on a generic definition.",
            messageFormat: "Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't call Invoke on a generic definition.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
