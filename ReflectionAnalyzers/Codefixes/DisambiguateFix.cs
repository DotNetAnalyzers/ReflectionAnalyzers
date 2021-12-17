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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisambiguateFix))]
    [Shared]
    internal class DisambiguateFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));
        private static readonly ArgumentSyntax NullArgument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
                                                                           .WithAdditionalAnnotations(Formatter.Annotation);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.REFL004AmbiguousMatch.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } argumentList &&
                    argumentList.Arguments.TrySingle(out var arg) &&
                    semanticModel is { } &&
                    arg.TryGetStringValue(semanticModel, context.CancellationToken, out var memberName) &&
                    diagnostic.Properties.TryGetValue(nameof(INamedTypeSymbol), out var typeName) &&
                    semanticModel.Compilation.GetTypeByMetadataName(typeName!) is { } type)
                {
                    if (invocation.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, context.CancellationToken, out _))
                    {
                        foreach (var member in type.GetMembers())
                        {
                            if (member is IMethodSymbol method &&
                                method.MetadataName == memberName &&
                                Flags.TryGetExpectedBindingFlags(type, method, out var flags) &&
                                flags.ToDisplayString(invocation) is { } flagsText &&
                                Types.TryGetTypesArrayText(method.Parameters, semanticModel, invocation.SpanStart, out var typesArrayText))
                            {
                                context.RegisterCodeFix(
                                    $"Use: {typesArrayText}.",
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

                    if (invocation.TryGetTarget(KnownSymbol.Type.GetProperty, semanticModel, context.CancellationToken, out _))
                    {
                        foreach (var member in type.GetMembers())
                        {
                            if (member is IPropertySymbol property &&
                                property.MetadataName == memberName &&
                                Flags.TryGetExpectedBindingFlags(type, property, out var flags) &&
                                flags.ToDisplayString(invocation) is { } flagsText &&
                                Types.TryGetTypesArrayText(property.Parameters, semanticModel, invocation.SpanStart, out var typesArrayText))
                            {
                                context.RegisterCodeFix(
                                    $"Use: {typesArrayText}.",
                                    (editor, _) => editor.AddUsing(SystemReflection)
                                                         .ReplaceNode(
                                                             argumentList,
                                                             x => x.WithArguments(x.Arguments.AddRange(new[]
                                                                   {
                                                                       ParseArgument(flagsText),
                                                                       NullArgument,
                                                                       ParseArgument($"typeof({property.Type.ToString(semanticModel, invocation.SpanStart)})"),
                                                                       ParseArgument(typesArrayText),
                                                                       NullArgument,
                                                                   }))
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
