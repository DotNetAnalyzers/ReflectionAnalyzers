namespace ReflectionAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct MethodInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IMethodSymbol Method;

        internal MethodInfo(INamedTypeSymbol reflectedType, IMethodSymbol method)
        {
            this.ReflectedType = reflectedType;
            this.Method = method;
        }

        internal static bool TryGet(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out MethodInfo methodInfo)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetMethod(invocation, semanticModel, cancellationToken, out var member, out _, out _, out _) &&
                         member is { ReflectedType: { }, Symbol: IMethodSymbol method }:
                    methodInfo = new MethodInfo(member.ReflectedType, method);
                    return true;
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyInfo.GetGetMethod, cancellationToken, out _) &&
                         PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out var propertyInfo) &&
                         propertyInfo.Property.GetMethod is { } getMethod:
                    methodInfo = new MethodInfo(propertyInfo.ReflectedType, getMethod);
                    return true;
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyInfo.GetSetMethod, cancellationToken, out _) &&
                     PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out var propertyInfo) &&
                     propertyInfo.Property.SetMethod is { } setMethod:
                    methodInfo = new MethodInfo(propertyInfo.ReflectedType, setMethod);
                    return true;
                case MemberAccessExpressionSyntax memberAccess
                    when semanticModel.TryGetSymbol(memberAccess, cancellationToken, out var symbol):
                    if (symbol == KnownSymbol.PropertyInfo.GetMethod &&
                        PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out var property))
                    {
                        methodInfo = new MethodInfo(property.ReflectedType, property.Property.GetMethod);
                        return true;
                    }

                    if (symbol == KnownSymbol.PropertyInfo.SetMethod &&
                        PropertyInfo.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out property))
                    {
                        methodInfo = new MethodInfo(property.ReflectedType, property.Property.SetMethod);
                        return true;
                    }

                    break;
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                semanticModel.TryGetSymbol(expression, cancellationToken, out var local))
            {
                methodInfo = default;
                return AssignedValue.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue) &&
                       TryGet(assignedValue, semanticModel, cancellationToken, out methodInfo);
            }

            methodInfo = default;
            return false;
        }
    }
}
