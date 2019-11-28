namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL013MemberIsOfWrongType
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL013",
            title: "The member is of the wrong type.",
            messageFormat: "The type {0} has a {1} named {2}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The member is of the wrong type.");
    }
}
