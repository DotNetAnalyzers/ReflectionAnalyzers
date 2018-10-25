namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class NameOf
    {
        private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.MinimallyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayFormat.MinimallyQualifiedFormat.MiscellaneousOptions | SymbolDisplayMiscellaneousOptions.ExpandNullable);

        internal static bool IsNameOf(ArgumentSyntax argument, out ExpressionSyntax expression)
        {
            if (argument.Expression is InvocationExpressionSyntax candidate &&
                candidate.ArgumentList is ArgumentListSyntax argumentList &&
                argumentList.Arguments.TrySingle(out var arg) &&
                candidate.Expression is IdentifierNameSyntax identifierName &&
                identifierName.Identifier.ValueText == "nameof")
            {
                expression = arg.Expression;
                return true;
            }

            expression = null;
            return false;
        }

        internal static bool CanUseFor(ISymbol symbol)
        {
            if (symbol == null ||
                !symbol.CanBeReferencedByName)
            {
                return false;
            }

            switch (symbol)
            {
                case IMethodSymbol method:
                    return method.MethodKind == MethodKind.Ordinary;
                default:
                    return true;
            }
        }

        internal static bool TryGetExpressionText(ReflectedMember member, SyntaxNodeAnalysisContext context, out string targetName)
        {
            targetName = null;
            if (member.Symbol.ContainingType.IsAnonymousType)
            {
                if (member.TypeSource is InvocationExpressionSyntax getType &&
                    getType.TryGetTarget(KnownSymbol.Object.GetType, context.SemanticModel, context.CancellationToken, out _) &&
                    getType.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is IdentifierNameSyntax identifierName)
                {
                    targetName = $"{identifierName}.{member.Symbol.Name}";
                    return true;
                }

                return false;
            }

            if (!context.SemanticModel.IsAccessible(context.Node.SpanStart, member.Symbol) ||
                (member.Symbol is INamedTypeSymbol type && type.IsGenericType) ||
                (member.Symbol is IMethodSymbol method &&
                 method.AssociatedSymbol != null))
            {
                return false;
            }

            if (member.Symbol.ContainingType.IsAssignableTo(context.ContainingSymbol.ContainingType, context.Compilation))
            {
                targetName = member.Symbol.IsStatic ||
                             member.Symbol is ITypeSymbol ||
                             IsStaticContext(context)
                    ? $"{member.Symbol.Name}"
                    : context.SemanticModel.UnderscoreFields()
                        ? $"{member.Symbol.Name}"
                        : $"this.{member.Symbol.Name}";
                return true;
            }

            targetName = context.SemanticModel.IsAccessible(context.Node.SpanStart, member.Symbol)
                ? $"{member.Symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart, Format)}.{member.Symbol.Name}"
                : $"\"{member.Symbol.Name}\"";
            return true;
        }

        internal static bool IsStaticContext(SyntaxNodeAnalysisContext context)
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
