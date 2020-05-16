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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseContainingTypeFix))]
    [Shared]
    internal class UseContainingTypeFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL015UseContainingType.Id,
            Descriptors.REFL018ExplicitImplementation.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(ITypeSymbol.ContainingType), out var typeName))
                {
                    if (syntaxRoot.TryFindNode(diagnostic, out TypeSyntax? type))
                    {
                        context.RegisterCodeFix(
                            $"Use containing type: {typeName}.",
                            (editor, _) => editor.ReplaceNode(
                                type,
                                x => SyntaxFactory.ParseTypeName(typeName)
                                                  .WithTriviaFrom(x)),
                            nameof(UseContainingTypeFix),
                            diagnostic);
                    }
                    else if (syntaxRoot.TryFindNode(diagnostic, out MemberAccessExpressionSyntax? memberAccess) &&
                             memberAccess.Expression is { } expression)
                    {
                        context.RegisterCodeFix(
                            $"Use containing type: {typeName}.",
                            (editor, _) => editor.ReplaceNode(
                                expression,
                                x => SyntaxFactory.ParseExpression($"typeof({typeName})").WithTriviaFrom(x)),
                            nameof(UseContainingTypeFix),
                            diagnostic);
                    }
                }
            }
        }
    }
}
