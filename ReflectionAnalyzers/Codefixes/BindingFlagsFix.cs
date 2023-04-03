namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
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
    private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(
        SyntaxFactory.QualifiedName(
            SyntaxFactory.IdentifierName("System"),
            SyntaxFactory.IdentifierName("Reflection")));

    private static readonly ArgumentSyntax NullArgument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
                                                                       .WithAdditionalAnnotations(Formatter.Annotation);

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.REFL005WrongBindingFlags.Id,
        Descriptors.REFL006RedundantBindingFlags.Id,
        Descriptors.REFL007BindingFlagsOrder.Id,
        Descriptors.REFL008MissingBindingFlags.Id,
        Descriptors.REFL011DuplicateBindingFlags.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                semanticModel is { } &&
                TryFindArgumentList(syntaxRoot, diagnostic, out var argumentList) &&
                argumentList.Parent is InvocationExpressionSyntax invocation &&
                diagnostic.Properties.TryGetValue(nameof(ArgumentSyntax), out var argumentString))
            {
                if (argumentList.Arguments.Count == 1)
                {
                    if (invocation.TryGetTarget(KnownSymbol.Type.GetEvent, semanticModel, context.CancellationToken, out _) ||
                        invocation.TryGetTarget(KnownSymbol.Type.GetField, semanticModel, context.CancellationToken, out _) ||
                        invocation.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, context.CancellationToken, out _) ||
                        invocation.TryGetTarget(KnownSymbol.Type.GetNestedType, semanticModel, context.CancellationToken, out _) ||
                        invocation.TryGetTarget(KnownSymbol.Type.GetProperty, semanticModel, context.CancellationToken, out _))
                    {
                        context.RegisterCodeFix(
                        $"Add argument: {argumentString}.",
                        editor =>
                        {
                            if (argumentString!.Contains("BindingFlags"))
                            {
                                _ = editor.AddUsing(SystemReflection);
                            }

                            _ = editor.ReplaceNode(
                                      argumentList,
                                      x => x.AddArguments(ParseArgument(argumentString)));
                        },
                        nameof(BindingFlagsFix),
                        diagnostic);
                    }
                    else if (invocation.TryGetTarget(KnownSymbol.Type.GetConstructor, semanticModel, context.CancellationToken, out _))
                    {
                        context.RegisterCodeFix(
                            $"Add argument: {argumentString}.",
                            editor =>
                            {
                                if (argumentString!.Contains("BindingFlags"))
                                {
                                    _ = editor.AddUsing(SystemReflection);
                                }

                                _ = editor.ReplaceNode(
                                    argumentList,
                                    x => x.WithArguments(
                                        x.Arguments.Insert(0, ParseArgument(argumentString))
                                         .Insert(1, NullArgument)
                                         .Add(NullArgument)));
                            },
                            nameof(BindingFlagsFix),
                            diagnostic);
                    }
                }
                else if (argumentList.Arguments.Count == 2 &&
                         invocation.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, context.CancellationToken, out var getMethod) &&
                         getMethod.Parameters.Length == 2 &&
                         getMethod.TryFindParameter("name", out _) &&
                         getMethod.TryFindParameter("types", out _))
                {
                    context.RegisterCodeFix(
                        $"Add argument: {argumentString}.",
                        editor =>
                        {
                            if (argumentString!.Contains("BindingFlags"))
                            {
                                _ = editor.AddUsing(SystemReflection);
                            }

                            _ = editor.ReplaceNode(
                                argumentList,
                                x => x.WithArguments(
                                    x.Arguments.Insert(1, ParseArgument(argumentString))
                                     .Insert(2, NullArgument)
                                     .Add(NullArgument)));
                        },
                        nameof(BindingFlagsFix),
                        diagnostic);
                }
            }
            else if (diagnostic.Properties.TryGetValue(nameof(ArgumentSyntax), out var expressionString) &&
                     syntaxRoot is { } &&
                     syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax? argument))
            {
                context.RegisterCodeFix(
                    $"Change to: {expressionString}.",
                    (editor, _) => editor.ReplaceNode(
                        argument.Expression,
                        SyntaxFactory.ParseExpression(expressionString!).WithTriviaFrom(argument.Expression)),
                    nameof(BindingFlagsFix),
                    diagnostic);
            }
        }
    }

    private static bool TryFindArgumentList(SyntaxNode syntaxRoot, Diagnostic diagnostic, [NotNullWhen(true)] out ArgumentListSyntax? argumentList)
    {
        if (syntaxRoot.TryFindNode(diagnostic, out argumentList))
        {
            return true;
        }

        if (diagnostic.Location.SourceSpan.Length == 1 &&
            syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start) is { Parent: ArgumentListSyntax list } token &&
            token.IsEither(SyntaxKind.OpenParenToken, SyntaxKind.CloseParenToken))
        {
            argumentList = list;
            return true;
        }

        return false;
    }

    private static ArgumentSyntax ParseArgument(string expressionString)
    {
        return SyntaxFactory.Argument(SyntaxFactory.ParseExpression(expressionString))
                            .WithLeadingTrivia(SyntaxFactory.ElasticSpace);
    }
}
