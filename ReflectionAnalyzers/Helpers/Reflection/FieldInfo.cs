namespace ReflectionAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct FieldInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IFieldSymbol Field;

        internal FieldInfo(INamedTypeSymbol reflectedType, IFieldSymbol field)
        {
            this.ReflectedType = reflectedType;
            this.Field = field;
        }

        internal static FieldInfo? Find(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return expression switch
            {
                InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetField(invocation, semanticModel, cancellationToken, out var member, out _, out _) &&
                         member is { ReflectedType: { } reflectedType, Symbol: IFieldSymbol field }
                    => new FieldInfo(reflectedType, field),
                IdentifierNameSyntax identifierName => FindAssigned(identifierName),
                MemberAccessExpressionSyntax memberAccess => FindAssigned(memberAccess),
                _ => null,
            };

            FieldInfo? FindAssigned(ExpressionSyntax member)
            {
                if (member.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                    semanticModel.TryGetSymbol(member, cancellationToken, out var local) &&
                    AssignedValue.FindSingle(local, semanticModel, cancellationToken) is { } assignedValue)
                {
                    return Find(assignedValue, semanticModel, cancellationToken);
                }

                return null;
            }
        }
    }
}
