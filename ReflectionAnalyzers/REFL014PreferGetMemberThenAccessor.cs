﻿namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL014PreferGetMemberThenAccessor
    {
        public const string DiagnosticId = "REFL014";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Prefer GetMember().AccessorMethod.",
            messageFormat: "Prefer {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer GetMember().AccessorMethod.");
    }
}
