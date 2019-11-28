namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL001CastReturnValue
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL001",
            title: "Cast return value to the correct type.",
            messageFormat: "Cast return value to the correct type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast return value to the correct type.");
    }
}
