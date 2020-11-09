namespace ReflectionAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
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
            switch (expression)
            {
                case InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetMethod(invocation, semanticModel, cancellationToken, out var member, out _, out _, out _) &&
                         member is { ReflectedType: { }, Symbol: IMethodSymbol method }:
                    return new MethodInfo(member.ReflectedType, method);
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyInfo.GetGetMethod, cancellationToken, out _) &&
                         PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out var propertyInfo) &&
                         propertyInfo.Property.GetMethod is { } getMethod:
                    return new MethodInfo(propertyInfo.ReflectedType, getMethod);
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyInfo.GetSetMethod, cancellationToken, out _) &&
                     PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out var propertyInfo) &&
                     propertyInfo.Property.SetMethod is { } setMethod:
                    return new MethodInfo(propertyInfo.ReflectedType, setMethod);
                case MemberAccessExpressionSyntax memberAccess
                    when semanticModel.TryGetSymbol(memberAccess, cancellationToken, out var symbol):
                    if (symbol == KnownSymbol.PropertyInfo.GetMethod &&
                        PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out var property) &&
                        property is { Property: { GetMethod: { } } })
                    {
                        return new MethodInfo(property.ReflectedType, property.Property.GetMethod);
                    }

                    if (symbol == KnownSymbol.PropertyInfo.SetMethod &&
                        PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out property) &&
                        property is { Property: { SetMethod: { } } })
                    {
                        return new MethodInfo(property.ReflectedType, property.Property.SetMethod);
                    }

                    break;
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                semanticModel.TryGetSymbol(expression, cancellationToken, out var local) &&
                AssignedValue.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue))
            {
                return Find(assignedValue, semanticModel, cancellationToken);
            }

            return null;
        }
    }
}
