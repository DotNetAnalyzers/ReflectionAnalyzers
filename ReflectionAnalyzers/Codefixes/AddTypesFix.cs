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
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddTypesFix))]
    [Shared]
    internal class AddTypesFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax System = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
        private static readonly ArgumentSyntax NullArgument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
                                                                           .WithAdditionalAnnotations(Formatter.Annotation);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL029MissingTypes.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentListSyntax argumentListSyntax) &&
                    diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeArrayString))
                {
                    if (argumentListSyntax.Arguments.Count == 1)
                    {
                        context.RegisterCodeFix(
                            "Specify types.",
                            (editor, token) =>
                            {
                                if (typeArrayString.Contains("Type.EmptyTypes"))
                                {
                                    _ = editor.AddUsing(System);
                                }

                                _ = editor.ReplaceNode(argumentListSyntax, x => x.AddArguments(ParseArgument(typeArrayString)));
                            },
                            nameof(AddTypesFix),
                            diagnostic);
                    }
                    else if (argumentListSyntax.Parent is InvocationExpressionSyntax invocation)
                    {
                        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                        if (invocation.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, context.CancellationToken, out var getMethod) &&
                            getMethod.Parameters.Length == 2 &&
                            getMethod.TryFindParameter("name", out _) &&
                            getMethod.TryFindParameter("bindingAttr", out _))
                        {
                            context.RegisterCodeFix(
                                $"Add argument: {typeArrayString}.",
                                (editor, __) =>
                                {
                                    if (typeArrayString.Contains("Type.EmptyTypes"))
                                    {
                                        _ = editor.AddUsing(System);
                                    }

                                    _ = editor.ReplaceNode(
                                        argumentListSyntax,
                                        x => x.WithArguments(
                                            x.Arguments.AddRange(new[] { NullArgument, ParseArgument(typeArrayString), NullArgument })));
                                },
                                nameof(AddTypesFix),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static ArgumentSyntax ParseArgument(string expressionString)
        {
            return SyntaxFactory.Argument(SyntaxFactory.ParseExpression(expressionString))
                                .WithLeadingTrivia(SyntaxFactory.ElasticSpace);
        }
    }
}
