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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CastReturnValueFix))]
    [Shared]
    internal class CastReturnValueFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL001CastReturnValue.Id,
            Descriptors.REFL028CastReturnValueToCorrectType.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeString))
                {
                    if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is TypeSyntax typeSyntax)
                    {
                        context.RegisterCodeFix(
                            $"Cast to {typeString}.",
                            (editor, _) => editor.ReplaceNode(
                                typeSyntax,
                                x => SyntaxFactory.ParseTypeName(typeString!)),
                            nameof(CastReturnValueFix),
                            diagnostic);
                    }
                    else if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is InvocationExpressionSyntax invocation)
                    {
                        context.RegisterCodeFix(
                            $"Cast to {typeString}.",
                            (editor, _) => editor.ReplaceNode(
                                invocation,
                                x => SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(typeString!), x)),
                            nameof(CastReturnValueFix),
                            diagnostic);
                    }
                }
            }
        }
    }
}
