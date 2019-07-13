namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL042FirstArgumentIsReferenceType
    {
        public const string DiagnosticId = "REFL042";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "First argument must be reference type.",
            messageFormat: "First argument must be reference type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "First argument must be reference type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
