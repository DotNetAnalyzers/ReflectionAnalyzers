namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL046DefaultMemberMustExist
    {
        public const string DiagnosticId = "REFL046";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The specified default member does not exist.",
            messageFormat: "The specified default member does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The specified default member does not exist.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
