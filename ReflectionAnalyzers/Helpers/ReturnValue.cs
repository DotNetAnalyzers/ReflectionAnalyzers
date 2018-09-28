namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ReturnValue
    {
        internal static bool ShouldCast(InvocationExpressionSyntax invocation)
        {
            switch (invocation.Parent)
            {
                case EqualsValueClauseSyntax equalsValueClause when equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator:
                    return !IsDiscardName(variableDeclarator.Identifier.ValueText);
                default:
                    return false;
            }
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
