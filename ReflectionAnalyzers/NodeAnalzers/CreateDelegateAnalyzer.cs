namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class CreateDelegateAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL041CreateDelegateType.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                invocation.TryGetTarget(KnownSymbol.Delegate.CreateDelegate, QualifiedParameter.Create(KnownSymbol.Type), QualifiedParameter.Create(KnownSymbol.MethodInfo), context.SemanticModel, context.CancellationToken, out var createDelegate, out var typeArg, out var memberArg))
            {
                if (typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                    GetX.TryMatchGetMethod(memberArg.Expression as InvocationExpressionSyntax, context, out var member, out _, out _, out _) &&
                    member.Symbol is IMethodSymbol method &&
                    method.IsStatic)
                {

                }
            }
        }
    }
}
