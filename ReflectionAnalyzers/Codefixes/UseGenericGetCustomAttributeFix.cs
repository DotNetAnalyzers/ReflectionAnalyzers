namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.CodeFixExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseGenericGetCustomAttributeFix))]
[Shared]
internal class UseGenericGetCustomAttributeFix : DocumentEditorCodeFixProvider
{
    private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.REFL010PreferGenericGetCustomAttribute.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? invocation) &&
                diagnostic.Properties.TryGetValue(nameof(InvocationExpressionSyntax), out var call) &&
                invocation.Parent is ExpressionSyntax cast &&
                cast.IsEither(SyntaxKind.CastExpression, SyntaxKind.AsExpression))
            {
                context.RegisterCodeFix(
                    "Use Generic",
                    (editor, _) => editor.AddUsing(SystemReflection)
                                         .ReplaceNode(
                                             cast,
                                             SyntaxFactory.ParseExpression(call!)
                                                          .WithTriviaFrom(cast)),
                    nameof(UseGenericGetCustomAttributeFix),
                    diagnostic);
            }
        }
    }
}
