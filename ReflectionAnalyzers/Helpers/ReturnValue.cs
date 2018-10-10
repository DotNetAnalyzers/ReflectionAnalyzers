namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class ReturnValue
    {
        internal static bool ShouldCast(InvocationExpressionSyntax invocation, ITypeSymbol returnType, SyntaxNodeAnalysisContext context)
        {
            if (invocation != null &&
                returnType != null &&
                returnType != KnownSymbol.Object &&
                context.SemanticModel.IsAccessible(context.Node.SpanStart, returnType))
            {
                switch (invocation.Parent)
                {
                    case EqualsValueClauseSyntax equalsValueClause when equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator:
                        return !IsDiscardName(variableDeclarator.Identifier.ValueText);
                    default:
                        return false;
                }
            }

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
