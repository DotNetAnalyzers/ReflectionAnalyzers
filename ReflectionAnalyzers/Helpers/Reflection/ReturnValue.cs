namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ReturnValue
    {
        internal static ExpressionSyntax? ShouldCast(InvocationExpressionSyntax invocation, ITypeSymbol returnType, SemanticModel semanticModel)
        {
            if (returnType != KnownSymbol.Object &&
                semanticModel.IsAccessible(invocation.SpanStart, returnType))
            {
                return Expression(invocation);

                static ExpressionSyntax? Expression(ExpressionSyntax candidate)
                {
                    return candidate.Parent switch
                    {
                        EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax variableDeclarator }
                            when !IsDiscardName(variableDeclarator.Identifier.ValueText)
                            => candidate,
                        ConditionalAccessExpressionSyntax conditionalAccess => Expression(conditionalAccess),
                        ArrowExpressionClauseSyntax => candidate,
                        ReturnStatementSyntax => candidate,
                        _ => null,
                    };
                }
            }

            return null;
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
