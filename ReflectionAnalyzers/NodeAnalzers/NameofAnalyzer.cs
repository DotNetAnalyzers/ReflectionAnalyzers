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
    internal class NameofAnalyzer : DiagnosticAnalyzer
    {
        private const string Key = nameof(NameSyntax);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL016UseNameof.Descriptor,
            REFL017DontUseNameof.Descriptor);

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
                    if (target.HasValue &&
                        TryGetTargetName(target.Value, context, out var name))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL016UseNameof.Descriptor,
                                literal.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(Key, $"nameof({name})")));
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
                                        REFL016UseNameof.Descriptor,
                                        literal.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(Key, $"nameof({symbol.Name})")));
                                break;
                            case IFieldSymbol _:
                            case IEventSymbol _:
                            case IPropertySymbol _:
                            case IMethodSymbol _:
                                if (TryGetTargetName(symbol, context, out var name))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            REFL016UseNameof.Descriptor,
                                            literal.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add(Key, $"nameof({name})")));
                                }

                                break;
                            case ILocalSymbol local when IsVisible(literal, local, context.CancellationToken):
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL016UseNameof.Descriptor,
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
                TryGetX(invocation, name, context, out var target))
            {
                if (!target.HasValue)
                {
                    if (containingArgument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out name))
                    {
                        context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL017DontUseNameof.Descriptor,
                            containingArgument.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(SyntaxKind.StringLiteralExpression), name)));
                    }
                }
                else if (context.SemanticModel.GetSymbolInfo(argument.Expression, context.CancellationToken).CandidateSymbols.TryFirst(out var symbol) &&
                         !target.Value.ContainingType.Equals(symbol.ContainingType) &&
                         TryGetTargetName(target.Value, context, out var targetName))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL016UseNameof.Descriptor,
                            argument.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(Key, $"{targetName}")));
                }
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
                        target = declaringType.TryFindFirstMemberRecursive(name, out var targetSymbol)
                            ? new Optional<ISymbol>(targetSymbol)
                            : default(Optional<ISymbol>);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryGetTargetName(ISymbol symbol, SyntaxNodeAnalysisContext context, out string name)
        {
            name = null;
            if (!context.SemanticModel.IsAccessible(context.Node.SpanStart, symbol))
            {
                return false;
            }

            if (symbol.ContainingType.IsAssignableTo(context.ContainingSymbol.ContainingType, context.Compilation))
            {
                name = symbol.IsStatic ||
                       symbol is ITypeSymbol ||
                       IsStaticContext(context)
                    ? symbol.Name
                    : context.SemanticModel.UnderscoreFields() ? symbol.Name : $"this.{symbol.Name}";
                return true;
            }

            name = context.SemanticModel.IsAccessible(context.Node.SpanStart, symbol)
                ? $"{symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)}.{symbol.Name}"
                : $"\"{symbol.Name}\"";
            return true;
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
