namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL043FirstArgumentType
    {
        internal const string DiagnosticId = "REFL043";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "First argument must match type.",
            messageFormat: "First argument must be of type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "First argument must match type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
