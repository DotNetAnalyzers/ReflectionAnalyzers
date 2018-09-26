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
        public static IReadOnlyList<ITypeSymbol> AnyTypes = new ITypeSymbol[0];
#pragma warning restore CA1825 // Avoid zero-length array allocations.

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod
        /// </summary>
        internal static GetXResult? TryMatchGetConstructor(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags, out ArgumentSyntax typesArg, out IReadOnlyList<ITypeSymbol> types)
        {
            targetType = null;
            target = null;
            flagsArg = null;
            effectiveFlags = 0;
            typesArg = null;
            types = null;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetConstructor, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(invocation, getX) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                return TryGetMember(getX, targetType, ".ctor", effectiveFlags, types, context, out target);
            }

            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetEvent
        /// </summary>
        internal static GetXResult? TryMatchGetEvent(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetEvent, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetField
        /// </summary>
        internal static GetXResult? TryMatchGetField(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetField, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod
        /// </summary>
        internal static GetXResult? TryMatchGetMethod(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags, out ArgumentSyntax typesArg, out IReadOnlyList<ITypeSymbol> types)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            effectiveFlags = 0;
            typesArg = null;
            types = null;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(invocation, getX) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                return TryGetMember(getX, targetType, targetName, effectiveFlags, types, context, out target);
            }

            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod
        /// </summary>
        internal static GetXResult? TryMatchGetMember(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags, out ArgumentSyntax typesArg, out IReadOnlyList<ITypeSymbol> types)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            effectiveFlags = 0;
            typesArg = null;
            types = null;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMember, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(invocation, getX) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                return TryGetMember(getX, targetType, targetName, effectiveFlags, types, context, out target);
            }

            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetNestedType
        /// </summary>
        internal static GetXResult? TryMatchGetNestedType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetNestedType, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetProperty
        /// </summary>
        internal static GetXResult? TryMatchGetProperty(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetProperty, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out effectiveFlags);
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

        internal static GetXResult TryGetMember(IMethodSymbol getX, ITypeSymbol targetType, string targetMetadataName, BindingFlags flags, IReadOnlyList<ITypeSymbol> types, SyntaxNodeAnalysisContext context, out ISymbol member)
        {
            var name = TrimName();
            member = null;
            if (targetType is ITypeParameterSymbol typeParameter)
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
                foreach (var candidate in targetType.GetMembers(name))
                {
                    if (!MatchesFilter(candidate, targetMetadataName, flags, types))
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
                var current = targetType;
                while (current != null)
                {
                    foreach (var candidate in current.GetMembers(name))
                    {
                        if (!MatchesFilter(candidate, targetMetadataName, flags, types))
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
                                !current.Equals(targetType) &&
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

            if (targetType.TryFindFirstMemberRecursive(name, out member))
            {
                if (IsUseContainingType(member))
                {
                    return GetXResult.UseContainingType;
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

            if (getX != KnownSymbol.Type.GetConstructor &&
                getX != KnownSymbol.Type.GetNestedType &&
                !Type.HasVisibleMembers(targetType, flags))
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
                return !targetType.Equals(symbol.ContainingType) &&
                       (getX == KnownSymbol.Type.GetNestedType ||
                        (symbol.IsStatic &&
                         symbol.DeclaredAccessibility == Accessibility.Private));
            }

            bool IsWrongFlags(ISymbol symbol)
            {
                if (symbol.MetadataName == targetMetadataName &&
                    !MatchesFilter(symbol, symbol.MetadataName, flags, null))
                {
                    return true;
                }

                if (!symbol.ContainingType.Equals(targetType) &&
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
                return symbol.MetadataName == targetMetadataName &&
                       !MatchesFilter(symbol, symbol.MetadataName, everything, types);
            }

            bool IsExplicitImplementation(out ISymbol result)
            {
                foreach (var @interface in targetType.AllInterfaces)
                {
                    if (@interface.TryFindFirstMemberRecursive(name, out result))
                    {
                        return true;
                    }
                }

                result = null;
                return false;
            }

            string TrimName()
            {
                var index = targetMetadataName.IndexOf('`');
                if (index > 0)
                {
                    return targetMetadataName.Substring(0, index);
                }

                return targetMetadataName;
            }
        }

        internal static bool TryGetDefaultFlags(QualifiedMethod getX, out BindingFlags defaultFlags)
        {
            return TryGetDefaultFlags(getX.Name, out defaultFlags);
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
        private static GetXResult? TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol member, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            member = null;
            flagsArg = null;
            effectiveFlags = 0;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(getXMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetType(invocation, context, out targetType, out _) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags))
            {
                return TryGetMember(getX, targetType, targetName, effectiveFlags, null, context, out member);
            }

            return null;
        }

        private static bool TryGetName(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out ArgumentSyntax argument, out string name)
        {
            argument = null;
            name = null;
            return getX.TryFindParameter(KnownSymbol.String, out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument) &&
                   context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out name);
        }

        private static bool TryGetFlagsOrDefault(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out ArgumentSyntax argument, out BindingFlags bindingFlags)
        {
            return TryGetFlags(invocation, getX, context, out argument, out bindingFlags) ||
                   TryGetDefaultFlags(getX.MetadataName, out bindingFlags);
        }

        private static bool TryGetDefaultFlags(string getXName, out BindingFlags flags)
        {
            switch (getXName)
            {
                case "GetConstructor":
                    flags = BindingFlags.Public | BindingFlags.Instance;
                    return true;
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
                    flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
                    return true;
            }

            flags = 0;
            return false;
        }

        private static bool TryGetFlags(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out ArgumentSyntax argument, out BindingFlags bindingFlags)
        {
            argument = null;
            bindingFlags = 0;
            return getX.TryFindParameter(KnownSymbol.BindingFlags, out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument) &&
                   context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out bindingFlags);
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

        private static bool MatchesFilter(ISymbol candidate, string metadataName, BindingFlags flags, IReadOnlyList<ITypeSymbol> types)
        {
            if (candidate.MetadataName != metadataName)
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
