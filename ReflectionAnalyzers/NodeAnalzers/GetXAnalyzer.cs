namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetXAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.REFL003MemberDoesNotExist,
            Descriptors.REFL004AmbiguousMatch,
            Descriptors.REFL005WrongBindingFlags,
            Descriptors.REFL006RedundantBindingFlags,
            Descriptors.REFL008MissingBindingFlags,
            Descriptors.REFL009MemberCannotBeFound,
            Descriptors.REFL013MemberIsOfWrongType,
            Descriptors.REFL014PreferGetMemberThenAccessor,
            Descriptors.REFL015UseContainingType,
            Descriptors.REFL016UseNameof,
            Descriptors.REFL017NameofWrongMember,
            Descriptors.REFL018ExplicitImplementation,
            Descriptors.REFL019NoMemberMatchesTypes,
            Descriptors.REFL029MissingTypes,
            Descriptors.REFL033UseSameTypeAsParameter,
            Descriptors.REFL045InsufficientFlags);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax { ArgumentList: { } argumentList } invocation)
            {
                if (TryGetX(context, out var member, out var name, out var flags, out var types))
                {
                    if (member.Match == FilterMatch.NoMatch &&
                        name.Argument is { })
                    {
                        if (member.ReflectedType?.IsSealed == true ||
                            member.ReflectedType?.IsStatic == true ||
                            member.ReflectedType?.TypeKind == TypeKind.Interface ||
                            member.GetX == KnownSymbol.Type.GetNestedType ||
                            member.GetX == KnownSymbol.Type.GetConstructor ||
                            member.TypeSource is TypeOfExpressionSyntax)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL003MemberDoesNotExist, name.Argument.GetLocation(), member.ReflectedType, name.MetadataName));
                        }
                        else if (!IsNullCheckedAfter(invocation))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL009MemberCannotBeFound, name.Argument.GetLocation(), name.MetadataName, member.ReflectedType));
                        }
                    }

                    if (member.Match == FilterMatch.Ambiguous &&
                        member.ReflectedType is { })
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL004AmbiguousMatch,
                                argumentList.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(
                                    nameof(INamedTypeSymbol),
                                    member.ReflectedType.QualifiedMetadataName())));
                    }

                    if (HasWrongFlags(member, flags, out var location, out var flagsText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL005WrongBindingFlags,
                                location,
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(ArgumentSyntax), flagsText),
                                $" Expected: {flagsText}."));
                    }

                    if (HasRedundantFlag(member, flags, out flagsText) &&
                        flags.Argument is { })
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL006RedundantBindingFlags,
                                flags.Argument.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(ArgumentSyntax), flagsText),
                                $" Expected: {flagsText}."));
                    }

                    if (HasMissingFlags(member, flags, out location, out flagsText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL008MissingBindingFlags,
                                location,
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(ArgumentSyntax), flagsText),
                                $" Expected: {flagsText}."));
                    }

                    if (member.Match == FilterMatch.WrongMemberType &&
                        member.Symbol is { })
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL013MemberIsOfWrongType,
                                invocation.GetNameLocation(),
                                member.ReflectedType,
                                member.Symbol.Kind.ToString().ToLower(CultureInfo.InvariantCulture),
                                name.MetadataName));
                    }

                    if (IsPreferGetMemberThenAccessor(member, name, flags, types, context, out var callText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL014PreferGetMemberThenAccessor,
                                invocation.GetNameLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(
                                    nameof(ExpressionSyntax),
                                    callText),
                                callText));
                    }

                    if (member.Match == FilterMatch.UseContainingType &&
                        member.Symbol is { })
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL015UseContainingType,
                                TargetTypeLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    member.Symbol.ContainingType.ToString(context)),
                                member.Symbol.ContainingType.Name));
                    }

                    if (ShouldUseNameof(member, name, context, out location, out var nameText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL016UseNameof,
                                location,
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(NameSyntax), nameText)));
                    }

                    if (UsesNameOfWrongMember(member, name, context, out location, out nameText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL017NameofWrongMember,
                                location,
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(ExpressionSyntax), nameText),
                                nameText));
                    }

                    if (member.Match == FilterMatch.ExplicitImplementation &&
                        member.Symbol is { })
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL018ExplicitImplementation,
                                TargetTypeLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(
                                    nameof(ISymbol.ContainingType),
                                    member.Symbol.ContainingType.ToString(context)),
                                member.Symbol.Name));
                    }

                    if (member.Match == FilterMatch.WrongTypes)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL019NoMemberMatchesTypes,
                                types.Argument?.GetLocation() ?? invocation.GetNameLocation()));
                    }

                    if (HasMissingTypes(member, types, context, out var typeArrayText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL029MissingTypes,
                                argumentList.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(TypeSyntax), typeArrayText)));
                    }

                    if (ShouldUseSameTypeAsParameter(member, types, context, out location, out var typeText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL033UseSameTypeAsParameter,
                                location,
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(TypeSyntax), typeText),
                                typeText));
                    }

                    if (member.Match == FilterMatch.InSufficientFlags)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.REFL045InsufficientFlags,
                                flags.Argument?.GetLocation() ?? invocation.GetNameLocation()));
                    }
                }
            }

            Location TargetTypeLocation()
            {
                return invocation.Expression is MemberAccessExpressionSyntax { Expression: TypeOfExpressionSyntax typeOf }
                    ? typeOf.Type.GetLocation()
                    : invocation.Expression.GetLocation();
            }
        }

        private static bool IsNullCheckedAfter(InvocationExpressionSyntax invocation)
        {
            return invocation.Parent switch
            {
                EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax { Parent: BlockSyntax { Statements: { } statements } } statement } } declarator }
                    when statements.TryElementAt(statements.IndexOf(statement) + 1, out var next) && next is IfStatementSyntax ifStatement
                    => IsNullCheck(ifStatement.Condition, declarator.Identifier.ValueText),
                IsPatternExpressionSyntax => true,
                ConditionalAccessExpressionSyntax => true,
                _ => false,
            };

            static bool IsNullCheck(ExpressionSyntax expression, string name)
            {
                return expression switch
                {
                    BinaryExpressionSyntax { Left: IdentifierNameSyntax left, Right: LiteralExpressionSyntax right } binary
                    => binary.IsEither(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression) &&
                       right.IsKind(SyntaxKind.NullLiteralExpression) &&
                       left.Identifier.ValueText == name,
                    IsPatternExpressionSyntax { Expression: IdentifierNameSyntax identifier, Pattern: ConstantPatternSyntax { Expression: LiteralExpressionSyntax constant } }
                    => identifier.Identifier.ValueText == name &&
                       constant.IsKind(SyntaxKind.NullLiteralExpression),

                    _ => false,
                };
            }
        }

        private static bool TryGetX(SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags, out Types types)
        {
            name = default;
            if (context.Node is InvocationExpressionSyntax candidate)
            {
                if (GetMethod.Match(candidate, context.SemanticModel, context.CancellationToken) is { } getMethod)
                {
                    member = getMethod.Member;
                    name = getMethod.Name;
                    flags = getMethod.Flags;
                    types = getMethod.Types;
                    return true;
                }

                if (GetConstructor.Match(candidate, context.SemanticModel, context.CancellationToken) is { } getCtor)
                {
                    member = getCtor.Member;
                    name = default;
                    flags = getCtor.Flags;
                    types = getCtor.Types;
                    return true;
                }

                types = default;
                return GetX.TryMatchGetEvent(candidate, context.SemanticModel, context.CancellationToken, out member, out name, out flags) ||
                       GetX.TryMatchGetField(candidate, context.SemanticModel, context.CancellationToken, out member, out name, out flags) ||
                       GetX.TryMatchGetNestedType(candidate, context.SemanticModel, context.CancellationToken, out member, out name, out flags) ||
                       GetX.TryMatchGetProperty(candidate, context.SemanticModel, context.CancellationToken, out member, out name, out flags, out types);
            }

            member = default;
            flags = default;
            types = default;
            return false;
        }

        private static bool HasMissingFlags(ReflectedMember member, Flags flags, [NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out string? flagsText)
        {
            if (member.Symbol is { } symbol &&
                member.ReflectedType is { } &&
                Flags.TryGetExpectedBindingFlags(member.ReflectedType, symbol, out var correctFlags) &&
                member.Invocation?.ArgumentList is { } argumentList &&
                (member.Match == FilterMatch.Single || member.Match == FilterMatch.WrongFlags))
            {
                if (flags.Argument is null)
                {
                    location = MissingFlagsLocation();
                    flagsText = correctFlags.ToDisplayString(member.Invocation);
                    return true;
                }

                if (flags.Argument is { } argument &&
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
                if (symbol is ITypeSymbol ||
                    symbol is IMethodSymbol { MethodKind: MethodKind.Constructor })
                {
                    return false;
                }

                return TypeSymbolComparer.Equal(symbol.ContainingType, member.ReflectedType) &&
                       !flags.Explicit.HasFlagFast(BindingFlags.DeclaredOnly);
            }

            Location MissingFlagsLocation()
            {
                return member.GetX == KnownSymbol.Type.GetConstructor
                    ? argumentList.OpenParenToken.GetLocation()
                    : argumentList.CloseParenToken.GetLocation();
            }
        }

        private static bool HasWrongFlags(ReflectedMember member, Flags flags, [NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out string? flagText)
        {
            if (member.Match == FilterMatch.WrongFlags &&
                member.Symbol is { } &&
                member.ReflectedType is { } &&
                Flags.TryGetExpectedBindingFlags(member.ReflectedType, member.Symbol, out var correctFlags))
            {
                flagText = correctFlags.ToDisplayString(flags.Argument);
                if (flags.Argument is { } argument)
                {
                    location = argument.GetLocation();
                    return true;
                }

                if (member.Invocation?.ArgumentList is { } argumentList)
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

        private static bool HasRedundantFlag(ReflectedMember member, Flags flags, [NotNullWhen(true)] out string? flagsText)
        {
            if (member.Match != FilterMatch.Single ||
                member.ReflectedType is null)
            {
                flagsText = null;
                return false;
            }

            if (member.Symbol is { } &&
                flags.Argument is { } argument &&
                Flags.TryGetExpectedBindingFlags(member.ReflectedType, member.Symbol, out var expectedFlags))
            {
                if (member.Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } &&
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
                    (TypeSymbolComparer.Equal(member.Symbol.ContainingType, member.ReflectedType) &&
                     flags.Explicit.HasFlagFast(BindingFlags.FlattenHierarchy)) ||
                    (!TypeSymbolComparer.Equal(member.Symbol.ContainingType, member.ReflectedType) &&
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

        private static bool ShouldUseNameof(ReflectedMember member, Name name, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out string? nameText)
        {
            if (name.Argument is { Expression: LiteralExpressionSyntax literal } &&
                literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                member.Symbol is { } &&
                IsMatch() &&
                NameOf.TryGetExpressionText(member, context, out var expressionText))
            {
                nameText = $"nameof({expressionText})";
                location = literal.GetLocation();
                return true;
            }

            location = null;
            nameText = null;
            return false;

            bool IsMatch()
            {
                return member.Match switch
                {
                    FilterMatch.Single => true,
                    FilterMatch.NoMatch => false,
                    FilterMatch.ExplicitImplementation => false,
                    FilterMatch.Ambiguous => true,
                    FilterMatch.WrongFlags => true,
                    FilterMatch.InSufficientFlags => true,
                    FilterMatch.WrongTypes => true,
                    FilterMatch.WrongMemberType => false,
                    FilterMatch.UseContainingType => false,
                    FilterMatch.PotentiallyInvisible => false,
                    _ => false,
                };
            }
        }

        private static bool UsesNameOfWrongMember(ReflectedMember member, Name name, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out string? nameText)
        {
            if (name.Argument is { } argument &&
                NameOf.IsNameOf(argument, out var expression))
            {
                if (member.Match == FilterMatch.NoMatch ||
                    (member.Match == FilterMatch.PotentiallyInvisible &&
                     member.Symbol is not IMethodSymbol))
                {
                    nameText = $"\"{name.MetadataName}\"";
                    location = argument.GetLocation();
                    return true;
                }

                if (member.Symbol is { } memberSymbol &&
                    TryGetSymbol(expression) is { } symbol &&
                    !symbol.ContainingType.IsAssignableTo(memberSymbol.ContainingType, context.Compilation) &&
                    NameOf.TryGetExpressionText(member, context, out nameText))
                {
                    location = expression.GetLocation();
                    return true;
                }
            }

            location = null;
            nameText = null;
            return false;

            ISymbol? TryGetSymbol(ExpressionSyntax e)
            {
                return context.SemanticModel.TryGetSymbol(e, context.CancellationToken, out var symbol) ||
                       context.SemanticModel.GetSymbolInfo(e, context.CancellationToken)
                              .CandidateSymbols.TryFirst(out symbol)
                    ? symbol
                    : null;
            }
        }

        private static bool IsPreferGetMemberThenAccessor(ReflectedMember member, Name name, Flags flags, Types types, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out string? callText)
        {
            if (member is { ReflectedType: { }, Invocation: { Expression: MemberAccessExpressionSyntax memberAccess } })
            {
                if (member is { Match: FilterMatch.Single, Symbol: IMethodSymbol method })
                {
                    if (method.AssociatedSymbol is IPropertySymbol property &&
                        Flags.TryGetExpectedBindingFlags(property.ContainingType, property, out var bindingFlags))
                    {
                        return TryGetPropertyAccessor(MemberName(property), bindingFlags, property.Type, out callText);
                    }

                    if (method.AssociatedSymbol is IEventSymbol eventSymbol &&
                        Flags.TryGetExpectedBindingFlags(eventSymbol.ContainingType, eventSymbol, out bindingFlags))
                    {
                        return TryGetEventAccessor(MemberName(eventSymbol), bindingFlags, out callText);
                    }
                }
                else if (member.Match == FilterMatch.PotentiallyInvisible &&
                         member.ReflectedType is { } &&
                         types.Argument is null &&
                         flags.Explicit.HasFlagFast(BindingFlags.NonPublic))
                {
                    if (TryGetInvisibleMemberName("get_", out var memberName) ||
                        TryGetInvisibleMemberName("set_", out memberName))
                    {
                        return TryGetPropertyAccessor(memberName, flags.Explicit, member.ReflectedType, out callText);
                    }

                    if (TryGetInvisibleMemberName("add_", out memberName) ||
                        TryGetInvisibleMemberName("remove_", out memberName) ||
                        TryGetInvisibleMemberName("raise_", out memberName))
                    {
                        return TryGetEventAccessor(memberName, flags.Explicit, out callText);
                    }
                }
            }

            callText = null;
            return false;

            bool TryGetPropertyAccessor(string propertyName, BindingFlags bindingFlags, ITypeSymbol type, out string result)
            {
                if (name.MetadataName.StartsWith("get_", StringComparison.Ordinal))
                {
                    result = $"{GetProperty()}.GetMethod";
                    return true;
                }

                if (name.MetadataName.StartsWith("set_", StringComparison.Ordinal))
                {
                    result = $"{GetProperty()}.SetMethod";
                    return true;
                }

                result = null!;
                return false;

                string GetProperty()
                {
                    if (flags.Argument is null)
                    {
                        return $"{memberAccess.Expression}.GetProperty({propertyName})";
                    }

                    if (types.Argument is null)
                    {
                        return $"{memberAccess.Expression}.GetProperty({propertyName}, {bindingFlags.ToDisplayString(memberAccess)})";
                    }

                    if (name.MetadataName.StartsWith("get_", StringComparison.Ordinal))
                    {
                        return $"{memberAccess.Expression}.GetProperty({propertyName}, {bindingFlags.ToDisplayString(memberAccess)}, null, typeof({type.ToString(context)}), {types.Argument}, null)";
                    }

                    if (member.Symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol { IsIndexer: true } property })
                    {
                        if (property.GetMethod is { } getMethod &&
                            Types.TryGetTypesArrayText(getMethod.Parameters, context.SemanticModel, context.Node.SpanStart, out var typesArrayText))
                        {
                            return $"{memberAccess.Expression}.GetProperty({propertyName}, {bindingFlags.ToDisplayString(memberAccess)}, null, typeof({type.ToString(context)}), {typesArrayText}, null)";
                        }

                        if (property.SetMethod is { } setMethod &&
                            Types.TryGetTypesArrayText(setMethod.Parameters.RemoveAt(setMethod.Parameters.Length - 1), context.SemanticModel, context.Node.SpanStart, out typesArrayText))
                        {
                            return $"{memberAccess.Expression}.GetProperty({propertyName}, {bindingFlags.ToDisplayString(memberAccess)}, null, typeof({type.ToString(context)}), {typesArrayText}, null)";
                        }
                    }

                    return $"{memberAccess.Expression}.GetProperty({propertyName}, {bindingFlags.ToDisplayString(memberAccess)}, null, typeof({type.ToString(context)}), Type.EmptyTypes, null)";
                }
            }

            bool TryGetEventAccessor(string eventName, BindingFlags bindingFlags, out string result)
            {
                if (name.MetadataName.StartsWith("add_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{GetEvent()}.AddMethod";
                    return true;
                }

                if (name.MetadataName.StartsWith("remove_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{GetEvent()}.RemoveMethod";
                    return true;
                }

                if (name.MetadataName.StartsWith("raise_", StringComparison.OrdinalIgnoreCase))
                {
                    result = $"{GetEvent()}.RaiseMethod";
                    return true;
                }

                result = null!;
                return false;

                string GetEvent() => flags.Argument is null
                    ? $"{memberAccess.Expression}.GetEvent({eventName})"
                    : $"{memberAccess.Expression}.GetEvent({eventName}, {bindingFlags.ToDisplayString(memberAccess)})";
            }

            bool TryGetInvisibleMemberName(string prefix, out string memberName)
            {
                if (name.MetadataName is { } metadataName &&
                    metadataName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    memberName = $"\"{metadataName.Substring(prefix.Length)}\"";
                    return true;
                }

                memberName = null!;
                return false;
            }

            string MemberName(ISymbol associatedSymbol)
            {
                if (associatedSymbol is IPropertySymbol { IsIndexer: true })
                {
                    return $"\"{associatedSymbol.MetadataName}\"";
                }

                if (TypeSymbolComparer.Equal(context.ContainingSymbol?.ContainingType, associatedSymbol.ContainingType))
                {
                    if (associatedSymbol.IsStatic)
                    {
                        return $"nameof({associatedSymbol.Name})";
                    }

                    return $"nameof(this.{associatedSymbol.Name})";
                }

                return context.SemanticModel.IsAccessible(context.Node.SpanStart, associatedSymbol)
                    ? $"nameof({member.ReflectedType.ToString(context)}.{associatedSymbol.Name})"
                    : $"\"{associatedSymbol.Name}\"";
            }
        }

        private static bool HasMissingTypes(ReflectedMember member, Types types, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out string? typesArrayText)
        {
            if ((member.Symbol as IMethodSymbol)?.AssociatedSymbol != null)
            {
                typesArrayText = null;
                return false;
            }

            if (types.Argument is null &&
                member is { Match: FilterMatch.Single, Symbol: IMethodSymbol { IsGenericMethod: false } method } &&
                member.GetX == KnownSymbol.Type.GetMethod)
            {
                return Types.TryGetTypesArrayText(method.Parameters, context.SemanticModel, context.Node.SpanStart, out typesArrayText);
            }

            typesArrayText = null;
            return false;
        }

        private static bool ShouldUseSameTypeAsParameter(ReflectedMember member, Types types, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out string? typeText)
        {
            if (types.Argument is { } argument &&
                member.Symbol is IMethodSymbol method)
            {
                if (method.Parameters.Length != types.Expressions.Length)
                {
                    location = null;
                    typeText = null;
                    return false;
                }

                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    if (!TypeSymbolComparer.Equal(types.Symbols[i], EffectiveType(method.Parameters[i].Type)) &&
                        context.SemanticModel.IsAccessible(context.Node.SpanStart, method.Parameters[i].Type))
                    {
                        typeText = method.Parameters[i].Type.ToString(context);
                        var expression = types.Expressions[i];
                        location = argument.Contains(expression)
                            ? expression is TypeOfExpressionSyntax typeOf
                                ? typeOf.Type.GetLocation()
                                : expression.GetLocation()
                            : argument.GetLocation();
                        return true;
                    }

                    static ITypeSymbol EffectiveType(ITypeSymbol t) =>
                        t.NullableAnnotation == NullableAnnotation.Annotated
                            ? t.WithNullableAnnotation(NullableAnnotation.None)
                            : t;
                }
            }

            location = null;
            typeText = null;
            return false;
        }
    }
}
