namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class REFL001InvokeCastReturnValue
    {
        public const string DiagnosticId = "REFL001";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Cast return value to the correct type.",
            messageFormat: "Cast return value to {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast return value to the correct type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
