namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL011DuplicateBindingFlags
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL011",
            title: "Duplicate BindingFlag.",
            messageFormat: "Duplicate flag.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Duplicate BindingFlag.");
    }
}
