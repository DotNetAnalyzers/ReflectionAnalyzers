namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL017DontUseNameofWrongMember
    {
        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "REFL017",
            title: "Don't use name of wrong member.",
            messageFormat: "Don't use name of wrong member. Expected: {0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use name of wrong member.");
    }
}
