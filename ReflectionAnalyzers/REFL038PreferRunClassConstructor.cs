namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL038PreferRunClassConstructor
    {
        public const string DiagnosticId = "REFL038";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Prefer RuntimeHelpers.RunClassConstructor.",
            messageFormat: "Prefer RuntimeHelpers.RunClassConstructor.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The static constructor should only be run once. Prefer RuntimeHelpers.RunClassConstructor().");
    }
}
