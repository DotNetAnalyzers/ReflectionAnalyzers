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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseTypeFix))]
    [Shared]
    internal class UseTypeFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL041CreateDelegateType.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeText) &&
                    syntaxRoot.TryFindNode(diagnostic, out TypeSyntax type))
                {
                    context.RegisterCodeFix(
                        $"Change to: {typeText}.",
                        (editor, _) => editor.ReplaceNode(
                            type,
                            x => SyntaxFactory.ParseTypeName(typeText)
                                              .WithSimplifiedNames()
                                              .WithTriviaFrom(x)),
                        nameof(UseTypeFix),
                        diagnostic);
                }
            }
        }
    }
}
