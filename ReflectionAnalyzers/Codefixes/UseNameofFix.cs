namespace ReflectionAnalyzers.Codefixes
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofFix))]
    [Shared]
    internal class UseNameofFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(REFL016UseNameof.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ArgumentSyntax argument) &&
                    argument.Expression is LiteralExpressionSyntax literal)
                {
                    var name = diagnostic.Properties.TryGetValue(nameof(ISymbol), out var symbolName)
                        ? symbolName
                        : literal.Token.ValueText;
                    context.RegisterCodeFix(
                        "Use nameof",
                        (editor, cancellationToken) => ApplyFix(editor, argument, name, cancellationToken),
                        nameof(UseNameofFix),
                        diagnostic);
                }
            }
        }

        private static void ApplyFix(DocumentEditor editor, ArgumentSyntax argument, string name, CancellationToken cancellationToken)
        {
            if (!IsStaticContext(argument, editor.SemanticModel, cancellationToken) &&
                editor.SemanticModel.LookupSymbols(argument.SpanStart, name: name).TrySingle(out var member) &&
                (member is IFieldSymbol || member is IPropertySymbol || member is IMethodSymbol) &&
                !member.IsStatic &&
                !editor.SemanticModel.UnderscoreFields())
            {
                if (editor.SemanticModel.UnderscoreFields())
                {
                    editor.ReplaceNode(
                        argument.Expression,
                        (x, _) => SyntaxFactory.ParseExpression($"nameof({name})").WithTriviaFrom(x));
                }
                else
                {
                    editor.ReplaceNode(
                        argument.Expression,
                        (x, _) => SyntaxFactory.ParseExpression($"nameof(this.{name})").WithTriviaFrom(x));
                }
            }
            else
            {
                editor.ReplaceNode(
                    argument.Expression,
                    (x, _) => SyntaxFactory.ParseExpression($"nameof({name})").WithTriviaFrom(x));
            }
        }

        private static bool IsStaticContext(SyntaxNode context, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var accessor = context.FirstAncestor<AccessorDeclarationSyntax>();
            if (accessor != null)
            {
                return semanticModel.GetDeclaredSymbolSafe(accessor.FirstAncestor<PropertyDeclarationSyntax>(), cancellationToken)
                                    ?.IsStatic != false;
            }

            var methodDeclaration = context.FirstAncestor<MethodDeclarationSyntax>();
            return semanticModel.GetDeclaredSymbolSafe(methodDeclaration, cancellationToken)?.IsStatic != false;
        }
    }
}
