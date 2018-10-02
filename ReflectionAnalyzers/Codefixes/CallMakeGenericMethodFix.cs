namespace ReflectionAnalyzers.Codefixes
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CallMakeGenericMethodFix))]
    [Shared]
    internal class CallMakeGenericMethodFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL035DontInvokeGenericDefinition.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typesText) &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax invocation) &&
                    invocation.Expression is MemberAccessExpressionSyntax)
                {
                    context.RegisterCodeFix(
                        $"Call MakeGenericMethod({typesText}).",
                        (editor, _) => editor.ReplaceNode(
                            invocation,
                            x =>
                            {
                                var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
                                return x.WithExpression(memberAccess.WithExpression(SyntaxFactory.ParseExpression($"{memberAccess.Expression.ToFullString()}.MakeGenericMethod({typesText})")));
                            }),
                        nameof(CallMakeGenericMethodFix),
                        diagnostic);
                }
            }
        }
    }
}
