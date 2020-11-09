namespace ReflectionAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class AssignedValue
    {
        internal static ExpressionSyntax? FindSingle(ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return symbol switch
            {
                ILocalSymbol local => FindSingle(local, semanticModel, cancellationToken),
                IFieldSymbol field => FindSingle(field, semanticModel, cancellationToken),
                IPropertySymbol field => FindSingle(field, semanticModel, cancellationToken),
                _ => null,
            };
        }

        internal static ExpressionSyntax? FindSingle(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (field.IsReadOnly &&
                field.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax? declaration) &&
                declaration.Variables.TrySingle(out var variable))
            {
                using var walker = MutationWalker.For(field, semanticModel, cancellationToken);
                if (walker.IsEmpty)
                {
                    return variable.Initializer?.Value;
                }

                if (variable.Initializer is null &&
                    walker.TrySingle(out var node) &&
                    node.Parent is AssignmentExpressionSyntax assignment)
                {
                    return assignment.Right;
                }

                return null;
            }

            return null;
        }

        internal static ExpressionSyntax? FindSingle(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (property.IsGetOnly() &&
                property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? declaration))
            {
                using var walker = MutationWalker.For(property, semanticModel, cancellationToken);
                if (walker.IsEmpty)
                {
                    return declaration.Initializer?.Value;
                }

                if (declaration.Initializer is null &&
                    walker.TrySingle(out var node) &&
                    node.Parent is AssignmentExpressionSyntax assignment)
                {
                    return assignment.Right;
                }

                return null;
            }

            return null;
        }

        internal static ExpressionSyntax? FindSingle(ILocalSymbol local, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (local.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax? declaration) &&
                declaration.Variables.TrySingle(out var variable))
            {
                using var walker = MutationWalker.For(local, semanticModel, cancellationToken);
                if (walker.IsEmpty)
                {
                    return variable.Initializer?.Value;
                }

                if (variable.Initializer is null &&
                   walker.TrySingle(out var node) &&
                   node.Parent is AssignmentExpressionSyntax assignment)
                {
                    return assignment.Right;
                }

                return null;
            }

            return null;
        }
    }
}
