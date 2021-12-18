namespace ReflectionAnalyzers;

using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct GetProperty
{
    internal readonly InvocationExpressionSyntax Invocation;
    internal readonly IMethodSymbol Target;
    internal readonly ReflectedMember Member;
    internal readonly Name Name;
    internal readonly Flags Flags;
    internal readonly Types Types;

    private GetProperty(InvocationExpressionSyntax invocation, IMethodSymbol target, ReflectedMember member, Name name, Flags flags, Types types)
    {
        this.Invocation = invocation;
        this.Target = target;
        this.Member = member;
        this.Name = name;
        this.Flags = flags;
        this.Types = types;
    }

    internal IPropertySymbol? Single => this.Member.Match == FilterMatch.Single ? (IPropertySymbol)this.Member.Symbol! : null;

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetField.
    /// </summary>
    internal static GetProperty? Match(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return candidate switch
        {
            InvocationExpressionSyntax invocation
                => Match(invocation, semanticModel, cancellationToken),
            PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression, Operand: InvocationExpressionSyntax invocation }
                => Match(invocation, semanticModel, cancellationToken),
            MemberAccessExpressionSyntax { Expression: { } inner }
                => Match(inner, semanticModel, cancellationToken),
            MemberBindingExpressionSyntax { Parent.Parent: ConditionalAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation } }
                => Match(invocation, semanticModel, cancellationToken),
            IdentifierNameSyntax identifierName
                when semanticModel.TryGetSymbol(identifierName, cancellationToken, out ILocalSymbol? local) &&
                     AssignedValue.FindSingle(local, semanticModel, cancellationToken) is { } value
                => Match(value, semanticModel, cancellationToken),
            _ => null,
        };
    }

    /// <summary>
    /// Check if <paramref name="candidate"/> is a call to Type.GetField.
    /// </summary>
    internal static GetProperty? Match(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (candidate.TryGetTarget(KnownSymbol.Type.GetProperty, semanticModel, cancellationToken, out var target))
        {
            if (ReflectedMember.TryGetType(candidate, semanticModel, cancellationToken, out var type, out var typeSource) &&
                Name.TryCreate(candidate, target, semanticModel, cancellationToken, out var name) &&
                Flags.TryCreate(candidate, target, semanticModel, cancellationToken, out var flags) &&
                Types.TryCreate(candidate, target, semanticModel, cancellationToken, out var types))
            {
                return ReflectedMember.TryCreate(target, candidate, type, typeSource, name, flags.Effective, types, semanticModel.Compilation, out var member)
                    ? new GetProperty(candidate, target, member, name, flags, types)
                    : null;
            }

            if (Flags.TryCreate(candidate, target, semanticModel, cancellationToken, out flags) &&
                flags.AreInSufficient)
            {
                _ = Name.TryCreate(candidate, target, semanticModel, cancellationToken, out name);
                _ = Types.TryCreate(candidate, target, semanticModel, cancellationToken, out types);
                var member = new ReflectedMember(type, typeSource, null, target, candidate, FilterMatch.InSufficientFlags);
                return new GetProperty(candidate, target, member, name, flags, types);
            }
        }

        return null;
    }
}
