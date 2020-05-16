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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PreferEmptyTypesFix))]
    [Shared]
    internal class PreferEmptyTypesFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax System = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL027PreferEmptyTypes.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentSyntax? argument))
                {
                    context.RegisterCodeFix(
                        "Prefer Type.EmptyTypes.",
                        (editor, _) => editor.AddUsing(System)
                                             .ReplaceNode(
                            argument.Expression,
                            x => SyntaxFactory.ParseExpression("Type.EmptyTypes")),
                        nameof(PreferEmptyTypesFix),
                        diagnostic);
                }
            }
        }
    }
}
