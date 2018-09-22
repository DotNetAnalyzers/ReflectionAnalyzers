namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class AssignedValueWalker : PooledWalker<AssignedValueWalker>
    {
        private readonly List<ExpressionSyntax> values = new List<ExpressionSyntax>();
        private bool unknown;
        private ILocalSymbol local;
        private SemanticModel semanticModel;
        private CancellationToken cancellationToken;

        private AssignedValueWalker(ILocalSymbol local, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            this.local = local;
            this.semanticModel = semanticModel;
            this.cancellationToken = cancellationToken;
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (this.IsLocal(node.Left) &&
                node.Right is ExpressionSyntax value)
            {
                this.values.Add(value);
            }

            base.VisitAssignmentExpression(node);
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            if (this.IsLocal(node.Operand))
            {
                this.unknown = true;
            }

            base.VisitPostfixUnaryExpression(node);
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            if (this.IsLocal(node.Operand))
            {
                this.unknown = true;
            }

            base.VisitPrefixUnaryExpression(node);
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            if (node.RefOrOutKeyword.IsEither(SyntaxKind.RefKeyword, SyntaxKind.OutKeyword) &&
                this.IsLocal(node.Expression))
            {
                this.unknown = true;
            }

            base.VisitArgument(node);
        }

        internal static bool TryGetSingle(ILocalSymbol local, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax expression)
        {
            expression = null;
            if (local.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax declaration) &&
                declaration.Variables.TrySingle(out var variable))
            {
                var scope = (SyntaxNode)declaration.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>() ??
                                        declaration.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                using (var walker = new AssignedValueWalker(local, semanticModel, cancellationToken))
                {
                    if (variable.Initializer is EqualsValueClauseSyntax initializer &&
                        initializer.Value != null &&
                        !initializer.Value.IsKind(SyntaxKind.NullLiteralExpression))
                    {
                        walker.values.Add(initializer.Value);
                    }

                    walker.Visit(scope);
                    if (walker.unknown)
                    {
                        return false;
                    }

                    return walker.values.TrySingle(out expression);
                }
            }

            return false;
        }

        protected override void Clear()
        {
            this.values.Clear();
            this.unknown = false;
            this.local = null;
            this.semanticModel = null;
            this.cancellationToken = CancellationToken.None;
        }

        private bool IsLocal(ExpressionSyntax expression)
        {
            return expression is IdentifierNameSyntax identifierName &&
                   identifierName.Identifier.ValueText == this.local.Name &&
                   this.semanticModel.TryGetSymbol(identifierName, this.cancellationToken, out ILocalSymbol candidate) &&
                   candidate.Equals(this.local);
        }
    }
}
