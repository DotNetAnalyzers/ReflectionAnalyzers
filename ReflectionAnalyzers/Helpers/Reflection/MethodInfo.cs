namespace ReflectionAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct MethodInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IMethodSymbol Method;

        internal MethodInfo(INamedTypeSymbol reflectedType, IMethodSymbol method)
        {
            this.ReflectedType = reflectedType;
            this.Method = method;
        }

        internal static MethodInfo? Find(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return expression switch
            {
                InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetMethod(invocation, semanticModel, cancellationToken, out var member, out _, out _, out _) &&
                         member is { ReflectedType: { }, Symbol: IMethodSymbol method }
                    => new MethodInfo(member.ReflectedType, method),

                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyInfo.GetGetMethod, cancellationToken, out _) &&
                         PropertyInfo.Find(memberAccess.Expression, semanticModel, cancellationToken) is { ReflectedType: { } reflectedType, Property: { GetMethod: { } getMethod } }
                    => new MethodInfo(reflectedType, getMethod),

                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyInfo.GetSetMethod, cancellationToken, out _) &&
                         PropertyInfo.Find(memberAccess.Expression, semanticModel, cancellationToken) is { ReflectedType: { } reflectedType, Property: { SetMethod: { } setMethod } }
                    => new MethodInfo(reflectedType, setMethod),

                MemberAccessExpressionSyntax memberAccess
                         when semanticModel.TryGetSymbol(memberAccess, cancellationToken, out var symbol)
                         => symbol switch
                         {
                             { } when symbol == KnownSymbol.PropertyInfo.GetMethod &&
                                      PropertyInfo.Find(memberAccess.Expression, semanticModel, cancellationToken) is { ReflectedType: { } reflectedType, Property: { GetMethod: { } getMethod } }
                                 => new MethodInfo(reflectedType, getMethod),

                             { } when symbol == KnownSymbol.PropertyInfo.SetMethod &&
                                      PropertyInfo.Find(memberAccess.Expression, semanticModel, cancellationToken) is { ReflectedType: { } reflectedType, Property: { SetMethod: { } setMethod } }
                                 => new MethodInfo(reflectedType, setMethod),

                             { } when AssignedValue.TryGetSingle(symbol, semanticModel, cancellationToken, out var assignedValue)
                                 => Find(assignedValue, semanticModel, cancellationToken),
                             _ => null,
                         },

                IdentifierNameSyntax identifierName
                    when semanticModel.TryGetSymbol(identifierName, cancellationToken, out var local) &&
                         AssignedValue.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue)
                         => Find(assignedValue, semanticModel, cancellationToken),
                _ => null,
            };
        }
    }
}
