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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisambiguateFix))]
    [Shared]
    internal class DisambiguateFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));
        private static readonly ArgumentSyntax NullArgument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
                                                                           .WithAdditionalAnnotations(Formatter.Annotation);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL004AmbiguousMatch.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentListSyntax argumentList) &&
                    argumentList.Arguments.TrySingle(out var arg) &&
                    arg.TryGetStringValue(semanticModel, context.CancellationToken, out var methodName) &&
                    argumentList.Parent is InvocationExpressionSyntax invocation &&
                    diagnostic.Properties.TryGetValue(nameof(INamedTypeSymbol), out var typeName) &&
                    semanticModel.Compilation.GetTypeByMetadataName(typeName) is INamedTypeSymbol type)
                {
                    if (invocation.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, context.CancellationToken, out _))
                    {
                        foreach (var member in type.GetMembers(methodName))
                        {
                            if (member is IMethodSymbol method &&
                                Flags.TryGetExpectedBindingFlags(type, method, out var flags) &&
                                flags.ToDisplayString(invocation) is string flagsText &&
                                Types.TryGetTypesArrayText(method.Parameters, semanticModel, invocation.SpanStart, out var typesArrayText))
                            {
                                context.RegisterCodeFix(
                                    $"Use: {flagsText}, {typesArrayText}.",
                                    (editor, _) => editor.AddUsing(SystemReflection)
                                                         .ReplaceNode(
                                                             argumentList,
                                                             x => x.WithArguments(x.Arguments.AddRange(new[] { ParseArgument(flagsText), NullArgument, ParseArgument(typesArrayText), NullArgument }))
                                                                   .WithTriviaFrom(x)),
                                    nameof(DisambiguateFix),
                                    diagnostic);
                            }
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
