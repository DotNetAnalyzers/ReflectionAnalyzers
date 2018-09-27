namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Arguments
    {
        internal static bool? IsMatch(ImmutableArray<IParameterSymbol> parameters, ImmutableArray<ExpressionSyntax> values, SyntaxNodeAnalysisContext context)
        {
            if (parameters.TryFirst(x => x.RefKind == RefKind.Ref || x.RefKind == RefKind.Out, out _))
            {
                return null;
            }

            if (values.Length != parameters.Length)
            {
                if (parameters.TryLast(out var last) &&
                    !last.IsParams)
                {
                    return false;
                }

                if (values.Length < parameters.Length - 1)
                {
                    return false;
                }
            }

            IParameterSymbol lastParameter = null;
            for (var i = 0; i < values.Length; i++)
            {
                if (lastParameter == null ||
                    !lastParameter.IsParams)
                {
                    if (!parameters.TryElementAt(i, out lastParameter))
                    {
                        return false;
                    }
                }

                if (IsNull(values[i]))
                {
                    if (lastParameter.IsParams)
                    {
                        return false;
                    }

                    continue;
                }

                var conversion = context.SemanticModel.ClassifyConversion(values[i], lastParameter.Type);
                if (!conversion.Exists)
                {
                    if (lastParameter.Type is IArrayTypeSymbol arrayType &&
                        context.SemanticModel.ClassifyConversion(values[i], arrayType.ElementType).IsIdentity)
                    {
                        continue;
                    }

                    return false;
                }

                if (conversion.IsIdentity || conversion.IsImplicit)
                {
                    continue;
                }

                if (conversion.IsExplicit &&
                    conversion.IsReference)
                {
                    continue;
                }

                return false;
            }

            return true;

            bool IsNull(ExpressionSyntax expression)
            {
                switch (expression)
                {
                    case LiteralExpressionSyntax literal:
                        return literal.IsKind(SyntaxKind.NullLiteralExpression);
                    case CastExpressionSyntax cast:
                        return IsNull(cast.Expression);
                    case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AsExpression):
                        return IsNull(binary.Left);
                    default:
                        return false;
                }
            }
        }
    }
}
