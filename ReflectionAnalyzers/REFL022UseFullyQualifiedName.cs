namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL022UseFullyQualifiedName
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL022",
            title: "Use fully qualified name.",
            messageFormat: "Use fully qualified name.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use fully qualified name.");
    }
}
