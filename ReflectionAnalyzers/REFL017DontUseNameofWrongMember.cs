namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL017DontUseNameofWrongMember
    {
        public const string DiagnosticId = "REFL017";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't use name of wrong member",
            messageFormat: "Don't use name of wrong member",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use name of wrong member",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
