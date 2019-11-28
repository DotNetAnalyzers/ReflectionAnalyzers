namespace ReflectionAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct PropertyInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IPropertySymbol Property;

        internal PropertyInfo(INamedTypeSymbol reflectedType, IPropertySymbol property)
        {
            this.ReflectedType = reflectedType;
            this.Property = property;
        }

        internal static bool TryGet(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out PropertyInfo propertyInfo)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetProperty(invocation, semanticModel, cancellationToken, out var member, out _, out _, out _) &&
                         member.ReflectedType is { } &&
                         member.Symbol is IPropertySymbol property:
                    propertyInfo = new PropertyInfo(member.ReflectedType, property);
                    return true;
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                semanticModel.TryGetSymbol(expression, cancellationToken, out var local))
            {
                propertyInfo = default;
                return AssignedValue.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue) &&
                       TryGet(assignedValue, semanticModel, cancellationToken, out propertyInfo);
            }

            propertyInfo = default;
            return false;
        }
    }
}
