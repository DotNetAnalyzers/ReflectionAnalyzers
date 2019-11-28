namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL003MemberDoesNotExist
    {
        public const string DiagnosticId = "REFL003";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "The member does not exist.",
            messageFormat: "The type {0} does not have a member named {1}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method does not exist.");
    }
}
