namespace ReflectionAnalyzers;

using System.Threading;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct GetConstructor
{
    internal readonly InvocationExpressionSyntax Invocation;
    internal readonly IMethodSymbol Target;
    internal readonly ReflectedMember Member;
    internal readonly Flags Flags;
    internal readonly Types Types;

    private GetConstructor(InvocationExpressionSyntax invocation, IMethodSymbol target, ReflectedMember member, Flags flags, Types types)
    {
        this.Invocation = invocation;
        this.Target = target;
        this.Member = member;
        this.Flags = flags;
        this.Types = types;
    }

    internal IMethodSymbol? Single => this.Member.Match == FilterMatch.Single ? (IMethodSymbol)this.Member.Symbol! : null;

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetConstructor.
    /// </summary>
    internal static GetConstructor? Match(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (GetX.FindInvocation(candidate, semanticModel, cancellationToken) is { } invocation)
        {
            return Match(invocation, semanticModel, cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetConstructor.
    /// </summary>
    internal static GetConstructor? Match(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (candidate.TryGetTarget(KnownSymbol.Type.GetConstructor, semanticModel, cancellationToken, out var target))
        {
            if (ReflectedMember.TryGetType(candidate, semanticModel, cancellationToken, out var type, out var typeSource) &&
                Flags.TryCreate(candidate, target, semanticModel, cancellationToken, out var flags) &&
                Types.TryCreate(candidate, target, semanticModel, cancellationToken, out var types) &&
                ReflectedMember.TryCreate(target, candidate, type, typeSource, Name.Ctor, flags.Effective, types, semanticModel.Compilation, out var member))
            {
                return new GetConstructor(candidate, target, member, flags, types);
            }

            if (Flags.TryCreate(candidate, target, semanticModel, cancellationToken, out flags) &&
                flags.AreInSufficient)
            {
                member = new ReflectedMember(type, typeSource, null, target, candidate, FilterMatch.InSufficientFlags);
                _ = Types.TryCreate(candidate, target, semanticModel, cancellationToken, out types);
                return new GetConstructor(candidate, target, member, flags, types);
            }
        }

        return null;
    }
}
