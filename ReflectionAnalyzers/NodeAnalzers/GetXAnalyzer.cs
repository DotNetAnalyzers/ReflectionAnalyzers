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
                switch (TryGetX(context, out var type, out var nameArg, out var memberName, out var member, out var flagsArg, out var effectiveFlags, out var typesArg))
                {
                    case GetXResult.NoMatch:
                        context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, nameArg.GetLocation(), type, memberName));
                        break;

                    case GetXResult.Ambiguous:
                        context.ReportDiagnostic(Diagnostic.Create(REFL004AmbiguousMatchMember.Descriptor, argumentList.GetLocation()));
                        break;

                    case GetXResult.WrongFlags when TryGetExpectedFlags(member, type, out var correctFlags):
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
                        if (TryGetExpectedFlags(member, type, out var expectedFlags) &&
                            effectiveFlags != expectedFlags)
                        {
                            if (flagsArg != null &&
                                HasRedundantFlag(member, type, effectiveFlags))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL006RedundantBindingFlags.Descriptor,
                                        flagsArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString()),
                                        $" Expected: {expectedFlags.ToDisplayString()}."));
                            }

                            if (flagsArg == null ||
                                HasMissingFlag(member, type, effectiveFlags))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL008MissingBindingFlags.Descriptor,
                                        flagsArg?.GetLocation() ?? argumentList.CloseParenToken.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString()),
                                        $" Expected: {expectedFlags.ToDisplayString()}."));
                            }
                        }

                        if (typesArg == null &&
                            HasMissingTypes(invocation, member as IMethodSymbol, context, out var argumentListString))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL029MissingTypes.Descriptor,
                                    argumentList.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentListSyntax), argumentListString)));
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
                                type,
                                memberName,
                                member.GetType().Name));
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
                                    member.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                member.ContainingType.Name));
                        break;
                    case GetXResult.ExplicitImplementation:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL018ExplicitImplementation.Descriptor,
                                TargetTypeLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    member.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                member.Name));
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
        }

        private static GetXResult TryGetX(SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags, out ArgumentSyntax typesArg)
        {
            nameArg = null;
            targetName = null;
            typesArg = null;
            if (context.Node is InvocationExpressionSyntax candidate)
            {
                var result = GetX.TryMatchGetConstructor(candidate, context, out targetType, out target, out flagsArg, out effectiveFlags, out typesArg, out _) ??
                             GetX.TryMatchGetEvent(candidate, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags) ??
                             GetX.TryMatchGetField(candidate, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags) ??
                             GetX.TryMatchGetMember(candidate, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags, out typesArg, out _) ??
                             GetX.TryMatchGetMethod(candidate, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags, out typesArg, out _) ??
                             GetX.TryMatchGetNestedType(candidate, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags) ??
                             GetX.TryMatchGetProperty(candidate, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags);
                if (result != null)
                {
                    return result.Value;
                }
            }

            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            effectiveFlags = BindingFlags.Default;
            return GetXResult.Unknown;
        }

        private static bool IsPreferGetMemberThenAccessor(InvocationExpressionSyntax getX, ISymbol target, SyntaxNodeAnalysisContext context, out string call)
        {
            if (target is IMethodSymbol targetMethod &&
                getX.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (targetMethod.AssociatedSymbol is IPropertySymbol property &&
                    TryGetExpectedFlags(property, property.ContainingType, out var flags))
                {
                    if (targetMethod.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetProperty({MemberName(property)}, {flags.ToDisplayString()}).GetMethod";
                        return true;
                    }

                    if (targetMethod.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetProperty({MemberName(property)}, {flags.ToDisplayString()}).SetMethod";
                        return true;
                    }
                }
                else if (targetMethod.AssociatedSymbol is IEventSymbol eventSymbol &&
                    TryGetExpectedFlags(eventSymbol, eventSymbol.ContainingType, out flags))
                {
                    if (targetMethod.Name.StartsWith("add_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetEvent({MemberName(eventSymbol)}, {flags.ToDisplayString()}).AddMethod";
                        return true;
                    }

                    if (targetMethod.Name.StartsWith("remove_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetEvent({MemberName(eventSymbol)}, {flags.ToDisplayString()}).RemoveMethod";
                        return true;
                    }

                    if (targetMethod.Name.StartsWith("raise_", StringComparison.OrdinalIgnoreCase))
                    {
                        call = $"{memberAccess.Expression}.GetEvent({MemberName(eventSymbol)}, {flags.ToDisplayString()}).RaiseMethod";
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
                    if (target.IsStatic)
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

                if (!(target is IMethodSymbol method &&
                      method.MethodKind == MethodKind.Constructor))
                {
                    if (Equals(target.ContainingType, targetType))
                    {
                        flags |= BindingFlags.DeclaredOnly;
                    }
                    else if (target.IsStatic)
                    {
                        flags |= BindingFlags.FlattenHierarchy;
                    }
                }
            }

            return true;
        }

        private static bool HasRedundantFlag(ISymbol target, ITypeSymbol targetType, BindingFlags flags)
        {
            if (target is IMethodSymbol method &&
                method.MethodKind == MethodKind.Constructor &&
                (flags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                 flags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                return true;
            }

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

        private static bool HasMissingTypes(InvocationExpressionSyntax invocation, IMethodSymbol member, SyntaxNodeAnalysisContext context, out string typesString)
        {
            if (member != null &&
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                invocation.TryGetMethodName(out var getXName))
            {
                if (getXName == "GetMethod" &&
                    argumentList.Arguments.Count == 1)
                {
                    return TryGetTypesString(member.Parameters, out typesString);
                }
            }

            typesString = null;
            return false;

            bool TryGetTypesString(ImmutableArray<IParameterSymbol> parameters, out string result)
            {
                if (argumentList.Arguments.TrySingle(out var argument))
                {
                    if (parameters.Length == 0)
                    {
                        result = $"({argument}, Type.EmptyTypes)";
                        return true;
                    }

                    var builder = StringBuilderPool.Borrow()
                                                   .Append("(")
                                                   .Append(argument)
                                                   .Append(", new[] { ");
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        if (!context.SemanticModel.IsAccessible(invocation.SpanStart, parameter.Type))
                        {
                            _ = builder.Return();
                            result = null;
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

                    result = builder.Append(" })")
                                    .Return();
                    return true;
                }

                result = null;
                return false;
            }
        }
    }
}
