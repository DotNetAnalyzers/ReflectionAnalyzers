namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
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
            REFL005WrongBindingFlags.Descriptor,
            REFL006RedundantBindingFlags.Descriptor,
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
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var targetType))
            {
                if (argumentList.Arguments.TryFirst(out var nameArg) &&
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
                                    var message = TryGetExpectedFlags(target, targetType, out var expectedFlags)
                                        ? $" Expected: {expectedFlags.ToDisplayString()}."
                                        : null;
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            REFL006RedundantBindingFlags.Descriptor,
                                            flagsArg.GetLocation(),
                                            message));
                                }
                            }
                            else
                            {
                                var message = TryGetExpectedFlags(target, targetType, out var expectedFlags)
                                    ? $" Expected: {expectedFlags.ToDisplayString()}."
                                    : null;
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL008MissingBindingFlags.Descriptor,
                                        argumentList.CloseParenToken.GetLocation(),
                                        message));
                            }
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, nameArg.GetLocation(), targetType, targetName));
                    }
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

        private static bool TryGetExpectedFlags(IMethodSymbol target, ITypeSymbol targetType, out BindingFlags flags)
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

        private static bool HasWrongFlags(IMethodSymbol target, ITypeSymbol targetType, BindingFlags flags)
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

        private static bool HasRedundantFlag(IMethodSymbol target, ITypeSymbol targetType, BindingFlags flags)
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
    }
}
