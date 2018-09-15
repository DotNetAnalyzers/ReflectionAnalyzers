namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL007BindingFlagsOrder
    {
        public const string DiagnosticId = "REFL007";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The binding flags are not in the expected order.",
            messageFormat: "The binding flags are not in the expected order.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The binding flags are not in the expected order.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
