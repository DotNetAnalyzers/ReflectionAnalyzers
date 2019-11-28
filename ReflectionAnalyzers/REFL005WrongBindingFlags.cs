namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL005WrongBindingFlags
    {
        public const string DiagnosticId = "REFL005";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "There is no member matching the filter.",
            messageFormat: "There is no member matching the filter.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is no member matching the filter.");
    }
}
