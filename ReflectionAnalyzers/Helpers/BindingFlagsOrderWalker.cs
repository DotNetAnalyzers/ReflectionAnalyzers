namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class BindingFlagsOrderWalker : PooledWalker<BindingFlagsOrderWalker>
    {
        private readonly List<string> flags = new List<string>();
        private bool isUnHandled;

        public static BindingFlagsOrderWalker Borrow(ArgumentSyntax argument) => BorrowAndVisit(argument.Expression, () => new BindingFlagsOrderWalker());

        public override void Visit(SyntaxNode node)
        {
            if (!this.isUnHandled)
            {
                switch (node)
                {
                    case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.BitwiseOrExpression):
                    case MemberAccessExpressionSyntax _:
                        base.Visit(node);
                        break;
                    case IdentifierNameSyntax identifierName:
                        if (identifierName.Parent is MemberAccessExpressionSyntax memberAccess &&
                            memberAccess.Name == identifierName)
                        {
                            this.flags.Add(identifierName.Identifier.ValueText);
                        }

                        break;
                    default:
                        this.isUnHandled = true;
                        break;
                }
            }
        }

        internal bool IsInWrongOrder()
        {
            if (this.isUnHandled)
            {
                return false;
            }

            var current = 0;
            foreach (var flag in this.flags)
            {
                var index = Index(flag);
                if (index == -1)
                {
                    return false;
                }

                current = index < current ? int.MaxValue : index;
            }

            return current == int.MaxValue;

            int Index(string text)
            {
                switch (text)
                {
                    case nameof(BindingFlags.Public):
                        return 0;
                    case nameof(BindingFlags.NonPublic):
                        return 1;
                    case nameof(BindingFlags.Static):
                        return 2;
                    case nameof(BindingFlags.Instance):
                        return 3;
                    case nameof(BindingFlags.DeclaredOnly):
                        return 4;
                    case nameof(BindingFlags.FlattenHierarchy):
                        return 5;
                    case nameof(BindingFlags.IgnoreCase):
                        return 6;
                    default:
                        return -1;
                }
            }
        }

        protected override void Clear()
        {
            this.flags.Clear();
            this.isUnHandled = false;
        }
    }
}
