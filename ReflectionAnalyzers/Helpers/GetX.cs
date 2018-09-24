namespace ReflectionAnalyzers
{
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
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
                TryGetTargetType(invocation, context.SemanticModel, context.CancellationToken, out targetType, out _) &&
                IsKnownSignature(getX) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                (TryGetFlags(invocation, getX, context, out flagsArg, out flags) ||
                 TryGetDefaultFlags(KnownSymbol.Type.GetMethod, out flags)))
            {
                return TryGetTarget(getX, targetType, targetName, flags, context, out target);
            }

            return null;

            bool IsKnownSignature(IMethodSymbol candidate)
            {
                // I don't know how binder works so limiting checks to what I know.
                return (candidate.Parameters.TrySingle(out var nameParameter) &&
                        nameParameter.Type == KnownSymbol.String) ||
                       (candidate.Parameters.Length == 2 &&
                        candidate.Parameters.TrySingle(x => x.Type == KnownSymbol.String, out nameParameter) &&
                        candidate.Parameters.TrySingle(x => x.Type == KnownSymbol.BindingFlags, out _));
            }
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
                TryGetTargetType(invocation, context.SemanticModel, context.CancellationToken, out targetType, out _) &&
                IsKnownSignature(getX) &&
                TryGetName(invocation, getX, context, out nameArg, out targetName) &&
                (TryGetFlags(invocation, getX, context, out flagsArg, out flags) ||
                 TryGetDefaultFlags(KnownSymbol.Type.GetMember, out flags)))
            {
                return TryGetTarget(getX, targetType, targetName, flags, context, out target);
            }

            return null;

            bool IsKnownSignature(IMethodSymbol candidate)
            {
                // I don't know how binder works so limiting checks to what I know.
                return (candidate.Parameters.TrySingle(out var nameParameter) &&
                        nameParameter.Type == KnownSymbol.String) ||
                       (candidate.Parameters.Length == 2 &&
                        candidate.Parameters.TrySingle(x => x.Type == KnownSymbol.String, out nameParameter) &&
                        candidate.Parameters.TrySingle(x => x.Type == KnownSymbol.BindingFlags, out _));
            }
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
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="result">The type.</param>
        /// <param name="instance">The instance the type was called GetType on. Can be null</param>
        /// <returns>True if the type could be determined.</returns>
        internal static bool TryGetTargetType(InvocationExpressionSyntax getX, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result, out Optional<IdentifierNameSyntax> instance)
        {
            result = null;
            instance = default(Optional<IdentifierNameSyntax>);
            return getX.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetTargetType(memberAccess.Expression, semanticModel, null, cancellationToken, out result, out instance);
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
                TryGetTargetType(invocation, context.SemanticModel, context.CancellationToken, out targetType, out _) &&
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

        private static GetXResult TryGetTarget(IMethodSymbol getX, ITypeSymbol targetType, string targetName, BindingFlags effectiveFlags, SyntaxNodeAnalysisContext context, out ISymbol target)
        {
            target = null;
            if (targetType is ITypeParameterSymbol typeParameter)
            {
                if (typeParameter.ConstraintTypes.Length == 0)
                {
                    return TryGetTarget(getX, context.Compilation.GetSpecialType(SpecialType.System_Object), targetName, effectiveFlags, context, out target);
                }
            }

            if (getX == KnownSymbol.Type.GetNestedType ||
                effectiveFlags.HasFlagFast(BindingFlags.DeclaredOnly) ||
                (effectiveFlags.HasFlagFast(BindingFlags.Static) &&
                 !effectiveFlags.HasFlagFast(BindingFlags.Instance) &&
                 !effectiveFlags.HasFlagFast(BindingFlags.FlattenHierarchy)))
            {
                foreach (var member in targetType.GetMembers(targetName))
                {
                    if (!MatchesFlags(member, effectiveFlags))
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

                if (targetType.TryFindFirstMemberRecursive(targetName, out target))
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

                if (!HasVisibleMembers(targetType, effectiveFlags))
                {
                    return GetXResult.Unknown;
                }

                return GetXResult.NoMatch;
            }

            var current = targetType;
            while (current != null)
            {
                foreach (var member in current.GetMembers(targetName))
                {
                    if (!MatchesFlags(member, effectiveFlags))
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
                if (targetType.TryFindFirstMemberRecursive(targetName, out target))
                {
                    return GetXResult.WrongFlags;
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
        }

        private static bool MatchesFlags(ISymbol candidate, BindingFlags filter)
        {
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

        private static bool TryGetTargetType(ExpressionSyntax expression, SemanticModel semanticModel, PooledSet<ExpressionSyntax> visited, CancellationToken cancellationToken, out ITypeSymbol result, out Optional<IdentifierNameSyntax> instance)
        {
            instance = default(Optional<IdentifierNameSyntax>);
            result = null;
            switch (expression)
            {
                case IdentifierNameSyntax identifierName when semanticModel.TryGetSymbol(identifierName, cancellationToken, out ILocalSymbol local):
                    using (visited = visited.IncrementUsage())
                    {
                        return AssignedValueWalker.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue) &&
                               visited.Add(assignedValue) &&
                               TryGetTargetType(assignedValue, semanticModel, visited, cancellationToken, out result, out instance);
                    }

                case TypeOfExpressionSyntax typeOf:
                    return semanticModel.TryGetType(typeOf.Type, cancellationToken, out result);
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

                                return semanticModel.TryGetType(typeAccess.Expression, cancellationToken, out result);
                            case IdentifierNameSyntax identifierName when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType):
                                return semanticModel.TryGetSymbol(containingType, cancellationToken, out result);
                        }
                    }
                    else if (args.Arguments.TrySingle(out var arg) &&
                             arg.TryGetStringValue(semanticModel, cancellationToken, out var typeName) &&
                             getType.TryGetTarget(KnownSymbol.Assembly.GetType, semanticModel, cancellationToken, out _))
                    {
                        switch (getType.Expression)
                        {
                            case MemberAccessExpressionSyntax typeAccess when semanticModel.TryGetType(typeAccess.Expression, cancellationToken, out var typeInAssembly):
                                result = typeInAssembly.ContainingAssembly.GetTypeByMetadataName(typeName);
                                return result != null;
                            case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType) &&
                                                            semanticModel.TryGetSymbol(containingType, cancellationToken, out var typeInAssembly):
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
