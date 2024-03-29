﻿namespace ReflectionAnalyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class BindingFlagsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.REFL007BindingFlagsOrder,
        Descriptors.REFL011DuplicateBindingFlags);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Handle, SyntaxKind.BitwiseOrExpression);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            TryGetFlags(context, out var flags))
        {
            if (BindingFlagsWalker.HasWrongOrder(flags, out var expectedFlags))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.REFL007BindingFlagsOrder,
                        flags.GetLocation(),
                        ImmutableDictionary<string, string?>.Empty.Add(nameof(ArgumentSyntax), expectedFlags),
                        $" Expected: {expectedFlags}."));
            }

            if (BindingFlagsWalker.HasDuplicate(flags, out var dupe, out expectedFlags))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.REFL011DuplicateBindingFlags,
                        dupe.GetLocation(),
                        ImmutableDictionary<string, string?>.Empty.Add(nameof(ArgumentSyntax), expectedFlags),
                        $" Expected: {expectedFlags}."));
            }
        }
    }

    private static bool TryGetFlags(SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out BinaryExpressionSyntax? flags)
    {
        flags = context.Node as BinaryExpressionSyntax;
        return flags?.Parent is ArgumentSyntax &&
               context.SemanticModel.TryGetType(flags, context.CancellationToken, out var type) &&
               type == KnownSymbol.BindingFlags;
    }

    private class BindingFlagsWalker : PooledWalker<BindingFlagsWalker>
    {
        private readonly List<IdentifierNameSyntax> flags = new();
        private IdentifierNameSyntax duplicate = null!;
        private bool isUnHandled;

        public override void Visit(SyntaxNode? node)
        {
            if (node is null)
            {
                return;
            }

            if (!this.isUnHandled)
            {
                switch (node)
                {
                    case BinaryExpressionSyntax binary
                        when binary.IsKind(SyntaxKind.BitwiseOrExpression):
                    case MemberAccessExpressionSyntax _:
                        base.Visit(node);
                        break;
                    case IdentifierNameSyntax identifierName
                        when (identifierName.Parent is MemberAccessExpressionSyntax { Parent: not MemberAccessExpressionSyntax } memberAccess && memberAccess.Name == identifierName) ||
                             identifierName.Parent is BinaryExpressionSyntax:
                        if (this.duplicate is null &&
                            this.flags.TryFirst(x => x.Identifier.ValueText == identifierName.Identifier.ValueText, out _))
                        {
                            this.duplicate = identifierName;
                        }

                        this.flags.Add(identifierName);

                        break;
                    case IdentifierNameSyntax _:
                        break;
                    default:
                        this.isUnHandled = true;
                        break;
                }
            }
        }

        internal static bool HasWrongOrder(BinaryExpressionSyntax flags, [NotNullWhen(true)] out string? inExpectedOrder)
        {
            inExpectedOrder = null;
            if (flags is null)
            {
                return false;
            }

            using var walker = Borrow(flags);
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
                inExpectedOrder = Format(walker.flags);
                return true;
            }

            return false;

            static int Index(IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.ValueText switch
                {
                    nameof(BindingFlags.Public) => 0,
                    nameof(BindingFlags.NonPublic) => 1,
                    nameof(BindingFlags.Static) => 2,
                    nameof(BindingFlags.Instance) => 3,
                    nameof(BindingFlags.DeclaredOnly) => 4,
                    nameof(BindingFlags.FlattenHierarchy) => 5,
                    nameof(BindingFlags.IgnoreCase) => 6,
                    _ => -1,
                };
            }
        }

        internal static bool HasDuplicate(BinaryExpressionSyntax flags, [NotNullWhen(true)] out IdentifierNameSyntax? dupe, [NotNullWhen(true)] out string? expectedFlags)
        {
            expectedFlags = null;
            dupe = null;
            if (flags is null)
            {
                return false;
            }

            using var walker = Borrow(flags);
            if (walker is { isUnHandled: false, duplicate: { } })
            {
                dupe = walker.duplicate;
                walker.flags.RemoveAt(walker.flags.LastIndexOf(dupe));
                expectedFlags = Format(walker.flags);
                return true;
            }

            return false;
        }

        protected override void Clear()
        {
            this.flags.Clear();
            this.duplicate = null!;
            this.isUnHandled = false;
        }

        private static BindingFlagsWalker Borrow(BinaryExpressionSyntax flags) => BorrowAndVisit(flags, () => new BindingFlagsWalker());

        private static string Format(IReadOnlyList<IdentifierNameSyntax> flags)
        {
            var builder = StringBuilderPool.Borrow();
            for (var i = 0; i < flags.Count; i++)
            {
                if (i > 0)
                {
                    _ = builder.Append(" | ");
                }

                ExpressionSyntax flag = flags[i];
                while (flag.Parent is MemberAccessExpressionSyntax memberAccess)
                {
                    flag = memberAccess;
                }

                _ = builder.Append(flag);
            }

            return builder.Return();
        }
    }
}
