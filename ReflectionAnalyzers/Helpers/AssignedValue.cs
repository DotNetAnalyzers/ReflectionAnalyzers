namespace ReflectionAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class AssignedValue
    {
        internal static bool TryGetSingle(ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax expression)
        {
            switch (symbol)
            {
                case ILocalSymbol local:
                    return TryGetSingle(local, semanticModel, cancellationToken, out expression);
                case IFieldSymbol field:
                    return TryGetSingle(field, semanticModel, cancellationToken, out expression);
                case IPropertySymbol field:
                    return TryGetSingle(field, semanticModel, cancellationToken, out expression);
            }

            expression = null;
            return false;
        }

        internal static bool TryGetSingle(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax expression)
        {
            expression = null;
            if (field.IsReadOnly &&
                field.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax declaration) &&
                declaration.Variables.TrySingle(out var variable))
            {
                using (var walker = MutationWalker.For(field, semanticModel, cancellationToken))
                {
                    if (walker.IsEmpty)
                    {
                        expression = variable.Initializer?.Value;
                        return expression != null;
                    }

                    if (variable.Initializer == null &&
                        walker.TrySingle(out var node) &&
                        node.Parent is AssignmentExpressionSyntax assignment)
                    {
                        expression = assignment.Right;
                        return expression != null;
                    }

                    return false;
                }
            }

            return false;
        }

        internal static bool TryGetSingle(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax expression)
        {
            expression = null;
            if (property.IsGetOnly() &&
                property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax declaration))
            {
                using (var walker = MutationWalker.For(property, semanticModel, cancellationToken))
                {
                    if (walker.IsEmpty)
                    {
                        expression = declaration.Initializer?.Value;
                        return expression != null;
                    }

                    if (declaration.Initializer == null &&
                        walker.TrySingle(out var node) &&
                        node.Parent is AssignmentExpressionSyntax assignment)
                    {
                        expression = assignment.Right;
                        return expression != null;
                    }

                    return false;
                }
            }

            return false;
        }

        internal static bool TryGetSingle(ILocalSymbol local, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax expression)
        {
            expression = null;
            if (local.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax declaration) &&
                declaration.Variables.TrySingle(out var variable))
            {
                using (var walker = MutationWalker.For(local, semanticModel, cancellationToken))
                {
                    if (walker.IsEmpty)
                    {
                        expression = variable.Initializer?.Value;
                        return expression != null;
                    }

                    if (variable.Initializer == null &&
                       walker.TrySingle(out var node) &&
                       node.Parent is AssignmentExpressionSyntax assignment)
                    {
                        expression = assignment.Right;
                        return expression != null;
                    }

                    return false;
                }
            }

            return false;
        }
    }
}
