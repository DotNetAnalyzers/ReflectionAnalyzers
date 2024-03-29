﻿namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.CodeFixExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowOnErrorFix))]
[Shared]
internal class ThrowOnErrorFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.REFL036CheckNull.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? invocation) &&
                invocation.ArgumentList is { Arguments: { } arguments } argumentList)
            {
                if (arguments.Count == 1)
                {
                    context.RegisterCodeFix(
                        "Throw on error.",
                        (e, cancellationToken) => e.ReplaceNode(
                            argumentList,
                            x => x.AddArguments(SyntaxFactory.Argument(SyntaxFactory.NameColon("throwOnError"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)))),
                        "Throw on error.",
                        diagnostic);
                }
                else if (arguments.Count == 2 &&
                         arguments.TrySingle(x => x.Expression.IsKind(SyntaxKind.FalseLiteralExpression), out var argument))
                {
                    context.RegisterCodeFix(
                        "Throw on error.",
                        (e, cancellationToken) => e.ReplaceNode(
                            argument.Expression,
                            x => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression).WithTriviaFrom(x)),
                        "Throw on error.",
                        diagnostic);
                }
            }
        }
    }
}
