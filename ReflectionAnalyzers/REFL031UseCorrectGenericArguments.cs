namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL031UseCorrectGenericArguments
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL031",
            title: "Use generic arguments that satisfies the type parameters.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use generic arguments that satisfies the type parameters.");
    }
}
