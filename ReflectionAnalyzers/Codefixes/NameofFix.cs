namespace ReflectionAnalyzers.Codefixes
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NameofFix))]
    [Shared]
    internal class NameofFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL016UseNameof.DiagnosticId,
            REFL017DontUseNameof.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentSyntax argument))
                {
                    if (diagnostic.Properties.TryGetValue(nameof(NameSyntax), out var expressionString))
                    {
                        var title = argument.Expression.IsKind(SyntaxKind.StringLiteralExpression)
                            ? $"Use {expressionString}."
                            : $"Use nameof({expressionString}).";
                        context.RegisterCodeFix(
                            title,
                            (editor, cancellationToken) => editor.ReplaceNode(
                                argument.Expression,
                                SyntaxFactory.ParseExpression(expressionString)
                                             .WithTriviaFrom(argument.Expression)),
                            nameof(NameofFix),
                            diagnostic);
                    }
                    else if (diagnostic.Properties.TryGetValue(nameof(SyntaxKind.StringLiteralExpression), out var literalString))
                    {
                        context.RegisterCodeFix(
                            $"Use string literal \"{literalString}\"",
                            (editor, cancellationToken) => editor.ReplaceNode(
                                argument.Expression,
                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(literalString))
                                             .WithTriviaFrom(argument.Expression)),
                            nameof(NameofFix),
                            diagnostic);
                    }
                }
            }
        }
    }
}
