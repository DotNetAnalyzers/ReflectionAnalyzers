namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class REFL016UseNameof : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "REFL016";
        private const string Key = nameof(NameSyntax);

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
            context.RegisterSyntaxNodeAction(c => HandleNameof(c), SyntaxKind.InvocationExpression);
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
                    TryGetX(invocation, text, context, out var target))
                {
                    if (target.HasValue)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptor,
                                literal.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(Key, $"nameof({TargetName(context, target.Value)})")));
                    }
                }
                else
                {
                    foreach (var symbol in context.SemanticModel.LookupSymbols(literal.SpanStart, name: text))
                    {
                        switch (symbol)
                        {
                            case IParameterSymbol _:
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptor,
                                        literal.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(Key, $"nameof({symbol.Name})")));
                                break;
                            case IFieldSymbol _:
                            case IEventSymbol _:
                            case IPropertySymbol _:
                            case IMethodSymbol _:
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptor,
                                        literal.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(Key, $"nameof({TargetName(context, symbol)})")));
                                break;
                            case ILocalSymbol local when IsVisible(literal, local, context.CancellationToken):
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptor,
                                        literal.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(Key, $"nameof({symbol.Name})")));
                                break;
                        }
                    }
                }
            }
        }

        private static void HandleNameof(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is InvocationExpressionSyntax candidate &&
                IsNameOf(out var argument) &&
                candidate.Parent is ArgumentSyntax containingArgument &&
                containingArgument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var name) &&
                containingArgument.Parent is ArgumentListSyntax containingArgumentList &&
                containingArgumentList.Parent is InvocationExpressionSyntax invocation &&
                TryGetX(invocation, name, context, out var target) &&
                target.HasValue &&
                context.SemanticModel.GetSymbolInfo(argument.Expression, context.CancellationToken).CandidateSymbols.TryFirst(out var symbol) &&
                !target.Value.ContainingType.Equals(symbol.ContainingType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        argument.GetLocation(),
                        ImmutableDictionary<string, string>.Empty.Add(Key, $"{TargetName(context, target.Value)}")));
            }

            bool IsNameOf(out ArgumentSyntax result)
            {
                result = null;
                return candidate.ArgumentList is ArgumentListSyntax argumentList &&
                       argumentList.Arguments.TrySingle(out result) &&
                       candidate.Expression is IdentifierNameSyntax identifierName &&
                       identifierName.Identifier.ValueText == "nameof";
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

        private static bool TryGetX(InvocationExpressionSyntax invocation, string name, SyntaxNodeAnalysisContext context, out Optional<ISymbol> target)
        {
            target = null;
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                !(memberAccess.Expression is InstanceExpressionSyntax))
            {
                if (invocation.TryGetTarget(KnownSymbol.Type.GetEvent, context.SemanticModel, context.CancellationToken, out _) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetField, context.SemanticModel, context.CancellationToken, out _) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetMember, context.SemanticModel, context.CancellationToken, out _) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out _) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetNestedType, context.SemanticModel, context.CancellationToken, out _) ||
                    invocation.TryGetTarget(KnownSymbol.Type.GetProperty, context.SemanticModel, context.CancellationToken, out _))
                {
                    if (GetX.TryGetTargetType(invocation, context.SemanticModel, context.CancellationToken, out var declaringType))
                    {
                        target = declaringType.TryFindFirstMember(name, out var targetSymbol)
                            ? new Optional<ISymbol>(targetSymbol)
                            : default(Optional<ISymbol>);
                        return true;
                    }
                }
            }

            return false;
        }

        private static string TargetName(SyntaxNodeAnalysisContext context, ISymbol symbol)
        {
            if (context.ContainingSymbol.ContainingType == symbol.ContainingType)
            {
                if (symbol.IsStatic ||
                    IsStaticContext(context))
                {
                    return $"{symbol.Name}";
                }

                return context.SemanticModel.UnderscoreFields() ? symbol.Name : $"this.{symbol.Name}";
            }

            return context.SemanticModel.IsAccessible(context.Node.SpanStart, symbol)
                ? $"{symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)}.{symbol.Name}"
                : $"\"{symbol.Name}\"";
        }

        private static bool IsStaticContext(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.TryFirstAncestor(out AccessorDeclarationSyntax accessor))
            {
                return context.SemanticModel.GetDeclaredSymbolSafe(accessor.FirstAncestor<PropertyDeclarationSyntax>(), context.CancellationToken)?.IsStatic != false;
            }

            if (context.Node.TryFirstAncestor(out BaseMethodDeclarationSyntax methodDeclaration))
            {
                return context.SemanticModel.GetDeclaredSymbolSafe(methodDeclaration, context.CancellationToken)?.IsStatic != false;
            }

            return !context.Node.TryFirstAncestor<AttributeArgumentListSyntax>(out _);
        }
    }
}
