namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL011DuplicateBindingFlags
    {
        public const string DiagnosticId = "REFL011";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Duplicate BindingFlag.",
            messageFormat: "Duplicate flag.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Duplicate BindingFlag.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
