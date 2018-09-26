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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CastReturnValueFix))]
    [Shared]
    internal class CastReturnValueFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL028CastReturnValueToCorrectType.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out TypeSyntax typeSyntax) &&
                    diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeString))
                {
                    context.RegisterCodeFix(
                        $"Cast to {typeString}.",
                        (editor, _) => editor.ReplaceNode(
                            typeSyntax,
                            x => SyntaxFactory.ParseTypeName(typeString)),
                        nameof(CastReturnValueFix),
                        diagnostic);
                }
            }
        }
    }
}
