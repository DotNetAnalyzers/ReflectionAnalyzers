namespace ReflectionAnalyzers;

using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct GetField
{
    internal readonly InvocationExpressionSyntax Invocation;
    internal readonly IMethodSymbol Target;
    internal readonly ReflectedMember Member;
    internal readonly Name Name;
    internal readonly Flags Flags;

    private GetField(InvocationExpressionSyntax invocation, IMethodSymbol target, ReflectedMember member, Name name, Flags flags)
    {
        this.Invocation = invocation;
        this.Target = target;
        this.Member = member;
        this.Name = name;
        this.Flags = flags;
    }

    internal IFieldSymbol? Single => this.Member.Match == FilterMatch.Single ? (IFieldSymbol)this.Member.Symbol! : null;

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetField.
    /// </summary>
    internal static GetField? Match(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (GetX.FindInvocation(candidate, semanticModel, cancellationToken) is { } invocation)
        {
            return Match(invocation, semanticModel, cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetField.
    /// </summary>
    internal static GetField? Match(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (candidate.TryGetTarget(KnownSymbol.Type.GetField, semanticModel, cancellationToken, out var target))
        {
            if (ReflectedMember.TryGetType(candidate, semanticModel, cancellationToken, out var type, out var typeSource) &&
                Name.TryCreate(candidate, target, semanticModel, cancellationToken, out var name) &&
                Flags.TryCreate(candidate, target, semanticModel, cancellationToken, out var flags) &&
                ReflectedMember.TryCreate(target, candidate, type, typeSource, name, flags.Effective, Types.Any, semanticModel.Compilation, out var member))
            {
                return new GetField(candidate, target, member, name, flags);
            }

            if (Flags.TryCreate(candidate, target, semanticModel, cancellationToken, out flags) &&
                flags.AreInSufficient)
            {
                _ = Name.TryCreate(candidate, target, semanticModel, cancellationToken, out name);
                member = new ReflectedMember(type, typeSource, null, target, candidate, FilterMatch.InSufficientFlags);
                return new GetField(candidate, target, member, name, flags);
            }
        }

        return null;
    }
}
