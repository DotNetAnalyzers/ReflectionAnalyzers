namespace ReflectionAnalyzers
{
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
        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetNestedType.
        /// </summary>
        internal static bool TryMatchGetNestedType(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetNestedType, semanticModel, cancellationToken, out member, out name, out flags);
        }

        internal static InvocationExpressionSyntax? FindInvocation(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return candidate switch
            {
                InvocationExpressionSyntax invocation
                    => invocation,
                PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression, Operand: InvocationExpressionSyntax invocation }
                    => invocation,
                MemberAccessExpressionSyntax { Expression: { } inner }
                    => FindInvocation(inner, semanticModel, cancellationToken),
                MemberBindingExpressionSyntax { Parent.Parent: ConditionalAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation } }
                    => invocation,
                IdentifierNameSyntax identifierName
                    when semanticModel.TryGetSymbol(identifierName, cancellationToken, out ILocalSymbol? local) &&
                         AssignedValue.FindSingle(local, semanticModel, cancellationToken) is { } value
                    => FindInvocation(value, semanticModel, cancellationToken),
                _ => null,
            };
        }

        /// <summary>
        /// Handles GetField, GetEvent, GetMember, GetMethod...
        /// </summary>
        private static bool TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags)
        {
            if (invocation.TryGetTarget(getXMethod, semanticModel, cancellationToken, out var getX))
            {
                if (ReflectedMember.TryGetType(invocation, semanticModel, cancellationToken, out var type, out var typeSource) &&
                    Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name) &&
                    Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    ReflectedMember.TryCreate(getX, invocation, type, typeSource, name, flags.Effective, Types.Any, semanticModel.Compilation, out member))
                {
                    return true;
                }

                if (getXMethod.Name != "GetNestedType" &&
                    Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    flags.AreInSufficient)
                {
                    _ = Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name);
                    member = new ReflectedMember(type, typeSource, null, getX, invocation, FilterMatch.InSufficientFlags);
                    return true;
                }
            }

            member = default;
            name = default;
            flags = default;
            return false;
        }
    }
}
