namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Helper for Type.GetField, Type.GetEvent, Type.GetMember, Type.GetMethod...
    /// </summary>
    internal static class GetX
    {
#pragma warning disable CA1825 // Avoid zero-length array allocations. We want to check by reference.
        public static readonly IReadOnlyList<ITypeSymbol> AnyTypes = new ITypeSymbol[0];
#pragma warning restore CA1825 // Avoid zero-length array allocations.

        internal static bool TryGetConstructor(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context, out IMethodSymbol constructor)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
            {
                var result = TryMatchGetConstructor(parentInvocation, context, out var member, out _, out _, out _);
                if (result == GetXResult.Single &&
                    member.Symbol is IMethodSymbol match)
                {
                    constructor = match;
                    return true;
                }
            }

            constructor = null;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static GetXResult? TryMatchGetConstructor(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Flags flags, out ArgumentSyntax typesArg, out IReadOnlyList<ITypeSymbol> types)
        {
            member = default(ReflectedMember);
            flags = default(Flags);
            typesArg = null;
            types = null;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetConstructor, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetType(invocation, context, out var type, out _) &&
                IsKnownSignature(invocation, getX) &&
                Flags.TryCreate(invocation, getX, context, out flags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                var result = TryGetMember(getX, type, new Name(null, ".ctor"), flags.Effective, types, context, out var symbol);
                member = new ReflectedMember(type, symbol);
                return result;
            }

            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetEvent.
        /// </summary>
        internal static GetXResult? TryMatchGetEvent(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetEvent, context, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetField.
        /// </summary>
        internal static GetXResult? TryMatchGetField(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetField, context, out member, out name, out flags);
        }

        internal static bool TryGetMethod(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context, out IMethodSymbol method)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
            {
                var result = TryMatchGetMethod(parentInvocation, context, out var member, out _, out _, out _, out _);
                if (result == GetXResult.Single &&
                    member.Symbol is IMethodSymbol match)
                {
                    method = match;
                    return true;
                }
            }

            method = null;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static GetXResult? TryMatchGetMethod(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags, out ArgumentSyntax typesArg, out IReadOnlyList<ITypeSymbol> types)
        {
            typesArg = null;
            types = null;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetType(invocation, context, out var type, out _) &&
                IsKnownSignature(invocation, getX) &&
                Name.TryCreate(invocation, getX, context, out name) &&
                Flags.TryCreate(invocation, getX, context, out flags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                if (type == KnownSymbol.Delegate &&
                    name.MetadataName == "Invoke")
                {
                    member = new ReflectedMember(type, null);
                    return GetXResult.Single;
                }

                var result = TryGetMember(getX, type, name, flags.Effective, types, context, out var symbol);
                member = new ReflectedMember(type, symbol);
                return result;
            }

            member = default(ReflectedMember);
            flags = default(Flags);
            name = default(Name);
            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetNestedType.
        /// </summary>
        internal static GetXResult? TryMatchGetNestedType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetNestedType, context, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetProperty.
        /// </summary>
        internal static GetXResult? TryMatchGetProperty(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetProperty, context, out member, out name, out flags);
        }

        internal static bool TryGetType(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out INamedTypeSymbol type)
        {
            if (Type.TryGet(expression, context, out var temp, out _) &&
                temp is INamedTypeSymbol namedType)
            {
                type = namedType;
                return true;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InvocationExpressionSyntax parentInvocation &&
                TryMatchGetNestedType(parentInvocation, context, out var nestedType, out _, out _) == GetXResult.Single &&
                nestedType.Symbol is INamedTypeSymbol namedNested)
            {
                type = namedNested;
                return true;
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Returns Foo for the invocation typeof(Foo).GetProperty(Bar).
        /// </summary>
        /// <param name="getX">The invocation of a GetX method, GetEvent, GetField etc.</param>
        /// <param name="context">The <see cref="SyntaxNodeAnalysisContext"/>.</param>
        /// <param name="result">The type.</param>
        /// <param name="typeSource">The expression the type was ultimately produced from.</param>
        /// <returns>True if the type could be determined.</returns>
        internal static bool TryGetType(InvocationExpressionSyntax getX, SyntaxNodeAnalysisContext context, out ITypeSymbol result, out Optional<ExpressionSyntax> typeSource)
        {
            result = null;
            typeSource = default(Optional<ExpressionSyntax>);
            return getX.Expression is MemberAccessExpressionSyntax memberAccess &&
                   Type.TryGet(memberAccess.Expression, context, out result, out typeSource);
        }

        internal static GetXResult TryGetMember(IMethodSymbol getX, ITypeSymbol type, Name name, BindingFlags flags, IReadOnlyList<ITypeSymbol> types, SyntaxNodeAnalysisContext context, out ISymbol member)
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
                    if (result != GetXResult.NoMatch)
                    {
                        return result;
                    }
                }

                return TryGetMember(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), name, flags, types, context, out member);
            }

            if (getX == KnownSymbol.Type.GetNestedType ||
                getX == KnownSymbol.Type.GetConstructor ||
                flags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                (flags.HasFlagFast(BindingFlags.Static) &&
                 !flags.HasFlagFast(BindingFlags.Instance) &&
                 !flags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                foreach (var candidate in type.GetMembers(name.MemberName()))
                {
                    if (!MatchesFilter(candidate, name, flags, types))
                    {
                        continue;
                    }

                    if (member == null)
                    {
                        member = candidate;
                        if (IsWrongMemberType(member))
                        {
                            return GetXResult.WrongMemberType;
                        }
                    }
                    else
                    {
                        return GetXResult.Ambiguous;
                    }
                }
            }
            else
            {
                var current = type;
                while (current != null)
                {
                    foreach (var candidate in current.GetMembers(name.MemberName()))
                    {
                        if (!MatchesFilter(candidate, name, flags, types))
                        {
                            continue;
                        }

                        if (IsOverriding(member, candidate))
                        {
                            continue;
                        }

                        if (member == null)
                        {
                            member = candidate;
                            if (IsUseContainingType(member))
                            {
                                return GetXResult.UseContainingType;
                            }

                            if (candidate.IsStatic &&
                                !current.Equals(type) &&
                                !flags.HasFlagFast(BindingFlags.FlattenHierarchy))
                            {
                                return GetXResult.WrongFlags;
                            }

                            if (IsWrongMemberType(candidate))
                            {
                                return GetXResult.WrongMemberType;
                            }
                        }
                        else
                        {
                            return GetXResult.Ambiguous;
                        }
                    }

                    current = current.BaseType;
                }
            }

            if (member != null)
            {
                return GetXResult.Single;
            }

            if (type.TryFindFirstMemberRecursive(name.MemberName(), out member))
            {
                if (IsUseContainingType(member))
                {
                    return GetXResult.UseContainingType;
                }

                if (!Type.HasVisibleMembers(type, flags))
                {
                    return GetXResult.Unknown;
                }

                if (IsWrongFlags(member))
                {
                    return GetXResult.WrongFlags;
                }

                if (IsWrongTypes(member))
                {
                    return GetXResult.WrongTypes;
                }
            }

            if (!Type.HasVisibleMembers(type, flags))
            {
                // Assigning member if it is explicit. Useful info but we can't be sure still.
                _ = IsExplicitImplementation(out member);
                return GetXResult.Unknown;
            }

            if (IsExplicitImplementation(out member))
            {
                return GetXResult.ExplicitImplementation;
            }

            return GetXResult.NoMatch;

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

            bool IsOverriding(ISymbol symbol, ISymbol candidateBase)
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
                return !type.Equals(symbol.ContainingType) &&
                       (getX == KnownSymbol.Type.GetNestedType ||
                        (symbol.IsStatic &&
                         symbol.DeclaredAccessibility == Accessibility.Private));
            }

            bool IsWrongFlags(ISymbol symbol)
            {
                if (symbol.MetadataName == name.MetadataName &&
                    !MatchesFilter(symbol, name, flags, null))
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
                if (types == null ||
                    ReferenceEquals(types, AnyTypes))
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
                    if (@interface.TryFindFirstMemberRecursive(name.MemberName(), out var interfaceMember))
                    {
                        result = interfaceMember;
                        return true;
                    }
                }

                result = null;
                return false;
            }
        }

        /// <summary>
        /// Defensive check to only handle known cases. Don't know how the binder works.
        /// </summary>
        private static bool IsKnownSignature(InvocationExpressionSyntax invocation, IMethodSymbol getX)
        {
            foreach (var parameter in getX.Parameters)
            {
                if (!IsKnownArgument(parameter))
                {
                    return false;
                }
            }

            return true;
            bool IsKnownArgument(IParameterSymbol parameter)
            {
                if (parameter.Type == KnownSymbol.String ||
                    parameter.Type == KnownSymbol.BindingFlags ||
                    parameter.Name == "types")
                {
                    return true;
                }

                return invocation.TryFindArgument(parameter, out var argument) &&
                       argument.Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true;
            }
        }

        /// <summary>
        /// Handles GetField, GetEvent, GetMember, GetMethod...
        /// </summary>
        private static GetXResult? TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(getXMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetType(invocation, context, out var type, out _) &&
                Name.TryCreate(invocation, getX, context, out name) &&
                Flags.TryCreate(invocation, getX, context, out flags))
            {
                var result = TryGetMember(getX, type, name, flags.Effective, null, context, out var symbol);
                member = new ReflectedMember(type, symbol);
                return result;
            }

            name = default(Name);
            member = default(ReflectedMember);
            flags = default(Flags);
            return null;
        }

        private static bool TryGetTypesOrDefault(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out ArgumentSyntax typesArg, out IReadOnlyList<ITypeSymbol> types)
        {
            if (TryGetTypesArgument(invocation, getX, out typesArg))
            {
                return Array.TryGetTypes(typesArg.Expression, context, out types);
            }

            types = null;
            return true;
        }

        private static bool TryGetTypesArgument(InvocationExpressionSyntax invocation, IMethodSymbol getX, out ArgumentSyntax argument)
        {
            argument = null;
            return getX.TryFindParameter("types", out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument);
        }

        private static bool MatchesFilter(ISymbol candidate, Name name, BindingFlags flags, IReadOnlyList<ITypeSymbol> types)
        {
            if (candidate.MetadataName != name.MetadataName)
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

            if (types != null &&
                !ReferenceEquals(types, AnyTypes))
            {
                switch (candidate)
                {
                    case IMethodSymbol method:
                        if (method.Parameters.Length != types.Count)
                        {
                            return false;
                        }

                        for (var i = 0; i < method.Parameters.Length; i++)
                        {
                            if (!method.Parameters[i].Type.Equals(types[i]))
                            {
                                return false;
                            }
                        }

                        break;
                }
            }

            return true;
        }
    }
}
