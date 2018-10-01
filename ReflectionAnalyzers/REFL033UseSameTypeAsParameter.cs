namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL033UseSameTypeAsParameter
    {
        public const string DiagnosticId = "REFL033";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use the same type as the parameter.",
            messageFormat: "Use the same type as the parameter. Expected: {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use the same type as the parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
