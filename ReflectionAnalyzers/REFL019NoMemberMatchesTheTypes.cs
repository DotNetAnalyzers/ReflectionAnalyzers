namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL019NoMemberMatchesTheTypes
    {
        public const string DiagnosticId = "REFL019";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No member matches the types.",
            messageFormat: "No member matches the types.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "No member matches the types.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}