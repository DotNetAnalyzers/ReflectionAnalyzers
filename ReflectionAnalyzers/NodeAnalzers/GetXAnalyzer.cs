namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetXAnalyzer : DiagnosticAnalyzer
    {
        private enum GetXResult
        {
            Unknown,
            Single,
            NoMatch,
            Ambiguous,
            WrongFlags,
            WrongMemberType,
            UseContainingType,
        }

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL003MemberDoesNotExist.Descriptor,
            REFL004AmbiguousMatch.Descriptor,
            REFL005WrongBindingFlags.Descriptor,
            REFL006RedundantBindingFlags.Descriptor,
            REFL008MissingBindingFlags.Descriptor,
            REFL013MemberIsOfWrongType.Descriptor,
            REFL014PreferGetProperty.Descriptor,
            REFL015UseContainingType.Descriptor);

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
                switch (TryGetX(context, out var targetType, out var nameArg, out var targetName, out var target, out var flagsArg, out var effectiveFlags))
                {
                    case GetXResult.NoMatch:
                        context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, nameArg.GetLocation(), targetType, targetName));
                        break;

                    case GetXResult.Ambiguous:
                        context.ReportDiagnostic(Diagnostic.Create(REFL004AmbiguousMatch.Descriptor, argumentList.GetLocation()));
                        break;

                    case GetXResult.WrongFlags when TryGetExpectedFlags(target, targetType, out var correctFlags):
                        if (flagsArg != null)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL005WrongBindingFlags.Descriptor,
                                    flagsArg.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), correctFlags.ToDisplayString()),
                                    $" Expected: {correctFlags.ToDisplayString()}."));
                        }
                        else
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL005WrongBindingFlags.Descriptor,
                                    argumentList.CloseParenToken.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), correctFlags.ToDisplayString()),
                                    $" Expected: {correctFlags.ToDisplayString()}."));

                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL008MissingBindingFlags.Descriptor,
                                    argumentList.CloseParenToken.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), correctFlags.ToDisplayString()),
                                    $" Expected: {correctFlags.ToDisplayString()}."));
                        }

                        break;

                    case GetXResult.Single:
                        if (TryGetExpectedFlags(target, targetType, out var expectedFlags) &&
                            effectiveFlags != expectedFlags)
                        {
                            if (flagsArg != null &&
                                HasRedundantFlag(target, targetType, effectiveFlags))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL006RedundantBindingFlags.Descriptor,
                                        flagsArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString()),
                                        $" Expected: {expectedFlags.ToDisplayString()}."));
                            }

                            if (flagsArg == null ||
                                HasMissingFlag(target, targetType, effectiveFlags))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL008MissingBindingFlags.Descriptor,
                                        flagsArg?.GetLocation() ?? argumentList.CloseParenToken.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString()),
                                        $" Expected: {expectedFlags.ToDisplayString()}."));
                            }
                        }

                        if (IsPreferGetProperty(invocation, target, context, out var call))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL014PreferGetProperty.Descriptor,
                                    invocation.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ExpressionSyntax), call),
                                    call));
                        }

                        break;

                    case GetXResult.WrongMemberType:
                        context.ReportDiagnostic(
                            Diagnostic.Create(REFL013MemberIsOfWrongType.Descriptor, invocation.GetNameLocation(), targetType, targetName, target.GetType().Name));
                        break;
                    case GetXResult.UseContainingType:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL015UseContainingType.Descriptor,
                                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                memberAccess.Expression is TypeOfExpressionSyntax typeOf
                                    ? typeOf.Type.GetLocation()
                                    : invocation.Expression.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    target.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                target.ContainingType.Name));
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
                GetX.TryGetTargetType(invocation, context.SemanticModel, context.CancellationToken, out targetType) &&
                TryGetTarget(out var getX) &&
                IsKnownSignature(getX, out var nameParameter) &&
                invocation.TryFindArgument(nameParameter, out nameArg) &&
                nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out targetName) &&
                (TryGetFlagsFromArgument(out flagsArg, out flags) ||
                 TryGetDefaultFlags(out flags)))
            {
                if (getX == KnownSymbol.Type.GetNestedType ||
                    flags.HasFlagFast(BindingFlags.DeclaredOnly))
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
                        if (targetType.TryFindFirstMemberRecursive(targetName, out target))
                        {
                            if (getX == KnownSymbol.Type.GetNestedType &&
                                !targetType.Equals(target.ContainingType))
                            {
                                return GetXResult.UseContainingType;
                            }

                            if (target.IsStatic &&
                                target.DeclaredAccessibility == Accessibility.Private &&
                               !targetType.Equals(target.ContainingType))
                            {
                                return GetXResult.UseContainingType;
                            }

                            return GetXResult.WrongFlags;
                        }

                        if (!HasVisibleMembers(targetType, flags))
                        {
                            return GetXResult.Unknown;
                        }

                        return GetXResult.NoMatch;
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
                            if (target.IsStatic &&
                                target.DeclaredAccessibility == Accessibility.Private &&
                                !target.ContainingType.Equals(targetType))
                            {
                                return GetXResult.UseContainingType;
                            }

                            if (IsOfWrongType(member))
                            {
                                return GetXResult.WrongMemberType;
                            }
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
                    if (targetType.TryFindFirstMemberRecursive(targetName, out target))
                    {
                        return GetXResult.WrongFlags;
                    }

                    if (!HasVisibleMembers(targetType, flags))
                    {
                        return GetXResult.Unknown;
                    }

                    return GetXResult.NoMatch;
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

            bool HasVisibleMembers(ITypeSymbol type, BindingFlags effectiveFlags)
            {
                if (effectiveFlags.HasFlagFast(BindingFlags.NonPublic) &&
                    !type.Locations.Any(x => x.IsInSource))
                {
                    return type.TryFindFirstMember<ISymbol>(x => x.DeclaredAccessibility != Accessibility.Public && !IsExplicitInterfaceImplementation(x), out _);
                }

                return true;

                bool IsExplicitInterfaceImplementation(ISymbol candidate)
                {
                    switch (candidate)
                    {
                        case IEventSymbol eventSymbol:
                            return eventSymbol.ExplicitInterfaceImplementations.Any();
                        case IMethodSymbol method:
                            return method.ExplicitInterfaceImplementations.Any();
                        case IPropertySymbol property:
                            return property.ExplicitInterfaceImplementations.Any();
                        default:
                            return false;
                    }
                }
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
                    case "GetNestedType": // https://referencesource.microsoft.com/#mscorlib/system/type.cs,751
                    case "GetNestedTypes":
                    case "GetProperty":
                    case "GetProperties":
                        defaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
                        return true;
                }

                defaultFlags = 0;
                return false;
            }

            bool MatchesFlags(ISymbol candidate, BindingFlags filter)
            {
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

                if (!(candidate is ITypeSymbol))
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
                }

                return true;
            }

            bool IsOfWrongType(ISymbol member)
            {
                if (getX.ReturnType == KnownSymbol.EventInfo &&
                    !(member is IEventSymbol))
                {
                    return true;
                }

                if (getX.ReturnType == KnownSymbol.FieldInfo &&
                    !(member is IFieldSymbol))
                {
                    return true;
                }

                if (getX.ReturnType == KnownSymbol.MethodInfo &&
                    !(member is IMethodSymbol))
                {
                    return true;
                }

                if (getX.ReturnType == KnownSymbol.PropertyInfo &&
                    !(member is IPropertySymbol))
                {
                    return true;
                }

                return false;
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

        private static bool IsPreferGetProperty(InvocationExpressionSyntax getX, ISymbol target, SyntaxNodeAnalysisContext context, out string call)
        {
            if (target is IMethodSymbol targetMethod &&
                targetMethod.AssociatedSymbol is IPropertySymbol property &&
                getX.Expression is MemberAccessExpressionSyntax memberAccess &&
                TryGetExpectedFlags(property, property.ContainingType, out var flags))
            {
                if (targetMethod.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase))
                {
                    call = $"{memberAccess.Expression}.GetProperty(nameof({property.ContainingType.ToMinimalDisplayString(context.SemanticModel, getX.SpanStart)}.{property.Name}), {flags.ToDisplayString()}).GetMethod";
                    return true;
                }

                if (targetMethod.Name.StartsWith("set_"))
                {
                    call = $"{memberAccess.Expression}.GetProperty(nameof({property.ContainingType.ToMinimalDisplayString(context.SemanticModel, getX.SpanStart)}.{property.Name}), {flags.ToDisplayString()}).SetMethod";
                    return true;
                }
            }

            call = null;
            return false;
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

            if (!(target is ITypeSymbol))
            {
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
            }

            return true;
        }

        private static bool HasRedundantFlag(ISymbol target, ITypeSymbol targetType, BindingFlags flags)
        {
            if (target is ITypeSymbol &&
                (flags.HasFlagFast(BindingFlags.Instance) ||
                 flags.HasFlagFast(BindingFlags.Static) ||
                 flags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                 flags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                return true;
            }

            if (!targetType.Locations.Any(x => x.IsInSource))
            {
                return false;
            }

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

        private static bool HasMissingFlag(ISymbol target, ITypeSymbol targetType, BindingFlags flags)
        {
            return Equals(target.ContainingType, targetType) &&
                   !flags.HasFlagFast(BindingFlags.DeclaredOnly);
        }
    }
}
