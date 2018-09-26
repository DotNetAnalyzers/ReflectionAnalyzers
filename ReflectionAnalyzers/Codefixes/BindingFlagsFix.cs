namespace ReflectionAnalyzers.Codefixes
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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BindingFlagsFix))]
    [Shared]
    internal class BindingFlagsFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));
        private static readonly ArgumentSyntax NullArgument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
                                                                           .WithAdditionalAnnotations(Formatter.Annotation);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL005WrongBindingFlags.DiagnosticId,
            REFL006RedundantBindingFlags.DiagnosticId,
            REFL007BindingFlagsOrder.DiagnosticId,
            REFL008MissingBindingFlags.DiagnosticId,
            REFL011DuplicateBindingFlags.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentListSyntax argumentList) &&
                    diagnostic.Properties.TryGetValue(nameof(ArgumentSyntax), out var argumentString))
                {
                    if (argumentList.Arguments.Count == 1)
                    {
                        context.RegisterCodeFix(
                            $"Add argument: {argumentString}.",
                            (editor, _) => editor.AddUsing(SystemReflection)
                                                 .ReplaceNode(
                                                     argumentList,
                                                     x => x.AddArguments(ParseArgument(argumentString))),
                            nameof(BindingFlagsFix),
                            diagnostic);
                    }
                    else if (argumentList.Parent is InvocationExpressionSyntax invocation)
                    {
                        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                        if (invocation.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, context.CancellationToken, out var getMethod) &&
                            getMethod.Parameters.Length == 2 &&
                            getMethod.TryFindParameter("name", out _) &&
                            getMethod.TryFindParameter("types", out _))
                        {
                            context.RegisterCodeFix(
                                $"Add argument: {argumentString}.",
                                (editor, _) => editor.AddUsing(SystemReflection)
                                                     .ReplaceNode(
                                                         argumentList,
                                                         x => x.WithArguments(
                                                             x.Arguments.Insert(1, ParseArgument(argumentString))
                                                              .Insert(2, NullArgument)
                                                              .Add(NullArgument))),
                                nameof(BindingFlagsFix),
                                diagnostic);
                        }
                    }
                }
                else if (diagnostic.Properties.TryGetValue(nameof(ArgumentSyntax), out var expressionString) &&
                         syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax argument))
                {
                    context.RegisterCodeFix(
                        $"Change to: {expressionString}.",
                        (editor, _) => editor.AddUsing(SystemReflection)
                                             .ReplaceNode(argument.Expression, SyntaxFactory.ParseExpression(expressionString)),
                        nameof(BindingFlagsFix),
                        diagnostic);
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
