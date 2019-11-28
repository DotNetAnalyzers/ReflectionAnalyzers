namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL028CastReturnValueToCorrectType
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL028",
            title: "Cast return value to correct type.",
            messageFormat: "Cast return value to {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast return value to correct type.");
    }
}
