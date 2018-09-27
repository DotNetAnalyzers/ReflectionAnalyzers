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
            REFL001CastReturnValue.Descriptor,
            REFL002DiscardReturnValue.Descriptor,
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
                invocation.ArgumentList != null &&
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
                    if (method.ReturnsVoid &&
                        !IsResultDiscarded(invocation))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL002DiscardReturnValue.Descriptor, invocation.GetLocation()));
                    }

                    if (!method.ReturnsVoid &&
                        ShouldCast(invocation))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL001CastReturnValue.Descriptor, invocation.GetLocation()));
                    }

                    if (Array.TryGetValues(parametersArg.Expression, context, out var values) &&
                    Arguments.TryFindFirstMisMatch(method.Parameters, values, context, out var misMatch) == true)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor, misMatch?.GetLocation() ?? parametersArg.GetLocation()));
                    }
                }
            }
        }

        private static bool IsResultDiscarded(InvocationExpressionSyntax invocation)
        {
            switch (invocation.Parent)
            {
                case ArgumentSyntax _:
                case MemberAccessExpressionSyntax _:
                case CastExpressionSyntax _:
                case BinaryExpressionSyntax _:
                    return false;
                case AssignmentExpressionSyntax assignment:
                    return assignment.Left is IdentifierNameSyntax identifierName && IsDiscardName(identifierName.Identifier.ValueText);
                case EqualsValueClauseSyntax equalsValueClause when equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator:
                    return IsDiscardName(variableDeclarator.Identifier.ValueText);
                default:
                    return true;
            }
        }

        private static bool ShouldCast(InvocationExpressionSyntax invocation)
        {
            switch (invocation.Parent)
            {
                case EqualsValueClauseSyntax equalsValueClause when equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator:
                    return !IsDiscardName(variableDeclarator.Identifier.ValueText);
                default:
                    return false;
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

        private static bool IsDiscardName(string text)
        {
            foreach (var c in text)
            {
                if (c != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
