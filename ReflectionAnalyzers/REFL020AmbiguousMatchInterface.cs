namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL020AmbiguousMatchInterface
    {
        public const string DiagnosticId = "REFL020";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "More than one interface is matching the name.",
            messageFormat: "More than one interface is matching the name.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "More than one interface is matching the name.");
    }
}
