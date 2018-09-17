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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GetCustomAttributeFix))]
    [Shared]
    internal class GetCustomAttributeFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL010PreferGenericGetCustomAttribute.DiagnosticId,
            REFL012PreferIsDefined.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax invocation) &&
                    diagnostic.Properties.TryGetValue(nameof(InvocationExpressionSyntax), out var call) &&
                    invocation.Parent is ExpressionSyntax cast &&
                    cast.IsEither(SyntaxKind.CastExpression, SyntaxKind.AsExpression))
                {
                    context.RegisterCodeFix(
                        "Use Generic",
                        (editor, _) => editor.AddUsing(SystemReflection)
                                             .ReplaceNode(
                                                 cast,
                                                 SyntaxFactory.ParseExpression(call)),
                        nameof(GetCustomAttributeFix),
                        diagnostic);
                }
                else if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out BinaryExpressionSyntax binary) &&
                         binary.IsEither(SyntaxKind.NotEqualsExpression, SyntaxKind.EqualsExpression) &&
                         diagnostic.Properties.TryGetValue(nameof(InvocationExpressionSyntax), out call))
                {
                    context.RegisterCodeFix(
                        "Use Attribute.IsDefined",
                        (editor, _) => editor.ReplaceNode(
                            binary,
                            SyntaxFactory.ParseExpression(call)),
                        nameof(GetCustomAttributeFix),
                        diagnostic);
                }
            }
        }
    }
}
