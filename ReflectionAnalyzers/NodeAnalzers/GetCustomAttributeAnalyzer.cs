﻿namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class GetCustomAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.REFL010PreferGenericGetCustomAttribute,
        Descriptors.REFL012PreferIsDefined,
        Descriptors.REFL044ExpectedAttributeType);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is InvocationExpressionSyntax invocation &&
            TryGetArgs(invocation, context, out var target, out var member, out var attributeType, out var inherits))
        {
            if (invocation.Parent?.IsEither(SyntaxKind.CastExpression, SyntaxKind.AsExpression) == true)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.REFL010PreferGenericGetCustomAttribute,
                        invocation.GetLocation(),
                        ImmutableDictionary<string, string?>.Empty.Add(
                            nameof(InvocationExpressionSyntax),
                            $"{member}.GetCustomAttribute<{attributeType.Value.ToString(context)}>({inherits})"),
                        attributeType.Value.ToString(context)));
            }

            if (PreferIsDefined(invocation, target, member, attributeType, inherits, out var location, out var invocationText))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.REFL012PreferIsDefined,
                        location,
                        ImmutableDictionary<string, string?>.Empty.Add(
                            nameof(InvocationExpressionSyntax),
                            invocationText)));
            }

            if (!attributeType.Value.IsAssignableTo(KnownSymbol.Attribute, context.Compilation))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.REFL044ExpectedAttributeType,
                        attributeType.Argument.GetLocation()));
            }
        }
    }

    private static bool TryGetArgs(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out IMethodSymbol? target, [NotNullWhen(true)] out ExpressionSyntax? member, out ArgumentAndValue<ITypeSymbol> attributeType, out ArgumentSyntax? inheritsArg)
    {
        if ((invocation.TryGetTarget(KnownSymbol.Attribute.GetCustomAttribute, context.SemanticModel, context.CancellationToken, out target) ||
             invocation.TryGetTarget(KnownSymbol.CustomAttributeExtensions.GetCustomAttribute, context.SemanticModel, context.CancellationToken, out target)) &&
            target.TryFindParameter("attributeType", out var attributeTypeParameter) &&
            invocation.TryFindArgument(attributeTypeParameter, out var attributeTypeArg) &&
            attributeTypeArg.Expression is TypeOfExpressionSyntax typeOf &&
            context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var typeSymbol))
        {
            attributeType = new ArgumentAndValue<ITypeSymbol>(attributeTypeArg, typeSymbol);
            if (!target.TryFindParameter("inherit", out var inheritParameter) ||
                !invocation.TryFindArgument(inheritParameter, out inheritsArg))
            {
                inheritsArg = null;
            }

            if (target.IsExtensionMethod &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                member = memberAccess.Expression;
                return true;
            }

            if (target.TryFindParameter("element", out var elementParameter) &&
                invocation.TryFindArgument(elementParameter, out var elementArg))
            {
                member = elementArg.Expression;
                return true;
            }
        }

        member = null;
        attributeType = default;
        inheritsArg = null;
        return false;
    }

    private static bool PreferIsDefined(InvocationExpressionSyntax invocation, IMethodSymbol target, ExpressionSyntax member, ArgumentAndValue<ITypeSymbol> attributeType, ArgumentSyntax? inherits, [NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out string? invocationText)
    {
        switch (invocation.Parent)
        {
            case BinaryExpressionSyntax binary when binary.Right.IsKind(SyntaxKind.NullLiteralExpression):
                if (binary.IsKind(SyntaxKind.EqualsExpression))
                {
                    location = binary.GetLocation();
                    invocationText = "!" + GetText();
                    return true;
                }

                if (binary.IsKind(SyntaxKind.NotEqualsExpression))
                {
                    location = binary.GetLocation();
                    invocationText = GetText();
                    return true;
                }

                break;
            case IsPatternExpressionSyntax { Pattern: ConstantPatternSyntax constantPattern } isPattern
                when constantPattern.Expression.IsKind(SyntaxKind.NullLiteralExpression):
                location = isPattern.GetLocation();
                invocationText = "!" + GetText();
                return true;
        }

        location = null;
        invocationText = null;
        return false;

        string GetText()
        {
            var inheritsText = inherits is null ? string.Empty : $", {inherits}";
            return target.IsExtensionMethod
                ? $"{member}.IsDefined({attributeType.Argument}{inheritsText})"
                : $"Attribute.IsDefined({member}, {attributeType.Argument}{inheritsText})";
        }
    }
}
