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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseParameterTypeFix))]
    [Shared]
    internal class UseParameterTypeFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL033UseSameTypeAsParameter.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeText))
                {
                    if (syntaxRoot.TryFindNode(diagnostic, out TypeSyntax? typeSyntax) &&
                        typeSyntax.Parent is TypeOfExpressionSyntax)
                    {
                        context.RegisterCodeFix(
                            $"Change to: {typeText}.",
                            (editor, _) => editor.ReplaceNode(
                                typeSyntax,
                                x => SyntaxFactory.ParseTypeName(typeText)
                                                  .WithTriviaFrom(x)),
                            nameof(UseParameterTypeFix),
                            diagnostic);
                    }
                    else if (syntaxRoot.TryFindNode(diagnostic, out ExpressionSyntax? expression))
                    {
                        context.RegisterCodeFix(
                            $"Change to: typeof({typeText}).",
                            (editor, _) => editor.ReplaceNode(
                                expression,
                                x => SyntaxFactory.ParseExpression($"typeof({typeText})")
                                                  .WithTriviaFrom(x)),
                            nameof(UseParameterTypeFix),
                            diagnostic);
                    }
                }
            }
        }
    }
}
