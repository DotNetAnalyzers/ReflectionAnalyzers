namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL013MemberIsOfWrongType
    {
        public const string DiagnosticId = "REFL013";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The member is of the wrong type.",
            messageFormat: "The type {0} has a {1} named {2}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The member is of the wrong type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
