namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Gu.Roslyn.CodeFixExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRunClassConstructorFix))]
[Shared]
internal class UseRunClassConstructorFix : DocumentEditorCodeFixProvider
{
    private static readonly UsingDirectiveSyntax SystemRuntimeCompilerServices = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Runtime.CompilerServices"));

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.REFL038PreferRunClassConstructor.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeText))
            {
                if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is InvocationExpressionSyntax invocation)
                {
                    context.RegisterCodeFix(
                        $"Change to: RuntimeHelpers.RunClassConstructor(typeof({typeText}).TypeHandle).",
                        (editor, _) => editor.AddUsing(SystemRuntimeCompilerServices)
                                             .ReplaceNode(
                                                 invocation,
                                                 x => SyntaxFactory.ParseExpression($"RuntimeHelpers.RunClassConstructor(typeof({typeText}).TypeHandle)")
                                                                   .WithTriviaFrom(x)),
                        nameof(UseRunClassConstructorFix),
                        diagnostic);
                }
            }
        }
    }
}
