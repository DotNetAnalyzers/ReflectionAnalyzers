namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL001CastReturnValue
    {
        public const string DiagnosticId = "REFL001";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Cast return value to the correct type.",
            messageFormat: "Cast return value to the correct type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast return value to the correct type.");
    }
}
