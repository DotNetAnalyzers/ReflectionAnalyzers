namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InvokeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL002InvokeDiscardReturnValue.Descriptor,
            REFL024PreferNullOverEmptyArray.Descriptor,
            REFL025ArgumentsDontMatchParameters.Descriptor);

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
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                invocation.TryGetMethodName(out var name) &&
                name == "Invoke" &&
                context.SemanticModel.TryGetSymbol(invocation, context.CancellationToken, out var invoke) &&
                invoke.ContainingType.IsAssignableTo(KnownSymbol.MemberInfo, context.Compilation) &&
                invoke.Parameters.Length == 2 &&
                invoke.TryFindParameter("obj", out var objParameter) &&
                invocation.TryFindArgument(objParameter, out var objArg) &&
                invoke.TryFindParameter("parameters", out var parametersParameter) &&
                invocation.TryFindArgument(parametersParameter, out var parametersArg))
            {
                if (context.SemanticModel.TryGetType(parametersArg.Expression, context.CancellationToken, out var type) &&
                    type is IArrayTypeSymbol arrayType &&
                    arrayType.ElementType == KnownSymbol.Object &&
                    Array.IsCreatingEmpty(parametersArg.Expression, context))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL024PreferNullOverEmptyArray.Descriptor, parametersArg.GetLocation()));
                }

                if (TryGetMethod(memberAccess, context, out var method))
                {
                    if (Array.TryGetValues(parametersArg.Expression, context, out var values) &&
                    Arguments.TryFindFirstMisMatch(method.Parameters, values, context, out var misMatch) == true)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor, misMatch?.GetLocation() ?? parametersArg.GetLocation()));
                    }
                }
            }
        }

        private static bool TryGetMethod(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context, out IMethodSymbol method)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
            {
                var result = GetX.TryMatchGetMethod(parentInvocation, context, out _, out _, out _, out var member, out _, out _, out _, out _);
                if (result == GetXResult.Single &&
                    member is IMethodSymbol match)
                {
                    method = match;
                    return true;
                }
            }

            method = null;
            return false;
        }
    }
}
