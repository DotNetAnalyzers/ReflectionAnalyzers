namespace ReflectionAnalyzers;

using System.Threading;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct GetMethod
{
    internal readonly InvocationExpressionSyntax Invocation;
    internal readonly IMethodSymbol Target;
    internal readonly ReflectedMember Member;
    internal readonly Name Name;
    internal readonly Flags Flags;
    internal readonly Types Types;

    private GetMethod(InvocationExpressionSyntax invocation, IMethodSymbol target, ReflectedMember member, Name name, Flags flags, Types types)
    {
        this.Invocation = invocation;
        this.Target = target;
        this.Member = member;
        this.Name = name;
        this.Flags = flags;
        this.Types = types;
    }

    internal IMethodSymbol? Single => this.Member.Match == FilterMatch.Single ? (IMethodSymbol)this.Member.Symbol! : null;

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetMethod.
    /// </summary>
    internal static GetMethod? Match(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return candidate switch
        {
            InvocationExpressionSyntax invocation
                => Match(invocation, semanticModel, cancellationToken),
            PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression, Operand: InvocationExpressionSyntax invocation }
                => Match(invocation, semanticModel, cancellationToken),
            MemberAccessExpressionSyntax { Expression: { } inner }
                => Match(inner, semanticModel, cancellationToken),
            IdentifierNameSyntax identifierName
                when semanticModel.TryGetSymbol(identifierName, cancellationToken, out ILocalSymbol? local) &&
                     AssignedValue.FindSingle(local, semanticModel, cancellationToken) is { } value
                => Match(value, semanticModel, cancellationToken),
            _ => null,
        };
    }

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetMethod.
    /// </summary>
    internal static GetMethod? Match(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (candidate.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, cancellationToken, out var target) &&
            Flags.TryCreate(candidate, target, semanticModel, cancellationToken, out var flags) &&
            ReflectedMember.TryGetType(candidate, semanticModel, cancellationToken, out var type, out var typeSource))
        {
            _ = Name.TryCreate(candidate, target, semanticModel, cancellationToken, out var name);
            _ = Types.TryCreate(candidate, target, semanticModel, cancellationToken, out var types);
            if (flags.AreInSufficient)
            {
                return new GetMethod(candidate, target, new ReflectedMember(type, typeSource, null, target, candidate, FilterMatch.InSufficientFlags), name, flags, types);
            }

            if (ReflectedMember.TryCreate(target, candidate, type, typeSource, name, flags.Effective, types, semanticModel.Compilation, out var member))
            {
                return new GetMethod(candidate, target, member, name, flags, types);
            }
        }

        return null;
    }
}
