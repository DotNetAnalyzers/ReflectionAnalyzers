namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL046DefaultMemberMustExist
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL046",
            title: "The specified default member does not exist.",
            messageFormat: "The specified default member does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The specified default member does not exist, or is not a valid target for Type.InvokeMember.");
    }
}
