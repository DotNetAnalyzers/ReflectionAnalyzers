namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL014PreferGetMemberThenAccessor
    {
        public const string DiagnosticId = "REFL014";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer GetMember().AccessorMethod.",
            messageFormat: "Prefer {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer GetMember().AccessorMethod.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
