namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ReturnValueWalker : PooledWalker<ReturnValueWalker>
    {
        private readonly List<ExpressionSyntax> returnValues = new List<ExpressionSyntax>();

        private ReturnValueWalker()
        {
        }

        internal IReadOnlyList<ExpressionSyntax> ReturnValues => this.returnValues;

        public override void Visit(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.AnonymousMethodExpression:
                    return;
                default:
                    base.Visit(node);
                    break;
            }
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            this.returnValues.Add(node.Expression);
        }

        public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            this.returnValues.Add(node.Expression);
        }

        internal static ReturnValueWalker Borrow(SyntaxNode scope) => BorrowAndVisit(scope, () => new ReturnValueWalker());

        internal static bool TrySingle(SyntaxNode scope, [NotNullWhen(true)] out ExpressionSyntax? returnValue)
        {
            if (scope is null)
            {
                returnValue = null;
                return false;
            }

            using var walker = BorrowAndVisit(scope, () => new ReturnValueWalker());
            return walker.returnValues.TrySingle(out returnValue);
        }

        protected override void Clear()
        {
            this.returnValues.Clear();
        }
    }
}
