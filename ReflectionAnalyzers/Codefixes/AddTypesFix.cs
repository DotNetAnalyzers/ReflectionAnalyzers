namespace ReflectionAnalyzers.Codefixes
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddTypesFix))]
    [Shared]
    internal class AddTypesFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax System = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL029MissingTypes.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentListSyntax argumentListSyntax) &&
                    diagnostic.Properties.TryGetValue(nameof(ArgumentListSyntax), out var argumentListString))
                {
                    context.RegisterCodeFix(
                        "Specify types.",
                        Apply,
                        nameof(AddTypesFix),
                        diagnostic);

                    void Apply(DocumentEditor editor, CancellationToken __)
                    {
                        if (argumentListString.Contains("Type.EmptyTypes"))
                        {
                            _ = editor.AddUsing(System);
                        }

                        _ = editor.ReplaceNode(argumentListSyntax, x => SyntaxFactory.ParseArgumentList(argumentListString));
                    }
                }
            }
        }
    }
}
