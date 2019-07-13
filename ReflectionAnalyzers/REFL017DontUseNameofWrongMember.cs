namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL017DontUseNameofWrongMember
    {
        public const string DiagnosticId = "REFL017";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't use name of wrong member.",
            messageFormat: "Don't use name of wrong member. Expected: {0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use name of wrong member.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
