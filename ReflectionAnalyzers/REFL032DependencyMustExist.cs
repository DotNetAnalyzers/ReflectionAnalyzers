namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL032DependencyMustExist
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL032",
            title: "The dependency does not exist.",
            messageFormat: "The dependency does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The dependency does not exist.");
    }
}
