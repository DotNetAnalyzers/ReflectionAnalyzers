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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseIsInstanceOfTypeFix))]
[Shared]
internal class UseIsInstanceOfTypeFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.REFL040PreferIsInstanceOfType.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is InvocationExpressionSyntax { ArgumentList: { } argumentList } invocation &&
                argumentList.Arguments.TrySingle(out var arg) &&
                semanticModel is { } &&
                IsAssignableFromAnalyzer.IsInstanceGetType(arg.Expression, semanticModel, context.CancellationToken, out var instance))
            {
                context.RegisterCodeFix(
                    $"Change to: IsInstanceOfType().",
                    (editor, _) => editor.ReplaceNode(
                                             invocation.Expression,
                                             x =>
                                             {
                                                 var memberAccess = (MemberAccessExpressionSyntax)x;
                                                 return memberAccess.WithName(memberAccess.Name.WithIdentifier(SyntaxFactory.Identifier("IsInstanceOfType")));
                                             })
                                         .ReplaceNode(
                                             arg.Expression,
                                             x => instance.WithTriviaFrom(x)),
                    nameof(UseIsInstanceOfTypeFix),
                    diagnostic);
            }
        }
    }
}
