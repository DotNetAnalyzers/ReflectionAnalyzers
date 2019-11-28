namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL018ExplicitImplementation
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL018",
            title: "The member is explicitly implemented.",
            messageFormat: "{0} is explicitly implemented.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The member is explicitly implemented.");
    }
}
