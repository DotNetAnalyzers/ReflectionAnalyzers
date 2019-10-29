namespace ReflectionAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Type
    {
        internal static string ToString(this ITypeSymbol type, SyntaxNodeAnalysisContext context) => ToString(type, context.SemanticModel, context.Node.SpanStart);

        internal static string ToString(this ITypeSymbol type, SemanticModel semanticModel, int position)
        {
            if (semanticModel.IsAccessible(position, type))
            {
                return type.ToMinimalDisplayString(semanticModel, position);
            }

            return ToString(type.BaseType, semanticModel, position);
        }

        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out ITypeSymbol? result, [NotNullWhen(true)] out ExpressionSyntax? source)
        {
            return TryGet(expression, context, null, out result, out source);
        }

        internal static bool HasVisibleMembers(ITypeSymbol type, BindingFlags flags)
        {
            if (!flags.HasFlagFast(BindingFlags.NonPublic))
            {
                return true;
            }

            if (flags.HasFlagFast(BindingFlags.DeclaredOnly))
            {
                return HasVisibleNonPublicMembers(type, recursive: false);
            }

            if (!flags.HasFlagFast(BindingFlags.Instance) &&
                !flags.HasFlagFast(BindingFlags.FlattenHierarchy))
            {
                return HasVisibleNonPublicMembers(type, recursive: false);
            }

            return HasVisibleNonPublicMembers(type, recursive: true);
        }

        internal static bool HasVisibleNonPublicMembers(ITypeSymbol type, bool recursive)
        {
            if (type == null ||
                type.TypeKind == TypeKind.Interface ||
                type == KnownSymbol.Object)
            {
                return true;
            }

            if (!type.Locations.TryFirst(x => x.IsInSource, out _))
            {
                return false;
            }

            return !recursive || HasVisibleNonPublicMembers(type.BaseType, recursive: true);
        }

        internal static bool IsCastToWrongType(InvocationExpressionSyntax invocation, ITypeSymbol expectedType, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out TypeSyntax? typeSyntax)
        {
            if (context.SemanticModel.IsAccessible(context.Node.SpanStart, expectedType))
            {
                switch (invocation.Parent)
                {
                    case CastExpressionSyntax castExpression when context.SemanticModel.TryGetType(castExpression.Type, context.CancellationToken, out var castType) &&
                                                                  !expectedType.IsAssignableTo(castType, context.Compilation):
                        typeSyntax = castExpression.Type;
                        return true;
                }
            }

            typeSyntax = null;
            return false;
        }

        internal static bool TryMatchTypeGetType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out TypeNameArgument typeName, out ArgumentAndValue<bool> ignoreCase)
        {
            if (invocation.TryGetTarget(KnownSymbol.Type.GetType, context.SemanticModel, context.CancellationToken, out var target) &&
                target.TryFindParameter("typeName", out var nameParameter) &&
                invocation.TryFindArgument(nameParameter, out var nameArg) &&
                nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var name))
            {
                typeName = new TypeNameArgument(nameArg, name);
                switch (target.Parameters.Length)
                {
                    case 1:
                        ignoreCase = default;
                        return true;
                    case 2 when target.TryFindParameter("throwOnError", out _):
                        ignoreCase = default;
                        return true;
                    case 3 when target.TryFindParameter("throwOnError", out _) &&
                                target.TryFindParameter("ignoreCase", out var ignoreCaseParameter) &&
                                invocation.TryFindArgument(ignoreCaseParameter, out var ignoreCaseArg) &&
                                context.SemanticModel.TryGetConstantValue(ignoreCaseArg.Expression, context.CancellationToken, out bool ignoreNameCase):
                        ignoreCase = new ArgumentAndValue<bool>(ignoreCaseArg, ignoreNameCase);
                        return true;
                }
            }

            typeName = default;
            ignoreCase = default;
            return false;
        }

        internal static bool TryMatchAssemblyGetType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out TypeNameArgument typeName, out ArgumentAndValue<bool> ignoreCase)
        {
            if (invocation.TryGetTarget(KnownSymbol.Assembly.GetType, context.SemanticModel, context.CancellationToken, out var target) &&
                target.TryFindParameter("name", out var nameParameter) &&
                invocation.TryFindArgument(nameParameter, out var nameArg) &&
                nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var name))
            {
                typeName = new TypeNameArgument(nameArg, name);
                switch (target.Parameters.Length)
                {
                    case 1:
                        ignoreCase = default;
                        return true;
                    case 2 when target.TryFindParameter("throwOnError", out _):
                        ignoreCase = default;
                        return true;
                    case 3 when target.TryFindParameter("throwOnError", out _) &&
                                target.TryFindParameter("ignoreCase", out var ignoreCaseParameter) &&
                                invocation.TryFindArgument(ignoreCaseParameter, out var ignoreCaseArg) &&
                                context.SemanticModel.TryGetConstantValue(ignoreCaseArg.Expression, context.CancellationToken, out bool ignoreNameCase):
                        ignoreCase = new ArgumentAndValue<bool>(ignoreCaseArg, ignoreNameCase);
                        return true;
                }
            }

            typeName = default;
            ignoreCase = default;
            return false;
        }

        private static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, PooledSet<ExpressionSyntax>? visited, [NotNullWhen(true)] out ITypeSymbol? result, [NotNullWhen(true)] out ExpressionSyntax? source)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Identifier.ValueText == "ReturnType" &&
                                                                    memberAccess.Expression is InvocationExpressionSyntax invocation &&
                                                                    GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _) &&
                                                                    reflectedMember.Match == FilterMatch.Single &&
                                                                    reflectedMember.Symbol is IMethodSymbol method:
                    source = memberAccess;
                    result = method.ReturnType;
                    return true;
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Identifier.ValueText == "FieldType" &&
                                                                    memberAccess.Expression is InvocationExpressionSyntax invocation &&
                                                                    GetX.TryMatchGetField(invocation, context, out var reflectedMember, out _, out _) &&
                                                                    reflectedMember.Match == FilterMatch.Single &&
                                                                    reflectedMember.Symbol is IFieldSymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Identifier.ValueText == "PropertyType" &&
                                                                    memberAccess.Expression is InvocationExpressionSyntax invocation &&
                                                                    GetX.TryMatchGetProperty(invocation, context, out var reflectedMember, out _, out _, out _) &&
                                                                    reflectedMember.Match == FilterMatch.Single &&
                                                                    reflectedMember.Symbol is IPropertySymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case TypeOfExpressionSyntax typeOf:
                    source = typeOf;
                    return context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out result);
                case InvocationExpressionSyntax invocation when invocation.ArgumentList is ArgumentListSyntax args &&
                                                                args.Arguments.Count == 0 &&
                                                                invocation.TryGetMethodName(out var name) &&
                                                                name == "GetType":
                    switch (invocation.Expression)
                    {
                        case MemberAccessExpressionSyntax typeAccess:
                            source = invocation;
                            if (context.SemanticModel.TryGetType(typeAccess.Expression, context.CancellationToken, out result))
                            {
                                if (result is INamedTypeSymbol namedType &&
                                    namedType.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
                                {
                                    result = namedType.TypeArguments[0];
                                }

                                return true;
                            }

                            return false;
                        case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax? containingType):
                            source = invocation;
                            return context.SemanticModel.TryGetSymbol(containingType, context.CancellationToken, out result);
                        case MemberBindingExpressionSyntax memberBinding when memberBinding.Parent?.Parent is ConditionalAccessExpressionSyntax conditionalAccess:
                            source = invocation;
                            return context.SemanticModel.TryGetType(conditionalAccess.Expression, context.CancellationToken, out result);
                    }

                    break;
                case InvocationExpressionSyntax candidate when TryMatchTypeGetType(candidate, context, out var typeName, out var ignoreCase):
                    source = candidate;
                    result = context.Compilation.GetTypeByMetadataName(typeName, ignoreCase.Value);
                    return result != null;
                case InvocationExpressionSyntax candidate when TryMatchAssemblyGetType(candidate, context, out var typeName, out var ignoreCase):
                    source = candidate;
                    result = Assembly.TryGet(candidate.Expression, context, out var assembly)
                        ? assembly.GetTypeByMetadataName(typeName, ignoreCase.Value)
                        : null;
                    return result != null;
                case InvocationExpressionSyntax invocation when invocation.TryGetTarget(KnownSymbol.Type.GetGenericTypeDefinition, context.SemanticModel, context.CancellationToken, out _) &&
                                                                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                                                TryGet(memberAccess.Expression, context, visited, out var definingType, out _) &&
                                                                definingType is INamedTypeSymbol namedType:
                    source = invocation;
                    result = namedType.ConstructedFrom;
                    return true;

                case InvocationExpressionSyntax invocation when GetX.TryMatchGetNestedType(invocation, context, out var reflectedMember, out _, out _):
                    source = invocation;
                    result = reflectedMember.Symbol as ITypeSymbol;
                    return result != null && reflectedMember.Match == FilterMatch.Single;
                case InvocationExpressionSyntax invocation when invocation.TryGetTarget(KnownSymbol.Type.MakeGenericType, context.SemanticModel, context.CancellationToken, out _) &&
                                                                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                                                TypeArguments.TryCreate(invocation, context, out var typeArguments) &&
                                                                typeArguments.TryGetArgumentsTypes(context, out var types):
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                    {
                        source = invocation;
                        if (visited.Add(invocation) &&
                            TryGet(memberAccess.Expression, context, visited, out var definition, out _) &&
                            definition is INamedTypeSymbol namedType &&
                            ReferenceEquals(namedType, namedType.ConstructedFrom) &&
                            namedType.Arity == types.Length)
                        {
                            result = namedType.Construct(types);
                            return result != null;
                        }

                        result = null;
                        return false;
                    }

                case ConditionalAccessExpressionSyntax conditionalAccess:
                    source = conditionalAccess;
                    return TryGet(conditionalAccess.WhenNotNull, context, out result, out _);
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out var local))
            {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                {
                    source = null;
                    result = null;
                    return AssignedValue.TryGetSingle(local, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                           visited.Add(assignedValue) &&
                           TryGet(assignedValue, context, visited, out result, out source);
                }
            }

            source = null;
            result = null;
            return false;
        }
    }
}
