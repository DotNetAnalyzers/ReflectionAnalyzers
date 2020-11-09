namespace ReflectionAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class NameOf
    {
        private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.MinimallyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.MinimallyQualifiedFormat.MiscellaneousOptions | SymbolDisplayMiscellaneousOptions.ExpandNullable);

        internal static bool TryGetExpressionText(ReflectedMember member, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out string? targetName)
        {
            targetName = null;
            if (member.Symbol is null ||
                member.ReflectedType is null ||
                !member.Symbol.CanBeReferencedByName ||
                !context.SemanticModel.IsAccessible(context.Node.SpanStart, member.Symbol) ||
                member.Symbol is INamedTypeSymbol { IsGenericType: true } ||
                (member.Symbol is IMethodSymbol method && method.MethodKind != MethodKind.Ordinary))
            {
                return false;
            }

            if (member.Symbol is { ContainingType: { IsAnonymousType: true } })
            {
                if (member.TypeSource is InvocationExpressionSyntax getType &&
                    getType.TryGetTarget(KnownSymbol.Object.GetType, context.SemanticModel, context.CancellationToken, out _) &&
                    getType.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifierName })
                {
                    targetName = $"{identifierName}.{member.Symbol.Name}";
                    return true;
                }

                return false;
            }

            if (member.Symbol is { ContainingType: { TupleUnderlyingType: { } tupleType } })
            {
                targetName = member.Symbol is IFieldSymbol { CorrespondingTupleField: { } tupleField }
                    ? $"{TypeOfString(tupleType)}.{tupleField.Name}"
                    : $"{TypeOfString(tupleType)}.{member.Symbol.Name}";
                return true;
            }

            if (context.ContainingSymbol.ContainingType.IsAssignableTo(member.Symbol.ContainingType, context.Compilation))
            {
                targetName = member.Symbol.IsStatic ||
                             member.Symbol is ITypeSymbol ||
                             context.Node.IsInStaticContext()
                    ? $"{member.Symbol.Name}"
                    : context.SemanticModel.UnderscoreFields() == CodeStyleResult.Yes
                        ? $"{member.Symbol.Name}"
                        : $"this.{member.Symbol.Name}";
                return true;
            }

            targetName = $"{TypeOfString(member.ReflectedType)}.{member.Symbol.Name}";
            return true;

            string TypeOfString(ITypeSymbol t)
            {
                if (t is INamedTypeSymbol { TupleUnderlyingType: { } utt } namedType &&
                    !TypeSymbolComparer.Equal(utt, namedType))
                {
                    return TypeOfString(utt);
                }

                return t.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart, Format);
            }
        }

        internal static bool IsNameOf(ArgumentSyntax argument, [NotNullWhen(true)] out ExpressionSyntax? expression)
        {
            if (argument.Expression is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier: { ValueText: "nameof" } }, ArgumentList: { Arguments: { Count: 1 } arguments } } &&
                arguments.TrySingle(out var arg))
            {
                expression = arg.Expression;
                return true;
            }

            expression = null;
            return false;
        }
    }
}
