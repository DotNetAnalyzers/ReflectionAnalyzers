namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class MakeGenericAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL031UseCorrectGenericParameters.Descriptor);

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
                invocation.ArgumentList != null &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (invocation.TryGetTarget(KnownSymbol.MethodInfo.MakeGenericMethod, context.SemanticModel, context.CancellationToken, out var makeGenericMethod) &&
                    makeGenericMethod.Parameters.Length == 1 &&
                    makeGenericMethod.TryFindParameter("typeArguments", out _) &&
                    GetX.TryGetMethod(memberAccess, context, out var method) &&
                    method.IsGenericMethod)
                {

                }
            }
        }
    }
}
