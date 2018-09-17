namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class BindingFlagsAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(REFL007BindingFlagsOrder.Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.BitwiseOrExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                TryGetFlags(context, out var flags))
            {
                if (BindingFlagsWalker.HasWrongOrder(flags, out var expectedFlags))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL007BindingFlagsOrder.Descriptor,
                            flags.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(ExpressionSyntax), expectedFlags),
                            $" Expected: {expectedFlags}."));
                }
            }
        }

        private static bool TryGetFlags(SyntaxNodeAnalysisContext context, out BinaryExpressionSyntax flags)
        {
            flags = context.Node as BinaryExpressionSyntax;
            return flags?.Parent is ArgumentSyntax &&
                   context.SemanticModel.TryGetType(flags, context.CancellationToken, out var type) &&
                   type == KnownSymbol.BindingFlags;
        }
    }
}
