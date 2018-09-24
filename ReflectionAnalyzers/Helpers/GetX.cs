namespace ReflectionAnalyzers
{
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
        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetEvent
        /// </summary>
        internal static GetXResult? TryMatchGetEvent(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetEvent, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetField
        /// </summary>
        internal static GetXResult? TryMatchGetField(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetField, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod
        /// </summary>
        internal static GetXResult? TryMatchGetMethod(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            flags = 0;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetTargetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(getX, invocation) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                (TryGetFlags(invocation, getX, context, out flagsArg, out flags) ||
                 TryGetDefaultFlags(KnownSymbol.Type.GetMethod, out flags)))
            {
                return TryGetTarget(getX, targetType, targetName, flags, context, out target);
            }

            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod
        /// </summary>
        internal static GetXResult? TryMatchGetMember(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            flags = 0;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMember, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetTargetType(invocation, context, out targetType, out _) &&
                IsKnownSignature(getX, invocation) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                (TryGetFlags(invocation, getX, context, out flagsArg, out flags) ||
                 TryGetDefaultFlags(KnownSymbol.Type.GetMember, out flags)))
            {
                return TryGetTarget(getX, targetType, targetName, flags, context, out target);
            }

            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetNestedType
        /// </summary>
        internal static GetXResult? TryMatchGetNestedType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetNestedType, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetProperty
        /// </summary>
        internal static GetXResult? TryMatchGetProperty(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetProperty, context, out targetType, out nameArg, out targetName, out target, out flagsArg, out flags);
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

        internal static GetXResult TryGetTarget(IMethodSymbol getX, ITypeSymbol targetType, string targetMetadataName, BindingFlags effectiveFlags, SyntaxNodeAnalysisContext context, out ISymbol target)
        {
            var name = TrimName();
            target = null;
            if (targetType is ITypeParameterSymbol typeParameter)
            {
                if (typeParameter.ConstraintTypes.Length == 0)
                {
                    return TryGetTarget(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), name, effectiveFlags, context, out target);
                }

                foreach (var constraintType in typeParameter.ConstraintTypes)
                {
                    var result = TryGetTarget(getX, constraintType, name, effectiveFlags, context, out target);
                    if (result != GetXResult.NoMatch)
                    {
                        return result;
                    }
                }

                return TryGetTarget(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), name, effectiveFlags, context, out target);
            }

            if (getX == KnownSymbol.Type.GetNestedType ||
                effectiveFlags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                (effectiveFlags.HasFlagFast(BindingFlags.Static) &&
                 !effectiveFlags.HasFlagFast(BindingFlags.Instance) &&
                 !effectiveFlags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                foreach (var member in targetType.GetMembers(name))
                {
                    if (!MatchesFilter(member, targetMetadataName, effectiveFlags))
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

                    return GetXResult.WrongFlags;
                }

                if (IsExplicitImplementation(out target))
                {
                    return GetXResult.ExplicitImplementation;
                }

                if (!HasVisibleMembers(targetType, effectiveFlags))
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
                    if (!MatchesFilter(member, targetMetadataName, effectiveFlags))
                    {
                        continue;
                    }

                    if (member.IsStatic &&
                        !current.Equals(targetType) &&
                        !effectiveFlags.HasFlagFast(BindingFlags.FlattenHierarchy))
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
                if (targetType.TryFindFirstMemberRecursive(name, out target))
                {
                    return GetXResult.WrongFlags;
                }

                if (IsExplicitImplementation(out target))
                {
                    return GetXResult.ExplicitImplementation;
                }

                if (!HasVisibleMembers(targetType, effectiveFlags))
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
            switch (getX.Name)
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

        /// <summary>
        /// Defensive check to only handle known cases. Don't know how the binder works.
        /// </summary>
        private static bool IsKnownSignature(IMethodSymbol candidate, InvocationExpressionSyntax invocation)
        {
            foreach (var parameter in candidate.Parameters)
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
                    parameter.Type == KnownSymbol.BindingFlags)
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
        private static GetXResult? TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SyntaxNodeAnalysisContext context, out ITypeSymbol targetType, out ArgumentSyntax nameArg, out string targetName, out ISymbol target, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            targetType = null;
            nameArg = null;
            targetName = null;
            target = null;
            flagsArg = null;
            flags = 0;
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(getXMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                TryGetTargetType(invocation, context, out targetType, out _) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                (TryGetFlags(invocation, getX, context, out flagsArg, out flags) ||
                 TryGetDefaultFlags(getXMethod, out flags)))
            {
                return TryGetTarget(getX, targetType, targetName, flags, context, out target);
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

        private static bool TryGetFlags(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out ArgumentSyntax argument, out BindingFlags bindingFlags)
        {
            argument = null;
            bindingFlags = 0;
            return getX.TryFindParameter(KnownSymbol.BindingFlags, out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument) &&
                   context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out bindingFlags);
        }

        private static bool MatchesFilter(ISymbol candidate, string metadataName, BindingFlags filter)
        {
            if (candidate.MetadataName != metadataName)
            {
                return false;
            }

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
