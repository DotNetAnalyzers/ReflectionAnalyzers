namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ReturnValue
    {
        internal static bool ShouldCast(InvocationExpressionSyntax invocation, ITypeSymbol returnType, SemanticModel semanticModel)
        {
            if (returnType != KnownSymbol.Object &&
                semanticModel.IsAccessible(invocation.SpanStart, returnType))
            {
                return invocation.Parent switch
                {
                    EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax variableDeclarator }
                        => !IsDiscardName(variableDeclarator.Identifier.ValueText),
                    _ => false,
                };
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
