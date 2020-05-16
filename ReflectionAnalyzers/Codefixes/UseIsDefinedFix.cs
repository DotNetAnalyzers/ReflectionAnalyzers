namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseIsDefinedFix))]
    [Shared]
    internal class UseIsDefinedFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL012PreferIsDefined.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (TryFindExpression(out var expression) &&
                    diagnostic.Properties.TryGetValue(nameof(InvocationExpressionSyntax), out var call))
                {
                    context.RegisterCodeFix(
                        $"Use {call}",
                        (editor, _) => editor.ReplaceNode(
                            expression,
                            SyntaxFactory.ParseExpression(call)
                                         .WithTriviaFrom(expression)),
                        nameof(UseIsDefinedFix),
                        diagnostic);
                }

                bool TryFindExpression(out ExpressionSyntax result)
                {
                    if (syntaxRoot.TryFindNode(diagnostic, out BinaryExpressionSyntax? binary) &&
                        binary.IsEither(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression))
                    {
                        result = binary;
                        return true;
                    }

                    if (syntaxRoot.TryFindNode(diagnostic, out IsPatternExpressionSyntax? isPattern))
                    {
                        result = isPattern;
                        return true;
                    }

                    result = null!;
                    return false;
                }
            }
        }
    }
}
