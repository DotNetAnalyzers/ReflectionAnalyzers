namespace ReflectionAnalyzers
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
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL016UseNameof.Id,
            Descriptors.REFL017NameofWrongMember.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is ArgumentSyntax argument)
                {
                    if (diagnostic.Properties.TryGetValue(nameof(NameSyntax), out var nameText))
                    {
                        var title = argument.Expression.IsKind(SyntaxKind.StringLiteralExpression)
                            ? $"Use {nameText}."
                            : $"Use nameof({nameText}).";
                        context.RegisterCodeFix(
                            title,
                            (editor, cancellationToken) => editor.ReplaceNode(
                                argument.Expression,
                                SyntaxFactory.ParseExpression(nameText)
                                             .WithTriviaFrom(argument.Expression)),
                            nameof(NameofFix),
                            diagnostic);
                    }
                    else if (diagnostic.Properties.TryGetValue(nameof(ExpressionSyntax), out var expressionText))
                    {
                        context.RegisterCodeFix(
                            $"Use {expressionText}.",
                            (editor, cancellationToken) => editor.ReplaceNode(
                                argument.Expression,
                                SyntaxFactory.ParseExpression(expressionText)
                                             .WithTriviaFrom(argument.Expression)),
                            nameof(NameofFix),
                            diagnostic);
                    }
                }
            }
        }
    }
}
