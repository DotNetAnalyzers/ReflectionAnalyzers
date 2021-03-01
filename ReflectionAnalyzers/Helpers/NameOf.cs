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
        private static readonly SymbolDisplayFormat Format = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
#pragma warning disable SA1118 // Parameter should not span multiple lines
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.ExpandNullable);
#pragma warning restore SA1118 // Parameter should not span multiple lines

        internal static bool TryGetExpressionText(ReflectedMember member, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out string? targetName)
        {
            targetName = null;
            if (member.Symbol is null ||
                member.ReflectedType is null ||
                !member.Symbol.CanBeReferencedByName ||
                !context.SemanticModel.IsAccessible(context.Node.SpanStart, member.Symbol) ||
#pragma warning disable CA1508 // Avoid dead conditional code, analyzer wrong
                member.Symbol is INamedTypeSymbol { IsGenericType: true } ||
#pragma warning restore CA1508 // Avoid dead conditional code
                member.Symbol is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
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

            if (member.ReflectedType.IsTupleType)
            {
                var builder = StringBuilderPool.Borrow();
                builder.Append(member.ReflectedType.Name)
                       .Append("<");
                for (var i = 0; i < member.ReflectedType.TypeArguments.Length; i++)
                {
                    builder.Append(member.ReflectedType.TypeArguments[i].ToMinimalDisplayString(context.SemanticModel, member.Invocation.SpanStart, Format));
                    if (i < member.ReflectedType.TypeArguments.Length - 1)
                    {
                        builder.Append(", ");
                    }
                }

                builder.Append(">.")
                       .Append(member.Symbol.Name);
                targetName = builder.Return();
                return true;
            }

            if (context.ContainingSymbol.ContainingType.IsAssignableTo(member.Symbol.ContainingType, context.SemanticModel.Compilation))
            {
                targetName = member.Symbol.IsStatic ||
                             member.Symbol is ITypeSymbol ||
                             context.Node.IsInStaticContext() ||
                             context.SemanticModel.UnderscoreFields() == CodeStyleResult.Yes
                    ? $"{member.Symbol.Name}"
                    : $"this.{member.Symbol.Name}";
                return true;
            }

            targetName = $"{member.ReflectedType.ToMinimalDisplayString(context.SemanticModel, member.Invocation.SpanStart, Format)}.{member.Symbol.Name}";
            return true;
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
