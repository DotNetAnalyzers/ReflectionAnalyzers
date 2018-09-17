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
        private enum GetXResult
        {
            Unknown,
            Single,
            None,
            Ambiguous,
            OtherFlags,
        }

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL003MemberDoesNotExist.Descriptor,
            REFL004AmbiguousMatch.Descriptor,
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
                invocation.ArgumentList is ArgumentListSyntax argumentList)
            {
                switch (TryGetX(context, out var targetType, out var nameArg, out var targetName, out var target, out var flagsArg, out var flags))
                {
                    case GetXResult.Single:
                        if (flagsArg != null)
                        {
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

                        break;
                    case GetXResult.OtherFlags:
                        if (flagsArg != null)
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
                        }
                        else
                        {
                            var messageArg = TryGetExpectedFlags(target, targetType, out var expectedFlags)
                                 ? $" Expected: {expectedFlags.ToDisplayString()}."
                                 : null;
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL005WrongBindingFlags.Descriptor,
                                    argumentList.CloseParenToken.GetLocation(),
                                    messageArg == null
                                        ? ImmutableDictionary<string, string>.Empty
                                        : ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString()),
                                    messageArg));
                        }

                        break;
                    case GetXResult.None:
                        context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, nameArg.GetLocation(), targetType, targetName));
                        break;
                    case GetXResult.Ambiguous:
                        context.ReportDiagnostic(Diagnostic.Create(REFL004AmbiguousMatch.Descriptor, argumentList.GetLocation()));
                        break;
                    case GetXResult.Unknown:
                        break;
                }
            }
        }

        /// <summary>
        /// Handles GetField, GetEvent, GetMember, GetMethod...
        /// </summary>
        private static GetXResult TryGetX(SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            flags = 0;
            if (context.Node is InvocationExpressionSyntax invocation &&
                invocation.ArgumentList != null &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is TypeOfExpressionSyntax typeOf &&
                TryGetTarget(out var getX) &&
                IsKnownSignature(getX, out var nameParameter) &&
                invocation.TryFindArgument(nameParameter, out nameArg) &&
                nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out targetName) &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out targetType) &&
                (TryGetFlagsFromArgument(out flagsArg, out flags) ||
                 TryGetDefaultFlags(out flags)))
            {
                if (flags.HasFlagFast(BindingFlags.DeclaredOnly))
                {
                    foreach (var member in targetType.GetMembers(targetName))
                    {
                        if (!MatchesFlags(member, flags))
                        {
                            continue;
                        }

                        if (target == null)
                        {
                            target = member;
                        }
                        else
                        {
                            return GetXResult.Ambiguous;
                        }
                    }

                    if (target == null)
                    {
                        return targetType.TryFindFirstMemberRecursive(targetName, out target)
                            ? GetXResult.OtherFlags
                            : GetXResult.None;
                    }

                    return GetXResult.Single;
                }

                var current = targetType;
                while (current != null)
                {
                    foreach (var member in current.GetMembers(targetName))
                    {
                        if (!MatchesFlags(member, flags))
                        {
                            continue;
                        }

                        if (member.IsStatic &&
                            !current.Equals(targetType) &&
                            !flags.HasFlagFast(BindingFlags.FlattenHierarchy))
                        {
                            continue;
                        }

                        if (target == null)
                        {
                            target = member;
                        }
                        else if (IsOverriding(target, member))
                        {
                            // continue
                        }
                        else
                        {
                            return GetXResult.Ambiguous;
                        }
                    }

                    current = current.BaseType;
                }

                if (target == null)
                {
                    return targetType.TryFindFirstMemberRecursive(targetName, out target)
                        ? GetXResult.OtherFlags
                        : GetXResult.None;
                }

                return GetXResult.Single;
            }

            return GetXResult.Unknown;

            bool TryGetTarget(out IMethodSymbol result)
            {
                return invocation.TryGetTarget(KnownSymbol.Type.GetEvent, context.SemanticModel, context.CancellationToken, out result) ||
                       invocation.TryGetTarget(KnownSymbol.Type.GetField, context.SemanticModel, context.CancellationToken, out result) ||
                       invocation.TryGetTarget(KnownSymbol.Type.GetMember, context.SemanticModel, context.CancellationToken, out result) ||
                       invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out result) ||
                       invocation.TryGetTarget(KnownSymbol.Type.GetNestedType, context.SemanticModel, context.CancellationToken, out result) ||
                       invocation.TryGetTarget(KnownSymbol.Type.GetProperty, context.SemanticModel, context.CancellationToken, out result);
            }

            bool IsKnownSignature(IMethodSymbol candidate, out IParameterSymbol nameParameterSymbol)
            {
                // I don't know how binder works so limiting checks to what I know.
                return (candidate.Parameters.TrySingle(out nameParameterSymbol) &&
                        nameParameterSymbol.Type == KnownSymbol.String) ||
                       (candidate.Parameters.Length == 2 &&
                        candidate.Parameters.TrySingle(x => x.Type == KnownSymbol.String, out nameParameterSymbol) &&
                        candidate.Parameters.TrySingle(x => x.Type == KnownSymbol.BindingFlags, out _));
            }

            bool TryGetFlagsFromArgument(out ArgumentSyntax argument, out BindingFlags bindingFlags)
            {
                argument = null;
                bindingFlags = 0;
                return getX.TryFindParameter(KnownSymbol.BindingFlags, out var parameter) &&
                       invocation.TryFindArgument(parameter, out argument) &&
                       context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out bindingFlags);
            }

            bool TryGetDefaultFlags(out BindingFlags defaultFlags)
            {
                switch (getX.MetadataName)
                {
                    case "GetField":
                    case "GetFields":
                    case "GetEvent":
                    case "GetEvents":
                    case "GetMethod":
                    case "GetMethods":
                    case "GetMember":
                    case "GetMembers":
                    case "GetNestedTypes":
                    case "GetProperty":
                    case "GetProperties":
                        defaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
                        return true;
                }

                defaultFlags = (BindingFlags)0;
                return false;
            }

            bool MatchesFlags(ISymbol candidate, BindingFlags filter)
            {
                if (candidate.IsStatic &&
                    !filter.HasFlagFast(BindingFlags.Static))
                {
                    return false;
                }

                if (!candidate.IsStatic &&
                    !filter.HasFlagFast(BindingFlags.Instance))
                {
                    return false;
                }

                if (candidate.DeclaredAccessibility == Accessibility.Public &&
                    !filter.HasFlagFast(BindingFlags.Public))
                {
                    return false;
                }

                if (candidate.DeclaredAccessibility != Accessibility.Public &&
                    !filter.HasFlagFast(BindingFlags.NonPublic))
                {
                    return false;
                }

                return true;
            }

            bool IsOverriding(ISymbol symbol, ISymbol candidateBase)
            {
                if (symbol.IsOverride)
                {
                    switch (symbol)
                    {
                        case IEventSymbol eventSymbol:
                            return Equals(eventSymbol.OverriddenEvent, candidateBase) ||
                                   IsOverriding(eventSymbol.OverriddenEvent, candidateBase);
                        case IMethodSymbol method:
                            return Equals(method.OverriddenMethod, candidateBase) ||
                                   IsOverriding(method.OverriddenMethod, candidateBase);
                        case IPropertySymbol property:
                            return Equals(property.OverriddenProperty, candidateBase) ||
                                   IsOverriding(property.OverriddenProperty, candidateBase);
                    }
                }

                return false;
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
            if (target is null ||
                targetType is null)
            {
                return false;
            }

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
    }
}
