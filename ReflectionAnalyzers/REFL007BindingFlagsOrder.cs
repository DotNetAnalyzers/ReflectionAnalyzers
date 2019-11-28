namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL007BindingFlagsOrder
    {
        public const string DiagnosticId = "REFL007";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "The binding flags are not in the expected order.",
            messageFormat: "The binding flags are not in the expected order.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "The binding flags are not in the expected order.");
    }
}
