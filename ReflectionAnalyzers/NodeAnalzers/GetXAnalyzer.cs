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
            REFL016UseNameof.Descriptor,
            REFL017DontUseNameof.Descriptor,
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
                if (TryGetX(context, out var member, out var name, out var flags, out var types))
                {
                    if (member.Match == FilterMatch.NoMatch)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, name.Argument.GetLocation(), member.ReflectedType, name.MetadataName));
                    }

                    if (member.Match == FilterMatch.Ambiguous)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL004AmbiguousMatchMember.Descriptor, argumentList.GetLocation()));
                    }

                    if (member.Match == FilterMatch.WrongMemberType)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL013MemberIsOfWrongType.Descriptor,
                                invocation.GetNameLocation(),
                                member.ReflectedType,
                                name.MetadataName,
                                member.Symbol.GetType().Name));
                    }

                    if (member.Match == FilterMatch.WrongTypes)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL019NoMemberMatchesTheTypes.Descriptor,
                                types.Argument?.GetLocation() ?? invocation.GetNameLocation()));
                    }

                    if (member.Match == FilterMatch.UseContainingType)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL015UseContainingType.Descriptor,
                                TargetTypeLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    member.Symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                member.Symbol.ContainingType.Name));
                    }

                    if (member.Match == FilterMatch.ExplicitImplementation)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL018ExplicitImplementation.Descriptor,
                                TargetTypeLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    member.Symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                member.Symbol.Name));
                    }

                    if (ShouldUseNameof(member, name, context, out var location, out var nameString))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL016UseNameof.Descriptor,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add(nameof(NameSyntax), nameString)));
                    }

                    if (ShouldUseStringLiteralName(member, name, context, out location, out nameString))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL017DontUseNameof.Descriptor,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add(nameof(SyntaxKind.StringLiteralExpression), nameString)));
                    }

                    switch (member.Match)
                    {
                        case FilterMatch.WrongFlags when TryGetExpectedFlags(member, out var correctFlags):
                            if (flags.Argument != null)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        REFL005WrongBindingFlags.Descriptor,
                                        flags.Argument.GetLocation(),
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

                        case FilterMatch.Single:
                            if (TryGetExpectedFlags(member, out var expectedFlags))
                            {
                                if (flags.Argument != null &&
                                    HasRedundantFlag(member, flags))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            REFL006RedundantBindingFlags.Descriptor,
                                            flags.Argument.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString(invocation)),
                                            $" Expected: {expectedFlags.ToDisplayString(invocation)}."));
                                }

                                if (flags.Argument == null)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            REFL008MissingBindingFlags.Descriptor,
                                            MissingFlagsLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString(invocation)),
                                            $" Expected: {expectedFlags.ToDisplayString(invocation)}."));
                                }
                                else if (HasMissingFlag(member, flags))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            REFL008MissingBindingFlags.Descriptor,
                                            flags.Argument.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), expectedFlags.ToDisplayString(invocation)),
                                            $" Expected: {expectedFlags.ToDisplayString(invocation)}."));
                                }
                            }

                            if (types.Argument == null &&
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

                        case FilterMatch.Unknown:
                            break;
                    }
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

        private static bool IsNameOf(ArgumentSyntax argument, out ExpressionSyntax expression)
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

        private static bool ShouldUseNameof(ReflectedMember member, Name name, SyntaxNodeAnalysisContext context, out Location location, out string nameString)
        {
            if (name.Argument is ArgumentSyntax argument &&
                member.Symbol != null &&
                (member.Match == FilterMatch.Single ||
                 member.Match == FilterMatch.Ambiguous ||
                 member.Match == FilterMatch.WrongFlags ||
                 member.Match == FilterMatch.WrongTypes))
            {
                if (argument.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                    TryGetTargetName(out var target))
                {
                    location = literal.GetLocation();
                    nameString = $"nameof({target})";
                    return true;
                }

                if (IsNameOf(argument, out var expression) &&
                    TryGetSymbol(expression, out var symbol) &&
                    !symbol.ContainingType.IsAssignableTo(member.Symbol.ContainingType, context.Compilation) &&
                    TryGetTargetName(out target))
                {
                    location = expression.GetLocation();
                    nameString = target;
                    return true;
                }
            }

            location = null;
            nameString = null;
            return false;

            bool TryGetSymbol(ExpressionSyntax expression, out ISymbol symbol)
            {
                return context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out symbol) ||
                       context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken)
                              .CandidateSymbols.TryFirst(out symbol);
            }

            bool TryGetTargetName(out string targetName)
            {
                targetName = null;
                if (member.Symbol.ContainingType.IsAnonymousType)
                {
                    if (ReflectedMember.TryGetType(context.Node as InvocationExpressionSyntax, context, out _, out var typeSource) &&
                        typeSource.HasValue &&
                        typeSource.Value is InvocationExpressionSyntax getType &&
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
                                 IsStaticContext()
                        ? member.Symbol.Name
                        : context.SemanticModel.UnderscoreFields() ? member.Symbol.Name : $"this.{member.Symbol.Name}";
                    return true;
                }

                targetName = context.SemanticModel.IsAccessible(context.Node.SpanStart, member.Symbol)
                    ? $"{member.Symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)}.{member.Symbol.Name}"
                    : $"\"{member.Symbol.Name}\"";
                return true;
            }

            bool IsStaticContext()
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

        private static bool ShouldUseStringLiteralName(ReflectedMember member, Name name, SyntaxNodeAnalysisContext context, out Location location, out string nameString)
        {
            if (name.Argument is ArgumentSyntax argument &&
                IsNameOf(argument, out _) &&
                (member.Match == FilterMatch.Unknown ||
                 member.Match == FilterMatch.NoMatch))
            {
                nameString = name.MetadataName;
                location = argument.GetLocation();
                return true;
            }

            location = null;
            nameString = null;
            return false;
        }

        private static bool TryGetX(SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags, out Types types)
        {
            name = default(Name);
            if (context.Node is InvocationExpressionSyntax candidate)
            {
                return GetX.TryMatchGetConstructor(candidate, context, out member, out flags, out types) ||
                       GetX.TryMatchGetEvent(candidate, context, out member, out name, out flags) ||
                       GetX.TryMatchGetField(candidate, context, out member, out name, out flags) ||
                       GetX.TryMatchGetMethod(candidate, context, out member, out name, out flags, out types) ||
                       GetX.TryMatchGetNestedType(candidate, context, out member, out name, out flags) ||
                       GetX.TryMatchGetProperty(candidate, context, out member, out name, out flags);
            }

            member = default(ReflectedMember);
            flags = default(Flags);
            types = default(Types);
            return false;
        }

        private static bool IsPreferGetMemberThenAccessor(InvocationExpressionSyntax getX, ReflectedMember member, SyntaxNodeAnalysisContext context, out string call)
        {
            if (member.Symbol is IMethodSymbol targetMethod &&
                getX.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (targetMethod.AssociatedSymbol is IPropertySymbol property &&
                    TryGetExpectedFlags(new ReflectedMember(property.ContainingType, property, FilterMatch.Single), out var flags))
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
                         TryGetExpectedFlags(new ReflectedMember(eventSymbol.ContainingType, eventSymbol, FilterMatch.Single), out flags))
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

        private static bool HasRedundantFlag(ReflectedMember member, Flags flags)
        {
            if (flags.Argument == null)
            {
                return false;
            }

            if (member.Symbol is IMethodSymbol method &&
                method.MethodKind == MethodKind.Constructor &&
                (flags.Explicit.HasFlagFast(BindingFlags.DeclaredOnly) ||
                 flags.Explicit.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                return true;
            }

            if (member.Symbol is ITypeSymbol &&
                (flags.Explicit.HasFlagFast(BindingFlags.Instance) ||
                 flags.Explicit.HasFlagFast(BindingFlags.Static) ||
                 flags.Explicit.HasFlagFast(BindingFlags.DeclaredOnly) ||
                 flags.Explicit.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                return true;
            }

            if (!member.ReflectedType.Locations.Any(x => x.IsInSource))
            {
                return false;
            }

            return (member.Symbol.DeclaredAccessibility == Accessibility.Public &&
                    flags.Explicit.HasFlagFast(BindingFlags.NonPublic)) ||
                   (member.Symbol.DeclaredAccessibility != Accessibility.Public &&
                    flags.Explicit.HasFlagFast(BindingFlags.Public)) ||
                   (member.Symbol.IsStatic &&
                    flags.Explicit.HasFlagFast(BindingFlags.Instance)) ||
                   (!member.Symbol.IsStatic &&
                    flags.Explicit.HasFlagFast(BindingFlags.Static)) ||
                   (!member.Symbol.IsStatic &&
                    flags.Explicit.HasFlagFast(BindingFlags.FlattenHierarchy)) ||
                   (Equals(member.Symbol.ContainingType, member.ReflectedType) &&
                    flags.Explicit.HasFlagFast(BindingFlags.FlattenHierarchy)) ||
                   (!Equals(member.Symbol.ContainingType, member.ReflectedType) &&
                    flags.Explicit.HasFlagFast(BindingFlags.DeclaredOnly)) ||
                   flags.Explicit.HasFlagFast(BindingFlags.IgnoreCase);
        }

        private static bool HasMissingFlag(ReflectedMember member, Flags flags)
        {
            if (member.Symbol is ITypeSymbol ||
                (member.Symbol is IMethodSymbol method &&
                 method.MethodKind == MethodKind.Constructor))
            {
                return false;
            }

            return Equals(member.Symbol.ContainingType, member.ReflectedType) &&
                   !flags.Explicit.HasFlagFast(BindingFlags.DeclaredOnly);
        }

        private static bool HasMissingTypes(InvocationExpressionSyntax invocation, IMethodSymbol member, SyntaxNodeAnalysisContext context, out string typesString)
        {
            if (member != null &&
                !member.IsGenericMethod)
            {
                if (invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out _))
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
