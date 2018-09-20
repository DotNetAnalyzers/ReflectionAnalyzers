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
            context.RegisterSyntaxNodeAction(c => HandleLiteral(c), SyntaxKind.StringLiteralExpression);
            context.RegisterSyntaxNodeAction(c => HandleNameof(c), SyntaxKind.NameOfKeyword);
        }

        private static void HandleLiteral(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is LiteralExpressionSyntax literal &&
                literal.Parent is ArgumentSyntax argument &&
                literal.Token.ValueText is string text &&
                SyntaxFacts.IsValidIdentifier(text))
            {
                if (argument.Parent is ArgumentListSyntax argumentList &&
                    argumentList.Parent is InvocationExpressionSyntax invocation &&
                    TryGetX(invocation, text, context, out var target) &&
                    target.ContainingType != context.ContainingSymbol.ContainingType)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            literal.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(
                                nameof(ISymbol),
                                $"{target.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)}.{target.Name}")));
                }
                else
                {
                    foreach (var symbol in context.SemanticModel.LookupSymbols(literal.SpanStart, name: text))
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
        }

        private static void HandleNameof(SyntaxNodeAnalysisContext context)
        {
            //if (context.Node is InvocationExpressionSyntax nameof &&
            //    nameof.Parent is ArgumentSyntax argument &&
            //    context.SemanticModel.TryGetConstantValue(nameof, context.CancellationToken, out string name))
            //{
            //    if (argument.Parent is ArgumentListSyntax argumentList &&
            //        argumentList.Parent is InvocationExpressionSyntax invocation &&
            //        TryGetX(invocation, name, context, out var target))
            //    {
            //        context.ReportDiagnostic(
            //            Diagnostic.Create(
            //                Descriptor,
            //                literal.GetLocation(),
            //                ImmutableDictionary<string, string>.Empty.Add(
            //                    nameof(ISymbol),
            //                    $"{target.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)}.{target.Name}")));
            //    }
            //}
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

        private static bool TryGetX(InvocationExpressionSyntax invocation, string name, SyntaxNodeAnalysisContext context, out ISymbol target)
        {
            target = null;
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (invocation.TryGetTarget(KnownSymbol.Type.GetEvent, context.SemanticModel, context.CancellationToken, out var getX) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetField, context.SemanticModel, context.CancellationToken, out getX) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetMember, context.SemanticModel, context.CancellationToken, out getX) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out getX) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetNestedType, context.SemanticModel, context.CancellationToken, out getX) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetProperty, context.SemanticModel, context.CancellationToken, out getX))
                {
                    return GetX.TryGetDeclaringType(invocation, context.SemanticModel, context.CancellationToken, out var declaringType) &&
                           declaringType.TryFindFirstMember(name, out target);
                }
            }

            return false;
        }
    }
}
