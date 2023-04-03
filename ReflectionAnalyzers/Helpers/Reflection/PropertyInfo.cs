namespace ReflectionAnalyzers;

using System.Threading;
using Microsoft.CodeAnalysis;
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
        if (GetProperty.Match(expression, semanticModel, cancellationToken) is { Member: { ReflectedType: { } reflectedType, Symbol: IPropertySymbol symbol } })
        {
            return new PropertyInfo(reflectedType, symbol);
        }

        return null;
    }
}
