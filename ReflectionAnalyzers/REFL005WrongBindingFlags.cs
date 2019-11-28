namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL005WrongBindingFlags
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL005",
            title: "There is no member matching the filter.",
            messageFormat: "There is no member matching the filter.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is no member matching the filter.");
    }
}
