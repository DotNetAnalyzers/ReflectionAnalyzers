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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseParameterTypeFix))]
    [Shared]
    internal class UseParameterTypeFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL033UseSameTypeAsParameter.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeText) &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out TypeOfExpressionSyntax typeOf) &&
                    typeOf.Type is TypeSyntax typeSyntax)
                {
                    context.RegisterCodeFix(
                        $"Change to: {typeText}.",
                        (editor, _) => editor.AddUsing(SystemReflection)
                                             .ReplaceNode(
                                                 typeSyntax,
                                                 x => SyntaxFactory.ParseTypeName(typeText)
                                                                   .WithTriviaFrom(x)),
                        nameof(UseParameterTypeFix),
                        diagnostic);
                }
            }
        }
    }
}
