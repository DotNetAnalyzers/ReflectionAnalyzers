namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Linq;
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
                TryGetTargetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(invocation, getX) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                return TryGetTarget(getX, targetType, ".ctor", effectiveFlags, types, context, out target);
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
                TryGetTargetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(invocation, getX) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                return TryGetTarget(getX, targetType, targetName, effectiveFlags, types, context, out target);
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
                TryGetTargetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(invocation, getX) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags) &&
                TryGetTypesOrDefault(invocation, getX, context, out typesArg, out types))
            {
                return TryGetTarget(getX, targetType, targetName, effectiveFlags, types, context, out target);
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
        /// <param name="instance">The instance the type was called GetType on. Can be null</param>
        /// <returns>True if the type could be determined.</returns>
        internal static bool TryGetTargetType(InvocationExpressionSyntax getX, SyntaxNodeAnalysisContext context, out ITypeSymbol result, out Optional<IdentifierNameSyntax> instance)
        {
            result = null;
            instance = default(Optional<IdentifierNameSyntax>);
            return getX.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetTargetType(memberAccess.Expression, context, null, out result, out instance);
        }

        internal static GetXResult TryGetTarget(IMethodSymbol getX, ITypeSymbol targetType, string targetMetadataName, BindingFlags flags, IReadOnlyList<ITypeSymbol> types, SyntaxNodeAnalysisContext context, out ISymbol target)
        {
            var name = TrimName();
            target = null;
            if (targetType is ITypeParameterSymbol typeParameter)
            {
                if (typeParameter.ConstraintTypes.Length == 0)
                {
                    return TryGetTarget(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), name, flags, types, context, out target);
                }

                foreach (var constraintType in typeParameter.ConstraintTypes)
                {
                    var result = TryGetTarget(getX, constraintType, name, flags, types, context, out target);
                    if (result != GetXResult.NoMatch)
                    {
                        return result;
                    }
                }

                return TryGetTarget(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), name, flags, types, context, out target);
            }

            if (getX == KnownSymbol.Type.GetNestedType ||
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

                    if (target == null)
                    {
                        target = candidate;
                        if (IsOfWrongType(target))
                        {
                            return GetXResult.WrongMemberType;
                        }
                    }
                    else
                    {
                        return GetXResult.Ambiguous;
                    }
                }

                if (target != null)
                {
                    return GetXResult.Single;
                }

                if (targetType.TryFindFirstMemberRecursive(name, out target))
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

                    if (target.MetadataName == targetMetadataName &&
                        !MatchesFilter(target, target.MetadataName, flags, null))
                    {
                        return GetXResult.WrongFlags;
                    }

                    if (!target.ContainingType.Equals(targetType) &&
                        (target.IsStatic ||
                         flags.HasFlagFast(BindingFlags.DeclaredOnly)))
                    {
                        return GetXResult.WrongFlags;
                    }
                }

                if (IsExplicitImplementation(out target))
                {
                    return GetXResult.ExplicitImplementation;
                }

                if (flags.HasFlagFast(BindingFlags.NonPublic) &&
                    !targetType.Locations.Any(x => x.IsInSource))
                {
                    return GetXResult.Unknown;
                }

                return GetXResult.NoMatch;
            }

            var current = targetType;
            while (current != null)
            {
                foreach (var member in current.GetMembers(name))
                {
                    if (!MatchesFilter(member, targetMetadataName, flags, types))
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

                        if (member.IsStatic &&
                            !current.Equals(targetType) &&
                            !flags.HasFlagFast(BindingFlags.FlattenHierarchy))
                        {
                            return GetXResult.WrongFlags;
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
                if (targetType.TryFindFirstMemberRecursive(name, out target))
                {
                    if (target.MetadataName == targetMetadataName &&
                        !MatchesFilter(target, target.MetadataName, flags, null))
                    {
                        return GetXResult.WrongFlags;
                    }
                }

                if (IsExplicitImplementation(out target))
                {
                    return GetXResult.ExplicitImplementation;
                }

                if (!HasVisibleMembers(targetType, flags))
                {
                    return GetXResult.Unknown;
                }

                return GetXResult.NoMatch;
            }

            return GetXResult.Single;

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

                if (getX.ReturnType == KnownSymbol.Type &&
                    !(member is ITypeSymbol))
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
        private static GetXResult? TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags effectiveFlags)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            effectiveFlags = 0;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(getXMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetTargetType(invocation, context, out targetType, out _) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                TryGetFlagsOrDefault(invocation, getX, context, out flagsArg, out effectiveFlags))
            {
                return TryGetTarget(getX, targetType, targetName, effectiveFlags, null, context, out target);
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

        private static bool TryGetDefaultFlags(string getXName, out BindingFlags defaultFlags)
        {
            switch (getXName)
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
                return TryGetTypes(typesArg, context, out types);
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

        private static bool TryGetTypes(ArgumentSyntax argument, SyntaxNodeAnalysisContext context, out IReadOnlyList<ITypeSymbol> types)
        {
            types = null;
            return TryGetTypes(argument, out types);

            bool TryGetTypes(ArgumentSyntax array, out IReadOnlyList<ITypeSymbol> result)
            {
                result = null;
                switch (array.Expression)
                {
                    case ImplicitArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer is InitializerExpressionSyntax initializer:
                        return TryGetTypesFromInitializer(initializer, out result);
                    case ArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer is InitializerExpressionSyntax initializer:
                        return TryGetTypesFromInitializer(initializer, out result);
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
                        return true;
                }

                return false;
            }

            bool TryGetTypesFromInitializer(InitializerExpressionSyntax initializer, out IReadOnlyList<ITypeSymbol> result)
            {
                var temp = new ITypeSymbol[initializer.Expressions.Count];
                for (var i = 0; i < initializer.Expressions.Count; i++)
                {
                    var expression = initializer.Expressions[i];
                    if (expression is TypeOfExpressionSyntax typeOf &&
                        context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var type))
                    {
                        temp[i] = type;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }

                result = temp;
                return true;
            }
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

        private static bool HasVisibleMembers(ITypeSymbol type, BindingFlags effectiveFlags)
        {
            if (effectiveFlags.HasFlagFast(BindingFlags.NonPublic))
            {
                return !HasSuperTypeNotInSource();
            }

            return true;

            bool HasSuperTypeNotInSource()
            {
                var current = type;
                while (current != null &&
                       current != KnownSymbol.Object)
                {
                    if (!current.Locations.Any(x => x.IsInSource))
                    {
                        return true;
                    }

                    current = current.BaseType;
                }

                return false;
            }
        }

        private static bool TryGetTargetType(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, PooledSet<ExpressionSyntax> visited, out ITypeSymbol result, out Optional<IdentifierNameSyntax> instance)
        {
            instance = default(Optional<IdentifierNameSyntax>);
            result = null;
            switch (expression)
            {
                case IdentifierNameSyntax identifierName when context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out ILocalSymbol local):
                    using (visited = visited.IncrementUsage())
                    {
                        return AssignedValueWalker.TryGetSingle(local, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                               visited.Add(assignedValue) &&
                               TryGetTargetType(assignedValue, context, visited, out result, out instance);
                    }

                case TypeOfExpressionSyntax typeOf:
                    return context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out result);
                case InvocationExpressionSyntax getType when getType.TryGetMethodName(out var name) &&
                                                 name == "GetType" &&
                                                 getType.ArgumentList is ArgumentListSyntax args:
                    if (args.Arguments.Count == 0)
                    {
                        switch (getType.Expression)
                        {
                            case MemberAccessExpressionSyntax typeAccess:
                                if (typeAccess.Expression is IdentifierNameSyntax identifier)
                                {
                                    instance = identifier;
                                }

                                return context.SemanticModel.TryGetType(typeAccess.Expression, context.CancellationToken, out result);
                            case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType):
                                return context.SemanticModel.TryGetSymbol(containingType, context.CancellationToken, out result);
                        }
                    }
                    else if (args.Arguments.TrySingle(out var arg) &&
                             arg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var typeName) &&
                             getType.TryGetTarget(KnownSymbol.Assembly.GetType, context.SemanticModel, context.CancellationToken, out _))
                    {
                        switch (getType.Expression)
                        {
                            case MemberAccessExpressionSyntax typeAccess when context.SemanticModel.TryGetType(typeAccess.Expression, context.CancellationToken, out var typeInAssembly):
                                result = typeInAssembly.ContainingAssembly.GetTypeByMetadataName(typeName);
                                return result != null;
                            case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType) &&
                                                             context.SemanticModel.TryGetSymbol(containingType, context.CancellationToken, out var typeInAssembly):
                                result = typeInAssembly.ContainingAssembly.GetTypeByMetadataName(typeName);
                                return result != null;
                        }
                    }

                    break;
            }

            return false;
        }
    }
}
