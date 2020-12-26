namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ArgumentAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.REFL027PreferEmptyTypes);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ArgumentSyntax { Expression: { } expression } argument &&
                ShouldCheck(expression) &&
                Array.IsCreatingEmpty(expression, context.SemanticModel, context.CancellationToken) &&
                context.SemanticModel.TryGetType(expression, context.CancellationToken, out var type) &&
                type is IArrayTypeSymbol { ElementType: { } elementType } &&
                elementType == KnownSymbol.Type)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL027PreferEmptyTypes, argument.GetLocation()));
            }
        }

        private static bool ShouldCheck(ExpressionSyntax argument)
        {
            return argument.Kind() switch
            {
                SyntaxKind.InvocationExpression or SyntaxKind.ArrayCreationExpression or SyntaxKind.ImplicitArrayCreationExpression => true,
                _ => false,
            };
        }
    }
}
