namespace ReflectionAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct EventInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IEventSymbol Event;

        internal EventInfo(INamedTypeSymbol reflectedType, IEventSymbol @event)
        {
            this.ReflectedType = reflectedType;
            this.Event = @event;
        }

        internal static EventInfo? Find(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return expression switch
            {
                InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetEvent(invocation, semanticModel, cancellationToken, out var member, out _, out _) &&
                         member.ReflectedType is { } &&
                         member.Symbol is IEventSymbol @event
                    => new EventInfo(member.ReflectedType, @event),
                IdentifierNameSyntax identifierName => FindAssigned(identifierName),
                MemberAccessExpressionSyntax memberAccess => FindAssigned(memberAccess),
                _ => null,
            };

            EventInfo? FindAssigned(ExpressionSyntax member)
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
