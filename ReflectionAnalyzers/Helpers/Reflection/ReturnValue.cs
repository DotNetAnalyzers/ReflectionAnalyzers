namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ReturnValue
    {
        internal static bool ShouldCast(InvocationExpressionSyntax invocation, ITypeSymbol returnType, SemanticModel semanticModel)
        {
            if (invocation != null &&
                returnType != null &&
                returnType != KnownSymbol.Object &&
                semanticModel.IsAccessible(invocation.SpanStart, returnType))
            {
                switch (invocation.Parent)
                {
                    case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax variableDeclarator }:
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
