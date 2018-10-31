namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct EventInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IEventSymbol Event;

        public EventInfo(INamedTypeSymbol reflectedType, IEventSymbol @event)
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
                default:
                    eventInfo = default(EventInfo);
                    return false;
            }
        }
    }
}
