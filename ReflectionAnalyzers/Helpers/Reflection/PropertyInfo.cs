namespace ReflectionAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct PropertyInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IPropertySymbol Property;

        internal PropertyInfo(INamedTypeSymbol reflectedType, IPropertySymbol property)
        {
            this.ReflectedType = reflectedType;
            this.Property = property;
        }

        internal static PropertyInfo? Find(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetProperty(invocation, semanticModel, cancellationToken, out var member, out _, out _, out _) &&
                         member.ReflectedType is { } &&
                         member.Symbol is IPropertySymbol property:
                    return new PropertyInfo(member.ReflectedType, property);
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
