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
        internal static bool? TryFindFirstMisMatch(ImmutableArray<IParameterSymbol> parameters, ImmutableArray<ExpressionSyntax> values, SyntaxNodeAnalysisContext context, out ExpressionSyntax expression)
        {
            if (parameters.Length == 0 &&
                values.Length > 0)
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

            IParameterSymbol lastParameter = null;
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
                    context.SemanticModel.TryGetSymbol(values[i], context.CancellationToken, out IFieldSymbol field) &&
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

                var conversion = context.SemanticModel.ClassifyConversion(values[i], lastParameter.Type);
                if (!conversion.Exists)
                {
                    if (lastParameter.Type is IArrayTypeSymbol arrayType &&
                        context.SemanticModel.ClassifyConversion(values[i], arrayType.ElementType).IsIdentity)
                    {
                        continue;
                    }

                    return true;
                }

                if (conversion.IsIdentity || conversion.IsImplicit)
                {
                    if (values[i] is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.Identifier.ValueText == "Value" &&
                        context.SemanticModel.TryGetSymbol(values[i], context.CancellationToken, out field) &&
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

            bool IsNull(ExpressionSyntax candidate)
            {
                switch (candidate)
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
