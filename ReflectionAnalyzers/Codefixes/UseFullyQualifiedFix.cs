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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseFullyQualifiedFix))]
    [Shared]
    internal class UseFullyQualifiedFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL022UseFullyQualifiedName.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: false, getInnermostNodeForTie: true) is LiteralExpressionSyntax literal &&
                    diagnostic.Properties.TryGetValue(nameof(SyntaxKind.StringLiteralExpression), out var text))
                {
                    context.RegisterCodeFix(
                        $"Use fully qualified name: {text}.",
                        (editor, _) => editor.ReplaceNode(
                            literal,
                            x => x.WithToken(SyntaxFactory.Literal(text))),
                        nameof(UseFullyQualifiedFix),
                        diagnostic);
                }

                if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: false, getInnermostNodeForTie: true) is IdentifierNameSyntax identifierName &&
                    diagnostic.Properties.TryGetValue(nameof(SimpleNameSyntax), out var name))
                {
                    context.RegisterCodeFix(
                        $"Use fully qualified name: {name}.",
                        (editor, _) => editor.ReplaceNode(
                            identifierName,
                            x => x.WithIdentifier(SyntaxFactory.Identifier(name))),
                        nameof(UseFullyQualifiedFix),
                        diagnostic);
                }
            }
        }
    }
}
