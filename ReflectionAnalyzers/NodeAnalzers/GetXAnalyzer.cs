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
            REFL017DontUseNameofWrongMember.Descriptor,
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

                    if (HasWrongFlags(member, flags, out var location, out var flagsText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL005WrongBindingFlags.Descriptor,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), flagsText),
                                $" Expected: {flagsText}."));
                    }

                    if (HasRedundantFlag(member, flags, out flagsText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL006RedundantBindingFlags.Descriptor,
                                flags.Argument.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), flagsText),
                                $" Expected: {flagsText}."));
                    }

                    if (HasMissingFlags(member, flags, out location, out flagsText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL008MissingBindingFlags.Descriptor,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add(nameof(ArgumentSyntax), flagsText),
                                $" Expected: {flagsText}."));
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

                    if (IsPreferGetMemberThenAccessor(member, name, flags, types, context, out var callText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL014PreferGetMemberThenAccessor.Descriptor,
                                invocation.GetNameLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(ExpressionSyntax), callText),
                                callText));
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

                    if (ShouldUseNameof(member, name, context, out location, out var nameString))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL016UseNameof.Descriptor,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add(nameof(NameSyntax), nameString)));
                    }

                    if (ShouldUseStringLiteralName(member, name, out location, out nameString))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL017DontUseNameofWrongMember.Descriptor,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add(nameof(SyntaxKind.StringLiteralExpression), nameString)));
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

                    if (member.Match == FilterMatch.WrongTypes)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL019NoMemberMatchesTheTypes.Descriptor,
                                types.Argument?.GetLocation() ?? invocation.GetNameLocation()));
                    }

                    if (HasMissingTypes(member, types, context, out var typeArrayText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL029MissingTypes.Descriptor,
                                argumentList.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), typeArrayText)));
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
        }

        private static bool HasMissingFlags(ReflectedMember member, Flags flags, out Location location, out string flagsText)
        {
            if (TryGetExpectedFlags(member.ReflectedType, member.Symbol, out var correctFlags) &&
                member.Invocation?.ArgumentList is ArgumentListSyntax argumentList &&
                (member.Match == FilterMatch.Single || member.Match == FilterMatch.WrongFlags))
            {
                if (flags.Argument == null)
                {
                    location = MissingFlagsLocation();
                    flagsText = correctFlags.ToDisplayString(member.Invocation);
                    return true;
                }

                if (flags.Argument is ArgumentSyntax argument &&
                    HasMissingFlag())
                {
                    location = argument.GetLocation();
                    flagsText = correctFlags.ToDisplayString(member.Invocation);
                    return true;
                }
            }

            location = null;
            flagsText = null;
            return false;

            bool HasMissingFlag()
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

            Location MissingFlagsLocation()
            {
                return member.GetX == KnownSymbol.Type.GetConstructor
                    ? argumentList.OpenParenToken.GetLocation()
                    : argumentList.CloseParenToken.GetLocation();
            }
        }

        private static bool HasWrongFlags(ReflectedMember member, Flags flags, out Location location, out string flagText)
        {
            if (member.Match == FilterMatch.WrongFlags &&
                TryGetExpectedFlags(member.ReflectedType, member.Symbol, out var correctFlags))
            {
                flagText = correctFlags.ToDisplayString(flags.Argument);
                if (flags.Argument is ArgumentSyntax argument)
                {
                    location = argument.GetLocation();
                    return true;
                }

                if (member.Invocation?.ArgumentList is ArgumentListSyntax argumentList)
                {
                    location = member.GetX == KnownSymbol.Type.GetConstructor
                        ? argumentList.OpenParenToken.GetLocation()
                        : argumentList.CloseParenToken.GetLocation();
                    return true;
                }
            }

            location = null;
            flagText = null;
            return false;
        }

        private static bool HasRedundantFlag(ReflectedMember member, Flags flags, out string flagsText)
        {
            if (member.Match != FilterMatch.Single ||
                !member.ReflectedType.Locations.Any(x => x.IsInSource))
            {
                flagsText = null;
                return false;
            }

            if (flags.Argument is ArgumentSyntax argument &&
                TryGetExpectedFlags(member.ReflectedType, member.Symbol, out var expectedFlags))
            {
                if (member.Symbol is IMethodSymbol method &&
                    method.MethodKind == MethodKind.Constructor &&
                    (flags.Explicit.HasFlagFast(BindingFlags.DeclaredOnly) ||
                     flags.Explicit.HasFlagFast(BindingFlags.FlattenHierarchy)))
                {
                    flagsText = expectedFlags.ToDisplayString(argument);
                    return true;
                }

                if (member.Symbol is ITypeSymbol &&
                    (flags.Explicit.HasFlagFast(BindingFlags.Instance) ||
                     flags.Explicit.HasFlagFast(BindingFlags.Static) ||
                     flags.Explicit.HasFlagFast(BindingFlags.DeclaredOnly) ||
                     flags.Explicit.HasFlagFast(BindingFlags.FlattenHierarchy)))
                {
                    flagsText = expectedFlags.ToDisplayString(argument);
                    return true;
                }

                if ((member.Symbol.DeclaredAccessibility == Accessibility.Public &&
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
                    flags.Explicit.HasFlagFast(BindingFlags.IgnoreCase))
                {
                    flagsText = expectedFlags.ToDisplayString(argument);
                    return true;
                }
            }

            flagsText = null;
            return false;
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

        private static bool ShouldUseNameof(ReflectedMember member, Name name, SyntaxNodeAnalysisContext context, out Location location, out string nameText)
        {
            if (name.Argument is ArgumentSyntax argument &&
                CanNameof(member.Symbol) &&
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
                    nameText = $"nameof({target})";
                    return true;
                }

                if (IsNameOf(argument, out var expression) &&
                    TryGetSymbol(expression, out var symbol) &&
                    !symbol.ContainingType.IsAssignableTo(member.Symbol.ContainingType, context.Compilation) &&
                    TryGetTargetName(out target))
                {
                    location = expression.GetLocation();
                    nameText = target;
                    return true;
                }
            }

            location = null;
            nameText = null;
            return false;

            bool CanNameof(ISymbol symbol)
            {
                if (symbol == null ||
                    !symbol.CanBeReferencedByName)
                {
                    return false;
                }

                switch (member.Symbol)
                {
                    case IMethodSymbol method:
                        return method.MethodKind == MethodKind.Ordinary;
                    default:
                        return true;
                }
            }

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
                    if (member.TypeSource is InvocationExpressionSyntax getType &&
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

        private static bool ShouldUseStringLiteralName(ReflectedMember member, Name name, out Location location, out string nameText)
        {
            if (name.Argument is ArgumentSyntax argument &&
                IsNameOf(argument, out _) &&
                (member.Match == FilterMatch.NoMatch ||
                 (member.Match == FilterMatch.Unknown &&
                  !Type.HasVisibleNonPublicMembers(member.ReflectedType, recursive: true))))
            {
                nameText = name.MetadataName;
                location = argument.GetLocation();
                return true;
            }

            location = null;
            nameText = null;
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

        private static bool IsPreferGetMemberThenAccessor(ReflectedMember member, Name name, Flags flags, Types types, SyntaxNodeAnalysisContext context, out string call)
        {
            // Not handling when explicit types are passed for now. No reason for this other than that I think it will be rare.
            if (types.Argument is null &&
                member.Invocation?.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (member.Symbol is IMethodSymbol method)
                {
                    if (method.AssociatedSymbol is IPropertySymbol property &&
                        TryGetExpectedFlags(property.ContainingType, property, out var bindingFlags))
                    {
                        return TryGetPropertyAccessor(MemberName(property), bindingFlags, out call);
                    }

                    if (method.AssociatedSymbol is IEventSymbol eventSymbol &&
                        TryGetExpectedFlags(eventSymbol.ContainingType, eventSymbol, out bindingFlags))
                    {
                        return TryGetEventAccessor(MemberName(eventSymbol), bindingFlags, out call);
                    }
                }
                //// For symbols not in source and not visible in metadata.
                else if (member.Symbol is null &&
                         flags.Explicit.HasFlagFast(BindingFlags.NonPublic))
                {
                    if (TryGetInvisibleMemberName("get_", out var memberName) ||
                        TryGetInvisibleMemberName("set_", out memberName))
                    {
                        return TryGetPropertyAccessor(memberName, flags.Explicit, out call);
                    }

                    if (TryGetInvisibleMemberName("add_", out memberName) ||
                        TryGetInvisibleMemberName("remove_", out memberName) ||
                        TryGetInvisibleMemberName("raise_", out memberName))
                    {
                        return TryGetEventAccessor(memberName, flags.Explicit, out call);
                    }
                }
            }

            call = null;
            return false;

            bool TryGetPropertyAccessor(string propertyName, BindingFlags bindingFlags, out string result)
            {
                if (name.MetadataName.StartsWith("get_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{memberAccess.Expression}.GetProperty({propertyName}, {bindingFlags.ToDisplayString(memberAccess)}).GetMethod";
                    return true;
                }

                if (name.MetadataName.StartsWith("set_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{memberAccess.Expression}.GetProperty({propertyName}, {bindingFlags.ToDisplayString(memberAccess)}).SetMethod";
                    return true;
                }

                result = null;
                return false;
            }

            bool TryGetEventAccessor(string eventName, BindingFlags bindingFlags, out string result)
            {
                if (name.MetadataName.StartsWith("add_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{memberAccess.Expression}.GetEvent({eventName}, {bindingFlags.ToDisplayString(memberAccess)}).AddMethod";
                    return true;
                }

                if (name.MetadataName.StartsWith("remove_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{memberAccess.Expression}.GetEvent({eventName}, {bindingFlags.ToDisplayString(memberAccess)}).RemoveMethod";
                    return true;
                }

                if (name.MetadataName.StartsWith("raise_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{memberAccess.Expression}.GetEvent({eventName}, {bindingFlags.ToDisplayString(memberAccess)}).RaiseMethod";
                    return true;
                }

                result = null;
                return false;
            }

            bool TryGetInvisibleMemberName(string prefix, out string memberName)
            {
                if (name.MetadataName is string metadataName &&
                    metadataName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    memberName = $"\"{metadataName.Substring(prefix.Length)}\"";
                    return true;
                }

                memberName = null;
                return false;
            }

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

        private static bool TryGetExpectedFlags(ITypeSymbol reflectedType, ISymbol member, out BindingFlags flags)
        {
            flags = 0;
            if (member is null ||
                reflectedType is null)
            {
                return false;
            }

            if (member.DeclaredAccessibility == Accessibility.Public)
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.NonPublic;
            }

            if (!(member is ITypeSymbol))
            {
                if (member.IsStatic)
                {
                    flags |= BindingFlags.Static;
                }
                else
                {
                    flags |= BindingFlags.Instance;
                }

                if (!(member is IMethodSymbol method &&
                      method.MethodKind == MethodKind.Constructor))
                {
                    if (Equals(member.ContainingType, reflectedType))
                    {
                        flags |= BindingFlags.DeclaredOnly;
                    }
                    else if (member.IsStatic)
                    {
                        flags |= BindingFlags.FlattenHierarchy;
                    }
                }
            }

            return true;
        }

        private static bool HasMissingTypes(ReflectedMember member, Types types, SyntaxNodeAnalysisContext context, out string typesString)
        {
            if ((member.Symbol as IMethodSymbol)?.AssociatedSymbol != null)
            {
                typesString = null;
                return false;
            }

            if (member.Match == FilterMatch.Single &&
                types.Argument == null &&
                member.GetX == KnownSymbol.Type.GetMethod &&
                member.Symbol is IMethodSymbol method &&
                !method.IsGenericMethod)
            {
                if (method.Parameters.Length == 0)
                {
                    typesString = "Type.EmptyTypes";
                    return true;
                }

                return TryGetTypesArray(method.Parameters, out typesString);
            }

            typesString = null;
            return false;

            bool TryGetTypesArray(ImmutableArray<IParameterSymbol> parameters, out string typesArrayString)
            {
                var builder = StringBuilderPool.Borrow().Append("new[] { ");
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (!context.SemanticModel.IsAccessible(context.Node.SpanStart, parameter.Type))
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
                               .Append(parameter.Type.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart))
                               .Append(")");
                }

                typesArrayString = builder.Append(" }").Return();
                return true;
            }
        }
    }
}
