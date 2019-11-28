namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL011DuplicateBindingFlags
    {
        public const string DiagnosticId = "REFL011";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Duplicate BindingFlag.",
            messageFormat: "Duplicate flag.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Duplicate BindingFlag.");
    }
}
