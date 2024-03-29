﻿namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PreferNullFix))]
[Shared]
internal class PreferNullFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.REFL024PreferNullOverEmptyArray.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is ArgumentSyntax argument)
            {
                context.RegisterCodeFix(
                    "Prefer null.",
                    (editor, _) => editor.ReplaceNode(
                        argument.Expression,
                        x => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                    nameof(PreferNullFix),
                    diagnostic);
            }
        }
    }
}
