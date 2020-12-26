namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Array
    {
        internal static bool IsCreatingEmpty(ExpressionSyntax creation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (creation)
            {
                case InvocationExpressionSyntax invocation
                    when invocation.TryGetTarget(KnownSymbol.Array.Empty, semanticModel, cancellationToken, out _):
                    return true;
                case ArrayCreationExpressionSyntax { Type: { } arrayType } arrayCreation:
                    foreach (var rankSpecifier in arrayType.RankSpecifiers)
                    {
                        foreach (var size in rankSpecifier.Sizes)
                        {
                            if (size is LiteralExpressionSyntax literal &&
                                literal.Token.ValueText != "0")
                            {
                                return false;
                            }
                        }
                    }

                    var initializer = arrayCreation.Initializer;
                    return initializer is null ||
                           initializer.Expressions.Count == 0;
                default:
                    return false;
            }
        }

        internal static bool TryGetAccessibleTypes(ImmutableArray<ExpressionSyntax> values, SemanticModel semanticModel, CancellationToken cancellationToken, out ImmutableArray<ITypeSymbol> types)
        {
            var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(values.Length);
            foreach (var value in values)
            {
                if (semanticModel.TryGetType(value, cancellationToken, out var type) &&
                    semanticModel.IsAccessible(value.SpanStart, type))
                {
                    builder.Add(type);
                }
                else
                {
                    types = ImmutableArray<ITypeSymbol>.Empty;
                    return false;
                }
            }

            types = builder.ToImmutable();
            return true;
        }

        internal static bool TryGetTypes(ExpressionSyntax creation, SemanticModel semanticModel, CancellationToken cancellationToken, out ImmutableArray<ITypeSymbol> types)
        {
            if (IsCreatingEmpty(creation, semanticModel, cancellationToken))
            {
                types = ImmutableArray<ITypeSymbol>.Empty;
                return true;
            }

            switch (creation)
            {
                case ImplicitArrayCreationExpressionSyntax { Initializer: { } initializer }:
                    return TryGetTypesFromInitializer(initializer, out types);
                case ArrayCreationExpressionSyntax { Initializer: { } initializer }:
                    return TryGetTypesFromInitializer(initializer, out types);
                case MemberAccessExpressionSyntax memberAccess
                    when semanticModel.TryGetSymbol(memberAccess, cancellationToken, out var symbol) &&
                         symbol == KnownSymbol.Type.EmptyTypes:
                    types = ImmutableArray<ITypeSymbol>.Empty;
                    return true;
            }

            types = default;
            return false;

            bool TryGetTypesFromInitializer(InitializerExpressionSyntax initializer, out ImmutableArray<ITypeSymbol> result)
            {
                var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(initializer.Expressions.Count);
                for (var i = 0; i < initializer.Expressions.Count; i++)
                {
                    if (Type.TryGet(initializer.Expressions[i], semanticModel, cancellationToken, out var type, out _))
                    {
                        builder.Add(type);
                    }
                    else
                    {
                        result = ImmutableArray<ITypeSymbol>.Empty;
                        return false;
                    }
                }

                result = builder.ToImmutable();
                return true;
            }
        }

        internal static bool TryGetValues(ExpressionSyntax creation, SemanticModel semanticModel, CancellationToken cancellationToken, out ImmutableArray<ExpressionSyntax> values)
        {
            values = default;
            if (IsCreatingEmpty(creation, semanticModel, cancellationToken))
            {
                values = ImmutableArray<ExpressionSyntax>.Empty;
                return true;
            }

            switch (creation)
            {
                case ImplicitArrayCreationExpressionSyntax { Initializer: { } initializer }:
                    values = ImmutableArray.CreateRange(initializer.Expressions);
                    return true;
                case ArrayCreationExpressionSyntax { Initializer: { } initializer }:
                    values = ImmutableArray.CreateRange(initializer.Expressions);
                    return true;
                case MemberAccessExpressionSyntax memberAccess
                    when semanticModel.TryGetSymbol(memberAccess, cancellationToken, out var symbol) &&
                         symbol == KnownSymbol.Type.EmptyTypes:
                    values = ImmutableArray<ExpressionSyntax>.Empty;
                    return true;
            }

            return false;
        }
    }
}
