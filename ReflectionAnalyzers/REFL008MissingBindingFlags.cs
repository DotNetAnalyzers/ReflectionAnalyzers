namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL008MissingBindingFlags
    {
        public const string DiagnosticId = "REFL008";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Specify binding flags for better performance and less fragile code.",
            messageFormat: "Specify binding flags for better performance and less fragile code.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Specify binding flags for better performance and less fragile code.");
    }
}
