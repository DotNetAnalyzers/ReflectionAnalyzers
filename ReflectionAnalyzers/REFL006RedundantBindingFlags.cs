namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL006RedundantBindingFlags
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL006",
            title: "The binding flags can be more precise.",
            messageFormat: "The binding flags can be more precise.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The binding flags can be more precise.");
    }
}
