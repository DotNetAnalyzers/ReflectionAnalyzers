namespace ReflectionAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
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
            if (GetEvent.Match(expression, semanticModel, cancellationToken) is { Member: { ReflectedType: { } reflectedType, Symbol: IEventSymbol symbol } })
            {
                return new EventInfo(reflectedType, symbol);
            }

            return null;
        }
    }
}
