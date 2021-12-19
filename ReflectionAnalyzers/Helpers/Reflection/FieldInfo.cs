namespace ReflectionAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct FieldInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IFieldSymbol Field;

        internal FieldInfo(INamedTypeSymbol reflectedType, IFieldSymbol field)
        {
            this.ReflectedType = reflectedType;
            this.Field = field;
        }

        internal static FieldInfo? Find(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (GetField.Match(expression, semanticModel, cancellationToken) is { Member: { ReflectedType: { } reflectedType, Symbol: IFieldSymbol symbol } })
            {
                return new FieldInfo(reflectedType, symbol);
            }

            return null;
        }
    }
}
