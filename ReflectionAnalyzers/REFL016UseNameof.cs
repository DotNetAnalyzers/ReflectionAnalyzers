namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class REFL016UseNameof : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "REFL016";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "Use nameof.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.StringLiteralExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is LiteralExpressionSyntax literal &&
                literal.Parent is ArgumentSyntax &&
                SyntaxFacts.IsValidIdentifier(literal.Token.ValueText))
            {
                foreach (var symbol in context.SemanticModel.LookupSymbols(literal.SpanStart, name: literal.Token.ValueText))
                {
                    switch (symbol)
                    {
                        case IParameterSymbol _:
                        case IFieldSymbol _:
                        case IEventSymbol _:
                        case IPropertySymbol _:
                        case IMethodSymbol _:
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, literal.GetLocation()));
                            break;
                        case ILocalSymbol local when IsVisible(literal, local, context.CancellationToken):
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, literal.GetLocation()));
                            break;
                    }
                }
            }
        }

        private static bool IsVisible(LiteralExpressionSyntax literal, ILocalSymbol local, CancellationToken cancellationToken)
        {
            if (local.DeclaringSyntaxReferences.Length == 1 &&
                local.DeclaringSyntaxReferences[0].Span.Start < literal.SpanStart)
            {
                var declaration = local.DeclaringSyntaxReferences[0]
                                       .GetSyntax(cancellationToken);
                return !declaration.Contains(literal);
            }

            return false;
        }
    }
}
