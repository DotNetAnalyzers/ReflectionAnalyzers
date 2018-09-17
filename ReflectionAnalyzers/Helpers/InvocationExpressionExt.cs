namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
        internal static Location GetNameLocation(this InvocationExpressionSyntax invocation)
        {
            switch (invocation.Expression)
            {
                case IdentifierNameSyntax identifierName:
                    return identifierName.GetLocation();
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Name.GetLocation();
            }

            return invocation.GetLocation();
        }
    }
}
