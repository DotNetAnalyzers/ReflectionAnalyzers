namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL010PreferGenericGetCustomAttribute
    {
        public const string DiagnosticId = "REFL010";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer the generic extension method GetCustomAttribute<T>.",
            messageFormat: "Prefer the generic extension method GetCustomAttribute<{0}>.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer the generic extension method GetCustomAttribute<T>.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
