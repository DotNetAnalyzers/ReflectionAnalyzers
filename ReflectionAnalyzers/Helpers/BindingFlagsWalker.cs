namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class BindingFlagsWalker : PooledWalker<BindingFlagsWalker>
    {
        private readonly List<string> flags = new List<string>();
        private bool isUnHandled;

        private static BindingFlagsWalker Borrow(BinaryExpressionSyntax flags) => BorrowAndVisit(flags, () => new BindingFlagsWalker());

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

        internal static bool HasWrongOrder(BinaryExpressionSyntax flags, out string inExpectedOrder)
        {
            inExpectedOrder = null;
            if (flags == null)
            {
                return false;
            }

            using (var walker = Borrow(flags))
            {
                var current = 0;
                foreach (var flag in walker.flags)
                {
                    var index = Index(flag);
                    if (index == -1)
                    {
                        return false;
                    }

                    current = index < current ? int.MaxValue : index;
                }

                if (current == int.MaxValue)
                {
                    walker.flags.Sort((x, y) => Index(x)
                                      .CompareTo(Index(y)));
                    var builder = StringBuilderPool.Borrow();
                    for (var i = 0; i < walker.flags.Count; i++)
                    {
                        if (i > 0)
                        {
                            _ = builder.Append(" | ");
                        }

                        _ = builder.Append("BindingFlags.")
                                   .Append(walker.flags[i]);
                    }

                    inExpectedOrder = builder.Return();
                    return true;
                }

                return false;
            }
        }

        protected override void Clear()
        {
            this.flags.Clear();
            this.isUnHandled = false;
        }

        private static int Index(string text)
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
                    // We don't support stuff like BindingFlags.GetProperty
                    return -1;
            }
        }
    }
}
