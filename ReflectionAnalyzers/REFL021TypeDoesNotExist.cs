namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL021TypeDoesNotExist
    {
        public const string DiagnosticId = "REFL021";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The type does not exist.",
            messageFormat: "The type does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not exist.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
