namespace ReflectionAnalyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class InvocationExpressionSyntaxExt
{
    internal static Location GetNameLocation(this InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifierName => identifierName.GetLocation(),
            MemberAccessExpressionSyntax { Name: { } name } => name.GetLocation(),
            _ => invocation.GetLocation(),
        };
    }
}
