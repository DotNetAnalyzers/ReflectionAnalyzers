namespace ReflectionAnalyzers;

using System.Threading;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Helper for Type.GetField, Type.GetEvent, Type.GetMember, Type.GetMethod...
/// </summary>
internal static class GetX
{
    internal static InvocationExpressionSyntax? FindInvocation(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return candidate switch
        {
            InvocationExpressionSyntax invocation
                => invocation,
            PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression, Operand: { } operand }
                => operand switch
                {
                    InvocationExpressionSyntax invocation => invocation,
                    IdentifierNameSyntax identifierName => FindInvocation(identifierName, semanticModel, cancellationToken),
                    _ => null,
                },
            MemberAccessExpressionSyntax { Expression: { } inner }
                => FindInvocation(inner, semanticModel, cancellationToken),
            MemberBindingExpressionSyntax { Parent.Parent: ConditionalAccessExpressionSyntax { Expression: { } expression } }
                => expression switch
                {
                    InvocationExpressionSyntax invocation => invocation,
                    IdentifierNameSyntax identifierName => FindInvocation(identifierName, semanticModel, cancellationToken),
                    _ => null,
                },
            IdentifierNameSyntax identifierName
                when semanticModel.TryGetSymbol(identifierName, cancellationToken, out ILocalSymbol? local) &&
                     AssignedValue.FindSingle(local, semanticModel, cancellationToken) is { } value
                => FindInvocation(value, semanticModel, cancellationToken),
            _ => null,
        };
    }
}
