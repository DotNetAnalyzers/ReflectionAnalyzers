namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Array
    {
        internal static bool IsCreatingEmpty(ExpressionSyntax creation, SyntaxNodeAnalysisContext context)
        {
            switch (creation)
            {
                case InvocationExpressionSyntax invocation when invocation.TryGetTarget(KnownSymbol.Array.Empty, context.SemanticModel, context.CancellationToken, out _):
                    return true;
                case ArrayCreationExpressionSyntax arrayCreation:
                    if (arrayCreation.Type is ArrayTypeSyntax arrayType)
                    {
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
                        return initializer == null ||
                               initializer.Expressions.Count == 0;
                    }

                    return false;
                default:
                    return false;
            }
        }

        internal static bool TryGetTypes(ExpressionSyntax creation, SyntaxNodeAnalysisContext context, out IReadOnlyList<ITypeSymbol> types)
        {
            types = null;
            if (IsCreatingEmpty(creation, context))
            {
                types = System.Array.Empty<ITypeSymbol>();
                return true;
            }

            switch (creation)
            {
                case ImplicitArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer is InitializerExpressionSyntax initializer:
                    return TryGetTypesFromInitializer(initializer, out types);
                case ArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer is InitializerExpressionSyntax initializer:
                    return TryGetTypesFromInitializer(initializer, out types);
                case MemberAccessExpressionSyntax memberAccess when context.SemanticModel.TryGetSymbol(memberAccess, context.CancellationToken, out ISymbol symbol) &&
                                                                    symbol == KnownSymbol.Type.EmptyTypes:
                    types = System.Array.Empty<ITypeSymbol>();
                    return true;
            }

            return false;

            bool TryGetTypesFromInitializer(InitializerExpressionSyntax initializer, out IReadOnlyList<ITypeSymbol> result)
            {
                var temp = new ITypeSymbol[initializer.Expressions.Count];
                for (var i = 0; i < initializer.Expressions.Count; i++)
                {
                    var expression = initializer.Expressions[i];
                    if (expression is TypeOfExpressionSyntax typeOf &&
                        context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var type))
                    {
                        temp[i] = type;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }

                result = temp;
                return true;
            }
        }

        internal static bool TryGetValues(ExpressionSyntax creation, SyntaxNodeAnalysisContext context, out ImmutableArray<ExpressionSyntax> values)
        {
            values = default(ImmutableArray<ExpressionSyntax>);
            if (IsCreatingEmpty(creation, context))
            {
                values = ImmutableArray<ExpressionSyntax>.Empty;
                return true;
            }

            switch (creation)
            {
                case ImplicitArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer is InitializerExpressionSyntax initializer:
                    values = ImmutableArray.CreateRange(initializer.Expressions);
                    return true;
                case ArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer is InitializerExpressionSyntax initializer:
                    values = ImmutableArray.CreateRange(initializer.Expressions);
                    return true;
                case MemberAccessExpressionSyntax memberAccess when context.SemanticModel.TryGetSymbol(memberAccess, context.CancellationToken, out ISymbol symbol) &&
                                                                    symbol == KnownSymbol.Type.EmptyTypes:
                    values = ImmutableArray<ExpressionSyntax>.Empty;
                    return true;
            }

            return false;
        }
    }
}
