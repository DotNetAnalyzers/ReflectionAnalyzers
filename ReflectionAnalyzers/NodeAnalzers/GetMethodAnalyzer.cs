namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetMethodAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL003MemberDoesNotExist.Descriptor,
            REFL004AmbiguousMatch.Descriptor,
            REFL005WrongBindingFlags.Descriptor,
            REFL006RedundantBindingFlags.Descriptor,
            REFL007BindingFlagsOrder.Descriptor,
            REFL008MissingBindingFlags.Descriptor);

        /// <inheritdoc/>
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
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is TypeOfExpressionSyntax typeOf &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context, out var getMethod) &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var targetType) &&
                argumentList.Arguments.TryFirst(out var nameArg) &&
                nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var targetName))
            {
                if (targetType.TryFindFirstMethodRecursive(targetName, out var target))
                {
                    if (targetType.TryFindSingleMethodRecursive(targetName, out target))
                    {
                        if (TryGetBindingFlags(getMethod, invocation, context, out var flagsArg, out var flags))
                        {
                            if (HasWrongFlags(target, targetType, flags))
                            {
                                var messageArg = TryGetExpectedFlags(target, targetType, out var expectedFlags)
                                    ? $" Expected: {expectedFlags.ToDisplayString()}."
                                    : null;
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL005WrongBindingFlags.Descriptor,
                                        flagsArg.GetLocation(),
                                        messageArg == null
                                            ? ImmutableDictionary<string, string>.Empty
                                            : ImmutableDictionary<string, string>.Empty.Add(nameof(ExpressionSyntax), expectedFlags.ToDisplayString()),
                                        messageArg));
                            }

                            if (HasRedundantFlag(target, targetType, flags))
                            {
                                var messageArg = TryGetExpectedFlags(target, targetType, out var expectedFlags)
                                    ? $" Expected: {expectedFlags.ToDisplayString()}."
                                    : null;
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL006RedundantBindingFlags.Descriptor,
                                        flagsArg.GetLocation(),
                                        messageArg == null
                                            ? ImmutableDictionary<string, string>.Empty
                                            : ImmutableDictionary<string, string>.Empty.Add(nameof(ExpressionSyntax), expectedFlags.ToDisplayString()),
                                        messageArg));
                            }

                            if (TryGetExpectedFlags(target, targetType, out var expectedBindingFlags) &&
                                flags == expectedBindingFlags &&
                                HasWrongOrder(flagsArg))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL007BindingFlagsOrder.Descriptor,
                                        flagsArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ExpressionSyntax), expectedBindingFlags.ToDisplayString()),
                                        $" Expected: {expectedBindingFlags.ToDisplayString()}."));
                            }
                        }
                        else
                        {
                            var messageArg = TryGetExpectedFlags(target, targetType, out var expectedFlags)
                                ? $" Expected: {expectedFlags.ToDisplayString()}."
                                : null;
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL008MissingBindingFlags.Descriptor,
                                    argumentList.CloseParenToken.GetLocation(),
                                    messageArg == null
                                        ? ImmutableDictionary<string, string>.Empty
                                        : ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString()),
                                    messageArg));
                        }
                    }
                    else
                    {
                        if (argumentList.Arguments.Count == 1 &&
                            targetType.GetMembers(targetName).Count(x => x.DeclaredAccessibility == Accessibility.Public) > 1)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(REFL004AmbiguousMatch.Descriptor, nameArg.GetLocation()));
                        }
                    }
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, nameArg.GetLocation(), targetType, targetName));
                }
            }
        }

        private static bool TryGetBindingFlags(IMethodSymbol getMethod, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            flagsArg = null;
            flags = default(BindingFlags);
            return getMethod.TryFindParameter(KnownSymbol.BindingFlags, out var parameter) &&
                   invocation.TryFindArgument(parameter, out flagsArg) &&
                   context.SemanticModel.TryGetConstantValue(flagsArg.Expression, context.CancellationToken, out flags);
        }

        private static bool TryGetExpectedFlags(ISymbol target, ITypeSymbol targetType, out BindingFlags flags)
        {
            flags = 0;
            if (target.DeclaredAccessibility == Accessibility.Public)
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.NonPublic;
            }

            if (target.IsStatic)
            {
                flags |= BindingFlags.Static;
            }
            else
            {
                flags |= BindingFlags.Instance;
            }

            if (Equals(target.ContainingType, targetType))
            {
                flags |= BindingFlags.DeclaredOnly;
            }
            else if (target.IsStatic)
            {
                flags |= BindingFlags.FlattenHierarchy;
            }

            return true;
        }

        private static bool HasWrongFlags(ISymbol target, ITypeSymbol targetType, BindingFlags flags)
        {
            return (target.DeclaredAccessibility != Accessibility.Public &&
                    !flags.HasFlagFast(BindingFlags.NonPublic)) ||
                   (target.DeclaredAccessibility == Accessibility.Public &&
                    !flags.HasFlagFast(BindingFlags.Public)) ||
                   (target.IsStatic &&
                    !flags.HasFlagFast(BindingFlags.Static)) ||
                   (!target.IsStatic &&
                    !flags.HasFlagFast(BindingFlags.Instance)) ||
                   (!Equals(target.ContainingType, targetType) &&
                    flags.HasFlagFast(BindingFlags.DeclaredOnly)) ||
                   (target.IsStatic &&
                    !Equals(target.ContainingType, targetType) &&
                    !flags.HasFlagFast(BindingFlags.FlattenHierarchy));
        }

        private static bool HasRedundantFlag(ISymbol target, ITypeSymbol targetType, BindingFlags flags)
        {
            return (target.DeclaredAccessibility == Accessibility.Public &&
                    flags.HasFlagFast(BindingFlags.NonPublic)) ||
                   (target.DeclaredAccessibility != Accessibility.Public &&
                    flags.HasFlagFast(BindingFlags.Public)) ||
                   (target.IsStatic &&
                    flags.HasFlagFast(BindingFlags.Instance)) ||
                   (!target.IsStatic &&
                    flags.HasFlagFast(BindingFlags.Static)) ||
                   (!target.IsStatic &&
                    flags.HasFlagFast(BindingFlags.FlattenHierarchy)) ||
                   (Equals(target.ContainingType, targetType) &&
                    flags.HasFlagFast(BindingFlags.FlattenHierarchy)) ||
                   (!Equals(target.ContainingType, targetType) &&
                    flags.HasFlagFast(BindingFlags.DeclaredOnly)) ||
                   flags.HasFlagFast(BindingFlags.IgnoreCase);
        }

        private static bool HasWrongOrder(ArgumentSyntax flags)
        {
            if (flags.Expression is BinaryExpressionSyntax binary)
            {
                if (binary.IsKind(SyntaxKind.BitwiseOrExpression) &&
                    binary.Left is MemberAccessExpressionSyntax lm &&
                    binary.Right is MemberAccessExpressionSyntax rm)
                {
                    return IsEither(lm, BindingFlags.Static, BindingFlags.Instance) &&
                           IsEither(rm, BindingFlags.Public, BindingFlags.NonPublic);
                }
            }

            return false;
            bool IsEither(ExpressionSyntax expression, BindingFlags flag1, BindingFlags flag2)
            {
                switch (expression)
                {
                    case IdentifierNameSyntax identifierName:
                        return identifierName.Identifier.ValueText is string name &&
                               (name == flag1.Name() ||
                                name == flag2.Name());
                    case MemberAccessExpressionSyntax memberAccess:
                        return IsEither(memberAccess.Name, flag1, flag2);
                    default:
                        return false;
                }
            }
        }
    }
}
