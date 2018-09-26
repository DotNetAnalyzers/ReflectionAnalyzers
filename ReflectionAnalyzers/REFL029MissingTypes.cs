namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL029MissingTypes
    {
        public const string DiagnosticId = "REFL029";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Specify types in case an overload is added in the future.",
            messageFormat: "Specify types in case an overload is added in the future.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Specify types in case an overload is added in the future.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}