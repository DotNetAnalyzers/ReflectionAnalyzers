namespace ReflectionAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct EventInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IEventSymbol Event;

        internal EventInfo(INamedTypeSymbol reflectedType, IEventSymbol @event)
        {
            this.ReflectedType = reflectedType;
            this.Event = @event;
        }

        internal static bool TryGet(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out EventInfo eventInfo)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetEvent(invocation, semanticModel, cancellationToken, out var member, out _, out _) &&
                         member.ReflectedType is { } &&
                         member.Symbol is IEventSymbol @event:
                    eventInfo = new EventInfo(member.ReflectedType, @event);
                    return true;
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                semanticModel.TryGetSymbol(expression, cancellationToken, out var local))
            {
                eventInfo = default;
                return AssignedValue.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue) &&
                       TryGet(assignedValue, semanticModel, cancellationToken, out eventInfo);
            }

            eventInfo = default;
            return false;
        }
    }
}
