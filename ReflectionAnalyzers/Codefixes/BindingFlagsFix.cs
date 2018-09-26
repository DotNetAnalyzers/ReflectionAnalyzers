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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BindingFlagsFix))]
    [Shared]
    internal class BindingFlagsFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));

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
                    diagnostic.Properties.TryGetValue(nameof(ArgumentSyntax), out var argumentString) &&
                    argumentList.Arguments.Count == 1)
                {
                    context.RegisterCodeFix(
                        $"Add argument: {argumentString}.",
                        (editor, _) => editor.AddUsing(SystemReflection)
                                             .ReplaceNode(
                                                 argumentList,
                                                 argumentList.AddArguments(
                                                     SyntaxFactory.Argument(SyntaxFactory.ParseExpression(argumentString)
                                                                  .WithLeadingTrivia(SyntaxFactory.ElasticSpace)))),
                        nameof(BindingFlagsFix),
                        diagnostic);
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
    }
}
