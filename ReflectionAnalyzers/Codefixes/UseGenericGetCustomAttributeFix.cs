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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseGenericGetCustomAttributeFix))]
    [Shared]
    internal class UseGenericGetCustomAttributeFix : DocumentEditorCodeFixProvider
    {
        private static readonly UsingDirectiveSyntax SystemReflection = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection"));

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            REFL010PreferGenericGetCustomAttribute.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax invocation) &&
                    diagnostic.Properties.TryGetValue(nameof(ExpressionSyntax), out var member) &&
                    diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var type))
                {
                    switch (invocation.Parent)
                    {
                        case CastExpressionSyntax castExpression:
                            context.RegisterCodeFix(
                                "Use Generic",
                                (editor, _) => editor.AddUsing(SystemReflection)
                                                     .ReplaceNode(
                                                         castExpression,
                                                         SyntaxFactory.ParseExpression($"{member}.GetCustomAttribute<{type}>()")),
                                nameof(UseGenericGetCustomAttributeFix),
                                diagnostic);
                            break;
                        case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AsExpression):
                            context.RegisterCodeFix(
                                "Use Generic",
                                (editor, _) => editor.AddUsing(SystemReflection)
                                                     .ReplaceNode(
                                                         binary,
                                                         SyntaxFactory.ParseExpression($"{member}.GetCustomAttribute<{type}>()")),
                                nameof(UseGenericGetCustomAttributeFix),
                                diagnostic);
                            break;
                    }
                }
            }
        }
    }
}
