namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetTypeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL036CheckNull.Descriptor,
            REFL037TypeDoesNotExits.Descriptor);

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
                TryGetTarget(invocation, context, out var target) &&
                invocation.ArgumentList != null)
            {
                if (ShouldCheckNull(invocation, target) &&
                    invocation.Parent is MemberAccessExpressionSyntax)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL036CheckNull.Descriptor, invocation.GetLocation()));
                }

                if (TypeExists(invocation, target, context, out var nameArgument) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL037TypeDoesNotExits.Descriptor, nameArgument.GetLocation()));
                }
            }
        }

        private static bool ShouldCheckNull(InvocationExpressionSyntax invocation, IMethodSymbol target)
        {
            if (target.Parameters.TrySingle(out var parameter) &&
                parameter.Type == KnownSymbol.String)
            {
                return true;
            }

            if (target.ReturnType == KnownSymbol.Type &&
                target.Parameters.TryFirst(out parameter) &&
                parameter.Type == KnownSymbol.String &&
                target.TryFindParameter("throwOnError", out parameter) &&
                invocation.TryFindArgument(parameter, out var arg) &&
                arg.Expression.IsKind(SyntaxKind.FalseLiteralExpression))
            {
                return true;
            }

            return false;
        }

        private static bool? TypeExists(InvocationExpressionSyntax invocation, IMethodSymbol target, SyntaxNodeAnalysisContext context, out ArgumentSyntax nameArgument)
        {
            if (TryGetName(out nameArgument, out var name))
            {
                if (target == KnownSymbol.Type.GetType)
                {
                    return context.Compilation.GetTypeByMetadataName(name) != null;
                }
            }

            return null;

            bool TryGetName(out ArgumentSyntax nameArg, out string result)
            {
                if (target.Parameters.TrySingle(out var parameter) &&
                    parameter.Type == KnownSymbol.String &&
                    invocation.TryFindArgument(parameter, out nameArg))
                {
                    return nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out result);
                }

                if (target.ReturnType == KnownSymbol.Type &&
                    target.Parameters.TryFirst(out parameter) &&
                    parameter.Type == KnownSymbol.String &&
                    invocation.TryFindArgument(parameter, out nameArg) &&
                    target.TryFindParameter("throwOnError", out parameter))
                {
                    return nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out result);
                }

                result = null;
                nameArg = null;
                return false;
            }
        }

        private static bool TryGetTarget(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out IMethodSymbol target)
        {
            return invocation.TryGetTarget(KnownSymbol.Type.GetType, context.SemanticModel, context.CancellationToken, out target) ||
                   invocation.TryGetTarget(KnownSymbol.Assembly.GetType, context.SemanticModel, context.CancellationToken, out target) ||
                   invocation.TryGetTarget(KnownSymbol.AssemblyBuilder.GetType, context.SemanticModel, context.CancellationToken, out target);
        }
    }
}
