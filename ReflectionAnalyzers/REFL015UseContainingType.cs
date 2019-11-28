﻿namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class REFL015UseContainingType
    {
        public const string DiagnosticId = "REFL015";

        public static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Use the containing type.",
            messageFormat: "Use the containing type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use the containing type.");
    }
}
