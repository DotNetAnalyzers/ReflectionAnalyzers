namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL045AssignStructMember
    {
        internal const string DiagnosticId = "REFL045";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Assigning struct member.",
            messageFormat: "Assigning struct member.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Assigning struct member.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
