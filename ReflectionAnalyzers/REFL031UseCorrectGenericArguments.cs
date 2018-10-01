namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL031UseCorrectGenericArguments
    {
        public const string DiagnosticId = "REFL031";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use generic arguments that satisfies the type parameters.",
            messageFormat: "Use generic arguments that satisfies the type parameters. {0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use generic arguments that satisfies the type parameters.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
