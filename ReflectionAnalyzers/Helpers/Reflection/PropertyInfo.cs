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
                    when GetProperty.Match(invocation, semanticModel, cancellationToken) is { Member: { ReflectedType: { } reflectedType, Symbol: IPropertySymbol property } }:
                    return new PropertyInfo(reflectedType, property);
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                semanticModel.TryGetSymbol(expression, cancellationToken, out var local) &&
                AssignedValue.FindSingle(local, semanticModel, cancellationToken) is { } assignedValue)
            {
                return Find(assignedValue, semanticModel, cancellationToken);
            }

            return null;
        }
    }
}
