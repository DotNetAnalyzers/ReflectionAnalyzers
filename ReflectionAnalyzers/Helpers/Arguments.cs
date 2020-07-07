namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Arguments
    {
        internal static bool? TryFindFirstMisMatch(ImmutableArray<IParameterSymbol> parameters, ImmutableArray<ExpressionSyntax> values, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax? expression)
        {
            if (parameters.IsEmpty &&
                !values.IsEmpty)
            {
                expression = null;
                return true;
            }

            if (parameters.TryFirst(x => x.RefKind == RefKind.Ref || x.RefKind == RefKind.Out, out _))
            {
                expression = null;
                return null;
            }

            if (values.Length != parameters.Length)
            {
                if (parameters.TryLast(out var last) &&
                    !last.IsParams)
                {
                    expression = null;
                    return true;
                }

                if (values.Length < parameters.Length - 1)
                {
                    expression = null;
                    return true;
                }
            }

            IParameterSymbol? lastParameter = null;
            for (var i = 0; i < values.Length; i++)
            {
                expression = values[i];
                if (lastParameter == null ||
                    !lastParameter.IsParams)
                {
                    if (!parameters.TryElementAt(i, out lastParameter))
                    {
                        return true;
                    }
                }

                if (lastParameter.IsOptional &&
                    semanticModel.TryGetSymbol(values[i], cancellationToken, out IFieldSymbol? field) &&
                    field == KnownSymbol.Missing.Value)
                {
                    continue;
                }

                if (IsNull(values[i]))
                {
                    if (lastParameter.IsParams)
                    {
                        return true;
                    }

                    continue;
                }

                var conversion = semanticModel.ClassifyConversion(values[i], lastParameter.Type);
                if (!conversion.Exists)
                {
                    if (lastParameter.Type is IArrayTypeSymbol arrayType &&
                        semanticModel.ClassifyConversion(values[i], arrayType.ElementType).IsIdentity)
                    {
                        continue;
                    }

                    return true;
                }

                if (conversion.IsIdentity || conversion.IsImplicit)
                {
                    if (values[i] is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.Identifier.ValueText == "Value" &&
                        semanticModel.TryGetSymbol(values[i], cancellationToken, out field) &&
                        field == KnownSymbol.Missing.Value)
                    {
                        return true;
                    }

                    continue;
                }

                if (conversion.IsExplicit &&
                    conversion.IsReference)
                {
                    continue;
                }

                return true;
            }

            expression = null;
            return false;

            static bool IsNull(ExpressionSyntax candidate)
            {
                return candidate switch
                {
                    LiteralExpressionSyntax literal => literal.IsKind(SyntaxKind.NullLiteralExpression),
                    CastExpressionSyntax cast => IsNull(cast.Expression),
                    BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AsExpression) => IsNull(binary.Left),
                    _ => false,
                };
            }
        }
    }
}
