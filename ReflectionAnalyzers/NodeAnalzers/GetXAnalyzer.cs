namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetXAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL003MemberDoesNotExist.Descriptor,
            REFL004AmbiguousMatchMember.Descriptor,
            REFL005WrongBindingFlags.Descriptor,
            REFL006RedundantBindingFlags.Descriptor,
            REFL008MissingBindingFlags.Descriptor,
            REFL013MemberIsOfWrongType.Descriptor,
            REFL014PreferGetMemberThenAccessor.Descriptor,
            REFL015UseContainingType.Descriptor,
            REFL018ExplicitImplementation.Descriptor,
            REFL019NoMemberMatchesTheTypes.Descriptor,
            REFL029MissingTypes.Descriptor);

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
                switch (TryGetX(context, out var member, out var nameArg, out var memberName, out var flagsArg, out var effectiveFlags, out var typesArg))
                {
                    case GetXResult.NoMatch:
                        context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, nameArg.GetLocation(), member.ReflectedType, memberName));
                        break;

                    case GetXResult.Ambiguous:
                        context.ReportDiagnostic(Diagnostic.Create(REFL004AmbiguousMatchMember.Descriptor, argumentList.GetLocation()));
                        break;

                    case GetXResult.WrongFlags when TryGetExpectedFlags(member, out var correctFlags):
                        if (flagsArg != null)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL005WrongBindingFlags.Descriptor,
                                    flagsArg.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), correctFlags.ToDisplayString(invocation)),
                                    $" Expected: {correctFlags.ToDisplayString(invocation)}."));
                        }
                        else
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL005WrongBindingFlags.Descriptor,
                                    MissingFlagsLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), correctFlags.ToDisplayString(invocation)),
                                    $" Expected: {correctFlags.ToDisplayString(invocation)}."));
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL008MissingBindingFlags.Descriptor,
                                    MissingFlagsLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), correctFlags.ToDisplayString(invocation)),
                                    $" Expected: {correctFlags.ToDisplayString(invocation)}."));
                        }

                        break;

                    case GetXResult.Single:
                        if (TryGetExpectedFlags(member, out var expectedFlags))
                        {
                            if (flagsArg != null &&
                                HasRedundantFlag(member, effectiveFlags))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL006RedundantBindingFlags.Descriptor,
                                        flagsArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString(invocation)),
                                        $" Expected: {expectedFlags.ToDisplayString(invocation)}."));
                            }

                            if (flagsArg == null)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL008MissingBindingFlags.Descriptor,
                                        MissingFlagsLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString(invocation)),
                                        $" Expected: {expectedFlags.ToDisplayString(invocation)}."));
                            }
                            else if (HasMissingFlag(member, effectiveFlags))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL008MissingBindingFlags.Descriptor,
                                        flagsArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString(invocation)),
                                        $" Expected: {expectedFlags.ToDisplayString(invocation)}."));
                            }
                        }

                        if (typesArg == null &&
                            HasMissingTypes(invocation, member.Symbol as IMethodSymbol, context, out var typeArrayString))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL029MissingTypes.Descriptor,
                                    argumentList.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), typeArrayString)));
                        }

                        if (IsPreferGetMemberThenAccessor(invocation, member, context, out var call))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL014PreferGetMemberThenAccessor.Descriptor,
                                    invocation.GetNameLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ExpressionSyntax), call),
                                    call));
                        }

                        break;

                    case GetXResult.WrongMemberType:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL013MemberIsOfWrongType.Descriptor,
                                invocation.GetNameLocation(),
                                member.ReflectedType,
                                memberName,
                                member.Symbol.GetType().Name));
                        break;
                    case GetXResult.WrongTypes:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL019NoMemberMatchesTheTypes.Descriptor,
                                typesArg?.GetLocation() ?? invocation.GetNameLocation()));
                        break;
                    case GetXResult.UseContainingType:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL015UseContainingType.Descriptor,
                                TargetTypeLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    member.Symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                member.Symbol.ContainingType.Name));
                        break;
                    case GetXResult.ExplicitImplementation:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL018ExplicitImplementation.Descriptor,
                                TargetTypeLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    member.Symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                member.Symbol.Name));
                        break;
                    case GetXResult.Unknown:
                        break;
                }
            }

            Location TargetTypeLocation()
            {
                return invocation.Expression is MemberAccessExpressionSyntax explicitMemberAccess &&
                        explicitMemberAccess.Expression is TypeOfExpressionSyntax typeOf
                    ? typeOf.Type.GetLocation()
                    : invocation.Expression.GetLocation();
            }

            Location MissingFlagsLocation()
            {
                return invocation.TryGetTarget(KnownSymbol.Type.GetConstructor, context.SemanticModel, context.CancellationToken, out _)
                    ? argumentList.OpenParenToken.GetLocation()
                    : argumentList.CloseParenToken.GetLocation();
            }
        }

        private static GetXResult TryGetX(SyntaxNodeAnalysisContext context, out ReflectedMember member, out ArgumentSyntax nameArg, out string targetName, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags, out ArgumentSyntax typesArg)
        {
            nameArg = null;
            targetName = null;
            typesArg = null;
            if (context.Node is InvocationExpressionSyntax candidate)
            {
                var result = GetX.TryMatchGetConstructor(candidate, context, out member, out flagsArg, out effectiveFlags, out typesArg, out _) ??
                             GetX.TryMatchGetEvent(candidate, context, out member, out nameArg, out targetName, out flagsArg, out effectiveFlags) ??
                             GetX.TryMatchGetField(candidate, context, out member, out nameArg, out targetName, out flagsArg, out effectiveFlags) ??
                             GetX.TryMatchGetMethod(candidate, context, out member, out nameArg, out targetName, out flagsArg, out effectiveFlags, out typesArg, out _) ??
                             GetX.TryMatchGetNestedType(candidate, context, out member, out nameArg, out targetName, out flagsArg, out effectiveFlags) ??
                             GetX.TryMatchGetProperty(candidate, context, out member, out nameArg, out targetName, out flagsArg, out effectiveFlags);
                if (result != null)
                {
                    return result.Value;
                }
            }

            member = default(ReflectedMember);
            nameArg = null;
            targetName = null;
            flagsArg = null;
            effectiveFlags = BindingFlags.Default;
            return GetXResult.Unknown;
        }

        private static bool IsPreferGetMemberThenAccessor(InvocationExpressionSyntax getX, ReflectedMember member, SyntaxNodeAnalysisContext context, out string call)
        {
            if (member.Symbol is IMethodSymbol targetMethod &&
                getX.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (targetMethod.AssociatedSymbol is IPropertySymbol property &&
                    TryGetExpectedFlags(new ReflectedMember(property.ContainingType, property), out var flags))
                {
                    if (targetMethod.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetProperty({MemberName(property)}, {flags.ToDisplayString(memberAccess)}).GetMethod";
                        return true;
                    }

                    if (targetMethod.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetProperty({MemberName(property)}, {flags.ToDisplayString(memberAccess)}).SetMethod";
                        return true;
                    }
                }
                else if (targetMethod.AssociatedSymbol is IEventSymbol eventSymbol &&
                    TryGetExpectedFlags(new ReflectedMember(eventSymbol.ContainingType, eventSymbol), out flags))
                {
                    if (targetMethod.Name.StartsWith("add_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetEvent({MemberName(eventSymbol)}, {flags.ToDisplayString(memberAccess)}).AddMethod";
                        return true;
                    }

                    if (targetMethod.Name.StartsWith("remove_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetEvent({MemberName(eventSymbol)}, {flags.ToDisplayString(memberAccess)}).RemoveMethod";
                        return true;
                    }

                    if (targetMethod.Name.StartsWith("raise_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetEvent({MemberName(eventSymbol)}, {flags.ToDisplayString(memberAccess)}).RaiseMethod";
                        return true;
                    }
                }
            }

            call = null;
            return false;

            string MemberName(ISymbol associatedSymbol)
            {
                if (context.ContainingSymbol.ContainingType == associatedSymbol.ContainingType)
                {
                    if (member.Symbol.IsStatic)
                    {
                        return $"nameof({associatedSymbol.Name})";
                    }

                    return context.SemanticModel.UnderscoreFields() ? associatedSymbol.Name : $"nameof(this.{associatedSymbol.Name})";
                }

                return context.SemanticModel.IsAccessible(context.Node.SpanStart, associatedSymbol)
                    ? $"nameof({associatedSymbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)}.{associatedSymbol.Name})"
                    : $"\"{associatedSymbol.Name}\"";
            }
        }

        private static bool TryGetExpectedFlags(ReflectedMember member, out BindingFlags flags)
        {
            flags = 0;
            if (member.Symbol is null ||
                member.ReflectedType is null)
            {
                return false;
            }

            if (member.Symbol.DeclaredAccessibility == Accessibility.Public)
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.NonPublic;
            }

            if (!(member.Symbol is ITypeSymbol))
            {
                if (member.Symbol.IsStatic)
                {
                    flags |= BindingFlags.Static;
                }
                else
                {
                    flags |= BindingFlags.Instance;
                }

                if (!(member.Symbol is IMethodSymbol method &&
                      method.MethodKind == MethodKind.Constructor))
                {
                    if (Equals(member.Symbol.ContainingType, member.ReflectedType))
                    {
                        flags |= BindingFlags.DeclaredOnly;
                    }
                    else if (member.Symbol.IsStatic)
                    {
                        flags |= BindingFlags.FlattenHierarchy;
                    }
                }
            }

            return true;
        }

        private static bool HasRedundantFlag(ReflectedMember member, BindingFlags flags)
        {
            if (member.Symbol is IMethodSymbol method &&
                method.MethodKind == MethodKind.Constructor &&
                (flags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                 flags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                return true;
            }

            if (member.Symbol is ITypeSymbol &&
                (flags.HasFlagFast(BindingFlags.Instance) ||
                 flags.HasFlagFast(BindingFlags.Static) ||
                 flags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                 flags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                return true;
            }

            if (!member.ReflectedType.Locations.Any(x => x.IsInSource))
            {
                return false;
            }

            return (member.Symbol.DeclaredAccessibility == Accessibility.Public &&
                    flags.HasFlagFast(BindingFlags.NonPublic)) ||
                   (member.Symbol.DeclaredAccessibility != Accessibility.Public &&
                    flags.HasFlagFast(BindingFlags.Public)) ||
                   (member.Symbol.IsStatic &&
                    flags.HasFlagFast(BindingFlags.Instance)) ||
                   (!member.Symbol.IsStatic &&
                    flags.HasFlagFast(BindingFlags.Static)) ||
                   (!member.Symbol.IsStatic &&
                    flags.HasFlagFast(BindingFlags.FlattenHierarchy)) ||
                   (Equals(member.Symbol.ContainingType, member.ReflectedType) &&
                    flags.HasFlagFast(BindingFlags.FlattenHierarchy)) ||
                   (!Equals(member.Symbol.ContainingType, member.ReflectedType) &&
                    flags.HasFlagFast(BindingFlags.DeclaredOnly)) ||
                   flags.HasFlagFast(BindingFlags.IgnoreCase);
        }

        private static bool HasMissingFlag(ReflectedMember member, BindingFlags flags)
        {
            if (member.Symbol is ITypeSymbol ||
                (member.Symbol is IMethodSymbol method &&
                 method.MethodKind == MethodKind.Constructor))
            {
                return false;
            }

            return Equals(member.Symbol.ContainingType, member.ReflectedType) &&
                   !flags.HasFlagFast(BindingFlags.DeclaredOnly);
        }

        private static bool HasMissingTypes(InvocationExpressionSyntax invocation, IMethodSymbol member, SyntaxNodeAnalysisContext context, out string typesString)
        {
            if (member != null &&
                !member.IsGenericMethod)
            {
                if (invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out var getMethod))
                {
                    if (member.Parameters.Length == 0)
                    {
                        typesString = "Type.EmptyTypes";
                        return true;
                    }

                    if (TryGetTypesArray(out typesString))
                    {
                        return true;
                    }

                    typesString = null;
                    return false;
                }
            }

            typesString = null;
            return false;

            bool TryGetTypesArray(out string typesArrayString)
            {
                var builder = StringBuilderPool.Borrow().Append("new[] { ");
                for (var i = 0; i < member.Parameters.Length; i++)
                {
                    var parameter = member.Parameters[i];
                    if (!context.SemanticModel.IsAccessible(invocation.SpanStart, parameter.Type))
                    {
                        _ = builder.Return();
                        typesArrayString = null;
                        return false;
                    }

                    if (i != 0)
                    {
                        _ = builder.Append(", ");
                    }

                    _ = builder.Append("typeof(")
                               .Append(parameter.Type.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart))
                               .Append(")");
                }

                typesArrayString = builder.Append(" }").Return();
                return true;
            }
        }
    }
}
