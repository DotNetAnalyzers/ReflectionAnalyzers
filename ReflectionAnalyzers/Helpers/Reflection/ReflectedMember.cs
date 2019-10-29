namespace ReflectionAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct ReflectedMember
    {
        internal static readonly ReflectedMember NoMatch = new ReflectedMember(null, null, null, null, null, FilterMatch.NoMatch);

        /// <summary>
        /// The type that was used to obtain <see cref="Symbol"/>.
        /// </summary>
        internal readonly INamedTypeSymbol ReflectedType;

        /// <summary>
        /// The expression the type was determined from when walking. Example foo.GetType() or typeof(Foo).
        /// </summary>
        internal readonly ExpressionSyntax TypeSource;

        internal readonly ISymbol Symbol;

        internal readonly IMethodSymbol GetX;

        internal readonly InvocationExpressionSyntax Invocation;

        internal readonly FilterMatch Match;

        internal ReflectedMember(INamedTypeSymbol reflectedType, ExpressionSyntax typeSource, ISymbol symbol, IMethodSymbol getX, InvocationExpressionSyntax invocation, FilterMatch match)
        {
            this.ReflectedType = reflectedType;
            this.TypeSource = typeSource;
            this.Symbol = symbol;
            this.GetX = getX;
            this.Invocation = invocation;
            this.Match = match;
        }

        internal static bool TryCreate(IMethodSymbol getX, InvocationExpressionSyntax invocation, INamedTypeSymbol type, ExpressionSyntax typeSource, Name name, BindingFlags flags, Types types, SyntaxNodeAnalysisContext context, out ReflectedMember member)
        {
            var match = TryGetMember(getX, type, name, flags, types, context, out var memberSymbol);
            member = new ReflectedMember(type, typeSource, memberSymbol, getX, invocation, match);
            return true;
        }

        /// <summary>
        /// Returns Foo for the invocation typeof(Foo).GetProperty(Bar).
        /// </summary>
        /// <param name="getX">The invocation of a GetX method, GetEvent, GetField etc.</param>
        /// <param name="context">The <see cref="SyntaxNodeAnalysisContext"/>.</param>
        /// <param name="result">The type.</param>
        /// <param name="typeSource">The expression the type was ultimately produced from.</param>
        /// <returns>True if the type could be determined.</returns>
        internal static bool TryGetType(InvocationExpressionSyntax getX, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out INamedTypeSymbol? result, [NotNullWhen(true)] out ExpressionSyntax? typeSource)
        {
            result = null;
            typeSource = null;
            if (getX.Expression is MemberAccessExpressionSyntax memberAccess &&
                Type.TryGet(memberAccess.Expression, context, out var type, out typeSource))
            {
                result = type as INamedTypeSymbol;
            }

            return result != null;
        }

        private static FilterMatch TryGetMember(IMethodSymbol getX, ITypeSymbol type, Name name, BindingFlags flags, Types types, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out ISymbol? member)
        {
            member = null;
            if (type is ITypeParameterSymbol typeParameter)
            {
                if (typeParameter.ConstraintTypes.Length == 0)
                {
                    return TryGetMember(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), name, flags, types, context, out member);
                }

                foreach (var constraintType in typeParameter.ConstraintTypes)
                {
                    var result = TryGetMember(getX, constraintType, name, flags, types, context, out member);
                    if (result != FilterMatch.NoMatch)
                    {
                        return result;
                    }
                }

                return TryGetMember(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), name, flags, types, context, out member);
            }

            var isAmbiguous = false;
            if (getX == KnownSymbol.Type.GetNestedType ||
                getX == KnownSymbol.Type.GetConstructor ||
                flags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                (flags.HasFlagFast(BindingFlags.Static) &&
                 !flags.HasFlagFast(BindingFlags.Instance) &&
                 !flags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                foreach (var candidate in type.GetMembers())
                {
                    if (!MatchesFilter(candidate, name, flags, types))
                    {
                        continue;
                    }

                    if (types.TryMostSpecific(member, candidate, out member))
                    {
                        isAmbiguous = false;
                        if (IsWrongMemberType(member))
                        {
                            return FilterMatch.WrongMemberType;
                        }
                    }
                    else
                    {
                        isAmbiguous = true;
                    }
                }
            }
            else
            {
                var current = type;
                while (current != null)
                {
                    foreach (var candidate in current.GetMembers())
                    {
                        if (!MatchesFilter(candidate, name, flags, types))
                        {
                            continue;
                        }

                        if (IsOverriding(member, candidate))
                        {
                            continue;
                        }

                        if (types.TryMostSpecific(member, candidate, out member))
                        {
                            isAmbiguous = false;
                            if (IsUseContainingType(member))
                            {
                                return FilterMatch.UseContainingType;
                            }

                            if (candidate.IsStatic &&
                                !current.Equals(type) &&
                                !flags.HasFlagFast(BindingFlags.FlattenHierarchy))
                            {
                                return FilterMatch.WrongFlags;
                            }

                            if (IsWrongMemberType(candidate))
                            {
                                return FilterMatch.WrongMemberType;
                            }
                        }
                        else
                        {
                            isAmbiguous = true;
                        }
                    }

                    current = current.BaseType;
                }
            }

            if (isAmbiguous)
            {
                return FilterMatch.Ambiguous;
            }

            if (member != null)
            {
                return FilterMatch.Single;
            }

            if (type == KnownSymbol.Delegate &&
                name.MetadataName == "Invoke")
            {
                return FilterMatch.Single;
            }

            if (type.TryFindFirstMemberRecursive(x => MatchesFilter(x, name, Flags.MatchAll.Effective, Types.Any), out member))
            {
                if (IsUseContainingType(member))
                {
                    return FilterMatch.UseContainingType;
                }

                if (!Type.HasVisibleMembers(type, flags))
                {
                    return FilterMatch.PotentiallyInvisible;
                }

                if (IsWrongFlags(member))
                {
                    return FilterMatch.WrongFlags;
                }

                if (IsWrongTypes(member))
                {
                    return FilterMatch.WrongTypes;
                }
            }

            if (!Type.HasVisibleMembers(type, flags))
            {
                // Assigning member if it is explicit. Useful info but we can't be sure still.
                _ = IsExplicitImplementation(out member);
                return FilterMatch.PotentiallyInvisible;
            }

            if (IsExplicitImplementation(out member))
            {
                return FilterMatch.ExplicitImplementation;
            }

            return FilterMatch.NoMatch;

            bool IsWrongMemberType(ISymbol symbol)
            {
                if (getX.ReturnType == KnownSymbol.EventInfo &&
                    !(symbol is IEventSymbol))
                {
                    return true;
                }

                if (getX.ReturnType == KnownSymbol.FieldInfo &&
                    !(symbol is IFieldSymbol))
                {
                    return true;
                }

                if (getX.ReturnType == KnownSymbol.MethodInfo &&
                    !(symbol is IMethodSymbol))
                {
                    return true;
                }

                if (getX.ReturnType == KnownSymbol.PropertyInfo &&
                    !(symbol is IPropertySymbol))
                {
                    return true;
                }

                if (getX.ReturnType == KnownSymbol.Type &&
                    !(symbol is ITypeSymbol))
                {
                    return true;
                }

                return false;
            }

            static bool IsOverriding(ISymbol symbol, ISymbol candidateBase)
            {
                if (symbol == null)
                {
                    return false;
                }

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

            bool IsUseContainingType(ISymbol symbol)
            {
                if (type.Equals(symbol.ContainingType))
                {
                    return false;
                }

                return getX == KnownSymbol.Type.GetNestedType ||
                       symbol.DeclaredAccessibility == Accessibility.Private;
            }

            bool IsWrongFlags(ISymbol symbol)
            {
                if (symbol.MetadataName == name.MetadataName &&
                    !MatchesFilter(symbol, name, flags, Types.Any))
                {
                    return true;
                }

                if (!symbol.ContainingType.Equals(type) &&
                    (symbol.IsStatic ||
                     flags.HasFlagFast(BindingFlags.DeclaredOnly)))
                {
                    return true;
                }

                return false;
            }

            bool IsWrongTypes(ISymbol symbol)
            {
                if (types.Argument == null)
                {
                    return false;
                }

                const BindingFlags everything = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
                return symbol.MetadataName == name.MetadataName &&
                       !MatchesFilter(symbol, name, everything, types);
            }

            bool IsExplicitImplementation(out ISymbol result)
            {
                foreach (var @interface in type.AllInterfaces)
                {
                    if (@interface.TryFindFirstMember(x => MatchesFilter(x, name, Flags.MatchAll.Effective, types), out result))
                    {
                        return true;
                    }
                }

                result = null;
                return false;
            }
        }

        private static bool MatchesFilter(ISymbol candidate, Name name, BindingFlags flags, Types types)
        {
            if (candidate.MetadataName != name.MetadataName)
            {
                if (candidate.MetadataName != ".cctor" ||
                    name.MetadataName != Name.Ctor.MetadataName)
                {
                    return false;
                }
            }

            if (candidate is IFieldSymbol field &&
                field.CorrespondingTupleField is IFieldSymbol tupleField &&
                tupleField.Name != field.Name)
            {
                return false;
            }

            if (candidate.DeclaredAccessibility == Accessibility.Public &&
                !flags.HasFlagFast(BindingFlags.Public))
            {
                return false;
            }

            if (candidate.DeclaredAccessibility != Accessibility.Public &&
                !flags.HasFlagFast(BindingFlags.NonPublic))
            {
                return false;
            }

            if (!(candidate is ITypeSymbol))
            {
                if (candidate.IsStatic &&
                    !flags.HasFlagFast(BindingFlags.Static))
                {
                    return false;
                }

                if (!candidate.IsStatic &&
                    !flags.HasFlagFast(BindingFlags.Instance))
                {
                    return false;
                }
            }

            if (types.Argument != null)
            {
                switch (candidate)
                {
                    case IMethodSymbol method when !types.Matches(method.Parameters):
                        return false;
                }
            }

            return true;
        }
    }
}
