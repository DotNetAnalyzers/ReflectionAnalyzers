namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Attribute = Gu.Roslyn.AnalyzerExtensions.Attribute;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DependencyAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL032DependencyMustExist.Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute &&
                Attribute.IsType(attribute, KnownSymbol.DependencyAttribute, context.SemanticModel, context.CancellationToken) &&
                Attribute.TryFindArgument(attribute, 0, "dependentAssemblyArgument", out var argument) &&
                context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string assemblyName) &&
                !context.Compilation.ReferencedAssemblyNames.TryFirst(x => x.Name == assemblyName, out _))
            {
                context.ReportDiagnostic(Diagnostic.Create(REFL032DependencyMustExist.Descriptor, argument.GetLocation()));
            }
        }
    }
}
