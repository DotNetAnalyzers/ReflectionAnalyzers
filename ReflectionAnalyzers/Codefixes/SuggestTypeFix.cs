namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuggestTypeFix))]
    [Shared]
    internal class SuggestTypeFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            "REFL037");

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                    TryGetNameAndArity(literal, out var typeName, out var arity))
                {
                    foreach (var assembly in new[] { semanticModel.Compilation.Assembly }.Concat(semanticModel.Compilation.Assembly.Modules.SelectMany(x => x.ReferencedAssemblySymbols)))
                    {
                        foreach (var type in GetTypes(assembly.GlobalNamespace, typeName, arity))
                        {
                            context.RegisterCodeFix(
                                $"Use: {type}.",
                                (editor, _) => editor.ReplaceNode(
                                    literal,
                                    x => x.WithToken(SyntaxFactory.Literal(type.QualifiedMetadataName()))),
                                nameof(SuggestTypeFix),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceSymbol ns, string name, int arity)
        {
            foreach (var type in ns.GetTypeMembers(name, arity))
            {
                yield return type;
            }

            foreach (var nested in ns.GetNamespaceMembers())
            {
                foreach (var type in GetTypes(nested, name, arity))
                {
                    yield return type;
                }
            }
        }

        private static bool TryGetNameAndArity(LiteralExpressionSyntax literal, out string name, out int arity)
        {
            name = literal.Token.ValueText;
            var i = name.LastIndexOf('.');
            if (i > 0)
            {
                name = name.Substring(i + 1);
            }

            if (name.Contains("`"))
            {
                var parts = name.Split('`');
                if (parts.Length == 2)
                {
                    name = parts[0];
                    return int.TryParse(parts[1], out arity);
                }

                arity = -1;
                return false;
            }

            arity = 0;
            return true;
        }
    }
}
