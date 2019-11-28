namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL031UseCorrectGenericArguments
    {
        public const string DiagnosticId = "REFL031";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Use generic arguments that satisfies the type parameters.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use generic arguments that satisfies the type parameters.");
    }
}
