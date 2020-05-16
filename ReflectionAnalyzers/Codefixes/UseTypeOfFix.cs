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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseTypeOfFix))]
    [Shared]
    internal class UseTypeOfFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL039PreferTypeof.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeText) &&
                    syntaxRoot.TryFindNode(diagnostic, out InvocationExpressionSyntax? invocation))
                {
                    context.RegisterCodeFix(
                        $"Change to: typeof({typeText}).",
                        (editor, _) => editor.ReplaceNode(
                            invocation,
                            x => SyntaxFactory.ParseExpression($"typeof({typeText})")
                                              .WithTriviaFrom(x)),
                        nameof(UseTypeOfFix),
                        diagnostic);
                }
            }
        }
    }
}
