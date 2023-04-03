namespace ReflectionAnalyzers;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class ReturnValueWalker : PooledWalker<ReturnValueWalker>
{
    private readonly List<ExpressionSyntax> returnValues = new();

    private ReturnValueWalker()
    {
    }

    public override void Visit(SyntaxNode? node)
    {
        if (node is null)
        {
            return;
        }

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
        if (node.Expression is { })
        {
            this.returnValues.Add(node.Expression);
        }
    }

    public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
    {
        this.returnValues.Add(node.Expression);
    }

    internal static bool TrySingle(SyntaxNode scope, [NotNullWhen(true)] out ExpressionSyntax? returnValue)
    {
        using var walker = BorrowAndVisit(scope, () => new ReturnValueWalker());
        return walker.returnValues.TrySingle(out returnValue);
    }

    protected override void Clear()
    {
        this.returnValues.Clear();
    }
}
