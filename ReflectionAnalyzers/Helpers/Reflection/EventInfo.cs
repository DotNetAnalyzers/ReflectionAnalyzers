namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct EventInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IEventSymbol Event;

        internal EventInfo(INamedTypeSymbol reflectedType, IEventSymbol @event)
        {
            this.ReflectedType = reflectedType;
            this.Event = @event;
        }

        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out EventInfo eventInfo)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation when GetX.TryMatchGetEvent(invocation, context, out var member, out _, out _) &&
                                                                member.Symbol is IEventSymbol @event:
                    eventInfo = new EventInfo(member.ReflectedType, @event);
                    return true;
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out ISymbol local))
            {
                eventInfo = default(EventInfo);
                return AssignedValue.TryGetSingle(local, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                       TryGet(assignedValue, context, out eventInfo);
            }

            eventInfo = default(EventInfo);
            return false;
        }
    }
}
