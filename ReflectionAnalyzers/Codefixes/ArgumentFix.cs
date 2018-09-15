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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArgumentFix))]
    [Shared]
    internal class ArgumentFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL005WrongBindingFlags.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentSyntax argument) &&
                    diagnostic.Properties.TryGetValue(nameof(ExpressionSyntax), out var expression))
                {
                    context.RegisterCodeFix(
                        $"Change to: {expression}.",
                        (editor, _) => editor.ReplaceNode(argument.Expression, SyntaxFactory.ParseExpression(expression)),
                        this.GetType().FullName,
                        diagnostic);
                }
            }
        }
    }
}
