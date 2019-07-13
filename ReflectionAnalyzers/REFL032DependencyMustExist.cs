namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL032DependencyMustExist
    {
        public const string DiagnosticId = "REFL032";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The dependency does not exist.",
            messageFormat: "The dependency does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The dependency does not exist.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
