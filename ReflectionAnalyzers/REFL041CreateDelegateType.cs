namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL041CreateDelegateType
    {
        public const string DiagnosticId = "REFL041";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Delegate type is not matching.",
            messageFormat: "Delegate type is not matching expected {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Delegate type is not matching.");
    }
}
