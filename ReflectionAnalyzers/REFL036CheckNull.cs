namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL036CheckNull
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL036",
            title: "Pass 'throwOnError: true' or check if null.",
            messageFormat: "Pass 'throwOnError: true' or check if null.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Pass 'throwOnError: true' or check if null.");
    }
}
