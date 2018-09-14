namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL003GetMethodTargetDoesNotExist
    {
        public const string DiagnosticId = "REFL003";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The method does not exist.",
            messageFormat: "The type {0} does not have a method named {1}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method does not exist.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
