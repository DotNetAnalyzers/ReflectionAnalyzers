namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL042FirstArgumentIsReferenceType
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL042",
            title: "First argument must be reference type.",
            messageFormat: "First argument must be reference type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "First argument must be reference type.");
    }
}
