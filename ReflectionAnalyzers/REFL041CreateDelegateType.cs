namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL041CreateDelegateType
    {
        internal const string DiagnosticId = "REFL041";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Delegate type is not matching.",
            messageFormat: "Delegate type is not matching expected {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Delegate type is not matching.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}